using Application.Models.Gateway;
using Application.Models.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models.Movies;

namespace Application.Interfaces
{
    public interface IMoviesService
    {
        Task<(List<Movie> Movies, int TotalResults)> SearchAsync(GatewayRequest request);
    }
}
