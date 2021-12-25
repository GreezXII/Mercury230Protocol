using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercury230Protocol
{
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

}
