using System.IO.Ports;

namespace Arduino
{
    public class ArduinoDevice : IHardware
    {
        SerialPort Port;
        Thread readThread;

        ILogger logger;


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

        public void Update()
        {
            if (RecievedMessages.Count <= 0) return;

            lock (RecievedMessages)
            {
                while (RecievedMessages.Count > 0)
                {
                    HardwareMessage msg = RecievedMessages.Dequeue();
                    properties[msg.Offset].RawValue = msg.Value;
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
            List<byte> msg = new();
            msg.Add(0x01);
            msg.Add(GetCRC(msg));
            Port.Write(msg.ToArray(), 0, msg.Count);
            logger.Log(">>>> Запрос всех полей {" + BitConverter.ToString(msg.ToArray()) + "}");
        }



        public void RequestValue(IProperty p)
        {
            List<byte> msg = new();
            msg.Add(0x03);
            msg.Add((byte)(p.Offset >> 8));
            msg.Add((byte)(p.Offset & 0xFF));
            msg.Add(GetCRC(msg));
            Port.Write(msg.ToArray(), 0, msg.Count);
            logger.Log(">>>> Запрос на обновление {" + BitConverter.ToString(msg.ToArray()) + "}");
        }

        public void SendValue(IProperty p)
        {
            byte[] data = p.RawValue;

            ushort len = (ushort)data.Length;
            List<byte> msg = new();
            msg.Add(0x02);
            msg.Add((byte)(p.Offset >> 8));
            msg.Add((byte)(p.Offset & 0xFF));
            msg.Add((byte)(len >> 8));
            msg.Add((byte)(len & 0xFF));
            msg.AddRange(data);
            msg.Add(GetCRC(msg));
            Port.Write(msg.ToArray(), 0, msg.Count);
            logger.Log(">>>> Изменение поля {" + BitConverter.ToString(msg.ToArray()) + "}");
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
            crc ^= b;
            ushort result = (ushort)(b << 8);
            b = (byte)Port.ReadByte();
            crc ^= b;
            result += b;
            return result;
        }

        /// <summary>
        ///  Метод чтения из порта (Запускается в отдельном потоке)
        /// </summary>
        private void Read()
        {
            ushort offset, len;
            byte crc;
            while (true)
            {
                try
                {
                    if (Port.ReadByte() == 0x02) // STX
                    {
                        crc = 0x02;
                        offset = ReadUshort(ref crc);
                        len = ReadUshort(ref crc);

                        byte[] data = new byte[len];
                        Port.Read(data, 0, len);

                        foreach (byte b in data) crc ^= b;
                        logger.Log("<<<< Получено значение поля {" + BitConverter.ToString(data) + "}");
                        if (crc == Port.ReadByte())
                        {
                            lock (RecievedMessages)
                            {
                                RecievedMessages.Enqueue(new HardwareMessage() { Offset = offset, Value = data });
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
            msg.Add(0x04);
            msg.Add((byte)(len >> 8));
            msg.Add((byte)(len & 0xFF));
            msg.AddRange(data);
            msg.Add(GetCRC(msg));
            Port.Write(msg.ToArray(), 0, msg.Count);
            logger.Log(">>>> Команда от кнопки {" + BitConverter.ToString(msg.ToArray()) + "}");
        }
    }
}
