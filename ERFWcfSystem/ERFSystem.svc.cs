using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Diagnostics;
using GenericSimulationComponents;
using SIMUL8;
//using Excel = Microsoft.Office.Interop.Excel;
using Simul8ModelComponents;

namespace ERFWcfSystem
{
    // 注意: 您可以使用 [重構] 功能表上的 [重新命名] 命令同時變更程式碼、svc 和組態檔中的類別名稱 "Service1"。
    public class ERFSystem : IERFSystem
    {

        static string xmlwebBase = @"~/XML";
        static string xmlBase = System.Web.Hosting.HostingEnvironment.MapPath(xmlwebBase);

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        //ERFSIMUL MySim = null;
        //static Excel.Application xlApp = new Excel.Application();
        static Simul8Runner S8Machine = new Simul8Runner();

        public List<Forecast_data> Forcasting(string time)
        {
            //StringBuilder strReturnValue = new StringBuilder();
            // return username prefixed as shown below
            //strReturnValue.Append(string.Format("You have entered userName as {0}", time));
            //return strReturnValue.ToString();

            //StreamReader sr = new StreamReader(data);
            //string stringData = sr.ReadToEnd();

            //string path = @"c:\MyTest.txt";
            int replicationnum = 5;
            int timestamp = Convert.ToInt32(DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            
            //string MyID = OperationContext.Current.SessionId;
            string filename = @"~/XML/ERPModel_SS.XML";
            //string datafilename = @"~/XML/H_ER.xlsx";
            //string datafilename = @"~/XML/" + timestamp + ".xlsx";

            List<Forecast_data> RTNValue = null;
            //string RTNTag = "";


            RTNValue = new List<Forecast_data>();

            //Forecast_data current = null;
            int timestamp1 = Convert.ToInt32(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            string path1 = System.Web.Hosting.HostingEnvironment.MapPath(filename);
            //string path2 = System.Web.Hosting.HostingEnvironment.MapPath(datafilename);

            //RTNTag = "'" + MyID + "'";
            //Excel.Workbook wb = null;
            //bool S8LoadFlag = false;
            //string filename = path1 + "TableTest1.XML";
            ERFSIMUL MySim = null;
            try
            {
                //string ans = run_cmd("E:/Project/OpenSrcData/data/createModelData.py", "1");
                //ScriptEngine engine = Python.CreateEngine();
                //xlApp = new Excel.Application();
                //engine.ExecuteFile("E:/Project/OpenSrcData/data/createModelData.py");
                //run_cmd("E:/Project/OpenSrcData/data/createModelData.py", "1");
                
                run_cmd("E:/Project/OpenSrcData/data/createModelDataSS.py", "-o " + xmlBase);

                //StreamWriter sw = new StreamWriter(
                //    System.Web.Hosting.HostingEnvironment.MapPath(@"~/XML/Result2.csv"));

                //RTNValue = "' >>"+ans+">> '";
                //sw.WriteLine("name,value,mean,sd-,sd+");
                /////////////////////////////////////////////////////
                //wb = xlApp.Workbooks.Open(path2);
                S8Machine.getSIMU8().Open(path1);
                //S8LoadFlag = true;
                MySim = new ERFSIMUL(S8Machine);
                MySim.Load(path1);
                //MySim.SetInputVariable(value);
                //sw.WriteLine(inputText[1]);
                List<S8Spreadsheet> NewSS = LoadSpreadsheet(MySim.InputVariables);
                MySim.SetTimesAnsLength(replicationnum, Double.Parse(time));
                MySim.SetSSInputVariable(NewSS);
                MySim.Run();
                RTNValue = MySim.GetHospitals(Double.Parse(time));
                MySim.Reset();
                //Forecast_data tmp = new Forecast_data();
                //tmp.Hospital_ID = "OK";
                //RTNValue.Add(tmp);

            }
            catch (Exception ex)
            {
                Forecast_data Err = new Forecast_data();
                Err.Hospital_ID = ex.ToString();

                RTNValue.Add(Err);
                
            }
            finally
            {
                              
                //S8Machine.getSIMU8().Close();
                if(MySim!=null)
                    MySim.close();
               

            }

            //RTNValue = ;

            //return RTNValue;
            return RTNValue;

        }

        private List<S8Spreadsheet> LoadSpreadsheet(List<Variable> ssVariable)
        {
            List<S8Spreadsheet> NewSS = new List<S8Spreadsheet>();


            for (int i = 0; i < ssVariable.Count; i++)
            {
                S8Spreadsheet tt = (S8Spreadsheet)ssVariable[i].Value;

            }

            for (int i = 0; i < ssVariable.Count; i++)
            {
                string lineReaded = null;
                string ssFilename = xmlwebBase + "/" + ssVariable[i].Name + ".csv";
                string path1 = System.Web.Hosting.HostingEnvironment.MapPath(ssFilename);

                StreamReader ssReader = new StreamReader(path1);
                S8Spreadsheet tempSheet = new S8Spreadsheet();
                S8Spreadsheet.Cell tmpCell = null;

                tempSheet.ColCount = 5;
                tempSheet.RowCount = Convert.ToInt16(ssReader.ReadLine());

                //ResulttextBox.Text += "1=>"+tempSheet.Cells.Count+" \r\n";
                lineReaded = ssReader.ReadLine(); //hospital name
                tmpCell = createCell(1, 1, VariableType.Text);
                tmpCell.Value = String.Copy(lineReaded);
                tempSheet.Cells.Add(tmpCell);

                //  plist
                lineReaded = ssReader.ReadLine(); //ARRIVAL
                tmpCell = createCell(2, 1, VariableType.Text);
                tmpCell.Value = String.Copy(lineReaded);
                tempSheet.Cells.Add(tmpCell);

                lineReaded = ssReader.ReadLine(); //TBA, Number
                string[] parseText = lineReaded.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                tmpCell = createCell(3, 1, VariableType.Text);
                tmpCell.Value = parseText[0];
                tempSheet.Cells.Add(tmpCell);

                tmpCell = createCell(3, 2, VariableType.Text);
                tmpCell.Value = parseText[1];
                tempSheet.Cells.Add(tmpCell);

                int count = Convert.ToInt16(ssReader.ReadLine());
                for (int j = 0; j < count; j++)
                {
                    int rowIdx = 4 + j;
                    lineReaded = ssReader.ReadLine();
                    parseText = lineReaded.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    tmpCell = createCell(rowIdx, 1, VariableType.Number);
                    tmpCell.Value = parseText[0];
                    tempSheet.Cells.Add(tmpCell);

                    tmpCell = createCell(rowIdx, 2, VariableType.Number);
                    tmpCell.Value = parseText[1];
                    tempSheet.Cells.Add(tmpCell);
                }

                //  nlist
                lineReaded = ssReader.ReadLine(); //SERVICE
                tmpCell = createCell(2, 3, VariableType.Text);
                tmpCell.Value = String.Copy(lineReaded);
                tempSheet.Cells.Add(tmpCell);

                lineReaded = ssReader.ReadLine(); //TBA, Number, Time
                parseText = lineReaded.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                tmpCell = createCell(3, 3, VariableType.Text);
                tmpCell.Value = parseText[0];
                tempSheet.Cells.Add(tmpCell);

                tmpCell = createCell(3, 4, VariableType.Text);
                tmpCell.Value = parseText[1];
                tempSheet.Cells.Add(tmpCell);

                tmpCell = createCell(3, 5, VariableType.Text);
                tmpCell.Value = parseText[2];
                tempSheet.Cells.Add(tmpCell);

                count = Convert.ToInt16(ssReader.ReadLine()); //plist
                for (int j = 0; j < count; j++)
                {
                    int rowIdx = 4 + j;
                    lineReaded = ssReader.ReadLine();
                    parseText = lineReaded.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    tmpCell = createCell(rowIdx, 3, VariableType.Number);
                    tmpCell.Value = parseText[0];
                    tempSheet.Cells.Add(tmpCell);

                    tmpCell = createCell(rowIdx, 4, VariableType.Number);
                    tmpCell.Value = parseText[1];
                    tempSheet.Cells.Add(tmpCell);

                    tmpCell = createCell(rowIdx, 5, VariableType.Number);
                    tmpCell.Value = parseText[2];
                    tempSheet.Cells.Add(tmpCell);

                }

                NewSS.Add(tempSheet);

            }

            return NewSS;
        }

        private S8Spreadsheet.Cell createCell(int row, int col, VariableType type)
        {
            S8Spreadsheet.Cell tmpCell = new S8Spreadsheet.Cell();
            tmpCell.CellType = type;
            tmpCell.Row = row; tmpCell.Col = col;
            return tmpCell;

        }
             

        private string run_cmd(string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "C:/Python27/python.exe";
            start.Arguments = string.Format("{0} {1}", cmd, args);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    return result;
                    //Console.Write(result);
                }
            }
        }
    }
}
