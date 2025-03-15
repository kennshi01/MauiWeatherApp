using System.Globalization;

namespace WeatherApp.Converters
{
	public class WeatherCodeToTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int weatherCode)
			{
				return weatherCode switch
				{
					0 => "Clear Sky ☀️",
					1 or 2 or 3 => "Partly Cloudy ⛅",
					45 or 48 => "Fog 🌫️",
					51 or 53 or 55 => "Drizzle 🌦️",
					61 or 63 or 65 => "Rain 🌧️",
					71 or 73 or 75 => "Snow ❄️",
					_ => "Unknown 🌍"
				};
			}
			return "Unknown";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
