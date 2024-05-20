using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Spotify
{
    public class SpotifyTrack
    {
        public string Name { get; set; }
        public List<SpotifyArtist> Artists { get; set; }
    }
}
