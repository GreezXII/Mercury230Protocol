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
        // Поддержка разных скоростей
        SerialPort Port = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);
        public byte Address { get; set; }
        public MeterAccessLevel AccessLevel { get; set; }
        public string Password { get; private set; }
        private int WaitAnswerTime = 150; // TODO: Разные значения для разных скоростей
        public bool AutoReconnect { get; set; }
        public bool IsConnected { get; private set; }
        private readonly System.Timers.Timer ConnectionTimer = new System.Timers.Timer() { Interval = 240000, AutoReset = true };

        public Meter(byte addr, MeterAccessLevel al, string pwd, bool AutoReconnect = false)
        {
            // TODO: Проверка ввода
            Address = addr;
            AccessLevel = al;
            Password = pwd;
            Port.Open();  // TODO: Исправить Access to the path 'COM1' is denied
            ConnectionTimer.Elapsed += new ElapsedEventHandler(ConnectionExpired);
        }
        public bool TestLink()
        {
            TestLinkRequest request = new TestLinkRequest(Address);
            request.Print();
            Write(request);
            Response response = Read();
            response.Print();
            return true; // TODO: Проверка успешности ответа
        }
        public bool OpenConnection()
        {
            OpenConnectionRequest request = new OpenConnectionRequest(Address, AccessLevel, Password);
            request.Print();
            Write(request);
            Response response = Read();
            response.Print();
            IsConnected = true;
            return true;  // TODO: Проверка успешности ответа
        }
        public bool CloseConnection()
        {
            CloseConnectionRequest request = new CloseConnectionRequest(Address);
            Write(request);
            Response response = Read();
            IsConnected = false;
            return true;  // TODO: Проверка успешности ответа
        }
        private void Write(Frame f)
        {
            byte[] buffer = f.ToArray();
            Port.Write(buffer, 0, buffer.Length);
        }
        private Response Read()
        {
            Thread.Sleep(WaitAnswerTime);
            if (Port.BytesToRead == 0)
                throw new Exception("Нет ответа.");
            byte[] buffer = new byte[Port.BytesToRead];
            Port.Read(buffer, 0, buffer.Length);
            Response response = new Response(buffer);
            return response;
        }
        private void ConnectionExpired(object o, ElapsedEventArgs e)
        {
            //if (AutoReconnect == true)
                //OpenConnection(); //TODO 
        }
    }
}
