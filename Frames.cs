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
                Trace.WriteLine($"{p.Name,-12} {p.PropertyType.Name,-8} {p.GetValue(this)}");
            }
        }
    }

    class Response : Frame
    {
        public Response(byte[] response)
        {
            Address = response[0];
            CRC = new byte[] { response[^2], response[^1] };
        }
    }

    class ReadStoredEnergyResponse : Response
    {
        public double ActivePositive { get; private set; }
        public double ActiveNegative { get; private set; }
        public double ReactivePositive { get; private set; }
        public double ReactiveNegative { get; private set; }
        public double Phase1 { get; private set; }
        public double Phase2 { get; private set; }
        public double Phase3 { get; private set; }
        public ReadStoredEnergyResponse(byte[] response)
            : base(response)
        {
            if (response.Length == 19)
                Pattern.AddRange(new string[] { "ActivePositive", "ActiveNegative", "ReactivePositive", "ReactiveNegative" });
            if (response.Length == 15)
                Pattern.AddRange(new string[] { "Phase1", "Phase2", "Phase3" });
            ParseBody(response);
        }
        private void ParseBody(byte[] response) // TODO: упростить без использования паттернов
        {
            byte[] buffer = new byte[4];
            int index = 1;
            int step = 4;
            PropertyInfo[] props = this.GetType().GetProperties();
            for (int i = 1; i < Pattern.Count; i++)
                foreach (PropertyInfo pi in props)
                    if (pi.Name != "Address" && pi.Name == Pattern[i])
                    {
                        Buffer.BlockCopy(response, index, buffer, 0, buffer.Length);
                        pi.SetValue(this, GetEnergyValue(buffer));
                        index += step;
                        break;
                    }
        }
        private double GetEnergyValue(byte[] array)
        {
            string[] buffer = new string[array.Length];
            buffer[0] = Convert.ToString(array[1], 16);
            buffer[1] = Convert.ToString(array[0], 16);
            buffer[2] = Convert.ToString(array[3], 16);
            buffer[3] = Convert.ToString(array[2], 16);

            for (int i = 0; i < buffer.Length; i++)
                if (buffer[i].Length == 1)
                    buffer[i] = "0" + buffer[i];
            string hex = String.Join("", buffer);
            int energy = Convert.ToInt32(hex, 16);
            return energy / 1000.0d;
        }
    }

    class SerialNumberAndReleaseDateResponse : Response
    {
        public string SerialNumber { get; private set; }
        public DateTime ReleaseDate { get; private set; }
        public SerialNumberAndReleaseDateResponse(byte[] response)
            : base(response)
        {
            byte[] serialNumberBuffer = new byte[4];
            Array.Copy(response, 1, serialNumberBuffer, 0, 4);
            string hex = "";
            foreach (byte b in serialNumberBuffer) // TODO: Объедиить с GetEnergyValue
            {
                string s = Convert.ToString(b);
                if (b / 10 == 0)
                    s = "0" + s;
                hex += s;
            }
            SerialNumber = hex;

            byte[] releaseDateBuffer = new byte[3];
            Array.Copy(response, 5, releaseDateBuffer, 0, 3);
            ReleaseDate = new DateTime(releaseDateBuffer[2], releaseDateBuffer[1], releaseDateBuffer[0]);
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
            frame.AddRange(CreateBody());
            CalculateCRC16Modbus();
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

    class ReadStoredEnergyRequest : Request
    {
        public byte ArrayMonth { get; private set; }
        public byte Rate { get; private set; }
        public ReadStoredEnergyRequest(byte addr, DataArrays dataArray, Months month, Rates rate)
            :base(addr)
        {
            RequestCode = (byte)RequestTypes.ReadArray;
            ArrayMonth = CombineHalfBytes((byte)dataArray, (byte)month);
            Rate = (byte)rate;
            Pattern.AddRange(new string[] { "ArrayMonth", "Rate" });
            Length += 2;
        }
        private byte CombineHalfBytes(byte a, byte b)
        {
            a = (byte)(a << 4);
            return (byte)(a | b);
        }
    }

    class ReadSettingsRequest : Request
    {
        public byte SettingNumber { get; private set; }
        public byte[] Parameters { get; private set; }
        public ReadSettingsRequest(byte addr, SettingNumber sn, byte[] param)
            :base(addr)
        {
            RequestCode = (byte)RequestTypes.ReadSettings;
            SettingNumber = (byte)sn;
            Parameters = param;
            Pattern.AddRange(new string[] { "SettingNumber", "Parameters" });
            Length += 2;
        }
    }
}
