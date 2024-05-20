using Application.Interfaces;
using Application.Models.Gateway;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Responses;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GatewayController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly ISpotifyService _spotifyService;

        public GatewayController(INewsService newsService, SpotifyService spotifyService)
        {
            _newsService = newsService;
            _spotifyService = spotifyService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<GatewayResponse>> Search(
            [FromQuery] GatewayRequest request)
        {

            var newsTask = _newsService.SearchEverythingAsync(request);

            var tracksTask = _spotifyService.SearchTracksAsync(request);

            await Task.WhenAll(newsTask, tracksTask);

            var response = new GatewayResponse
            {
                Articles = newsTask.Result.Articles,
                TotalNewsResults = newsTask.Result.TotalResults,
                SpotifyTracks = tracksTask.Result.SpotifyTracks,
                TotalSpotifyResults = tracksTask.Result.TotalResults
            };

            return Ok(response);

        }

    }
}
