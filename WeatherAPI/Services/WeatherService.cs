using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using WeatherAPI.Configuration;
using WeatherAPI.Models;

namespace WeatherAPI.Services
{
	public class WeatherService : IWeatherService
	{
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;
		private readonly IDistributedCache _cache;
		private readonly SemaphoreSlim _rateLimiter;

		public WeatherService(HttpClient httpClient, IOptions<WeatherApiOptions> options, IDistributedCache cache)
		{
			_httpClient = httpClient;
			_apiKey = options.Value.ApiKey;
			_cache = cache;
			_rateLimiter = new SemaphoreSlim(5);
		}



		public async Task<WeatherDataModel> GetWeatherDataAsync(string location)
		{
			await _rateLimiter.WaitAsync();
			try
			{
				var cacheKey = $"WeatherData_{location}";
				var cachedData = await _cache.GetStringAsync(cacheKey);

				if (!string.IsNullOrEmpty(cachedData))
				{
					return JsonSerializer.Deserialize<WeatherDataModel>(cachedData);
				}


				var response = await _httpClient.GetAsync($"{location}?key={_apiKey}");

				if (response.StatusCode == HttpStatusCode.BadRequest)
				{
					throw new HttpRequestException("Bad API Request", null, HttpStatusCode.BadRequest);
				}

				response.EnsureSuccessStatusCode();

				var responseBody = await response.Content.ReadAsStringAsync();
				using var document = JsonDocument.Parse(responseBody);

				var root = document.RootElement;
				var temperatureFahrenheit = root.GetProperty("currentConditions").GetProperty("temp").GetDouble();
				var temperatureCelsius = Math.Round((temperatureFahrenheit - 32) * 5 / 9, 1);
				var weatherData = new WeatherDataModel
				{
					City = root.GetProperty("address").GetString(),
					Conditions = root.GetProperty("currentConditions").GetProperty("conditions").GetString(),
					Temperature = temperatureCelsius,
					Humidity = root.GetProperty("currentConditions").GetProperty("humidity").GetDouble()
				};

				var serializedData = JsonSerializer.Serialize(weatherData);
				var cacheOptions = new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
				};

				await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions);

				return weatherData;
			}
			finally
			{
				_rateLimiter.Release();
			}


		}
	}
}