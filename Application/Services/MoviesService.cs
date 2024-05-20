using Application.Models.Gateway;
using Application.Models.News;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Models.Movies;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class MoviesService : IMoviesService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MoviesService> _logger;
        private readonly IMemoryCache _cache;
        private readonly string _apiKey;


        public MoviesService(HttpClient httpClient, 
            ILogger<MoviesService> logger,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
            _apiKey = configuration["MoviesApi:ApiKey"];
        }

        public async Task<(List<Movie> Movies, int TotalResults)> SearchAsync(GatewayRequest request)
        {
            var cacheKey = $"{request.Query}_{request.MoviesYear}_{request.MoviesPage}";

            if (!_cache.TryGetValue(cacheKey, out (List<Movie> Movies, int TotalResults) cacheEntry))
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(request.Query))
                    queryParams.Add($"s={Uri.EscapeDataString(request.Query)}");
                if (!string.IsNullOrEmpty(request.MoviesYear))
                    queryParams.Add($"y={Uri.EscapeDataString(request.MoviesYear)}");

                queryParams.Add($"page={request.MoviesPage}");
                queryParams.Add($"apiKey={_apiKey}");

                var queryString = string.Join("&", queryParams);
                var url = $"?{queryString}";

                try
                {
                    var response = await _httpClient.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<MoviesSearchResponse>(content);

                    if (result.Response != "True")
                    {
                        _logger.LogWarning("Movie API returned a failure response: {Response}", result.Response);
                        return (new List<Movie>(), 0);
                    }

                    cacheEntry = (result.Search, result.TotalResults);
                    _cache.Set(cacheKey, cacheEntry, TimeSpan.FromMinutes(10)); // Cache for 10 minutes
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Error fetching data from Movie API with URL: {Url}", url);
                    return (new List<Movie>(), 0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "General error occurred.");
                    return (new List<Movie>(), 0);
                }
            }

            return cacheEntry;
        }
    }
}
