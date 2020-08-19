using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class ExcelPjcljs
    {
        [Column(0)]
        public string PERSON_NAME { get; set; }
        [Column(1)]
        public string ID_CARD_NUMBER { get; set; }
        [Column(2)]
        public string PHONE { get; set; }
        [Column(3)]
        public string ATTRIBUTE2 { get; set; }
    }
}
