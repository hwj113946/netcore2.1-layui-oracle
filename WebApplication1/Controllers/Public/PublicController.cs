using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using WebApplication1.Models;


namespace WebApplication1.Controllers.Public
{
    public class PublicController : Controller
    {
        DBHelper data = new DBHelper();

        [CheckCustomer]
        public IActionResult GetPerson()
        {
            ViewBag.CORP_ID = HttpContext.Session.GetString("CORP_ID");
            ViewBag.DEPT_ID = HttpContext.Session.GetString("DEPT_ID");
            ViewBag.POST_ID = HttpContext.Session.GetString("POST_ID");
            return View();
        }

        [CheckCustomer]
        public IActionResult GenerateCode()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult ChooseSinglePerson()
        {
            ViewBag.CORP_ID = HttpContext.Session.GetString("CORP_ID");
            ViewBag.DEPT_ID = HttpContext.Session.GetString("DEPT_ID");
            ViewBag.POST_ID = HttpContext.Session.GetString("POST_ID");
            return View();
        }

        #region 获取角色菜单按钮
        [HttpPost]
        public async Task<IActionResult> GetButton(string menu_id)
        {
            string Msg = "";
            if (menu_id==null)
            {
                return Content("{\"code\":1,\"msg\":\"请传入菜单主键\"}");
            }
            string sql = @"select distinct b.button_id,   b.button_name,   b.button_icon,   b.button_event,   b.button_sort,   b.attribute1,b.attribute2
                             from app_button b, app_role_button rb where b.button_id = rb.button_id   and rb.attribute1 = :menu_id   And Rb.Role_Id In
       (Select Role_Id From App_User_Role Where User_Id = :user_id) order by b.button_sort";
            OracleParameter[] sp = { data.MakeInParam(":menu_id", menu_id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count+ ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"当前登录角色在此菜单未分配按钮\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 获取公司
        [HttpPost]
        public async Task<IActionResult> GetCorp()
        {
            string sql = @"select corp_id,corp_code,corp_name from app_corp where status=1";
            DataSet ds = await data.GetDataSet(sql);
            string msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未获取到数据\",\"count\":0,\"data\":[]}";
            return Content(msg);
        }
        #endregion

        #region 根据公司获取部门
        [HttpPost]
        public async Task<IActionResult> GetDeptByCorp(string corp_id)
        {
            if (corp_id == null)
            {
                return Content("{\"code\":1,\"msg\":\"请传入公司主键\"}");
            }
            string sql = @"select dept_id,dept_code,dept_name from app_dept where status=0 And corp_id=:corp_id";
            OracleParameter[] sp = { data.MakeInParam(":corp_id",corp_id??"")};
            DataSet ds = await data.GetDataSetByParam(sql,sp);
            string msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未获取到数据\",\"count\":0,\"data\":[]}";
            return Content(msg);
        }
        #endregion

        #region 获取人员
        [HttpPost]
        public async Task<IActionResult> GetSinglePerson(int limit, int page, string person_name, string dept_id, string corp_id, string post_id)
        {
            string msg = "";
            corp_id = corp_id ?? HttpContext.Session.GetString("CORP_ID");
            string pj = (corp_id != null && dept_id == null) ? " and d.corp_id=:Corp_Id "
                : ((corp_id != null && dept_id != null && post_id == null) ? " and t.dept_id=:Dept_Id "
                : ((corp_id != null && dept_id != null && post_id != null)
                ? " and t.dept_id=:Dept_Id and t.post_id=:Post_Id " : ""));
            string sql = @"SELECT *
                  FROM (SELECT ROWNUM AS rowno, r.*
                          FROM (select t.Person_Id,
                               t.Person_Code,
                               t.Person_Name,
                               t.Id_Card_Number,
                               t.Sex,
                               t.Mobile_Phone,
                               t.Fixed_Phone,
                               t.Email,
                               c.Corp_Name,
                               d.Dept_Name,
                               p.Post_Name
                          From App_Person t, App_Corp c, App_Dept d, App_Posts p
                         Where t.Dept_Id = d.Dept_Id
                           And d.Corp_Id = c.Corp_Id
                           And t.Post_Id = p.Post_Id
                           And t.status=0
                           and (t.person_code like '%'|| :person_code ||'%' or t.person_name like '%'|| :person_name || '%')
                           " + pj + @" ) r
                 where ROWNUM <= :page * :limit) table_alias
             WHERE table_alias.rowno > (:page - 1) * :limit";
            string sql1 = @"select count(*)
                          From App_Person t, App_Corp c, App_Dept d, App_Posts p
                         Where t.Dept_Id = d.Dept_Id
                           And d.Corp_Id = c.Corp_Id
                           And t.Post_Id = p.Post_Id
                           And t.Status = 0
                           and (t.person_code like '%'|| :person_code ||'%' or t.person_name like '%'|| :person_name || '%')
                           " + pj;
            OracleParameter[] sp2 = (corp_id != null && dept_id == null) ?
                new OracleParameter[] {
                    
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":Corp_Id",corp_id??"")}
                : ((corp_id != null && dept_id != null && post_id == null) ?
                new OracleParameter[] {
                    
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":Dept_Id",dept_id??"")}
                : ((corp_id != null && dept_id != null && post_id != null) ?
                new OracleParameter[] {
                    
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":Dept_Id",dept_id??""),
                data.MakeInParam(":Post_Id",post_id??"") } :
                new OracleParameter[] {
                    
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??"")}
                ));
            string n = await data.GetStringByParam(sql1, sp2);
            OracleParameter[] sp1 = (corp_id != null && dept_id == null) ?
                new OracleParameter[] {
                
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":Corp_Id",corp_id??""),
                data.MakeInParam(":page", page),data.MakeInParam(":limit", limit)}
                : ((corp_id != null && dept_id != null && post_id == null) ?
                new OracleParameter[] {
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":Dept_Id",dept_id??""),
                data.MakeInParam(":page", page),data.MakeInParam(":limit", limit)}
                : ((corp_id != null && dept_id != null && post_id != null) ?
                new OracleParameter[] {
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":Dept_Id",dept_id??""),
                data.MakeInParam(":Post_Id",post_id??""),
                data.MakeInParam(":page", page),data.MakeInParam(":limit", limit) } :
                new OracleParameter[] {
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":page", page),data.MakeInParam(":limit", limit) }
                ));
            DataSet ds = await data.GetDataSetByParam(sql, sp1);
            msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + n + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(msg);
        }
        #endregion

        #region 获取数据表的字段
        [HttpPost]
        public async Task<IActionResult> GetColByTableName(string table_name)
        {
            string Msg = "";
            if (table_name == null)
            {
                return Content("{\"code\":1,\"msg\":\"请传入数据表名称\"}");
            }
            string sql = @"Select Utc.Column_Name, Utc.Data_Type, Ucc.Comments, Utc.Table_Name
                              From User_Tab_Columns Utc, User_Col_Comments Ucc
                             Where Utc.Column_Name = Ucc.Column_Name
                               And Utc.Table_Name = Ucc.Table_Name
                               And Utc.Table_Name = :table_name
                             Order By Utc.Column_Id";
            OracleParameter[] sp = { data.MakeInParam(":table_name", table_name) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 获取数据表
        [HttpPost]
        public async Task<IActionResult> GetCurrentUserDataTable()
        {
            string Msg = "";
            string sql = @"Select a.Table_Name, b.Comments From User_Tables a, User_Tab_Comments b Where a.Table_Name = b.Table_Name Order By Table_Name";
            DataSet ds = await data.GetDataSet(sql);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 获取单位
        [HttpPost]
        public async Task<IActionResult> GetDept()
        {
            string Msg = "";
            DataSet ds = new DataSet();
            string sql = @"select t.dept_id id,nvl(t.attribute1,t.dept_name) name from app_dept t where t.status=0 and corp_id=:corp_id order by nvl(t.attribute4,9999),t.dept_name";
            OracleParameter[] sp = { data.MakeInParam(":corp_id",HttpContext.Session.GetString("CORP_ID"))};
            ds = await data.GetDataSetByParam(sql,sp);
            Msg = ds.Tables[0].Rows.Count > 0
                ? "{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}"
                : "{\"code\":0,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 获取岗位
        [HttpPost]
        public async Task<IActionResult> GetPost()
        {
            string Msg = "";
            DataSet ds = new DataSet();
            string sql = @"select f.fixvalue_code id, f.fixvalue_name name
                              from app_fixvalue f, app_fixvalue_type ft
                             where f.fixvalue_type_id = ft.fixvalue_type_id
                             and ft.fixvalue_type_code='APP_POST'";
            ds =await data.GetDataSet(sql);
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
    }
}