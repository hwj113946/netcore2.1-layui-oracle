using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class APP_WORKFLOW_APPR_NODES
    {
        [Column(0)] public long? ID { get; set; }//点ID（Key）
        [Column(1)] public long? APPR_FLOW_ID { get; set; }//类型ID（）
        [Column(2)] public string NODE_CODE { get; set; }//点代码（）
        [Column(3)] public string NODE_NAME { get; set; }//点名称（）
        [Column(4)] public string TYPE { get; set; }//类型（）
        [Column(5)] public string SHAPE { get; set; }//形状（）
        [Column(6)] public long? NUM { get; set; }//序列（）
        [Column(7)] public long? LEFT { get; set; }//左（）
        [Column(8)] public long? TOP { get; set; }//顶（）
        [Column(9)] public long? WIDTH { get; set; }//宽（）
        [Column(10)] public long? HEIGHT { get; set; }//高（）
        [Column(11)] public string PROPERTY { get; set; }//（）
        [Column(12)] public long? APPR_PERSON { get; set; }//审批人（）
        [Column(13)] public string APPR_POST { get; set; }//审批岗位（）
        [Column(14)] public string START_PRO { get; set; }//开始流程（）
        [Column(15)] public string ATTRIBUTE1 { get; set; }//属性1（备用）
        [Column(16)] public string ATTRIBUTE2 { get; set; }//属性2（备用）
        [Column(17)] public string ATTRIBUTE3 { get; set; }//属性3（备用）
        [Column(18)] public string ATTRIBUTE4 { get; set; }//属性4（备用）
        [Column(19)] public string ATTRIBUTE5 { get; set; }//属性5（备用）
        [Column(20)] public string ATTRIBUTE6 { get; set; }//属性6（备用）
        [Column(21)] public string ATTRIBUTE7 { get; set; }//属性7（备用）
        [Column(22)] public string ATTRIBUTE8 { get; set; }//属性8（备用）
        [Column(23)] public string ATTRIBUTE9 { get; set; }//属性9（备用）
        [Column(24)] public string ATTRIBUTE10 { get; set; }//属性10（备用）
        [Column(25)] public string ATTRIBUTE11 { get; set; }//属性11（备用）
        [Column(26)] public string ATTRIBUTE12 { get; set; }//属性12（备用）
        [Column(27)] public string ATTRIBUTE13 { get; set; }//属性13（备用）
        [Column(28)] public string ATTRIBUTE14 { get; set; }//属性14（备用）
        [Column(29)] public string ATTRIBUTE15 { get; set; }//属性15（备用）
        [Column(30)] public DateTime? LAST_UPDATE_DATE { get; set; }//最后更改时间（）
        [Column(31)] public long? LAST_UPDATED_BY { get; set; }//最后更改人（）
        [Column(32)] public DateTime? CREATION_DATE { get; set; }//创建时间（）
        [Column(33)] public long? CREATED_BY { get; set; }//创建人（）

    }
}
