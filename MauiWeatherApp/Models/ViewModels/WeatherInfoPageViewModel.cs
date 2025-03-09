using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiWeatherApp.Models.ViewModels;

internal partial class WeatherInfoPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string latitude;

    [ObservableProperty]
    private string longitude;

    [ObservableProperty]
    private string weatherIcon;

    [ObservableProperty]
    private string temperature;

    [ObservableProperty]
    private string weatherDescription;

    [ObservableProperty]
    private string location;

    [ObservableProperty]
    private string humidity;

    [ObservableProperty]
    private string cloudCoverLevel;

    [ObservableProperty]
    private string isDay;

    [RelayCommand]
    private async Task FetchWeatherInformationAsync()
    {

    }
}
