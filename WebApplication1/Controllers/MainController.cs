using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Common;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Text.RegularExpressions;

namespace WebApplication1.Controllers
{
    public class MainController : Controller
    {
        DBHelper data = new DBHelper();

        [CheckCustomer]
        public IActionResult MainIndex()
        {
            ViewBag.USER_CODE = HttpContext.Session.GetString("USER_CODE");
            ViewBag.USER_NAME = HttpContext.Session.GetString("USER_NAME");
            return View();
        }

        [CheckCustomer]
        public async Task<IActionResult> Index()
        {
            var password = HttpContext.Session.GetString("PASSWORD");
            Regex regex = new Regex("^(?=.*[a-zA-Z])(?=.*[0-9])(?=.*[._~!@#$^&*])[A-Za-z0-9._~!@#$^&*]{8,20}$");
            if (password=="klapp")
            {
                ViewBag.needChangePws = false;
            }
            else
            {
                var time = await data.GetStringByParam(@"Select To_Date(To_Char(Sysdate, 'yyyymmdd'), 'YYYY-MM-DD') -
                    To_Date(To_Char(Nvl(Modifypassworddate, Sysdate - 90), 'yyyymmdd'), 'YYYY-MM-DD') From App_User where user_id=:user_id",
                    new OracleParameter[] { data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID"))});
                ViewBag.needChangePws = int.Parse(time)>=90 ? true : regex.IsMatch(password) ? false : true;
            }            
            string n = await data.GetString(@"Select Count(*) From Txry_Person t");
            string dhd = await data.GetString(@"Select Count(*) From Txry_Person t Where t.Status = 0");
            string yhd = await data.GetString(@"Select Count(*) From Txry_Person t Where t.Status = 1");
            string done = await data.GetString(@"Select Count(*) From Txry_Person t Where t.Status = 2");
            ViewBag.Effective = dhd;
            ViewBag.Invalid = yhd;
            ViewBag.Done = done;
            ViewBag.All = n;
            //ViewBag.NotFinish = 10;
            return View();
        }

        #region 获取左侧菜单树
        public IActionResult GetMenuData()
        {
            var data = new
            {
                UseMenuDatas = this.GetMenuList(),
            };
            return Content(data.ToJson());
        }

        public object GetMenuList()
        {
            string sql = @"select distinct m.menu_id,
                           m.parent_menu_id,
                           m.menu_code,
                           m.menu_name,
                           m.menu_icon,
                           M.menu_type,
                           m.menu_url,
                           m.menu_sort
                      from app_menu m, app_role_menu rm, app_user_role ur
                     where m.menu_id = rm.menu_id(+)
                       and rm.role_id = ur.role_id(+)
                       and ur.user_id =:user_id  order by m.menu_sort asc";
            Hashtable ht = new Hashtable();
            ht.Add("user_id", HttpContext.Session.GetString("USER_ID"));
            List<Models.Menu> list = data.GetList1<Models.Menu>(sql, ht);
            var json = ToMenuJson(list, 0);
            return json;
        }

        private string ToMenuJson(List<Models.Menu> data, decimal parentId)
        {
            if (data==null)
            {
                return "";
            }
            StringBuilder sbJson = new StringBuilder();
            sbJson.Append("[");
            List<Models.Menu> entitys = data.FindAll(t => t.PARENT_MENU_ID == parentId);
            if (entitys.Count > 0)
            {
                foreach (var item in entitys)
                {
                    string strJson = item.ToJson();
                    strJson = strJson.Insert(strJson.Length - 1, ",\"ChildNodes\":" + ToMenuJson(data, item.MENU_ID) + "");
                    sbJson.Append(strJson + ",");
                }
                sbJson = sbJson.Remove(sbJson.Length - 1, 1);
            }
            sbJson.Append("]");
            return sbJson.ToString();
        }
        #endregion

        #region 获取审批通知
        [HttpGet]
        public async Task<IActionResult> GetApprNotice()
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
    }
}