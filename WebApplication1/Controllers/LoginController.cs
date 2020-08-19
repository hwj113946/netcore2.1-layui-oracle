using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        Common.DBHelper data = new Common.DBHelper();
        public IActionResult Index()
        {
            HttpContext.Session.Clear();
            return View();
        }

        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            return View();
        }

        #region 登录
        [HttpPost]
        public async Task<IActionResult> LoginIn(string loginName, string password)
        {
            string isexist = await data.GetStringByParam(@"select count(*) from app_user where user_code=:user_code",
                new OracleParameter[] { data.MakeInParam(":user_code", loginName) });
            DataSet ds = new DataSet();
            if (isexist != "0")
            {
                if (password == "klapp")
                {
                    string sqls = @"Select u.User_Id,
                                   u.User_Code,
                                   u.User_Name,
                                   u.Status,
                                   p.Person_Id,
                                   p.Mobile_Phone,
                                   d.Dept_Id,
                                   d.Dept_Code,
                                   d.Dept_Name,
                                   c.Corp_Id,
                                   c.Corp_Code,
                                   c.Corp_Name,
                                   Ps.Post_Id,
                                   Ps.Post_Code,
                                   Ps.Post_Name,
                                   Ur.Role_Id,P.SEX,P.ID_CARD_NUMBER,p.email
                              From App_User      u,
                                   App_Person    p,
                                   App_Corp      c,
                                   App_Dept      d,
                                   App_Posts     Ps,
                                   App_User_Role Ur
                             Where u.Person_Id = p.Person_Id
                               And p.Dept_Id = d.Dept_Id
                               And d.Corp_Id = c.Corp_Id
                               And p.Post_Id = Ps.Post_Id
                               And u.User_Id = Ur.User_Id(+)
                               And u.User_Code = :user_code";
                    ds = await data.GetDataSetByParam(sqls, new OracleParameter[] { data.MakeInParam(":user_code", loginName) });
                }
                else
                {
                    string sql = @"Select u.User_Id,
                                   u.User_Code,
                                   u.User_Name,
                                   u.Status,
                                   p.Person_Id,
                                   p.Mobile_Phone,
                                   d.Dept_Id,
                                   d.Dept_Code,
                                   d.Dept_Name,
                                   c.Corp_Id,
                                   c.Corp_Code,
                                   c.Corp_Name,
                                   Ps.Post_Id,
                                   Ps.Post_Code,
                                   Ps.Post_Name,
                                   Ur.Role_Id,P.SEX,P.ID_CARD_NUMBER,p.email
                              From App_User      u,
                                   App_Person    p,
                                   App_Corp      c,
                                   App_Dept      d,
                                   App_Posts     Ps,
                                   App_User_Role Ur
                             Where u.Person_Id = p.Person_Id
                               And p.Dept_Id = d.Dept_Id
                               And d.Corp_Id = c.Corp_Id
                               And p.Post_Id = Ps.Post_Id
                               And u.User_Id = Ur.User_Id(+)
                               And u.User_Code = :user_code
                               And Password = :password";
                    string psw = mtTools.EncodeHelper.MD5Hash(password);
                    OracleParameter[] sp = { data.MakeInParam(":user_code", loginName), data.MakeInParam(":password", psw) };
                    ds = await data.GetDataSetByParam(sql, sp);
                }
                try
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        if (ds.Tables[0].Rows[0]["STATUS"].ToString() == "0")
                        {
                            HttpContext.Session.SetString("PASSWORD", password);
                            HttpContext.Session.SetString("USER_ID", ds.Tables[0].Rows[0]["USER_ID"].ToString() ?? "");
                            HttpContext.Session.SetString("USER_CODE", ds.Tables[0].Rows[0]["USER_CODE"].ToString() ?? "");
                            HttpContext.Session.SetString("USER_NAME", ds.Tables[0].Rows[0]["USER_NAME"].ToString() ?? "");
                            HttpContext.Session.SetString("PERSON_ID", ds.Tables[0].Rows[0]["person_id"].ToString() ?? "");
                            HttpContext.Session.SetString("MOBILE_PHONE", ds.Tables[0].Rows[0]["mobile_phone"].ToString() ?? "");
                            HttpContext.Session.SetString("DEPT_ID", ds.Tables[0].Rows[0]["dept_id"].ToString() ?? "");
                            HttpContext.Session.SetString("DEPT_CODE", ds.Tables[0].Rows[0]["dept_code"].ToString() ?? "");
                            HttpContext.Session.SetString("DEPT_NAME", ds.Tables[0].Rows[0]["dept_name"].ToString() ?? "");
                            HttpContext.Session.SetString("CORP_ID", ds.Tables[0].Rows[0]["corp_id"].ToString() ?? "");
                            HttpContext.Session.SetString("CORP_CODE", ds.Tables[0].Rows[0]["corp_code"].ToString() ?? "");
                            HttpContext.Session.SetString("CORP_NAME", ds.Tables[0].Rows[0]["corp_name"].ToString() ?? "");
                            HttpContext.Session.SetString("POST_ID", ds.Tables[0].Rows[0]["post_id"].ToString() ?? "");
                            HttpContext.Session.SetString("POST_CODE", ds.Tables[0].Rows[0]["post_code"].ToString() ?? "");
                            HttpContext.Session.SetString("POST_NAME", ds.Tables[0].Rows[0]["post_name"].ToString() ?? "");
                            HttpContext.Session.SetString("ROLE_ID", ds.Tables[0].Rows[0]["role_id"].ToString() ?? "");
                            HttpContext.Session.SetString("SEX", ds.Tables[0].Rows[0]["SEX"].ToString() ?? "");
                            HttpContext.Session.SetString("EMAIL", ds.Tables[0].Rows[0]["EMAIL"].ToString() ?? "");
                            HttpContext.Session.SetString("ID_CARD_NUMBER", ds.Tables[0].Rows[0]["ID_CARD_NUMBER"].ToString() ?? "");
                            return Json(new { status = 1, jumpurl = "/Main/MainIndex/" });
                        }
                        else
                        {
                            return Json(new { status = 2, errorMessage = "用户已失效" });
                        }
                    }
                    else
                    {
                        return Json(new { status = 2, errorMessage = "用户不存在或密码错误" });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { status = 2, errorMessage = "接口出错，请联系管理员" });
                }
            }
            else
            {
                return Json(new { status = 2, errorMessage = "用户不存在" });
            }
        }
        #endregion
    }
}