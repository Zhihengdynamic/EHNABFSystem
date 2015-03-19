using System;

namespace HTTPClientConsoleApp
{
    public class InfluxNet5ServerInfo
    {
        public long Id { get; set; }

        public string ProtobufConnectString { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0}, ProtobufConnectString: {1}", this.Id, this.ProtobufConnectString);
        }
    }
}