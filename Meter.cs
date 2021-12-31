using System;
using System.IO.Ports;
using System.Timers;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;

namespace Mercury230Protocol
{
    class Meter
    {
        SerialPort Port;
        public byte Address { get; set; }
        public MeterAccessLevels AccessLevel { get; set; }
        public string Password { get; private set; }
        private int WaitAnswerTime = 150;

        public Meter() { }
        public Meter(byte addr, string comPort, MeterAccessLevels al, string pwd, int wt)
        {
            if (addr <= 0 || addr > 240)
                throw new Exception($"Адрес счётчика {addr} является некорректным.");
            Address = addr;
            AccessLevel = al;
            Password = pwd;
            WaitAnswerTime = wt;
            Port = new SerialPort(comPort, 9600, Parity.None, 8, StopBits.One);
            Port.Open();
        }
        public bool TestLink()
        {
            TestLinkRequest request = new TestLinkRequest(Address);
            Write(request);
            Response response = new Response(Read()); // TODO: response class?
            if (response == null)
                return false;
            return true;
        }
        public bool OpenConnection()
        {
            OpenConnectionRequest request = new OpenConnectionRequest(Address, AccessLevel, Password);
            Write(request);
            Response response = new Response(Read()); // TODO: response class?
            if (response == null)
                return false;
            return true;
        }
        public bool CloseConnection()
        {
            CloseConnectionRequest request = new CloseConnectionRequest(Address);
            Write(request);
            Response response = new Response(Read()); // TODO: response class?
            if (response == null)
                return false;
            return true;
        }
        public bool ReadJournal(Journals j)
        {
            ReadJournalRecordRequest request = new ReadJournalRecordRequest(Address, j, 0);
            Write(request);
            ReadJournalResponse response = new ReadJournalResponse(Read());
            if (response == null)
                return false;
            response.Print();
            return true;
        }
        public bool ReadStoredEnergy(DataArrays da, Months m, Rates r)
        {
            ReadStoredEnergyRequest request = new ReadStoredEnergyRequest(Address, da, m, r);
            Write(request);
            ReadStoredEnergyResponse response = new ReadStoredEnergyResponse(Read());
            if (response == null)
                return false;
            response.Print();
            return true;
        }
        public bool ReadSerialNumberAndReleaseDate()
        {
            ReadSettingsRequest request = new ReadSettingsRequest(Address, Settings.SerialNumberAndReleaseDate, Array.Empty<byte>());
            Write(request);
            request.Print();
            SerialNumberAndReleaseDateResponse response = new SerialNumberAndReleaseDateResponse(Read());
            if (response == null)
                return false;
            response.Print();
            return true;
        }
        public bool ReadSoftwareVersion()
        {
            ReadSettingsRequest request = new ReadSettingsRequest(Address, Settings.SoftwareVersion, Array.Empty<byte>());
            Write(request);
            SoftwareVersionResponse response = new SoftwareVersionResponse(Read());
            if (response == null)
                return false;
            response.Print();
            return true;
        }
        public bool SetLocation(string loc)
        {
            WriteLocationRequest request = new WriteLocationRequest(Address, loc);
            request.Print();
            Write(request);
            Response response = new Response(Read());
            response.Print();
            return true;
        }
        public bool GetLocation()
        {
            ReadSettingsRequest request = new ReadSettingsRequest(Address, Settings.Location, Array.Empty<byte>());
            Write(request);
            LocationResponse response = new LocationResponse(Read());
            if (response == null)
                return false;
            response.Print();
            return true;
        }
        private void Write(Request f)
        {
            byte[] buffer = f.Create();
            Port.Write(buffer, 0, buffer.Length);
        }
        private byte[] Read()
        {
            Thread.Sleep(WaitAnswerTime);
            if (Port.BytesToRead == 0)
                throw new Exception("Нет ответа от счётчика");
            byte[] buffer = new byte[Port.BytesToRead];
            Port.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}
