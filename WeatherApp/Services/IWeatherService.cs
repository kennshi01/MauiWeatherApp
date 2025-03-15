using WeatherApp.Models.ApiModels;

namespace WeatherApp.Services;

public interface IWeatherService
{
	Task<WeatherData?> GetCurrentWeatherAsync(double latitude, double longitude);
	Task<ForecastData?> GetWeatherForecastAsync(double latitude, double longitude);
}
