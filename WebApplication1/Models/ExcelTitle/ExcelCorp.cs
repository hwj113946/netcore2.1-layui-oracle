using System;

namespace WebApplication1.Models
{
    public class ExcelCorp
    {
        [Column(0)]
        public string CORP_CODE { get; set; }
        [Column(1)]
        public string CORP_NAME { get; set; }
        [Column(2)]
        public string ATTRIBUTE1 { get; set; }
        [Column(3)]
        public string DETAILED_ADDRESS { get; set; }
        [Column(4)]
        public string LAW_PERSON_NAME { get; set; }
        [Column(5)]
        public string FAX { get; set; }
        [Column(6)]
        public string ZIP { get; set; }
        [Column(7)]
        public string TAX_RQ_NUMBER { get; set; }
        [Column(8)]
        public string E_MAIL { get; set; }      
        [Column(9)]
        public string NOTE { get; set; }
        
    }

    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(int index)
        {
            Index = index;
        }
        public int Index { get; set; }
    }
}
