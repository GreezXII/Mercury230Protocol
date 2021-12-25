﻿using System;
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
        SerialPort Port = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);
        public byte Address { get; set; }
        public MeterAccessLevel AccessLevel { get; set; }
        public string Password { get; private set; }
        private int WaitAnswerTime = 150;
        public bool AutoReconnect { get; set; }
        public bool IsConnected { get; private set; }
        private readonly System.Timers.Timer ConnectionTimer = new System.Timers.Timer() { Interval = 240000, AutoReset = true };

        public Meter(byte addr, MeterAccessLevel al, string pwd, bool AutoReconnect = false)
        {
            if (addr <= 0 || addr > 240)
                throw new Exception($"Адрес счётчика {addr} является некорректным.");
            Address = addr;
            AccessLevel = al;
            Password = pwd;
            Port.Open();
            ConnectionTimer.Elapsed += new ElapsedEventHandler(ConnectionExpired);
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
            IsConnected = true;
            if (response == null)
                return false;
            ConnectionTimer.Start();
            return true;
        }
        public bool CloseConnection()
        {
            CloseConnectionRequest request = new CloseConnectionRequest(Address);
            Write(request);
            Response response = new Response(Read()); // TODO: response class?
            IsConnected = false;
            if (response == null)
                return false;
            ConnectionTimer.Stop();
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
        public bool WriteRateSchedule(MMSKH mm, WDPM dm, TRECORDH recs)
        {
            WriteRateScheduleRequest request = new WriteRateScheduleRequest(Address, mm, dm, recs);
            byte[] req = request.Create();
            foreach (byte b in req)
                Trace.Write($"{Convert.ToString(b, 16)}-");
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
                return null;
            byte[] buffer = new byte[Port.BytesToRead];
            Port.Read(buffer, 0, buffer.Length);
            return buffer;
        }
        private void ConnectionExpired(object o, ElapsedEventArgs e)
        {
            if (AutoReconnect == true)
                OpenConnection();
        }
    }
}
