using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AsteroidExplorer.Models;

namespace AsteroidExplorer.Services
{
    public class NasaApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://api.nasa.gov";

        public NasaApiService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<List<Asteroid>> GetAsteroidsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var url = $"{BaseUrl}/neo/rest/v1/feed?start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}&api_key={_apiKey}";
                Console.WriteLine($"Fetching asteroids from URL: {url}");
                
                var response = await _httpClient.GetStringAsync(url);
                var data = JObject.Parse(response);
                var asteroids = new List<Asteroid>();
                
                // gets the near_earth_objects object
                var nearEarthObjects = data["near_earth_objects"] as JObject;
                if (nearEarthObjects == null)
                {
                    throw new Exception("No near earth objects found in the response");
                }

                // iterates through each date
                foreach (var dateEntry in nearEarthObjects)
                {
                    var dateAsteroids = dateEntry.Value as JArray;
                    if (dateAsteroids == null) continue;

                    foreach (var asteroidData in dateAsteroids)
                    {
                        try
                        {
                            var asteroid = new Asteroid
                            {
                                Id = asteroidData["id"]?.ToString(),
                                Name = asteroidData["name"]?.ToString(),
                                EstimatedDiameter = asteroidData["estimated_diameter"]?["kilometers"]?["estimated_diameter_max"]?.Value<double>() ?? 0,
                                IsPotentiallyHazardous = asteroidData["is_potentially_hazardous_asteroid"]?.Value<bool>() ?? false,
                                CloseApproachDate = DateTime.Parse(asteroidData["close_approach_data"]?[0]?["close_approach_date"]?.ToString() ?? DateTime.Now.ToString()),
                                MissDistance = double.Parse(asteroidData["close_approach_data"]?[0]?["miss_distance"]?["kilometers"]?.ToString() ?? "0"),
                                OrbitingBody = asteroidData["close_approach_data"]?[0]?["orbiting_body"]?.ToString()
                            };
                            asteroids.Add(asteroid);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing asteroid: {ex.Message}");
                            continue;
                        }
                    }
                }

                return asteroids;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching asteroids: {ex.Message}");
                throw;
            }
        }

        public async Task<Apod> GetApodAsync(DateTime date)
        {
            try
            {
                var url = $"{BaseUrl}/planetary/apod?date={date:yyyy-MM-dd}&api_key={_apiKey}";
                Console.WriteLine($"Fetching APOD from URL: {url}");
                
                var response = await _httpClient.GetStringAsync(url);
                Console.WriteLine($"Raw APOD Response: {response}");
                
                var data = JObject.Parse(response);
                Console.WriteLine($"Parsed JSON data: {JsonConvert.SerializeObject(data, Formatting.Indented)}");
                
                var apod = new Apod
                {
                    Copyright = data["copyright"]?.ToString(),
                    Date = DateTime.Parse(data["date"]?.ToString() ?? DateTime.Now.ToString()),
                    Explanation = data["explanation"]?.ToString(),
                    HdUrl = data["hdurl"]?.ToString(),
                    MediaType = data["media_type"]?.ToString(),
                    ServiceVersion = data["service_version"]?.ToString(),
                    Title = data["title"]?.ToString(),
                    Url = data["url"]?.ToString()
                };

                // if media type is "other" and URL is empty, constructs the NASA APOD URL
                if (apod.MediaType?.ToLower() == "other" && string.IsNullOrEmpty(apod.Url))
                {
                    apod.Url = $"https://apod.nasa.gov/apod/ap{apod.Date:yyMMdd}.html";
                }

                Console.WriteLine($"APOD Media Type: {apod.MediaType}");
                Console.WriteLine($"APOD URL: {apod.Url}");
                Console.WriteLine($"APOD Title: {apod.Title}");
                Console.WriteLine($"APOD Explanation: {apod.Explanation}");
                Console.WriteLine($"APOD HD URL: {apod.HdUrl}");
                Console.WriteLine($"APOD Service Version: {apod.ServiceVersion}");
                Console.WriteLine($"Raw URL from API: {data["url"]}");
                Console.WriteLine($"Raw Media Type from API: {data["media_type"]}");
                
                // only checks URL accessibility if we have a valid URL
                if (!string.IsNullOrEmpty(apod.Url))
                {
                    try
                    {
                        var urlCheck = await _httpClient.GetAsync(apod.Url);
                        Console.WriteLine($"URL accessibility check: {urlCheck.StatusCode}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"URL accessibility check failed: {ex.Message}");
                    }
                }
                
                return apod;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching APOD: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 