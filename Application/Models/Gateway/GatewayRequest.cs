using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Gateway
{
    public class GatewayRequest
    {
        public string Query { get; set; }
        public string? NewsSortBy { get; set; } = "relevancy";
        public string? NewsLanguage { get; set; } = "en";
        public DateTime? NewsFromDateTime { get; set; }
        public DateTime? NewsToDateTime { get; set; }

        public int? NewsPage { get; set; } = 1;
        public int? NewsPageSize { get; set; } = 10;

        public int? SpotifyPageSize { get; set; } = 10;

        public int? SpotifyOffset { get; set; } = 0;

        public int? MoviesPage { get; set; } = 1;

        public string? MoviesYear { get; set; } 
    }
}
