using System;

namespace WebApplication1.Models
{
public class APP_CORP_BANK
{
[Column(0)] public long? CORP_BANK_ID {get;set;}//银行ID（Key） 
[Column(1)] public long? CORP_ID {get;set;}//公司ID（） 
[Column(2)] public string BANK_PROVINCE {get;set;}//开户行省份（） 
[Column(3)] public string BANK_CITY {get;set;}//开户行城市（） 
[Column(4)] public string BANK_NAME {get;set;}//开户银行（） 
[Column(5)] public string BANK_ACCOUNT {get;set;}//银行帐号（） 
[Column(6)] public string BANK_NO {get;set;}//行号（） 
[Column(7)] public DateTime? START_DATE {get;set;}//有效开始时间（） 
[Column(8)] public DateTime? END_DATE {get;set;}//有效结束时间（） 
[Column(9)] public long? STATUS {get;set;}//状态（0-编辑；1-确定；3-取消） 
[Column(10)] public string NOTE {get;set;}//备注（） 
[Column(11)] public string ATTRIBUTE1 {get;set;}//属性1（） 
[Column(12)] public string ATTRIBUTE2 {get;set;}//属性2（） 
[Column(13)] public string ATTRIBUTE3 {get;set;}//属性3（备用） 
[Column(14)] public string ATTRIBUTE4 {get;set;}//属性4（备用） 
[Column(15)] public string ATTRIBUTE5 {get;set;}//属性5（备用） 
[Column(16)] public string ATTRIBUTE6 {get;set;}//属性6（备用） 
[Column(17)] public string ATTRIBUTE7 {get;set;}//属性7（备用） 
[Column(18)] public string ATTRIBUTE8 {get;set;}//属性8（备用） 
[Column(19)] public string ATTRIBUTE9 {get;set;}//属性9（备用） 
[Column(20)] public string ATTRIBUTE10 {get;set;}//属性10（备用） 
[Column(21)] public string ATTRIBUTE11 {get;set;}//属性11（备用） 
[Column(22)] public string ATTRIBUTE12 {get;set;}//属性12（备用） 
[Column(23)] public string ATTRIBUTE13 {get;set;}//属性13（备用） 
[Column(24)] public string ATTRIBUTE14 {get;set;}//属性14（备用） 
[Column(25)] public string ATTRIBUTE15 {get;set;}//属性15（备用） 
[Column(26)] public DateTime? LAST_UPDATE_DATE {get;set;}//最后更改时间（） 
[Column(27)] public long? LAST_UPDATED_BY {get;set;}//最后更改人（） 
[Column(28)] public DateTime? CREATION_DATE {get;set;}//创建时间（） 
[Column(29)] public long? CREATED_BY {get;set;}//创建人（） 
}
}
