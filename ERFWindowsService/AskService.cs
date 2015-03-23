using System;
using System.Collections.Generic;
using System.IO;
using HTTPClientConsoleApp;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;


namespace ERFWinServiceNet5
{
    
    public class AskService 
    {
        string influxDBIP = "Your IP";
        string DBName = "Your Database";
        string DBUser = "Username";
        string Pass = "Password";
        public System.Threading.Timer timerRef { get; set; }
        public int counter { get; set; }
        private int dueTime, perior;
        public AskService(int dueTime, int perior)
        {
            this.dueTime = dueTime;
            this.perior = perior;
            counter = 0;
            //TimerDelegate = new System.Threading.TimerCallback(RunForcasting);
            //timer = new System.Threading.Timer(TimerDelegate, null, Timeout.Infinite, Timeout.Infinite);

        }

        public void stop()
        {
            timerRef.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void start()
        {
            timerRef.Change(dueTime, perior);
        }

        public void RunForcasting(object StateObj)
        {
            counter++;
            //Initial Database
            List<Forecast_data> list = null;
            InfluxDBNet5Client client = new InfluxDBNet5Client(influxDBIP, 8086, DBUser, Pass, DBName);
            // Create 
            var serie = new Serie
            {
                ColumnNames = new[] { "time", "flownum","Forecast_Time", "HpF_BED", "HpF_BED_Lower", "HpF_BED_Upper"
                                                     , "HpF_WARN", "HpF_WARN_Lower", "HpF_WARN_Upper"
                                                     , "HpF_ICU", "HpF_ICU_Lower", "HpF_ICU_Upper"}
            };
            var series = new List<Serie> { serie };
            var serieshow = new List<Serie>();
            
            string FlowNum = Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds).ToString();
            //20 miniutes
            Console.WriteLine(DateTime.UtcNow.ToLocalTime()+ ": 20");
            string results = (string)BedForcasting(20);
            if (results != null)
            {
                list = (List<Forecast_data>)JsonConvert.DeserializeObject(results, typeof(List<Forecast_data>));

                foreach (Forecast_data data in list)
                {
                    serie.Name = new String(data.Hospital_ID.ToCharArray());
                    serie.Points.Add(new object[] { data.Basetime, FlowNum, data.Forecast_Time, data.HpF_BED[0], data.HpF_BED[1],data.HpF_BED[2]
                                                                  , data.HpF_WARN[0],data.HpF_WARN[1],data.HpF_WARN[2]
                                                                  , data.HpF_ICU[0],data.HpF_ICU[1],data.HpF_ICU[2]});
                    Thread.Sleep(200);
                    serie.Points.Clear();

                    var serieh = new Serie
                    {
                        ColumnNames = new[] { "time", "flownum","Forecast_Time", "HpF_BED", "HpF_BED_Lower", "HpF_BED_Upper"
                                                     , "HpF_WARN", "HpF_WARN_Lower", "HpF_WARN_Upper"
                                                     , "HpF_ICU", "HpF_ICU_Lower", "HpF_ICU_Upper"},
                        Name = new String(data.Hospital_ID.ToCharArray())
                    };
                    serieh.Points.Add(new object[] { data.Basetime, FlowNum, data.Forecast_Time, data.HpF_BED[0], data.HpF_BED[1],data.HpF_BED[2]
                                                                  , data.HpF_WARN[0],data.HpF_WARN[1],data.HpF_WARN[2]
                                                                  , data.HpF_ICU[0],data.HpF_ICU[1],data.HpF_ICU[2]});
                    serieshow.Add(serieh);

                }
            }

            //40 miniutes
            Console.WriteLine(DateTime.UtcNow.ToLocalTime() + ": 40 ");
            results = (string)BedForcasting(40);
            if (results != null)
            {
                list = (List<Forecast_data>)JsonConvert.DeserializeObject(results, typeof(List<Forecast_data>));
                foreach (Forecast_data data in list)
                {
                    serie.Name = new String(data.Hospital_ID.ToCharArray());
                    serie.Points.Add(new object[] { data.Basetime, FlowNum, data.Forecast_Time, data.HpF_BED[0], data.HpF_BED[1],data.HpF_BED[2]
                                                                  , data.HpF_WARN[0],data.HpF_WARN[1],data.HpF_WARN[2]
                                                                  , data.HpF_ICU[0],data.HpF_ICU[1],data.HpF_ICU[2]});
                    Thread.Sleep(200);
                    serie.Points.Clear();

                    var serieh = new Serie
                    {
                        ColumnNames = new[] { "time", "flownum","Forecast_Time", "HpF_BED", "HpF_BED_Lower", "HpF_BED_Upper"
                                                     , "HpF_WARN", "HpF_WARN_Lower", "HpF_WARN_Upper"
                                                     , "HpF_ICU", "HpF_ICU_Lower", "HpF_ICU_Upper"},
                        Name = new String(data.Hospital_ID.ToCharArray())
                    };
                    serieh.Points.Add(new object[] { data.Basetime, FlowNum, data.Forecast_Time, data.HpF_BED[0], data.HpF_BED[1],data.HpF_BED[2]
                                                                  , data.HpF_WARN[0],data.HpF_WARN[1],data.HpF_WARN[2]
                                                                  , data.HpF_ICU[0],data.HpF_ICU[1],data.HpF_ICU[2]});
                    serieshow.Add(serieh);
                }
            }

            //60 miniutes
            Console.WriteLine(DateTime.UtcNow.ToLocalTime() + ": 60");
            results = (string)BedForcasting(60);
            if (results != null)
            {
                list = (List<Forecast_data>)JsonConvert.DeserializeObject(results, typeof(List<Forecast_data>));
                foreach (Forecast_data data in list)
                {
                    serie.Name = new String(data.Hospital_ID.ToCharArray());
                    serie.Points.Add(new object[] { data.Basetime, FlowNum, data.Forecast_Time, data.HpF_BED[0], data.HpF_BED[1],data.HpF_BED[2]
                                                                  , data.HpF_WARN[0],data.HpF_WARN[1],data.HpF_WARN[2]
                                                                  , data.HpF_ICU[0],data.HpF_ICU[1],data.HpF_ICU[2]});
                    Thread.Sleep(200);
                    serie.Points.Clear();

                    var serieh = new Serie
                    {
                        ColumnNames = new[] { "time", "flownum","Forecast_Time", "HpF_BED", "HpF_BED_Lower", "HpF_BED_Upper"
                                                     , "HpF_WARN", "HpF_WARN_Lower", "HpF_WARN_Upper"
                                                     , "HpF_ICU", "HpF_ICU_Lower", "HpF_ICU_Upper"},
                        Name = new String(data.Hospital_ID.ToCharArray())
                    };
                    serieh.Points.Add(new object[] { data.Basetime, FlowNum, data.Forecast_Time, data.HpF_BED[0], data.HpF_BED[1],data.HpF_BED[2]
                                                                  , data.HpF_WARN[0],data.HpF_WARN[1],data.HpF_WARN[2]
                                                                  , data.HpF_ICU[0],data.HpF_ICU[1],data.HpF_ICU[2]});
                    serieshow.Add(serieh);
                }
            }

            for (int i = 1; i <= 12; i++)
            {
                client.Query("DROP SERIES H" + i);
            }
            client.Insert(serieshow);

            Console.WriteLine("---------------------------------");

        }

        private object BedForcasting(int Runlength)
        {

            string url = "http://140.128.135.12/ERFSystem/ERFSystem.svc/Forcasting/time/" + Runlength;
            
            object result = this.Request(url, "GET");

            
            return result.ToString();

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

        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

    }
    
}