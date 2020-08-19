using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using WebApplication1.Models;
using System.Collections; 
namespace WebApplication1.Controllers.TEST_APPR
{
    public class TEST_APPRController : Controller
    {
        DBHelper data = new DBHelper();

        [CheckCustomer]
        public IActionResult TEST_APPR()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult TEST_APPR_EDIT()
        {
            ViewBag.status = HttpContext.Request.Query["status"].ToString();
            ViewBag.test_id = ViewBag.status == "add" ? "" : HttpContext.Request.Query["Rowid"].ToString();
            return View();
        }

        #region 获取全部：分页
        [HttpGet]
        public async Task<IActionResult> GetTEST_APPR(string search_text, int page, int limit)
        {
            string sql=@"SELECT * FROM(SELECT ROWNUM AS rowno, r.* FROM(select (Select Count(*) From TEST_APPR where  TITLE like '%'|| :search_text ||'%') totalPage,
TEST_ID
,TITLE
,CONTENT
,STATUS
,LAST_UPDATE_DATE
,LAST_UPDATE_BY
,CREATION_DATE
,CREATION_BY
,APPR_ID
 from TEST_APPR where TITLE like '%' || :search_text1  || '%') r
                                  where ROWNUM <= :page * :limit) table_alias
                          WHERE table_alias.rowno > (: page - 1) * :limit";
            OracleParameter[] sp = {data.MakeInParam(":search_text", search_text??""),
                                    data.MakeInParam(":search_text1", search_text??""),
                                     data.MakeInParam(":page", page),
                                    data.MakeInParam(":limit", limit)};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            if(ds.Tables[0].Rows.Count > 0)
{
                return Json(new { code=0,msg="已查询到数据",count=ds.Tables[0].Rows[0]["totalPage"],data= ds.Tables[0] });
            }
            else
            {
                return Json(new { code = 1, msg = "未查询到数据", count = 0, data = new { } });
            }
        }
        #endregion

        #region 获取：根据ID查询
        [HttpGet]
        public async Task<IActionResult> GetTEST_APPRById(string test_id)
        {
            string sql=@"Select TEST_ID
,TITLE
,CONTENT
,STATUS
,LAST_UPDATE_DATE
,LAST_UPDATE_BY
,CREATION_DATE
,CREATION_BY
,APPR_ID
 from TEST_APPR where TEST_ID = :test_id ";
            OracleParameter[] sp = {data.MakeInParam(":test_id", test_id ??"")};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            if(ds.Tables[0].Rows.Count > 0)
{
                return Json(new { code=0,msg="已查询到数据",count=ds.Tables[0].Rows.Count,data= ds.Tables[0] });
            }
            else
            {
                return Json(new { code = 1, msg = "未查询到数据", count = 0, data = new { } });
            }
        }
        #endregion

        #region 新增
        [HttpPost]
        public async Task<IActionResult> Insert(string title,string content,string status,string last_update_date,string last_update_by,string creation_date,string creation_by,string appr_id)
        {
            string sql = @"insert into TEST_APPR( test_id
,title
,content
,status
)
                                values( TEST_APPR_s.nextval 
,:title
,:content
,0
 ) ";
            OracleParameter[] sp = {
data.MakeInParam("title",title??"")
,data.MakeInParam("content",content??"")
,};
            bool flag = await data.DoSqlByParam(sql, sp);
            return flag ? Json(new { code = 200, msg = "保存成功" }) : Json(new { code = 300, msg = "保存失败，请联系管理员" });
        }
        #endregion

        #region 编辑
        [HttpPost]
        public async Task<IActionResult> Modify(string test_id,string title,string content,string status,string last_update_date,string last_update_by,string creation_date,string creation_by,string appr_id)
        {
            string sql=@"Update TEST_APPR set 
                                    title = :title
,                                   content = :content
,                                   last_update_date = sysdate
,                                   last_update_by = :last_update_by
             where test_id = :test_id ";            OracleParameter[] sp = {
data.MakeInParam(":title",title??""),
data.MakeInParam(":content",content??""),
data.MakeInParam(":last_update_by",HttpContext.Session.GetString("USER_ID")??"")
};
            bool flag = await data.DoSqlByParam(sql, sp);
            return flag ? Json(new { code = 200, msg = "保存成功" }) : Json(new { code = 300, msg = "保存失败，请联系管理员" });
        }
        #endregion

        #region 删除
        [HttpPost]
        public async Task<IActionResult> Delete(string[] test_id)
        {
            Hashtable ht = new Hashtable();
            for (int i = 0; i < test_id.Length; i++)
            {
            ht.Add(@"delete from TEST_APPR where TEST_ID =:test_id"+i,new OracleParameter[] { data.MakeInParam(":test_id"+i,test_id[i].ToString()??"")});
            }
            bool flag = await data.DoSqlList(ht);
            return flag ? Json(new { code = 200, msg = "删除成功" }) : Json(new { code = 300, msg = "删除失败，请联系管理员" });
        }
        #endregion
        

    }
}