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
            Response response = Read();
            if (response == null)
                return false;
            return true;
        }
        public bool OpenConnection()
        {
            OpenConnectionRequest request = new OpenConnectionRequest(Address, AccessLevel, Password);
            Write(request);
            Response response = Read();
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
            Response response = Read();
            IsConnected = false;
            if (response == null)
                return false;
            ConnectionTimer.Stop();
            return true;
        }
        public bool ReadStoredEnergy()
        {
            ReadStoredEnergyRequest request = new ReadStoredEnergyRequest(Address,
                                                                          DataArrays.FromReset,
                                                                          Months.None,
                                                                          Rates.Sum);
            request.Print();
            Write(request);
            Response response = Read();
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
        private Response Read()
        {
            Thread.Sleep(WaitAnswerTime);
            if (Port.BytesToRead == 0)
                return null;
            byte[] buffer = new byte[Port.BytesToRead];
            Port.Read(buffer, 0, buffer.Length);
            Response response = new Response(buffer);
            return response;
        }
        private void ConnectionExpired(object o, ElapsedEventArgs e)
        {
            if (AutoReconnect == true)
                OpenConnection(); 
        }
    }
}
