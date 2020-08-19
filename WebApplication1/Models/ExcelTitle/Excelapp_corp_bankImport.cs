using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Excelapp_corp_bankImport
    {
        [Column(0)] public string CORP_NAME { get; set; }//公司名称
        [Column(1)] public string BANK_PROVINCE { get; set; }//开户行省份（） 
        [Column(2)] public string BANK_CITY { get; set; }//开户行城市（） 
        [Column(3)] public string BANK_NAME { get; set; }//开户银行（） 
        [Column(4)] public string BANK_ACCOUNT { get; set; }//银行帐号（） 
        [Column(5)] public string BANK_NO { get; set; }//行号（） 
        [Column(6)] public string START_DATE { get; set; }//有效开始时间（） 
        [Column(7)] public string END_DATE { get; set; }//有效结束时间（） 
        [Column(8)] public string NOTE { get; set; }//备注
    }
}
