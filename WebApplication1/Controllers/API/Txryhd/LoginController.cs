using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using WebApplication1.Models.ApiModels;
using System.Net.Http;
using System;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace WebApplication1.Controllers.API.Txryhd
{
    [Route("api/Login")]
    [Produces("application/json")]
    [EnableCors("AllowSpecificOrigin")]//跨域
    public class LoginController : Controller
    {
        DBHelper data = new DBHelper();
        DataBaseContext context;
        private IHttpClientFactory _httpClient;
        private readonly ILogger<LoginController> _logger;
        public LoginController(DataBaseContext context, IHttpClientFactory _httpClient,ILogger<LoginController> logger)
        {
            this.context = context;
            this._httpClient = _httpClient;
            _logger = logger;
        }

        #region 工作人员登录
        [Route("MGLogin")]
        [HttpPost]
        public async Task<IActionResult> MGLogin(Login login)
        {
            if (login.APPID==null)
            {
                return Json(new { code = 1, msg = "请传入APPID" });
            }
            else
            {
                var result = "";
                var open_id = "";
                var session_key = "";
                try
                {
                    //设置请求的路径
                    var url = "https://api.weixin.qq.com/sns/jscode2session?appid=" + login.APPID + "&secret=" + login.SECRET + "&js_code=" + login.JS_CODE + "&grant_type=" + login.GRANT_TYPE;
                    //使用注入的httpclientfactory获取client
                    var client = _httpClient.CreateClient();
                    client.BaseAddress = new Uri(url);
                    //设置请求体中的内容，并以post的方式请求
                    var response = await client.GetAsync(url);
                    //获取请求到数据，并转化为字符串
                    result = response.Content.ReadAsStringAsync().Result;
                    JObject j = JObject.Parse(result);
                    open_id = j["openid"].ToString() ?? "";
                    session_key = j["session_key"].ToString() ?? "";
                }
                catch (Exception ex)
                {
                    _logger.LogError("微信接口返回："+result+"。程序捕获异常："+ex.Message);
                    return Json(new { code = 1, msg = "请求微信小程序标识出错" });
                }
                string sql = @"select * from app_user where open_id=:open_id";
                OracleParameter[] sp = {data.MakeInParam(":open_id",open_id??"") };
                DataSet ds = await data.GetDataSetByParam(sql,sp);
                _logger.LogInformation("返回集合行数："+ds.Tables[0].Rows.Count);
                if (ds.Tables[0].Rows.Count==1)
                {
                    if ((ds.Tables[0].Rows[0]["STATUS"].ToString() ?? "") == "0")
                    {
                        _logger.LogInformation("["+ ds.Tables[0].Rows[0]["USER_CODE"] + "]"+ds.Tables[0].Rows[0]["USER_NAME"]+"登录成功");
                        return Json(new { code = 0, msg = "登录成功", data = ds.Tables[0] });
                    }
                    else
                    {
                        _logger.LogInformation("[" + ds.Tables[0].Rows[0]["USER_CODE"] + "]" + ds.Tables[0].Rows[0]["USER_NAME"] + "账户已失效");
                        return Json(new { code = 1, msg = "账户已失效" });
                    }
                }
                else
                {
                    if (login.USER_CODE==null||login.USER_CODE=="")
                    {
                        _logger.LogInformation("open_id(" + open_id + ")未在数据库中，且传入的USER_CODE是空的");
                        return Json(new { code = 1, msg = "未绑定用户信息" });
                    }
                    else
                    {
                        string sql1 = @"select * from app_user where user_code=:user_code and password=:password";
                        sp = new OracleParameter[] { data.MakeInParam(":user_code",login.USER_CODE??""),
                        data.MakeInParam(":password",mtTools.EncodeHelper.MD5Hash(login.PASSWORD??""))};
                        ds = await data.GetDataSetByParam(sql1, sp);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            if ((ds.Tables[0].Rows[0]["STATUS"].ToString()??"")=="0")
                            {
                                await data.DoSqlByParam(@"update app_user set open_id=:open_id where user_id=:user_id",
                                new OracleParameter[] { data.MakeInParam(":open_id", open_id ?? "") ,
                                data.MakeInParam(":user_id", ds.Tables[0].Rows[0]["USER_ID"] ?? "") });
                                _logger.LogInformation("open_id(" + open_id + ")写入数据库，用户["+ds.Tables[0].Rows[0]["USER_CODE"]+"]"+ ds.Tables[0].Rows[0]["USER_NAME"]+"登录成功");
                                return Json(new { code = 0, msg = "登录成功", data = ds.Tables[0] });
                            }
                            else
                            {
                                _logger.LogInformation("[" + ds.Tables[0].Rows[0]["USER_CODE"] + "]" + ds.Tables[0].Rows[0]["USER_NAME"] + "登录成功");
                                return Json(new { code = 1, msg = "账户已失效" });
                            }                            
                        }
                        else
                        {
                            _logger.LogInformation("[" + ds.Tables[0].Rows[0]["USER_CODE"] + "]" + ds.Tables[0].Rows[0]["USER_NAME"] + "账户或密码错误(user_code："+login.USER_CODE+"，password："+login.PASSWORD+")");
                            return Json(new { code = 1, msg = "账户或密码错误" });
                        }
                    }
                }
            }
        }
        #endregion

        #region 工作人员注销
        [Route("MGLogOut")]
        [HttpPost]
        public async Task<IActionResult> MGLogOut(Login login)
        {
            if (login.USER_ID==null||login.USER_ID=="")
            {
                return Json(new { code = 1, msg = "请传入账户主键" });
            }
            else
            {
                string sql = @"update app_user set open_id='' where user_id=:user_id";
                OracleParameter[] sp ={ data.MakeInParam(":user_id", login.USER_ID??"") };
                bool flag = await data.DoSqlByParam(sql,sp);
                return flag ? Json(new { code = 0, msg = "注销成功" }) : Json(new { code = 0, msg = "注销成功" });
            }
        }
        #endregion

        #region 退休人员登录
        [Route("PersonLogin")]
        [HttpPost]
        public async Task<IActionResult> PersonLogin(Login login)
        {
            if (login.APPID == null)
            {
                return Json(new { code = 1, msg = "请传入APPID" });
            }
            else
            {
                var result = "";
                var open_id = "";
                var session_key = "";
                try
                {
                    //设置请求的路径
                    var url = "https://api.weixin.qq.com/sns/jscode2session?appid=" + login.APPID + "&secret=" + login.SECRET + "&js_code=" + login.JS_CODE + "&grant_type=" + login.GRANT_TYPE;
                    //使用注入的httpclientfactory获取client
                    var client = _httpClient.CreateClient();
                    client.BaseAddress = new Uri(url);
                    //设置请求体中的内容，并以post的方式请求
                    var response = await client.GetAsync(url);
                    //获取请求到数据，并转化为字符串
                    result = response.Content.ReadAsStringAsync().Result;
                    JObject j = JObject.Parse(result);
                    open_id = j["openid"].ToString() ?? "";
                    session_key = j["session_key"].ToString() ?? "";
                }
                catch (Exception ex)
                {
                    return Json(new { code = 1, msg = "请求微信小程序标识出错" });
                }
                string sql = @"select * from TXRY_PERSON where open_id=:open_id";
                OracleParameter[] sp = { data.MakeInParam(":open_id", open_id ?? "") };
                DataSet ds = await data.GetDataSetByParam(sql, sp);
                if (ds.Tables[0].Rows.Count==1)
                {
                    return Json(new { code = 0, msg = "登录成功", data = ds.Tables[0] });
                }
                else
                {
                    if (login.PERSON_NAME == null || login.PERSON_NAME == "")
                    {
                        return Json(new { code = 1, msg = "未绑定用户信息" });
                    }
                    else
                    {
                        string sql1 = @"select * from TXRY_PERSON where person_name=:person_name and id_card_number=:id_card_number";
                        sp = new OracleParameter[] { data.MakeInParam(":person_name",login.PERSON_NAME??""),
                        data.MakeInParam(":id_card_number",login.ID_CARD_NUMBER??"")};
                        ds = await data.GetDataSetByParam(sql1, sp);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            await data.DoSqlByParam(@"update TXRY_PERSON set open_id=:open_id where person_id=:person_id",
                                new OracleParameter[] { data.MakeInParam(":open_id", open_id ?? "") ,
                                data.MakeInParam(":person_id", ds.Tables[0].Rows[0]["PERSON_ID"] ?? "") });
                            return Json(new { code = 0, msg = "登录成功", data = ds.Tables[0] });
                        }
                        else
                        {
                            return Json(new { code = 1, msg = "账户或密码错误" });
                        }
                    }
                }
            }
        }
        #endregion

        #region 退休人员注销
        [Route("PersonLogOut")]
        [HttpPost]
        public async Task<IActionResult> PersonLogOut(Login login)
        {
            if (login.PERSON_ID == null || login.PERSON_ID == "")
            {
                return Json(new { code = 1, msg = "请传入人员主键" });
            }
            else
            {
                string sql = @"update TXRY_PERSON set open_id='' where person_id=:person_id";
                OracleParameter[] sp = { data.MakeInParam(":person_id", login.PERSON_ID ?? "") };
                bool flag = await data.DoSqlByParam(sql, sp);
                return flag ? Json(new { code = 0, msg = "注销成功" }) : Json(new { code = 0, msg = "注销成功" });
            }
        }
        #endregion

        #region 工作人员登录
        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login(Login login)
        {
            if (login.APPID == null)
            {
                return Json(new { code = 1, msg = "请传入APPID" });
            }
            else
            {
                var result = "";
                var open_id = "";
                var session_key = "";
                try
                {
                    //设置请求的路径
                    var url = "https://api.weixin.qq.com/sns/jscode2session?appid=" + login.APPID + "&secret=" + login.SECRET + "&js_code=" + login.JS_CODE + "&grant_type=" + login.GRANT_TYPE;
                    //使用注入的httpclientfactory获取client
                    var client = _httpClient.CreateClient();
                    client.BaseAddress = new Uri(url);
                    //设置请求体中的内容，并以post的方式请求
                    var response = await client.GetAsync(url);
                    //获取请求到数据，并转化为字符串
                    result = response.Content.ReadAsStringAsync().Result;
                    JObject j = JObject.Parse(result);
                    open_id = j["openid"].ToString() ?? "";
                    session_key = j["session_key"].ToString() ?? "";
                }
                catch (Exception ex)
                {
                    _logger.LogError("微信接口返回：" + result + "。程序捕获异常：" + ex.Message);
                    return Json(new { code = 1, msg = "请求微信小程序标识出错" });
                }
                Regex regex = new Regex("^(?=.*[a-zA-Z])(?=.*[0-9])(?=.*[._~!@#$^&*])[A-Za-z0-9._~!@#$^&*]{8,20}$");
                string sql = @"select * from app_user where open_id=:open_id";
                string sql1 = @"select * from TXRY_PERSON where open_id=:open_id";
                OracleParameter[] sp = { data.MakeInParam(":open_id", open_id ?? "") };
                DataSet ds = await data.GetDataSetByParam(sql, sp);
                DataSet ds1 = await data.GetDataSetByParam(sql1, sp);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    if ((ds.Tables[0].Rows[0]["STATUS"].ToString() ?? "") == "0")
                    {
                        var time = await data.GetStringByParam(@"Select To_Date(To_Char(Sysdate, 'yyyymmdd'), 'YYYY-MM-DD') -
                    To_Date(To_Char(Nvl(Modifypassworddate, Sysdate - 90), 'yyyymmdd'), 'YYYY-MM-DD') From App_User where user_id=:user_id",
                    new OracleParameter[] { data.MakeInParam(":user_id", ds.Tables[0].Rows[0]["USER_ID"]) });
                        return Json(new { code = 0, msg = "登录成功", needModifyPassword = int.Parse(time) >= 90 ? true : false, type = "工作人员", data = ds.Tables[0] });
                    }
                    else
                    {
                        return Json(new { code = 1, msg = "账户已失效" });
                    }
                }
                else
                {
                    if (ds1.Tables[0].Rows.Count > 0)
                    {
                        return Json(new { code = 0, msg = "登录成功", type = "退休人员", data = ds1.Tables[0] });
                    }
                    else
                    {
                        if (login.USER_CODE == null || login.USER_CODE == "")
                        {
                            if (login.PERSON_NAME == null || login.PERSON_NAME == "")
                            {
                                return Json(new { code = 1, msg = "未绑定用户信息" });
                            }
                            else
                            {
                                string sql2 = @"select * from TXRY_PERSON where person_name=:person_name and id_card_number=:id_card_number";
                                sp = new OracleParameter[] { data.MakeInParam(":person_name",login.PERSON_NAME??""),
                                        data.MakeInParam(":id_card_number",login.ID_CARD_NUMBER??"")};
                                ds1 = await data.GetDataSetByParam(sql2, sp);
                                if (ds1.Tables[0].Rows.Count > 0)
                                {
                                    await data.DoSqlByParam(@"update TXRY_PERSON set open_id=:open_id where person_id=:person_id",
                                        new OracleParameter[] { data.MakeInParam(":open_id", open_id ?? "") ,
                                data.MakeInParam(":person_id", ds1.Tables[0].Rows[0]["PERSON_ID"] ?? "") });
                                    return Json(new { code = 0, msg = "登录成功",type="退休人员", data = ds1.Tables[0] });
                                }
                                else
                                {
                                    return Json(new { code = 1, msg = "姓名与身份证信息不匹配，请检查" });
                                }
                            }
                        }
                        else
                        {
                            string sql3 = @"select * from app_user where user_code=:user_code and password=:password";
                            sp = new OracleParameter[] { data.MakeInParam(":user_code",login.USER_CODE??""),
                                    data.MakeInParam(":password",mtTools.EncodeHelper.MD5Hash(login.PASSWORD??""))};
                            ds = await data.GetDataSetByParam(sql3, sp);
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                if ((ds.Tables[0].Rows[0]["STATUS"].ToString() ?? "") == "0")
                                {
                                    await data.DoSqlByParam(@"update app_user set open_id=:open_id where user_id=:user_id",
                                    new OracleParameter[] { data.MakeInParam(":open_id", open_id ?? "") ,
                                        data.MakeInParam(":user_id", ds.Tables[0].Rows[0]["USER_ID"] ?? "") });
                                    var time = await data.GetStringByParam(@"Select To_Date(To_Char(Sysdate, 'yyyymmdd'), 'YYYY-MM-DD') -
                                        To_Date(To_Char(Nvl(Modifypassworddate, Sysdate - 90), 'yyyymmdd'), 'YYYY-MM-DD') From App_User where user_id=:user_id",
                                        new OracleParameter[] { data.MakeInParam(":user_id", ds.Tables[0].Rows[0]["USER_ID"]) });
                                                   
                                    return Json(new { code = 0, msg = "登录成功", type = "工作人员", needModifyPassword = int.Parse(time) >= 90 ? true : (regex.IsMatch(login.PASSWORD) ? false : true) , data = ds.Tables[0] });
                                }
                                else
                                {
                                    return Json(new { code = 1, msg = "账户已失效" });
                                }
                            }
                            else
                            {
                                return Json(new { code = 1, msg = "账户或密码错误" });
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 工作人员修改密码
        [Route("ModifyPassword")]
        [HttpPost]
        public async Task<IActionResult> ModifyPassword(Login login)
        {
            if (login.USER_ID == null)
            {
                return Json(new { code = 300, msg = "请传入关键值" });
            }
            var old = await data.GetStringByParam(@"select password from app_user where user_id=:user_id",
                    new OracleParameter[] { data.MakeInParam(":user_id", login.USER_ID) });
            if (mtTools.EncodeHelper.MD5Hash(login.OLD_PASSWORD) == old)
            {
                Regex regex = new Regex("^(?=.*[a-zA-Z])(?=.*[0-9])(?=.*[._~!@#$^&*])[A-Za-z0-9._~!@#$^&*]{8,20}$");
                if (regex.IsMatch(login.PASSWORD))
                {
                    if (mtTools.EncodeHelper.MD5Hash(login.PASSWORD) == old)
                    {
                        return Json(new { code = 300, msg = "新密码与原密码一致，无法修改" });
                    }
                    string sql = @"update app_user
                               set password         = :psw,
                                   last_update_by   = :user_id,
                                   last_update_date = sysdate,MODIFYPASSWORDDATE=sysdate
                             where user_id = :user_id1";
                    OracleParameter[] sp = { data.MakeInParam(":psw",mtTools.EncodeHelper.MD5Hash(login.PASSWORD)),
                                     data.MakeInParam(":user_id",login.USER_ID),
                                     data.MakeInParam(":user_id1",login.USER_ID)};
                    bool flag = await data.DoSqlByParam(sql, sp);
                    return flag ? Json(new { code = 200, msg = "更改成功" }) : Json(new { code = 300, msg = "更改失败,请联系管理员" });
                }
                else
                {
                    return Json(new { code = 300, msg = "密码不符合要求（长度8-20位，数字、字母、特殊符号组合；特殊符号包含（ . _ ~ ! @ # $ ^ & * ））" });
                }
            }
            else
            {
                return Json(new { code = 300, msg = "原密码输入错误" });
            }
        } 
        #endregion
    }
}