//Author: Chandler Ford
//Class: CSS 490 A
//Last Modified: 10/19/18
//Description: Weather API

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace WeatherAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new HttpClient())
            {
                //Intro
                Console.WriteLine("-------------------------------------");
                Console.WriteLine("CLOUD COMPUTING: WEATHER API PROGRAM");

                //Main program loop
                while (true)
                {
                    //User input for city name
                    Console.WriteLine("-------------------------------------");
                    Console.WriteLine("Enter a city name, e.g. Seattle (enter nothing or \"exit\" to quit):");
                    string userCityName = Console.ReadLine();
                    if (userCityName == "exit")
                    {
                        break;
                    }
                    else if (userCityName == "")
                    {
                        break;
                    }
                    Console.WriteLine();

                    //User input for country initials
                    Console.WriteLine("Enter a country initials, e.g. US (enter nothing or \"exit\" to quit):");
                    string userCountryInitials = Console.ReadLine();
                    if (userCountryInitials == "exit")
                    {
                        break;
                    }
                    else if (userCountryInitials == "")
                    {
                        break;
                    }
                    Console.WriteLine();

                    //URL string building blocks
                    string baseURL = "https://api.openweathermap.org/data/2.5/weather?q=";
                    string cityName = userCityName;
                    string countryInitials = userCountryInitials;
                    string units = "&units=imperial";
                    string appID = "&appid=371c1108d6ead6e61db9ed6bb5018bb2";
                    string searchTerm = baseURL + cityName + "," + countryInitials + units + appID;

                    //Get JSON
                    HttpResponseMessage response = client.GetAsync(searchTerm).Result;
                    if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                    {
                        Console.WriteLine("Program has encountered return error code - the location you have searched for can't be found.");
                        continue;
                    }
                    response.EnsureSuccessStatusCode();
                    String result = response.Content.ReadAsStringAsync().Result;
                    //Console.WriteLine(result);
                    //Console.WriteLine();

                    //Deserialize JSON and print information
                    Rootobject theWeather = JsonConvert.DeserializeObject<Rootobject>(result);
                    Console.WriteLine("Location: " + theWeather.name + ", " + theWeather.sys.country);
                    Console.WriteLine("Temperature: " + theWeather.main.temp + " F");
                    Console.WriteLine("Min Temperature: " + theWeather.main.temp_min + " F");
                    Console.WriteLine("Max Temperature: " + theWeather.main.temp_max + " F");
                    Console.WriteLine("Current condition: " + theWeather.weather[0].description);
                    Console.WriteLine("Wind: " + theWeather.wind.speed + " mph");
                    Console.WriteLine("Pressure: " + theWeather.main.pressure + " hPa");
                    Console.WriteLine("Humidity: " + theWeather.main.humidity + "%");
                }
                Console.WriteLine();
                Console.WriteLine("[Exiting program]");
                Console.WriteLine();
            }
        }
    }
}

//OpenWeatherMap API JSON classes
public class Rootobject
{
    public Coord coord { get; set; }
    public Weather[] weather { get; set; }
    public string _base { get; set; }
    public Main main { get; set; }
    public int visibility { get; set; }
    public Wind wind { get; set; }
    public Clouds clouds { get; set; }
    public int dt { get; set; }
    public Sys sys { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public int cod { get; set; }
}

public class Coord
{
    public float lon { get; set; }
    public float lat { get; set; }
}

public class Main
{
    public float temp { get; set; }
    public int pressure { get; set; }
    public int humidity { get; set; }
    public float temp_min { get; set; }
    public float temp_max { get; set; }
}

public class Wind
{
    public float speed { get; set; }
    public float deg { get; set; }
}

public class Clouds
{
    public int all { get; set; }
}

public class Sys
{
    public int type { get; set; }
    public int id { get; set; }
    public float message { get; set; }
    public string country { get; set; }
    public int sunrise { get; set; }
    public int sunset { get; set; }
}

public class Weather
{
    public int id { get; set; }
    public string main { get; set; }
    public string description { get; set; }
    public string icon { get; set; }
}
