using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Buffers;

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
            Address = addr;
            Length = 3;
        }
        internal byte[] CalculateCRC16Modbus(byte[] buffer)
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
            CRC = BitConverter.GetBytes(crc);
            return CRC;
        }
        public static bool CRCMatch(byte[] a, byte[] b)
        {
            if (a == null || b == null)
                return false;
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;

            return true;
        }
        public void Print()
        {
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo p in properties)
            {
                Trace.WriteLine($"{p.Name,-20} {p.PropertyType.Name,-8} {p.GetValue(this)}");
            }
        }
    }

    class Response : Frame
    {
        public Response(byte[] response)
        {
            Address = response[0];
            CRC = new byte[] { response[^2], response[^1] };
            if (!CheckCRC(response))
                throw new Exception("CRC принятого пакета не совпадает с полученным значением CRC при проверке.");
            foreach (byte b in response)
                Trace.Write($"{Convert.ToString(b, 16)} ");
            Trace.WriteLine("");
        }
        private bool CheckCRC(byte[] response)
        {
            byte[] buffer = new byte[response.Length - 2];
            Array.Copy(response, 0, buffer, 0, response.Length - 2);
            byte[] CRCval = CalculateCRC16Modbus(buffer);
            return CRCMatch(CRC, CRCval);
        }
        internal int FullHexToInt(byte[] buffer)
        {
            string hex = "";
            for (int i = 0; i < buffer.Length; i++)
                hex += ByteToHexString(buffer[i]);
            return Convert.ToInt32(hex, 16);
        }
        internal int BiwiseBytesToInt(byte[] buffer)
        {
            int result = buffer[0];
            for (int i = 1; i < buffer.Length; i++)
            {
                result *= 100;
                result += buffer[i];
            }
            return result;
        }
        internal string ByteToHexString(byte b)
        {
            string hex = Convert.ToString(b, 16);
            if (hex.Length == 1)
                hex = "0" + hex;
            return hex;
        }
        internal byte ByteToHexByte(byte b)
        {
            string hex = ByteToHexString(b);
            return byte.Parse(hex);
        }
    }

    class Request : Frame
    {
        public byte RequestCode { get; internal set; }
        public Request(byte addr)
            : base(addr)
        {
            Length += 1;
            Pattern.Add("RequestCode");
        }
        public byte[] Create()
        {
            List<byte> frame = new List<byte>();
            byte[] body = CreateBody();
            frame.AddRange(body);
            CalculateCRC16Modbus(body);
            frame.AddRange(CRC);
            return frame.ToArray();
        }
        private byte[] CreateBody()
        {
            List<byte> body = new List<byte>();
            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (string s in Pattern)
                foreach (PropertyInfo pi in props)
                    if (pi.Name == s)
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
                        break;
                    }
            return body.ToArray();
        }
        internal byte[] StringToBCD(string s)  // BCD - Binary-coded decimal
        {
            byte[] bytePassword = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                byte b;
                bool result = byte.TryParse(s[i].ToString(), out b);
                if (!result)
                    throw new Exception($"Не удалось преобразовать {s} в двоично-десятичное представление.");
                bytePassword[i] = b;
            }
            return bytePassword;
        }
    }

}