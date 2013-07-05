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
using System.Collections.ObjectModel;
using Weather.ViewModels;
using System.Net.NetworkInformation;

namespace Weather
{
    public partial class AddCity : PhoneApplicationPage
    {
        public ObservableCollection<City> cityResults;
        public List<City> myCities;
        public CurrentWeatherGetter myGetter;
        public bool isOldInstance;

        public AddCity()
        {
            InitializeComponent();

            myCities = (Application.Current as App).myCities;
            cityResults = new ObservableCollection<City>();
            isOldInstance = false;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            myGetter = new CurrentWeatherGetter(CitiesListBox, CallType.AddCity, ProgressBar);
            AddCityTextBox.Focus();  
        }

        private void CitiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListBox).SelectedIndex != -1)
            {
                myCities.Add((City)e.AddedItems[0]);
                NavigationService.Navigate(new Uri("/Weather.xaml?id=" + (myCities.Count - 1).ToString(), UriKind.Relative));
            }
            (sender as ListBox).SelectedIndex = -1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("There is currently no network information available. No weather data will be displayed.", "No network connection", MessageBoxButton.OK);
                return;
            }

            if (AddCityTextBox.Text != string.Empty)
            {
                ProgressBar.Visibility = System.Windows.Visibility.Visible;
                string s = AddCityTextBox.Text.Replace(" ", "%20");
                myGetter.TryGetWeather(new Uri("http://autocomplete.wunderground.com/aq?query=" + s + "&format=JSON"));
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NavigationService.RemoveBackEntry();
        }
    }
}