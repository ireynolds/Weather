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
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.IO.IsolatedStorage;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Weather.ViewModels;
using System.Net.NetworkInformation;
using Microsoft.Phone.Shell;

namespace Weather
{
    public partial class Weather : PhoneApplicationPage
    {
        public ObservableCollection<CurrentWeatherViewModel> Forecasts { get; set; }
        public List<City> myCities { get; set; }
        public City currCity { get; set; }
        public bool isOldInstance { get; set; }

        // Constructor
        public Weather()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            this.Forecasts = new ObservableCollection<CurrentWeatherViewModel>();
            myCities = (Application.Current as App).myCities;

            if (myCities.Count > 0)
                currCity = myCities.ElementAt(0);

            isOldInstance = false;
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            myPanorama.Title = currCity.ColloquialName;

            // load data from isolated storage
            //LoadOldData();


            // search for new data
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                ProgressBarCurrent.Visibility = Visibility.Visible;
                ProgressBarForecast.Visibility = Visibility.Visible;
                LoadNewData();
            }
            else
            {
                MessageBox.Show("There is currently no network information available. No weather data will be displayed.", "No network connection", MessageBoxButton.OK);
            }
        }

        private void LoadNewData()
        {
            BackgroundWorker bw1 = new BackgroundWorker();
            bw1.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw1.RunWorkerAsync(new object[] { ForecastListBox, 
                                              new Uri("http://api.wunderground.com/api/d359263cceaa5dd7/forecast7day" + currCity.l + ".json"),
                                              CallType.SevenDay,
                                              ProgressBarForecast
                                            });


            BackgroundWorker bw2 = new BackgroundWorker();
            bw2.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw2.RunWorkerAsync(new object[] { CurrentWeather, 
                                              new Uri("http://api.wunderground.com/api/d359263cceaa5dd7/conditions" + currCity.l + ".json"),
                                              CallType.Current,
                                              ProgressBarCurrent
                                            });
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] state = (object[])e.Argument;
            FrameworkElement list = (FrameworkElement)state[0];
            Uri uri = (Uri)state[1];
            CallType typeOf = (CallType)state[2];
            ProgressBar myProgressBar = (ProgressBar)state[3];

            CurrentWeatherGetter cwg = new CurrentWeatherGetter(list, typeOf, myProgressBar);
            cwg.TryGetWeather(uri);
        }

        private void LoadOldData()
        {
            CurrentConditionsWrapper myWrapper;
            using (IsolatedStorageFile myIso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!myIso.FileExists("Seattle.txt"))
                    throw new FileNotFoundException();

                using (StreamReader myReader = new StreamReader(new IsolatedStorageFileStream("Seattle.txt", FileMode.Open, myIso)))
                {
                    myWrapper = CurrentWeatherGetter.TranslateJson<CurrentConditionsWrapper>(myReader.ReadToEnd());
                }
            }

            // remove any previous data
            if (myWrapper != null)
            {
                CurrentWeatherViewModel forecast = new CurrentWeatherViewModel(myWrapper);
                this.Forecasts.Remove(forecast);
                this.Forecasts.Add(forecast);
            }
        }

        private void ForecastListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ForecastListBox.SelectedIndex = -1;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if ((App.Current as App).myCities.Count == 0)
            {
                NavigationService.Navigate(new Uri("/AddCity.xaml", UriKind.Relative));
                return;
            }

            if (!isOldInstance)
            {
                // Load selected city
                string s = string.Empty;
                if (NavigationContext.QueryString.TryGetValue("id", out s))
                    this.currCity = myCities.ElementAt(int.Parse(s));
                else
                    this.currCity = myCities.ElementAt(0); // default

                isOldInstance = true;
            }

            base.OnNavigatedTo(e);
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AllCities.xaml", UriKind.Relative));
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            MainPage_Loaded(this, default(RoutedEventArgs));
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void ApplicationBar_StateChanged(object sender, Microsoft.Phone.Shell.ApplicationBarStateChangedEventArgs e)
        {
            if (e.IsMenuVisible)
                (sender as ApplicationBar).Opacity = 0.999;
            else if (!e.IsMenuVisible)
                (sender as ApplicationBar).Opacity = 0.5;
        }
    }

    public class RequestState
    {
        // This class stores the State of the request.
        const int BUFFER_SIZE = 1024;
        public StringBuilder requestData;
        public byte[] BufferRead;
        public HttpWebRequest request;
        public HttpWebResponse response;
        public Stream streamResponse;

        public RequestState()
        {
            BufferRead = new byte[BUFFER_SIZE];
            requestData = new StringBuilder("");
            request = null;
            streamResponse = null;
        }
    }

    public class CurrentWeatherGetter
    {
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        private const int BUFFER_SIZE = 1024;
        private FrameworkElement control;
        public CallType typeOf;
        public ProgressBar progBar;


        public CurrentWeatherGetter(FrameworkElement control, CallType typeOf, ProgressBar myProgressBar)
        {
            this.control = control;
            this.typeOf = typeOf;
            this.progBar = myProgressBar;
        }

        public CurrentWeatherGetter(FrameworkElement control, CallType typeOf)
        {
            this.control = control;
            this.typeOf = typeOf;
        }

        public static valueType TranslateJson<valueType>(string json)
        {
            valueType value;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(valueType));
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                value = (valueType)serializer.ReadObject(stream);
            }
            return value;
        }

        public static valueType TranslateJson<valueType>(Stream jsonStream)
        {
            valueType value;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(valueType));
            value = (valueType)serializer.ReadObject(jsonStream);
            return value;
        }

        public void TryGetWeather(Uri source)
        {
            HttpWebRequest myHttpWebRequest1 = (HttpWebRequest)WebRequest.Create(source);

            RequestState myRequestState = new RequestState();
            myRequestState.request = myHttpWebRequest1;

            IAsyncResult result =
                (IAsyncResult)myHttpWebRequest1.BeginGetResponse(new AsyncCallback(RespCallback), myRequestState);
        }

        private void RespCallback(IAsyncResult asynchronousResult)
        {
            // State is asynchronous
            RequestState myRequestState = (RequestState)asynchronousResult.AsyncState;
            HttpWebRequest myHttpWebRequest2 = myRequestState.request;
            myRequestState.response = (HttpWebResponse)myHttpWebRequest2.EndGetResponse(asynchronousResult);

            // Read the response into a Stream object
            Stream responseStream = myRequestState.response.GetResponseStream();

            // new

            // do something with the response stream here
            //if (this.typeOf == CallType.SevenDay)
            //{
            //    ParseAsForecast(responseStream);
            //}
            //else if (this.typeOf == CallType.Current)
            //{
            //    ParseAsCurrent(responseStream);
            //}
            //else if (this.typeOf == CallType.AddCity)
            //{
            //    ParseAsCities(responseStream);
            //}

            //if (progBar != null)
            //    progBar.Dispatcher.BeginInvoke(delegate { progBar.Visibility = Visibility.Collapsed; });
            //responseStream.Close();
            //allDone.Set();
            
            // end new
            
            myRequestState.streamResponse = responseStream;

            // Begin the Reading of the contents of the HTML page
            IAsyncResult AsynchronousInputRead = responseStream.BeginRead(myRequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallback), myRequestState);
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            RequestState myRequestState = (RequestState)asyncResult.AsyncState;
            Stream responseStream = myRequestState.streamResponse;
            int read = responseStream.EndRead(asyncResult);

            // read the HTML page and do something with it
            if (read > 0)
            {
                myRequestState.requestData.Append(Encoding.UTF8.GetString(myRequestState.BufferRead, 0, read));
                IAsyncResult asynchronousResult = responseStream.BeginRead(myRequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallback), myRequestState);
            }
            else
            {
                if (myRequestState.requestData.Length > 1)
                {
                    string stringContent;
                    stringContent = myRequestState.requestData.ToString();

                    // do something with the response stream here
                    if (this.typeOf == CallType.SevenDay)
                    {
                        ParseAsForecast(stringContent);
                    }
                    else if (this.typeOf == CallType.Current)
                    {
                        ParseAsCurrent(stringContent);
                    }
                    else if (this.typeOf == CallType.AddCity)
                    {
                        ParseAsCities(stringContent);
                    }
                }

                if (progBar != null)
                    progBar.Dispatcher.BeginInvoke(delegate { progBar.Visibility = Visibility.Collapsed; });
                responseStream.Close();
                allDone.Set();
            }
        }

        private void ParseAsCurrent(string stringContent)
        {
            CurrentConditionsWrapper myWrapper = CurrentWeatherGetter.TranslateJson<CurrentConditionsWrapper>(stringContent);

            if (myWrapper == null)
                return;

            CurrentWeatherViewModel myView = new CurrentWeatherViewModel(myWrapper);
            //ObservableCollection<CurrentWeatherViewModel> newDataContext = new ObservableCollection<CurrentWeatherViewModel>();
            //newDataContext.Add(new CurrentWeatherViewModel(myWrapper));
            this.control.Dispatcher.BeginInvoke(delegate { control.DataContext = myView; });
        }

        private void ParseAsForecast(string stringContent)
        {
            ForecastWrapper myWrapper = CurrentWeatherGetter.TranslateJson<ForecastWrapper>(stringContent);

            if (myWrapper == null)
                return;

            ObservableCollection<ForecastViewModel> newDataContext = new ObservableCollection<ForecastViewModel>();

            List<TxtForecastDay> myForecasts = new List<TxtForecastDay>();
            bool hasHitTomorrow = false;
            foreach (TxtForecastDay txtForecast in myWrapper.forecast.txt_forecast.forecastday)
            {
                // skip any forecasts for today
                if (!hasHitTomorrow)
                    if (DateTime.Now.AddDays(1).DayOfWeek.ToString().ToLower().Equals(txtForecast.title.ToLower()))
                        hasHitTomorrow = true;
                
                // e.g., [0,1] are [tomorrow,tomorrow_night] 
                if (hasHitTomorrow)
                    myForecasts.Add(txtForecast);
            }

            bool haveHitTomorrow = false;
            int count = -1;
            foreach (ForecastDay day in myWrapper.forecast.simpleforecast.forecastday)
            {
                if (haveHitTomorrow)
                    newDataContext.Add(new ForecastViewModel(day) { DayPrediction = myForecasts.ElementAt(count).fcttext, NightPrediction = myForecasts.ElementAt(count + 1).fcttext });
                else
                    haveHitTomorrow = true;
                count++;
            }

            this.control.Dispatcher.BeginInvoke(delegate { control.DataContext = newDataContext; });
        }

        private void ParseAsCities(string stringContent)
        {
            CityWrapper myWrapper = CurrentWeatherGetter.TranslateJson<CityWrapper>(stringContent);

            if (myWrapper == null)
                return;

            ObservableCollection<City> myCities = new ObservableCollection<City>();

            foreach (City c in myWrapper.RESULTS)
            {
                myCities.Add(c);
            }

            this.control.Dispatcher.BeginInvoke(delegate { control.DataContext = myCities; });
        }

        private void ParseAsCurrent(Stream jsonStream)
        {
            CurrentConditionsWrapper myWrapper = CurrentWeatherGetter.TranslateJson<CurrentConditionsWrapper>(jsonStream);

            if (myWrapper == null)
                return;

            CurrentWeatherViewModel myView = new CurrentWeatherViewModel(myWrapper);
            //ObservableCollection<CurrentWeatherViewModel> newDataContext = new ObservableCollection<CurrentWeatherViewModel>();
            //newDataContext.Add(new CurrentWeatherViewModel(myWrapper));
            this.control.Dispatcher.BeginInvoke(delegate { control.DataContext = myView; });
        }

        private void ParseAsForecast(Stream jsonStream)
        {
            ForecastWrapper myWrapper = CurrentWeatherGetter.TranslateJson<ForecastWrapper>(jsonStream);

            if (myWrapper == null)
                return;

            ObservableCollection<ForecastViewModel> newDataContext = new ObservableCollection<ForecastViewModel>();

            List<TxtForecastDay> myForecasts = new List<TxtForecastDay>();
            bool hasHitTomorrow = false;
            foreach (TxtForecastDay txtForecast in myWrapper.forecast.txt_forecast.forecastday)
            {
                // skip any forecasts for today
                if (!hasHitTomorrow)
                    if (DateTime.Now.AddDays(1).DayOfWeek.ToString().ToLower().Equals(txtForecast.title.ToLower()))
                        hasHitTomorrow = true;

                // e.g., [0,1] are [tomorrow,tomorrow_night] 
                if (hasHitTomorrow)
                    myForecasts.Add(txtForecast);
            }

            bool haveHitTomorrow = false;
            int count = -1;
            foreach (ForecastDay day in myWrapper.forecast.simpleforecast.forecastday)
            {
                if (haveHitTomorrow)
                    newDataContext.Add(new ForecastViewModel(day) { DayPrediction = myForecasts.ElementAt(count).fcttext, NightPrediction = myForecasts.ElementAt(count + 1).fcttext });
                else
                    haveHitTomorrow = true;
                count++;
            }

            this.control.Dispatcher.BeginInvoke(delegate { control.DataContext = newDataContext; });
        }

        private void ParseAsCities(Stream jsonStream)
        {
            CityWrapper myWrapper = CurrentWeatherGetter.TranslateJson<CityWrapper>(jsonStream);

            if (myWrapper == null)
                return;

            ObservableCollection<City> myCities = new ObservableCollection<City>();

            foreach (City c in myWrapper.RESULTS)
            {
                myCities.Add(c);
            }

            this.control.Dispatcher.BeginInvoke(delegate { control.DataContext = myCities; });
        }
    }

    public class myMemoryStream : MemoryStream
    {
        public bool canTimeout
        {
            get
            {
                return true;
            }
        }

        public myMemoryStream(Byte[] bytes)
            : base(bytes) { }
    }
}