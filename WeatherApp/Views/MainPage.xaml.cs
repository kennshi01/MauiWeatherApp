using WeatherApp.Services;
using WeatherApp.ViewModels;

namespace WeatherApp.Views;

public partial class MainPage : ContentPage
{
	public MainPage(IWeatherService weatherService)
	{
		InitializeComponent();
		BindingContext = new MainViewModel(weatherService);
	}
}