using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class ExcelPost
    {
        [Column(0)]
        public string POST_CODE { get; set; }
        [Column(1)]
        public string POST_NAME { get; set; }
        [Column(2)]
        public string CORP_CODE { get; set; }
        [Column(3)]
        public string CORP_NAME { get; set; }
        [Column(4)]
        public string DEPT_CODE { get; set; }
        [Column(5)]
        public string DEPT_NAME { get; set; }
        [Column(6)]
        public string STATUS { get; set; }
    }
}
