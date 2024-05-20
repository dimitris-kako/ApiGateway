﻿
using Application.Models.News;
using Application.Models.Spotify;

namespace WebApi.Responses
{
    public class GatewayResponse
    {
        public List<Article> Articles { get; set; }
        public int TotalNewsResults { get; set; }
        public List<SpotifyTrack> SpotifyTracks { get; set; }
        public int TotalSpotifyResults { get; set; }
    }
}
