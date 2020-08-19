using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class ExcelTxryhd
    {
        [Column(0)]
        public string PERSON_NAME { get; set; }
        [Column(1)]
        public string SEX { get; set; }
        [Column(2)]
        public decimal AGE { get; set; }
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
        [Column(9)]
        public string SPECIAL_PERSON { get; set; }
        [Column(10)]
        public string HEALTH { get; set; }
        [Column(11)]
        public string E_I_ADDRESS { get; set; }
        [Column(12)]
        public string MEDICAL_I_ADDRESS { get; set; }
        [Column(13)]
        public string IS_GSBX { get; set; }
        [Column(14)]
        public string LIVING_SITUATION { get; set; }
        [Column(15)]
        public string SPOUSE_NAME { get; set; }
        [Column(16)]
        public string SPOUSE_HEALTH { get; set; }
        [Column(17)]
        public string SPOUSE_PHONE { get; set; }
        [Column(18)]
        public string FAMILY_MAJOR_PERSON_NAME { get; set; }
        [Column(19)]
        public string FAMILY_MAJOR_P_RELATIONSHIP { get; set; }
        [Column(20)]
        public string FAMILY_MAJOR_PERSON_ADDRESS { get; set; }
        [Column(21)]
        public string FAMILY_MAJOR_PERSON_PHONE { get; set; }        
        [Column(22)]
        public string EMERGENCY_PERSON { get; set; }
        [Column(23)]
        public string EMERGENCY_PHONE { get; set; }
        [Column(24)]
        public string EMERGENCY_ADDRESS { get; set; }
        [Column(25)]
        public string TRANSFER_TYPE { get; set; }
        [Column(26)]
        public string STATUS { get; set; }
    }
}
