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

namespace WebApplication1.Controllers.FixValue
{
    public class FixValueController : Controller
    {
        DBHelper data = new DBHelper();

        [CheckCustomer]
        public IActionResult FixValue()
        {
            return View();
        }

        [CheckCustomer]
        public ActionResult FixValueTypeEdit()
        {
            ViewBag.status = HttpContext.Request.Query["status"];
            ViewBag.type_id =ViewBag.status=="add"?"": HttpContext.Request.Query["Rowid"].ToString();
            return View();
        }

        [CheckCustomer]
        public ActionResult FixValueEdit()
        {
            ViewBag.status = HttpContext.Request.Query["status"];
            ViewBag.type_id= HttpContext.Request.Query["type_id"];
            ViewBag.fixvalue_id = ViewBag.status == "add" ? "" : HttpContext.Request.Query["Rowid"].ToString();
            return View();
        }

        #region 获取公共代码类型
        [HttpPost]
        public async Task<IActionResult> GetFixValueType(string fixvalue, int page, int limit)
        {
            string Msg = "";
            string sqlpj = fixvalue == null ? " " : @" where (t.fixvalue_type_code like '%' || :code  || '%' or t.fixvalue_type_name like '%' || :name || '%')";
            string sqlpj1 = fixvalue == null ? " " : @" where (t.fixvalue_type_code like '%' || :code1  || '%' or t.fixvalue_type_name like '%' || :name1 || '%')";
            string sql = @"select * from (select rownum as rowno,r.* from(select (select count(*) from App_Fixvalue_Type "+sqlpj1+@") totalPage,
                                            t.fixvalue_type_id, t.fixvalue_type_code, t.fixvalue_type_name
                                             from App_Fixvalue_Type t " + sqlpj+@") r
                                          where rownum<= :page * :limit) table_alias
                                      where table_alias.rowno>( :page - 1) * :limit";
            OracleParameter[] sp = fixvalue == null ? 
                new OracleParameter[] {
                    data.MakeInParam(":page", page),
                    data.MakeInParam(":limit", limit)
                } 
            :
                new OracleParameter[] {
                    data.MakeInParam(":code1", fixvalue??""),
                    data.MakeInParam(":name1", fixvalue??""),
                    data.MakeInParam(":code", fixvalue??""),
                    data.MakeInParam(":name", fixvalue??""),
                    data.MakeInParam(":page", page),
                    data.MakeInParam(":limit", limit) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 获取公共代码
        [HttpPost]
        public async Task<IActionResult> GetFixValue(string fixvalue,string fixvaluetypeid, int page, int limit)
        {
            if (fixvaluetypeid==null)
            {
                return Content("{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}");
            }
            string Msg = "";
            string sqlpj = fixvalue == null ? " " : @" and (f.fixvalue_code like '%' || :code || '%' or  f.fixvalue_name like '%' || :name || '%')";
            string sqlpj1 = fixvalue == null ? " " : @" and (f.fixvalue_code like '%' || :code1 || '%' or  f.fixvalue_name like '%' || :name1 || '%')";
            string sql = @"select * from (select rownum as rowno,r.* from(select (select count(*)  from app_fixvalue f, app_fixvalue_type ft
                                        where f.fixvalue_type_id = ft.fixvalue_type_id "+sqlpj1+@" and ft.fixvalue_type_id=:type_id1) totalPage,
                                        f.fixvalue_id, f.fixvalue_code, f.fixvalue_name, f.note,f.fixvalue_type_id
                                         from app_fixvalue f, app_fixvalue_type ft
                                        where f.fixvalue_type_id = ft.fixvalue_type_id
                                          " + sqlpj+@"
                                          and ft.fixvalue_type_id = :type_id) r where rownum<= :page * :limit) table_alias where table_alias.rowno>( :page - 1) * :limit";
            OracleParameter[] sp = fixvalue == null ? 
                new OracleParameter[] {
                    data.MakeInParam(":type_id1", fixvaluetypeid),
                    data.MakeInParam(":type_id", fixvaluetypeid),
                    data.MakeInParam(":page", page),
                    data.MakeInParam(":limit", limit)
                    } 
                : 
                new OracleParameter[] {
                    data.MakeInParam(":code1", fixvalue??""),
                    data.MakeInParam(":name1", fixvalue??""),
                    data.MakeInParam(":type_id1", fixvaluetypeid),
                    data.MakeInParam(":code", fixvalue??""),
                    data.MakeInParam(":name", fixvalue??""),
                    data.MakeInParam(":type_id", fixvaluetypeid),
                    data.MakeInParam(":page", page),
                    data.MakeInParam(":limit", limit)
                    };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 删除类型
        [HttpPost]
        public async Task<IActionResult> DeleteType(string[] type_id)
        {
            string Msg = "";
            Hashtable ht = new Hashtable();
            for (int i = 0; i < type_id.Length; i++)
            {
                ht.Add(@"delete from app_fixvalue where fixvalue_type_id=:type_id"+i, new OracleParameter[] { data.MakeInParam(":type_id" + i, type_id[i]) });
                ht.Add(@"delete from app_fixvalue_type where fixvalue_type_id=:type_id"+i,new OracleParameter[] { data.MakeInParam(":type_id"+i,type_id[i])});
            }
            if (ht.Count>0)
            {
                //string sql = "declare type_id varchar2(100);str2 varchar2(3000);str3 varchar2(3000); begin type_id:=:type_id;str2:='delete from app_fixvalue_type where fixvalue_type_id in('||type_id||')';str3:='delete from app_fixvalue where fixvalue_type_id in('||type_id||')'; execute immediate str2;execute immediate str3; end;";
                //OracleParameter[] sp = { data.MakeInParam(":type_id", OracleDbType.Varchar2, 100, type_id) };
                bool flag = await data.DoSqlList(ht);// data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"删除成功\"}" : "{\"code\":300,\"msg\":\"删除失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中类型\"}");
            }
           
        }
        #endregion

        #region 删除代码
        [HttpPost]
        public async Task<IActionResult> DeleteFixValue(string[] fixvalue_id)
        {
            string Msg = "";
            Hashtable ht = new Hashtable();
            for (int i = 0; i < fixvalue_id.Length; i++)
            {
                ht.Add(@"delete from app_fixvalue where fixvalue_id=:fixvalue_id" + i, new OracleParameter[] { data.MakeInParam(":fixvalue_id" + i, fixvalue_id[i]) });
            }
            if (ht.Count>0)
            {
                //string sql = "declare fixvalue_id varchar2(100);str3 varchar2(3000); begin fixvalue_id:=:fixvalue_id;str3:='delete from app_fixvalue where fixvalue_id in('||fixvalue_id||')'; execute immediate str3; end;";
                //OracleParameter[] sp = { data.MakeInParam(":fixvalue_id", OracleDbType.Varchar2, 100, fixvalue_id) };
                bool flag = await data.DoSqlList(ht);// data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"删除成功\"}" : "{\"code\":300,\"msg\":\"删除失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中值\"}");
            }

        }
        #endregion

        #region 新增公共代码
        [HttpPost]
        public async Task<IActionResult> InsertFixValue(string code,string name,string note,string type_id)
        {
            string Msg = "";
            string sql = @"insert into app_fixvalue (fixvalue_id,  fixvalue_type_id,  fixvalue_code,  fixvalue_name,  note,  last_update_date,  last_updated_by,  creation_date,  created_by)
                            values (app_fixvalue_s.nextval,  :type_id,  :code,  :name,  :note,  sysdate,  :user_id,  sysdate,  :user_id)";
            OracleParameter[] sp = { data.MakeInParam(":type_id",type_id),data.MakeInParam(":code", code ?? ""), data.MakeInParam(":name", name), data.MakeInParam(":note",note??""), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 编辑公共代码
        [HttpPost]
        public async Task<IActionResult> ModifyFixValue(string code, string name, string note, string fixvalue_id)
        {
            if (fixvalue_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string Msg = "";
            string sql = @"update app_fixvalue
                           set fixvalue_code    = :code,
                               fixvalue_name    = :name,
                               note             = :note,
                               last_update_date = sysdate,
                               last_updated_by  = :user_id
                         where fixvalue_id = :fixvalue_id";
            OracleParameter[] sp = { data.MakeInParam(":code", code ?? ""), data.MakeInParam(":name", name), data.MakeInParam(":note", note ?? ""), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) ,data.MakeInParam(":fixvalue_id", fixvalue_id) };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 新增公共代码类型
        [HttpPost]
        public async Task<IActionResult> InsertFixValueType(string code, string name )
        {
            string Msg = "";
            string sql = @"insert into app_fixvalue_type(fixvalue_type_id, fixvalue_type_code, fixvalue_type_name, last_update_date, last_updated_by, creation_date, created_by)
                            values(app_fixvalue_type_s.nextval, :code, :name, sysdate, :user_id, sysdate, :user_id)";
            OracleParameter[] sp = {  data.MakeInParam(":code", code ?? ""), data.MakeInParam(":name", name??""),  data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 编辑公共代码类型
        [HttpPost]
        public async Task<IActionResult> ModifyFixValueType(string code, string name, string type_id)
        {
            if (type_id==null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string Msg = "";
            string sql = @"update app_fixvalue_type
                                   set fixvalue_type_code = :code,
                                       fixvalue_type_name = :name,
                                       last_update_date = sysdate,
                                       last_updated_by = :user_id
                                 where fixvalue_type_id = :type_id";
            OracleParameter[] sp = { data.MakeInParam(":code", code ?? ""), data.MakeInParam(":name", name??""), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")),data.MakeInParam(":type_id",type_id) };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 根据ID获取公共代码
        [HttpPost]
        public async Task<IActionResult> GetFixValueById(string fixvalue_id)
        {
            if (fixvalue_id == null)
            {
                return Content("{\"code\":1,\"msg\":\"请传入关键值\",\"count\":0,\"data\":[]}");
            }
            string Msg = "";
            string sql = @"select f.fixvalue_id, f.fixvalue_code, f.fixvalue_name, f.note,f.fixvalue_type_id from app_fixvalue f where fixvalue_id=:fixvalue_id";
            OracleParameter[] sp =  { data.MakeInParam(":fixvalue_id", fixvalue_id) } ;
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 根据ID获取公共代码类型
        [HttpPost]
        public async Task<IActionResult> GetFixValueTypeById(string type_id)
        {
            if (type_id == null)
            {
                return Content("{\"code\":1,\"msg\":\"请传入关键值\",\"count\":0,\"data\":[]}");
            }
            string Msg = "";
            string sql = @"select fixvalue_type_id, fixvalue_type_code, fixvalue_type_name  from app_fixvalue_type where fixvalue_type_id=:type_id";
            OracleParameter[] sp = { data.MakeInParam(":type_id", type_id) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion
    }
}