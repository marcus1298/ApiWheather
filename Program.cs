using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace YahooWeatherExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string city = "New York";
            string weatherApiUrl = $"https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20weather.forecast%20where%20woeid%20in%20(select%20woeid%20from%20geo.places(1)%20where%20text%3D%22{city}%22)&format=xml&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";

            var weatherTask = WeatherService.GetWeatherAsync(weatherApiUrl);

            string citiesFile = Path.Combine(Directory.GetCurrentDirectory(), "cities.txt");
            var citiesTask = CityListReader.ReadCityListAsync(citiesFile);

            await Task.WhenAll(weatherTask, citiesTask);

            var weatherInfo = weatherTask.Result;
            var cities = citiesTask.Result;

            Console.WriteLine("Current weather in " + city + ": " + weatherInfo.Description + ", " + weatherInfo.Temperature + "°F");
            Console.WriteLine("List of cities: ");
            foreach (var c in cities)
            {
                Console.WriteLine(c);
            }

            Console.ReadLine();
        }
    }

    public class WeatherService
    {
        public static async Task<WeatherInfo> GetWeatherAsync(string apiUrl)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(apiUrl);
                var xml = await response.Content.ReadAsStringAsync();
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                var temp = doc.SelectSingleNode("//yweather:condition", GetNamespaceManager()).Attributes["temp"].Value;
                var description = doc.SelectSingleNode("//yweather:condition", GetNamespaceManager()).Attributes["text"].Value;

                return new WeatherInfo { Temperature = temp, Description = description };
            }
        }

        private static XmlNamespaceManager GetNamespaceManager()
        {
            var nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace("yweather", "http://xml.weather.yahoo.com/ns/rss/1.0");
            return nsManager;
        }
    }

    public class CityListReader
    {
        public static async Task<List<string>> ReadCityListAsync(string fileName)
        {
            var cities = new List<string>();

            using (var reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    cities.Add(line);
                }
            }

            return cities;
        }
    }

    public class WeatherInfo
    {
        public string Temperature { get; set; }
        public string Description { get; set; }
    }
}