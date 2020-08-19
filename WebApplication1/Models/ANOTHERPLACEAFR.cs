using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class ANOTHERPLACEAFR
    {
        [Column(0)] public long? AFR_ID { get; set; }//
        [Column(1)] public string FLOW_NO { get; set; }//流程号
        [Column(2)] public long? DEAL_WITH_NUMBER { get; set; }//处理序号（序号）
        [Column(3)] public string FLOW_TYPE { get; set; }//流程类型（票据材料接收、韶钢医保办审核、市医保局复核、申请支付）
        [Column(4)] public string ID_CARD_NUMBER { get; set; }//身份证号码
        [Column(5)] public string PERSON_NAME { get; set; }//姓名
        [Column(6)] public DateTime? TRAN_DATE { get; set; }//处理时间
        [Column(7)] public string TRAN_PERSON { get; set; }//处理人
        [Column(8)] public string ACCESS_MSG { get; set; }//通过通知内容(短信通知，来源：公共代码)
        [Column(9)] public string FAIL_MSG { get; set; }//退回通知内容(短信通知，来源：公共代码)
        [Column(10)] public string FAIL_REASON { get; set; }//退回原因（手工录入）
        [Column(11)] public long? STATUS { get; set; }//状态（0-编辑；1-待补充材料；2-票据材料已接收；3-韶钢医保办待复核；4-韶钢医保办复核通过；5-韶钢医保办复核退回；6-医保局待复核；7-市医保局复核通过；8-市医保局退回；9-待申请支付；10-已申请支付；）
        [Column(12)] public string ATTRIBUTE1 { get; set; }//备用字段1（退回时填写）
        [Column(13)] public string ATTRIBUTE2 { get; set; }//备用字段2（接收时间：票据材料接收时填写）
        [Column(14)] public string ATTRIBUTE3 { get; set; }//备用字段3
        [Column(15)] public string ATTRIBUTE4 { get; set; }//备用字段4
        [Column(16)] public string ATTRIBUTE5 { get; set; }//备用字段5
        [Column(17)] public string ATTRIBUTE6 { get; set; }//备用字段6
        [Column(18)] public string ATTRIBUTE7 { get; set; }//备用字段7
        [Column(19)] public string ATTRIBUTE8 { get; set; }//备用字段8
        [Column(20)] public string ATTRIBUTE9 { get; set; }//备用字段9
        [Column(21)] public string ATTRIBUTE10 { get; set; }//备用字段10
        [Column(22)] public string ATTRIBUTE11 { get; set; }//备用字段11
        [Column(23)] public string ATTRIBUTE12 { get; set; }//备用字段12
        [Column(24)] public string ATTRIBUTE13 { get; set; }//备用字段13
        [Column(25)] public string ATTRIBUTE14 { get; set; }//备用字段14
        [Column(26)] public string ATTRIBUTE15 { get; set; }//备用字段15
        [Column(27)] public DateTime? LAST_UPDATE_DATE { get; set; }//最后更新时间
        [Column(28)] public long? LAST_UPDATE_BY { get; set; }//最后更新人
        [Column(29)] public DateTime? CREATION_DATE { get; set; }//创建时间
        [Column(30)] public long? CREATION_BY { get; set; }//创建人
        [Column(31)] public long? PERSON_ID { get; set; }//人员id
        [Column(32)] public decimal? PAY_AMT { get; set; }//支付金额
        [Column(33)] public string PHONE { get; set; }//手机号码
    }
}
