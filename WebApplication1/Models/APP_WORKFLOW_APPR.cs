using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class APP_WORKFLOW_APPR
    {
        [Column(0)] public long? APPR_ID { get; set; }//审批ID（Key）
        [Column(1)] public long? DOC_ID { get; set; }//文档ID（）
        [Column(2)] public long? APPR_FLOW_ID { get; set; }//工作流ID（）
        [Column(3)] public string HTTP { get; set; }//页面连接（）
        [Column(4)] public string DOC_TITLE { get; set; }//文档标题（）
        [Column(5)] public string DOC_NOTE { get; set; }//文档说明（）
        [Column(6)] public long? APPL_PERSON_ID { get; set; }//发布人（）
        [Column(7)] public long? APPL_DEPT_ID { get; set; }//发布单位（）
        [Column(8)] public DateTime? APPL_DATE { get; set; }//发布时间（）
        [Column(9)] public long? APPR_PERSON_ID { get; set; }//当前审批人（）
        [Column(10)] public string APPR_NOTE { get; set; }//当前审批人意见（）
        [Column(11)] public long? STATUS { get; set; }//状态（0-编辑；1-审批中；2-审批通过；3-退回）
        [Column(12)] public string ATTRIBUTE1 { get; set; }//属性1（备用）提交人Node_code
        [Column(13)] public string ATTRIBUTE2 { get; set; }//属性2（备用）提交人Node_type
        [Column(14)] public string ATTRIBUTE3 { get; set; }//属性3（备用）审批人Node_code
        [Column(15)] public string ATTRIBUTE4 { get; set; }//属性4（备用）审批人Node_type
        [Column(16)] public string ATTRIBUTE5 { get; set; }//属性5（备用）上一层审批code
        [Column(17)] public string ATTRIBUTE6 { get; set; }//属性6（备用）上一层审批flow_id
        [Column(18)] public string ATTRIBUTE7 { get; set; }//属性7（备用）当前审批节点flow_id
        [Column(19)] public string ATTRIBUTE8 { get; set; }//属性8（备用）接口进来
        [Column(20)] public string ATTRIBUTE9 { get; set; }//属性9（备用）
        [Column(21)] public string ATTRIBUTE10 { get; set; }//属性10（备用）
        [Column(22)] public string ATTRIBUTE11 { get; set; }//属性11（备用）
        [Column(23)] public string ATTRIBUTE12 { get; set; }//属性12（备用）
        [Column(24)] public string ATTRIBUTE13 { get; set; }//属性13（备用）
        [Column(25)] public string ATTRIBUTE14 { get; set; }//属性14（备用）
        [Column(26)] public string ATTRIBUTE15 { get; set; }//属性15（备用）
        [Column(27)] public DateTime? LAST_UPDATE_DATE { get; set; }//最后更改时间（）
        [Column(28)] public long? LAST_UPDATED_BY { get; set; }//最后更改人（）
        [Column(29)] public DateTime? CREATION_DATE { get; set; }//创建时间（）
        [Column(30)] public long? CREATED_BY { get; set; }//创建人（）

    }
}
