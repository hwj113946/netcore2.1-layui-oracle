using System;
using System.Data;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using WebApplication1.Models;
using WebApplication1.Models.ApiModels;

namespace WebApplication1.Controllers.API.Txryhd
{
    [Route("api/Notice")]
    [Produces("application/json")]
    [EnableCors("AllowSpecificOrigin")]//跨域
    public class NoticeController : Controller
    {
        DBHelper data = new DBHelper();

        #region 获取通知
        [HttpGet]
        [Route("GetNotice")]
        public async Task<IActionResult> GetNotice()
        {
            try
            {
                string sql = @"Select t.Title, t.Nickname, t.Message, to_char(t.Notice_Date,'yyyy-MM-dd') Notice_Date, t.Company From Notice t Where t.Status = 1";
                DataSet ds = await data.GetDataSet(sql);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    return Json(new { code = 0, msg = "已查询到数据", count = ds.Tables[0].Rows.Count, data = ds.Tables[0] });
                }
                else
                {
                    return Json(new { code = 1, msg = "未查询到数据", count = 0, data = new { } });
                }
            }
            catch (Exception ex)
            {
                return Json(new { code = 1, msg = "请求出错，请联系管理员", count = 0, data = new { } });
            }
        }
        #endregion

        #region 获取政策
        [HttpGet]
        [Route("GetPolicys")]
        public async Task<IActionResult> GetPolicys(Notice_param param)
        {
            try
            {
                string sql = @"SELECT *
                      FROM(SELECT ROWNUM AS rowno, r.*
                              FROM(select to_char(ceil(COUNT(*) OVER ()/ :limit1)) totalPage,
                    To_Char(Policy_Id) Policy_Id,
                           Title,
                           Upload_Time,
                           File_Link,
                           u.User_Name Creation_By
                      From Policy p, App_User u
                     Where p.Creation_By = u.User_Id(+)
                       And p.Title Like '%' || :Title1 || '%') r
                             where ROWNUM <= :page * :limit) table_alias
                     WHERE table_alias.rowno > (: page - 1) * :limit";
                OracleParameter[] sp = {
                                     data.MakeInParam(":limit1", param.LIMIT),
                                     data.MakeInParam(":Title1", param.SEARCH_TEXT ?? ""),
                                     data.MakeInParam(":page", param.PAGE),
                                    data.MakeInParam(":limit", param.LIMIT)};
                DataSet ds = await data.GetDataSetByParam(sql, sp);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    return Json(new { code = 0, msg = "已查询到数据", count = ds.Tables[0].Rows[0]["totalPage"], data = ds.Tables[0] });
                }
                else
                {
                    return Json(new { code = 1, msg = "未查询到数据", count = 0, data = new { } });
                }
            }
            catch (Exception ex)
            {
                return Json(new { code = 1, msg = "请求接口出错", count = 0, data = new { } });
            }
        }
        #endregion
    }
}