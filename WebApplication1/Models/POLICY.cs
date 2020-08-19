using System;

namespace WebApplication1.Models
{
    public class POLICY
    {
        [Column(0)] public long? POLICY_ID { get; set; }// 
        [Column(1)] public string TITLE { get; set; }//标题/名称 
        [Column(2)] public DateTime? UPLOAD_TIME { get; set; }//上传时间 
        [Column(3)] public string FILE_LINK { get; set; }//文件地址 
        [Column(4)] public string ATTRIBUTE1 { get; set; }//备用字段1（） 
        [Column(5)] public string ATTRIBUTE2 { get; set; }//备用字段2（） 
        [Column(6)] public string ATTRIBUTE3 { get; set; }//备用字段3 
        [Column(7)] public string ATTRIBUTE4 { get; set; }//备用字段4 
        [Column(8)] public string ATTRIBUTE5 { get; set; }//备用字段5 
        [Column(9)] public string ATTRIBUTE6 { get; set; }//备用字段6 
        [Column(10)] public string ATTRIBUTE7 { get; set; }//备用字段7 
        [Column(11)] public string ATTRIBUTE8 { get; set; }//备用字段8 
        [Column(12)] public string ATTRIBUTE9 { get; set; }//备用字段9 
        [Column(13)] public string ATTRIBUTE10 { get; set; }//备用字段10 
        [Column(14)] public string ATTRIBUTE11 { get; set; }//备用字段11 
        [Column(15)] public string ATTRIBUTE12 { get; set; }//备用字段12 
        [Column(16)] public string ATTRIBUTE13 { get; set; }//备用字段13 
        [Column(17)] public string ATTRIBUTE14 { get; set; }//备用字段14 
        [Column(18)] public string ATTRIBUTE15 { get; set; }//备用字段15 
        [Column(19)] public DateTime? LAST_UPDATE_DATE { get; set; }//最后更新时间 
        [Column(20)] public long? LAST_UPDATE_BY { get; set; }//最后更新人 
        [Column(21)] public DateTime? CREATION_DATE { get; set; }//创建时间 
        [Column(22)] public long? CREATION_BY { get; set; }//创建人 
    }
}
