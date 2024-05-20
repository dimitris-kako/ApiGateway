using Application.Models.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Movies
{
    public class MoviesSearchResponse
    {
        public List<Movie> Search{ get; set; }

        public int TotalResults { get; set; }

        public string Response { get; set; }
    }
}
