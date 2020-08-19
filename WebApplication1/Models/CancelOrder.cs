using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class CancelOrder
    {
        public string ACTION { get; set; }
        public string PERSON_ID { get; set; }
        public string PERSON_CODE { get; set; }
        public string ORDER_ID { get; set; }
    }
}
