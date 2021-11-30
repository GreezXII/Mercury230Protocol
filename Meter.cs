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
        public bool IsConnected { get; private set; }
        public bool AutoReconnect { get; set; }
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
        public bool TestConnection()
        {
            TestConnectionRequest request = new TestConnectionRequest(Address);
            Write(request);
            //byte[] request = new byte[] { 0x00 };
            //Frame requestFrame = new Frame(Address, request);
            //Write(requestFrame);
            //Frame responseFrame = Read();
            //// TODO: найти способ получать подтверждение иначе
            //bool result = (responseFrame.Address == requestFrame.Address) && responseFrame.Body[0] == 0x00;
            //return result;
            return true;
        }
        //public void OpenConnection()
        //{
        //    // TODO: Способ формирования запроса со сложными параметрами (отдельный класс от Frame?)
        //    List<byte> requestConstructor = new List<byte>();
        //    requestConstructor.Add((byte)RequestTypes.OpenConnection);
        //    requestConstructor.Add((byte)AccessLevel);
        //    requestConstructor.AddRange(StringToBytes(Password));

        //    byte[] request = requestConstructor.ToArray();
        //    Frame requestFrame = new Frame(Address, request);
        //    Write(requestFrame);
        //    Frame responseFrame = Read();
        //    // TODO
        //    bool result = (responseFrame.Address == requestFrame.Address) && responseFrame.Body[0] == 0x00;
        //    if (result)
        //    {
        //        IsConnected = true;
        //        ConnectionTimer.Start();
        //    }
        //    Trace.WriteLine("OpenConnection");
        //}
        //public void CloseConnection()
        //{
        //    byte[] request = { (byte)RequestTypes.CloseConnection };
        //    Frame requestFrame = new Frame(Address, request);
        //    Write(requestFrame);
        //    Frame responseFrame = Read();
        //    // TODO
        //    bool result = (responseFrame.Address == requestFrame.Address) && responseFrame.Body[0] == 0x00;
        //    if (result)
        //    {
        //        IsConnected = false;
        //        ConnectionTimer.Stop();
        //    }
        //    Trace.WriteLine("CloseConnection");
        //}
        private void Write(Frame f)
        {
            byte[] buffer = f.ToArray();
            Port.Write(buffer, 0, buffer.Length);
        }
        private Frame Read()
        {
            Thread.Sleep(WaitAnswerTime);
            if (Port.BytesToRead == 0)
                throw new Exception("Нет ответа.");
            byte[] buffer = new byte[Port.BytesToRead];
            Port.Read(buffer, 0, buffer.Length);
            Frame response = new Frame(buffer);
            return response;
        }
        private byte[] StringToBytes(string s)
        {
            byte[] bytePassword = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                byte b = byte.Parse(s[i].ToString());  // TODO: Проверка на ввод только чисел
                bytePassword[i] = b;
            }
            return bytePassword;
        }
        private void ConnectionExpired(object o, ElapsedEventArgs e)
        {
            if (AutoReconnect == true)
                OpenConnection();
        }
        public void Test()
        {
            Frame f = new Frame(89);
            f.Print();
        }
    }
}
