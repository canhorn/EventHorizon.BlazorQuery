namespace EventHorizon.BlazorQuery.Web.Weather;

using EventHorizon.BlazorQuery.Web.Components;

public class WeatherForecastApi(WeatherApiClient weatherApi)
{
    public readonly BlazorDataQuery<string, List<WeatherForecast>> GetWeather =
        new(async (_) => await weatherApi.GetWeatherAsync());

    public readonly BlazorDataQuery<string, List<WeatherForecast>> GetWeatherLongLoading =
        new(
            async (arg) =>
            {
                await Task.Delay(5000);
                return await weatherApi.GetWeatherAsync();
            }
        );

    public readonly BlazorDataQuery<string, List<WeatherForecast>> GetWeatherFailed =
        new(
            async (arg) =>
            {
                await Task.Delay(1000);
                throw new Exception("Failed to get weather");
            }
        );
}
