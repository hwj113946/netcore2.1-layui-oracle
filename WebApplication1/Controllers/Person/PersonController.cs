using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;


namespace WebApplication1.Controllers.Person
{
    public class PersonController : Controller
    {
        DBHelper data = new DBHelper();

        [CheckCustomer]
        public IActionResult Person()
        {
            ViewBag.CORP_ID = HttpContext.Session.GetString("CORP_ID");
            ViewBag.DEPT_ID = HttpContext.Session.GetString("DEPT_ID");
            ViewBag.POST_ID = HttpContext.Session.GetString("POST_ID");
            return View();
        }

        [CheckCustomer]
        public IActionResult EditPerson()
        {
            ViewBag.status = HttpContext.Request.Query["status"];
            ViewBag.person_id = ViewBag.status == "add" ? "" : HttpContext.Request.Query["Rowid"].ToString();
            return View();
        }

        #region 获取人员
        [HttpPost]
        public async Task<IActionResult> GetPersonByCorpDeptPost(int limit, int page, string person_name, string dept_id, string corp_id,string post_id,string status)
        {
            string msg = "";
            corp_id=corp_id ?? HttpContext.Session.GetString("CORP_ID");
            //dept_id=dept_id ?? HttpContext.Session.GetString("DEPT_ID");
            //post_id=post_id?? HttpContext.Session.GetString("POST_ID");
            string pj = (corp_id!=null&&dept_id==null)?" and d.corp_id=:Corp_Id "
                :((corp_id!=null&&dept_id!=null&&post_id==null)?" and t.dept_id=:Dept_Id "
                :((corp_id != null && dept_id != null && post_id != null)
                ?" and t.dept_id=:Dept_Id and t.post_id=:Post_Id ":""));
            string sql = @"SELECT *
                  FROM (SELECT ROWNUM AS rowno, r.*
                          FROM (select t.Person_Id,
                               Decode(Nvl(t.Person_Type, 3), 0, '员工', 1, '非员工', 3, '未填写') Person_Type,
                               t.Person_Code,
                               t.Person_Name,
                               t.Id_Card_Number,
                               t.Sex,
                               t.Mobile_Phone,
                               t.Fixed_Phone,
                               t.Email,
                               c.Corp_Name,
                               d.Dept_Name,
                               p.Post_Name,
                               Decode(t.Status, 0, '有效', 1, '无效') Status
                          From App_Person t, App_Corp c, App_Dept d, App_Posts p
                         Where t.Dept_Id = d.Dept_Id
                           And d.Corp_Id = c.Corp_Id
                           And t.Post_Id = p.Post_Id
                           And t.Status = :Status
                           and (t.person_code like '%'|| :person_code ||'%' or t.person_name like '%'|| :person_name || '%')
                           "+pj+@" ) r
                 where ROWNUM <= :page * :limit) table_alias
             WHERE table_alias.rowno > (:page - 1) * :limit";
            string sql1 = @"select count(*)
                          From App_Person t, App_Corp c, App_Dept d, App_Posts p
                         Where t.Dept_Id = d.Dept_Id
                           And d.Corp_Id = c.Corp_Id
                           And t.Post_Id = p.Post_Id
                           And t.Status = :Status
                           and (t.person_code like '%'|| :person_code ||'%' or t.person_name like '%'|| :person_name || '%')
                           " + pj;
            OracleParameter[] sp2 = (corp_id != null && dept_id == null) ?
                new OracleParameter[] {
                
                data.MakeInParam(":status",status),
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":Corp_Id",corp_id??"")}
                : ((corp_id != null && dept_id != null && post_id == null) ?
                new OracleParameter[] {
                
                data.MakeInParam(":status",status),
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":Dept_Id",dept_id??"")}
                : ((corp_id != null && dept_id != null && post_id != null) ?
                new OracleParameter[] {
               
                data.MakeInParam(":status",status),
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":Dept_Id",dept_id??""),
                data.MakeInParam(":Post_Id",post_id??"") } :
                new OracleParameter[] {
                
                data.MakeInParam(":status",status),
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??"")}
                ));
            string n = await data.GetStringByParam(sql1,sp2);
            OracleParameter[] sp1 = (corp_id != null && dept_id == null)?
                new OracleParameter[] {
                
                data.MakeInParam(":status",status),
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":Corp_Id",corp_id??""),
                data.MakeInParam(":page", page),data.MakeInParam(":limit", limit)}                
                :((corp_id != null && dept_id != null && post_id == null)?
                new OracleParameter[] {
                data.MakeInParam(":status",status),
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":Dept_Id",dept_id??""),
                data.MakeInParam(":page", page),data.MakeInParam(":limit", limit)}
                :((corp_id != null && dept_id != null && post_id != null)?
                new OracleParameter[] {
                data.MakeInParam(":status",status),
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":Dept_Id",dept_id??""),
                data.MakeInParam(":Post_Id",post_id??""),
                data.MakeInParam(":page", page),data.MakeInParam(":limit", limit) } :
                new OracleParameter[] {
                data.MakeInParam(":status",status),
                data.MakeInParam(":person_code",person_name??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":page", page),data.MakeInParam(":limit", limit) }
                ));
            DataSet ds = await data.GetDataSetByParam(sql, sp1);
            msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + n + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(msg);
        }
        #endregion

        #region 启用人员
        [HttpPost]
        public async Task<IActionResult> EnableStatusForPerson(string id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = "declare str varchar2(1000);user_id varchar2(100);str2 varchar2(3000); begin str:=:Person_id;user_id:=:user_id;str2:='update app_person set status=0,last_update_date=sysdate,LAST_UPDATED_BY='||user_id||' where person_id in('||str||')'; execute immediate str2; end;";
            OracleParameter[] sp = { data.MakeInParam(":Person_id", OracleDbType.Varchar2, 3000, id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"启用成功\"}" : "{\"code\":300,\"msg\":\"启用失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 失效人员
        [HttpPost]
        public async Task<IActionResult> FailureStatusForPerson(string id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = "declare str varchar2(1000);user_id varchar2(100);str2 varchar2(3000); begin str:=:Person_id;user_id:=:user_id;str2:='update app_person set status=1,last_update_date=sysdate,LAST_UPDATED_BY='||user_id||' where person_id in('||str||')'; execute immediate str2; end;";
            OracleParameter[] sp = { data.MakeInParam(":Person_id", OracleDbType.Varchar2, 3000, id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"失效成功\"}" : "{\"code\":300,\"msg\":\"失效失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 根据人员ID获取人员
        [HttpPost]
        public async Task<IActionResult> GetPersonByID(string person_id)
        {
            string Msg = "";
            if (person_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"Select t.Person_Id,
                                   t.Person_Type,
                                   t.Person_Code,
                                   t.Person_Name,
                                   t.Id_Card_Number,
                                   t.Sex,
                                   t.Mobile_Phone,
                                   t.Fixed_Phone,
                                   t.Email,
                                   d.Corp_Id,
                                   t.Dept_Id,
                                   t.Post_Id
                              From App_Person t, App_Dept d
                             Where t.Dept_Id = d.Dept_Id(+)
                               And t.Person_Id = :person_id";
            OracleParameter[] sp = { data.MakeInParam(":person_id", person_id) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 新增人员
        [HttpPost]
        public async Task<IActionResult> Insert(string person_code, string person_name, string dept_id, string post_id,string phone,string fixed_phone,string id_card_number,string sex,string email)
        {
            string Msg = "";
            string sql = @"Insert Into App_Person
                                      (Person_Id,
                                       Person_Code,
                                       Person_Name,
                                       Id_Card_Number,
                                       Sex,
                                       Mobile_Phone,
                                       Fixed_Phone,
                                       Email,
                                       Dept_Id,
                                       Post_Id,
                                       Status,
                                       Creation_Date,
                                       Created_By)
                                    Values
                                      (App_Person_s.Nextval,
                                       :person_code,
                                       :person_name,
                                       :id_card_number,
                                       :sex,
                                       :mobile_phone,
                                       :fixed_phone,
                                       :email,
                                       :dept_id,
                                       :post_id,
                                       0,
                                       Sysdate,
                                       :user_id)";
            OracleParameter[] sp = {    data.MakeInParam(":person_code", person_code??""),
                                        data.MakeInParam(":person_name",person_name??"" ),
                                        data.MakeInParam(":id_card_number",id_card_number??"" ),
                                        data.MakeInParam(":sex",sex??"" ),
                                        data.MakeInParam(":mobile_phone",phone??"" ),
                                        data.MakeInParam(":fixed_phone",fixed_phone??"" ),
                                        data.MakeInParam(":email",email??"" ),
                                        data.MakeInParam(":dept_id",dept_id??"" ),
                                        data.MakeInParam(":post_id",post_id??"" ),
                                        data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID") )};
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 修改
        [HttpPost]
        public async Task<IActionResult> Modify(string person_code, string person_name, string dept_id, string post_id, string phone, string fixed_phone, string id_card_number, string sex, string email,string person_id)
        {
            string Msg = "";
            if (person_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"Update App_Person
                               Set Person_Code      = :person_code,
                                   Person_Name      = :person_name,
                                   Id_Card_Number   = :id_card_number,
                                   Sex              = :sex,
                                   Mobile_Phone     = :mobile_phone,
                                   Fixed_Phone      = :fixed_phone,
                                   Email            = :email,
                                   Dept_Id          = :dept_id,
                                   Post_Id          = :post_id,
                                   Last_Update_Date = Sysdate,
                                   Last_Updated_By  = :user_id
                             Where Person_Id = :person_id";
            OracleParameter[] sp = {    data.MakeInParam(":person_code", person_code??""),
                                        data.MakeInParam(":person_name",person_name??"" ),
                                        data.MakeInParam(":id_card_number",id_card_number??"" ),
                                        data.MakeInParam(":sex",sex??"" ),
                                        data.MakeInParam(":mobile_phone",phone??"" ),
                                        data.MakeInParam(":fixed_phone",fixed_phone??"" ),
                                        data.MakeInParam(":email",email??"" ),
                                        data.MakeInParam(":dept_id",dept_id??"" ),
                                        data.MakeInParam(":post_id",post_id??"" ),
                                        data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")),
                                        data.MakeInParam(":person_id",person_id)};
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion
    }
}