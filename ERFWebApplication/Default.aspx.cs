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
        struct HINFO
        {
            public string Name;
            public double Latitude;
            public double Longitude;
            public HINFO(string Name, double Latitude, double Longitude)
            {
                this.Name = Name;
                this.Latitude=Latitude;
                this.Longitude=Longitude;
            }
           
        };
        static HINFO[] hInfo =
            new HINFO[]{
                new HINFO( "台北榮民總醫院",25.1201836,121.5201598 ),                  
                new HINFO("林口長庚醫院",    25.0618495,  121.3676923),                
                new HINFO("台中榮民總醫院",    24.183744,   120.60371),                
                new HINFO("台北市立萬芳醫院",    24.9996897,  121.5575845),            
                new HINFO("國立成功大學醫學院附設醫院",    23.0021803,  120.2189867),  
                new HINFO("花蓮慈濟醫院",    23.9950092,  121.5923675),                
                new HINFO("三軍總醫院附設民眾診療服務處",    25.054373,   121.557672), 
                new HINFO("秀傳紀念醫院",    24.0647705,  120.5374429),                
                new HINFO("彰化基督教醫院",    24.0715309,  120.5446075),              
                new HINFO("高雄榮民總醫院",    22.6797317,  120.3223833),              
                new HINFO("高雄醫學大學附設中和紀念醫院",    22.6477347,  120.310416), 
                new HINFO("童綜合醫院",    24.2472297,  120.5428236)                   
            };
        protected void Page_Load(object sender, EventArgs e)
        {

            InfluxDBClient influxDB = new InfluxDBClient("59.126.164.5", 8086, "ERFadmin", "eRfe19", "ERFdb");
          
            DataSet info=new DataSet();

            for (int i = 0; i < hInfo.Length; i++)
            {
                string hID = "H" + (i + 1);
                DataRow workRow = info.Locations.NewRow();
                workRow["ID"] = hID;
                workRow["Name"] = hInfo[i].Name;
                workRow["Latitude"] = hInfo[i].Latitude;
                workRow["Longitude"] = hInfo[i].Longitude;
                workRow["HTMLcontent"] = new String(getBedHTMLInfo(influxDB, hID).ToCharArray());
                info.Locations.Rows.Add(workRow);

            }
           

            PutHospitalMark(info.Locations);
            
   
        }

        private String getBedHTMLInfo(InfluxDBClient influxDB, String hID)
        {

           
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
                string infowindowId = "infowindow[" + (mID - 1) + "]";
                //string infowindowId = "infowindow" + mID;
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
                Locations += Environment.NewLine + infowindowId + @"= new google.maps.InfoWindow({
                
                                    content: contentString
                             });";
                //Locations += "google.maps.event.addListener(" + MId + ", 'mouseover', function() {" +
                //                infowindowId + ".open(map," + MId + @");
                                
                //             });";
                Locations += "google.maps.event.addListener(" + MId + ", 'click', function() {" +
                                //infowindowId + ".open(map," + MId + @");
                                "showMarkerInfo(" + infowindowId + "," + MId + @");
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
