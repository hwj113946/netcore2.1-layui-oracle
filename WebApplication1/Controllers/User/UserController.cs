using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace WebApplication1.Controllers.User
{
    public class UserController : Controller
    {
        DBHelper data = new DBHelper();

        [CheckCustomer]
        public IActionResult UserView()
        {
            ViewBag.USER_CODE = HttpContext.Session.GetString("USER_CODE");
            ViewBag.USER_NAME = HttpContext.Session.GetString("USER_NAME");
            ViewBag.SEX = HttpContext.Session.GetString("SEX");
            ViewBag.EMAIL = HttpContext.Session.GetString("EMAIL");
            ViewBag.MOBILE_PHONE = HttpContext.Session.GetString("MOBILE_PHONE");
            ViewBag.ID_CARD_NUMBER = HttpContext.Session.GetString("ID_CARD_NUMBER");
            ViewBag.CORP_NAME = HttpContext.Session.GetString("CORP_NAME");
            ViewBag.DEPT_NAME = HttpContext.Session.GetString("DEPT_NAME");
            ViewBag.POST_NAME = HttpContext.Session.GetString("POST_NAME");
            return View();
        }

        [CheckCustomer]
        public IActionResult User()
        {
            ViewBag.CORP_ID = HttpContext.Session.GetString("CORP_ID");
            ViewBag.DEPT_ID = HttpContext.Session.GetString("DEPT_ID");
            ViewBag.POST_ID = HttpContext.Session.GetString("POST_ID");
            return View();
        }

        [CheckCustomer]
        public IActionResult EditUser()
        {
            ViewBag.status = HttpContext.Request.Query["status"];
            ViewBag.user_id = ViewBag.status == "add" ? "" : HttpContext.Request.Query["Rowid"].ToString();
            return View();
        }

        [CheckCustomer]
        public IActionResult AllotRoleForUser()
        {
            //ViewBag.status = HttpContext.Request.Query["status"];
            ViewBag.user_id = ViewBag.status == "add" ? "" : HttpContext.Request.Query["Rowid"].ToString();
            return View();
        }

        [CheckCustomer]
        public IActionResult ChangePassWord()
        {
            ViewBag.user_id = HttpContext.Session.GetString("USER_ID");
            return View();
        }

        #region 获取用户
        [HttpPost]
        public async Task<IActionResult> GetUserByCorpDeptPost(int limit, int page, string user_name, string dept_id, string corp_id, string post_id, string status)
        {
            string msg = "";
            corp_id = corp_id ?? HttpContext.Session.GetString("CORP_ID");
            string pj = (corp_id != null && dept_id == null) ? " and d.corp_id=:Corp_Id "
                : ((corp_id != null && dept_id != null && post_id == null) ? " and t.dept_id=:Dept_Id "
                : ((corp_id != null && dept_id != null && post_id != null)
                ? " and t.dept_id=:Dept_Id and t.post_id=:Post_Id " : ""));
            string sql = @"SELECT *
                  FROM (SELECT ROWNUM AS rowno, r.*
                          FROM (select ceil(count(t.post_id) over()/ :limit) totalPage,u.user_id,t.Person_Id,
                               u.user_code,
                               u.user_name,
                               t.Sex,
                               t.Mobile_Phone,
                               t.Fixed_Phone,
                               t.Email,
                               c.Corp_Name,
                               d.Dept_Name,
                               p.Post_Name,
                               Decode(u.Status, 0, '有效', 1, '无效') Status
                          From app_user u,App_Person t, App_Corp c, App_Dept d, App_Posts p
                         Where u.person_id=t.person_id(+) and t.Dept_Id = d.Dept_Id(+)
                           And d.Corp_Id = c.Corp_Id(+)
                           And t.Post_Id = p.Post_Id(+)
                           And u.Status = :Status
                           and (u.user_code like '%'|| :user_code ||'%' or u.user_name like '%'|| :user_name || '%')
                           " + pj + @" ) r
                 where ROWNUM <= :page * :limit) table_alias
             WHERE table_alias.rowno > (:page - 1) * :limit";
            OracleParameter[] sp1 = (corp_id != null && dept_id == null) ?
                new OracleParameter[] {
                data.MakeInParam(":limit", limit),
                data.MakeInParam(":status",status),
                data.MakeInParam(":user_code",user_name??""),
                data.MakeInParam(":user_name",user_name??""),
                data.MakeInParam(":Corp_Id",corp_id??""),
                data.MakeInParam(":page", page)}
                : ((corp_id != null && dept_id != null && post_id == null) ?
                new OracleParameter[] {
                data.MakeInParam(":limit", limit),
                data.MakeInParam(":status",status),
                data.MakeInParam(":user_code",user_name??""),
                data.MakeInParam(":user_name",user_name??""),
                data.MakeInParam(":Dept_Id",dept_id??""),
                data.MakeInParam(":page", page)}
                : ((corp_id != null && dept_id != null && post_id != null) ?
                new OracleParameter[] {
                data.MakeInParam(":limit", limit),
                data.MakeInParam(":status",status),
                data.MakeInParam(":user_code",user_name??""),
                data.MakeInParam(":user_name",user_name??""),
                data.MakeInParam(":Dept_Id",dept_id??""),
                data.MakeInParam(":Post_Id",post_id??""),
                data.MakeInParam(":page", page) } :
                new OracleParameter[] {
                data.MakeInParam(":limit", limit),
                data.MakeInParam(":status",status),
                data.MakeInParam(":user_code",user_name??""),
                data.MakeInParam(":user_name",user_name??""),
                data.MakeInParam(":page", page) }
                ));
            DataSet ds = await data.GetDataSetByParam(sql, sp1);
            msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(msg);
        }
        #endregion

        #region 新增用户
        [HttpPost]
        public async Task<IActionResult> Insert(string user_code, string user_name, string person_id)
        {
            string Msg = "";
            OracleParameter[] sp1 = { data.MakeInParam(":person_id", person_id)};
            string n = await data.GetStringByParam(@"select count(*) from app_user where person_id=:person_id",sp1);
            if (n=="0")
            {
                string sql = @"Insert Into App_User
                                      (User_Id,
                                       User_Code,
                                       User_Name,
                                       Password,
                                       Person_Id,
                                       Status,
                                       Creation_Date,
                                       Creation_By)
                                    Values
                                      (App_User_s.Nextval,
                                       :user_code,
                                       :user_name,
                                       :password,
                                       :person_id,
                                       0,
                                       Sysdate,
                                       :user_id)";
                OracleParameter[] sp = {    data.MakeInParam(":user_code", user_code??""),
                                        data.MakeInParam(":user_name",user_name??"" ),
                                        data.MakeInParam(":password",mtTools.EncodeHelper.MD5Hash("111111")??"" ),
                                        data.MakeInParam(":person_id",person_id??"" ),
                                        data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID") )};
                bool flag = await data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            }
            else
            {
                Msg= "{\"code\":300,\"msg\":\"人员已经存在用户表,无法再次选择\"}";
            }            
            return Content(Msg);
        }
        #endregion

        #region 修改
        [HttpPost]
        public async Task<IActionResult> Modify(string user_code, string user_name,  string person_id,string user_id)
        {
            string Msg = "";
            if (user_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            OracleParameter[] sp1 = { data.MakeInParam(":person_id", person_id) };
            string n = await data.GetStringByParam(@"select count(*) from app_user where person_id=:person_id", sp1);
            if (n == "0")
            {
                string sql = @"Update App_User
                               Set User_Code        = :user_code,
                                   User_Name        = :user_name,
                                   Person_Id        = :person_id,
                                   Last_Update_Date = Sysdate,
                                   Last_Update_By   = :user_id
                             Where User_Id = :user_id1";
                OracleParameter[] sp = {    data.MakeInParam(":user_code", user_code??""),
                                        data.MakeInParam(":user_name",user_name??"" ),
                                        data.MakeInParam(":person_id",person_id??"" ),
                                        data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")),
                                        data.MakeInParam(":user_id1",user_id)};
                bool flag = await data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            }
            else
            {
                Msg = "{\"code\":300,\"msg\":\"人员已经存在用户表,无法再次选择\"}";
            }
            return Content(Msg);
        }
        #endregion

        #region 启用用户
        [HttpPost]
        public async Task<IActionResult> EnableStatusForUser(string[] id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"Update App_User
                                   Set Status           = 0,
                                       Last_Update_Date = Sysdate,
                                       LAST_UPDATE_BY  = :LAST_UPDATE_BY
                                 Where User_Id = :User_Id" + i, new OracleParameter[]
                {
                     data.MakeInParam(":LAST_UPDATE_BY",HttpContext.Session.GetString("USER_ID")),
                    data.MakeInParam(":User_Id"+i,id[i])
                });
            }
            if (ht.Count>0)
            {
                //string sql = "declare str varchar2(1000);user_id varchar2(100);str2 varchar2(3000); begin str:=:user_id1;user_id:=:user_id;str2:='update app_user set status=0,last_update_date=sysdate,LAST_UPDATED_BY='||user_id||' where user_id in('||str||')'; execute immediate str2; end;";
                //OracleParameter[] sp = { data.MakeInParam(":user_id1", OracleDbType.Varchar2, 3000, id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
                bool flag = await data.DoSqlList(ht);// data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"启用成功\"}" : "{\"code\":300,\"msg\":\"启用失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未勾选用户\"}");
            }
           
        }
        #endregion

        #region 失效用户
        [HttpPost]
        public async Task<IActionResult> FailureStatusForUser(string[] id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"Update App_User
                                   Set Status           = 1,
                                       Last_Update_Date = Sysdate,
                                       LAST_UPDATE_BY  = :LAST_UPDATE_BY
                                 Where User_Id = :User_Id" + i, new OracleParameter[]
                {
                     data.MakeInParam(":LAST_UPDATE_BY",HttpContext.Session.GetString("USER_ID")),
                    data.MakeInParam(":User_Id"+i,id[i])
                });
            }
            if (ht.Count>0)
            {
                //string sql = "declare str varchar2(1000);user_id varchar2(100);str2 varchar2(3000); begin str:=:user_id1;user_id:=:user_id;str2:='update app_user set status=1,last_update_date=sysdate,LAST_UPDATED_BY='||user_id||' where user_id in('||str||')'; execute immediate str2; end;";
                //OracleParameter[] sp = { data.MakeInParam(":user_id1", OracleDbType.Varchar2, 3000, id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
                bool flag = await data.DoSqlList(ht);// data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"失效成功\"}" : "{\"code\":300,\"msg\":\"失效失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未勾选用户\"}");
            }
            
        }
        #endregion

        #region 根据人员ID获取人员
        [HttpPost]
        public async Task<IActionResult> GetUserByID(string user_id)
        {
            string Msg = "";
            if (user_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"select u.user_id,u.user_code,u.user_name,u.person_id,p.person_name from app_user u,app_person p where u.person_id=p.person_id(+) and u.user_id=:user_id";
            OracleParameter[] sp = { data.MakeInParam(":user_id", user_id) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 获取角色
        [HttpPost]
        public async Task<IActionResult> GetRoleForUser(string limit, string page, string status, string role_name, string user_id)
        {
            string msg = "";
            string pj = status == "0" ? " And Rr.Role_Id Is Null " : " And Rr.Role_Id Is not Null ";
            string sql = @"SELECT *
                  FROM (SELECT ROWNUM AS rowno, r.*
                          FROM (select ceil(count(*) over()/ :limit) totalPage,Role_Id,Role_Name
  From (Select Sr.Role_Id, Sr.Role_Name
          From App_Role Sr,
               (Select Role_Id From App_User_Role Where User_Id = :user_Id) Rr
         Where Sr.Role_Id = Rr.Role_Id(+)
           " + pj + @" ) Sst
 Where Sst.Role_Name Like '%'||:role_name ||'%') r
                 where ROWNUM <= :page * :limit) table_alias
             WHERE table_alias.rowno > (:page - 1) * :limit";
            OracleParameter[] sp = { data.MakeInParam(":limit",limit),
                                     data.MakeInParam(":user_id",user_id),
                                     data.MakeInParam(":role_name",role_name),
                                     data.MakeInParam(":page",page)};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(msg);
        }
        #endregion

        #region 分配角色
        [HttpPost]
        public async Task<IActionResult> AllotRole(string[] id,string user_id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"Insert Into App_User_Role
                          (User_Role_Id, User_Id, Role_Id, Creation_Date, Creation_By)
                        Values
                          (App_User_Role_s.Nextval, :User_Id, :Role_Id"+i+", Sysdate, :Creation_By)", new OracleParameter[]
                    {
                        data.MakeInParam(":User_Id",user_id),
                         data.MakeInParam(":Role_Id"+i,id[i]),
                         data.MakeInParam(":Creation_By",HttpContext.Session.GetString("USER_ID"))
                    });
            }
            //string sql = @"Declare
            //          Str     Varchar2(1000);
            //          User_Id Varchar2(100);
            //          User_Id1 Varchar2(100);
            //          Str2    Varchar2(3000);
            //        Begin
            //          Str      := :Role_Id;
            //          User_Id  := :User_Id;
            //          User_Id1 := :User_Id1;
            //          Str2     := 'Insert Into App_User_Role (User_Role_Id, User_Id, Role_Id, Creation_Date, Creation_By)
            //        Select App_User_Role_s.Nextval, ' ||
            //                      User_Id || ', role_id, Sysdate, ' || User_Id1 ||
            //                      ' From App_Role Where Role_Id In (' || Str || ')';
            //          Execute Immediate Str2;
            //        End;";
            //OracleParameter[] sp = { data.MakeInParam(":Role_Id", OracleDbType.Varchar2, 3000, id),
            //    data.MakeInParam(":User_Id", OracleDbType.Varchar2, 3000, user_id),
            //    data.MakeInParam(":User_Id1", HttpContext.Session.GetString("USER_ID")) };
            if (ht.Count>0)
            {
                bool flag = await data.DoSqlList(ht);// data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"分配成功\"}" : "{\"code\":300,\"msg\":\"分配失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中角色\"}");
            }
            
        }
        #endregion

        #region 移除角色
        [HttpPost]
        public async Task<IActionResult> RemoveRole(string[] id, string user_id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"delete from app_user_role where user_id=:user_id and role_id=:role_id"+i,new OracleParameter[] 
                {
                     data.MakeInParam(":user_id",user_id),
                    data.MakeInParam(":role_id"+i,id[i])
                });
            }
            //string sql = @"Declare
            //                  Str     Varchar2(1000);
            //                  User_Id Varchar2(100);
            //                  Str2    Varchar2(3000);
            //                Begin
            //                  Str     := :Role_Id;
            //                  User_Id := :User_Id;
            //                  Str2    := 'delete from app_user_role where user_id=' || User_Id ||
            //                             ' and role_id in (' || Str || ')';
            //                  Execute Immediate Str2;
            //                End;";
            //OracleParameter[] sp = { data.MakeInParam(":Role_Id", OracleDbType.Varchar2, 3000, id),
            //    data.MakeInParam(":User_Id", OracleDbType.Varchar2, 3000, user_id) };
            if (ht.Count>0)
            {
                bool flag = await data.DoSqlList(ht);// data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"移除角色成功\"}" : "{\"code\":300,\"msg\":\"移除角色失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中角色\"}");
            }
            
        }
        #endregion

        #region 更改密码
        [HttpPost]
        public async Task<IActionResult> ChangeUserPassWord(string password,string user_id)
        {
            string Msg = "";
            if (user_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            if (mtTools.EncodeHelper.MD5Hash(password)==await data.GetStringByParam(@"select password from app_user where user_id=:user_id",
                new OracleParameter[] { data.MakeInParam(":user_id",user_id)}))
            {
                return Content("{\"code\":300,\"msg\":\"新密码与原密码一致，无法修改\"}");
            }
            string sql = @"update app_user
                               set password         = :psw,
                                   last_update_by   = :user_id,
                                   last_update_date = sysdate,MODIFYPASSWORDDATE=sysdate
                             where user_id = :user_id1";
            OracleParameter[] sp = { data.MakeInParam(":psw",mtTools.EncodeHelper.MD5Hash(password)),
                                     data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")),
                                     data.MakeInParam(":user_id1",user_id)};
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"更改成功\"}" : "{\"code\":300,\"msg\":\"更改失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 重置密码
        [HttpPost]
        public async Task<IActionResult> ResetUserPassword(string[] id)
        {
            string Msg = "";
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
               var code= await data.GetStringByParam(@"select user_code || '_'|| Extract(year from sysdate) from app_user where user_id=:user_id",
                    new OracleParameter[] { data.MakeInParam(":user_id", id[i] ?? "") });
                Regex regEnglish = new Regex("^[a-zA-Z]");
                var psw = "";
                if (regEnglish.IsMatch(code))
                {
                    psw = mtTools.EncodeHelper.MD5Hash(code);
                }
                else
                {
                    psw = mtTools.EncodeHelper.MD5Hash("A"+code);
                }
                //var psw = mtTools.EncodeHelper.MD5Hash(await data.GetStringByParam(@"select user_code || '_'|| Extract(year from sysdate) from app_user where user_id=:user_id",
                //    new OracleParameter[] { data.MakeInParam(":user_id", id[i] ?? "") }));
                ht.Add(@"update app_user set password=:password,
                            Last_Update_Date=sysdate,Last_Update_By=:Last_Update_By,MODIFYPASSWORDDATE=null
                            where user_id=:user_id" + i, new OracleParameter[]
                {
                    data.MakeInParam(":password",psw??""),
                     data.MakeInParam(":Last_Update_By",HttpContext.Session.GetString("USER_ID")),
                    data.MakeInParam(":user_id"+i,id[i])
                });
            }
            if (ht.Count>0)
            {
                bool flag = await data.DoSqlList(ht);// data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"重置成功\"}" : "{\"code\":300,\"msg\":\"重置失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中用户\"}");
            }
           
        }
        #endregion
    }
}