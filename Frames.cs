using System;
using System.Collections.Generic;

namespace Mercury230Protocol
{
    enum RequestTypes : byte
    {
        TestLink = 0x00,        // Тестирование канала связи
        OpenConnection = 0x01,  // Запрос на открытие канала связи
        CloseConnection = 0x02, // Запрос на закрытие канала связи
    }

    enum MeterAccessLevel : byte
    {
        User = 0x01,
        Admin = 0x02
    }

    class Frame
    {
        public byte Address { get; set; }
        public byte[] Body { get; set; }
        public Frame() { }
        public Frame(byte addr, byte[] body)
        {
            // Проверка ввода
            Address = addr;
            Body = body;
        }
        public byte[] CalculateCRC16Modbus(byte[] buffer)
        {
            ushort crc = 0xFFFF;

            for (int pos = 0; pos < buffer.Length; pos++)
            {
                crc ^= (ushort)buffer[pos];

                for (int i = 8; i != 0; i--)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                        crc >>= 1;
                }
            }
            //byte lowByteCRC = (byte)((crc << 8) >> 8);
            //byte highByteCRC = (byte)(crc >> 8);
            //return new byte[] { highByteCRC, lowByteCRC };
            return BitConverter.GetBytes(crc);
        }
        public byte[] Create()
        {
            List<byte> frame = new List<byte>();
            frame.Add(Address);
            frame.AddRange(Body);
            byte[] crc = CalculateCRC16Modbus(frame.ToArray());
            frame.AddRange(crc);
            return frame.ToArray();
        }
    }
}
