using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using WeatherApp.Models;
using WeatherApp.Models.ApiModels;
using WeatherApp.Services;

namespace WeatherApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IWeatherService _weatherService;

    [ObservableProperty]
    private string locationName;

    [ObservableProperty]
    private WeatherData? currentWeather;

    [ObservableProperty]
    private ForecastData? forecast;

    [ObservableProperty]
    private HourlyForecastData? hourlyForecast;

    [ObservableProperty]
    private bool isLoading;

	[ObservableProperty]
	private ObservableCollection<HourlyForecastItem> hourlyForecastList = new();

	// 5-Day forecast chart
	public ObservableCollection<ISeries> FiveDayForecastSeries { get; set; } = new();
    public ObservableCollection<Axis> FiveDayForecastXAxis { get; set; } = new();

    public MainViewModel(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

	[RelayCommand]
	public async Task LoadWeatherAsync()
	{
		try
		{
			IsLoading = true;

			// Clear previous data
			FiveDayForecastSeries.Clear();
			FiveDayForecastXAxis.Clear();

			// Request location permission
			var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
			if (status != PermissionStatus.Granted)
			{
				IsLoading = false;
				return;
			}

			// Get the user's current location (avoid unnecessary calls)
			var location = await Geolocation.GetLastKnownLocationAsync() ??
						   await Geolocation.GetLocationAsync(new GeolocationRequest
						   {
							   DesiredAccuracy = GeolocationAccuracy.Medium,
							   Timeout = TimeSpan.FromSeconds(30)
						   });

			if (location == null)
			{
				LocationName = "Location not available";
				IsLoading = false;
				return;
			}

			double latitude = location.Latitude;
			double longitude = location.Longitude;

			var placemarks = await Geocoding.GetPlacemarksAsync(latitude, longitude);
			var placemark = placemarks?.FirstOrDefault();

			if (placemark != null)
			{
				var city = placemark.Locality ?? "Unknown City";
				var region = placemark.AdminArea ?? "Unknown Region";

				// Run UI update on the main thread to prevent crashes
				MainThread.BeginInvokeOnMainThread(() =>
				{
					LocationName = $"{city}, {region}";
				});
			}
			else
			{
				LocationName = "Unknown Location";
			}

			// Fetch weather data asynchronously
			var currentWeatherTask = _weatherService.GetCurrentWeatherAsync(latitude, longitude);
			var forecastTask = _weatherService.GetWeatherForecastAsync(latitude, longitude);
			var hourlyForecastTask = _weatherService.GetHourlyForecastAsync(latitude, longitude);

			await Task.WhenAll(currentWeatherTask, forecastTask, hourlyForecastTask);

			// Assign results after all tasks are completed
			CurrentWeather = currentWeatherTask.Result;
			Forecast = forecastTask.Result;
			HourlyForecast = hourlyForecastTask.Result;

			// Populate forecast data
			PopulateHourlyForecastList();
			PopulateFiveDayForecastChart();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error fetching weather data: {ex.Message}");
			LocationName = "Error loading weather";
		}
		finally
		{
			IsLoading = false; // Ensure loading state is reset
		}
	}

	/// <summary>
	/// Populates Hourly Forecast List
	/// </summary>
	private void PopulateHourlyForecastList()
	{
		if (HourlyForecast?.Hourly == null) return;

		HourlyForecastList.Clear();

		DateTime now = DateTime.UtcNow; // Get current UTC time

		for (int i = 0; i <= 24; i += 3) // Every 3rd hour
		{
			DateTime forecastTime = DateTime.Parse(HourlyForecast.Hourly.Time[i], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

			if (forecastTime < now)
				continue; // Skip past hours ✅

			var temp = $"{HourlyForecast.Hourly.Temperature2m[i]}°C";
			var precipitation = $"{HourlyForecast.Hourly.PrecipitationProbability[i]}%";
			var windSpeed = $"{HourlyForecast.Hourly.Windspeed10m[i]} m/s";
			var weatherText = ConvertWeatherCode(HourlyForecast.Hourly.WeatherCode[i]);

			HourlyForecastList.Add(new HourlyForecastItem
			{
				Time = forecastTime.ToString("HH:mm"), // Keep format consistent
				Temperature = temp,
				PrecipitationProbability = precipitation,
				WindSpeed = windSpeed,
				WeatherText = weatherText
			});
		}
	}

	private string ConvertWeatherCode(int weatherCode)
	{
		return weatherCode switch
		{
			0 => "Clear Sky ☀️",
			1 or 2 or 3 => "Partly Cloudy ⛅",
			45 or 48 => "Fog 🌫️",
			51 or 53 or 55 => "Drizzle 🌦️",
			61 or 63 or 65 => "Rain 🌧️",
			71 or 73 or 75 => "Snow ❄️",
			_ => "Unknown 🌍"
		};
	}

	/// <summary>
	/// Populates the 5-day max/min temperature trend chart.
	/// </summary>
	private void PopulateFiveDayForecastChart()
	{
		if (Forecast?.Daily == null) return;

		var days = Forecast.Daily.Time.Take(5)
					  .Select(date => DateTime.Parse(date).ToString("MMM dd"))
					  .ToArray();
		var maxTemperatures = Forecast.Daily.Temperature2mMax.Take(5).Select(t => (double)t).ToArray();
		var minTemperatures = Forecast.Daily.Temperature2mMin.Take(5).Select(t => (double)t).ToArray();

		// Max Temperature Bar Series (Smaller & Softer Color)
		var maxTempSeries = new ColumnSeries<double>
		{
			Values = maxTemperatures,
			Name = "Max Temp",
			Stroke = new SolidColorPaint(new SKColor(255, 140, 0)) { StrokeThickness = 0.8f },  // Softer orange
			Fill = new SolidColorPaint(new SKColor(255, 165, 0, 180)),  // Muted orange with transparency
			Rx = 3,  // Reduced rounding for better fit
			Ry = 3
		};

		// Min Temperature Smooth Line Series (Thinner & Less Intense White)
		var minTempSeries = new LineSeries<double>
		{
			Values = minTemperatures,
			Name = "Min Temp",
			Stroke = new SolidColorPaint(new SKColor(200, 200, 200)) { StrokeThickness = 0.8f },  // Softer white
			GeometrySize = 4,  // Smaller points for subtle look
			GeometryFill = new SolidColorPaint(new SKColor(220, 220, 220)),  // Light gray
			GeometryStroke = new SolidColorPaint(new SKColor(220, 220, 220)),
			LineSmoothness = 0.8  // Slight curve but not too smooth
		};

		// Clear and update the series
		FiveDayForecastSeries.Clear();
		FiveDayForecastSeries.Add(maxTempSeries);
		FiveDayForecastSeries.Add(minTempSeries);

		FiveDayForecastXAxis.Clear();
		FiveDayForecastXAxis.Add(new Axis
		{
			Labels = days,
			TextSize = 10,  // Smaller labels
			Padding = new LiveChartsCore.Drawing.Padding(3)
		});
	}
}
