using System.IO.Ports;

namespace Arduino
{

    public enum ArduinoError: byte { OK = 0x00, ReadError, CodeInvalid, OffsetInvalid, LenInvalid, CRCError }

    public class ArduinoDevice : IHardware
    {
        const byte markerStart = 0xF8;
        const byte markerStop = 0xF9;
        const byte markerEsc = 0xFA;

        const byte cmdRead = 0x01;
        const byte cmdWrite = 0x02;
        const byte cmdCommand = 0x03;
        const byte cmdError = 0x04;



        SerialPort Port;

        ILogger logger;

        public int PropsLen => properties.Sum(x => x.Value.RawValue.Length);

        //private Queue<HardwareMessage> RecievedMessages = new();

        private Queue<byte> RecievedBytes = new();

        Dictionary<int, IProperty> properties = new();    

        private bool _escaped = false;

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
           // Port.DataReceived += new SerialDataReceivedEventHandler(OnData);
        }


        public void Kill()
        {
           if (Port.IsOpen) Port.Close();
        }

        void ProcessByte(byte b)
        {
            if (_escaped)
            {
                RecievedBytes.Enqueue(b);
                _escaped = false;
                return;
            }
            switch (b)
            {
                case markerStart:
                    {
                        if (RecievedBytes.Count > 0)
                        {
                            logger.Log("Неожиданный старт пакета");
                            RecievedBytes.Clear();
                        }
                    }
                    break;
                case markerStop:
                    {
                        if (RecievedBytes.Count < 2) // Минимально возможный в теории пакет - код команды + CRC
                        {
                            logger.Log("Неожиданный конец пакета");
                        }
                        else 
                        {
                            if (RecievedBytes.Aggregate((x, y) => (byte)(x ^ y))!=0)
                            {
                                logger.Log("Ошибка контрольной суммы");
                                RecievedBytes.Clear();
                            }
                            else 
                            {
                                if (!ProcessPacket())
                                {
                                    RecievedBytes.Clear();
                                }
                            }
                        }
                    }
                    break;
                case markerEsc:
                    {
                        if (RecievedBytes.Count == 0)
                        {
                            logger.Log("Неожиданный Esc символ");
                        }
                        else
                        {
                            _escaped = true;
                        }
                    }
                    break;
                default:
                    {
                        RecievedBytes.Enqueue(b);
                    }
                    break;
            }
        }
        bool ProcessPacket()
        {
            ushort offset, len;
            byte code = RecievedBytes.Dequeue();

            switch (code)
            {
                case cmdRead:
                    {
                        if (RecievedBytes.Count < 5)
                        {
                            logger.Log("Слишком короткий пакет"); return false;
                        }
                        offset = RecievedBytes.Dequeue();
                        offset += (ushort)(RecievedBytes.Dequeue() << 8);
                        len = RecievedBytes.Dequeue();
                        len += (ushort)(RecievedBytes.Dequeue() << 8);

                        if (offset + len > PropsLen)
                        {
                            logger.Log("Слишком длинный пакет данных"); return false;
                        }

                        if (RecievedBytes.Count != len + 1)
                        {
                            logger.Log("Неверная длина данных"); return false;
                        }
                        byte[] data = new byte[len];
                        for (int i = 0; i < len; i++)
                        {
                            data[i] = RecievedBytes.Dequeue(); 
                        }
                        //logger.Log("<<<<< Пакет Arduino {" + BitConverter.ToString(data) + "}");
                        ProcessMessage(offset, data);
                    }
                    break;
                case cmdError:
                    {
                        if (RecievedBytes.Count < 2)
                        {
                            logger.Log("Слишком короткий пакет"); return false;
                        }
                        byte data = RecievedBytes.Dequeue();


                        if (Enum.TryParse(data.ToString(),true, out ArduinoError res))
                        {
                            logger.Log("<<<<< Ошибка: " + res.ToString());
                        }
                        else
                        {
                            logger.Log("<<<<< неизвестная ошибка");
                        }

                        // TODO
                    }
                    break;
                default:
                    {
                        logger.Log("Неверный код команды"); return false;
                    }
            }
            RecievedBytes.Clear(); // Удаление CRC
            return true;
        }

        public void OnData(object sender, SerialDataReceivedEventArgs e)
        {
            int count = Port.BytesToRead;
            try
            {
                while (Port.BytesToRead > 0)
                {
                    int res = Port.ReadByte();
                    if (res >= 0)
                    {
                        ProcessByte((byte)res);
                    }
                    else
                    {
                        logger.Log("Неожиданный конец потока");
                    }
                }
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Порт закрыт");
            }
            catch(TimeoutException)
            {
                MessageBox.Show("Таймаут чтения данных");
            }
        }


        void ProcessMessage(ushort offset, byte[] data)
        {
            int position = 0;

            try
            {
                while (position < data.Length)
                {
                    int step = properties[offset + position].RawValue.Length;
                    properties[offset + position].RawValue = data.Skip(position).Take(step).ToArray();
                    position += step;
                }
            }
            catch (Exception)
            {
                logger.Log("[Ошибка расшифровки данных]  {смещение: " + (offset+position).ToString() + "}");
            }

        }


        public void Connect()
        {
            Port.Open();
        }



        public void SendCommand(byte cmd, ushort offset, ushort len, byte[] data)
        {

            // Формирование пакета
            List<byte> msg = new();
            msg.Add(cmd);

            // Только для чтения или записи
            if (cmd == cmdRead || cmd == cmdWrite)
            {
                msg.Add((byte)(offset & 0xFF));
                msg.Add((byte)(offset >> 8));
            }

            msg.Add((byte)(len & 0xFF));
            msg.Add((byte)(len >> 8));

            // Только для записи
            if (cmd == cmdWrite)
            {
                msg.AddRange(data);
            }

            // Добавление CRC
            msg.Add(msg.Aggregate((x, y) => (byte)(x ^ y)));


            // Отправка с экранированием
            Port.Write(new byte[] {markerStart},0,1);

            foreach (byte b in msg)
            {
                if (b == markerStart || b == markerStop || b == markerEsc)
                {
                    Port.Write(new byte[] { markerEsc }, 0, 1);
                }
                Port.Write(new byte[] { b }, 0, 1);
            }
            Port.Write(new byte[] { markerStop }, 0, 1);
            logger.Log(">>>> Команда {" + BitConverter.ToString(msg.ToArray()) + "}");
        }

        public void RequestAllValues()
        {
            SendCommand(cmdRead, 0, (ushort)PropsLen, Array.Empty<byte>());
        }



        public void RequestValue(IProperty p)
        {
            SendCommand(cmdRead, p.Offset, (ushort)p.RawValue.Length, Array.Empty<byte>());
        }

        public void SendValue(IProperty p)
        {
            SendCommand(cmdWrite, p.Offset, (ushort)p.RawValue.Length, p.RawValue);
        }

        public void SendCommand(string cmd)
        {
            String[] arr = cmd.Split('-');
            byte[] data = new byte[arr.Length];
            for (int i = 0; i < arr.Length; i++) data[i] = Convert.ToByte(arr[i], 16);
            SendCommand(cmdCommand, 0, (ushort)data.Length, data);
        }
    }
}
