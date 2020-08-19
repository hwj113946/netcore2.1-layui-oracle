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

namespace WebApplication1.Controllers.Role
{
    public class RoleController : Controller
    {
        DBHelper data = new DBHelper();

        [CheckCustomer]
        public IActionResult Role()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult RoleEdit()
        {
            ViewBag.status = HttpContext.Request.Query["status"];
            ViewBag.role_id = ViewBag.status == "add" ? "" : HttpContext.Request.Query["Rowid"].ToString();
            return View();
        }

        [CheckCustomer]
        public IActionResult AllotUser()
        {
            ViewBag.role_id = HttpContext.Request.Query["Rowid"];
            ViewBag.dept_id = HttpContext.Session.GetString("DEPT_ID");
            ViewBag.corp_id= HttpContext.Session.GetString("CORP_ID");
            return View();
        }

        [CheckCustomer]
        public IActionResult AllotButton()
        {
            ViewBag.role_id = HttpContext.Request.Query["Rowid"];
            ViewBag.menu_id = HttpContext.Request.Query["menu_id"];
            return View();
        }

        #region 获取角色
        [HttpPost]
        public async Task<IActionResult> GetRole(string role_name, int page, int limit)
        {
            string Msg = "";
            string sqlpj = role_name == null ? " " : "where role_name like '%'||:role_name ||'%' ";
            string sql = @"select * from (select rownum as rowno,r.* from(select ceil(count(*) over()/ :limit) totalPage, t.role_id,t.role_name from app_role t " + sqlpj + " order by t.creation_date) r where rownum<= :page * :limit) table_alias where table_alias.rowno>( :page - 1) * :limit";
            OracleParameter[] sp = role_name == null ? new OracleParameter[] { data.MakeInParam(":limit", limit), data.MakeInParam(":page", page) } : new OracleParameter[] { data.MakeInParam(":limit", limit), data.MakeInParam(":role_name", role_name), data.MakeInParam(":page", page) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 获取角色权限树
        [HttpPost]
        public async Task<IActionResult> GetRoleMenuTree(string role_id)
        {
            string sql = @"select t.menu_id Id, t.parent_menu_id PId,  t.menu_type, t.menu_name title,
                        case  when rm.menu_id is null then 'false' else case  when t.menu_type = 0 then  'false'  else  'true' end end as checked, 'true' as spread
                       from app_menu t left join app_role_menu rm  on t.menu_id = rm.menu_id  and rm.role_id = :role_id order by t.menu_sort";
            Hashtable ht = new Hashtable();
            ht.Add("role_id", role_id ?? "");
            List<RoleMenuTree> list = await data.GetList<RoleMenuTree>(sql, ht);
            string json = ToMenuJson(list, 0);
            return Json(json.ToJson().ToString().Replace("\"false\"", "false").Replace("\"true\"", "true").ToJson());
        }

        private string ToMenuJson(List<RoleMenuTree> data, decimal parentId)
        {
            StringBuilder sbJson = new StringBuilder();
            sbJson.Append("[");
            List<RoleMenuTree> entitys = data.FindAll(t => t.pid == parentId);
            if (entitys.Count > 0)
            {
                foreach (var item in entitys)
                {
                    string strJson = item.ToJson();
                    strJson = strJson.Insert(strJson.Length - 1, ",\"children\":" + ToMenuJson(data, item.id) + "");
                    sbJson.Append(strJson + ",");
                }
                sbJson = sbJson.Remove(sbJson.Length - 1, 1);
            }
            sbJson.Append("]");
            return sbJson.ToString();
        }
        #endregion

        #region 根据角色ID获取数据信息
        [HttpPost]
        public async Task<IActionResult> GetRoleById(string role_id)
        {
            string Msg = "";
            if (role_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = "select role_id,role_name from app_role where role_id=:role_id";
            OracleParameter[] sp = { data.MakeInParam(":role_id", role_id ?? "") };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 新增
        [HttpPost]
        public async Task<IActionResult> Insert(string role_name)
        {
            string Msg = "";
            string sql = @"insert into app_role(role_id,role_name,last_update_date,last_update_by,creation_date,creation_by)
                            values(app_role_s.nextval,:role_name,sysdate,:user_id,sysdate,:user_id)";
            OracleParameter[] sp = { data.MakeInParam(":role_name", role_name ?? ""), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion
        
        #region 修改
        [HttpPost]
        public async Task<IActionResult> Modify(string role_name, string role_id)
        {
            string Msg = "";
            if (role_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"update app_role set role_name  = :role_name, last_update_date = sysdate, last_update_by = :user_id where role_id = :role_id";
            OracleParameter[] sp = { data.MakeInParam(":role_name", role_name ?? ""), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")), data.MakeInParam(":role_id", role_id) };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 删除
        [HttpPost]
        public async Task<IActionResult> Delete(string[] id)
        {
            string Msg = "";
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"delete from app_role where role_id=:role_id"+i, new OracleParameter[] { data.MakeInParam(":role_id"+i,id[i])});
            }
            bool flag = await data.DoSqlList(ht);
            Msg = flag ? "{\"code\":200,\"msg\":\"删除成功\"}" : "{\"code\":300,\"msg\":\"删除失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 获取部门：根据当前公司筛选
        [HttpPost]
        public async Task<IActionResult> GetDept()
        {
            string Msg = "";
            string sql = @"select t.dept_id,t.dept_name from app_dept t,app_corp c where t.corp_id=c.corp_id and t.status=0 and t.corp_id=:corp_id";
            OracleParameter[] sp = { data.MakeInParam(":corp_id", HttpContext.Session.GetString("CORP_ID")) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 获取用户：分配或未分配
        [HttpPost]
        public async Task<IActionResult> GetUser(string user_name, string dept_id,string corp_id, string status, string role_id, int page, int limit)
        {
            string Msg = "";
            string sqlpj = status == "未选" ? " not " : "";
            string sqlpj2 = dept_id == "-99" ? " and c.corp_id=:corp_id " : " and d.dept_id=:dept_id ";
            string sql1 = @"select count(*) from app_user   t,
                                                       app_person p,
                                                       app_dept   d,
                                                       app_posts  ps,
                                                       app_corp   c
                                                 where t.person_id = p.person_id
                                                   and p.dept_id = d.dept_id
                                                   and p.post_id = ps.post_id
                                                   and d.corp_id = c.corp_id
                                                  and t.user_id " + sqlpj + @" in (select user_id from app_user_role where role_id = :role_id)
                                                   and t.user_name like '%' || :user_name || '%'
                                                   " + sqlpj2 ;
            OracleParameter[] sp1 = dept_id == "-99"
                ? new OracleParameter[] { 
                                    data.MakeInParam(":role_id",role_id),
                                    data.MakeInParam(":user_name",user_name),
                                    data.MakeInParam(":corp_id",corp_id??"")}
            : new OracleParameter[] {
                                    data.MakeInParam(":role_id",role_id),
                                    data.MakeInParam(":user_name",user_name),
                                    data.MakeInParam(":dept_id",dept_id)};
            string n = await data.GetStringByParam(sql1,sp1);
            string sql = @"SELECT * FROM (SELECT ROWNUM AS rowno, r.*
                                          FROM (select  t.USER_ID,  t.user_code,  t.user_name,
                                                p.mobile_phone,  d.dept_name,   ps.post_name,  c.corp_name
                                                  from app_user   t,
                                                       app_person p,
                                                       app_dept   d,
                                                       app_posts  ps,
                                                       app_corp   c
                                                 where t.person_id = p.person_id
                                                   and p.dept_id = d.dept_id
                                                   and p.post_id = ps.post_id
                                                   and d.corp_id = c.corp_id
                                                  and t.user_id " + sqlpj + @" in (select user_id from app_user_role where role_id = :role_id)
                                                   and t.user_name like '%' || :user_name || '%'
                                                   " + sqlpj2 + @") r
                                         where ROWNUM <= :page * :limit) table_alias
                                 WHERE table_alias.rowno > (:page - 1) * :limit";
            OracleParameter[] sp = dept_id == "-99"
                ? new OracleParameter[] {
                                    data.MakeInParam(":role_id",role_id),
                                    data.MakeInParam(":user_name",user_name),
                                    data.MakeInParam(":corp_id",corp_id??""),
                                    data.MakeInParam(":page",page),
                                    data.MakeInParam(":limit",limit)}
            : new OracleParameter[] {
                                    data.MakeInParam(":role_id",role_id),
                                    data.MakeInParam(":user_name",user_name),
                                    data.MakeInParam(":dept_id",dept_id),
                                    data.MakeInParam(":page",page),
                                    data.MakeInParam(":limit",limit)};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + n + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 分配
        [HttpPost]
        public async Task<IActionResult> Allot(string role_id, string[] user_id)
        {
            string Msg = "";
            if (role_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < user_id.Length; i++)
            {
                ht.Add(@"Insert Into App_User_Role
                          (User_Role_Id, User_Id, Role_Id, Creation_Date, Creation_By)
                        Values
                          (App_User_Role_s.Nextval, :User_Id"+i+@", :Role_Id, Sysdate, :Creation_By)", new OracleParameter[]
                {
                    data.MakeInParam(":User_Id"+i,user_id[i]),
                    data.MakeInParam(":Role_Id",role_id),
                     data.MakeInParam(":Creation_By",HttpContext.Session.GetString("USER_ID"))
                });
            }
            if (ht.Count>0)
            {
                bool flag = await data.DoSqlList(ht);
                //string sql = @"declare user_id  varchar2(1500);  user_id2 varchar2(800);  role_id  varchar2(800);  str2     varchar2(5000);
                //begin user_id  := :user_id;user_id2:= :user_id2;role_id:= :role_id;
                // str2:= 'insert into app_user_role(user_role_id, user_id, role_id, creation_date, CREATION_BY) select app_user_role_s.nextval, user_id, ' ||
                //   role_id || ', sysdate, ' || user_id2 || '  from app_user where user_id in (' || user_id || ')'; execute immediate str2; end; ";
                //OracleParameter[] sp = { data.MakeInParam(":user_id", user_id), data.MakeInParam(":user_id2", HttpContext.Session.GetString("USER_ID")), data.MakeInParam(":role_id", role_id) };
                //bool flag = await data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"分配成功\"}" : "{\"code\":300,\"msg\":\"分配失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中用户\"}");
            }
           
        }
        #endregion

        #region 移除
        [HttpPost]
        public async Task<IActionResult> Remove(string role_id, string[] user_id)
        {
            string Msg = "";
            if (role_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < user_id.Length; i++)
            {
                ht.Add(@"delete from app_user_role where role_id=:role_id and user_id=:user_id"+i,new OracleParameter[] 
                {
                    data.MakeInParam(":role_id",role_id),
                    data.MakeInParam(":user_id"+i,user_id[i])
                });
            }
            if (ht.Count>0)
            {
                bool flag = await data.DoSqlList(ht);
                Msg = flag ? "{\"code\":200,\"msg\":\"移除成功\"}" : "{\"code\":300,\"msg\":\"移除失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中用户\"}");
            }            
        }
        #endregion

        #region 保存权限
        [HttpPost]
        public async Task<IActionResult> SaveRole(string role_id, string[] menu_id)
        {
            string Msg = "";
            if (role_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < menu_id.Length; i++)
            {
                ht.Add(@"Insert Into App_Role_Menu
                          (Role_Menu_Id, Role_Id, Menu_Id, Creation_By, Creation_Date)
                        Values
                          (App_Role_Menu_s.Nextval, :Role_Id, :Menu_Id" + i + ", :Creation_By, Sysdate)", new OracleParameter[]
                {
                        data.MakeInParam(":Role_Id",role_id),
                        data.MakeInParam(":Menu_Id"+i,menu_id[i]),
                        data.MakeInParam(":Creation_By",HttpContext.Session.GetString("USER_ID"))
                });
            }
            if (ht.Count>0)
            {
                //string sql = @"declare  menu_id varchar2(1500);  role_id varchar2(100);  user_id varchar2(100);  str1 varchar2(1000);  str2    varchar2(3000);
                //            begin  menu_id := :menu_id;  role_id := :role_id;  user_id := :user_id;  str1    := 'delete from app_role_menu where role_id = ' || role_id;
                //              str2    := 'insert into app_role_menu (ROLE_MENU_ID,  role_id,  menu_id,  last_update_by,  last_update_date,  CREATION_BY,  creation_date)
                //             select app_role_menu_s.nextval,    ' || role_id || ',    menu_id,    ' || user_id || ',    sysdate,    ' || user_id || ',    sysdate from app_menu where menu_id in (' ||
                //             menu_id || ')';  execute immediate str1;  execute immediate str2;end;";
                //OracleParameter[] sp = { data.MakeInParam(":menu_id", menu_id), data.MakeInParam(":role_id", role_id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
                await data.DoSqlByParam(@"delete from app_role_menu where role_id =:role_id", new OracleParameter[]
                    {
                        data.MakeInParam(":role_id",role_id)
                    });
                bool flag = await data.DoSqlList(ht);
                Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中菜单\"}");
            }
            
        }
        #endregion

        #region 清空权限
        [HttpPost]
        public async Task<IActionResult> AcceptReset(string role_id)
        {
            string Msg = "";
            if (role_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"delete from app_role_menu where role_id=:role_id";
            OracleParameter[] sp = { data.MakeInParam(":role_id", role_id) };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"重置成功\"}" : "{\"code\":300,\"msg\":\"重置失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 获取按钮：分配或未分配
        [HttpPost]
        public async Task<IActionResult> GetButton(string button_name, string menu_id, string status, string role_id, int page, int limit)
        {
            string Msg = "";
            string sqlpj = button_name == null ? "" : " and b.button_name like '%'|| :button_name ||'%' ";
            string sql1 = status == "未选" ? @"Select count(m.Menu_Button_Id)
                                                          From App_Menu_Button m, App_Button b
                                                         Where m.Button_Id = b.Button_Id
                                                           And m.Menu_Id = :menu_id
                                                           And m.Button_Id Not In (Select Rb.Button_Id
                                                                                     From App_Role_Button Rb
                                                                                    Where Rb.Role_Id = :role
                                                                                      And Rb.Attribute1 = :menu_id1)
                                                            " + sqlpj :
                                                            @"Select count(rb.Role_Button_Id)
                                                      From App_Role_Button Rb, App_Button b
                                                     Where Rb.Button_Id = b.Button_Id(+)
                                                       And Rb.Role_Id = :role_id
                                                       And Rb.Attribute1 =:menu_id
                                                       And Rb.Button_Id In
                                                           (Select Button_Id From App_Menu_Button Where Menu_Id = :menu_id1)
                                                    " + sqlpj ;
            OracleParameter[] sp1 = { };
            if (button_name == null)
            {
                if (status=="未选")
                {
                    sp1 = new OracleParameter[]{
                                    data.MakeInParam(":menu_id",menu_id),
                                    data.MakeInParam(":role_id",role_id),
                                    data.MakeInParam(":menu_id1",menu_id)};
                }
                else
                {
                    sp1 = new OracleParameter[]{
                                    data.MakeInParam(":role_id", role_id),
                                    data.MakeInParam(":menu_id", menu_id),
                                    data.MakeInParam(":menu_id", menu_id)};
                }
                
            }
            else
            {
                if (status == "未选")
                {
                    sp1 = new OracleParameter[]{
                                    data.MakeInParam(":menu_id",menu_id),
                                    data.MakeInParam(":role_id",role_id),
                                    data.MakeInParam(":menu_id1",menu_id),
                                    data.MakeInParam(":button_name", button_name??"")};
                }
                else
                {
                    sp1 = new OracleParameter[]{
                                    data.MakeInParam(":role_id", role_id),
                                    data.MakeInParam(":menu_id", menu_id),
                                    data.MakeInParam(":menu_id", menu_id),
                                    data.MakeInParam(":button_name", button_name??"")};
                }
            }               
            string n = await data.GetStringByParam(sql1,sp1);
            string sql = status == "未选" ? @"SELECT * FROM (SELECT ROWNUM AS rowno, r.*
                                          FROM (Select b.button_id,b.Button_Name,
                                                       b.Button_Icon,
                                                       b.Button_Event,
                                                       b.Button_Sort,
                                                       0 Role_Button_Id
                                                  From App_Menu_Button m, App_Button b
                                                 Where m.Button_Id = b.Button_Id
                                                   And m.Menu_Id = :menu_id
                                                   And m.Button_Id Not In (Select Rb.Button_Id
                                                                             From App_Role_Button Rb
                                                                            Where Rb.Role_Id = :role_id
                                                                              And Rb.Attribute1 = :menu_id1)
                                                            " + sqlpj + @") r
                                                      where ROWNUM <= :page * :limit) table_alias
                                            WHERE table_alias.rowno > (:page - 1) * :limit"
                                    :
                                    @"SELECT * FROM (SELECT ROWNUM AS rowno, r.*
                                          FROM (Select b.button_id,b.Button_Name,
                                                           b.Button_Icon,
                                                           b.Button_Event,
                                                           b.Button_Sort,
                                                           Nvl(Rb.Role_Button_Id, 0) Role_Button_Id
                                                      From App_Role_Button Rb, App_Button b
                                                     Where Rb.Button_Id = b.Button_Id(+)
                                                       And Rb.Role_Id = :role_id
                                                       And Rb.Attribute1 =:menu_id
                                                       And Rb.Button_Id In
                                                           (Select Button_Id From App_Menu_Button Where Menu_Id = :menu_id1)
                                                    " + sqlpj + @") r
                                                             where ROWNUM <= :page * :limit) table_alias
                                                     WHERE table_alias.rowno > (:page - 1) * :limit";
            OracleParameter[] sp = { };
            if (button_name == null)
            {
                if (status=="未选")
                {
                    sp = new OracleParameter[]{
                                    data.MakeInParam(":menu_id", menu_id),
                                    data.MakeInParam(":role_id", role_id),
                                    data.MakeInParam(":menu_id1", menu_id),
                                    data.MakeInParam(":page",page),
                                    data.MakeInParam(":limit",limit)};
                }
                else
                {
                    sp = new OracleParameter[]{
                                    data.MakeInParam(":role_id", role_id),
                                    data.MakeInParam(":menu_id", menu_id),
                                    data.MakeInParam(":menu_id1", menu_id),
                                    data.MakeInParam(":page", page),
                                    data.MakeInParam(":limit",limit)};
                }
                
            }
            else
            {
                if (status == "未选")
                {
                    sp = new OracleParameter[]{
                                    data.MakeInParam(":menu_id", menu_id),
                                    data.MakeInParam(":role_id", role_id),
                                    data.MakeInParam(":menu_id1", menu_id),
                                    data.MakeInParam(":button_name", button_name??""),
                                    data.MakeInParam(":page",page),
                                    data.MakeInParam(":limit",limit)};
                }
                else
                {
                    sp = new OracleParameter[]{
                                    data.MakeInParam(":role_id", role_id),
                                    data.MakeInParam(":menu_id", menu_id),
                                    data.MakeInParam(":menu_id1", menu_id),
                                    data.MakeInParam(":button_name", button_name??""),
                                    data.MakeInParam(":page", page),
                                    data.MakeInParam(":limit",limit)};
                }
            }                                    
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + n + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 分配按钮
        [HttpPost]
        public async Task<IActionResult> AllotButton(string role_id, string menu_id,string[] button_id)
        {
            string Msg = "";
            if (role_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < button_id.Length; i++)
            {
                ht.Add(@"Insert Into App_Role_Button
                                      (Role_Button_Id,
                                       Role_Id,
                                       Button_Id,
                                       Attribute1,
                                       Creation_Date,
                                       Creation_By)
                                    Values
                                      (App_Role_Button_s.Nextval,
                                       :Role_Id,
                                       :Button_Id"+i+@",
                                       :Menu_Id,
                                       Sysdate,
                                       :Creation_By)",new OracleParameter[] 
                                                     {
                                                         data.MakeInParam(":Role_Id",role_id),
                                                         data.MakeInParam(":Button_Id"+1,button_id[i]),
                                                         data.MakeInParam(":Menu_Id",menu_id),
                                                         data.MakeInParam(":Creation_By",HttpContext.Session.GetString("USER_ID")),
                                                     });
            }
            if (ht.Count>0)
            {
                bool flag = await data.DoSqlList(ht);
  //              string sql = @"declare  menu_id   varchar2(100);  role_id   varchar2(100);  button_id varchar2(1000);  user_id   varchar2(100);
  //str2      varchar2(3000);begin  menu_id := :menu_id;  role_id := :role_id;  user_id := :user_id;button_id:=:button_id;
  //str2    := 'insert into app_role_button (role_button_id,  role_id,  button_id,attribute1,  last_update_date,  last_update_by,
  //creation_date,  creation_by) select app_role_button_s.nextval,   ' || role_id || ',    button_id,' || menu_id || ',
  //  sysdate,'||user_id||',    sysdate,    '||user_id||' from app_button where button_id in(' ||  button_id || ')';  execute immediate str2; end;";
  //              OracleParameter[] sp = { data.MakeInParam(":menu_id", menu_id), data.MakeInParam(":role_id", role_id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")), data.MakeInParam(":button_id", button_id) };
  //              bool flag = await data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"分配成功\"}" : "{\"code\":300,\"msg\":\"分配失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中按钮\"}");
            }
        }
        #endregion

        #region 移除按钮
        [HttpPost]
        public async Task<IActionResult> RemoveButton(string[] role_button_id)
        {
            string Msg = "";
            if (role_button_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < role_button_id.Length; i++)
            {
                ht.Add(@"delete from app_role_button where role_button_id=:role_button_id"+i,new OracleParameter[]
                    {
                        data.MakeInParam(":role_button_id"+i,role_button_id[i])
                    });
            }
            if (ht.Count>0)
            {
                bool flag = await data.DoSqlList(ht);
                //string sql = @"declare  role_button_id varchar2(1500);  str2  varchar2(3000);begin  role_button_id := :role_button_id;
                //         str2      := 'delete from app_role_button where role_button_id in('||role_button_id||')';
                //         execute immediate str2;end;";
                //OracleParameter[] sp = { data.MakeInParam(":role_button_id", role_button_id) };
                //bool flag = await data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"移除成功\"}" : "{\"code\":300,\"msg\":\"移除失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中按钮\"}");
            }
            
        }
        #endregion
    }
}