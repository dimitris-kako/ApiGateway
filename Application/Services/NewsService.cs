using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models.Gateway;
using Application.Models.News;

namespace Application.Services
{
    public class NewsService : INewsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NewsService> _logger;
        private readonly IMemoryCache _cache;


        public NewsService(HttpClient httpClient, ILogger<NewsService> logger, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<(List<Article> Articles, int TotalResults)> SearchEverythingAsync(GatewayRequest request)
        {
            var cacheKey = $"{request.Query}_{request.NewsSortBy}_{request.NewsLanguage}_{request.NewsFromDateTime}_{request.NewsToDateTime}_{request.NewsPage}_{request.NewsPageSize}";

            if (!_cache.TryGetValue(cacheKey, out (List<Article> Articles, int TotalResults) cacheEntry))
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(request.Query))
                    queryParams.Add($"q={Uri.EscapeDataString(request.Query)}");
                if (!string.IsNullOrEmpty(request.NewsSortBy))
                    queryParams.Add($"sortBy={Uri.EscapeDataString(request.NewsSortBy)}");
                if (!string.IsNullOrEmpty(request.NewsLanguage))
                    queryParams.Add($"language={Uri.EscapeDataString(request.NewsLanguage)}");
                if (request.NewsFromDateTime.HasValue)
                    queryParams.Add($"from={request.NewsFromDateTime.Value:yyyy-MM-dd}");
                if (request.NewsToDateTime.HasValue)
                    queryParams.Add($"to={request.NewsToDateTime.Value:yyyy-MM-dd}");

                queryParams.Add($"page={request.NewsPage}");
                queryParams.Add($"pageSize={request.NewsPageSize}");

                var queryString = string.Join("&", queryParams);
                var url = $"everything?{queryString}";

                try
                {
                    var response = await _httpClient.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<NewsResponse>(content);

                    cacheEntry = (result.Articles, result.TotalResults);
                    _cache.Set(cacheKey, cacheEntry, TimeSpan.FromMinutes(10)); // Cache for 10 minutes
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Error ", ex);
                    return (null, 0);
                }
            }

            return cacheEntry;
        }
    }

}
