using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;


namespace WebApplication1.Controllers.Menu
{
    public class MenuController : Controller
    {
        DBHelper data = new DBHelper();

        [CheckCustomer]
        public IActionResult Menu()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult MenuEdit()
        {
            ViewBag.zt = HttpContext.Request.Query["zt"];
            ViewBag.type = HttpContext.Request.Query["type"];
            ViewBag.menuid = (ViewBag.zt == "add" ? "" : HttpContext.Request.Query["menuid"].ToString());
            ViewBag.isone = HttpContext.Request.Query["isone"];
            ViewBag.parentid = ViewBag.isone=="0"? HttpContext.Request.Query["menuid"]: HttpContext.Request.Query["parentid"];
            return View();
        }

        #region 获取所有菜单
        /// <summary>
        /// 菜单转json
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllMenu()
        {
            string sql = @"select m.menu_id id,
                           m.parent_menu_id pid,
                           m.menu_code,
                           m.menu_name name,
                           m.menu_icon,
                           M.menu_type,
                           m.menu_url,
                           m.menu_sort
                      from app_menu m
                      order by m.menu_sort asc";
            DataSet ds = await data.GetDataSet(sql);
            //List<Models.MenuTree> list = data.GetList1<Models.MenuTree>(sql);
            string json = "{\"code\":200,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + DataTableToJson(ds.Tables[0]) + "]}";
            //string json=
            return Json(json.ToJson());
        }

       

        private string DataTableToJson(DataTable dt)
        {
            StringBuilder Json = new StringBuilder();
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Json.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        Json.Append("\"" + dt.Columns[j].ColumnName.ToLower().ToString() + "\":\"" + dt.Rows[i][j].ToString().Replace("\"", "\\\"") + "\"").Replace("\t", "").Replace("\n", "");
                        if (j < dt.Columns.Count - 1)
                        {
                            Json.Append(",");
                        }
                    }
                    Json.Append("}");
                    if (i < dt.Rows.Count - 1)
                    {
                        Json.Append(",");
                    }
                }
            }
            return Json.ToString();
        }
        #endregion

        #region 新增
        [HttpPost]
        public async Task<IActionResult> Insert(string menu_code, string menu_name, string menu_icon, string menu_url, string menu_type, string menu_sort, string parent_menu_id)
        {
            string Msg = "";
            string sql = @"insert into app_menu (menu_id, parent_menu_id, menu_code, menu_name, menu_icon, menu_type, menu_url, menu_sort, creation_date, creation_by)
                    values(app_menu_s.nextval, :parent_menu_id, :menu_code, :menu_name, :menu_icon, :menu_type, :menu_url, :menu_sort, sysdate, :user_id)";
            OracleParameter[] sp = {
                data.MakeInParam(":parent_menu_id",parent_menu_id??""),
                data.MakeInParam(":menu_code",menu_code??""),
                data.MakeInParam(":menu_name",menu_name??""),
                data.MakeInParam(":menu_icon",menu_icon??""),
                data.MakeInParam(":menu_type",menu_type??""),
                data.MakeInParam(":menu_url",menu_url??""),
                data.MakeInParam(":menu_sort",menu_sort??""),
                data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID"))};
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 编辑
        [HttpPost]
        public async Task<IActionResult> Modify(string menu_code, string menu_name, string menu_icon, string menu_url, string menu_type, string menu_sort, string parent_menu_id, string menu_id)
        {
            string Msg = "";
            if (menu_id==null||menu_id=="")
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"update app_menu
                                  set parent_menu_id   = :parent_menu_id,
                                      menu_code        = :menu_code,
                                      menu_name        = :menu_name,
                                      menu_icon        = :menu_icon,
                                      menu_type        = :menu_type,
                                      menu_url         = :menu_url,
                                      menu_sort        = :menu_sort,
                                      last_update_date = sysdate,
                                      last_update_by   = :user_id
                                where menu_id = :menu_id";
            OracleParameter[] sp = {
                data.MakeInParam(":parent_menu_id",parent_menu_id??""),
                data.MakeInParam(":menu_code",menu_code??""),
                data.MakeInParam(":menu_name",menu_name??""),
                data.MakeInParam(":menu_icon",menu_icon??""),
                data.MakeInParam(":menu_type",menu_type??""),
                data.MakeInParam(":menu_url",menu_url??""),
                data.MakeInParam(":menu_sort",menu_sort??""),
                data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")),
                data.MakeInParam(":menu_id",menu_id??"")
            };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 删除
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            string Msg = "";
            if (id==null||id=="")
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            OracleParameter[] sp = { data.MakeInParam(":menu_id", id ?? "") };
            Hashtable ht = new Hashtable();
            ht.Add("delete from app_role_menu where menu_id in(select t.menu_id  from app_menu t start with t.menu_id = :menu_id connect by prior t.menu_id = t.parent_menu_id)", sp);
            ht.Add("delete from app_menu_button where menu_id in(select t.menu_id  from app_menu t start with t.menu_id = :menu_id connect by prior t.menu_id = t.parent_menu_id)", sp);
            ht.Add("delete from app_menu where menu_id in(select t.menu_id  from app_menu t start with t.menu_id = :menu_id connect by prior t.menu_id = t.parent_menu_id)", sp);
            bool flag = await data.DoSqlList(ht);
            Msg = flag ? "{\"code\":200,\"msg\":\"删除成功\"}" : "{\"code\":300,\"msg\":\"删除失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 根据菜单ID获取菜单信息
        [HttpPost]
        public async Task<IActionResult> GetMenuInfoById(string menu_id)
        {
            string Msg = "";
            if (menu_id == null || menu_id == "")
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"select m.menu_id , m.parent_menu_id , m.menu_code, m.menu_name, m.menu_icon, M.menu_type, m.menu_url, m.menu_sort from app_menu m where m.menu_id=:menu_id";
            OracleParameter[] sp = { data.MakeInParam(":menu_id", menu_id ?? "") };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":1,\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

       
    }
}