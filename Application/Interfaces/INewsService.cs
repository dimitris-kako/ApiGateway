using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models.Gateway;
using Application.Models.News;

namespace Application.Interfaces
{
    public interface INewsService
    {
        Task<(List<Article> Articles, int TotalResults)> SearchEverythingAsync(GatewayRequest request);

    }
}
