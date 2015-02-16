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

namespace getERInfoService
{
    public partial class Service1 : ServiceBase
    {
        private System.Timers.Timer timer1=null;
        string[] scrapers = null;
        static TimeSpan waitTime = new TimeSpan(0, 1, 30);
        public Service1()
        {
            InitializeComponent();
            this.AutoLog = false;
            if (!System.Diagnostics.EventLog.SourceExists("ERInfo"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "ERInfo", "ERInfoLog");
            }
            eventLog1.Source = "ERInfo";

            scrapers = new string[] {   "tp_vgh.py",
                                        "cgmh3.php",
                                        "vghtc.php",
                                        "wanfangh.py",
                                        "ncku.php",
                                        "hltch.py",
                                        "ndmctsgh.py",
                                        "scmh.php",
                                        "cch.php",
                                        "vghks.php",
                                        "kmuh.php",
                                        "sltung.php",
                                            "cgmh6.php",
                                            "erkcgmh.py",
                                            "ntuh.php",
                                            "tsmh.py",
                                            "chimei.php",
                                            "pohai.py",
                                            "femh.py",
                                            "ercych.py",
                                            "skh.php",
                                            "edah.php",
                                            "shh.py",
                                            "csh.php",
                                            "cgmh8.php",
                                            "tsuchi.php",
                                            "cgh.py",
                                            "ktgh80.php",
                                            "ktgh81.php",
                                            "cmuh.php"
            };
            
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("ERInfo Start.");
            RunGetERInfo();
            timer1 = new System.Timers.Timer();
            timer1.Elapsed+=new ElapsedEventHandler(timer1_Elapsed);
            timer1.Interval = 1200000;
            timer1.Start();            
        }

        protected override void OnStop()
        {
            timer1.Stop();
            timer1 = null;
            eventLog1.WriteEntry("ERInfo Stop.");
        }

        private void timer1_Elapsed(object sender, EventArgs e)
        {
            RunGetERInfo();
            eventLog1.WriteEntry("Geted!!");

        }

        private void RunGetERInfo()
        {
            try
            {
                List<Thread> workerThreads = new List<Thread>();
                for (int i = 0; i < scrapers.Length; i++)
                {
                    int id = i + 1;
                    getInfoThread obj = new getInfoThread(scrapers[i], "H" + id);
                    Thread t = new Thread(obj.runMe);
                    t.IsBackground = true;
                    workerThreads.Add(t);
                    t.Start();
                    //obj.runMe();
                }

                foreach (Thread thread in workerThreads)
                {
                    thread.Join(waitTime);
                }
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry("ERROR: "+ ex.ToString() );
            }

        }

        
        
        /*private string run_cmd(string CMD, string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = CMD;
            start.Arguments = string.Format("{0} {1}", cmd, args);
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = Process.Start(start);
            process.WaitForExit();
            StreamReader reader = process.StandardOutput;
            string result = reader.ReadToEnd();
            process.Close();
            return result;

        }*/
        ////////////////////////////////////////////////////////////////////////
    }

    class ER_data
    {
        public bool full_reported { get; set; }      // (bool) 向119通報滿床, null 表示未提供
        public string hospital_sn { get; set; } // (string) 醫院代號 length = 10
        public int pending_doctor { get; set; }           // (integer) 等待看診人數
        public int pending_bed { get; set; }           // (integer) 等待推床人數
        public int pending_ward { get; set; }           // (integer) 等待住院人數
        public int pending_icu { get; set; }           // (integer) 等待加護病床人數
        public string update_time { get; set; }   // (timestamp) null 表示原始資料未提供
    }

    class getInfoThread
    {
        private String scrapername;
        private String seriename;
        
        //String CMD1 = "C:/Python27/python.exe";
        //String CMD2 = "C:/php/php.exe";
        private String CMD = null;
        public getInfoThread(String scrapername, String seriename)
        {
            this.scrapername = scrapername;
            this.seriename = seriename;

            String[] words = scrapername.Split('.');

            String ext = words[words.Length - 1];
            if(ext.Equals("py")){
                CMD = "C:/Python27/python.exe";
            }

            if (ext.Equals("php"))
            {
                CMD = "C:/php/php.exe";
            }
            
           
        }
        public void runMe()
        {
            var serie = new Serie
            {
                ColumnNames = new[] { "full_reported", 
                                      "hospital_sn",
                                      "pending_doctor", 
                                      "pending_bed", 
                                      "pending_ward", 
                                      "pending_icu",
                                      "update_time"}
            };
            var series = new List<Serie> { serie };

            string info = run_cmd(CMD, "C:/EHNABF/data/" + scrapername, "");

            ER_data ER = (ER_data)JsonConvert.DeserializeObject(info, typeof(ER_data));


            serie.Name = seriename;


            serie.Points.Add(new object[] { ER.full_reported, 
                                            ER.hospital_sn,
                                            ER.pending_doctor, 
                                            ER.pending_bed, 
                                            ER.pending_ward, 
                                            ER.pending_icu,
                                            ER.update_time});

            InfluxDBClient client = new InfluxDBClient("59.126.164.5", 8086, "ERman", "erM12", "twer");
            client.Insert(series);
         
            //Thread.Sleep(500);
            Thread.Sleep(500);

            client.Query("delete from " + seriename + " where time < now() - 35d");
            //serie.Points.Clear();
        }

        private string run_cmd(string CMD, string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = CMD;
            start.Arguments = string.Format("{0} {1}", cmd, args);
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = Process.Start(start);
            process.WaitForExit();
            StreamReader reader = process.StandardOutput;
            string result = reader.ReadToEnd();
            process.Close();
            return result;

        }

    }
}
