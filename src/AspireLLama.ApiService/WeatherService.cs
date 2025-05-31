using System.Globalization;
using System.Text.Json.Serialization;

namespace AspireLLama.ApiService;

public class WeatherService(IHttpClientFactory clientFactory, string googleApiKey)
{
    private readonly System.Text.Json.JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<TemperatureResponse?> GetTemperature(string cityName)
    {
        var (CityName, Coordinates) = await GetGeoLocation(cityName) ?? throw new Exception("Geo Location not found");
        var (longitude, latitude) = Coordinates;

        var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude.ToString(CultureInfo.CreateSpecificCulture("en-US"))}&longitude={longitude.ToString(CultureInfo.CreateSpecificCulture("en-US"))}&hourly=temperature_2m";

        using var client = clientFactory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();

            var weatherData = System.Text.Json.JsonSerializer.Deserialize<Root>(data, options);

            if (weatherData?.hourly?.time is null)
            {
                return null;
            }

            var h = weatherData.hourly;
            var t = h?.time;
            var temps = new List<Temperature>();
            for (int i = 0; i < t?.Count; i++)
            {
                temps.Add(new(t[i], h?.temperature_2m[i]));
            }

            return new(CityName, temps.OrderBy(x => x.Time).Take(24).ToList());
        }

        throw new System.Exception("Unable to get temperatures");
    }

    private async Task<GeoLocationResponse?> GetGeoLocation(string cityName)
    {
        var apiUrl = $"https://maps.googleapis.com/maps/api/geocode/json?key={googleApiKey}&address={cityName}";

        using var client = clientFactory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.GetAsync(apiUrl);

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsStringAsync();

        var googleResponse = System.Text.Json.JsonSerializer.Deserialize<GoogleApiResponse>(data, options);

        if (googleResponse?.Results.Count > 0 && googleResponse.Results.First() is GoogleApiResponse.Result result)
        {
            if (result?.Geometry?.Location is GoogleApiResponse.Location location)
            {
                return new(result?.FormattedAddress ?? "", new(location.Longitude, location.Latitude));
            }
        }
        return null;
    }
}

public record TemperatureResponse(string CityName, IList<Temperature> Temperatures);
public record Temperature(string Time, double? TemperatureCelsius);
public record GeoLocationResponse(string CityName, GeoLocation GeoLocation);
public record GeoLocation(double Longitude, double Latitude);

internal class GoogleApiResponse
{
    public List<Result> Results { get; set; } = [];
    public class Result
    {
        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; } = "";
        public Geometry? Geometry { get; set; }
    }

    public class Geometry
    {
        public Location? Location { get; set; }
    }
    public class Location
    {
        [JsonPropertyName("lat")]
        public double Latitude { get; set; }
        [JsonPropertyName("lng")]
        public double Longitude { get; set; }
    }
}

public class Hourly
{
    public List<string> time { get; set; }
    public List<double> temperature_2m { get; set; }
}

public class HourlyUnits
{
    public string time { get; set; }
    public string temperature_2m { get; set; }
}

public class Root
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public double generationtime_ms { get; set; }
    public int utc_offset_seconds { get; set; }
    public string timezone { get; set; }
    public string timezone_abbreviation { get; set; }
    public double elevation { get; set; }
    public HourlyUnits hourly_units { get; set; }
    public Hourly hourly { get; set; }
}
