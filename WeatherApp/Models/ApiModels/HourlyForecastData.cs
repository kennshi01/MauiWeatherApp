using System.Text.Json.Serialization;

namespace WeatherApp.Models.ApiModels;
public class HourlyForecastData
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

	[JsonPropertyName("hourly_units")]
	public HourlyUnits HourlyUnits { get; set; }

	[JsonPropertyName("hourly")]
	public Hourly Hourly { get; set; }
}

public class HourlyUnits
{
	[JsonPropertyName("time")]
	public string Time { get; set; }

	[JsonPropertyName("temperature_2m")]
	public string Temperature2m { get; set; }

	[JsonPropertyName("precipitation_probability")]
	public string PrecipitationProbability { get; set; }

	[JsonPropertyName("windspeed_10m")]
	public string Windspeed10m { get; set; }

	[JsonPropertyName("weathercode")]
	public string WeatherCode { get; set; }
}

public class Hourly
{
	[JsonPropertyName("time")]
	public string[] Time { get; set; }

	[JsonPropertyName("temperature_2m")]
	public float[] Temperature2m { get; set; }

	[JsonPropertyName("precipitation_probability")]
	public float[] PrecipitationProbability { get; set; }

	[JsonPropertyName("windspeed_10m")]
	public float[] Windspeed10m { get; set; }

	[JsonPropertyName("weathercode")]
	public int[] WeatherCode { get; set; }
}

