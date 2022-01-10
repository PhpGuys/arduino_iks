using System.IO.Ports;

namespace Arduino
{
    public class ArduinoDevice : IHardware
    {
        SerialPort Port;
        Thread readThread;

        ILogger logger;

        public int PropsLen => properties.Sum(x => x.Value.RawValue.Length);

        private Queue<HardwareMessage> RecievedMessages = new();

        Dictionary<int, IProperty> properties = new();    
        public void RegisterProperty(IProperty prop)
        {
            properties[prop.Offset] = prop;
            prop.PropertyChanged += (o, e) => SendValue((IProperty)o);

            prop.PropertyRequest += (o, e) => RequestValue((IProperty)o);

        }

        public ArduinoDevice(SerialPort port, ILogger log)
        {
            logger = log;
            Port = port;
        }


        public void Kill()
        {
            Port.Close();
        }


        void ProcessMessage(HardwareMessage msg)
        {
            int position = 0;

            try
            {
                while (position < msg.Value.Length)
                {
                    int step = properties[msg.Offset + position].RawValue.Length;
                    properties[msg.Offset + position].RawValue = msg.Value.Skip(position).Take(step).ToArray();
                    position += step;
                }
            }
            catch (Exception)
            {
                logger.Log(Environment.NewLine + "[Ошибка расшифровки данных]  {смещение: " + (msg.Offset+position).ToString() + "}");
            }

        }
        public void Update()
        {
            if (RecievedMessages.Count <= 0) return;

            lock (RecievedMessages)
            {
                while (RecievedMessages.Count > 0)
                {
                    HardwareMessage msg = RecievedMessages.Dequeue();
                    ProcessMessage(msg);
                    //properties[msg.Offset].RawValue = msg.Value;
                }
            }
        }

        public void Connect()
        {
            Port.Open();
            readThread = new Thread(Read);
            readThread.Start();
        }

        // void OnPropertyChanged(e)

        public void RequestAllValues()
        {
            int len = PropsLen;
            List<byte> msg = new();
            msg.Add(0x01);
            // Смещение
            msg.Add(0x00);
            msg.Add(0x00);
            // Длина
            msg.Add((byte)(len & 0xFF));
            msg.Add((byte)(len >> 8));

            msg.Add(GetCRC(msg));
            Port.Write(msg.ToArray(), 0, msg.Count);
            logger.Log(">>>> Запрос всех полей {" + BitConverter.ToString(msg.ToArray()) + "}");
        }



        public void RequestValue(IProperty p)
        {
            int len = p.RawValue.Length;
            List<byte> msg = new();
            msg.Add(0x01);
            // Смещение
            msg.Add((byte)(p.Offset & 0xFF));
            msg.Add((byte)(p.Offset >> 8));
            // Длина
            msg.Add((byte)(len & 0xFF));
            msg.Add((byte)(len >> 8));

            msg.Add(GetCRC(msg));
            Port.Write(msg.ToArray(), 0, msg.Count);
            logger.Log(Environment.NewLine + ">>>> Запрос на обновление {" + BitConverter.ToString(msg.ToArray()) + "}");
        }

        public void SendValue(IProperty p)
        {
            byte[] data = p.RawValue;

            ushort len = (ushort)data.Length;
            List<byte> msg = new();
            msg.Add(0x02);
            msg.Add((byte)(p.Offset & 0xFF));
            msg.Add((byte)(p.Offset >> 8));

            msg.Add((byte)(len & 0xFF));
            msg.Add((byte)(len >> 8));

            msg.AddRange(data);
            msg.Add(GetCRC(msg));
            Port.Write(msg.ToArray(), 0, msg.Count);
            logger.Log(Environment.NewLine + ">>>> Изменение поля {" + BitConverter.ToString(msg.ToArray()) + "}");
        }

        byte GetCRC(List<byte>msg)
        {
            byte crc = 0x00;
            foreach (byte b in msg)
            {
                crc ^= b;
            }
            return crc;
        }



        public ushort ReadUshort(ref byte crc)
        {
            byte b = (byte)Port.ReadByte();
            logger.Log(".."+b.ToString());
            crc ^= b;
            ushort result = (ushort)b;
            b = (byte)Port.ReadByte();
            logger.Log(".." + b.ToString());
            crc ^= b;
            result += (ushort)(b<<8);
            return result;
        }

        /// <summary>
        ///  Метод чтения из порта (Запускается в отдельном потоке)
        /// </summary>
        private void Read()
        {
            ushort offset, len;
            byte crc; 
            int code;
            byte errCode;
            while (true)
            {
                try
                {
                    code = Port.ReadByte();
                    if (code >= 0)
                    {
                        crc = (byte)code;
                        if (code == 0x01) // Возврат данных
                        {
                            offset = ReadUshort(ref crc);
                            len = ReadUshort(ref crc);
                            byte[] data = new byte[len];
                            Port.Read(data, 0, len);
                            foreach (byte b in data)
                            {
                                logger.Log("-" + b.ToString());
                                crc ^= b;
                            }
                            logger.Log(Environment.NewLine + "<<<< Получено значение данных {" + BitConverter.ToString(data) + "}");
                            if (crc == Port.ReadByte())
                            {
                                lock (RecievedMessages)
                                {
                                    RecievedMessages.Enqueue(new HardwareMessage() { Offset = offset, Value = data });
                                }
                            }
                        }
                        if (code == 0x04)
                        {
                            errCode = (byte)Port.ReadByte();
                            crc ^= errCode;
                            if (crc == Port.ReadByte())
                            {
                                logger.Log(Environment.NewLine + "<<<< Ошибка {" + errCode.ToString() + "}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                   // MessageBox.Show(e.Message);
                    break;
                }
            }
        }

        public void SendCommand(string cmd)
        {

            String[] arr = cmd.Split('-');
            byte[] data = new byte[arr.Length];
            for (int i = 0; i < arr.Length; i++) data[i] = Convert.ToByte(arr[i], 16);


            ushort len = (ushort)data.Length;
            List<byte> msg = new();
            msg.Add(0x03);
            msg.Add((byte)(len & 0xFF));
            msg.Add((byte)(len >> 8));
            msg.AddRange(data);
            msg.Add(GetCRC(msg));
            Port.Write(msg.ToArray(), 0, msg.Count);
            logger.Log(Environment.NewLine + ">>>> Команда от кнопки {" + BitConverter.ToString(msg.ToArray()) + "}");
        }
    }
}
