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

            // Get the user's current location
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
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
                LocationName = $"{placemark.Locality ?? "Unknown City"}";
            }
            else
            {
                LocationName = "Unknown Location";
            }

            CurrentWeather = await _weatherService.GetCurrentWeatherAsync(latitude, longitude);
            Forecast = await _weatherService.GetWeatherForecastAsync(latitude, longitude);
            HourlyForecast = await _weatherService.GetHourlyForecastAsync(latitude, longitude);

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
            IsLoading = false;
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

        int count = 0;
        for (int i = 0; i < HourlyForecast.Hourly.Time.Length; i++) // Iterate all forecast hours
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
                Time = forecastTime.ToString("HH:mm"),
                Temperature = temp,
                PrecipitationProbability = precipitation,
                WindSpeed = windSpeed,
                WeatherText = weatherText
            });

            count++;
            if (count == 24) break;
        }
    }

    private string ConvertWeatherCode(int weatherCode)
	{
		return weatherCode switch
		{
			0 => "☀️",
			1 or 2 or 3 => "⛅",
			45 or 48 => "🌫️",
			51 or 53 or 55 => "🌦️",
			61 or 63 or 65 => "🌧️",
			71 or 73 or 75 => "❄️",
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

        var maxTempSeries = new ColumnSeries<double>
        {
            Values = maxTemperatures,
            Name = "Max Temp",
            Stroke = new SolidColorPaint(new SKColor(255, 69, 0)) { StrokeThickness = 1f },  // Red-Orange Stroke
            Fill = new SolidColorPaint(new SKColor(255, 99, 71, 180)),  // Tomato Red with Transparency
            Rx = 3,  // Rounded edges for bar effect
            Ry = 3
        };

        // Min Temperature Line Series (Cool Color: Blue)
        var minTempSeries = new LineSeries<double>
        {
            Values = minTemperatures,
            Name = "Min Temp",
            Stroke = new SolidColorPaint(new SKColor(30, 144, 255)) { StrokeThickness = 1f },  // Dodger Blue Stroke
            GeometrySize = 4,  // Small points for subtle look
            GeometryFill = new SolidColorPaint(new SKColor(70, 130, 180)),  // Steel Blue Fill
            GeometryStroke = new SolidColorPaint(new SKColor(70, 130, 180)),
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
