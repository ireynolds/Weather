using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.IO.IsolatedStorage;
using System.Collections.Generic;

namespace Weather
{
    [DataContract]
    public class ForecastWrapper
    {
        [DataMember]
        public Forecast forecast;
    }

    [DataContract]
    public class Forecast
    {
        [DataMember]
        public TxtForecast txt_forecast;

        [DataMember]
        public SimpleForecast simpleforecast;
    }

    [DataContract]
    public class TxtForecast
    {
        [DataMember]
        public List<TxtForecastDay> forecastday;
    }

    [DataContract]
    public class TxtForecastDay
    {
        [DataMember]
        public string fcttext;

        [DataMember]
        public int period;

        [DataMember]
        public string title;
    }

    [DataContract]
    public class SimpleForecast
    {
        [DataMember]
        public List<ForecastDay> forecastday;
    }

    [DataContract]
    public class ForecastDay
    {
        [DataMember]
        public Date date;

        [DataMember]
        public High high;

        [DataMember]
        public Low low;

        [DataMember]
        public string conditions;

        [DataMember]
        public Wind avewind;
    }

    [DataContract]
    public class Date
    {
        [DataMember]
        public string weekday;

        [DataMember]
        public int day;
    }

    [DataContract]
    public class High
    {
        [DataMember]
        public string fahrenheit;

        [DataMember]
        public string celsius;
    }

    [DataContract]
    public class Low
    {
        [DataMember]
        public string fahrenheit;

        [DataMember]
        public string celsius;
    }

    [DataContract]
    public class Wind
    {
        [DataMember]
        public double mph;

        [DataMember]
        public double kph;
    }

    public class ForecastViewModel
    {
        public string High_F;
        public string High_C;
        public string High
        {
            get
            {
                if ((Units)IsolatedStorageSettings.ApplicationSettings["Units"] == Units.SI)
                    return High_C + (char)176;
                else if ((Units)IsolatedStorageSettings.ApplicationSettings["Units"] == Units.English)
                    return High_F + (char)176;
                return string.Empty;
            }
        }

        public string Low_F;
        public string Low_C;
        public string Low
        {
            get
            {
                if ((Units)IsolatedStorageSettings.ApplicationSettings["Units"] == Units.SI)
                    return Low_C + (char)176;
                else if ((Units)IsolatedStorageSettings.ApplicationSettings["Units"] == Units.English)
                    return Low_F + (char)176;
                return string.Empty;
            }
        }

        private string _weekday;
        public string Weekday 
        {
            get
            {
                if (_weekday.Equals(DateTime.Now.AddDays(1).DayOfWeek.ToString().ToLower()))
                    return "tomorrow";

                return (_weekday + " the " + DayNumber + GetSuffix(this.DayNumber)).ToLower();
            }
            set
            {
                _weekday = value;
            }
        }

        public string Condition { get; set; }

        public string DayPrediction { get; set; }

        public string NightPrediction { get; set; }

        private int DayNumber;

        public ForecastViewModel(ForecastDay day)
        {
            if (day == null)
                throw new ArgumentNullException();

            this.High_F = day.high.fahrenheit;
            this.High_C = day.high.celsius;
            this.Low_F = day.low.fahrenheit;
            this.Low_C = day.low.celsius;
            this.Weekday = day.date.weekday.ToLower();
            this.DayNumber = day.date.day;
            this.Condition = day.conditions.ToLower();
        }

        private string GetSuffix(int day)
        {
            string s = string.Empty;
            switch (day % 10)
            {
                case 1: s = "st"; break;
                case 2: s = "nd"; break;
                case 3: s = "rd"; break;
                default: s = "th"; break;
            }

            if (day == 11 || day == 12 || day == 13)
                s = "th";

            return s;
        }
    }
}
