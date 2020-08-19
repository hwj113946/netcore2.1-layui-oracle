using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class ExcelSqzf
    {
        [Column(0)]
        public string FLOW_NO { get; set; }
        [Column(1)]
        public string FLOW_TYPE { get; set; }
        [Column(2)]
        public string PERSON_NAME { get; set; }
        [Column(3)]
        public string ID_CARD_NUMBER { get; set; }
        [Column(4)]
        public string PAY_AMT { get; set; }
    }
}
