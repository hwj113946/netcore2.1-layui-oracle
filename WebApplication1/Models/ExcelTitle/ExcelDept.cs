namespace WebApplication1.Models
{
    public class ExcelDept
    {
        [Column(0)]
        public string CORP_NAME { get; set; }
        [Column(1)]
        public string DEPT_CODE { get; set; }
        [Column(2)]
        public string DEPT_NAME { get; set; }
        [Column(3)]
        public string STATUS { get; set; }
    }
}
