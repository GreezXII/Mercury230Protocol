using System;
using System.IO.Ports;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Linq;
using System.Text;

namespace Mercury230Protocol
{
    class Meter
    {
        // Тип подключения
        public ConnectionTypes ConnectionType;
        // Настройки для подключения по Com-порту
        SerialPort ComPort;
        public string PortName;
        public int BaudRate = 9600;
        public Parity PortParity = Parity.None;
        public int DataBits = 8;
        public StopBits PortStopBits = StopBits.One;
        public int WriteTimeout = 5000;
        // Настройки для подключения по TCP/IP
        public string IPAddress { get; set; }
        public int TCPPort { get; set; }
        public TcpClient TCPClient;
        // Данные для счётчика
        public byte Address { get; set; }
        public MeterAccessLevels AccessLevel { get; set; }
        public string Password { get; private set; }
        private int WaitAnswerTime = 150;

        public Meter() { }
        public Meter(byte addr, MeterAccessLevels al, string pwd, int wt)
        {
            if (addr <= 0 || addr > 240)
                throw new Exception($"Адрес счётчика {addr} является некорректным.");
            Address = addr;
            AccessLevel = al;
            Password = pwd;
            WaitAnswerTime = wt;
        }
        public Meter(byte addr, string comPort, MeterAccessLevels al, string pwd, int wt)
            : this(addr, al, pwd, wt)
        {
            ConnectionType = ConnectionTypes.Com;
            PortName = comPort;
        }
        public Meter(byte addr, string ip, int tcpPort, MeterAccessLevels al, string pwd, int wt)
            : this(addr, al, pwd, wt)
        {
            if (String.IsNullOrWhiteSpace(ip))
                throw new Exception("Неверный формат IP адреса");

            string[] splitValues = ip.Split('.');
            if (splitValues.Length != 4)
                throw new Exception("Неверное количество октетов в IP адресе");

            byte tempForParsing;
            if (!splitValues.All(r => byte.TryParse(r, out tempForParsing)))
                throw new Exception("Неправильный формат IP адреса");

            if (tcpPort < 0 || tcpPort > 65535)
                throw new Exception("Неверное значение TCP порта");

            ConnectionType = ConnectionTypes.TCP;
            IPAddress = ip;
            TCPPort = tcpPort;
            TCPClient = new TcpClient();
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
        public ReadStoredEnergyResponse ReadStoredEnergy(DataArrays da, Months m, Rates r) //TODO: Возврат значения
        {
            ReadStoredEnergyRequest request = new ReadStoredEnergyRequest(Address, da, m, r);
            byte[] buffer = RunCommand(request);
            ReadStoredEnergyResponse response = new ReadStoredEnergyResponse(buffer, r);
            return response;
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
        private byte[] RunCommand(Request req)
        {
            byte[] writeBuffer = req.Create();
            if (ConnectionType == ConnectionTypes.Com)
            {
                ComPort = new SerialPort(PortName, BaudRate, PortParity, DataBits, PortStopBits);

                ComPort.WriteTimeout = WriteTimeout;
                using (ComPort)
                {
                    ComPort.Open();
                    ComPort.Write(writeBuffer, 0, writeBuffer.Length);
                    Thread.Sleep(WaitAnswerTime);
                    byte[] readBuffer = new byte[ComPort.BytesToRead];
                    if (ComPort.BytesToRead == 0)
                        throw new Exception("Нет ответа от счётчика");
                    ComPort.Read(readBuffer, 0, readBuffer.Length);
                    return readBuffer;
                }
            }
            if (ConnectionType == ConnectionTypes.TCP)
            {
                TCPClient.Connect(IPAddress, TCPPort);
                NetworkStream ns = TCPClient.GetStream();
                ns.WriteTimeout = 10000;

                ns.Write(writeBuffer, 0, writeBuffer.Length);
                Thread.Sleep(WaitAnswerTime);
                byte[] readBuffer = new byte[req.ResponseLength];
                int bytesReaded = ns.Read(readBuffer, 0, req.ResponseLength);
                if (bytesReaded != req.ResponseLength)
                    throw new Exception("Получено неверное количество байт");
                TCPClient.Close();
                return readBuffer;
            }
            else
                throw new Exception("Вызван неправильный тип соединения");
        }
    }
}
