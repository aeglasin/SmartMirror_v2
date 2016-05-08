using SmartMirror.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

using Windows.UI.Xaml.Navigation;
using System.Xml.Serialization;

namespace SmartMirror.Weather
{
    public class Weather_ViewModel : ViewModelBase
    {
        private string _location;
        private string _description;
        private ImageSource _icon;
        private string _temperature;
        private string _temperatureMin;
        private string _temperatureMax;
        private string _humidity;
        private string _wind;

        public Weather_ViewModel()
        {
            getWeather();
        }

        public async void getWeather()
        {
                string city = "Heilbronn";
                string countryCode = "DE";
                string countryName = "Germany";

                location = city + ", " + countryName;
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"http://api.openweathermap.org/data/2.5/weather?q={city},{countryCode}&APPID=f15ff1318496f5bf9792606706a5059a");
                HttpClient client = new HttpClient();
                var response = client.SendAsync(request).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    var bytes = Encoding.Unicode.GetBytes(result);
                    using (MemoryStream stream = new MemoryStream(bytes))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(WeatherInfo));
                        var weatherInfo = (WeatherInfo)serializer.ReadObject(stream);


                        description = weatherInfo.weather[0].description.ToString().ToUpper();

                        var imagePath = "http://openweathermap.org/img/w/" + weatherInfo.weather[0].icon + ".png";
                        Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                        ImageSource imgSource = new BitmapImage(uri);
                        icon = imgSource;


                    temperature = "Temperature:";
                        temperatureMin = "Low:°C";
                        temperatureMax = "High:";

                        humidity = "Humidity: ";
                        wind = "Wind Speed: ";

                    temperature = $"Temperature: {(weatherInfo.main.temp - 273.15f):F2} °C";
                    temperatureMin = $"Low: {(weatherInfo.main.temp_min - 273.15f):F2} °C";
                    temperatureMax = $"High: {(weatherInfo.main.temp_max - 273.15f):F2} °C";

                    humidity = "Humidity: " + weatherInfo.main.humidity + " %";
                    wind = "Wind Speed: " + weatherInfo.wind.speed + " km/h";

                }
            }
                else
                {
                    description = "Error";
                }
        }


        public string location
        {
            get { return _location; }
            set { SetProperty(ref _location, value); }
        }

        public string description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        public ImageSource icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }

        public string temperature
        {
            get { return _temperature; }
            set { SetProperty(ref _temperature, value); }
        }

        public string temperatureMin
        {
            get { return _temperatureMin; }
            set { SetProperty(ref _temperatureMin, value); }
        }

        public string temperatureMax
        {
            get { return _temperatureMax; }
            set { SetProperty(ref _temperatureMax, value); }
        }

        public string humidity
        {
            get { return _humidity; }
            set { SetProperty(ref _humidity, value); }
        }

        public string wind
        {
            get { return _wind; }
            set { SetProperty(ref _wind, value); }
        }

        public class Temperature
        {
            public double temp { get; set; }
            public double humidity { get; set; }
            public double temp_min { get; set; }
            public double temp_max { get; set; }
        }

        public class Wind
        {
            public double speed { get; set; }
        }

        public class Weather
        {
            public string description { get; set; }
            public string icon { get; set; }
        }

        public class WeatherInfo
        {
            internal object Weather;

            public Temperature main { get; set; }
            public Wind wind { get; set; }
            public List<Weather> weather { get; set; }
        }
    }
}

