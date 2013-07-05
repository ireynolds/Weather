using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.IO;
using Weather.ViewModels;

namespace Weather
{
    public enum Units
    {
        SI,
        English
    };

    public enum CallType
    {
        Current,
        SevenDay,
        AddCity
    };

    public partial class App : Application
    {
        protected const string WUNDERGROUND_APPID = "d359263cceaa5dd7";
        protected const string SEATTLE_WOEID = "2490383";
        public List<City> myCities;
        public bool WasTombstoned { get; set; }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            //using (IsolatedStorageFile myIso = IsolatedStorageFile.GetUserStoreForApplication())
            //{
            //    using (StreamWriter myWriter = new StreamWriter(new IsolatedStorageFileStream("Seattle.txt", System.IO.FileMode.Create, myIso)))
            //    {
            //        myWriter.Write("{   \"response\": {    \"version\": \"0.1\"    ,\"termsofService\": \"http://www.wunderground.com/weather/api/d/terms.html\"    ,\"features\": {     \"conditions\": 1    }   },    \"current_observation\": {    \"image\": {     \"url\":\"http://icons-ak.wxug.com/graphics/wu2/logo_130x80.png\",     \"title\":\"Weather Underground\",     \"link\":\"http://www.wunderground.com\"    },    \"display_location\": {     \"full\":\"Seattle, WA\",     \"city\":\"Seattle\",     \"state\":\"WA\",     \"state_name\":\"Washington\",     \"country\":\"US\",     \"country_iso3166\":\"US\",     \"zip\":\"98101\",     \"latitude\":\"47.61167908\",     \"longitude\":\"-122.33325958\",     \"elevation\":\"63.00000000\"    },    \"observation_location\": {     \"full\":\"Herrera, Inc., Seattle, Washington\",     \"city\":\"Herrera, Inc., Seattle\",     \"state\":\"Washington\",     \"country\":\"US\",     \"country_iso3166\":\"US\",     \"latitude\":\"47.616558\",     \"longitude\":\"-122.341240\",     \"elevation\":\"270 ft\"    },    \"estimated\": {    },    \"station_id\":\"KWASEATT187\",    \"observation_time\":\"Last Updated on January 9, 2:27 PM PST\",    \"observation_time_rfc822\":\"Mon, 09 Jan 2012 14:27:53 -0800\",    \"observation_epoch\":\"1326148073\",    \"local_time_rfc822\":\"Mon, 09 Jan 2012 14:27:57 -0800\",    \"local_epoch\":\"1326148077\",    \"local_tz_short\":\"PST\",    \"local_tz_long\":\"America/Los_Angeles\",    \"weather\":\"Overcast\",    \"temperature_string\":\"47.8 F (8.8 C)\",    \"temp_f\":47.8,    \"temp_c\":8.8,    \"relative_humidity\":\"87%\",    \"wind_string\":\"From the WSW at 1.0 MPH Gusting to 4.0 MPH\",    \"wind_dir\":\"WSW\",    \"wind_degrees\":237,    \"wind_mph\":1.0,    \"wind_gust_mph\":\"4.0\",    \"pressure_mb\":\"1024.3\",    \"pressure_in\":\"30.25\",    \"pressure_trend\":\"+\",    \"dewpoint_string\":\"44 F (7 C)\",    \"dewpoint_f\":44,    \"dewpoint_c\":7,    \"heat_index_string\":\"NA\",    \"heat_index_f\":\"NA\",    \"heat_index_c\":\"NA\",    \"windchill_string\":\"48 F (9 C)\",    \"windchill_f\":\"48\",    \"windchill_c\":\"9\",    \"visibility_mi\":\"10.0\",    \"visibility_km\":\"16.1\",    \"solarradiation\":\"0\",    \"UV\":\"0\",    \"precip_1hr_string\":\"0.00 in ( 0 mm)\",    \"precip_1hr_in\":\"0.00\",    \"precip_1hr_metric\":\" 0\",    \"precip_today_string\":\"0.00 in (0 mm)\",    \"precip_today_in\":\"0.00\",    \"precip_today_metric\":\"0\",    \"icon\":\"cloudy\",    \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/cloudy.gif\",    \"forecast_url\":\"http://www.wunderground.com/US/WA/Seattle.html\",    \"history_url\":\"http://www.wunderground.com/history/airport/KWASEATT187/2012/1/9/DailyHistory.html\",    \"ob_url\":\"http://www.wunderground.com/cgi-bin/findweather/getForecast?query=47.616558,-122.341240\"   }  }  ");
            //    }
            //    using (StreamWriter myWriter = new StreamWriter(new IsolatedStorageFileStream("Forecast.txt", FileMode.Create, myIso)))
            //    {
            //        myWriter.Write("{ \"response\": {  \"version\": \"0.1\"  ,\"termsofService\": \"http://www.wunderground.com/weather/api/d/terms.html\"  ,\"features\": {   \"forecast7day\": 1  } }  , \"forecast\":{  \"txt_forecast\": {   \"date\":\"3:30 PM PST\",   \"forecastday\": [       {     \"period\":1,     \"icon\":\"nt_cloudy\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/nt_cloudy.gif\",     \"title\":\"Tonight\",     \"fcttext\":\"Mostly cloudy with a chance of rain showers in the evening...then partly cloudy after midnight. Lows in the 30s to lower 40s. North wind 10 to 20 mph becoming northeast after midnight.\"    },        {     \"period\":2,     \"icon\":\"partlycloudy\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/partlycloudy.gif\",     \"title\":\"Tuesday\",     \"fcttext\":\"Mostly sunny except patchy fog in the morning. Highs near 40. Northeast wind 10 to 15 mph becoming north in the afternoon. \"    },        {     \"period\":3,     \"icon\":\"nt_sunny\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/nt_sunny.gif\",     \"title\":\"Tuesday Night\",     \"fcttext\":\"Clear. Patchy fog and freezing fog after midnight. Lows in the upper 20s to mid 30s. Northeast wind 10 to 15 mph. \"    },        {     \"period\":4,     \"icon\":\"sunny\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/sunny.gif\",     \"title\":\"Wednesday\",     \"fcttext\":\"Sunny except areas of freezing fog and fog in the morning. Highs in the mid 30s to lower 40s. North wind 10 to 15 mph. \"    },        {     \"period\":5,     \"icon\":\"nt_partlycloudy\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/nt_partlycloudy.gif\",     \"title\":\"Wednesday Night\",     \"fcttext\":\"Partly cloudy. Areas of freezing fog after midnight. Lows in the mid 20s to lower 30s. North wind 10 to 15 mph.\"    },        {     \"period\":6,     \"icon\":\"partlysunny\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/partlysunny.gif\",     \"title\":\"Thursday\",     \"fcttext\":\"Partly sunny except areas of freezing fog in the morning. Highs in the lower 40s.\"    },        {     \"period\":7,     \"icon\":\"nt_cloudy\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/nt_cloudy.gif\",     \"title\":\"Thursday Night\",     \"fcttext\":\"Mostly cloudy. Areas of freezing fog and fog after midnight. Lows in the mid 20s to lower 30s. \"    }    ,    {     \"period\":8,     \"icon\":\"partlysunny\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/partlysunny.gif\",     \"title\":\"Friday\",     \"fcttext\":\"Partly sunny. Highs in the lower 40s. \"    }    ,    {     \"period\":9,     \"icon\":\"nt_cloudy\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/nt_cloudy.gif\",     \"title\":\"Friday Night\",     \"fcttext\":\"Mostly cloudy. Lows in the upper 20s to mid 30s. \"    }    ,    {     \"period\":10,     \"icon\":\"cloudy\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/cloudy.gif\",     \"title\":\"Saturday\",     \"fcttext\":\"Mostly cloudy with a chance of rain. Highs in the lower 40s.\"    }    ,    {     \"period\":11,     \"icon\":\"nt_cloudy\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/nt_cloudy.gif\",     \"title\":\"Saturday Night\",     \"fcttext\":\"Cloudy with a chance of rain showers. Lows in the lower to mid 30s.\"    }    ,    {     \"period\":12,     \"icon\":\"cloudy\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/cloudy.gif\",     \"title\":\"Sunday\",     \"fcttext\":\"Cloudy with a chance of rain showers. Highs in the lower 40s.\"    }    ,    {     \"period\":13,     \"icon\":\"nt_cloudy\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/nt_cloudy.gif\",     \"title\":\"Sunday Night\",     \"fcttext\":\"Cloudy with a chance of rain showers. Lows in the mid to upper 30s.\"    }    ,    {     \"period\":14,     \"icon\":\"cloudy\",     \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/cloudy.gif\",     \"title\":\"Martin Luther King Jr Day\",     \"fcttext\":\"Cloudy with a chance of rain showers. Highs near 40. \"    }   ]  },    \"simpleforecast\": {   \"forecastday\": [      {    \"date\":{     \"epoch\":\"1326175254\",     \"pretty\":\"10:00 PM PST on January 09, 2012\",     \"day\":9,     \"month\":1,     \"year\":2012,     \"yday\":8,     \"hour\":22,     \"min\":\"00\",     \"sec\":54,     \"isdst\":\"0\",     \"monthname\":\"January\",     \"weekday_short\":\"Mon\",     \"weekday\":\"Monday\",     \"ampm\":\"PM\",     \"tz_short\":\"PST\",     \"tz_long\":\"America/Los_Angeles\"    },    \"period\":1,    \"high\": {     \"fahrenheit\":\"49\",     \"celsius\":\"9\"    },    \"low\": {     \"fahrenheit\":\"38\",     \"celsius\":\"3\"    },    \"conditions\":\"Rain\",    \"icon\":\"rain\",    \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/rain.gif\",    \"skyicon\":\"mostlycloudy\",    \"pop\":80,    \"qpf_allday\": {     \"in\": 0.13,     \"mm\": 3.3    },    \"qpf_day\": {     \"in\": 0.10,     \"mm\": 2.5    },    \"qpf_night\": {     \"in\": 0.02,     \"mm\": 0.5    },    \"snow_allday\": {     \"in\": 0,     \"cm\": 0    },    \"snow_day\": {     \"in\": 0,     \"cm\": 0    },    \"snow_night\": {     \"in\": 0,     \"cm\": 0    },    \"maxwind\": {     \"mph\": 6,     \"kph\": 10,     \"dir\": \"South\",     \"degrees\": 180    },    \"avewind\": {     \"mph\": 2,     \"kph\": 3,     \"dir\": \"West\",     \"degrees\": 265    },    \"avehumidity\": 85,    \"maxhumidity\": 88,    \"minhumidity\": 76  },    {   \"date\":{    \"epoch\":\"1326261654\",    \"pretty\":\"10:00 PM PST on January 10, 2012\",    \"day\":10,    \"month\":1,    \"year\":2012,    \"yday\":9,    \"hour\":22,    \"min\":\"00\",    \"sec\":54,    \"isdst\":\"0\",    \"monthname\":\"January\",    \"weekday_short\":\"Tue\",    \"weekday\":\"Tuesday\",    \"ampm\":\"PM\",    \"tz_short\":\"PST\",    \"tz_long\":\"America/Los_Angeles\"   },   \"period\":2,   \"high\": {    \"fahrenheit\":\"45\",    \"celsius\":\"7\"   },   \"low\": {    \"fahrenheit\":\"32\",    \"celsius\":\"0\"   },   \"conditions\":\"Partly Cloudy\",   \"icon\":\"partlycloudy\",   \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/partlycloudy.gif\",   \"skyicon\":\"partlycloudy\",   \"pop\":10,   \"qpf_allday\": {    \"in\": 0.00,    \"mm\": 0.0   },   \"qpf_day\": {    \"in\": 0.00,    \"mm\": 0.0   },   \"qpf_night\": {    \"in\": 0.00,    \"mm\": 0.0   },   \"snow_allday\": {    \"in\": 0,    \"cm\": 0   },   \"snow_day\": {    \"in\": 0,    \"cm\": 0   },   \"snow_night\": {    \"in\": 0,    \"cm\": 0   },   \"maxwind\": {    \"mph\": 5,    \"kph\": 8,    \"dir\": \"NE\",    \"degrees\": 40   },   \"avewind\": {    \"mph\": 3,    \"kph\": 5,    \"dir\": \"ENE\",    \"degrees\": 60   },   \"avehumidity\": 81,   \"maxhumidity\": 91,   \"minhumidity\": 75  }  ,  {\"date\":{ \"epoch\":\"1326348054\", \"pretty\":\"10:00 PM PST on January 11, 2012\", \"day\":11, \"month\":1, \"year\":2012, \"yday\":10, \"hour\":22, \"min\":\"00\", \"sec\":54, \"isdst\":\"0\", \"monthname\":\"January\", \"weekday_short\":\"Wed\", \"weekday\":\"Wednesday\", \"ampm\":\"PM\", \"tz_short\":\"PST\", \"tz_long\":\"America/Los_Angeles\"},  \"period\":3,  \"high\": {  \"fahrenheit\":\"43\",  \"celsius\":\"6\"  },  \"low\": {  \"fahrenheit\":\"31\",  \"celsius\":\"-1\"  },  \"conditions\":\"Partly Cloudy\",  \"icon\":\"partlycloudy\",  \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/partlycloudy.gif\",  \"skyicon\":\"partlycloudy\",  \"pop\":0,  \"qpf_allday\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"qpf_day\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"qpf_night\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"snow_allday\": {  \"in\": 0,  \"cm\": 0  },  \"snow_day\": {  \"in\": 0,  \"cm\": 0  },  \"snow_night\": {  \"in\": 0,  \"cm\": 0  },  \"maxwind\": {  \"mph\": 4,  \"kph\": 6,  \"dir\": \"NNE\",  \"degrees\": 20  },  \"avewind\": {  \"mph\": 2,  \"kph\": 3,  \"dir\": \"NNE\",  \"degrees\": 31  },  \"avehumidity\": 73,  \"maxhumidity\": 85,  \"minhumidity\": 64  }  ,  {\"date\":{ \"epoch\":\"1326434454\", \"pretty\":\"10:00 PM PST on January 12, 2012\", \"day\":12, \"month\":1, \"year\":2012, \"yday\":11, \"hour\":22, \"min\":\"00\", \"sec\":54, \"isdst\":\"0\", \"monthname\":\"January\", \"weekday_short\":\"Thu\", \"weekday\":\"Thursday\", \"ampm\":\"PM\", \"tz_short\":\"PST\", \"tz_long\":\"America/Los_Angeles\"},  \"period\":4,  \"high\": {  \"fahrenheit\":\"45\",  \"celsius\":\"7\"  },  \"low\": {  \"fahrenheit\":\"36\",  \"celsius\":\"2\"  },  \"conditions\":\"Mostly Cloudy\",  \"icon\":\"mostlycloudy\",  \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/mostlycloudy.gif\",  \"skyicon\":\"mostlycloudy\",  \"pop\":10,  \"qpf_allday\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"qpf_day\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"qpf_night\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"snow_allday\": {  \"in\": 0,  \"cm\": 0  },  \"snow_day\": {  \"in\": 0,  \"cm\": 0  },  \"snow_night\": {  \"in\": 0,  \"cm\": 0  },  \"maxwind\": {  \"mph\": 4,  \"kph\": 6,  \"dir\": \"NNE\",  \"degrees\": 20  },  \"avewind\": {  \"mph\": 1,  \"kph\": 2,  \"dir\": \"NNE\",  \"degrees\": 26  },  \"avehumidity\": 71,  \"maxhumidity\": 85,  \"minhumidity\": 64  }  ,  {\"date\":{ \"epoch\":\"1326520854\", \"pretty\":\"10:00 PM PST on January 13, 2012\", \"day\":13, \"month\":1, \"year\":2012, \"yday\":12, \"hour\":22, \"min\":\"00\", \"sec\":54, \"isdst\":\"0\", \"monthname\":\"January\", \"weekday_short\":\"Fri\", \"weekday\":\"Friday\", \"ampm\":\"PM\", \"tz_short\":\"PST\", \"tz_long\":\"America/Los_Angeles\"},  \"period\":5,  \"high\": {  \"fahrenheit\":\"45\",  \"celsius\":\"7\"  },  \"low\": {  \"fahrenheit\":\"36\",  \"celsius\":\"2\"  },  \"conditions\":\"Mostly Cloudy\",  \"icon\":\"mostlycloudy\",  \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/mostlycloudy.gif\",  \"skyicon\":\"mostlycloudy\",  \"pop\":10,  \"qpf_allday\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"qpf_day\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"qpf_night\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"snow_allday\": {  \"in\": 0,  \"cm\": 0  },  \"snow_day\": {  \"in\": 0,  \"cm\": 0  },  \"snow_night\": {  \"in\": 0,  \"cm\": 0  },  \"maxwind\": {  \"mph\": 0,  \"kph\": 0,  \"dir\": \"East\",  \"degrees\": 80  },  \"avewind\": {  \"mph\": 0,  \"kph\": 0,  \"dir\": \"East\",  \"degrees\": 80  },  \"avehumidity\": 66,  \"maxhumidity\": 84,  \"minhumidity\": 57  }  ,  {\"date\":{ \"epoch\":\"1326607254\", \"pretty\":\"10:00 PM PST on January 14, 2012\", \"day\":14, \"month\":1, \"year\":2012, \"yday\":13, \"hour\":22, \"min\":\"00\", \"sec\":54, \"isdst\":\"0\", \"monthname\":\"January\", \"weekday_short\":\"Sat\", \"weekday\":\"Saturday\", \"ampm\":\"PM\", \"tz_short\":\"PST\", \"tz_long\":\"America/Los_Angeles\"},  \"period\":6,  \"high\": {  \"fahrenheit\":\"45\",  \"celsius\":\"7\"  },  \"low\": {  \"fahrenheit\":\"38\",  \"celsius\":\"3\"  },  \"conditions\":\"Chance of Rain\",  \"icon\":\"chancerain\",  \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/chancerain.gif\",  \"skyicon\":\"cloudy\",  \"pop\":50,  \"qpf_allday\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"qpf_day\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"qpf_night\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"snow_allday\": {  \"in\": 0,  \"cm\": 0  },  \"snow_day\": {  \"in\": 0,  \"cm\": 0  },  \"snow_night\": {  \"in\": 0,  \"cm\": 0  },  \"maxwind\": {  \"mph\": 5,  \"kph\": 8,  \"dir\": \"SSW\",  \"degrees\": 200  },  \"avewind\": {  \"mph\": 2,  \"kph\": 3,  \"dir\": \"South\",  \"degrees\": 185  },  \"avehumidity\": 75,  \"maxhumidity\": 78,  \"minhumidity\": 70  }  ,  {\"date\":{ \"epoch\":\"1326693654\", \"pretty\":\"10:00 PM PST on January 15, 2012\", \"day\":15, \"month\":1, \"year\":2012, \"yday\":14, \"hour\":22, \"min\":\"00\", \"sec\":54, \"isdst\":\"0\", \"monthname\":\"January\", \"weekday_short\":\"Sun\", \"weekday\":\"Sunday\", \"ampm\":\"PM\", \"tz_short\":\"PST\", \"tz_long\":\"America/Los_Angeles\"},  \"period\":7,  \"high\": {  \"fahrenheit\":\"43\",  \"celsius\":\"6\"  },  \"low\": {  \"fahrenheit\":\"36\",  \"celsius\":\"2\"  },  \"conditions\":\"Snow Showers\",  \"icon\":\"snow\",  \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/snow.gif\",  \"skyicon\":\"cloudy\",  \"pop\":60,  \"qpf_allday\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"qpf_day\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"qpf_night\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"snow_allday\": {  \"in\": 0,  \"cm\": 0  },  \"snow_day\": {  \"in\": 0,  \"cm\": 0  },  \"snow_night\": {  \"in\": 0,  \"cm\": 0  },  \"maxwind\": {  \"mph\": 6,  \"kph\": 10,  \"dir\": \"South\",  \"degrees\": 190  },  \"avewind\": {  \"mph\": 4,  \"kph\": 6,  \"dir\": \"South\",  \"degrees\": 180  },  \"avehumidity\": 80,  \"maxhumidity\": 85,  \"minhumidity\": 78  }  ,  {\"date\":{ \"epoch\":\"1326758454\", \"pretty\":\"4:00 PM PST on January 16, 2012\", \"day\":16, \"month\":1, \"year\":2012, \"yday\":15, \"hour\":16, \"min\":\"00\", \"sec\":54, \"isdst\":\"0\", \"monthname\":\"January\", \"weekday_short\":\"Mon\", \"weekday\":\"Monday\", \"ampm\":\"PM\", \"tz_short\":\"PST\", \"tz_long\":\"America/Los_Angeles\"},  \"period\":8,  \"high\": {  \"fahrenheit\":\"45\",  \"celsius\":\"7\"  },  \"low\": {  \"fahrenheit\":\"\",  \"celsius\":\"\"  },  \"conditions\":\"Rain Showers\",  \"icon\":\"rain\",  \"icon_url\":\"http://icons-ak.wxug.com/i/c/k/rain.gif\",  \"skyicon\":\"cloudy\",  \"pop\":60,  \"qpf_allday\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"qpf_day\": {  \"in\": 0.00,  \"mm\": 0.0  },  \"qpf_night\": {  \"in\": null,  \"mm\": null  },  \"snow_allday\": {  \"in\": 0,  \"cm\": 0  },  \"snow_day\": {  \"in\": 0,  \"cm\": 0  },  \"snow_night\": {  \"in\": null,  \"cm\": null  },  \"maxwind\": {  \"mph\": 5,  \"kph\": 8,  \"dir\": \"SSE\",  \"degrees\": 160  },  \"avewind\": {  \"mph\": 5,  \"kph\": 8,  \"dir\": \"SSE\",  \"degrees\": 150  },  \"avehumidity\": 78,  \"maxhumidity\": 79,  \"minhumidity\": 76  }  ]  } }}");
            //    }
            //}

            // Add default Units
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("Units"))
                IsolatedStorageSettings.ApplicationSettings.Add("Units", Units.English);

            if (!IsolatedStorageSettings.ApplicationSettings.Contains("MyCities"))
                IsolatedStorageSettings.ApplicationSettings.Add("MyCities", new List<City>());

            // for testing: new City() { name = "Seattle, Washington", c = "US", l = "/q/zmw:98101.1.99999" }

            myCities = (List<City>)IsolatedStorageSettings.ApplicationSettings["MyCities"];

            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            WasTombstoned = !e.IsApplicationInstancePreserved;
            if (WasTombstoned)
            {
                myCities = (List<City>)IsolatedStorageSettings.ApplicationSettings["MyCities"];
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings["MyCities"] = myCities;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings["MyCities"] = myCities;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject.GetType() != typeof(ApplicationShouldExit))
            {
                MessageBox.Show(e.ExceptionObject.Message, e.ExceptionObject.GetType().ToString(), MessageBoxButton.OK);

                if (e.ExceptionObject.Data.Contains("Handled"))
                {
                    e.Handled = true;
                }
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}