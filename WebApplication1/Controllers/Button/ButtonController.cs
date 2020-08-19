using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Collections;
using System.Data;
using System.Threading.Tasks;

namespace WebApplication1.Controllers.Button
{
    public class ButtonController : Controller
    {
        Common.DBHelper data = new Common.DBHelper();

        [CheckCustomer]
        public IActionResult Button()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult EditButton()
        {
            ViewBag.status = HttpContext.Request.Query["status"].ToString();
            ViewBag.Rowid = (ViewBag.status == "Add"?"":HttpContext.Request.Query["Rowid"].ToString());
            return View();
        }

        [CheckCustomer]
        public IActionResult ChooseColor()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult AllotButtonForMenu()
        {
            ViewBag.menu_id = HttpContext.Request.Query["menu_id"];
            return View();
        }

        #region 获取按钮
        [HttpPost]
        public async Task<IActionResult> GetButton(string BUTTON_NAME,int page,int limit)
        {
            string Msg = "";
            string sqlpj = (BUTTON_NAME == null ? "" : " where BUTTON_NAME like '%'|| :BUTTON_NAME || '%'");
            string sql = @"Select *
                              From (Select Rownum As Rowno, r.*
                                      From (Select Button_Id,
                                                   Button_Name,
                                                   Button_Icon,
                                                   Button_Event,
                                                   Button_Sort,
                                                   Attribute1,
                                                   Attribute2
                                              From App_Button " + sqlpj + @"
                                             Order By Button_Sort Asc) r
                                     Where Rownum <= :Page * :Limit) Table_Alias
                             Where Table_Alias.Rowno > (:Page - 1) * :Limit";
            string sql1 = @"Select count(*)
                  From App_Button " + sqlpj + @"
                 Order By Button_Sort Asc";
            OracleParameter[] p1 = sqlpj == "" ? 
                new OracleParameter[] { } : 
                new OracleParameter[] { 
                    data.MakeInParam(":BUTTON_NAME", BUTTON_NAME) };
            string n = await data.GetStringByParam(sql1,p1);
            OracleParameter[] p=sqlpj==""? new OracleParameter[] {
                data.MakeInParam(":page", page),
             data.MakeInParam(":limit", limit)}
            :  new OracleParameter[] {                    
                    data.MakeInParam(":BUTTON_NAME", BUTTON_NAME),
                    data.MakeInParam(":page", page),data.MakeInParam(":limit", limit) };
            DataSet ds = await data.GetDataSetByParam(sql,p);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + n + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 新增
        [HttpPost]
        public async Task<IActionResult> Insert(string button_name, string button_icon, string button_event, string button_sort, string attribute1,string attribute2)
        {
            string Msg = "";
            string sql = @"insert into app_button(button_id,button_name,button_icon,button_event,button_sort,attribute1,attribute2,creation_date,creation_by)
                           values(app_button_s.nextval,:button_name,:button_icon,:button_event,:button_sort,:attribute1,:attribute2,sysdate,:user_id) ";
            OracleParameter[] sp = { 
            data.MakeInParam(":button_name",button_name ??"" ),
            data.MakeInParam(":button_icon",button_icon ??"" ),
            data.MakeInParam(":button_event",button_event ??"" ),
            data.MakeInParam(":button_sort",button_sort ??"" ),
            data.MakeInParam(":attribute1",attribute1 ??"" ),data.MakeInParam(":attribute2",attribute2 ??"" ),
            data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID"))};
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 编辑
        [HttpPost]
        public async Task<IActionResult> Modify(string button_name, string button_icon, string button_event, string button_sort, string attribute1,string Rowid,string attribute2)
        {
            string Msg = "";
            string sql = @"update app_button
                                   set button_name      = :button_name,
                                       button_icon      = :button_icon,
                                       button_event     = :button_event,
                                       button_sort      = :button_sort,
                                       attribute1       = :attribute1,attribute2=:attribute2,
                                       last_update_date = sysdate,
                                       last_update_by   = :user_id
                                 where button_id = :button_id ";
            OracleParameter[] sp = {
            data.MakeInParam(":button_name",button_name ??"" ),
            data.MakeInParam(":button_icon",button_icon ??"" ),
            data.MakeInParam(":button_event",button_event ??"" ),
            data.MakeInParam(":button_sort",button_sort ??"" ),
            data.MakeInParam(":attribute1",attribute1 ??"" ),data.MakeInParam(":attribute2",attribute2 ??"" ),
            data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")),
            data.MakeInParam(":button_id", Rowid ??"" )};
            if (Rowid==null||Rowid=="")
            {
                return Content("{\"code\":300,\"msg\":\"请传入数据唯一值\"}");
            }
            bool flag =await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 删除
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            string Msg = "";
            string sql = "declare str varchar2(3000);str2 varchar2(3000); begin str:=:BUTTON_ID;str2:='delete from app_button where button_id in('||str||')'; execute immediate str2; end;";
            OracleParameter[] sp = { data.MakeInParam(":BUTTON_ID",OracleDbType.Varchar2,30, id) };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"删除成功\"}" : "{\"code\":300,\"msg\":\"删除失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 根据按钮ID获取按钮信息
        [HttpPost]
        public async Task<IActionResult> GetButtonById(string id)
        {
            string Msg = "";
            string sql = "select button_id,button_name,button_icon,button_event,button_sort,attribute1,attribute2 from app_button where button_id=:button_id";
            OracleParameter[] sp = { data.MakeInParam(":button_id", id) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":1,\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}" ): "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 导出：获取按钮数据（按钮名称可有可无）
        [HttpPost]
        public async Task<IActionResult> GetButtonForExport(string button_name)
        {
            string Msg = "";
            string sql = @"select button_id,button_name,button_icon,button_event,button_sort,attribute1 from app_button  where BUTTON_NAME like '%'|| :BUTTON_NAME || '%' order by button_sort asc";
            OracleParameter[] p = new OracleParameter[] { data.MakeInParam(":BUTTON_NAME", button_name??"") };
            DataSet ds = await data.GetDataSetByParam(sql, p);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 获取菜单分配或未分配的按钮
        [HttpPost]
        public async Task<IActionResult> GetButtons(string button_name, string menu_id, string status, int limit, int page)
        {
            string Msg = "";
            string sqlpj = status == "未选" ? @"button_id, button_name, button_icon, button_event,button_sort, attribute1, 
menu_button_id from (select t.button_id,t.button_name,t.button_icon, t.button_event,
t.button_sort, t.attribute1, nvl(mb.menu_button_id, 0) menu_button_id
 from app_button t, app_menu_button mb where t.button_id = mb.button_id(+)
and t.button_name like '%' || :button_name || '%'
 and mb.menu_id(+) = :menu_id) where menu_button_id = 0" 
: @" t.button_id, t.button_name, t.button_icon, t.button_event, t.button_sort, t.attribute1, 
nvl(mb.menu_button_id, 0) menu_button_id from app_button t, app_menu_button mb 
where t.button_id = mb.button_id and t.button_name like '%' || :button_name || '%' 
and mb.menu_id = :menu_id order by t.button_sort asc";
            string sql = @"select * from (select rownum as rowno,r.* from(select "+sqlpj+@") r where rownum<= :page * :limit) table_alias where table_alias.rowno>( :page - 1) * :limit";

            string sql1 = status == "未选" ? @"Select Count(*)
                                              From (Select *
                                                      From (Select t.Button_Id,
                                                                   t.Button_Name,
                                                                   t.Button_Icon,
                                                                   t.Button_Event,
                                                                   t.Button_Sort,
                                                                   t.Attribute1,
                                                                   Nvl(Mb.Menu_Button_Id, 0) Menu_Button_Id
                                                              From App_Button t, App_Menu_Button Mb
                                                             Where t.Button_Id = Mb.Button_Id(+)
                                                               And t.Button_Name Like '%' || :Button_Name || '%'
                                                               And Mb.Menu_Id(+) = :Menu_Id)
                                                     Where Menu_Button_Id = 0)" :
                                             @"Select Count(*)
                                              From App_Button t, App_Menu_Button Mb
                                             Where t.Button_Id = Mb.Button_Id
                                               And t.Button_Name Like '%' || :Button_Name || '%'
                                               And Mb.Menu_Id = :Menu_Id
                                             Order By t.Button_Sort Asc";
            OracleParameter[] p1 = { 
                data.MakeInParam(":Button_Name", button_name ?? ""),
                data.MakeInParam(":Menu_Id", menu_id) };
            string n = await data.GetStringByParam(sql1,p1);
            OracleParameter[] p = { 
                data.MakeInParam(":button_name", button_name ?? ""),
                data.MakeInParam(":menu_id", menu_id),
                data.MakeInParam(":page", page),data.MakeInParam(":limit", limit) };
            DataSet ds = await data.GetDataSetByParam(sql, p);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + n + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 移除
        [HttpPost]
        public async Task<IActionResult> Remove(string menu_button_id)
        {
            string Msg = "";
            if (menu_button_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"declare  menu_button_id varchar2(1500);  str2    varchar2(3000);begin  menu_button_id := :menu_button_id;  str2    := 'delete from app_menu_button where menu_button_id in(' || menu_button_id || ')';  execute immediate str2;end;";
            OracleParameter[] sp = { data.MakeInParam(":menu_button_id", menu_button_id) };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 分配
        [HttpPost]
        public async Task<IActionResult> Allot(string button_id, string menu_id)
        {
            string Msg = "";
            if (menu_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"declare  button_id varchar2(1500); menu_id varchar2(100);  user_id  varchar2(100); str2  varchar2(3000);
                            begin  button_id := :button_id;  menu_id   := :menu_id;  user_id   := :user_id;
                              str2      := 'insert into app_menu_button (menu_button_id, menu_id, button_id, last_update_date, last_update_by, creation_date, creation_by)
                            select app_menu_button_s.nextval, ' || menu_id || ', button_id, sysdate, ' || user_id || ', sysdate, ' || user_id || ' from app_button where button_id in(' || button_id || ')';  execute immediate str2;end;";
            OracleParameter[] sp = { data.MakeInParam(":button_id", button_id),data.MakeInParam(":menu_id", menu_id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion
    }
}