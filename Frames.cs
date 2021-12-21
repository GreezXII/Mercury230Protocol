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
            {
                byte[] buffer = new byte[4];
                Array.Copy(response, 1, buffer, 0, 4);
                ActivePositive = GetEnergyValue(buffer);
                Array.Copy(response, 5, buffer, 0, 4);
                ActiveNegative = GetEnergyValue(buffer);
                Array.Copy(response, 9, buffer, 0, 4);
                ReactivePositive = GetEnergyValue(buffer);
                Array.Copy(response, 13, buffer, 0, 4);
                ReactiveNegative = GetEnergyValue(buffer);
            }
            if (response.Length == 15)
            {
                byte[] buffer = new byte[4];
                Array.Copy(response, 1, buffer, 0, 4);
                Phase1 = GetEnergyValue(buffer);
                Array.Copy(response, 1, buffer, 0, 4);
                Phase2 = GetEnergyValue(buffer);
                Array.Copy(response, 1, buffer, 0, 4);
                Phase3 = GetEnergyValue(buffer);
            }
        }
        private double GetEnergyValue(byte[] array)
        {
            // Изменить порядок байт согласно документации
            byte[] buffer = new byte[array.Length];
            buffer[0] = array[1];
            buffer[1] = array[0];
            buffer[2] = array[3];
            buffer[3] = array[2];

            return FullHexToInt(buffer) / 1000.0d;
        }
    }

    class SerialNumberAndReleaseDateResponse : Response
    {
        public int SerialNumber { get; private set; }
        public DateTime ReleaseDate { get; private set; }
        public SerialNumberAndReleaseDateResponse(byte[] response)
            : base(response)
        {
            byte[] serialNumberBuffer = new byte[4];
            Array.Copy(response, 1, serialNumberBuffer, 0, 4);
            SerialNumber = BiwiseBytesToInt(serialNumberBuffer);

            byte[] releaseDateBuffer = new byte[3];
            Array.Copy(response, 5, releaseDateBuffer, 0, 3);
            ReleaseDate = new DateTime(2000 + releaseDateBuffer[2], releaseDateBuffer[1], releaseDateBuffer[0]);
        }
    }

    class SoftwareVersionResponse : Response
    {
        public string SoftwareVersion { get; private set; }
        public SoftwareVersionResponse(byte[] response)
            : base(response)
        {
            byte[] buffer = new byte[3];
            Array.Copy(response, 1, buffer, 0, 3);

            SoftwareVersion = buffer[0].ToString();
            for (int i = 1; i < buffer.Length; i++)
            {
                SoftwareVersion += "." + buffer[i].ToString();
            }
        }
    }

    class ReadJournalResponse : Response
    {
        public List<DateTime> Records { get; private set; } = new List<DateTime>();
        public ReadJournalResponse(byte[] response)
            : base(response)
        {
            byte[] buffer = new byte[6];
            if (response.Length == 9)  // В ответе присутствует одна дата
            {
                Array.Copy(response, 1, buffer, 0, 6);
                Records.Add(ParseDateTime(buffer));
            }
            if (response.Length == 15) // В ответе присутствует две даты
            {
                Array.Copy(response, 1, buffer, 0, 6);
                Records.Add(ParseDateTime(buffer));
                Array.Copy(response, 7, buffer, 0, 6);
                Records.Add(ParseDateTime(buffer));
            }
            foreach (DateTime dt in Records)
                Trace.WriteLine(dt);
        }
        private DateTime ParseDateTime(byte[] buffer)
        {
            byte year = ByteToHexByte(buffer[5]);
            if (year == 0)
                return default(DateTime);
            byte month = ByteToHexByte(buffer[4]);
            byte day = ByteToHexByte(buffer[3]);
            byte hour = ByteToHexByte(buffer[2]);
            byte minute = ByteToHexByte(buffer[1]);
            byte second = ByteToHexByte(buffer[0]);
            return new DateTime(2000 + year, month, day, hour, minute, second);
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
            : base(addr)
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
        public ReadSettingsRequest(byte addr, Settings sn, byte[] param)
            : base(addr)
        {
            RequestCode = (byte)RequestTypes.ReadSettings;
            SettingNumber = (byte)sn;
            Parameters = param;
            Pattern.AddRange(new string[] { "SettingNumber", "Parameters" });
            Length += 2;
        }
    }

    class ReadJournalRecordRequest : Request
    {
        public byte JournalNumber { get; private set; }
        public byte RecordNumber { get; private set; }
        public ReadJournalRecordRequest(byte addr, Journals j, byte n)
            : base(addr)
        {
            RequestCode = (byte)RequestTypes.ReadJournal;
            JournalNumber = (byte)j;
            RecordNumber = n;
            Pattern.AddRange(new string[] { "JournalNumber", "RecordNumber" });
            Length += 2;
        }
    }
}
