using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercury230Protocol
{
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
        public OpenConnectionRequest(byte addr, MeterAccessLevels accLvl, string pwd)
            : base(addr)
        {
            RequestCode = (byte)RequestTypes.OpenConnection;
            AccessLevel = (byte)accLvl;
            Password = StringToBCD(pwd);
            Pattern.AddRange(new string[] { "AccessLevel", "Password" });
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

    class WriteRateScheduleRequest : Request
    {
        public byte ParameterNumber { get; private set; }
        public byte[] MMSKH { get; private set; }   // Маска месяцев
        public byte WDPM { get; private set; }        // Маска дней недели и праздников
        public byte[] TRECORDH { get; private set; }  // Тарифные интервалы расписания
        public WriteRateScheduleRequest(byte addr, MMSKH mm, WDPM dm, TRECORDH recs)
            : base(addr)
        {
            RequestCode = (byte)RequestTypes.WriteSettings;
            ParameterNumber = 0x1D;
            MMSKH = mm.Create();
            WDPM = dm.Create();
            TRECORDH = recs.Create();
            Pattern.AddRange(new string[] { "ParameterNumber", "MMSKH", "WDPM", "TRECORDH" });
            Length += 4;
        }
    }
    // Закодированные значения времени начала интервала и номера тарифа
    class TTF
    {
        public byte Rate { get; private set; }
        public byte Hour { get; private set; }
        public TTF(Rates r, byte h)
        {
            Rate = (byte)r;
            if (Rate < 1 || Rate > 3)  // Неверное значение тарифа
                throw new Exception($"Выбрано неверное значение для тарифа в расписании тарифов: {Rate}");
            Hour = h;                  // Неверное значение начала временного интервала
            if (Hour < 0 || Hour > 24)
                throw new Exception($"Выбрано неверное значение начала временного интервала тарифа: {Hour}");
        }
        public byte[] Create()
        {
            byte[] TTF = new byte[2];
            TTF[0] = 0; // 0 указывает, что поминутная тарификация не используется
            TTF[1] = (byte)((Rate << 5) | Hour);  // Поместить значение тарифа и часа в один байт
            return TTF;
        }
    }
    // Закодированный массив тарифных интервалов на сутки
    class TRECORDH
    {
        public List<TTF> Records = new List<TTF>(8);
        public TRECORDH(List<TTF> arrayTTF)
        {
            if (arrayTTF.Count < 2)
                throw new Exception("Неправильный формат записи интервалов: недостаточное количество записей.");
            if (arrayTTF.Count > 8)
                throw new Exception("Неправильный формат записи интервалов: слишком много записей.");

            Records.AddRange(arrayTTF);
            Records.Sort((a, b) => a.Hour.CompareTo(b.Hour));  // Интервалы должны быть отсортированы

            if (Records.Count < 8)  // Дополнить значение до 16 байт
                for (int i = Records.Count; i < 8; i++)
                    Records.Add(new TTF(Rates.Rate1, 24));

            if (Records[0].Hour != 0)
                throw new Exception("Неправильный формат записи интервалов: отсутствует запись на начало дня (0 часов)");
            if (Records[^1].Hour != 24)
                throw new Exception("Неправильный формат записи интервалов: отсутствует запись на конец дня (24 часа)");
        }
        public byte[] Create()
        {
            byte[] buffer = new byte[16];
            int index = 0;
            byte[] ba = new byte[2];
            foreach (TTF ttf in Records)
            {
                ba = ttf.Create();
                Array.Copy(ba, 0, buffer, index, 2);
                index += 2;
            }
            return buffer;
        }
    }
    // Маска дней недели и праздников  для чтения-записи тарифного расписания
    class WDPM
    {
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }
        public bool Holiday { get; set; }

        public byte Create()
        {
            byte buffer = 0b0000_0000;
            if (Monday)
                buffer = (byte)(buffer | 0b00000001);
            if (Tuesday)
                buffer = (byte)(buffer | 0b00000010);
            if (Wednesday)
                buffer = (byte)(buffer | 0b00000100);
            if (Thursday)
                buffer = (byte)(buffer | 0b00001000);
            if (Friday)
                buffer = (byte)(buffer | 0b00010000);
            if (Saturday)
                buffer = (byte)(buffer | 0b00100000);
            if (Sunday)
                buffer = (byte)(buffer | 0b01000000);
            if (Holiday)
                buffer = (byte)(buffer | 0b10000000);

            return buffer;
        }
    }
    // Маска месяцев для чтения-записи тарифного расписания
    class MMSKH
    {
        public bool January { get; set; }
        public bool February { get; set; }
        public bool March { get; set; }
        public bool April { get; set; }
        public bool May { get; set; }
        public bool June { get; set; }
        public bool July { get; set; }
        public bool August { get; set; }
        public bool September { get; set; }
        public bool October { get; set; }
        public bool November { get; set; }
        public bool December { get; set; }

        public byte[] Create()
        {
            byte b1 = 0b0000_0000;
            byte b2 = 0b0000_0000;

            if (January)
                b2 = (byte)(b2 | 0b00000001);
            if (February)
                b2 = (byte)(b2 | 0b00000010);
            if (March)
                b2 = (byte)(b2 | 0b00000100);
            if (April)
                b2 = (byte)(b2 | 0b00001000);
            if (May)
                b2 = (byte)(b2 | 0b00010000);
            if (June)
                b2 = (byte)(b2 | 0b00100000);
            if (July)
                b2 = (byte)(b2 | 0b01000000);
            if (August)
                b2 = (byte)(b2 | 0b10000000);
            if (September)
                b1 = (byte)(b1 | 0b00000001);
            if (October)
                b1 = (byte)(b1 | 0b00000010);
            if (November)
                b1 = (byte)(b1 | 0b00000100);
            if (December)
                b1 = (byte)(b1 | 0b00001000);

            return new byte[] { b1, b2 };
        }
    }
    class ChangePasswordRequest: Request
    {
        public byte ParameterNumber { get; private set; }
        public byte AccessLevel { get; private set; }
        public byte[] OldPassword { get; private set; }
        public byte[] NewPassword { get; private set; }
        public ChangePasswordRequest(byte addr, MeterAccessLevels al, string op, string np)
            : base(addr)
        {
            if (op.Length != 6 || np.Length != 6)
                throw new Exception("Пароль должен состоять из 6 символов.");
            // Для пароля принимаются только цифры и буквы
            for (int i = 0; i < op.Length; i++)
                if (!char.IsLetterOrDigit(op[i]) || !char.IsLetterOrDigit(np[i]))
                    throw new Exception("Пароль должен состоять только из букв и цифр.");
            RequestCode = (byte)RequestTypes.WriteSettings;
            ParameterNumber = 0x1F;
            AccessLevel = (byte)al;
            OldPassword = Encoding.ASCII.GetBytes(op);
            NewPassword = Encoding.ASCII.GetBytes(np);
            Pattern.AddRange(new string[] { "ParameterNumber", "AccessLevel", "OldPassword", "NewPassword" });
            Length += 4;
        }
    }
    class WriteLocationRequest : Request
    {
        public byte ParameterNumber { get; private set; }
        public byte[] Location { get; private set; }
        public WriteLocationRequest(byte addr, string loc)
            : base(addr)
        {
            if (loc.Length < 0 || loc.Length > 4)
                throw new Exception("Местоположение может содержать от 0 до 4 символов.");
            RequestCode = (byte)RequestTypes.WriteSettings;
            ParameterNumber = 0x22;
            loc = loc.PadRight(4, ' ');
            Location = Encoding.ASCII.GetBytes(loc);
            Pattern.AddRange(new string[] { "ParameterNumber", "Location" });
            Length += 2;
        }
    }
}
