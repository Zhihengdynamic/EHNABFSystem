using System;
using System.Collections.Generic;
using System.Data;
using HTTPClientConsoleApp;

namespace WebApplicationNet5.Models
{
    public class Hospitals
    {
        struct HINFO
        {
            public string Name;
            public double Latitude;
            public double Longitude;
            public HINFO(string Name, double Latitude, double Longitude)
            {
                this.Name = Name;
                this.Latitude = Latitude;
                this.Longitude = Longitude;
            }

        };

        struct DataRow
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string HTMLcontent { get; set; }

        }

        static HINFO[] hInfo =
            new HINFO[]{
                new HINFO( "台北榮民總醫院",25.1201836d,121.5201598d),
                new HINFO("林口長庚醫院",    25.0618495d,  121.3676923d),
                new HINFO("台中榮民總醫院",    24.183744d,   120.60371d),
                new HINFO("台北市立萬芳醫院",    24.9996897d,  121.5575845d),
                new HINFO("國立成功大學醫學院附設醫院",    23.0021803d,  120.2189867d),
                new HINFO("花蓮慈濟醫院",    23.9950092d,  121.59236750d),
                new HINFO("三軍總醫院附設民眾診療服務處",    25.054373d,   121.557672d),
                new HINFO("秀傳紀念醫院",    24.0647705d,  120.5374429d),
                new HINFO("彰化基督教醫院",    24.0715309d,  120.5446075d),
                new HINFO("高雄榮民總醫院",    22.6797317d,  120.3223833d),
                new HINFO("高雄醫學大學附設中和紀念醫院",    22.6477347d,  120.310416d),
                new HINFO("童綜合醫院",    24.2472297d,  120.5428236d)
            };
        public string Load()
        {

            InfluxDBNet5Client influxDB = new InfluxDBNet5Client("59.126.164.5", 8086, "ERFadmin", "eRfe19", "ERFdb");

            List<DataRow> info = new List<DataRow>();

            for (int i = 0; i < hInfo.Length; i++)
            {
                string hID = "H" + (i + 1);
                DataRow workRow = new DataRow();
                workRow.ID = new string(hID.ToCharArray());
                workRow.Name = new string(hInfo[i].Name.ToCharArray());
                workRow.Latitude = hInfo[i].Latitude;
                workRow.Longitude = hInfo[i].Longitude;
                workRow.HTMLcontent = new string(getBedHTMLInfo(influxDB, hID).ToCharArray());
                info.Add(workRow);

            }


            string js=PutHospitalMark(info);
            return js;


        }

        private String getBedHTMLInfo(InfluxDBNet5Client influxDB, String hID)
        {


            String HPInfoTableHead = @"'<table style=""width:100%"">'+
                                 '<tr>'+
                                    '<th>預測時間</th>'+
                                    '<th>普通病床</th>'+	
                                    '<th>加護病床</th>'+
                                    '<th>急診推床</th>'+
                                 '</tr>'";

            String HPInfoTableTail = "'</table>'";
            List<Serie> result = influxDB.Query("select * from " + hID + " where time < now()+2h group by flownum order asc");

            Serie tmpSerie = result[0];



            String HPInfoTableContent = "";

            //SimpleDateFormat sdf = new SimpleDateFormat("HH:mm:ss"); // the format of your date

            for (int i = 0; i < 3; i++)
            {
                Dictionary<string, object> Map = new Dictionary<string, object>();
                for (int j = 0; j < tmpSerie.ColumnNames.Length; j++)
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
                value[0] = String.Format("{0:0}", Map["HpF_WARN"]);//.ToString(); //WARN
                Raw += "'<th>" + value[0] + "</th>'+";
                value[1] = String.Format("{0:0}", Map["HpF_ICU"]); //ICU
                Raw += "'<th>" + value[1] + "</th>'+";
                value[2] = String.Format("{0:0}", Map["HpF_BED"]); //BED
                Raw += "'<th>" + value[2] + "</th>'+";
                Raw += "'</tr>'";

                HPInfoTableContent += "+" + Raw;

            }

            return HPInfoTableHead + HPInfoTableContent + "+" + HPInfoTableTail;
            //return HPInfoTableHead + "+" + HPInfoTableTail;
        }



        private string PutHospitalMark(List<DataRow> tbl)
        {


            string js = @"<script type='text/javascript'>
                 var geocoder = null;
                 var map = null;
                 var directionsService = null;
                 var directionsDisplay;

                 var hospMarker = [];
                 var infowindow = [];
                 
                
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
                    directionsDisplay = new google.maps.DirectionsRenderer(rendererOptions);
                    geocoder = new google.maps.Geocoder(); 
                    directionsService = new google.maps.DirectionsService(); 
                    getLocation();";
            int mID = 0;
            foreach (DataRow r in tbl)
            {
                // bypass empty rows	 	
                if (r.Latitude.ToString().Trim().Length == 0)
                    continue;

                string Latitude = r.Latitude.ToString();
                string Longitude = r.Longitude.ToString();

                // create a line of JavaScript for marker on map for this record	
                //Locations += Environment.NewLine + 
                //    " map.addOverlay(new GMarker(new GLatLng(" + Latitude + "," + Longitude + ")));";
                mID++;
                string MId = "hospMarker[" + (mID - 1) + "]";
                string infowindowId = "infowindow[" + (mID - 1) + "]";
                //string infowindowId = "infowindow" + mID;
                String Locations = Environment.NewLine + MId + @"=new google.maps.Marker({
                         position: new google.maps.LatLng(" + Latitude + "," + Longitude + @"),
                         map: map,
                         title: '" + r.Name + "' });";
                Locations += @"var contentString = '<div id=""content"">'+
      '<div id=""siteNotice" + mID + @""">'+
      '</div>'+
      '<ul id=""menu"">'+
        '<li><img src=""/images/ic_bed.png"" alt=""bed"" height=""42"" width=""42""></li>'+
        '<li><h1 id=""firstHeading"" class=""firstHeading"" >" + r.Name + @"<BR/></h1></li>'+
      '</ul>'+
      '<div id=""bodyContent"" style=""color:#000;"">'+" + r.HTMLcontent + @"+
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

                js += Locations;
            }

            js += @"}
                    
                    google.maps.event.addDomListener(window, 'load', initialize);
                                        

                </script>";

            return js;

        }
    }
}