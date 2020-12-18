using System;

namespace NetworkSpeedTest
{
    public class SpeedTestResult
    {
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }
        public Ping Ping { get; set; }
        public Download Download { get; set; }
        public Upload Upload { get; set; }
        public double PacketLoss { get; set; }
        public string Isp { get; set; }
        public Interface Interface { get; set; }
        public Server Server { get; set; }
        public Result Result { get; set; }
    }

    public class Ping
    {
        public double Jitter { get; set; }
        public double Latency { get; set; }
    }

    public class Download
    {
        public int Bandwidth { get; set; }
        public int Bytes { get; set; }
        public int Elapsed { get; set; }
    }

    public class Upload
    {
        public int Bandwidth { get; set; }
        public int Bytes { get; set; }
        public int Elapsed { get; set; }
    }

    public class Interface
    {
        public string InternalIp { get; set; }
        public string Name { get; set; }
        public string MacAddr { get; set; }
        public bool IsVpn { get; set; }
        public string ExternalIp { get; set; }
    }

    public class Server
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Country { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Ip { get; set; }
    }

    public class Result
    {
        public string Id { get; set; }
        public string Url { get; set; }
    }
}