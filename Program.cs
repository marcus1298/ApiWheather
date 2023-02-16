using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClimaELeituraDeCidades
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Cria as tarefas
            var task1 = Task.Run(() => GetWeatherAsync("Rio de Janeiro"));
            var task2 = Task.Run(() => ReadCityListAsync("cidades.txt"));

            // Aguarda o término de ambas as tarefas
            await Task.WhenAll(task1, task2);

            // Exibe as informações
            Console.WriteLine($"Tempo em {task1.Result.City}: {task1.Result.Weather}");
            Console.WriteLine("Cidades:");
            foreach (var city in task2.Result)
            {
                Console.WriteLine(city);
            }

            Console.ReadKey();
        }

        static async Task<WeatherInfo> GetWeatherAsync(string city)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"http://api.openweathermap.org/data/2.5/weather?q={city}&appid=YOUR_API_KEY");
                var content = await response.Content.ReadAsStringAsync();

                // Trata a resposta da API e retorna as informações do tempo
                return new WeatherInfo
                {
                    City = city,
                    Weather = ParseWeather(content)
                };
            }
        }

        static async Task<List<string>> ReadCityListAsync(string fileName)
        {
            var cities = new List<string>();
            using (var reader = new StreamReader(fileName))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    cities.Add(line);
                }
            }

            return cities;
        }

        static string ParseWeather(string content)
        {
            // Faz o parsing do JSON retornado pela API para obter as informações sobre o tempo
            // Neste exemplo, retorna somente a descrição do tempo
            // Você pode alterar esta implementação para obter outras informações como temperatura, umidade, etc.
            return content.Contains("\"weather\":") ?
                content.Substring(content.IndexOf("\"description\":") + 15, content.IndexOf(",\"icon\"") - content.IndexOf("\"description\":") - 15) :
                "N/A";
        }
    }

    class WeatherInfo
    {
        public string City { get; set; }
        public string Weather { get; set; }
    }
}