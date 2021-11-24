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
        public MeterAccessLevel AccessLevel { get; set; }
        public string Password { get; private set; }
        private int WaitAnswerTime = 150; // TODO: Разные значения для разных скоростей

        public Meter(byte addr, MeterAccessLevel al, string pwd)
        {
            // TODO: Проверка ввода
            Address = addr;
            AccessLevel = al;
            Password = pwd;
            Port.Open();  // TODO: Исправить Access to the path 'COM1' is denied
        }
        public void TestConnection()
        {
            byte[] request = new byte[] { 0x00 };
            Frame requestFrame = new Frame(Address, request);
            Write(requestFrame);
            Frame responseFrame = Read();
            // TODO: найти способ получать подтверждение иначе
            //bool result = (responseFrame.Address == requestFrame.Address) && responseFrame.Body[0] == 0x00;
            responseFrame.Print();
        }
        public void OpenConnection()
        {
            // TODO: Способ формирования запроса со сложными параметрами (отдельный класс от Frame?)
            List<byte> requestConstructor = new List<byte>();
            requestConstructor.Add((byte)RequestTypes.OpenConnection);
            requestConstructor.Add((byte)MeterAccessLevel.User);
            requestConstructor.AddRange(StringToBytes("111111"));

            byte[] request = requestConstructor.ToArray();
            Frame requestFrame = new Frame(Address, request);
            Write(requestFrame);
            Frame responseFrame = Read();
            responseFrame.Print();
        }
        private void Write(Frame f)
        {
            byte[] buffer = f.ToArray();
            Port.Write(buffer, 0, buffer.Length);
        }
        private Frame Read()
        {
            Thread.Sleep(WaitAnswerTime);
            if (Port.BytesToRead == 0)
                throw new Exception("Входной буфер пуст.");
            byte[] buffer = new byte[Port.BytesToRead];
            Port.Read(buffer, 0, buffer.Length);
            Frame response = new Frame(buffer);
            return response;
        }
        private bool MatchCRC(Frame a, Frame b)
        {
            return a.CRC.Equals(b.CRC);
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
    }
}
