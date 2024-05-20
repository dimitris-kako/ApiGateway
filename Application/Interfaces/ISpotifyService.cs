using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models.Gateway;
using Application.Models.Spotify;

namespace Application.Interfaces
{
    public interface ISpotifyService
    {
        Task<(List<SpotifyTrack> SpotifyTracks, int TotalResults)> SearchTracksAsync(GatewayRequest request);

    }
}
