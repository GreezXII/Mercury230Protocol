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
        public List<string> Pattern = new List<string> { "Address" };
        public Frame() { }
        public Frame(byte addr)
        {
            // TODO: Проверка ввода
            Address = addr;
            Length = 3;
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
        public void Print()
        {
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo p in properties)
            {
                Trace.WriteLine($"{p.Name,-8} [{p.PropertyType.Name,-8}] {p.GetValue(this)}");
            }
        }
    }

    class Response : Frame
    {
        public Response(byte[] body)
        {
            Address = body[0];
            CRC = new byte[] { body[^2], body[^1] };
        }
    }

    class Request : Frame
    {
        public byte RequestCode { get; internal set; }
        public Request(byte addr)
            : base(addr)
        {
            Length += 1;
            Pattern.Add("Request");
        }
        public byte[] Create()
        {
            List<byte> frame = new List<byte>();
            frame.AddRange(CreateBody());
            CRC = CalculateCRC16Modbus();
            frame.AddRange(CRC);
            return frame.ToArray();
        }
        private byte[] CreateBody()
        {
            List<byte> body = new List<byte>();
            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (string s in Pattern)
            {
                foreach (PropertyInfo pi in props)
                {
                    string propertyName = pi.Name;
                    if (propertyName != "CRC" && propertyName != "Length")
                    {
                        if (propertyName == s)
                        {
                            if (pi.PropertyType.Name == "Byte")
                            {
                                byte value = (byte)pi.GetValue(this);
                                body.Add(value);
                            }
                            else if (pi.PropertyType.Name == "Byte[]")
                            {
                                byte[] value = (byte[])pi.GetValue(this);
                                body.AddRange(value);
                            }
                        }
                    }
                }
            }
            return body.ToArray();
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
        internal byte[] StringToBCD(string s)
        {
            byte[] bytePassword = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                byte b = byte.Parse(s[i].ToString());  // TODO: Проверка на ввод только чисел
                bytePassword[i] = b;
            }
            return bytePassword;
        } // BCD - Binary-coded decimal
    }

    class TestLinkRequest : Request
    {
        public TestLinkRequest(byte addr)
            : base(addr)
        {
            RequestCode = (byte)RequestTypes.TestConnection;
        }
    }

    class OpenConnectionRequest : Request
    {
        public byte AccessLevel { get; private set; }
        public byte[] Password { get; private set; }
        public OpenConnectionRequest(byte addr, MeterAccessLevel accLvl, string pwd)
            : base(addr)
        {
            RequestCode = (byte)RequestTypes.OpenConnection;
            AccessLevel = (byte)accLvl;
            Password = StringToBCD(pwd);
            Pattern.AddRange( new string[] { "AccessLevel", "Password" });
            Length += 1 + Password.Length;
        }
    }

    class CloseConnectionRequest : Request
    {
        public CloseConnectionRequest(byte addr)
            : base(addr)
        {
            RequestCode = (byte)RequestTypes.CloseConnection;
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
