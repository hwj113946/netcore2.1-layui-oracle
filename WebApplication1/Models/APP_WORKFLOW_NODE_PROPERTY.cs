using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class APP_WORKFLOW_NODE_PROPERTY
    {
        [Column(0)] public long? APPR_FLOW_ID { get; set; }//类型ID（Key）
        [Column(1)] public string NODE_CODE { get; set; }//节点代码（）
        [Column(2)] public long? APPR_PERSON { get; set; }//审批人（）
        [Column(3)] public string APPR_POST { get; set; }//岗位（）
        [Column(4)] public string TYPE { get; set; }//类型（节点、线、环节）（）
        [Column(5)] public string RECT { get; set; }//跳转模块（）
        [Column(6)] public string VIEW_PAGE { get; set; }//查阅页面（）
        [Column(7)] public string ATTRIBUTE1 { get; set; }//属性1（备用）
        [Column(8)] public string ATTRIBUTE2 { get; set; }//属性2（备用）
        [Column(9)] public string ATTRIBUTE3 { get; set; }//属性3（备用）
        [Column(10)] public string ATTRIBUTE4 { get; set; }//属性4（备用）
        [Column(11)] public string ATTRIBUTE5 { get; set; }//属性5（备用）
        [Column(12)] public string ATTRIBUTE6 { get; set; }//属性6（备用）
        [Column(13)] public string ATTRIBUTE7 { get; set; }//属性7（备用）
        [Column(14)] public string ATTRIBUTE8 { get; set; }//属性8（备用）
        [Column(15)] public string ATTRIBUTE9 { get; set; }//属性9（备用）
        [Column(16)] public string ATTRIBUTE10 { get; set; }//属性10（备用）
        [Column(17)] public string ATTRIBUTE11 { get; set; }//属性11（备用）
        [Column(18)] public string ATTRIBUTE12 { get; set; }//属性12（备用）
        [Column(19)] public string ATTRIBUTE13 { get; set; }//属性13（备用）
        [Column(20)] public string ATTRIBUTE14 { get; set; }//属性14（备用）
        [Column(21)] public string ATTRIBUTE15 { get; set; }//属性15（备用）
        [Column(22)] public DateTime? LAST_UPDATE_DATE { get; set; }//最后更改时间（）
        [Column(23)] public long? LAST_UPDATED_BY { get; set; }//最后更改人（）
        [Column(24)] public DateTime? CREATION_DATE { get; set; }//创建时间（）
        [Column(25)] public long? CREATED_BY { get; set; }//创建人（）
        [Column(26)] public long? PRO_ID { get; set; }//序列ID（Key）
        [Column(27)] public long? DEPT_ID { get; set; }//部门
        [Column(28)] public long? TEAM_ID { get; set; }//组织
        [Column(29)] public string COL_SET { get; set; }//SQL获取人
        [Column(30)] public string PLSQLPRO { get; set; }//调用存储过程
        [Column(31)] public long? SFTJR { get; set; }//是否取提交人
        [Column(32)] public string DEPT_SQL { get; set; }//部门SQL定义
        [Column(33)] public string TEAM_SQL { get; set; }//科室班组SQL定义
        [Column(34)] public long? SFSJGW { get; set; }//是否上级岗位
        [Column(35)] public string POST_SQL { get; set; }//岗位SQL定义
        [Column(36)] public string PERSON_CODE { get; set; }//自定义工号组合
        [Column(37)] public string KQURL { get; set; }//
        [Column(38)] public string GJURL { get; set; }//
        [Column(39)] public string KQTJ { get; set; }//
        [Column(40)] public string GJTJ { get; set; }//
        [Column(41)] public string CDDM { get; set; }//

    }
}
