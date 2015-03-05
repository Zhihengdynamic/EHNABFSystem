using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GenericSimulationComponents;
using Newtonsoft.Json;


using Simul8ModelComponents;
using SIMUL8;
namespace ERFWcfSystem
{
    public class ERFSIMUL
    {
        private ModelMetaClass modelToRun=null;
        public int ResultsCount;
        private List<Variable> UserVariables;
        public List<Variable> InputVariables { get; set; }
        public List<Variable> SSInputVariables { get; set; }
        public int ReplicationTimes = 5;
        public double RunLength = 3000;

        Simul8Runner S8Machine = null;
        

        public List<IS8Result> Results;

        public ERFSIMUL(Simul8Runner S8Machine)
        {
            this.S8Machine = S8Machine;
            //modelToRun = null;
            //Reset();

        }       

        public void Load(string fileName)
        {

            ////string fileName = "C:\\Dropbox\\TungHai\\YYtest2.xml";
            ////string fileName = "C:\\Dropbox\\TungHai\\test.s8";
            
            modelToRun = new ModelMetaClass
            {
                FileName = fileName,
                Runner = S8Machine, //new Simul8Runner(fileName),
                Reader = new Simul8XMLReader(fileName),
                Writer = new Simul8XMLWriter()
            };

            //Simul8Runner Tmp = (Simul8Runner)modelToRun.Runner;

            //ResultsCount = Tmp.getSIMU8().ResultsCount;

            ////modelToRun.OnSimulationComplete += this.SimulationComplete;

            ReplicationTimes = (modelToRun.Reader).ReadReplications();
            RunLength = (modelToRun.Reader).ReadRunLength();

            List<Variable> Vars = (modelToRun.Reader).ReadVariables();



            ////if (UserVariables.Count != 0)
            ////    UserVariables.Clear();
            UserVariables = new List<Variable> { };
            InputVariables = new List<Variable> { };

            foreach (Variable v in Vars)
            {
                if (v.Name.StartsWith("ssH"))
                    InputVariables.Add(v);
                else
                    UserVariables.Add(v);
            }




        }

        public void Exit()
        {
            if (modelToRun.Runner != null)
            {
                Simul8Runner S8Run = (Simul8Runner)(modelToRun.Runner);
                S8Run.CloseSimul8COMServer();
            }

        }

        public void SetInputVariable(string NewVariable)
        {
            List<Variable> InputVariables = this.InputVariables;
            int dimension = InputVariables.Count;

            //char[] delimiterChars = { ',' };
            string[] stringSeparators = new string[] { "%2C" };
            string[] words = NewVariable.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < dimension; i++)
            {
                double tmp = Convert.ToDouble(words[i]);
                InputVariables[i].Value = tmp;
                InputVariables[i].ResetValue = tmp;
                //Trace.Write(tmp + " ");
                //pHandler(tmp.ToString("F3") + " ");
            }

            this.InputVariables = InputVariables;
       }

        public void SetTimesAnsLength(int ReplicationTimes, double RunLength)
        {
            this.ReplicationTimes=ReplicationTimes;
            this.RunLength = RunLength;
        }
        public void SetSSInputVariable(List<S8Spreadsheet> NewSS)
        {
            List<Variable> InputVariables = this.InputVariables;
            //int dimension = InputVariables.Count;
            for (int i = 0; i < NewSS.Count(); i++)
            {
                InputVariables[i].Value = NewSS[i];
                InputVariables[i].ResetValue = NewSS[i];
                //InputVariables[i].
            }
            this.InputVariables = InputVariables;

        }
        public string GetResults()
        {
            String Resultstring = "";
            /*for (int i = 0; i < Results.Count; i++)
            {
                if (Resultstring.Length != 0)
                    Resultstring += "%2C";
                Resultstring += Results[i].Value.ToString() + "%2C"
                    + Results[i].Mean.ToString() + "%2C"
                    + Results[i].get_ConfLimit(-95) + "%2C"
                    + Results[i].get_ConfLimit(95);

            }*/

            foreach (IS8Result item in Results)
            {
                if (item.Name.StartsWith("gH"))
                {
                    if (Resultstring.Length != 0)
                        Resultstring += "%2C";

                    Resultstring += item.Name.Substring(1, 3) + "%2D"
                    + item.Value.ToString() + "%2D"
                    + item.Mean.ToString() + "("
                    + item.get_ConfLimit(-95) + "_"
                    + item.get_ConfLimit(95) + ")";

                }
            }

            return Resultstring;

        }

