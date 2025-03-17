using WeatherApp.Services;
using WeatherApp.ViewModels;
using WeatherApp.Views;

namespace WeatherApp;

public partial class App : Application
{
	public App(IServiceProvider serviceProvider) // Inject DI container
	{
		InitializeComponent();

		// Get MainPage from DI (which automatically provides dependencies)
		MainPage = serviceProvider.GetRequiredService<MainPage>();
	}
}