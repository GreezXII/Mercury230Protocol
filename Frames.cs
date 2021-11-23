using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        public byte Address { get; private set; }
        public byte[] Body { get; private set; }
        public byte[] CRC { get; private set; }
        public int Length { get; private set; }
        public Frame(byte addr, byte[] body)
        {
            // TODO: Проверка ввода
            Address = addr;
            Body = body;
            CRC = CalculateCRC16Modbus();
            Length = Body.Length + CRC.Length + 1;
        }
        public Frame(byte[] array)
        {
            Length = array.Length;
            // Адрес счётчика
            Address = array[0];
            // Тело запроса
            byte[] b = new byte[Length - 3];  // Исключить адрес счётчика и CRC
            for (int i = 0; i < Length - 3; i++)
            {
                b[i] = array[i + 1];
            }
            Body = b;
            // CRC
            byte[] crc = { array[Length - 2], array[Length - 1] };
            CRC = crc;
        }
        public static bool operator == (Frame a, Frame b)
        {   
            if (a is null)
                return b is null;
            if (b is null)
                return false;

            if (a.Length != b.Length)
                return false;

            byte[] b1 = a.ToArray();
            byte[] b2 = b.ToArray();

            for (int i = 0; i < a.Length; i++)
                if (b1[i] != b2[i])
                    return false;
            
            return true;
        }
        public static bool operator !=(Frame a, Frame b)
        {
            if (a.Length != b.Length)
                return true;

            byte[] b1 = a.ToArray();
            byte[] b2 = b.ToArray();

            for (int i = 0; i < a.Length; i++)
                if (b1[i] != b2[i])
                    return true;

            return false;
        }
        private byte[] CalculateCRC16Modbus()
        {
            List<byte> frameFields = new List<byte>();
            frameFields.Add(Address);
            frameFields.AddRange(Body);
            byte[] buffer = frameFields.ToArray();

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
            CRC = BitConverter.GetBytes(crc);
            return CRC;
        }
        public static bool CRCMatch(Frame a, Frame b)
        {
            if (a == null || b == null)
                return false;
            if (a.CRC.Length != b.CRC.Length)
                return false;
            for (int i = 0; i < a.CRC.Length; i++)
                if (a.CRC[i] != b.CRC[i])
                    return false;

            return true;
        }
        public byte[] ToArray()
        {
            List<byte> frame = new List<byte>();
            frame.Add(Address);
            frame.AddRange(Body);
            frame.AddRange(CRC);
            return frame.ToArray();
        }
        public void Print()
        {
            Trace.WriteLine($"Address: {Address}");
            Trace.Write($"Body: ");
            foreach (byte b in Body)
                Trace.Write($"{b}");
            Trace.WriteLine("");
            Trace.Write($"CRC: ");
            foreach (byte b in CRC)
                Trace.Write($"{b}");
            Trace.WriteLine("");
        }
    }
}
