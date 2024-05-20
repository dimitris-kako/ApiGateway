using Application.Models.Gateway;
using Application.Models.News;
using Application.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using WebApi.Settings;
using Xunit;

namespace UnitTests
{
    public class NewsServiceTests
    {
        private readonly Mock<ILogger<NewsService>> _loggerMock;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<IOptions<NewsApiSettings>> _settingsMock;
        private const string _apiKey = "api_key";

        public NewsServiceTests()
        {
            _loggerMock = new Mock<ILogger<NewsService>>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _settingsMock = new Mock<IOptions<NewsApiSettings>>();
            _settingsMock.Setup(s => s.Value)
                .Returns(new NewsApiSettings { ApiKey = _apiKey, BaseUrl = "https://newsapi.org/v2/" });
        }

        [Fact]
        public async Task SearchEverythingAsync_ReturnsArticles_WhenApiResponseIsSuccessful()
        {
            var request = new GatewayRequest
            {
                Query = "test",
                NewsSortBy = "relevancy",
                NewsLanguage = "en",
                NewsFromDateTime = DateTime.Now.AddDays(-1),
                NewsToDateTime = DateTime.Now,
                NewsPage = 1,
                NewsPageSize = 10
            };

            var expectedResponse = new NewsResponse
            {
                Articles = new List<Article>
                {
                    new Article { Title = "Test Article", Description = "Test Description", Url = "http://test.com" }
                },
                TotalResults = 1
            };

            var jsonExpectedResponse = JsonConvert.SerializeObject(expectedResponse);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonExpectedResponse),
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.news.com/")
            };

            var newsService = new NewsService(httpClient, _loggerMock.Object, _memoryCache);

            var result = await newsService.SearchEverythingAsync(request);

            Assert.Single(result.Articles);
            Assert.Equal("Test Article", result.Articles[0].Title);
            Assert.Equal(1, result.TotalResults);
        }

        [Fact]
        public async Task SearchEverythingAsync_ReturnsEmptyList_WhenApiResponseIsUnsuccessful()
        {
            // Arrange
            var request = new GatewayRequest
            {
                Query = "test",
                NewsSortBy = "relevancy",
                NewsLanguage = "en",
                NewsFromDateTime = DateTime.Now.AddDays(-1),
                NewsToDateTime = DateTime.Now,
                NewsPage = 1,
                NewsPageSize = 10
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.news.com/")
            };

            var newsService = new NewsService(httpClient, _loggerMock.Object, _memoryCache);

            var result = await newsService.SearchEverythingAsync(request);

            Assert.Empty(result.Articles);
            Assert.Equal(0, result.TotalResults);
        }

    }
}
