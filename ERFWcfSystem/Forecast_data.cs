using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERFWcfSystem
{
    public class Forecast_data
    {
        public string Hospital_ID { get; set; }
        public int Basetime { get; set; }
        public double Forecast_Time { get; set; }
        public double[] HpF_BED { get; set; }
        public double[] HpF_WARN { get; set; }
        public double[] HpF_ICU { get; set; }
    }
}