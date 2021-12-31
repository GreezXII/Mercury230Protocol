﻿using System;
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
