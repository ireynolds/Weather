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

namespace Weather
{

    [DataContract]
    public class CurrentConditionsWrapper
    {
        [DataMember]
        public CurrentObservation current_observation;
    }

    [DataContract]
    public class CurrentObservation
    {
        [DataMember]
        public DisplayLocation display_location;

        [DataMember]
        public string weather;

        [DataMember]
        public double temp_f;
        [DataMember]
        public double temp_c;
        public string temp
        {
            get
            {
                if ((Units)IsolatedStorageSettings.ApplicationSettings["Units"] == Units.SI)
                    return Math.Round(temp_c).ToString();
                else if ((Units)IsolatedStorageSettings.ApplicationSettings["Units"] == Units.English)
                    return Math.Round(temp_f).ToString();
                return string.Empty;
            }
        }

        [DataMember]
        public string relative_humidity;      // as in, "87%"

        [DataMember]
        public string wind_dir;

        [DataMember]
        public double wind_degrees;

        [DataMember]
        public double wind_mph;

        [DataMember]
        public double wind_gust_mph;

        [DataMember]
        public string pressure_in;

        [DataMember]
        public double dewpoint_f;
        [DataMember]
        public double dewpoint_c;
        public string dewpoint
        {
            get
            {
                if ((Units)IsolatedStorageSettings.ApplicationSettings["Units"] == Units.SI)
                    return dewpoint_c.ToString();
                else if ((Units)IsolatedStorageSettings.ApplicationSettings["Units"] == Units.English)
                    return dewpoint_f.ToString();
                return string.Empty;
            }
        }

        [DataMember]
        public string windchill_f;
        [DataMember]
        public string windchill_c;
        public string windchill
        {
            get
            {
                if ((Units)IsolatedStorageSettings.ApplicationSettings["Units"] == Units.SI)
                    return windchill_c.ToString();
                else if ((Units)IsolatedStorageSettings.ApplicationSettings["Units"] == Units.English)
                    return windchill_f.ToString();
                return string.Empty;
            }
        }

        [DataMember]
        public string precip_today_in;

        [DataMember]
        public string local_epoch;
    }

    [DataContract]
    public class DisplayLocation
    {

        [DataMember]
        public string city;

        [DataMember]
        public string state;

        [DataMember]
        public string state_name;

        [DataMember]
        public string zip;

        [DataMember]
        public string latitude;

        [DataMember]
        public string longitude;
    }


    public class CurrentWeatherViewModel : IComparable<CurrentWeatherViewModel>
    {

        // Define city name, e.g., "Seattle"            
        public string City { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
        
        // Define ZIP code, e.g., "98105"
        public string Zip { get; set; }

        // Define condition, e.g., "Overcast"
        public string Condition { get; set; }

        // Define temperature
        private double _tempF { get; set; }
        private double _tempC { get; set; }
        public string Temperature
        {
            get
            {
                if ((Units)IsolatedStorageSettings.ApplicationSettings["Units"] == Units.SI)
                    return _tempC.ToString() + (char)176;
                else if ((Units)IsolatedStorageSettings.ApplicationSettings["Units"] == Units.English)
                    return _tempF.ToString() + (char)176;
                return string.Empty;
            }
        }

        public string Weekday { get; set; }

        public CurrentWeatherViewModel(CurrentConditionsWrapper weatherData)
        {
            if (weatherData == null)
                throw new ArgumentNullException();

            City = weatherData.current_observation.display_location.city.ToLower();
            Latitude = weatherData.current_observation.display_location.latitude.ToLower();
            Longitude = weatherData.current_observation.display_location.longitude.ToLower();
            Zip = weatherData.current_observation.display_location.zip.ToLower();
            Condition = weatherData.current_observation.weather.ToLower();
            //Weekday = DateTime.Now.DayOfWeek.ToString().ToLower();
            Weekday = "today";

            _tempF = weatherData.current_observation.temp_f;
            _tempC = weatherData.current_observation.temp_c;
        }

        public int CompareTo(CurrentWeatherViewModel other)
        {
            return this.City.CompareTo(other.City);
        }

    }
}
