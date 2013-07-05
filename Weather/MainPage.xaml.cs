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
using Weather.ViewModels;

namespace Weather
{
    public partial class MainPage : PhoneApplicationPage
    {
        private bool isOldInstance;

        public MainPage()
        {
            InitializeComponent();
            isOldInstance = false;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (isOldInstance)
                throw new ApplicationShouldExit();
            
            List<City> myCities = (App.Current as App).myCities;
            if (myCities == null || myCities.Count == 0)
            {
                NavigationService.Navigate(new Uri("/AddCity.xaml", UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri("/Weather.xaml", UriKind.Relative));
            }

            isOldInstance = true;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NavigationService.RemoveBackEntry();
        }
    }
}