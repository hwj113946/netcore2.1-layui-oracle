using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models.ApiModels
{
    //票据材料接收参数
    public class pjcljs_param
    {
        public string PERSON_ID { get; set; }
        public string PERSON_NAME { get; set; }
        public string USER_ID { get; set; }//处理人id
        public string USER_NAME { get; set; }//处理人
        public string ATTRIBUTE2 { get; set; }//接收时间
        public string ID_CARD_NUMBER { get; set; }
        public string AFR_ID { get; set; }
        public string STATUS { get; set; }//票据状态
        public string FLOW_NO { get; set; }//流程号
        public string PHONE { get; set; }
        public int PAGE { get; set; }
        public int LIMIT { get; set; }
    }
}
