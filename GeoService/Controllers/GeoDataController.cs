using Dadata;
using Dadata.Model;
using GeoService.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class GeoDataController : ControllerBase
{
    private readonly ILogger<GeoDataController> _logger;
    private readonly HttpClient _httpClient;

    public GeoDataController(ILogger<GeoDataController> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;

    }

    [HttpGet("address")]
    public async Task<IActionResult> GetGeoDataByAddress(string country, string city, string street)
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "GeoService/1.0");

        _logger.LogInformation($"Get addresses method started:");

        try
        {

            var response = await _httpClient.GetFromJsonAsync<GeoData[]>($"https://nominatim.openstreetmap.org/search?country={country}&city={city}&street={street}&format=json&limit=2");


            if (response == null || response.Length == 0)
            {
                _logger.LogError("Address not found");
                return NotFound(new { Error = "Address not found" });
            }

            _logger.LogInformation($"lat = {response[0].Lat?.ToString()} \n lon = {response[0].Lon?.ToString()}");
            return Ok(response[0]);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching geo data");
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }


    [HttpPost("geoposition")]
    public async Task<IActionResult> GetAddressesByGeoPosition(double latitude, double longitude)
    {
        string token = "701f667677b4245a7d218f7cb411cb1d813b43ee";

        var api = new SuggestClientAsync(token);


        _logger.LogInformation($"Post geoposition method started:");

        try
        {
            var response = await api.Geolocate(lat: latitude, lon: longitude, count: 10);
            

            if (response == null || response.suggestions.Count == 0)
            {
                _logger.LogError("Addresses not found");
                return NotFound(new { Error = "Addresses not found" });
            }

            var address = MapperResponseToAddress(response);

            _logger.LogInformation(address.ToString());
            return Ok(address);
        
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching addresses");
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    private List<GeoService.Models.Address> MapperResponseToAddress(SuggestResponse<Dadata.Model.Address> response)
    {
        List<GeoService.Models.Address> address = new List<GeoService.Models.Address>();

        foreach (var item in response.suggestions)
        {
            address.Add(new GeoService.Models.Address()
            {
                Region = item.data.region_with_type,
                City = item.data.city_with_type,
                Street = item.data.street_with_type,
                House = item.data.house,
                Block = item.data.block
            });
        }

        return address;
    }
}
