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
using System.Collections.ObjectModel;
using Microsoft.Phone.Shell;

namespace Weather
{
    public partial class AllCities : PhoneApplicationPage
    {
        public ObservableCollection<City> myCities { get; set; }
        public ApplicationBarIconButton deleteButton { get; set; }
        public ApplicationBarIconButton defaultButton { get; set; }
        public ApplicationBarIconButton addButton { get; set; }
        public EditMode PageMode { get; set; }

        public enum EditMode
        {
            Edit,
            Static
        }


        public AllCities()
        {
            InitializeComponent();

            myCities = new ObservableCollection<City>((App.Current as App).myCities);
            this.PageMode = EditMode.Static;
            CitiesListBox.DataContext = myCities;
        }

        private void CitiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CitiesListBox.SelectedIndex == -1)
                return;

            if (PageMode == EditMode.Edit)
            {
                if (deleteButton == null)
                    BuildDeleteButton();
                if (defaultButton == null)
                    BuildDefaultButton();

                ApplicationBar.Buttons.Add(deleteButton);
                ApplicationBar.Buttons.Add(defaultButton);
            }
            else if (PageMode == EditMode.Static)
            {
                NavigationService.Navigate(new Uri("/Weather.xaml?id=" + CitiesListBox.SelectedIndex, UriKind.Relative));
            }
        }

        public void BuildDeleteButton()
        {
            this.deleteButton = new ApplicationBarIconButton(new Uri("/Images/appbar.delete.rest.png", UriKind.Relative));
            deleteButton.Text = "delete";
            deleteButton.Click += new EventHandler(button1_click);
        }

        public void BuildDefaultButton()
        {
            this.defaultButton = new ApplicationBarIconButton(new Uri("/Images/appbar.favs.rest.png", UriKind.Relative));
            defaultButton.Text = "default";
            defaultButton.Click += new EventHandler(button2_click);
        }

        public void BuildAddButton()
        {
            this.addButton = new ApplicationBarIconButton(new Uri("/Images/appbar.add.rest.png", UriKind.Relative));
            addButton.Text = "add";
            addButton.Click += new EventHandler(button3_click);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (deleteButton == null)
                BuildDeleteButton();

            base.OnNavigatedTo(e);
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddCity.xaml", UriKind.Relative));
        }

        public void button1_click(object sender, EventArgs e)
        {
            if (CitiesListBox.SelectedIndex == -1)
                return;

            City toDelete = (City)CitiesListBox.SelectedItem;
            (App.Current as App).myCities.Remove(toDelete);

            this.myCities = new ObservableCollection<City>((App.Current as App).myCities);
        }

        public void button2_click(object sender, EventArgs e)
        {
            if (CitiesListBox.SelectedIndex == -1)
                return;

            City defaultCity = (City)CitiesListBox.SelectedItem;
            (App.Current as App).myCities.Remove(defaultCity);
            (App.Current as App).myCities.Insert(0, defaultCity);

            MessageBox.Show(defaultCity.ColloquialName + " is now your default city.", "Operation successful", MessageBoxButton.OK);

            this.myCities = new ObservableCollection<City>((App.Current as App).myCities);
        }

        public void button3_click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddCity.xaml", UriKind.Relative));
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            this.PageMode = EditMode.Edit;
            ApplicationBar.Buttons.RemoveAt(0);

            BuildAddButton();
            ApplicationBar.Buttons.Add(addButton);
        }

    }
}