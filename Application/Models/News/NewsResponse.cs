using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.News
{
    public class NewsResponse
    {
        public List<Article> Articles { get; set; }

        public int TotalResults { get; set; }
    }
}
