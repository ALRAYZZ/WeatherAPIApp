using WeatherAPI.Models;

namespace WeatherAPI.Services
{
	public interface IWeatherService
	{
		Task<WeatherDataModel> GetWeatherDataAsync(string location);
	}
}
