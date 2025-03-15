using WeatherApp.Models.ViewModels;
using WeatherApp.Services;

namespace WeatherApp.Views;

public partial class TestWeatherPage : ContentPage
{
	public TestWeatherPage(IWeatherService weatherService)
	{
		InitializeComponent();
		BindingContext = new TestWeatherViewModel(weatherService);
	}
}