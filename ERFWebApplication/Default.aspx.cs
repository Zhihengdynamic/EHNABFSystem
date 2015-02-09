using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using InfluxDB;

namespace ERFWebApplication
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            InfluxDBClient influxDB = new InfluxDBClient("grainflux.cloudapp.net", 8086, "ERFadmin", "eRfe19", "ERFdb");
            
            DataSet info=new DataSet();

            
            DataRow workRow = info.Locations.NewRow();

            workRow["ID"] = "H1";
            workRow["Name"] = "台北榮民總醫院";
            workRow["Latitude"] = 25.1201836;
            workRow["Longitude"] = 121.5201598;
            workRow["HTMLcontent"] = new String(getBedHTMLInfo(influxDB, "H1").ToCharArray());
            info.Locations.Rows.Add(workRow);

            DataRow workRow1 = info.Locations.NewRow();
            workRow1["ID"] = "H2";
            workRow1["Name"] = "林口長庚醫院";
            workRow1["Latitude"] = 25.0618495;
            workRow1["Longitude"] = 121.3676923;
            workRow1["HTMLcontent"] = new String(getBedHTMLInfo(influxDB, "H2").ToCharArray());
            info.Locations.Rows.Add(workRow1);

            DataRow workRow2 = info.Locations.NewRow();
            workRow2["ID"] = "H3";
            workRow2["Name"] = "台中榮民總醫院";
            workRow2["Latitude"] = 24.183744;
            workRow2["Longitude"] = 120.60371;
            workRow2["HTMLcontent"] = new String(getBedHTMLInfo(influxDB, "H3").ToCharArray());
            info.Locations.Rows.Add(workRow2);

            PutHospitalMark(info.Locations);
            
   
        }

        private String getBedHTMLInfo(InfluxDBClient influxDB, String hID)
        {

            /*    0==> time:1423139131720 
             1==> sequence_number:333150001 
             2==> HpF_BED_Upper:11 
               3==> HpF_WARN:11.8666667938232 
             4==> HpF_WARN_Lower:11.6717901229858 
             5==> HpF_WARN_Upper:12.0615425109863 
             6==> flownum:1423137920519 
               7==> Forecast_Time:20 
               8==> HpF_ICU:12 
             9==> HpF_ICU_Lower:12 
             10==> HpF_ICU_Upper:12 
               11==> HpF_BED:11 
             12==> HpF_BED_Lower:11
            */

            String HPInfoTableHead = @"'<table style=""width:100%"">'+
                                 '<tr>'+
                                    '<th>預測時間</th>'+
                                    '<th>普通病床</th>'+	
                                    '<th>加護病床</th>'+
                                    '<th>急診推床</th>'+
                                 '</tr>'";

            String HPInfoTableTail = "'</table>'";
            List<Serie> result = influxDB.Query("select * from "+hID+ " where time < now()+2h group by flownum order asc");

            Serie tmpSerie = result[0];

           

            String HPInfoTableContent="";

            //SimpleDateFormat sdf = new SimpleDateFormat("HH:mm:ss"); // the format of your date

            for (int i = 0; i < 3; i++)
            {
                 Dictionary<string, object> Map = new Dictionary<string, object>();
                for (int j = 0; j< tmpSerie.ColumnNames.Length; j++)
                {
                    Map.Add(tmpSerie.ColumnNames[j], tmpSerie.Points[i].GetValue(j));
                }
                String[] value = new String[3];
                String Ftime = Map["Forecast_Time"].ToString();//tmpSerie.Points[i].GetValue(7).ToString(); //Forecast_Time
                //tmpTextBox.Text += " "+i+"==> "+tmpSerie.ColumnNames[i]+":"+ Ftime;
                String Otime = Map["time"].ToString(); //mpSerie.Points[i].GetValue(0).ToString(); //time
                double Ot = Convert.ToDouble(Otime);
                double Ft = Convert.ToDouble(Ftime);
                double timestamp = Ot + Ft;
                DateTime dt = (new DateTime(1970, 1, 1, 0, 0, 0)).AddHours(8).AddMilliseconds(timestamp);

                String Raw = "'<tr>'+";

                Raw += "'<th>" + String.Format("{0:yyyy/M/dd HH:mm}", dt) + "</th>'+";
                value[0] = String.Format("{0:0.00}",Map["HpF_WARN"]);//.ToString(); //WARN
                Raw += "'<th>" + value[0] + "</th>'+";
                value[1] = String.Format("{0:0.00}", Map["HpF_ICU"]); //ICU
                Raw += "'<th>" + value[1] + "</th>'+";
                value[2] = String.Format("{0:0.00}", Map["HpF_BED"]); //BED
                Raw += "'<th>" + value[2] + "</th>'+";
                Raw += "'</tr>'";

                HPInfoTableContent += "+"+Raw;

            }

            return HPInfoTableHead + HPInfoTableContent + "+"+HPInfoTableTail;
            //return HPInfoTableHead + "+" + HPInfoTableTail;
        }



        private void PutHospitalMark(DataTable tbl)
        {
             

            js.Text = @"<script type='text/javascript'>
                 geocoder = new google.maps.Geocoder();
                 
                
                 function initialize() {
                    var mapOptions = {
                         center: new google.maps.LatLng(24.183744, 120.60371),
                         zoom: 8,
                         mapTypeId: google.maps.MapTypeId.ROADMAP
                     };
                    map = new google.maps.Map(document.getElementById('map_canvas'),mapOptions);
                    var rendererOptions = {
                        map: map,
                        suppressMarkers : true
                    };
                    directionsDisplay = new google.maps.DirectionsRenderer(rendererOptions);";
            int mID = 0;
            foreach (DataRow r in tbl.Rows)
            {
                // bypass empty rows	 	
                if (r["Latitude"].ToString().Trim().Length == 0)
                    continue;

                string Latitude = r["Latitude"].ToString();
                string Longitude = r["Longitude"].ToString();
              
                // create a line of JavaScript for marker on map for this record	
                //Locations += Environment.NewLine + 
                //    " map.addOverlay(new GMarker(new GLatLng(" + Latitude + "," + Longitude + ")));";
                mID++;
                string MId = "hospMarker[" + (mID-1)+"]";
                string infowindowId = "infowindow" + mID;
                String Locations = Environment.NewLine + MId+@"=new google.maps.Marker({
                         position: new google.maps.LatLng(" + Latitude + "," + Longitude + @"),
                         map: map,
                         title: '"+r["Name"] +"' });";
                Locations += @"var contentString = '<div id=""content"">'+
      '<div id=""siteNotice" + mID + @""">'+
      '</div>'+
      '<ul id=""menu"">'+
        '<li><img src=""img/ic_bed.png"" alt=""bed"" height=""42"" width=""42""></li>'+
        '<li><h1 id=""firstHeading"" class=""firstHeading"" >" + r["Name"] + @"<BR/></h1></li>'+
      '</ul>'+
      '<div id=""bodyContent"">'+" + r["HTMLcontent"] +@"+
      '</div>'+
      '</div>';";
                Locations += "var " + infowindowId + @"= new google.maps.InfoWindow({
                                    content: contentString
                             });";
                //Locations += "google.maps.event.addListener(" + MId + ", 'mouseover', function() {" +
                //                infowindowId + ".open(map," + MId + @");
                                
                //             });";
                Locations += "google.maps.event.addListener(" + MId + ", 'click', function() {" +
                                infowindowId + ".open(map," + MId + @");
                                showRoute(" + MId + @"); 
                             });";
                //Locations += "google.maps.event.addListener(" + MId + ", 'mouseout', function() {" +
                //                infowindowId + ".close(map," + MId + @");
                //             });";

                js.Text += Locations;
            }

            js.Text+= @"}
                    
                    google.maps.event.addDomListener(window, 'load', initialize);
                                        

                </script>";
           
        }       
        
    }
}
