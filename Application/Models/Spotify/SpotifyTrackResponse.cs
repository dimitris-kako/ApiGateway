using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Spotify
{
    public class SpotifyTrackResponse
    {
        public List<SpotifyTrack> Items { get; set; }

        public int Total { get; set; }
    }
}
