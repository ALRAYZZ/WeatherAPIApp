using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using WeatherAPI.Services;

namespace WeatherAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class WeatherController : Controller
	{
		private readonly IWeatherService _weatherService;
		public WeatherController(IWeatherService weatherService)
		{
			_weatherService = weatherService;
		}

		[HttpGet("current")]
		public async Task<IActionResult> GetWeatherData(string location)
		{

			try
			{
				var weatherData = await _weatherService.GetWeatherDataAsync(location);

				return Ok(weatherData);
			}
			catch (ArgumentException ex) when (ex.Message == "Bad API Request")
			{

				return BadRequest("Bad API Request");
			}
			catch(Exception ex)
			{
				return StatusCode(400, ex.Message);
			}
		}
	}
}
