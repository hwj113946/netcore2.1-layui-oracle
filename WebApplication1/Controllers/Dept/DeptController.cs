using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using WebApplication1.Models;

namespace WebApplication1.Controllers.Dept
{
    public class DeptController : Controller
    {
        DBHelper data = new DBHelper();

        [CheckCustomer]
        public IActionResult Dept()
        {
            ViewBag.CORP_ID = HttpContext.Session.GetString("CORP_ID");
            return View();
        }

        [CheckCustomer]
        public IActionResult EditDept()
        {
            ViewBag.status = HttpContext.Request.Query["status"];
            ViewBag.dept_id = ViewBag.status == "add" ? "" : HttpContext.Request.Query["Rowid"].ToString();
            return View();
        }

        #region 根据公司获取部门
        [HttpPost]
        public async Task<IActionResult> GetDeptByCorp(int limit, int page, string dept_name, string corp_id, string status)
        {
            string msg = "";
            string sql = @"SELECT *
                  FROM (SELECT ROWNUM AS rowno, r.*
                          FROM (select t.Dept_Id,
                       c.Corp_Name,
                       t.Dept_Code,
                       t.Dept_Name,
                       Decode(t.Status, 0, '有效', 1, '无效') Status
                  From App_Dept t, App_Corp c
                 Where (t.Dept_Code Like '%' || :dept_code|| '%' Or t.Dept_Name Like '%' || :dept_name || '%') 
                   And t.Corp_Id = c.Corp_Id(+)
                   And t.Status = :status
                   And t.Corp_Id = :corp_id) r
                 where ROWNUM <= :page * :limit) table_alias
             WHERE table_alias.rowno > (:page - 1) * :limit";
            string sql1 = @"select count(*)
                  From App_Dept t, App_Corp c
                 Where (t.Dept_Code Like '%' || :dept_code|| '%' Or t.Dept_Name Like '%' || :dept_name || '%') 
                   And t.Corp_Id = c.Corp_Id(+)
                   And t.Status = :status
                   And t.Corp_Id = :corp_id";
            OracleParameter[] sp1 = {
                                    data.MakeInParam(":dept_code",dept_name??""),
                                    data.MakeInParam(":dept_name",dept_name??""),
                                    data.MakeInParam(":status",status),
                                    data.MakeInParam(":corp_id",corp_id??"")};
            string n = await data.GetStringByParam(sql1,sp1);
            OracleParameter[] sp = {
                                    data.MakeInParam(":dept_code",dept_name??""),
                                    data.MakeInParam(":dept_name",dept_name??""),
                                    data.MakeInParam(":status",status),
                                    data.MakeInParam(":corp_id",corp_id??""),
                                    data.MakeInParam(":page",page),data.MakeInParam(":limit",limit)};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + n + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(msg);
        }
        #endregion

