using System.Text.Json.Serialization;

namespace WeatherApp.Models.ApiModels;

public class WeatherData
{
	[JsonPropertyName("latitude")]
	public float Latitude { get; set; }

	[JsonPropertyName("longitude")]
	public float Longitude { get; set; }

	[JsonPropertyName("generationtime_ms")]
	public float GenerationTimeMs { get; set; }

	[JsonPropertyName("utc_offset_seconds")]
	public int UtcOffsetSeconds { get; set; }

	[JsonPropertyName("timezone")]
	public string Timezone { get; set; }

	[JsonPropertyName("timezone_abbreviation")]
	public string TimezoneAbbreviation { get; set; }

	[JsonPropertyName("elevation")]
	public float Elevation { get; set; }

	[JsonPropertyName("current_weather_units")]
	public CurrentWeatherUnits CurrentWeatherUnits { get; set; }

	[JsonPropertyName("current_weather")]
	public CurrentWeather CurrentWeather { get; set; }
}

public class CurrentWeatherUnits
{
	[JsonPropertyName("time")]
	public string Time { get; set; }

	[JsonPropertyName("interval")]
	public string Interval { get; set; }

	[JsonPropertyName("temperature")]
	public string Temperature { get; set; }

	[JsonPropertyName("windspeed")]
	public string WindSpeed { get; set; }

	[JsonPropertyName("winddirection")]
	public string WindDirection { get; set; }

	[JsonPropertyName("is_day")]
	public string IsDay { get; set; }

	[JsonPropertyName("weathercode")]
	public string WeatherCode { get; set; }
}

public class CurrentWeather
{
	[JsonPropertyName("time")]
	public string Time { get; set; }

	[JsonPropertyName("interval")]
	public int Interval { get; set; }

	[JsonPropertyName("temperature")]
	public float Temperature { get; set; }

	[JsonPropertyName("windspeed")]
	public float WindSpeed { get; set; }

	[JsonPropertyName("winddirection")]
	public int WindDirection { get; set; }

	[JsonPropertyName("is_day")]
	public int IsDay { get; set; }

	[JsonPropertyName("weathercode")]
	public int WeatherCode { get; set; }
}