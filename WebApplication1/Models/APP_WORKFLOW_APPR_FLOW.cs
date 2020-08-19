using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class APP_WORKFLOW_APPR_FLOW
    {
        [Column(0)] public long? APPR_FLOW_ID { get; set; }//key
        [Column(1)] public string APPR_FLOW_NAME { get; set; }//工作流名称
        [Column(2)] public long? APPR_TYPE_ID { get; set; }//审批类型ID
        [Column(3)] public long? DEPT_ID { get; set; }//所属部门 公共为：-99
        [Column(4)] public string NOTE { get; set; }//
        [Column(5)] public string ATTRIBUTE1 { get; set; }//
        [Column(6)] public string ATTRIBUTE2 { get; set; }//
        [Column(7)] public string ATTRIBUTE3 { get; set; }//
        [Column(8)] public string ATTRIBUTE4 { get; set; }//
        [Column(9)] public string ATTRIBUTE5 { get; set; }//
        [Column(10)] public string ATTRIBUTE6 { get; set; }//
        [Column(11)] public string ATTRIBUTE7 { get; set; }//
        [Column(12)] public string ATTRIBUTE8 { get; set; }//
        [Column(13)] public string ATTRIBUTE9 { get; set; }//
        [Column(14)] public string ATTRIBUTE10 { get; set; }//
        [Column(15)] public string ATTRIBUTE11 { get; set; }//
        [Column(16)] public string ATTRIBUTE12 { get; set; }//
        [Column(17)] public string ATTRIBUTE13 { get; set; }//
        [Column(18)] public string ATTRIBUTE14 { get; set; }//
        [Column(19)] public string ATTRIBUTE15 { get; set; }//
        [Column(20)] public DateTime? LAST_UPDATE_DATE { get; set; }//
        [Column(21)] public long? LAST_UPDATED_BY { get; set; }//
        [Column(22)] public DateTime? CREATION_DATE { get; set; }//
        [Column(23)] public long? CREATED_BY { get; set; }//

    }
}
