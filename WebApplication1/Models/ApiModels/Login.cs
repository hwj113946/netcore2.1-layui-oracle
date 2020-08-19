using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models.ApiModels
{
    public class Login
    {
        public string USER_ID { get; set; }
        public string USER_CODE { get; set; }
        public string PASSWORD { get; set; }
        public string PERSON_ID { get; set; }
        public string PERSON_NAME { get; set; }
        public string ID_CARD_NUMBER { get; set; }
        public string APPID { get; set; }
        public string SECRET { get; set; }
        public string JS_CODE { get; set; }
        public string GRANT_TYPE { get; set; }
        public string OLD_PASSWORD { get; set; }
    }
}
