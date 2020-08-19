using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using WebApplication1.Models;

namespace WebApplication1.Controllers.NOTICE
{
    public class NOTICEController : Controller
    {
        DBHelper data = new DBHelper();
        DataBaseContext context;
        public NOTICEController(DataBaseContext context)
        {
            this.context = context;
        }
        [CheckCustomer]
        public IActionResult NOTICE()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult NOTICE_EDIT()
        {
            ViewBag.status = HttpContext.Request.Query["status"].ToString();
            ViewBag.NOTICE_ID = HttpContext.Request.Query["Rowid"].ToString();
            return View();
        }

        #region 获取通知
        [HttpGet]
        public async Task<IActionResult> GetNoticeForPC(string search_text, int page, int limit)
        {
            string Msg = "";
            string sql = @"SELECT *
                           FROM(SELECT ROWNUM AS rowno, r.*
                                   FROM(select (Select Count(*) From Notice where  status not in(3) and title like '%'|| :title ||'%') totalPage,
                                                    Notice_Id,
                                                    Title,
                                                    Message,
                                                    to_char(Notice_Date,'yyyy-MM-dd') Notice_Date,
                                                    Company,
                                                    Nickname,
                                                    Decode(Status, 0, '编辑', 1, '发布', 3, '取消') Status
                                                    From Notice where  status not in(3) and title like '%'|| :title1 ||'%' ) r
                                  where ROWNUM <= :page * :limit) table_alias
                          WHERE table_alias.rowno > (: page - 1) * :limit";
            OracleParameter[] sp = {data.MakeInParam(":title", search_text??""),
                                    data.MakeInParam(":title1", search_text??""),
                                     data.MakeInParam(":page", page),
                                    data.MakeInParam(":limit", limit)};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 获取通知
        [HttpGet]
        public async Task<IActionResult> GetNOTICEById(string Rowid)
        {
            string Msg = "";
            string sql = @"Select Notice_Id,
                                          Title,
                                          Message,
                                          to_char(Notice_Date,'yyyy-MM-dd') Notice_Date,
                                          Company,
                                          Nickname,
                                          Decode(Status, 0, '编辑', 1, '发布', 3, '取消') Status
                                          From Notice where Notice_Id=:Notice_Id";
            OracleParameter[] sp = {data.MakeInParam(":Notice_Id", Rowid ?? "")};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 新增
        [HttpPost]
        public async Task<IActionResult> Insert(string TITLE, string NICKNAME, string COMPANY, string MESSAGE,string NOTICE_DATE)
        {
            string sql = @"Insert Into Notice
                                  (Notice_Id,
                                   Title,
                                   Message,
                                   Notice_Date,
                                   Company,
                                   Nickname,
                                   Status,
                                   Creation_Date,
                                   Creation_By)
                                Values
                                  (Notice_s.Nextval,
                                   :Title,
                                   :Message,
                                   to_date(:Notice_Date,'yyyy-MM-dd'),
                                   :Company,
                                   :Nickname,
                                   0,
                                   Sysdate,
                                   :user_id)";
            OracleParameter[] sp = {
                data.MakeInParam(":Title",TITLE??""),
                data.MakeInParam(":Message",MESSAGE??""),
                data.MakeInParam(":Notice_Date",NOTICE_DATE),
                data.MakeInParam(":Company",COMPANY??""),
                data.MakeInParam(":Nickname",NICKNAME??""),
                data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")??"")};
            bool flag = await data.DoSqlByParam(sql, sp);
            string Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 编辑
        [HttpPost]
        public async Task<IActionResult> Modify(string TITLE, string NICKNAME, string COMPANY, string MESSAGE, string NOTICE_DATE,string Rowid)
        {
            string sql = @"Update Notice
                               Set 
                                   Title            = :Title,
                                   Message          = :Message,
                                   Notice_Date      = to_date(:Notice_Date,'yyyy-MM-dd'),
                                   Company          = :Company,
                                   Nickname         = :Nickname,
                                   Last_Update_Date = Sysdate,
                                   Last_Update_By   = :USER_ID
                             Where Notice_Id = :Notice_Id";
            OracleParameter[] sp = {
                data.MakeInParam(":Title",TITLE??""),
                data.MakeInParam(":Message",MESSAGE??""),
                data.MakeInParam(":Notice_Date",NOTICE_DATE),
                data.MakeInParam(":Company",COMPANY??""),
                data.MakeInParam(":Nickname",NICKNAME??""),
                data.MakeInParam(":USER_ID",HttpContext.Session.GetString("USER_ID")??""),
                data.MakeInParam(":Notice_Id",Rowid??""),};
            bool flag = await data.DoSqlByParam(sql, sp);
            string Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 发布
        [HttpPost]
        public async Task<IActionResult> Release(long[] id)
        {
            string Msg = "";
            using (var tran = context.Database.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < id.Length; i++)
                    {
                        var rm = await Task.FromResult(context.NOTICE.ToList().Where(u => u.NOTICE_ID.Equals(id[i])).FirstOrDefault());
                        rm.STATUS = 1;
                        rm.LAST_UPDATE_BY = int.Parse(HttpContext.Session.GetString("USER_ID"));
                        rm.LAST_UPDATE_DATE = DateTime.Now;
                        context.NOTICE.Update(rm);
                    }
                    await context.SaveChangesAsync();
                    tran.Commit();
                    Msg = "{\"code\":200,\"msg\":\"发布成功\"}";
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Msg = "{\"code\":300,\"msg\":\"发布失败，请联系管理员\"}";
                }
            }
            return Content(Msg);
        }
        #endregion

        #region 关闭
        [HttpPost]
        public async Task<IActionResult> Close(long[] id)
        {
            string Msg = "";
            using (var tran = context.Database.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < id.Length; i++)
                    {
                        var rm = await Task.FromResult(context.NOTICE.ToList().Where(u => u.NOTICE_ID.Equals(id[i])).FirstOrDefault());
                        rm.STATUS = 3;
                        rm.LAST_UPDATE_BY = int.Parse(HttpContext.Session.GetString("USER_ID"));
                        rm.LAST_UPDATE_DATE = DateTime.Now;
                        context.NOTICE.Update(rm);
                    }
                    await context.SaveChangesAsync();
                    tran.Commit();
                    Msg = "{\"code\":200,\"msg\":\"关闭成功\"}";
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Msg = "{\"code\":300,\"msg\":\"关闭失败，请联系管理员\"}";
                }
            }
            return Content(Msg);
        }
        #endregion
    }
}