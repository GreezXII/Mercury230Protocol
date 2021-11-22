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
            //Port.Open();
        }
        public async void TestConnection()
        {
            //byte[] request = { 0x00 };
            //Frame requestFrame = new Frame(Address, request);
            //await WriteAsync(requestFrame);

            //Frame responseFrame = await ReadAsync();

            Frame f1 = new Frame(new byte[] { 89, 0x00, 0x3b, 0x0f });
            Frame f2 = new Frame(new byte[] { 89, 0x00, 0x3b, 0x0f });
            Frame f3 = new Frame(new byte[] { 90, 0x00, 0x3b, 0x0f });
            Frame f4 = new Frame(new byte[] { 89, 0x01, 0x3b, 0x0f });

            f1.Print();
            f2.Print();
            Trace.WriteLine($"f1 == f2 -> {f1 == f2}");
            Trace.WriteLine($"f1 != f2 -> {f1 != f2}");

            f3.Print();
            Trace.WriteLine($"f1 == f3 -> {f1 == f3}");
            Trace.WriteLine($"f1 != f3 -> {f1 != f3}");

            f4.Print();
            Trace.WriteLine($"f1 == f4 -> {f1 == f4}");
            Trace.WriteLine($"f1 != f4 -> {f1 != f4}");
        }

        private async Task WriteAsync(Frame f)
        {
            await Task.Run(() =>
            {
                byte[] buffer = f.ToArray();
                Port.Write(buffer, 0, buffer.Length);
            });
        }
        private async Task<Frame> ReadAsync()
        {
            return await Task.Run(() =>
                {
                    Thread.Sleep(WaitAnswerTime);
                    byte[] buffer = new byte[Port.BytesToRead];
                    Port.Read(buffer, 0, buffer.Length);
                    Frame response = new Frame(buffer);
                    return response;
                });
        }

        private bool MatchCRC(Frame a, Frame b)
        {
            return a.CRC.Equals(b.CRC);
        }
    }
}
