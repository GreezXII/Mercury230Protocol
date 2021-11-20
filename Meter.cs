using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercury230Protocol
{
    class Meter
    {
        // Поддержка разных скоростей
        SerialPort Port = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);
        public byte Address { get; set; }
        public MeterAccessLevel AccessLevel { get; set; } = MeterAccessLevel.User;
        public string Password { get; set; }
        public bool AutoReconnect = false;
        //private Timer ConnectionTimer = new Timer();
        private int WaitAnswerTime = 150; // TODO: Разные значения для разных скоростей

        public Meter(byte addr, MeterAccessLevel al, string pwd)
        {
            // TODO: Проверка ввода
            Address = addr;
            AccessLevel = al;
            Password = pwd;
            Port.Open();
        }
        public void TestConnection()
        {
            //Port.Open();
            byte[] request = { 0x00 };
            Frame f = new Frame(Address, request);
            byte[] buffer = f.Create();
            Print(buffer);
            Port.Write(buffer, 0, buffer.Length);
            Port.DataReceived += new SerialDataReceivedEventHandler(Read);
        }

        private void Read(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[Port.BytesToRead];
            Port.Read(buffer, 0, buffer.Length);
            Print(buffer);
        }

        private void Print(byte[] array)
        {
            foreach (byte b in array)
            {
                string a = Convert.ToString(b, 16);
                Trace.Write($"{a} ");
            }
        }
    }
}
