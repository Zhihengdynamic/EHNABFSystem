using System.Collections.Generic;

namespace HTTPClientConsoleApp
{
    public interface IInfluxDBNet5Client
    {
        object Request(string url, string method, object data = null);

        void CreateDatabase(string name);

        void DeleteDatabase(string name);

        void DeleteSerie(string name);

        List<string> GetDatabaseList();

        List<InfluxNet5ServerInfo> GetServers();

        void Insert(List<Serie> series);

        List<Serie> Query(string query, TimePrecision? precision = null);

        bool Ping();
    }
}