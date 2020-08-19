using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models.ApiModels
{
    public class Query_param
    {
        public string PERSON_ID { get; set; }
        public string STATUS { get; set; }
        public int LIMIT { get; set; }
        public int PAGE { get; set; }
        public string FLOW_NO { get; set; }
    }
}
