using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
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

    // Hourly temperature trend chart
    public ObservableCollection<ISeries> TemperatureChartSeries { get; set; } = new();
    public ObservableCollection<Axis> TemperatureChartXAxis { get; set; } = new();

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
		IsLoading = true;

		// Clear previous data
		TemperatureChartSeries.Clear();
		TemperatureChartXAxis.Clear();
		FiveDayForecastSeries.Clear();
		FiveDayForecastXAxis.Clear();

		// Request location permission
		var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
		if (status != PermissionStatus.Granted)
		{
			// Handle permission not granted
			IsLoading = false;
			return;
		}

		// Get the user's current location
		var location = await Geolocation.GetLastKnownLocationAsync();
		if (location == null)
		{
			location = await Geolocation.GetLocationAsync(new GeolocationRequest
			{
				DesiredAccuracy = GeolocationAccuracy.Medium,
				Timeout = TimeSpan.FromSeconds(30)
			});
		}

		if (location == null)
		{
			// Handle location not available
			IsLoading = false;
			return;
		}

		double latitude = location.Latitude;
		double longitude = location.Longitude;

		// Perform reverse geocoding to get the location name
		var placemarks = await Geocoding.GetPlacemarksAsync(latitude, longitude);
		var placemark = placemarks?.FirstOrDefault();
		if (placemark != null)
		{
			LocationName = $"{placemark.Locality}, {placemark.AdminArea}";
		}
		else
		{
			LocationName = "Unknown Location";
		}

		CurrentWeather = await _weatherService.GetCurrentWeatherAsync(latitude, longitude);
		Forecast = await _weatherService.GetWeatherForecastAsync(latitude, longitude);
		HourlyForecast = await _weatherService.GetHourlyForecastAsync(latitude, longitude);

		PopulateTemperatureChart();
		PopulateFiveDayForecastChart();

		IsLoading = false;
	}


	/// <summary>
	/// Populates the 24-hour temperature trend chart.
	/// </summary>
	private void PopulateTemperatureChart()
    {
        if (HourlyForecast != null && HourlyForecast.Hourly != null)
        {
            var times = HourlyForecast.Hourly.Time.Take(24).ToArray();
            var temperatures = HourlyForecast.Hourly.Temperature2m.Take(24).Select(t => (double)t).ToList();

            var temperatureSeries = new LineSeries<double>
            {
                Values = temperatures,
                Fill = null,  // No fill below the line
                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 3 },
                GeometrySize = 10
            };

            TemperatureChartSeries.Add(temperatureSeries);

            // Set X-axis labels to hourly timestamps (formatted as HH:mm)
            TemperatureChartXAxis.Add(new Axis
            {
                Labels = times.Select(t => DateTime.Parse(t).ToString("HH:mm")).ToArray(),
                LabelsRotation = 45,
                TextSize = 12,
                Padding = new LiveChartsCore.Drawing.Padding(5)
            });
        }
    }

    /// <summary>
    /// Populates the 5-day max/min temperature trend chart.
    /// </summary>
    private void PopulateFiveDayForecastChart()
    {
        if (Forecast != null && Forecast.Daily != null)
        {
            var days = Forecast.Daily.Time.Take(5).Select(date => DateTime.Parse(date).ToString("MMM dd")).ToArray();
            var maxTemperatures = Forecast.Daily.Temperature2mMax.Take(5).Select(t => (double)t).ToArray();
            var minTemperatures = Forecast.Daily.Temperature2mMin.Take(5).Select(t => (double)t).ToArray();

            var maxTempSeries = new ColumnSeries<double>
            {
                Values = maxTemperatures,
                Name = "Max Temp",
                Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 2 },
                Fill = new SolidColorPaint(SKColors.Red.WithAlpha(100))
            };

            var minTempSeries = new ColumnSeries<double>
            {
                Values = minTemperatures,
                Name = "Min Temp",
                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
                Fill = new SolidColorPaint(SKColors.Blue.WithAlpha(100))
            };

            FiveDayForecastSeries.Add(maxTempSeries);
            FiveDayForecastSeries.Add(minTempSeries);

            FiveDayForecastXAxis.Add(new Axis
            {
                Labels = days,
                TextSize = 12
            });
        }
    }
}
