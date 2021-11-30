using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Mercury230Protocol
{
    class Frame
    {
        public byte Address { get; internal set; }
        public byte[] CRC { get; internal set; }
        public int Length { get; internal set; }
        public Frame(byte addr)
        {
            // TODO: Проверка ввода
            Address = addr;
            Length = CRC.Length + 1;
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
        internal byte[] CalculateCRC16Modbus()
        {
            byte[] buffer = CreateBody();
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
        public byte[] ToArray()
        {
            List<byte> frame = new List<byte>();
            frame.AddRange(CreateBody());
            frame.AddRange(CRC);
            return frame.ToArray();
        }
        private byte[] CreateBody()
        {
            List<byte> body = new List<byte>();
            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (PropertyInfo pi in props)
            {
                string propertyName = pi.Name;
                if (propertyName != "CRC" && propertyName != "Length")
                {
                    string propertyTypeName = pi.PropertyType.Name;
                    if (propertyTypeName == "Byte")
                    {
                        byte value = (byte)pi.GetValue(this);
                        body.Add(value);
                    }
                    else if (propertyTypeName == "Byte[]")
                    {
                        byte[] value = (byte[])pi.GetValue(this);
                        body.AddRange(value);
                    }
                }
            }
            return body.ToArray();
        }
        public void Print()
        {
            byte[] request = ToArray();
            foreach (byte b in request)
                Trace.WriteLine(b);
        }
    }

    //class ReadArrayRequest : Frame
    //{
    //    public DataArrays DataArray { get; set; }
    //    public Months Month { get; set; }
    //    public Rates Rate { get; set; }
    //    public ReadArrayRequest(byte addr, DataArrays dataArray, Months month, Rates rate)
    //    {
    //        Address = addr;
    //        DataArray = dataArray;
    //        Month = month;
    //        Rate = rate;
    //        Length = 6;
    //        //CRC = CalculateCRC16Modbus();

    //    }
    //}
}
