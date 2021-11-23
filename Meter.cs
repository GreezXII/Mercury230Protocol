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
        public async void TestConnection()
        {
            byte[] request = new byte[] { 0x01 };
            Frame requestFrame = new Frame(Address, request);
            await WriteAsync(requestFrame);
            Frame responseFrame = await ReadAsync();
            Trace.WriteLine($"CRCMatch = {Frame.CRCMatch(requestFrame, responseFrame)}");
            requestFrame.Print();
            if (!(responseFrame is null))
                responseFrame.Print();
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
                    // Если нет ответа в течение времени ожидания, вернуть null
                    Thread.Sleep(WaitAnswerTime);
                    if (Port.BytesToRead == 0)  
                        return null;
                    // Иначе сформировать кадр
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
