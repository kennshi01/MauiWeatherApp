using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WeatherApp.Models.ApiModels;
using WeatherApp.Services;

namespace WeatherApp.Models.ViewModels;

public partial class TestWeatherViewModel : ObservableObject
{
	private readonly IWeatherService _weatherService;

	[ObservableProperty]
	private WeatherData? currentWeather;

	[ObservableProperty]
	private ForecastData? forecast;

	[ObservableProperty]
	private bool isLoading;

	public ObservableCollection<string> ForecastDetails { get; } = new();

	public TestWeatherViewModel(IWeatherService weatherService)
	{
		_weatherService = weatherService;
	}

	[RelayCommand]
	public async Task LoadWeatherAsync()
	{
		IsLoading = true;
		ForecastDetails.Clear();

		double latitude = 47.0;  // Example coordinates (Moldova)
		double longitude = 28.8;

		CurrentWeather = await _weatherService.GetCurrentWeatherAsync(latitude, longitude);
		Forecast = await _weatherService.GetWeatherForecastAsync(latitude, longitude);

		if (Forecast != null && Forecast.Daily != null)
		{
			for (int i = 0; i < Forecast.Daily.Time.Length; i++)
			{
				ForecastDetails.Add($"{Forecast.Daily.Time[i]}: Max {Forecast.Daily.Temperature2mMax[i]}°C, Min {Forecast.Daily.Temperature2mMin[i]}°C");
			}
		}

		IsLoading = false;
	}
}
