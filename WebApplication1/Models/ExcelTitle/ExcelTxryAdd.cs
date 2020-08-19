using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class ExcelTxryAdd
    {
        [Column(0)]
        public string PERSON_NAME { get; set; }
        [Column(1)]
        public string SEX { get; set; }
        [Column(2)]
        public string AGE { get; set; }
        [Column(3)]
        public string PHONE { get; set; }
        [Column(4)]
        public string ID_CARD_NUMBER { get; set; }
        [Column(5)]
        public string NATIONAL { get; set; }
        [Column(6)]
        public string POLITICAL_LANDSCAPE { get; set; }
        [Column(7)]
        public string LONG_TERM_RESIDENCE { get; set; }
        [Column(8)]
        public string DOMICILE_PLACE { get; set; }
    }
}
