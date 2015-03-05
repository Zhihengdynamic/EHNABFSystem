using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using InfluxDB;
using System.Threading;
using System.Timers;
using ERFWindowsService.ServiceReference1;

namespace ERFWindowsService
{
    public partial class Service1 : ServiceBase
    {
        private System.Timers.Timer timer1 = null;
        public Service1()
        {
            InitializeComponent();
            this.AutoLog = false;
            if(!System.Diagnostics.EventLog.SourceExists("ERFSource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "ERFSource", "ERFLog");
            }
            eventLog1.Source = "ERFSource";

        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("ERF Start.");
            RunForcasting();
            timer1 = new System.Timers.Timer();
            timer1.Elapsed += new ElapsedEventHandler(timer1_Elapsed);
            timer1.Interval = 420000;
            timer1.Start(); 
        }

        protected override void OnStop()
        {

            timer1.Stop();
            timer1 = null;
            eventLog1.WriteEntry("ERF Stop.");
        }

        private void timer1_Elapsed(object sender, EventArgs e)
        {
            //RunGetERInfo();
            eventLog1.WriteEntry("Forcast.");
            while (true)
            {
                try
                {
                    timer1.Stop();
                    RunForcasting();
                    timer1.Start();
                    break;

                }
                catch (Exception ex)
                {
                    //errortextBox.Text += "/r/n " + DateTime.Now.TimeOfDay + "/r/n" + ex.ToString();
                    eventLog1.WriteEntry("Exception:" + ex.ToString());
                    Thread.Sleep(10000);
                }
            }

        }

        private void RunForcasting()
        {
            //Initial Database
            List<Forecast_data> list = null;
            //InfluxDBClient client = new InfluxDBClient("210.240.226.146", 8086, "ERFadmin", "eRfe19", "ERFdbHistory");
            //eventLog1.WriteEntry("1");//grainflux.cloudapp.net
            InfluxDBClient clientshow = new InfluxDBClient("59.126.164.5", 8086, "ERFadmin", "eRfe19", "ERFdb");
            InfluxDBClient client = new InfluxDBClient("59.126.164.5", 8086, "ERFadmin", "eRfe19", "ERFdbHistory");
            //eventLog1.WriteEntry("2");
            // Create 
            var serie = new Serie
            {
                ColumnNames = new[] { "time", "flownum","Forecast_Time", "HpF_BED", "HpF_BED_Lower", "HpF_BED_Upper"
                                                     , "HpF_WARN", "HpF_WARN_Lower", "HpF_WARN_Upper"
                                                     , "HpF_ICU", "HpF_ICU_Lower", "HpF_ICU_Upper"}
            };
            var series = new List<Serie> { serie };
            var serieshow = new List<Serie>();
            //eventLog1.WriteEntry("3");
            ///Initial Web Service
            ERFServiceClient AS = new ERFServiceClient("BasicHttpBinding_IERFService1");
            AS.InnerChannel.OperationTimeout = new TimeSpan(0, 7, 0);
            //eventLog1.WriteEntry("4");

            string FlowNum = Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds).ToString();
            //eventLog1.WriteEntry("5");
            //20 miniutes
            //client = new InfluxDBClient("210.240.226.146", 8086, "ERFadmin", "eRfe19", "ERFdb");
            string[] results = Simul8Forcasting(AS, 15, 20.0);
            //eventLog1.WriteEntry("6");
            if (results != null)
            {
                list = (List<Forecast_data>)JsonConvert.DeserializeObject(results[2], typeof(List<Forecast_data>));

                foreach (Forecast_data data in list)
                {
                    serie.Name = String.Copy((string)data.Hospital_ID);
                    serie.Points.Add(new object[] { data.time, FlowNum, data.Forecast_Time, data.HpF_BED[0], data.HpF_BED[1],data.HpF_BED[2]
                                                                  , data.HpF_WARN[0],data.HpF_WARN[1],data.HpF_WARN[2] 
                                                                  , data.HpF_ICU[0],data.HpF_ICU[1],data.HpF_ICU[2]});
                    //client.Insert(series);
                    Thread.Sleep(200);
                    serie.Points.Clear();

                    var serieh = new Serie
                    {
                        ColumnNames = new[] { "time", "flownum","Forecast_Time", "HpF_BED", "HpF_BED_Lower", "HpF_BED_Upper"
                                                     , "HpF_WARN", "HpF_WARN_Lower", "HpF_WARN_Upper"
                                                     , "HpF_ICU", "HpF_ICU_Lower", "HpF_ICU_Upper"},
                        Name = String.Copy((string)data.Hospital_ID)
                    };
                    serieh.Points.Add(new object[] { data.time, FlowNum, data.Forecast_Time, data.HpF_BED[0], data.HpF_BED[1],data.HpF_BED[2]
                                                                  , data.HpF_WARN[0],data.HpF_WARN[1],data.HpF_WARN[2] 
                                                                  , data.HpF_ICU[0],data.HpF_ICU[1],data.HpF_ICU[2]});
                    serieshow.Add(serieh);

                }
            }

            //40 miniutes
            //client = new InfluxDBClient("210.240.226.146", 8086, "ERFadmin", "eRfe19", "ERFdb");
            results = Simul8Forcasting(AS, 15, 40.0);
            if (results != null)
            {
                list = (List<Forecast_data>)JsonConvert.DeserializeObject(results[2], typeof(List<Forecast_data>));
                foreach (Forecast_data data in list)
                {
                    serie.Name = String.Copy((string)data.Hospital_ID);
                    serie.Points.Add(new object[] { data.time, FlowNum, data.Forecast_Time, data.HpF_BED[0], data.HpF_BED[1],data.HpF_BED[2]
                                                                  , data.HpF_WARN[0],data.HpF_WARN[1],data.HpF_WARN[2] 
                                                                  , data.HpF_ICU[0],data.HpF_ICU[1],data.HpF_ICU[2]});
                    //client.Insert(series);
                    Thread.Sleep(200);
                    serie.Points.Clear();

                    var serieh = new Serie
                    {
                        ColumnNames = new[] { "time", "flownum","Forecast_Time", "HpF_BED", "HpF_BED_Lower", "HpF_BED_Upper"
                                                     , "HpF_WARN", "HpF_WARN_Lower", "HpF_WARN_Upper"
                                                     , "HpF_ICU", "HpF_ICU_Lower", "HpF_ICU_Upper"},
                        Name = String.Copy((string)data.Hospital_ID)
                    };
                    serieh.Points.Add(new object[] { data.time, FlowNum, data.Forecast_Time, data.HpF_BED[0], data.HpF_BED[1],data.HpF_BED[2]
                                                                  , data.HpF_WARN[0],data.HpF_WARN[1],data.HpF_WARN[2] 
                                                                  , data.HpF_ICU[0],data.HpF_ICU[1],data.HpF_ICU[2]});
                    serieshow.Add(serieh);
                }
            }

            //Thread.Sleep(1000);
            //60 miniutes
            //client = new InfluxDBClient("210.240.226.146", 8086, "ERFadmin", "eRfe19", "ERFdb");
            results = Simul8Forcasting(AS, 15, 60.0);
            if (results != null)
            {
                list = (List<Forecast_data>)JsonConvert.DeserializeObject(results[2], typeof(List<Forecast_data>));
                foreach (Forecast_data data in list)
                {
                    serie.Name = String.Copy((string)data.Hospital_ID);
                    serie.Points.Add(new object[] { data.time, FlowNum, data.Forecast_Time, data.HpF_BED[0], data.HpF_BED[1],data.HpF_BED[2]
                                                                  , data.HpF_WARN[0],data.HpF_WARN[1],data.HpF_WARN[2] 
                                                                  , data.HpF_ICU[0],data.HpF_ICU[1],data.HpF_ICU[2]});
                    //client.Insert(series);
                    Thread.Sleep(200);
                    serie.Points.Clear();

                    var serieh = new Serie
                    {
                        ColumnNames = new[] { "time", "flownum","Forecast_Time", "HpF_BED", "HpF_BED_Lower", "HpF_BED_Upper"
                                                     , "HpF_WARN", "HpF_WARN_Lower", "HpF_WARN_Upper"
                                                     , "HpF_ICU", "HpF_ICU_Lower", "HpF_ICU_Upper"},
                        Name = String.Copy((string)data.Hospital_ID)
                    };
                    serieh.Points.Add(new object[] { data.time, FlowNum, data.Forecast_Time, data.HpF_BED[0], data.HpF_BED[1],data.HpF_BED[2]
                                                                  , data.HpF_WARN[0],data.HpF_WARN[1],data.HpF_WARN[2] 
                                                                  , data.HpF_ICU[0],data.HpF_ICU[1],data.HpF_ICU[2]});
                    serieshow.Add(serieh);
                }
            }

            for(int i=1;i<=12;i++)
                clientshow.Query("DROP SERIES H"+i);
            
            clientshow.Insert(serieshow);
            client.Insert(serieshow);


            AS.Close();

        }

        private string[] Simul8Forcasting(ERFServiceClient AS, int rep, double Runlength)
        {

            string tag = "tag=A%2C" + rep.ToString() + "%2C" + Runlength.ToString();
            string[] results = null;

            //while (true)
            //{

                results = AS.getvalue(GenerateStreamFromString(tag));
                if (results[0] == "VALUE")
                {
                    //break;
                }
                else
                {
                    //errortextBox.Text += "/r/n" + DateTime.Now.TimeOfDay + "/r/n" + results[2];
                    eventLog1.WriteEntry("Error:" + results[2]);
                    Thread.Sleep(2000);
                }
            //}

            return results;

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
