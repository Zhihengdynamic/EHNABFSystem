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
using System.Runtime.InteropServices;
using GenericSimulationComponents;
using SIMUL8;
using Excel = Microsoft.Office.Interop.Excel;
using Simul8ModelComponents;

namespace WcfService2
{
    // 注意: 您可以使用 [重構] 功能表上的 [重新命名] 命令同時變更程式碼、svc 和組態檔中的類別名稱 "Service1"。
    public class ERFService : IERFService
    {
        static Simul8Runner S8Machine = new Simul8Runner();
        static string xmlBase = @"~/XML/";
        //static Excel.Application xlApp = new Excel.Application();
        //xlApp.Visible = true;

        //Excel.Application xlApp = null;

       
        
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

        
        //Excel.Application xlApp = null;
        
        public string[] getvalue(Stream data)
        {

            //string[] resultStringArray = getBasedonExcel(data);
            string[] resultStringArray = getBasedonCSV(data);

            return resultStringArray;
        }

        string[] getBasedonExcel(Stream data)
        {

            /*
             * resultStringArray[0] = RTNstatus;
             * resultStringArray[1] = RTNTag;
             * resultStringArray[2] = RTNValue;
             */
            string[] resultStringArray = new string[3];
            
            ERFSIMUL MySim = null;
            //new Excel.Application();
            Excel.Application xlApp = null; //new Excel.Application();
            Excel.Workbook xlbook = null;
            bool S8LoadFlag = false;

            StreamReader sr = new StreamReader(data);
            string stringData = sr.ReadToEnd();

            //string path = @"c:\MyTest.txt";
            int timestamp = Convert.ToInt32(DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            string MyID = OperationContext.Current.SessionId;
            string filename = xmlBase+"ERPModel.XML";
            //string datafilename = @"~/XML/H_ER.xlsx";
            string datafilename = xmlBase + timestamp + ".xlsx";

            string value = stringData.Substring(4);

            string[] inputText = value.Split(new string[] { "%2C" }, StringSplitOptions.RemoveEmptyEntries);


            resultStringArray[1] = value;
            string path1 = System.Web.Hosting.HostingEnvironment.MapPath(filename);
            string path2 = System.Web.Hosting.HostingEnvironment.MapPath(datafilename);

            //RTNTag = "'" + MyID + "'";

            //string filename = path1 + "TableTest1.XML";
            try
            {

                if (inputText[0] == "A")
                {
                    string info = run_cmd("E:/Project/OpenSrcData/data/createModelData.py", "-o " + path2);

                    //StreamWriter sw = new StreamWriter(
                    //    System.Web.Hosting.HostingEnvironment.MapPath(@"~/XML/Result2.csv"));

                    //RTNValue = "' >>"+ans+">> '";
                    //sw.WriteLine("name,value,mean,sd-,sd+");
                    /////////////////////////////////////////////////////
                    //Excel.Workbook wb = 

                    if (File.Exists(path2) == true)
                    {
                        //xlApp = new Excel.Application();
                        //initailExcel();
                        xlApp = new Excel.Application();
                        xlApp.Visible = false;
                        //xlApp.DisplayAlerts = false;
                        //initailExcel();
                        xlbook = xlApp.Workbooks.Open(path2);
                        S8Machine.getSIMU8().Open(path1);
                        S8LoadFlag = true;
                        MySim = new ERFSIMUL(S8Machine);
                        MySim.Load(path1);
                        //MySim.SetInputVariable(value);
                        //sw.WriteLine(inputText[1]);
                        MySim.SetTimesAnsLength(int.Parse(inputText[1]), Double.Parse(inputText[2]));
                        MySim.Run();
                        //RTNValue = MySim.GetResults();
                        resultStringArray[2] = MySim.GetJSON(Double.Parse(inputText[2]));

                        resultStringArray[0] = "VALUE";
                        //xlApp.Quit();

                    }
                    else
                    {

                        resultStringArray[0] = "ERROR";
                        resultStringArray[2] = "' Open xlsx error: " + info + "'";

                    }
                    /////////////////////////////////////////////////////
                    //sw.Close();

                }


            }
            catch (Exception ex)
            {
                resultStringArray[0] = "ERROR";
                resultStringArray[2] = "'" + ex.ToString() + "'";
                //RTNValue = "'" + RTNValue + "'";
            }
            finally
            {
                if (xlbook != null)
                {
                    xlbook.Close(Type.Missing, Type.Missing, Type.Missing);
                    xlbook = null;
                }

                if (xlApp != null)
                {
                    xlApp.Quit();
                    xlApp = null;
                }

                if (File.Exists(path2) == true)
                {
                    File.Delete(path2);
                }

                if (S8LoadFlag != false)
                {
                    S8Machine.getSIMU8().Close();
                }

                GC.Collect();

            }
            return resultStringArray;

        }

        string[] getBasedonCSV(Stream data)
        {

            /*
             * resultStringArray[0] = RTNstatus;
             * resultStringArray[1] = RTNTag;
             * resultStringArray[2] = RTNValue;
             */
            string[] resultStringArray = new string[3];

            
            //new Excel.Application();
            bool S8LoadFlag = false;

            StreamReader sr = new StreamReader(data);
            string stringData = sr.ReadToEnd();

            //string path = @"c:\MyTest.txt";
            //int timestamp = Convert.ToInt32(DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            string MyID = OperationContext.Current.SessionId;
            string filename = xmlBase+"ERPModel_SS.XML";
            //string datafilename = @"~/XML/H_ER.xlsx";
            //string datafilename = @"~/XML/" + timestamp + ".xlsx";

            string value = stringData.Substring(4);

            string[] inputText = value.Split(new string[] { "%2C" }, StringSplitOptions.RemoveEmptyEntries);


            resultStringArray[1] = value;
            string path1 = System.Web.Hosting.HostingEnvironment.MapPath(filename);
           
            try
            {

                if (inputText[0] == "A")
                {
                    //S8Machine = new Simul8Runner();
                    ERFSIMUL MySim = null;
                    string info = run_cmd("E:/Project/OpenSrcData/data/createModelDataSS.py","");

                    S8Machine.getSIMU8().Open(path1);
                    S8LoadFlag = true;
                    MySim = new ERFSIMUL(S8Machine);
                    MySim.Load(path1);

                    List<S8Spreadsheet> NewSS = LoadSpreadsheet(MySim.InputVariables);
                    MySim.SetTimesAnsLength(int.Parse(inputText[1]), Double.Parse(inputText[2]));
                    MySim.SetSSInputVariable(NewSS);
                    MySim.Run();
                    //RTNValue = MySim.GetResults();
                    resultStringArray[2] = MySim.GetJSON(Double.Parse(inputText[2]));

                    resultStringArray[0] = "VALUE";
       
                }

            }
            catch (Exception ex)
            {
                resultStringArray[0] = "ERROR";
                resultStringArray[2] = "'" + ex.ToString() + "'";
                //RTNValue = "'" + RTNValue + "'";
            }
            finally
            {
                //if (S8LoadFlag != false)
                //{
                    S8Machine.getSIMU8().Close();
                    //S8Machine.getSIMU8().Quit();
                    
                    
                //}

                //if (S8Machine!=null)
                //    S8Machine.CloseSimul8COMServer();

                
                GC.Collect();

            }
            return resultStringArray;

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
                string ssFilename = xmlBase + ssVariable[i].Name + ".csv";
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

        public string[] storeavalue(Stream data)
        {

            string[] resultStringArray = new string[3];
            resultStringArray[0] = "STORED";
            resultStringArray[1] = "testValue";
            resultStringArray[2] = "Hello World!";

            return resultStringArray;
        }

        private string run_cmd(string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "C:/Python27/python.exe";
            start.Arguments = string.Format("{0} {1}", cmd, args);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            Process process = Process.Start(start);
            process.WaitForExit();
            StreamReader reader = process.StandardError;
            string result = reader.ReadToEnd();
            process.Close();
            return result;
            
        }
    }
}
