using Application.Models.Gateway;
using Application.Models.Movies;
using Application.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using Xunit;

namespace UnitTests
{
    public class MoviesServiceTests
    {
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly Mock<ILogger<MoviesService>> _mockLogger;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly MoviesService _moviesService;

        public MoviesServiceTests()
        {
            _mockHttpClient = new Mock<HttpClient>();
            _mockLogger = new Mock<ILogger<MoviesService>>();
            _mockCache = new Mock<IMemoryCache>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["MoviesApi:ApiKey"]).Returns("fake_api_key");

            _moviesService = new MoviesService(
                _mockHttpClient.Object,
                _mockLogger.Object,
                _mockCache.Object,
                _mockConfiguration.Object);
        }

        [Fact]
        public async Task SearchAsync_ReturnsMovies_FromCache()
        {
            // Arrange
            var request = new GatewayRequest
            {
                Query = "Batman",
                MoviesYear = "2021",
                MoviesPage = 1
            };

            var cacheKey = $"{request.Query}_{request.MoviesYear}_{request.MoviesPage}";

            var expectedMovies = new List<Movie> { new Movie { Title = "Batman Begins" } };
            var expectedTotalResults = 1;
            var cacheEntry = (expectedMovies, expectedTotalResults);

            _mockCache.Setup(c => c.TryGetValue(cacheKey, out cacheEntry)).Returns(true);

            // Act
            var result = await _moviesService.SearchAsync(request);

            // Assert
            Assert.Equal(expectedMovies, result.Movies);
            Assert.Equal(expectedTotalResults, result.TotalResults);
        }

        [Fact]
        public async Task SearchAsync_ReturnsMovies_WhenApiResponseIsSuccessful()
        {
            // Arrange
            var request = new GatewayRequest
            {
                Query = "Batman",
                MoviesYear = "2021",
                MoviesPage = 1
            };

            var cacheKey = $"{request.Query}_{request.MoviesYear}_{request.MoviesPage}";

            var expectedMovies = new List<Movie> { new Movie { Title = "Batman Begins" } };
            var expectedTotalResults = 1;

            var apiResponse = new MoviesSearchResponse
            {
                Response = "True",
                Search = expectedMovies,
                TotalResults = expectedTotalResults
            };

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(apiResponse))
                });

            var httpClient = new HttpClient(httpMessageHandler.Object);

            var service = new MoviesService(
                httpClient,
                _mockLogger.Object,
                _mockCache.Object,
                _mockConfiguration.Object);

            object cacheEntry;
            _mockCache.Setup(c => c.TryGetValue(cacheKey, out cacheEntry)).Returns(false);
            _mockCache.Setup(c => c.Set(cacheKey, It.IsAny<object>(), It.IsAny<TimeSpan>()));

            // Act
            var result = await service.SearchAsync(request);

            // Assert
            Assert.Equal(expectedMovies, result.Movies);
            Assert.Equal(expectedTotalResults, result.TotalResults);
        }

        [Fact]
        public async Task SearchAsync_ReturnsEmptyList_WhenApiResponseIsUnsuccessful()
        {
            // Arrange
            var request = new GatewayRequest
            {
                Query = "Batman",
                MoviesYear = "2021",
                MoviesPage = 1
            };

            var cacheKey = $"{request.Query}_{request.MoviesYear}_{request.MoviesPage}";

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var httpClient = new HttpClient(httpMessageHandler.Object);

            var service = new MoviesService(
                httpClient,
                _mockLogger.Object,
                _mockCache.Object,
                _mockConfiguration.Object);

            object cacheEntry;
            _mockCache.Setup(c => c.TryGetValue(cacheKey, out cacheEntry)).Returns(false);

            // Act
            var result = await service.SearchAsync(request);

            // Assert
            Assert.Empty(result.Movies);
            Assert.Equal(0, result.TotalResults);
        }

    }
}
