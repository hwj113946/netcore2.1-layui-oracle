using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models.ApiModels
{
    public class Notice_param
    {
        public string SEARCH_TEXT { get; set; }
        public int PAGE { get; set; }
        public int LIMIT { get; set; }
    }
}
