namespace WeatherApp.Models;

public class HourlyForecastItem
{
	public string Time { get; set; } = string.Empty;
	public string Temperature { get; set; } = string.Empty;
	public string PrecipitationProbability { get; set; } = string.Empty;
	public string WindSpeed { get; set; } = string.Empty;
	public string WeatherText { get; set; } = string.Empty; // Emoji representation
}