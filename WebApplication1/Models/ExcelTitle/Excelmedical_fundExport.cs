using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Excelmedical_fundExport
    {
        [Column(0)] public string PERSON_NAME { get; set; }//姓名 
        [Column(1)] public string ID_CARD_NUMBER { get; set; }//身份证号码 
        [Column(2)] public string PHONE { get; set; }//手机号码 
        [Column(3)] public string BANK_NAME { get; set; }//开户银行 
        [Column(4)] public string BANK_ACCOUNT { get; set; }//银行账号 
        [Column(5)] public string MOHTN_AMT { get; set; }//月度（元） 
        [Column(6)] public string FUND_AMT { get; set; }//医疗备用金（元） 
        [Column(7)] public string NOTE { get; set; }//备注 
        [Column(8)] public string CREATION_DATE { get; set; }//创建时间 
        [Column(9)] public string CREATED_BY { get; set; }//创建人 
    }
}
