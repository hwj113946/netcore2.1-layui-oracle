using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using WebApplication1.Models;

namespace WebApplication1.Controllers.WorkFlow
{
    public class WorkFlowController : Controller
    {
        DBHelper data = new DBHelper();
        DataBaseContext context;        
        public WorkFlowController(DataBaseContext context)
        {
            this.context = context;
        }

        [CheckCustomer]
        public IActionResult WorkFlowType()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult WorkFlowTypeEdit()
        {
            ViewBag.zt = HttpContext.Request.Query["zt"];
            if (ViewBag.zt == "update")
            {
                ViewBag.id = HttpContext.Request.Query["id"];
                ViewBag.code = HttpContext.Request.Query["code"];
                ViewBag.name = HttpContext.Request.Query["name"];
                ViewBag.attribute3 = HttpContext.Request.Query["attribute3"];
                ViewBag.start_status = HttpContext.Request.Query["start_status"];
                ViewBag.end_status = HttpContext.Request.Query["end_status"];
                ViewBag.cancel_status = HttpContext.Request.Query["cancel_status"];
                ViewBag.note = HttpContext.Request.Query["note"];
                ViewBag.status = HttpContext.Request.Query["status"];
                ViewBag.attribute9 = HttpContext.Request.Query["attribute9"];
                ViewBag.attribute2 = HttpContext.Request.Query["attribute2"];
                ViewBag.attribute10 = HttpContext.Request.Query["attribute10"];
                ViewBag.attribute11 = HttpContext.Request.Query["attribute11"];
                ViewBag.attribute4 = HttpContext.Request.Query["attribute4"];
                ViewBag.attribute6 = HttpContext.Request.Query["attribute6"];
                ViewBag.attribute1 = HttpContext.Request.Query["attribute1"];
            }
            return View();
        }

        [CheckCustomer]
        public IActionResult WorkFlowFlow()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult WorkFlowFlowEdit()
        {
            ViewBag.zt = HttpContext.Request.Query["zt"];
            if (ViewBag.zt == "update")
            {
                ViewBag.id = HttpContext.Request.Query["id"];
                ViewBag.note = HttpContext.Request.Query["note"];
                ViewBag.name = HttpContext.Request.Query["name"];
                ViewBag.typeid = HttpContext.Request.Query["typeid"];
            }
            return View();
        }

        [CheckCustomer]
        public IActionResult WorkFlow_Appr_Setting()
        {
            ViewBag.flowid = HttpContext.Request.Query["id"];
            return View();
        }
        [CheckCustomer]
        public IActionResult WorkFlowNodeUpdate()
        {
            ViewBag.flowid = HttpContext.Request.Query["flowid"];
            ViewBag.node_code = HttpContext.Request.Query["node_code"];
            ViewBag.node_name = HttpContext.Request.Query["node_name"];
            ViewBag.dept_id = HttpContext.Session.GetString("DEPT_ID");
            return View();
        }

        [CheckCustomer]
        public IActionResult WorkFlowLineUpdate()
        {
            ViewBag.flowid = HttpContext.Request.Query["flowid"];
            ViewBag.line_code = HttpContext.Request.Query["line_code"];
            return View();
        }

        [CheckCustomer]
        public IActionResult WorkFlowAppr()
        {
            ViewBag.docid = HttpContext.Request.Query["Doc_id"];
            ViewBag.apprid = HttpContext.Request.Query["apprid"];
            ViewBag.status = HttpContext.Request.Query["status"];
            ViewBag.type = HttpContext.Request.Query["type"];
            ViewBag.des = HttpContext.Request.Query["des"];
            ViewBag.name = HttpContext.Request.Query["name"];
            return View();
        }

        [CheckCustomer]
        public IActionResult Appr()
        {
            return View();
        }

        #region 获取审批流类型
        [HttpPost]
        public async Task<IActionResult> GetWorkFlowType(int limit, int page, string flowvalue)
        {
            string Msg = "";
            DataSet ds = new DataSet();
            string sql = @"SELECT * FROM (SELECT ROWNUM AS rowno, r.*
                                     FROM (SELECT APPR_TYPE_ID,
                                             APPR_TYPE_CODE,
                                             APPR_TYPE_NAME,
                                             APPR_START_STATUS,
                                             APPR_END_STATUS,
                                             APPR_CANCEL_STATUS,
                                             NOTE,
                                             STATUS,
                                             ATTRIBUTE1,
                                             ATTRIBUTE2,
                                             ATTRIBUTE3,
                                             ATTRIBUTE4,
                                             ATTRIBUTE6,
                                             ATTRIBUTE9,
                                             ATTRIBUTE10,
                                             ATTRIBUTE11
                                                 FROM APP_WORKFLOW_APPR_TYPE
                        where (APPR_TYPE_CODE like '%'|| :flowvalue ||'%' or APPR_TYPE_NAME like '%'|| :flowvalue1 ||'%')
                        ORDER BY APPR_TYPE_ID ) r
                                    where ROWNUM <= :page * :limit) table_alias
                            WHERE table_alias.rowno > (:page - 1) * :limit";
            OracleParameter[] param = { data.MakeInParam(":flowvalue", flowvalue??""),
                data.MakeInParam(":flowvalue1", flowvalue??""),
                                        data.MakeInParam(":page", page),
                                        data.MakeInParam(":limit", limit)
                                      };
            ds = await data.GetDataSetByParam(sql, param);
            string sql2 = @"select count(*) FROM APP_WORKFLOW_APPR_TYPE
                        where (APPR_TYPE_CODE like '%'|| :flowvalue ||'%' or APPR_TYPE_NAME like '%'|| :flowvalue ||'%')
                        ORDER BY APPR_TYPE_ID";
            OracleParameter[] param2 = { data.MakeInParam(":flowvalue", flowvalue ?? ""), data.MakeInParam(":flowvalue1", flowvalue ?? ""), };
            string count = await data.GetStringByParam(sql2, param2);
            if (ds.Tables[0].Rows.Count > 0)
            {
                Msg = "{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}";
            }
            else
            {
                Msg = "{\"code\":0,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            }
            return Content(Msg);
        }
        #endregion