        public string GetJSON(double forecast_time)
        {
            List<Forecast_data> list = new List<Forecast_data>();

            Forecast_data current = null;
            int timestamp = Convert.ToInt32(DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            
            
            for (int i = 0; i < Results.Count; i+=3)
            {
                current = new Forecast_data();
                IS8Result itemB = Results[i];
                IS8Result itemW = Results[i+1];
                IS8Result itemI = Results[i+2];
                
                current.Hospital_ID = itemB.Name.Substring(1, 2);
                current.Forecast_Time = forecast_time;
                current.Basetime = timestamp;

                current.HpF_BED = new double[] { itemB.Mean, itemB.get_ConfLimit(-95), itemB.get_ConfLimit(95) };
                current.HpF_WARN = new double[] { itemW.Mean, itemW.get_ConfLimit(-95), itemW.get_ConfLimit(95) };
                current.HpF_ICU = new double[] { itemI.Mean, itemI.get_ConfLimit(-95), itemI.get_ConfLimit(95) };

                list.Add(current);
            }
            
            return JsonConvert.SerializeObject(list);

        }

        public List<Forecast_data> GetHospitals(double forecast_time)
        {
            List<Forecast_data> list = new List<Forecast_data>();

            Forecast_data current = null;
            int timestamp = Convert.ToInt32(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);


            for (int i = 0; i < Results.Count; i += 3)
            {
                current = new Forecast_data();
                IS8Result itemB = Results[i];
                IS8Result itemW = Results[i + 1];
                IS8Result itemI = Results[i + 2];

                int length = itemB.Name.IndexOf('B') - 1;
                current.Hospital_ID = itemB.Name.Substring(1, length);
                current.Forecast_Time = forecast_time;
                current.Basetime = timestamp;

                current.HpF_BED = new double[] { itemB.Mean, itemB.get_ConfLimit(-95), itemB.get_ConfLimit(95) };
                current.HpF_WARN = new double[] { itemW.Mean, itemW.get_ConfLimit(-95), itemW.get_ConfLimit(95) };
                current.HpF_ICU = new double[] { itemI.Mean, itemI.get_ConfLimit(-95), itemI.get_ConfLimit(95) };

                list.Add(current);
            }

            return list;

        }

        public void Run()
        {

            createRunModel();

            

            modelToRun.RunMultipleReplications(ReplicationTimes);
            

            Simul8Runner S8Run = (Simul8Runner)modelToRun.Runner;

            while (S8Run.isEnd() == false)
                System.Threading.Thread.Sleep(10);

            Results = S8Run.getResults();
            
            
            // S8Run.showResults(ResultView);


        }

        public void createRunModel()
        {

            if (!modelToRun.IsLoaded)
            {
                modelToRun.Load();
            }

            modelToRun.Variables.Clear();

            //Trace.Write("(");
            foreach (Variable v in InputVariables)
            {
                modelToRun.Variables.Add(v);
                //Trace.Write(" "+ v.ResetValue+" ");

            }
            //Trace.Write(")");

            foreach (Variable v in UserVariables)
            {
                modelToRun.Variables.Add(v);

            }

            modelToRun.Replications = ReplicationTimes;
            modelToRun.RunLength = RunLength;
            modelToRun.Write();

        }

        public void close()
        {
            modelToRun.Reader = null;
            modelToRun.Writer = null;
            S8Machine.getSIMU8().Close();
            //S8Machine.getSIMU8().Quit();
            modelToRun = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            

        }

        public void Reset()
        {
            S8Machine.getSIMU8().ResetSim(0);

        }

    }

   
}