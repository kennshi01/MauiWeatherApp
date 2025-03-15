using System.Text.Json;
using WeatherApp.Models.ApiModels;

namespace WeatherApp.Services;

public class OpenMeteoService : IWeatherService
{
	private readonly HttpClient _httpClient;
	private const string BASE_URL = "https://api.open-meteo.com/v1/forecast";

	public OpenMeteoService(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<WeatherData?> GetCurrentWeatherAsync(double latitude, double longitude)
	{
		try
		{
			string url = $"{BASE_URL}?latitude={latitude}&longitude={longitude}&current_weather=true";
			var response = await _httpClient.GetStringAsync(url);

			if (string.IsNullOrEmpty(response))
				return null;

			var weatherData = JsonSerializer.Deserialize<WeatherData>(response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});

			return weatherData;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error fetching current weather: {ex.Message}");
			return null;
		}
	}

	public async Task<ForecastData?> GetWeatherForecastAsync(double latitude, double longitude)
	{
		try
		{
			string url = $"{BASE_URL}?latitude={latitude}&longitude={longitude}&daily=temperature_2m_max,temperature_2m_min&timezone=auto";
			var response = await _httpClient.GetStringAsync(url);

			if (string.IsNullOrEmpty(response))
				return null;

			var forecastData = JsonSerializer.Deserialize<ForecastData>(response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});

			return forecastData;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error fetching forecast: {ex.Message}");
			return null;
		}
	}

	public async Task<HourlyForecastData?> GetHourlyForecastAsync(double latitude, double longitude)
	{
		try
		{
			string url = $"{BASE_URL}?latitude={latitude}&longitude={longitude}&hourly=temperature_2m&timezone=auto";
			var response = await _httpClient.GetStringAsync(url);

			if (string.IsNullOrEmpty(response))
				return null;

			var hourlyForecastData = JsonSerializer.Deserialize<HourlyForecastData>(response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});

			return hourlyForecastData;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error fetching hourly forecast: {ex.Message}");
			return null;
		}
	}
}