        #region 删除审批流类型
        [HttpPost]
        public async Task<IActionResult> RemoveWorkFlowType(string id)
        {
            string Msg = "";
            string sql = @"delete from APP_WORKFLOW_APPR_TYPE where APPR_TYPE_ID=:id ";
            OracleParameter[] param = { data.MakeInParam(":id", id) };
            bool flag = await data.DoSqlByParam(sql, param);
            Msg = flag ? "{\"code\":200,\"msg\":\"删除成功\"}" : "{\"code\":300,\"msg\":\"删除失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 新增审批流类型
        [HttpPost]
        public async Task<IActionResult> WorkFlowTypeInsert(string code, string name, string attribute3, string start_status, string end_status, string cancel_status,
            string note, string attribute9, string attribute2, string attribute10, string attribute11, string attribute4, string attribute6, string attribute1)
        {
            string Msg = "";
            string sql = @"INSERT INTO APP_WORKFLOW_APPR_TYPE
                                              (APPR_TYPE_ID,
                                               APPR_TYPE_CODE,
                                               APPR_TYPE_NAME,
                                               APPR_START_STATUS,
                                               APPR_END_STATUS,
                                               APPR_CANCEL_STATUS,dept_id,
                                               NOTE,
                                               CREATION_DATE,
                                               CREATED_BY,
                                               ATTRIBUTE1,
                                               ATTRIBUTE2,
                                               ATTRIBUTE3,
                                               ATTRIBUTE4,
                                               ATTRIBUTE6,
                                               ATTRIBUTE9,
                                               ATTRIBUTE10,
                                               ATTRIBUTE11,
                                               status)
                                            VALUES
                                              (APP_WORKFLOW_APPR_TYPE_S.NEXTVAL,
                                               :APPR_TYPE_CODE,
                                               :APPR_TYPE_NAME,
                                               :APPR_START_STATUS,
                                               :APPR_END_STATUS,
                                               :APPR_CANCEL_STATUS,'-99',
                                               :NOTE,
                                               sysdate,
                                               :userid,
                                               :ATTRIBUTE1,
                                               :ATTRIBUTE2,
                                               :ATTRIBUTE3,
                                               :ATTRIBUTE4,
                                               :ATTRIBUTE6,
                                               :ATTRIBUTE9,
                                               :ATTRIBUTE10,
                                               :ATTRIBUTE11,
                                               1)";
            OracleParameter[] param = { data.MakeInParam(":APPR_TYPE_CODE", code??""),
                                        data.MakeInParam(":APPR_TYPE_NAME",name??"" ),
                                        data.MakeInParam(":APPR_START_STATUS",start_status??"" ),
                                        data.MakeInParam(":APPR_END_STATUS",end_status??"" ),
                                        data.MakeInParam(":APPR_CANCEL_STATUS",cancel_status??"" ),
                                        data.MakeInParam(":NOTE",note??"" ),
                                        data.MakeInParam(":userid",HttpContext.Session.GetString("USER_ID")),
                                        data.MakeInParam(":ATTRIBUTE1",attribute1??"" ),
                                        data.MakeInParam(":ATTRIBUTE2",attribute2??""),
                                        data.MakeInParam(":ATTRIBUTE3",attribute3??""),
                                        data.MakeInParam(":ATTRIBUTE4",attribute4??""),
                                        data.MakeInParam(":ATTRIBUTE6",attribute6??""),
                                        data.MakeInParam(":ATTRIBUTE9",attribute9??""),
                                        data.MakeInParam(":ATTRIBUTE10",attribute10??"" ),
                                        data.MakeInParam(":ATTRIBUTE11",attribute11??"" )};
            Msg = await data.DoSqlByParam(sql, param) ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 编辑审批流类型
        [HttpPost]
        public async Task<IActionResult> WorkFlowTypeUpdate(string id, string code, string name, string attribute3, string start_status, string end_status, string cancel_status,
            string note, string attribute9, string attribute2, string attribute10, string attribute11, string attribute4, string attribute6, string attribute1)
        {
            string Msg = "";
            string sql = @"UPDATE APP_WORKFLOW_APPR_TYPE
                               SET APPR_TYPE_CODE     = :APPR_TYPE_CODE,
                                   APPR_TYPE_NAME     = :APPR_TYPE_NAME,
                                   APPR_START_STATUS  = :APPR_START_STATUS,
                                   APPR_END_STATUS    = :APPR_END_STATUS,
                                   APPR_CANCEL_STATUS = :APPR_CANCEL_STATUS,
                                   NOTE               = :NOTE,
                                   LAST_UPDATE_DATE   = sysdate,
                                   LAST_UPDATED_BY    = :userid,
                                   ATTRIBUTE1         = :ATTRIBUTE1,
                                   ATTRIBUTE2         = :ATTRIBUTE2,
                                   ATTRIBUTE3         = :ATTRIBUTE3,
                                   ATTRIBUTE4         = :ATTRIBUTE4,
                                   ATTRIBUTE6         = :ATTRIBUTE6,
                                   ATTRIBUTE9         = :ATTRIBUTE9,
                                   ATTRIBUTE10        = :ATTRIBUTE10,
                                   ATTRIBUTE11        = :ATTRIBUTE11
                             WHERE APPR_TYPE_ID = :APPR_TYPE_ID";
            OracleParameter[] param = { data.MakeInParam(":APPR_TYPE_CODE", code??""),
                                        data.MakeInParam(":APPR_TYPE_NAME",name ??""),
                                        data.MakeInParam(":APPR_START_STATUS",start_status??"" ),
                                        data.MakeInParam(":APPR_END_STATUS",end_status??"" ),
                                        data.MakeInParam(":APPR_CANCEL_STATUS",cancel_status??"" ),
                                        data.MakeInParam(":NOTE",note??"" ),
                                         data.MakeInParam(":userid",HttpContext.Session.GetString("USER_ID") ),
                                        data.MakeInParam(":ATTRIBUTE1",attribute1??""),
                                        data.MakeInParam(":ATTRIBUTE2",attribute2??""),
                                        data.MakeInParam(":ATTRIBUTE3",attribute3??""),
                                        data.MakeInParam(":ATTRIBUTE4",attribute4??""),
                                        data.MakeInParam(":ATTRIBUTE6",attribute6??""),
                                        data.MakeInParam(":ATTRIBUTE9",attribute9??""),
                                        data.MakeInParam(":ATTRIBUTE10",attribute10??"" ),
                                        data.MakeInParam(":ATTRIBUTE11",attribute11??"" ),
                                        data.MakeInParam(":APPR_TYPE_ID",id??"" )
                                       };
            Msg = await data.DoSqlByParam(sql, param) ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 审批流设置：获取所有审批流
        [HttpPost]
        public async Task<IActionResult> GetWorkFlow_Flow(int page, int limit, string flowvalue)
        {
            string Msg = "";
            DataSet ds = new DataSet();
            string sql = @"SELECT * FROM (SELECT ROWNUM AS rowno, r.*
                                     FROM (SELECT f.APPR_FLOW_ID,
                                           f.APPR_FLOW_NAME,
                                           f.APPR_TYPE_ID,
                                           f.NOTE,
                                           t.appr_type_name
                                      FROM APP_WORKFLOW_APPR_FLOW f, app_workflow_appr_type t
                                     where f.appr_type_id = t.appr_type_id
                                       and f.appr_flow_name like '%' || :name || '%' ) r
                                    where ROWNUM <= :page * :limit) table_alias
                            WHERE table_alias.rowno > (:page - 1) * :limit";
            OracleParameter[] param = { data.MakeInParam(":name", flowvalue??""),
                                        data.MakeInParam(":page", page),
                                        data.MakeInParam(":limit", limit)
                                      };
            ds = await data.GetDataSetByParam(sql, param);
            string sql2 = @"select count(*)  FROM APP_WORKFLOW_APPR_FLOW f, app_workflow_appr_type t
                                     where f.appr_type_id = t.appr_type_id
                                       and f.appr_flow_name like '%' || :name || '%'";
            OracleParameter[] param2 = { data.MakeInParam(":name", flowvalue ?? "") };
            string count = await data.GetStringByParam(sql2, param2);
            if (ds.Tables[0].Rows.Count > 0)
            {
                Msg = "{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}";
            }
            else
            {
                Msg = "{\"code\":0,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            }
            return Content(Msg);
        }
        #endregion

        #region 新增审批流
        [HttpPost]
        public async Task<IActionResult> WorkFlow_FlowInsert(string name, string typeid, string note)
        {
            string Msg = "";
            string sql = @"insert into app_workflow_appr_flow
                                          (appr_flow_id,
                                           appr_flow_name,
                                           appr_type_id,dept_id,
                                           note,
                                           last_update_date,
                                           last_updated_by,
                                           creation_date,
                                           created_by)
                                        values
                                          (app_workflow_appr_flow_s.nextval,
                                           :appr_flow_name,
                                           :appr_type_id,'-99',
                                           :note,
                                           sysdate,
                                           :userid,
                                           sysdate,
                                           :userid)";
            OracleParameter[] param = { data.MakeInParam(":appr_flow_name",name??"" ),
                                        data.MakeInParam(":appr_type_id",typeid??"" ),
                                        data.MakeInParam(":note",note??"" ),
                                        data.MakeInParam(":userid",HttpContext.Session.GetString("USER_ID") )};
            Msg = await data.DoSqlByParam(sql, param) ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 编辑审批流
        [HttpPost]
        public async Task<IActionResult> WorkFlow_FlowUpdate(string id, string name, string typeid, string note)
        {
            string Msg = "";
            string sql = @"update app_workflow_appr_flow
                                   set appr_flow_name   = :appr_flow_name,
                                       appr_type_id     = :appr_type_id,
                                       note             = :note,
                                       last_update_date = sysdate,
                                       last_updated_by  = :userid
                                 where appr_flow_id = :id";
            OracleParameter[] param = { data.MakeInParam(":appr_flow_name",name??"" ),
                                        data.MakeInParam(":appr_type_id",typeid??"" ),
                                        data.MakeInParam(":note",note??"" ),
                                        data.MakeInParam(":userid",HttpContext.Session.GetString("USER_ID")),
                                        data.MakeInParam(":id",id )};
            Msg = await data.DoSqlByParam(sql, param) ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 删除审批流
        [HttpPost]
        public async Task<IActionResult> RemoveWorkFlow_FLow(long? id)
        {
            string Msg = "";
            using (var tran = context.Database.BeginTransaction())
            {
                try
                {
                    var appr = context.APP_WORKFLOW_APPR_FLOW.ToList().Where(u => u.APPR_FLOW_ID == id).ToList();
                    var nodes = context.APP_WORKFLOW_APPR_NODES.ToList().Where(u => u.APPR_FLOW_ID == id).ToList();
                    var lines = context.APP_WORKFLOW_APPR_LINES.ToList().Where(u => u.APPR_FLOW_ID == id).ToList();
                    var lp = context.APP_WORKFLOW_LINE_PROPERTY.ToList().Where(u => u.APPR_FLOW_ID == id).ToList();
                    var np = context.APP_WORKFLOW_NODE_PROPERTY.ToList().Where(u => u.APPR_FLOW_ID == id).ToList();
                    if (appr.Count > 0)
                    {
                        context.APP_WORKFLOW_APPR_FLOW.RemoveRange(appr);
                    }
                    if (nodes.Count > 0)
                    {
                        context.APP_WORKFLOW_APPR_NODES.RemoveRange(nodes);
                    }
                    if (lines.Count > 0)
                    {
                        context.APP_WORKFLOW_APPR_LINES.RemoveRange(lines);
                    }
                    if (lp.Count > 0)
                    {
                        context.APP_WORKFLOW_LINE_PROPERTY.RemoveRange(lp);
                    }
                    if (np.Count > 0)
                    {
                        context.APP_WORKFLOW_NODE_PROPERTY.RemoveRange(np);
                    }
                    await context.SaveChangesAsync();
                    tran.Commit();
                    Msg = "{\"code\":200,\"msg\":\"删除成功\"}";
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Msg = "{\"code\":300,\"msg\":\"删除失败，请联系管理员\"}";
                }
            }
            return Content(Msg);
        }
        #endregion

        #region 获取审批流类型:有效
        [HttpPost]
        public async Task<IActionResult> GetWorkFlowTypes(string flowvalue)
        {
            string Msg = "";
            DataSet ds = new DataSet();
            string sql = @"SELECT APPR_TYPE_ID, APPR_TYPE_CODE, APPR_TYPE_NAME FROM APP_WORKFLOW_APPR_TYPE where status=1";
            ds = await data.GetDataSet(sql);
            Msg = ds.Tables[0].Rows.Count > 0
                ? "{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}"
                : "{\"code\":0,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 通过flowid获取nodes
        public async Task<DataSet> get_nodes(string flowid)
        {
            string sql = "select * from app_WORKFLOW_APPR_NODES where APPR_flow_ID=:flowid";
            OracleParameter[] param = { data.MakeInParam(":flowid", flowid) };
            DataSet ds = await data.GetDataSetByParam(sql, param);
            return ds;
        }
        #endregion

        #region 通过flowid获取lines
        public async Task<DataSet> get_lines(string flowid)
        {
            string sql = "select * from app_WORKFLOW_APPR_LINES where APPR_flow_ID=:flowid";
            OracleParameter[] param = { data.MakeInParam(":flowid", flowid) };
            DataSet ds = await data.GetDataSetByParam(sql, param);
            return ds;
        }
        #endregion

        #region 通过flowid获取最大序列
        public async Task<Int32> get_max_seq(string flowid)
        {
            OracleParameter[] param = { data.MakeInParam(":flowid", flowid) };
            string sql = "select nvl(max(num),0) from app_workflow_appr_nodes t where t.APPR_flow_ID=:flowid";
            Int32 node = Convert.ToInt32(await data.GetStringByParam(sql, param));
            string sql1 = "select nvl(max(num),0) from app_workflow_appr_lines t where t.APPR_flow_ID=:flowid";
            Int32 line = Convert.ToInt32(await data.GetStringByParam(sql1, param));
            return node > line ? node : line;
        }
        #endregion

        #region 通过flowid获取流名称
        public async Task<string> get_flowName(string flowid)
        {
            string sql = "select APPR_FLOW_NAME from APP_WORKFLOW_APPR_FLOW where APPR_flow_ID=:flowid";
            OracleParameter[] param = { data.MakeInParam(":flowid", flowid) };
            string ds = await data.GetStringByParam(sql, param);
            return ds;
        }
        #endregion

        #region 获取流json
        [HttpPost]
        public async Task<IActionResult> GetWorkFlowJson(string flowid)
        {
            string Msg = "";
            DataSet ds = new DataSet();
            DataSet nodes = await get_nodes(flowid);
            DataSet lines = await get_lines(flowid);
            string flowname = await get_flowName(flowid);
            int n = await get_max_seq(flowid);
            if (n == 0)
            {
                Msg = "{\"title\":\"\",\"nodes\":{},\"lines\":{},\"areas\":{},\"initNum\":0}";
                return Content(Msg);
            }
            string node_json = "{\"title\":\"" + flowname + "\",\"nodes\":{";
            string type = "";
            for (int i = 0; i < nodes.Tables[0].Rows.Count; i++)
            {
                if (nodes.Tables[0].Rows[i]["TYPE"].ToString() == "start")
                {
                    type = "start round";
                }
                if (nodes.Tables[0].Rows[i]["TYPE"].ToString() == "node")
                {
                    type = "task round";
                }
                if (nodes.Tables[0].Rows[i]["TYPE"].ToString() == "end")
                {
                    type = "end round";
                }
                node_json += "\"" + nodes.Tables[0].Rows[i]["NODE_CODE"] + "\":{\"name\":\"" + nodes.Tables[0].Rows[i]["NODE_NAME"] + "\",\"left\":" + nodes.Tables[0].Rows[i]["LEFT"]
                    + ",\"top\":" + nodes.Tables[0].Rows[i]["TOP"] + ",\"type\":\"" + type + "\",\"width\":" + nodes.Tables[0].Rows[i]["WIDTH"] + ",\"height\":" + nodes.Tables[0].Rows[i]["HEIGHT"] + "},";
            }
            node_json = node_json.Remove(node_json.Length - 1, 1) + "},\"lines\":{";
            for (int i = 0; i < lines.Tables[0].Rows.Count; i++)
            {
                node_json += "\"" + lines.Tables[0].Rows[i]["LINE_CODE"] + "\":{\"type\":\"sl\",\"from\":\"" + lines.Tables[0].Rows[i]["FROM1"] + "\",\"to\":\"" + lines.Tables[0].Rows[i]["TO1"]
                    + "\",\"name\":\"" + lines.Tables[0].Rows[i]["LINE_NAME"] + "\"},";
            }
            node_json = node_json.Remove(node_json.Length - 1, 1) + "},\"areas\":{},\"initNum\":" + (Convert.ToInt32(nodes.Tables[0].Rows.Count) + Convert.ToInt32(lines.Tables[0].Rows.Count)) + "}";

            return Content(node_json);
        }
        #endregion

        #region 保存流
        [HttpPost]
        public async Task<IActionResult> SaveWorkFlow(string flowid, string json)
        {
            string Msg = "";
            JObject j = JObject.Parse(json);
            JToken jn = (JToken)j["nodes"];
            JToken jl = (JToken)j["lines"];
            List<Models.WorkFlow.Node> nodes = new List<Models.WorkFlow.Node>();
            List<Models.WorkFlow.Line> lines = new List<Models.WorkFlow.Line>();
            //节点
            foreach (JProperty jp in jn)
            {
                JObject node = (JObject)jp.Value;
                string n = node.ToString().Remove(node.ToString().Length - 1, 1) + ",\"id\":\"" + jp.Name + "\"}";
                Models.WorkFlow.Node rb = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.WorkFlow.Node>(n);
                nodes.Add(rb);
            }
            //线
            foreach (JProperty jp in jl)
            {
                JObject line = (JObject)jp.Value;
                string n = line.ToString().Remove(line.ToString().Length - 1, 1) + ",\"id\":\"" + jp.Name + "\"}";
                Models.WorkFlow.Line rb = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.WorkFlow.Line>(n);
                lines.Add(rb);
            }
            using (var tran = context.Database.BeginTransaction())
            {
                try
                {
                    var ls = await Task.FromResult(context.APP_WORKFLOW_APPR_LINES.ToList().Where(u => u.APPR_FLOW_ID == int.Parse(flowid ?? "-99")).ToList());
                    var ns = await Task.FromResult(context.APP_WORKFLOW_APPR_NODES.ToList().Where(u => u.APPR_FLOW_ID == int.Parse(flowid ?? "-99")).ToList());
                    context.APP_WORKFLOW_APPR_LINES.RemoveRange(ls);
                    context.APP_WORKFLOW_APPR_NODES.RemoveRange(ns);
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        string type = "";
                        if (nodes[i].type == "start round") { type = "start"; }
                        if (nodes[i].type == "task round") { type = "node"; }
                        if (nodes[i].type == "end round") { type = "end"; }
                        APP_WORKFLOW_APPR_NODES node = new APP_WORKFLOW_APPR_NODES()
                        {
                            APPR_FLOW_ID = int.Parse(flowid),
                            NODE_CODE = nodes[i].id ?? "",
                            NODE_NAME = nodes[i].name ?? "",
                            TYPE = type ?? "",
                            NUM = i + 1,
                            LEFT = int.Parse(nodes[i].left),
                            TOP = int.Parse(nodes[i].top),
                            WIDTH = int.Parse(nodes[i].width),
                            HEIGHT = int.Parse(nodes[i].height)
                        };
                        context.APP_WORKFLOW_APPR_NODES.Add(node);
                    }
                    for (int i = 0; i < lines.Count; i++)
                    {
                        APP_WORKFLOW_APPR_LINES line = new APP_WORKFLOW_APPR_LINES()
                        {
                            APPR_FLOW_ID = int.Parse(flowid),
                            LINE_CODE = lines[i].id ?? "",
                            LINE_NAME = lines[i].name ?? "",
                            TYPE = "line",
                            NUM = i + 1,
                            FROM1 = lines[i].from ?? "",
                            TO1 = lines[i].to ?? ""
                        };
                        context.APP_WORKFLOW_APPR_LINES.Add(line);
                    }
                    context.SaveChanges();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Msg = "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
                }
            }
            if (Msg == "")
            {
                OracleParameter[] param2 = { data.MakeInParam(":flowid", flowid) };
                string sql_delete_line = @"delete from app_WORKFLOW_APPR_LINES where APPR_flow_ID=:flowid and from1 not in ( select node_code from  app_WORKFLOW_APPR_NODES where APPR_flow_ID=:flowid)";
                string sql_delete_line2 = @"delete from app_WORKFLOW_APPR_LINES where APPR_flow_ID = :flowid and to1 not in (select node_code from app_WORKFLOW_APPR_NODES where APPR_flow_ID = :flowid)";
                await data.DoSqlByParam(sql_delete_line, param2);
                await data.DoSqlByParam(sql_delete_line2, param2);
                Msg = "{\"code\":200,\"msg\":\"保存成功\"}";
            }
            return Content(Msg);
        }
        #endregion

        #region 编辑节点名称
        [HttpPost]
        public async Task<IActionResult> WorkFlowNodeUpdate(string code, string name)
        {
            string Msg = "";
            string sql = @"update app_workflow_appr_nodes
                                           set node_name        = :name,
                                               last_update_date = sysdate,
                                               last_updated_by  = :userid
                                         where NODE_CODE = :code";
            OracleParameter[] param = { data.MakeInParam(":name",name??"" ),
                                        data.MakeInParam(":userid",HttpContext.Session.GetString("USER_ID")),
                                        data.MakeInParam(":code",code??"" )
                                        };
            Msg = await data.DoSqlByParam(sql, param) ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 编辑线名称
        [HttpPost]
        public async Task<IActionResult> WorkFlowLineUpdate(string code, string name)
        {
            string Msg = "";
            string sql = @"update app_workflow_appr_lines
                                       set line_name        = :name,
                                           last_update_date = sysdate,
                                           last_updated_by  = :userid
                                     where line_code = :code";
            OracleParameter[] param = { data.MakeInParam(":name",name??"" ),
                                        data.MakeInParam(":userid",HttpContext.Session.GetString("USER_ID")),
                                        data.MakeInParam(":code",code??"" )
                                        };
            Msg = await data.DoSqlByParam(sql, param) ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 获取工作流类型
        [HttpPost]
        public async Task<IActionResult> GetFlowType(string flowid)
        {
            string Msg = "";
            DataSet ds = new DataSet();
            string sql = @"select t.*
                          from app_workflow_appr_type t,app_workflow_appr_flow f
                         where t.status = 1
                           and t.appr_type_id = f.appr_type_id
                           and f.appr_flow_id=:flowid";
            OracleParameter[] param = { data.MakeInParam(":flowid", flowid) };
            ds = await data.GetDataSetByParam(sql, param);
            Msg = ds.Tables[0].Rows.Count > 0
                ? "{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}"
                : "{\"code\":0,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 获取列
        [HttpPost]
        public async Task<IActionResult> GetColName(string flowid)
        {
            string Msg = "";
            DataSet ds = new DataSet();
            OracleParameter[] param2 = { data.MakeInParam(":flowid", flowid) };
            string tableName = await data.GetStringByParam(@"select t.attribute9 from app_workflow_appr_type t,app_workflow_appr_flow f
                                                        where t.status = 1 and t.appr_type_id = f.appr_type_id and f.appr_flow_id=:flowid", param2);
            string sql = @"select COMMENTS,COLUMN_NAME from  user_col_comments t where t.TABLE_NAME=:tablename";
            OracleParameter[] param = { data.MakeInParam(":tablename", tableName.ToUpper()) };
            ds = await data.GetDataSetByParam(sql, param);
            Msg = ds.Tables[0].Rows.Count > 0
                ? "{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}"
                : "{\"code\":0,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 获取线条件
        [HttpPost]
        public async Task<IActionResult> GetLinePro(string limit, string page, string flowid, string line_code)
        {
            string Msg = "";
            DataSet ds = new DataSet();
            string sql = @"SELECT * FROM (SELECT ROWNUM AS rowno, r.*
                                     FROM (SELECT *
                                          FROM APP_WORKFLOW_LINE_PROPERTY
                                         where APPR_FLOW_ID = :flowid
                                           and LINE_CODE = :line_code) r
                                    where ROWNUM <= :page * :limit) table_alias
                            WHERE table_alias.rowno > (:page - 1) * :limit";
            OracleParameter[] param = { data.MakeInParam(":flowid", flowid),
                                        data.MakeInParam(":line_code", line_code),
                                        data.MakeInParam(":page", page),
                                        data.MakeInParam(":limit", limit)
                                      };
            ds = await data.GetDataSetByParam(sql, param);
            string sql2 = @"select count(*) FROM APP_WORKFLOW_LINE_PROPERTY where APPR_FLOW_ID = :flowid and LINE_CODE = :line_code";
            OracleParameter[] param2 = { data.MakeInParam(":flowid", flowid), data.MakeInParam(":line_code", line_code) };
            string count = await data.GetStringByParam(sql2, param2);
            Msg = ds.Tables[0].Rows.Count > 0
                ? "{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}"
                : "{\"code\":0,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 写入线条件:Sql
        [HttpPost]
        public async Task<ActionResult> LineSettingSqlInsert(string flowid, string line_code, string sqls)
        {
            string Msg = "";
            string sql = @"INSERT INTO APP_WORKFLOW_LINE_PROPERTY (PRO_ID, APPR_flow_ID, LINE_CODE, SQL)
                            VALUES  (APP_WORKFLOW_LINE_PROPERTY_s.nextval, :flowid, :LINE_CODE, :SQL)";
            OracleParameter[] param = { data.MakeInParam(":flowid",flowid ),
                                        data.MakeInParam(":LINE_CODE",line_code ),
                                        data.MakeInParam(":SQL",sqls)};
            Msg = await data.DoSqlByParam(sql, param) ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 写入线条件
        [HttpPost]
        public async Task<IActionResult> LineSettingInsert(string flowid, string line_code, string type, string col_if, string value, string col_name)
        {
            string Msg = "";
            string sql1 = "";
            string values = "";
            switch (type)
            {
                case "数字": values = value; break;
                case "文本": values = " '" + value + "'"; break;
                case "日期(yyyy-MM-dd)": values = " to_date('" + value + "','yyyy-mm-dd')"; break;
            }
            switch (col_if)
            {
                case "like ": sql1 = col_name + " " + col_if + " %" + values + "% "; break;
                case "not like ": sql1 = col_name + " " + col_if + " %" + values + "% "; break;
                default: sql1 = col_name + " " + col_if + " " + values; break;
            }
            string sql = @"INSERT INTO APP_WORKFLOW_LINE_PROPERTY (PRO_ID, APPR_flow_ID, LINE_CODE, SQL)
                            VALUES  (APP_WORKFLOW_LINE_PROPERTY_s.nextval, :flowid, :LINE_CODE, :SQL)";
            OracleParameter[] param = { data.MakeInParam(":flowid",flowid ),
                                        data.MakeInParam(":LINE_CODE",line_code ),
                                        data.MakeInParam(":SQL",sql1)};
            Msg = await data.DoSqlByParam(sql, param) ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 编辑线条件
        [HttpPost]
        public async Task<IActionResult> LineSettingEdit(string pro_id, string sqls)
        {
            string Msg = "";
            string sql = @"UPDATE APP_WORKFLOW_LINE_PROPERTY SET  SQL = :SQL WHERE PRO_ID = :PRO_ID";
            OracleParameter[] param = { data.MakeInParam(":SQL", sqls), data.MakeInParam(":PRO_ID", pro_id) };
            Msg = await data.DoSqlByParam(sql, param) ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 删除线条件
        [HttpPost]
        public async Task<IActionResult> LineSettingDelete(string id)
        {
            string Msg = "";
            string sql = @"DELETE FROM APP_WORKFLOW_LINE_PROPERTY WHERE PRO_ID = :id ";
            OracleParameter[] param = { data.MakeInParam(":id", id) };
            Msg = await data.DoSqlByParam(sql, param) ? "{\"code\":200,\"msg\":\"删除成功\"}" : "{\"code\":300,\"msg\":\"删除失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 获取节点属性
        [HttpPost]
        public async Task<IActionResult> GetNodePro(string flowid, string nodecode)
        {
            string Msg = "";
            DataSet ds = new DataSet();
            string sql = @"select wn.node_code,
                               wn.node_name,
                               decode(wn.shape,'rect','rect',wn.type) type,
                               wn.left,
                               wn.top,
                               wp.appr_person,
                                (select sp.person_name from  app_person sp where sp.person_id=wp.appr_person) appr_person_name,
                               wp.appr_post,                           
                               wp.RECT,
                               wp.VIEW_PAGE,
                                (select nvl(nvl(d.attribute1, d.dept_name), '公共') || '_' ||
               f.appr_flow_name
          from APP_WORKFLOW_APPR_FLOW f, app_dept d
         where f.appr_flow_id = wp.RECT
           and f.dept_id = d.dept_id(+)) rect_name,
                               wp.DEPT_ID,
                               wp.TEAM_ID,
                               (select team_name from app_team where team_id=wp.team_id) team,
                               wp.COL_SET,
                               wp.PLSQLPRO,
                               wp.SFTJR,
                               wp.DEPT_SQL,
                               wp.TEAM_SQL,
                               wp.SFSJGW,
                               wp.POST_SQL,
                                wp.PERSON_CODE
                          from app_workflow_appr_nodes wn,app_WORKFLOW_NODE_PROPERTY wp
                         where wn.APPR_flow_ID = :flowid
                         and wn.node_code =:nodecode
                         and wn.APPR_flow_ID=wp.APPR_flow_ID(+)
                         and wn.type=wp.type(+)
                         and wn.node_code=wp.node_code(+)
                         order by round(top/100),round(left/100)";
            OracleParameter[] param = { data.MakeInParam(":flowid", flowid),
                                      data.MakeInParam(":nodecode", nodecode)};
            ds = await data.GetDataSetByParam(sql, param);
            Msg = ds.Tables[0].Rows.Count > 0
                ? "{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}"
                : "{\"code\":0,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 写入或编辑节点属性
        [HttpPost]
        public async Task<IActionResult> SaveNodePro(string flowid, string nodecode, string type, string page, string person, string post,
            string rect, string dept, string personSql, string plsql, string tjr, string deptSql,
            /*string sjgw,*/ string postSql, string personCode)
        {
            string Msg = "";
            string sql1 = @"select count(*) from app_WORKFLOW_NODE_PROPERTY t where t.APPR_flow_ID=:flowid and t.node_code=:nodecode";
            OracleParameter[] param1 = { data.MakeInParam(":flowid",flowid ),
                                        data.MakeInParam(":nodecode",nodecode )};
            int n = int.Parse(await data.GetStringByParam(sql1, param1));
            string sql = "";
            OracleParameter[] param = { };
            if (n == 0)
            {
                sql = @"Insert Into App_Workflow_Node_Property
                                              (Pro_Id,
                                               Appr_Flow_Id,
                                               Type,
                                               Node_Code,
                                               View_Page,
                                               Appr_Person,
                                               --Appr_Post,
                                               Rect,
                                               Dept_Id,
                                               Col_Set,
                                               Plsqlpro,
                                               Sftjr,
                                               Dept_Sql,
                                               --Sfsjgw,
                                               Post_Sql,
                                               Person_Code)
                                            Values
                                              (App_Workflow_Node_Property_s.Nextval,
                                               :Flowid,
                                               :Type,
                                               :Nodecode,
                                               :Page,
                                               :Person,
                                               --:Post,
                                               :Rect,
                                               :Dept,
                                               :Personsql,
                                               :Plsql,
                                               :Tjr,
                                               :Deptsql,
                                               --:Sjgw,
                                               :Postsql,
                                               :Personcode)";
                param = new OracleParameter[] {
                                        data.MakeInParam(":Flowid",flowid??"" ),
                                        data.MakeInParam(":Type",type??"" ),
                                        data.MakeInParam(":Nodecode",nodecode??"" ),
                                        data.MakeInParam(":Page",page??"" ),
                                        data.MakeInParam(":Person",person??"" ),
                                        //data.MakeInParam(":Post",post??"" ),
                                        data.MakeInParam(":Rect",rect??"" ),
                                        data.MakeInParam(":Dept",dept??"" ),
                                        data.MakeInParam(":Personsql",personSql??"" ),
                                        data.MakeInParam(":Plsql",plsql??"" ),
                                        data.MakeInParam(":Tjr",tjr??"" ),
                                        data.MakeInParam(":Deptsql",deptSql??"" ),
                                        //data.MakeInParam(":Sjgw",sjgw??"" ),
                                        data.MakeInParam(":Postsql",postSql??"" ),
                                        data.MakeInParam(":Personcode",personCode??"" )
                };
            }
            else
            {
                sql = @"Update App_Workflow_Node_Property
                                           Set View_Page   = :Page,
                                               Type        = :Type,
                                               Appr_Person = :Person,
                                               --Appr_Post   = :Post,
                                               Rect        = :Rect,
                                               Dept_Id     = :Dept,
                                               Col_Set     = :Personsql,
                                               Plsqlpro    = :Plsql,
                                               Sftjr       = :Tjr,
                                               Dept_Sql    = :Deptsql,
                                               --Sfsjgw      = :Sjgw,
                                               Post_Sql    = :Postsql,
                                               Person_Code = :Personcode
                                         Where Appr_Flow_Id = :Flowid
                                           And Node_Code = :Nodecode";
                param = new OracleParameter[] {
                                        data.MakeInParam(":Page",page??"" ),
                                        data.MakeInParam(":Type",type??"" ),
                                        data.MakeInParam(":Person",person??"" ),
                                        //data.MakeInParam(":Post",post??"" ),
                                        data.MakeInParam(":Rect",rect??"" ),
                                        data.MakeInParam(":Dept",dept??"" ),
                                        data.MakeInParam(":Personsql",personSql??"" ),
                                        data.MakeInParam(":Plsql",plsql??"" ),
                                        data.MakeInParam(":Tjr",tjr??"" ),
                                        data.MakeInParam(":Deptsql",deptSql??"" ),
                                        //data.MakeInParam(":Sjgw",sjgw??"" ),
                                        data.MakeInParam(":Postsql",postSql??"" ),
                                        data.MakeInParam(":Personcode",personCode??"" ),
                                        data.MakeInParam(":Flowid",flowid??"" ),
                                        data.MakeInParam(":Nodecode",nodecode??"" )};
            }
            Msg = await data.DoSqlByParam(sql, param) ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 根据人员获取审批信息：待办
        [HttpGet]
        public async Task<IActionResult> GetWorkFlowByPerson()
        {
            string Msg = "";
            OracleParameter[] prams ={
                         data.MakeInParam("v_person_id",HttpContext.Session.GetString("PERSON_ID")),
                data.MakeInParam("c_f",OracleDbType.RefCursor,0,ParameterDirection.Output)
                             };
            DataSet ds = await data.RunProcCur("APP_WolkFlow_Appr_pag.Get_WolkFlow_by_Person", prams);
            Msg = ds.Tables[0].Rows.Count > 0
                ? "{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}"
                : "{\"code\":0,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 根据人员获取审批信息:已办
        [HttpGet]
        public async Task<IActionResult> GetFinishWorkFlowByPerson(string startDate, string endDate)
        {
            string Msg = "";
            OracleParameter[] prams ={
                         data.MakeInParam("v_person_id",HttpContext.Session.GetString("PERSON_ID")),
                data.MakeInParam("v_date1",OracleDbType.Date,0,startDate==null? DateTime.Now.AddDays(-7):Convert.ToDateTime(startDate)),
                          data.MakeInParam("v_date2",OracleDbType.Date,0,endDate==null?DateTime.Now.AddDays(7):Convert.ToDateTime(endDate)),
                data.MakeInParam("c_f",OracleDbType.RefCursor,0,ParameterDirection.Output),
                             };
            DataSet ds = await data.RunProcCur("APP_WolkFlow_Appr_pag.Get_WolkFlow_done_by_Ps", prams);
            if (ds.Tables[0].Rows.Count > 0)
            {
                Msg = "{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}";
            }
            else
            {
                Msg = "{\"code\":0,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            }
            return Content(Msg);
        }
        #endregion

        #region 获取审批信息
        [HttpGet]
        public async Task<IActionResult> GetInfo(string docid, string name, string apprid, string status, string type, string des)
        {
            #region 参数
            apprid = apprid == null ? "" : (apprid == "null" ? "" : apprid);
            //获取审批ID
            var appr_id = apprid == "" ? await data.GetString(@"select APP_WORKFLOW_APPR_S.Nextval from dual") : apprid;
            //打印流程的链接地址
            var print_link = "../../Reports/WorkFlow_Appr_Rep?Rowid=" + appr_id;
            //文档标题
            var title_name = "";
            //文档超链接
            var doc_link = "";
            //文档说明
            var doc_des = "";
            //发布人
            var fbr = "";
            //发布单位
            var fbdw = "";
            //发布时间
            var fbsj = "";
            //联系电话
            var lxdh = "";
            //文档内容是否只读
            var doc_descReadOnly = true;
            //提交按钮是否启用
            var btnCommitEnable = true;
            //返回信息
            var msg = "";
            //传阅按钮是否启用
            var btnSendReadEnable = true;
            //退回按钮是否启用
            var btnBackEnable = true;
            //收回按钮是否启用
            var btnTakeBackEnable = false;
            //是否显示自定义人员
            var spr1 = true;
            //是否显示下一审批人
            var spr2 = true;
            //下一审批人的id
            var nextpersonid = "";
            //下一审批人的项目
            var nextpersonname = "";
            //退回人集合
            DataSet ds_thr = new DataSet();
            //审批人id
            var zhspr = "";
            //当前审批人
            var dqspr = "";
            //事务记录集合
            DataSet ds_tran = new DataSet();
            //审批信息集合
            DataSet ds_info = new DataSet();
            //获取下一审批人集合
            DataSet ds_nextperson = new DataSet();
            //下一环节
            var nextnode = "";
            //提交按钮文本
            var btnCommitText = "";
            //事务id
            string tran_id = "";
            //审批类型id
            string appr_type_id = "";
            //系统名称
            string xtmc = "";
            //审批流id
            string appr_flow_id = "";
            //attribute3
            string attribute3 = "";
            //attribute4
            string attribute4 = "";
            //attribute5
            string attribute5 = "";
            //当前节点
            string dqjd = "";
            //上级节点
            string upper_node = "";
            //上级节点类型
            string upper_rect = "";
            //当前节点类型
            string curr_rect = "";
            //提交节点
            string tjjd = "";
            //提交节点类型
            string tjjdlx = "";
            //当前节点类型
            string curr_node_type = "";
            //当前节点
            string curr_node = "";
            //下一节点类型
            string next_node_type = "";
            //下一节点
            string next_code = "";
            //审批流节点集合
            DataSet ds_flow_node = new DataSet();
            //当前审批节点
            string curr_appr_node = "";
            #endregion

            if ((status ?? "") == "New")//新
            {
                #region 创建审批流相关
                title_name = name ?? "";
                doc_des = des ?? "";
                fbr = HttpContext.Session.GetString("USER_NAME");
                fbdw = HttpContext.Session.GetString("DEPT_NAME");
                fbsj = DateTime.Now.ToString("yyyy-MM-dd");
                doc_descReadOnly = false;
                btnCommitEnable = true;
                btnBackEnable = false;
                btnSendReadEnable = false;
                appr_type_id = await data.GetStringByParam(@"select t.appr_type_id from app_workflow_appr_type t where t.appr_type_code=:code",
                    new OracleParameter[] { data.MakeInParam(":code", type ?? "") });
                //审批类型id不是空的
                if (appr_type_id != "")
                {
                    //创建审批流
                    #region 创建审批
                    OracleParameter[] prams ={
                        data.MakeInParam("v_appr_type_id",appr_type_id),
                       data.MakeInParam("v_doc_id",docid??""),
                        data.MakeInParam("v_dept_id","-99"),
                        data.MakeInParam("p_inf",OracleDbType.Varchar2,1000,ParameterDirection.Output),
                             };
                    var aa = await data.RunProcStr("app_wolkflow_appr_pag.create_wolkflow", prams);
                    //不等于Y就是失败
                    if (aa != "Y")
                    {
                        msg = aa;
                        btnCommitEnable = false;
                        btnSendReadEnable = false;
                        btnBackEnable = false;
                        btnTakeBackEnable = false;
                        spr1 = false;
                        spr2 = false;
                        return Json(new
                        {
                            code = 1,
                            msg = msg,
                            apprid = appr_id,
                            appr_type_id = appr_type_id,
                            title_name = title_name,
                            doc_des = doc_des,
                            doc_descReadOnly = doc_descReadOnly,
                            btnCommitEnable = btnCommitEnable,
                            btnSendReadEnable = btnSendReadEnable,
                            btnBackEnable = btnBackEnable,
                            btnTakeBackEnable = btnTakeBackEnable,
                            spr1 = spr1,
                            spr2 = spr2,
                            xtmc = xtmc,
                            appr_flow_id = appr_flow_id,
                            upper_node = upper_node,
                            upper_rect = upper_rect,
                            dqjd = dqjd,
                            curr_rect = curr_rect
                        });
                    }
                    #endregion
                    //执行到这里，证明已经生成成功
                    #region 执行成功后，获取信息
                    string sql_strart_node = @"select t.APPR_Flow_ID ,t.curr_node_code as dqjd,t.curr_rect,
                                               decode(at.attribute15, null, '', '【' || at.attribute15 || '】') xtmc,
                                                '' as Upper_Node,'' as Upper_Rect
                                          from app_workflow_appr_tran_temp t, app_workflow_appr_type at
                                         where t.doc_id = :docid
                                           and t.appr_type_id = :appr_type_id
                                           and t.curr_node_type <> 'rect'
                                           and t.curr_node_type = 'start'
                                           and t.appr_type_id=at.appr_type_id
                                         order by t.w_n";
                    OracleParameter[] start_note = { data.MakeInParam(":docid", docid ?? ""), data.MakeInParam(":appr_type_id", appr_type_id ?? "") };
                    var ds_strart_node = await data.GetDataSetByParam(sql_strart_node, start_note);
                    if (ds_strart_node.Tables[0].Rows.Count > 0)
                    {
                        xtmc = ds_strart_node.Tables[0].Rows[0]["xtmc"].ToString() ?? "";
                        appr_flow_id = ds_strart_node.Tables[0].Rows[0]["APPR_Flow_ID"].ToString() ?? "";
                        upper_node = "";
                        upper_rect = "";
                        dqjd = ds_strart_node.Tables[0].Rows[0]["dqjd"].ToString() ?? "";
                        curr_rect = ds_strart_node.Tables[0].Rows[0]["curr_rect"].ToString() ?? "";
                    }
                    else
                    {
                        msg = "未生成对应审批流，请联系本部门管理员！";
                        btnCommitEnable = false;
                        btnSendReadEnable = false;
                        btnBackEnable = false;
                        btnTakeBackEnable = false;
                        spr1 = false;
                        spr2 = false;
                        return Json(new
                        {
                            code = 1,
                            msg = msg,
                            apprid = appr_id,
                            appr_type_id = appr_type_id,
                            title_name = title_name,
                            doc_des = doc_des,
                            doc_descReadOnly = doc_descReadOnly,
                            btnCommitEnable = btnCommitEnable,
                            btnSendReadEnable = btnSendReadEnable,
                            btnBackEnable = btnBackEnable,
                            btnTakeBackEnable = btnTakeBackEnable,
                            spr1 = spr1,
                            spr2 = spr2,
                            xtmc = xtmc,
                            appr_flow_id = appr_flow_id,
                            upper_node = upper_node,
                            upper_rect = upper_rect,
                            dqjd = dqjd,
                            curr_rect = curr_rect
                        });
                    }
                    #endregion
                    //继续获取节点信息
                    #region 获取节点信息
                    string sql_jd_node = @"select t.*,
                                               decode(at.attribute15, null, '', '【' || at.attribute15 || '】') xtmc
                                          from app_workflow_appr_tran_temp t, app_workflow_appr_type at
                                         where t.doc_id = :docid
                                           and t.appr_type_id = :appr_type_id
                                           and t.curr_node_type <> 'rect'
                                           and t.curr_node_type = 'node'
                                           and t.appr_type_id=at.appr_type_id
                                         order by t.w_n";
                    OracleParameter[] jd_note = { data.MakeInParam(":docid", docid ?? ""), data.MakeInParam(":appr_type_id", appr_type_id ?? "") };
                    var ds_jd_node = await data.GetDataSetByParam(sql_jd_node, jd_note);
                    if (ds_jd_node.Tables[0].Rows.Count == 0)
                    {
                        msg = "未生成对应审批流，请联系本部门管理员！";
                        btnCommitEnable = false;
                        btnSendReadEnable = false;
                        btnBackEnable = false;
                        btnTakeBackEnable = false;
                        spr1 = false;
                        spr2 = false;
                        return Json(new
                        {
                            code = 1,
                            msg = msg,
                            apprid = appr_id,
                            appr_type_id = appr_type_id,
                            title_name = title_name,
                            doc_des = doc_des,
                            doc_descReadOnly = doc_descReadOnly,
                            btnCommitEnable = btnCommitEnable,
                            btnSendReadEnable = btnSendReadEnable,
                            btnBackEnable = btnBackEnable,
                            btnTakeBackEnable = btnTakeBackEnable,
                            spr1 = spr1,
                            spr2 = spr2,
                            xtmc = xtmc,
                            appr_flow_id = appr_flow_id,
                            upper_node = upper_node,
                            upper_rect = upper_rect,
                            dqjd = dqjd,
                            curr_rect = curr_rect
                        });
                    }
                    #endregion
                }
                else
                {
                    #region 未定义对应审批流，退出
                    msg = "未定义对应审批流，请联系本部门管理员！";
                    btnCommitEnable = false;
                    btnSendReadEnable = false;
                    btnBackEnable = false;
                    btnTakeBackEnable = false;
                    spr1 = false;
                    spr2 = false;
                    return Json(new
                    {
                        code = 1,
                        msg = msg,
                        apprid = appr_id,
                        appr_type_id = appr_type_id,
                        title_name = title_name,
                        doc_des = doc_des,
                        doc_descReadOnly = doc_descReadOnly,
                        btnCommitEnable = btnCommitEnable,
                        btnSendReadEnable = btnSendReadEnable,
                        btnBackEnable = btnBackEnable,
                        btnTakeBackEnable = btnTakeBackEnable,
                        spr1 = spr1,
                        spr2 = spr2,
                        xtmc = xtmc,
                        appr_flow_id = appr_flow_id,
                        upper_node = upper_node,
                        upper_rect = upper_rect,
                        dqjd = dqjd,
                        curr_rect = curr_rect
                    });
                    #endregion
                }
                #endregion
                ds_thr.Tables.Add(new DataTable());
            }
            else
            {
                ds_thr.Tables.Clear();
                #region 获取退回人集合
                string sql_thr = @"select distinct t.appr_person_id || '/' || t.attribute1|| '/' || t.attribute7  as value,
                         t.attribute11 || '：' ||
                         (select p.person_name
                            from app_person p
                           where p.person_id = t.appr_person_id) as text
                       from app_workflow_appr_tran t
                      where t.appr_id = :apprid
                        and t.status = 1
                        and t.attribute11 is not null";
                OracleParameter[] thr = { data.MakeInParam(":apprid", appr_id) };
                ds_thr = await data.GetDataSetByParam(sql_thr, thr);
                DataRow dr = ds_thr.Tables[0].NewRow();
                dr["VALUE"] = "-1";
                dr["TEXT"] = "发起人";
                ds_thr.Tables[0].Rows.Add(dr);
                btnSendReadEnable = true;
                doc_descReadOnly = true;
                if ((status ?? "") == "sp")
                {
                    btnCommitEnable = true;
                    btnBackEnable = true;
                    btnSendReadEnable = true;
                }
                else
                {
                    spr1 = false;
                    spr2 = false;
                }
                #endregion
            }
            OracleParameter[] prams1 ={
                         data.MakeInParam("v_Appr_ID",appr_id),
                data.MakeInParam("c_f",OracleDbType.RefCursor,0,ParameterDirection.Output)};
            //获取到事务记录
            ds_tran = await data.RunProcCur("app_WolkFlow_Appr_pag.Get_app_WorkFlow_Appr_Tran", prams1);
            if (ds_tran.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds_tran.Tables[0].Rows.Count; i++)
                {
                    zhspr = ds_tran.Tables[0].Rows[i]["appr_person_id"].ToString() ?? "";
                    tran_id = ds_tran.Tables[0].Rows[i]["appr_tran_id"].ToString() ?? "";
                }
            }
            OracleParameter[] prams3 ={
                         data.MakeInParam("v_Appr_ID",appr_id),
                data.MakeInParam("c_f",OracleDbType.RefCursor,0,ParameterDirection.Output)};
            //获取审批信息
            ds_info = await data.RunProcCur("app_WolkFlow_Appr_pag.Get_app_WolkFlow", prams3);
            #region 审批信息行数大于0
            if (ds_info.Tables[0].Rows.Count > 0)
            {
                if ((ds_info.Tables[0].Rows[0]["Status"].ToString() ?? "") == "3")
                {
                    doc_descReadOnly = false;
                    doc_des = ds_info.Tables[0].Rows[0]["Doc_Note"].ToString() ?? "";
                }
                appr_type_id = ds_info.Tables[0].Rows[0]["appr_type_id"].ToString() ?? "";
                title_name = ds_info.Tables[0].Rows[0]["Doc_Title"].ToString() ?? "";
                doc_link = ds_info.Tables[0].Rows[0]["Http"].ToString() ?? "";
                xtmc = ds_info.Tables[0].Rows[0]["xtmc"].ToString() ?? "";
                doc_des = ds_info.Tables[0].Rows[0]["Doc_Note"].ToString() ?? "";
                doc_descReadOnly = true;
                fbdw = ds_info.Tables[0].Rows[0]["Appl_Dept"].ToString() ?? "";
                fbr = ds_info.Tables[0].Rows[0]["Appl_Person"].ToString() ?? "";
                fbsj = ds_info.Tables[0].Rows[0]["Appl_Date"].ToString() ?? "";
                lxdh = ds_info.Tables[0].Rows[0]["LXDH"].ToString() ?? "";
                appr_flow_id = ds_info.Tables[0].Rows[0]["Appr_Flow_Id"].ToString() ?? "";
                attribute3 = ds_info.Tables[0].Rows[0]["attribute3"].ToString() ?? "";
                attribute4 = ds_info.Tables[0].Rows[0]["attribute4"].ToString() ?? "";
                attribute5 = ds_info.Tables[0].Rows[0]["attribute5"].ToString() ?? "";
                dqjd = ds_info.Tables[0].Rows[0]["dqjd"].ToString() ?? "";
                tjjd = ds_info.Tables[0].Rows[0]["tjjd"].ToString() ?? "";
                tjjdlx = ds_info.Tables[0].Rows[0]["tjjdlx"].ToString() ?? "";
                upper_node = ds_info.Tables[0].Rows[0]["Upper_Node"].ToString() ?? "";
                upper_rect = ds_info.Tables[0].Rows[0]["Upper_Rect"].ToString() ?? "";
                curr_rect = ds_info.Tables[0].Rows[0]["Curr_Rect"].ToString() ?? "";
                //是否重新生成流程  0-否；1-是
                if ((ds_info.Tables[0].Rows[0]["attribute4"].ToString() ?? "") == "1")
                {
                    OracleParameter[] prams ={
                        data.MakeInParam("v_appr_type_id",appr_type_id),
                       data.MakeInParam("v_doc_id",docid),
                        data.MakeInParam("v_dept_id",ds_info.Tables[0].Rows[0]["Appl_Dept_ID"].ToString()??""),
                        data.MakeInParam("p_inf",OracleDbType.Varchar2,1000,ParameterDirection.Output)};
                    await data.RunProcStr("app_wolkflow_appr_pag.create_wolkflow", prams);
                }
                //审批人不为空，显示一个table的当前审批人信息
                if ((ds_info.Tables[0].Rows[0]["Appr_Person"].ToString() ?? "") != "")
                {
                    //这里不写
                }
                dqspr = ds_info.Tables[0].Rows[0]["Appr_Person_ID"].ToString() ?? "";
                if (dqspr == HttpContext.Session.GetString("PERSON_ID"))
                {
                    if ((ds_info.Tables[0].Rows[0]["dqUrl"].ToString() ?? "") != "")
                    {
                        doc_link = (ds_info.Tables[0].Rows[0]["dqUrl"].ToString() ?? "") + "&Rowid=" + docid;
                    }
                }
                if (zhspr == HttpContext.Session.GetString("PERSON_ID") && dqspr == HttpContext.Session.GetString("PERSON_ID")
                    && ds_info.Tables[0].Rows[0]["STATUS"].ToString() == "1")
                {
                    btnTakeBackEnable = true;
                }
            }
            #endregion

            #region 下一审批人信息
            if ((status ?? "") == "New" || (ds_info.Tables[0].Rows[0]["Status"].ToString() ?? "") != "2" && appr_flow_id != "-99")
            {
                OracleParameter[] prams ={
                         data.MakeInParam("v_appr_type_id",appr_type_id),
                         data.MakeInParam("v_Appr_Flow_ID",appr_flow_id),
                         data.MakeInParam("v_Appr_ID",appr_id),
                         data.MakeInParam("V_Doc_ID",docid),
                         data.MakeInParam("v_Node_code",dqjd),
                         data.MakeInParam("v_curr_dept",HttpContext.Session.GetString("DEPT_ID")),
                         data.MakeInParam("v_curr_team",""),
                         data.MakeInParam("v_curr_post_id",HttpContext.Session.GetString("POST_ID")),
                         data.MakeInParam("p_Curr_Rect",curr_rect),
                         data.MakeInParam("p_Upper_Node",upper_node),
                         data.MakeInParam("p_Upper_Rect",upper_rect),
                    data.MakeInParam("c_f",OracleDbType.RefCursor,0,ParameterDirection.Output)};
                ds_nextperson = await data.RunProcCur("APP_WolkFlow_Appr_pag.Get_APP_WorkFlow_Next_Person2", prams);
                if (ds_nextperson.Tables[0].Rows[0][0].ToString() != "-99" && ds_nextperson.Tables[0].Rows[0][0].ToString() != "-88")
                {
                    if (ds_nextperson.Tables[0].Rows.Count == 1)
                    {
                        spr1 = false;
                        nextpersonid = ds_nextperson.Tables[0].Rows[0][0].ToString();
                        nextpersonname = ds_nextperson.Tables[0].Rows[0][1].ToString();
                    }
                    else
                    {
                        //返回ds_nextperson集合，渲染radiobutton
                    }
                    curr_node_type = ds_nextperson.Tables[0].Rows[0][2].ToString();
                    curr_node = ds_nextperson.Tables[0].Rows[0][3].ToString();
                    next_node_type = ds_nextperson.Tables[0].Rows[0][4].ToString();
                    next_code = ds_nextperson.Tables[0].Rows[0][5].ToString();
                    upper_node = ds_nextperson.Tables[0].Rows[0][6].ToString();
                    upper_rect = ds_nextperson.Tables[0].Rows[0][7].ToString();
                    curr_rect = ds_nextperson.Tables[0].Rows[0][8].ToString();
                    nextnode = await data.GetString(@"select t.curr_node_name
                                                      from app_workflow_appr_tran_temp t
                                                     where t.doc_id = " + docid + @"
                                                       and t.appr_flow_id =" + appr_flow_id + @"
                                                       and t.curr_rect=" + curr_rect + @"
                                                       and t.curr_node_code = '" + next_code + "'");
                    if (next_node_type == "end")
                    {
                        spr1 = false;
                        spr2 = false;
                        btnCommitText = "同意";
                    }
                }
                #region 获取不正常
                if (ds_nextperson.Tables[0].Rows[0][0].ToString() == "-88")
                {
                    curr_node_type = ds_nextperson.Tables[0].Rows[0][2].ToString();
                    curr_node = ds_nextperson.Tables[0].Rows[0][3].ToString();
                    next_node_type = ds_nextperson.Tables[0].Rows[0][4].ToString();
                    next_code = ds_nextperson.Tables[0].Rows[0][5].ToString();
                    upper_node = ds_nextperson.Tables[0].Rows[0][6].ToString();
                    upper_rect = ds_nextperson.Tables[0].Rows[0][7].ToString();
                    curr_rect = ds_nextperson.Tables[0].Rows[0][8].ToString();
                    nextnode = await data.GetString(@"select t.curr_node_name
                                                      from app_workflow_appr_tran_temp t
                                                     where t.doc_id = " + docid + @"
                                                       and t.appr_flow_id =" + appr_flow_id + @"
                                                       and t.curr_rect=" + curr_rect + @"
                                                       and t.curr_node_code = '" + next_code + "'");
                }
                if (ds_nextperson.Tables[0].Rows[0][0].ToString() == "-99")
                {
                    msg = ds_nextperson.Tables[0].Rows[0][1].ToString() ?? "";
                    btnCommitEnable = false;
                    btnSendReadEnable = false;
                    btnBackEnable = false;
                    spr1 = false;
                    spr2 = false;
                }
                #endregion
            }
            #endregion

            #region 审批流节点
            ds_flow_node = await data.GetDataSetByParam(@"Select t.w_n,
                   t.Appr_Flow_Id,
                   t.Curr_Rect,
                   t.Doc_Id,
                   t.Curr_Node_Code,
                   Decode(t.Curr_Node_Type, 'start', '开始', t.Curr_Node_Name) Curr_Node_Name,
                   t.Curr_Node_Type,
                   Decode(t.Curr_Node_Type, 'node', 'img', 'oval') Shape,
                   Decode(t.Curr_Node_Type, 'node', 75, 20) Width,
                   Decode(t.Curr_Node_Type, 'node', 70, 20) Height
              From App_Workflow_Appr_Tran_Temp t
             Where t.Doc_Id = :docid
               And Nvl(t.Appr_Type_Id, :type_id) = :type_id1
               And t.Appr_Flow_Id = :flow_id
               And t.Curr_Node_Type Not In ('rect', 'end')
             Order By t.w_n", new OracleParameter[] { data.MakeInParam(":docid",docid??""),
                data.MakeInParam(":type_id",appr_type_id??""),
            data.MakeInParam(":type_id1",appr_type_id??""),
                data.MakeInParam(":flow_id",appr_flow_id??"")});
            #endregion

            //当前审批节点
            curr_appr_node = await data.GetStringByParam(@"Select Wat.Attribute3 || '_' || Wat.Attribute7
                  From App_Workflow_Appr Wat
                 Where Wat.Attribute4 != 'end'
                   And Wat.Attribute4 != 'start'
                   And Wat.Appr_Flow_Id =:flow_id
                   And Wat.Doc_Id =:doc_id", new OracleParameter[] { 
                data.MakeInParam(":flow_id",appr_flow_id??""),data.MakeInParam(":doc_id",docid??"")});
            if (ds_nextperson.Tables.Count<=0)
            {
                ds_nextperson.Tables.Add(new DataTable());
            }
            try
            {
                var json = new
                {
                    code = 0,
                    msg = msg,
                    apprid = appr_id,//审批id
                    print_link = print_link,//打印流程的链接
                    appr_type_id = appr_type_id,//审批流类型ID
                    title_name = title_name,//文档标题
                    doc_link = doc_link,//文档超链接
                    doc_des = doc_des,//文档说明
                    fbr = fbr,//发布人
                    fbdw = fbdw,//发布单位
                    fbsj = fbsj,//发布时间
                    lxdh = lxdh,//联系电话
                    doc_descReadOnly = doc_descReadOnly,//文档内容只读
                    btnCommitEnable = btnCommitEnable,//提交按钮启用
                    btnSendReadEnable = btnSendReadEnable,//传阅按钮启用
                    btnBackEnable = btnBackEnable,//退回按钮启用
                    btnTakeBackEnable = btnTakeBackEnable,//收回按钮启用
                    spr1 = spr1,//是否显示自定义人员
                    spr2 = spr2,//是否显示下一审批人
                    nextpersonid = nextpersonid,//下一审批人id
                    nextpersonname = nextpersonname,//下一审批人名称
                    ds_thr = ds_thr.Tables[0],//退回人集合
                    zhspr = zhspr,//审批人id
                    dqspr = dqspr,//当前审批人
                    ds_tran = ds_tran.Tables[0],//事务记录集合
                    ds_info = ds_info.Tables[0],//审批信息集合
                    ds_nextperson = ds_nextperson.Tables[0],//下一审批人集合
                    nextnode = nextnode,//下一环节
                    btnCommitText = btnCommitText,//提交按钮文本
                    tran_id = tran_id,//事务id	
                    xtmc = xtmc,//系统名称
                    appr_flow_id = appr_flow_id,//审批流类型id
                    attribute3 = attribute3,
                    attribute4 = attribute4,
                    attribute5 = attribute5,
                    dqjd = dqjd,//当前节点
                    upper_node = upper_node,//上级节点
                    upper_rect = upper_rect,//上级节点类型
                    curr_rect = curr_rect,//当前节点类型
                    tjjd = tjjd,//提交节点
                    tjjdlx = tjjdlx,//提交节点类型
                    curr_node_type = curr_node_type,//当前节点类型
                    curr_node = curr_node,//当前节点
                    next_node_type = next_node_type,//下一节点类型
                    next_code = next_code,//下一节点
                    ds_flow_node = ds_flow_node.Tables[0],//该审批流的节点
                    curr_appr_node = curr_appr_node//当前审批节点
                };
                return Json(json);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #region 提交审批
        [HttpPost]
        public async Task<IActionResult> CommitAppr(string appr_flow_id,string docid,string person_id,string spyj,string status,string appr_id,string curr_rect,string title,string des,string curr_node_type,string curr_node,string next_node_type,string next_node)
        {
            var n = await data.GetStringByParam(@"select count(*) from APP_WORKFLOW_APPR_TRAN_TEMP t
                                        where t.appr_flow_id =:appr_flow_id and t.doc_id=:docid and t.curr_node_type='end'",
                                        new OracleParameter[] { data.MakeInParam(":appr_flow_id",appr_flow_id??""),data.MakeInParam(":docid",docid??"")});
            if (n=="0")
            {
                return Json(new { code = 300, msg = "对应审批流未定义，请联系本部门管理员！", returnStatus = "" });
            }
            string msg = "";
            if (status=="New")
            {
                OracleParameter[] prams ={
                      data.MakeInParam("v_Appr_ID",appr_id??""),
                       data.MakeInParam("V_Doc_ID",docid??""),
                       data.MakeInParam("v_Appr_Flow_ID",appr_flow_id??""),
                       data.MakeInParam("v_Curr_Rect",curr_rect??""),
                       data.MakeInParam("v_Doc_Title",title??""),
                        data.MakeInParam("v_Doc_Note",des??""),
                       data.MakeInParam("v_Appl_Person_ID",HttpContext.Session.GetString("PERSON_ID")),
                       data.MakeInParam("v_Appl_dept_id",HttpContext.Session.GetString("DEPT_ID") ),
                       data.MakeInParam("v_Appr_Person_ID",person_id??""),
                        data.MakeInParam("v_Appl_Node_Type",curr_node_type??"" ),
                       data.MakeInParam("v_Appl_Node_Code",curr_node??""),
                       data.MakeInParam("v_Appr_Node_Type",next_node_type??"" ),
                       data.MakeInParam("v_Appr_Node_Code",next_node??""),
                        data.MakeInParam("v_APPR_NOTE",spyj==null?"":(spyj=="null"?"":spyj)),
                         data.MakeInParam("v_CREATED_BY",HttpContext.Session.GetString("USER_ID")),
                data.MakeInParam("p_inf",OracleDbType.Varchar2,100,ParameterDirection.Output)};
                msg = await data.RunProcStr("APP_WolkFlow_Appr_pag.APP_WolkFlow_Start", prams);
            }
            else
            {
                var zt = "";
                switch (next_node_type ?? "")
                {
                    case "node":
                        zt = "1";
                        break;
                    case "end":
                        zt = "2";
                        break;
                    default:
                        zt = "1";
                        break;
                }
                OracleParameter[] prams ={
                       data.MakeInParam("v_Appr_ID",appr_id??""),
                       data.MakeInParam("v_STATUS",zt),
                       data.MakeInParam("v_Appr_Person_ID",person_id??""),
                       data.MakeInParam("v_Appl_Node_Type",curr_node_type??"" ),
                       data.MakeInParam("v_Appl_Node_Code",curr_node??""),
                       data.MakeInParam("v_Appr_Node_Type",next_node_type??"" ),
                       data.MakeInParam("v_Appr_Node_Code",next_node??""),
                       data.MakeInParam("v_APPR_NOTE",spyj==null?"":(spyj=="null"?"":spyj)),
                       data.MakeInParam("v_dqyh",HttpContext.Session.GetString("PERSON_ID")),
                       data.MakeInParam("p_Upper_Node",""),
                       data.MakeInParam("p_Upper_Rect",""),
                       data.MakeInParam("p_Curr_Rect",curr_rect??""),
                       data.MakeInParam("v_CREATED_BY",HttpContext.Session.GetString("USER_ID")),
                       data.MakeInParam("p_inf",OracleDbType.Varchar2,100,ParameterDirection.Output)
                             };
                msg =await data.RunProcStr("APP_WolkFlow_Appr_pag.WolkFlow_Next", prams);
            }
            var st = 400;
            switch (msg)
            {
                case "审批通过！": st = 200;break;
                case "审批结束！": st = 200; break;
                case "退回成功！": st = 200; break;
                case "收回成功！": st = 200; break;
                case "提交成功！": st = 200; break;
            }
            return Json(new { code = st, msg = msg, returnStatus = "view" });
        }
        #endregion

        #region 审批退回
        [HttpPost]
        public async Task<IActionResult> BackAppr(string appr_flow_id, string apprid,string thr,string spyj)
        {
            var zt = 3;
            var spr = "";
            var node_code = "";
            var node_type = "";
            var flow_id = "";
            if (thr!= "-1")
            {
                zt = 5;
                string[] a = thr.Split('/');
                spr = a[0];
                node_code = a[1];
                node_type = "node";
                flow_id = a[2];
            }
            else
            {
                flow_id = appr_flow_id;
            }
            OracleParameter[] prams ={
                      data.MakeInParam("v_Appr_ID",apprid),
                      data.MakeInParam("v_STATUS",zt),
                      data.MakeInParam("v_Appr_Person_ID",spr??""),
                      data.MakeInParam("v_Appl_Node_Type","" ),
                      data.MakeInParam("v_Appl_Node_Code",""),
                      data.MakeInParam("v_Appr_Node_Type",node_type??""),
                      data.MakeInParam("v_Appr_Node_Code",node_code??""),
                      data.MakeInParam("v_APPR_NOTE",spyj==null?"":(spyj=="null"?"":spyj)),
                      data.MakeInParam("v_dqyh",HttpContext.Session.GetString("PERSON_ID")),
                       data.MakeInParam("p_Upper_Node",""),
                    data.MakeInParam("p_Upper_Rect",""),
                    data.MakeInParam("p_Curr_Rect",flow_id),
                    data.MakeInParam("v_CREATED_BY",HttpContext.Session.GetString("USER_ID")),
                data.MakeInParam("p_inf",OracleDbType.Varchar2,100,ParameterDirection.Output),
                             };
            var msg =await data.RunProcStr("APP_WolkFlow_Appr_pag.WolkFlow_Next", prams);
            var st = 400;
            switch (msg)
            {
                case "审批通过！": st = 200; break;
                case "审批结束！": st = 200; break;
                case "退回成功！": st = 200; break;
                case "收回成功！": st = 200; break;
                case "提交成功！": st = 200; break;
                default: st = 300; break;
            }
            return Json(new { code = st, msg = msg, returnStatus = "view" });
        }
        #endregion

        #region 审批收回
        [HttpPost]
        public async Task<IActionResult> TakeBackAppr( string apprid, string tjjdlx,string tjjd, string spyj,string upper_node,string upper_rect,string curr_rect)
        {
            
            OracleParameter[] prams ={
                       data.MakeInParam("v_Appr_ID",apprid??""),
                       data.MakeInParam("v_STATUS","4"),
                       data.MakeInParam("v_Appr_Person_ID",HttpContext.Session.GetString("PERSON_ID")),
                       data.MakeInParam("v_Appl_Node_Type",tjjdlx??"" ),
                       data.MakeInParam("v_Appl_Node_Code",tjjd??""),
                       data.MakeInParam("v_Appr_Node_Type",tjjdlx??"" ),
                       data.MakeInParam("v_Appr_Node_Code",tjjd??""),
                       data.MakeInParam("v_APPR_NOTE",(spyj==null?"":(spyj=="null"?"":spyj))),
                       data.MakeInParam("v_dqyh",HttpContext.Session.GetString("PERSON_ID")),
                       data.MakeInParam("p_Upper_Node",upper_node??""),
                       data.MakeInParam("p_Upper_Rect",upper_rect??""),
                       data.MakeInParam("p_Curr_Rect",curr_rect??""),
                       data.MakeInParam("v_CREATED_BY",HttpContext.Session.GetString("USER_ID")),
                       data.MakeInParam("p_inf",OracleDbType.Varchar2,100,ParameterDirection.Output)
                             };
            var msg = await data.RunProcStr("APP_WolkFlow_Appr_pag.WolkFlow_Next", prams);
            var st = 400;
            switch (msg)
            {
                case "审批通过！": st = 200; break;
                case "审批结束！": st = 200; break;
                case "退回成功！": st = 200; break;
                case "收回成功！": st = 200; break;
                case "提交成功！": st = 200; break;
                default: st = 300; break;
            }
            return Json(new { code = st, msg = msg, returnStatus = "view" });
        }
        #endregion
    }
}