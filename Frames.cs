using System;
using System.Collections.Generic;
using System.Text;

namespace Mercury230Protocol
{
    class Frame
    {
        public byte Address { get; set; }

        public byte[] Body { get; set; }

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
