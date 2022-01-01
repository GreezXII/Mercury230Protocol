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
        public string PortName;
        public int BaudRate;
        public Parity PortParity;
        public int DataBits;
        public StopBits PortStopBits;
        public int WriteTimeout;
            
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

            PortName = comPort;
            BaudRate = 9600;
            PortParity = Parity.None;
            DataBits = 8;
            PortStopBits = StopBits.One;
            WriteTimeout = 10000;
            Port = new SerialPort(PortName, BaudRate, PortParity, DataBits, PortStopBits);
        }
        public bool TestLink()
        {
            TestLinkRequest request = new TestLinkRequest(Address);
            byte[] buffer = RunCommand(request);
            Response response = new Response(buffer);
            if (response.Body.Length == 1 && response.Body[0] == 0)
                return true;
            return false;
        }
        public bool OpenConnection()
        {
            OpenConnectionRequest request = new OpenConnectionRequest(Address, AccessLevel, Password);
            byte[] buffer = RunCommand(request);
            Response response = new Response(buffer);
            if (response.Body.Length == 1 && response.Body[0] == 0)
                return true;
            return false;
        }
        public bool CloseConnection()
        {
            CloseConnectionRequest request = new CloseConnectionRequest(Address);
            byte[] buffer = RunCommand(request);
            Response response = new Response(buffer);
            if (response.Body.Length == 1 && response.Body[0] == 0)
                return true;
            return false;
        }
        public bool ReadJournal(Journals j)  // TODO: Возврат значения
        {
            ReadJournalRecordRequest request = new ReadJournalRecordRequest(Address, j, 0);
            byte[] buffer = RunCommand(request);
            ReadJournalResponse response = new ReadJournalResponse(buffer);
            if (response == null)
                return false;
            return true;
        }
        public bool ReadStoredEnergy(DataArrays da, Months m, Rates r) //TODO: Возврат значения
        {
            ReadStoredEnergyRequest request = new ReadStoredEnergyRequest(Address, da, m, r);
            byte[] buffer = RunCommand(request);
            ReadStoredEnergyResponse response = new ReadStoredEnergyResponse(buffer);
            if (response == null)
                return false;
            return true;
        }
        public bool ReadSerialNumberAndReleaseDate() // TODO: Возврат значения
        {
            ReadSettingsRequest request = new ReadSettingsRequest(Address, Settings.SerialNumberAndReleaseDate, Array.Empty<byte>());
            byte[] buffer = RunCommand(request);
            SerialNumberAndReleaseDateResponse response = new SerialNumberAndReleaseDateResponse(buffer);
            if (response == null)
                return false;
            return true;
        }
        public bool ReadSoftwareVersion() // TODO: Возврат значения
        {
            ReadSettingsRequest request = new ReadSettingsRequest(Address, Settings.SoftwareVersion, Array.Empty<byte>());
            byte[] buffer = RunCommand(request);
            SoftwareVersionResponse response = new SoftwareVersionResponse(buffer);
            if (response == null)
                return false;
            response.Print();
            return true;
        }
        public bool SetLocation(string loc) // TODO: Возврат значения
        {
            WriteLocationRequest request = new WriteLocationRequest(Address, loc);
            byte[] buffer = RunCommand(request);
            Response response = new Response(buffer);
            response.Print();
            return true;
        }
        public bool GetLocation() // Возврат значения
        {
            ReadSettingsRequest request = new ReadSettingsRequest(Address, Settings.Location, Array.Empty<byte>());
            byte[] buffer = RunCommand(request);
            LocationResponse response = new LocationResponse(buffer);
            if (response == null)
                return false;
            response.Print();
            return true;
        }
        private byte[] RunCommand(Request f)
        {
            byte[] writeBuffer = f.Create();
            using (Port)
            {
                Port.Open();
                Port.Write(writeBuffer, 0, writeBuffer.Length);
                Thread.Sleep(WaitAnswerTime);
                byte[] readBuffer = new byte[Port.BytesToRead];
                if (Port.BytesToRead == 0)
                    throw new Exception("Нет ответа от счётчика.");
                Port.Read(readBuffer, 0, readBuffer.Length);
                return readBuffer;
            }
        }
    }
}