        #region 删除部门
        [HttpPost]
        public async Task<IActionResult> DeleteDept(string[] id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"delete from app_dept where dept_id=:dept_id" + i, new OracleParameter[] { data.MakeInParam(":dept_id" + i, id[i]) });
            }
            if (ht.Count>0)
            {
                //string sql = "declare str varchar2(1000);str2 varchar2(3000); begin str:=:dept_id;str2:='delete from app_dept where dept_id in('||str||')'; execute immediate str2; end;";
                //OracleParameter[] sp = { data.MakeInParam(":dept_id", OracleDbType.Varchar2, 3000, id) };
                bool flag = await data.DoSqlList(ht);// data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"删除成功\"}" : "{\"code\":300,\"msg\":\"删除失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中部门\"}");
            }
            
        }
        #endregion

        #region 启用部门
        [HttpPost]
        public async Task<IActionResult> EnableStatusForDept(string[] id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"Update App_Dept
                           Set Status           = 0,
                               Last_Update_Date = Sysdate,
                               Last_Updated_By  = :Last_Updated_By
                         Where Dept_Id = :Dept_Id" + i, new OracleParameter[] 
                       {
                           data.MakeInParam(":Last_Updated_By" + i, HttpContext.Session.GetString("USER_ID")),
                           data.MakeInParam(":Dept_Id" + i, id[i])
                       });
            }
            if (ht.Count>0)
            {
                //string sql = "declare str varchar2(1000);user_id varchar2(100);str2 varchar2(3000); begin str:=:dept_id;user_id:=:user_id;str2:='update app_dept set status=1,last_update_date=sysdate,LAST_UPDATED_BY='||user_id||' where dept_id in('||str||')'; execute immediate str2; end;";
                //OracleParameter[] sp = { data.MakeInParam(":dept_id", OracleDbType.Varchar2, 3000, id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
                bool flag = await data.DoSqlList(ht);// data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"启用成功\"}" : "{\"code\":300,\"msg\":\"启用失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中部门\"}");
            }
           
        }
        #endregion

        #region 失效部门
        [HttpPost]
        public async Task<IActionResult> FailureStatusForDept(string[] id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"Update App_Dept
                           Set Status           = 1,
                               Last_Update_Date = Sysdate,
                               Last_Updated_By  = :Last_Updated_By
                         Where Dept_Id = :Dept_Id" + i, new OracleParameter[]
                       {
                           data.MakeInParam(":Last_Updated_By" + i, HttpContext.Session.GetString("USER_ID")),
                           data.MakeInParam(":Dept_Id" + i, id[i])
                       });
            }
            if (ht.Count>0)
            {
                //string sql = "declare str varchar2(1000);user_id varchar2(100);str2 varchar2(3000); begin str:=:dept_id;user_id:=:user_id;str2:='update app_dept set status=1,last_update_date=sysdate,LAST_UPDATED_BY='||user_id||' where dept_id in('||str||')'; execute immediate str2; end;";
                //OracleParameter[] sp = { data.MakeInParam(":corp_id", OracleDbType.Varchar2, 3000, id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
                bool flag = await data.DoSqlList(ht);// data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"失效成功\"}" : "{\"code\":300,\"msg\":\"失效失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中部门\"}");
            }

        }
        #endregion

        #region 根据部门ID获取部门
        [HttpPost]
        public async Task<IActionResult> GetDeptById(string dept_id)
        {
            string Msg = "";
            if (dept_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"select t.corp_id,t.dept_id,t.dept_code,t.dept_name from app_dept t where t.dept_id=:dept_id";
            OracleParameter[] sp = { data.MakeInParam(":dept_id", dept_id) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 新增部门
        [HttpPost]
        public async Task<IActionResult> Insert(string corp_id, string dept_code, string dept_name)
        {
            string Msg = "";
            string sql = @"Insert Into App_Dept
                          (Dept_Id,
                           Corp_Id,
                           Dept_Code,
                           Dept_Name,
                           Status,
                           Creation_Date,
                           Created_By)
                        Values
                          (App_Dept_s.Nextval,
                           :Corp_Id,
                           :Dept_Code,
                           :Dept_Name,
                           0,
                           Sysdate,
                           :User_Id)";
            OracleParameter[] sp = { data.MakeInParam(":corp_id", corp_id),
                                        data.MakeInParam(":dept_code",dept_code ),
                                        data.MakeInParam(":dept_name",dept_name ),
                                        data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID") )};
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 修改
        [HttpPost]
        public async Task<IActionResult> Modify(string dept_code, string dept_name, string corp_id,string dept_id)
        {
            string Msg = "";
            if (corp_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"Update App_dept
                                       Set dept_code        = :dept_code,
                                           dept_name        = :dept_name,
                                           corp_id = :corp_id,
                                           Last_Update_Date = Sysdate,
                                           Last_Updated_By  = :user_id
                                     Where dept_id = :dept_id";
            OracleParameter[] sp = { data.MakeInParam(":dept_code",dept_code??""),
                                     data.MakeInParam(":dept_name", dept_name??""),
                                     data.MakeInParam(":corp_id", corp_id??""),
                                     data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")),
                                     data.MakeInParam(":dept_id",dept_id)};
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 导出：获取部门
        [HttpPost]
        public async Task<IActionResult> GetDeptForExport(string dept_name, string status,string corp_id)
        {
            string sql = @"Select c.Corp_Name,
                                   t.Dept_Code,
                                   t.Dept_Name,
                                   Decode(t.Status, 0, '有效', 1, '无效') Status
                              From App_Dept t, App_Corp c
                             Where t.Corp_Id = c.Corp_Id(+)
                               And (t.Dept_Code Like '%' || :dept_code || '%' Or
                                   t.Dept_Name Like '%' || :dept_name || '%')
                               And t.Status = :status
                               And t.Corp_Id = :corp_id";
            OracleParameter[] sp = { data.MakeInParam(":dept_code", dept_name ?? ""),
                data.MakeInParam(":dept_name", dept_name ?? ""),
                data.MakeInParam(":status", status ?? ""),
                data.MakeInParam(":corp_id", corp_id??"") };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            List<ExcelDept> dept = data.DataSetToIList1<ExcelDept>(ds, 0).ToList();
            byte[] buffer = ExcelHelper.Export(dept, "部门", ExcelTitle.Dept).GetBuffer();
            return File(buffer, "application/ms-excel", "部门数据导出.xls");
        }
        #endregion
    }
}