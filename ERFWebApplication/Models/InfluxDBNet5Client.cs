using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
//using System.Web;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HTTPClientConsoleApp
{
    public enum TimePrecision
    {
        Second = 0,
        Millisecond = 1,
        Microsecond = 2
    }
    public class InfluxDBNet5Client : IInfluxDBNet5Client
    {
        private const string Template = "{0}://{1}:{2}{3}?u={4}&p={5}";
        private readonly string database;
        private readonly string host;
        private readonly string password;
        private readonly int port;
        private readonly string schema;
        private readonly string[] timePrecision = { "s", "m", "u" };
        private readonly string user;

        public InfluxDBNet5Client(string host, int port, string user, string password, string database, string schema = "http")
        {
            this.host = host;
            this.port = port;
            this.user = user;
            this.password = password;
            this.database = database;
            this.schema = schema;
        }

        private string GetUrl(string path)
        {
            return string.Format(Template, this.schema, this.host, this.port, path, this.user, this.password);
        }

        public object Request(string url, string method, object data = null)
        {


            var result = GetResult(url);
            return result.Result;
        }

        public async Task<object> GetResult(string uri)
        {
            var httpClient = new HttpClient();
            //var content = await httpClient.GetStringAsync(uri);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Stream resStream = await httpClient.GetStreamAsync(uri);
            //Stream resStream = result.

            if (resStream != null)
            {
                var resStreamReader = new StreamReader(resStream);
                string resString = resStreamReader.ReadToEnd();

                return await Task.Run(() => JsonConvert.DeserializeObject<object>(resString));
            }
            return null;
            //return await Task.Run(() => JsonConvert.DeserializeObject<object>(content));           
        }

        public void CreateDatabase(string name)
        {
            string url = this.GetUrl("/db");
            var data = new { name };
            this.Request(url, "POST", data);
        }

        public void DeleteDatabase(string name)
        {
            string url = this.GetUrl("/db/" + name);
            var data = new { name };
            this.Request(url, "DELETE", data);
        }

        public void DeleteSerie(string name)
        {
            this.Query("DROP SERIES " + name);
        }

        public List<string> GetDatabaseList()
        {
            var result = new List<string>();
            string url = this.GetUrl("/db");
            var res = (IEnumerable<dynamic>)this.Request(url, "GET");

            if (res != null)
            {
                result.AddRange(res.Select(db => (string)db["name"]));
            }

            return result;
        }

        public List<InfluxNet5ServerInfo> GetServers()
        {
            var result = new List<InfluxNet5ServerInfo>();
            string url = this.GetUrl("/cluster/servers");
            var res = (IEnumerable<dynamic>)this.Request(url, "GET");

            if (res != null)
            {
                result.AddRange(res.Select(server => new InfluxNet5ServerInfo { Id = server.id.Value, ProtobufConnectString = server.protobufConnectString.Value }));
            }

            return result;
        }

        public void Insert(List<Serie> series)
        {
            string url = this.GetUrl("/db/" + this.database + "/series");

            //TODO options
            this.Request(url, "POST", series);
        }

        public List<Serie> Query(string query, TimePrecision? precision = null)
        {
            string escapedQuery = System.Uri.EscapeDataString(query);//HttpUtility.UrlEncode(query);
            string url = this.GetUrl("/db/" + this.database + "/series");
            if (precision.HasValue)
            {
                url += "&time_precision=" + this.timePrecision[(int)precision.Value];
            }
            url += "&q=" + escapedQuery;
            object result = this.Request(url, "GET");

            if (result != null)
            {
                return JsonConvert.DeserializeObject<List<Serie>>(result.ToString());
            }

            return null;
        }

        public bool Ping()
        {
            string url = this.GetUrl("/ping");
            dynamic response = this.Request(url, "GET");

            return response.status == "ok";
        }
    }
}