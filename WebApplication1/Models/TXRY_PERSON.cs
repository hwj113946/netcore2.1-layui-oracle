using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class TXRY_PERSON
    {
        [Column(0)] public long? PERSON_ID { get; set; }//主键
        [Column(1)] public string PERSON_NAME { get; set; }//姓名
        [Column(2)] public string SEX { get; set; }//性别
        [Column(3)] public long? AGE { get; set; }//年龄
        [Column(4)] public string PHONE { get; set; }//联系电话
        [Column(5)] public string ID_CARD_NUMBER { get; set; }//身份证号
        [Column(6)] public string NATIONAL { get; set; }//民族
        [Column(7)] public string POLITICAL_LANDSCAPE { get; set; }//政治面貌
        [Column(8)] public string LONG_TERM_RESIDENCE { get; set; }//长期居住地
        [Column(9)] public string DOMICILE_PLACE { get; set; }//户籍所在地
        [Column(10)] public string SPECIAL_PERSON { get; set; }//特殊人员及其说明
        [Column(11)] public string HEALTH { get; set; }//健康状况
        [Column(12)] public string E_I_ADDRESS { get; set; }//养老保险参保地
        [Column(13)] public string MEDICAL_I_ADDRESS { get; set; }//医疗保险参保地
        [Column(14)] public string IS_GSBX { get; set; }//是否还继续享受工伤保险待遇
        [Column(15)] public string LIVING_SITUATION { get; set; }//居住情况
        [Column(16)] public string SPOUSE_NAME { get; set; }//配偶姓名
        [Column(17)] public string SPOUSE_HEALTH { get; set; }//配偶健康状况
        [Column(18)] public string SPOUSE_PHONE { get; set; }//配偶联系电话
        [Column(19)] public string FAMILY_MAJOR_PERSON_NAME { get; set; }//家庭主要联系人姓名
        [Column(20)] public string FAMILY_MAJOR_P_RELATIONSHIP { get; set; }//家庭主要联系人与本人关系
        [Column(21)] public string FAMILY_MAJOR_PERSON_ADDRESS { get; set; }//家庭主要联系人地址
        [Column(22)] public string FAMILY_MAJOR_PERSON_PHONE { get; set; }//家庭主要联系人电话
        [Column(23)] public string REGISTERED_IMAGE_FIRST { get; set; }//户口本第一张照片链接地址
        [Column(24)] public string REGISTERED_IMAGE_SELF { get; set; }//户口本本人照片链接地址
        [Column(25)] public long? STATUS { get; set; }//状态(0-编辑；1-本人已核验；2-核验完成)
        [Column(26)] public string EMERGENCY_PERSON { get; set; }//紧急联系人
        [Column(27)] public string EMERGENCY_PHONE { get; set; }//紧急联系人电话
        [Column(28)] public string EMERGENCY_ADDRESS { get; set; }//紧急联系人地址
        [Column(29)] public string TRANSFER_TYPE { get; set; }//转交接收地：企业所在地、户籍所在地
        [Column(30)] public string OPEN_ID { get; set; }//微信小程序唯一标识
    }
}
