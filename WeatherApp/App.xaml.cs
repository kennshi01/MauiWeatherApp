using WeatherApp.Services;
using WeatherApp.Views;

namespace WeatherApp;

public partial class App : Application
{
	private readonly IWeatherService _weatherService;

	public App()
	{
		_weatherService = new OpenMeteoService(new HttpClient());
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// For testing purposes, TestPage
		//return new Window(new TestWeatherPage(_weatherService));
		return new Window(new MainPage(_weatherService));
	}
}