using System;

namespace WebApplication1.Models
{
    public class NOTICE
    {
        [Column(0)] public long? NOTICE_ID { get; set; }// 
        [Column(1)] public string TITLE { get; set; }//标题 
        [Column(2)] public string MESSAGE { get; set; }//通知内容 
        [Column(3)] public DateTime? NOTICE_DATE { get; set; }//落款时间 
        [Column(4)] public string COMPANY { get; set; }//落款公司/人 
        [Column(5)] public string NICKNAME { get; set; }//昵称/尊称 
        [Column(6)] public long? STATUS { get; set; }//状态（0-编辑；1-发布；3-关闭；） 
        [Column(7)] public DateTime? LAST_UPDATE_DATE { get; set; }//最后更新时间 
        [Column(8)] public long? LAST_UPDATE_BY { get; set; }//最后更新人 
        [Column(9)] public DateTime? CREATION_DATE { get; set; }//创建时间 
        [Column(10)] public long? CREATION_BY { get; set; }//创建人 
    }
}
