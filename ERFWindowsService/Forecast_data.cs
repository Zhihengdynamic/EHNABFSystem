using System;

namespace ERFWinServiceNet5
{
    public class Forecast_data
    {
        public string Hospital_ID { get; set; }
        public long Basetime { get; set; }
        public double Forecast_Time { get; set; }
        public double[] HpF_BED { get; set; }
        public double[] HpF_WARN { get; set; }
        public double[] HpF_ICU { get; set; }
    }
}