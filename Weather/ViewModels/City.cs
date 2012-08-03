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

namespace Weather.ViewModels
{
    [DataContract]
    public class CityWrapper
    {
        [DataMember]
        public List<City> RESULTS;
    }

    [DataContract]
    public class City : IComparable<City>
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string c { get; set; }

        [DataMember]
        public string l { get; set; }

        public string ColloquialName 
        {
            get
            {
                return name.Split(',')[0].ToLower().Trim();
            }
        }

        public int CompareTo(City other)
        {
            return this.l.CompareTo(other.l);
        }
    }
}
