using Application.Models.Spotify;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Models.Gateway;

namespace Application.Services
{
    public class SpotifyService : ISpotifyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SpotifyService> _logger;
        private readonly IMemoryCache _cache;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public SpotifyService(HttpClient httpClient,
            ILogger<SpotifyService> logger,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
            _clientId = configuration["Spotify:ClientId"];
            _clientSecret = configuration["Spotify:ClientSecret"];
        }

        public async Task<(List<SpotifyTrack> SpotifyTracks, int TotalResults)> SearchTracksAsync(GatewayRequest request)
        {
            var token = await GetAccessTokenAsync();

            var cacheKey = $"{request.Query}_{request.SpotifyOffset}_{request.SpotifyPageSize}";

            if (!_cache.TryGetValue(cacheKey, out (List<SpotifyTrack> Tracks, int TotalResults) cacheEntry))
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(request.Query))
                    queryParams.Add($"q={Uri.EscapeDataString(request.Query)}");

                queryParams.Add($"offset={request.SpotifyOffset}");
                queryParams.Add($"limit={request.SpotifyPageSize}");
                queryParams.Add($"type=track");

                var queryString = string.Join("&", queryParams);
                var url = $"?{queryString}";

                try
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<SpotifySearchResponse>(content);

                    var tracks = result.Tracks.Items;

                    cacheEntry = (tracks, result.Tracks.Total);
                    _cache.Set(cacheKey, cacheEntry, TimeSpan.FromMinutes(10)); // Cache for 10 minutes
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Error fetching data from Spotify API with URL: {Url}", url);
                    throw;
                }
            }

            return cacheEntry;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var cacheKey = "spotify_access_token";
            if (!_cache.TryGetValue(cacheKey, out string accessToken))
            {
                var requestBody = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret)
                });

                var response = await _httpClient.PostAsync("https://accounts.spotify.com/api/token", requestBody);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<SpotifyTokenResponse>(content);

                accessToken = tokenResponse.AccessToken;
                var expiresIn = tokenResponse.ExpiresIn;

                _cache.Set(cacheKey, accessToken, TimeSpan.FromSeconds(expiresIn - 60));
            }

            return accessToken;
        }
    }

}
