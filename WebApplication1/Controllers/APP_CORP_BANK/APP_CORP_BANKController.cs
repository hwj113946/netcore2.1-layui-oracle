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
namespace WebApplication1.Controllers.APP_CORP_BANK
{
    public class APP_CORP_BANKController : Controller
    {
        DBHelper data = new DBHelper();
        DataBaseContext context;
        public APP_CORP_BANKController(DataBaseContext context)
        {
            this.context = context;
        }

        [CheckCustomer]
        public IActionResult APP_CORP_BANK()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult APP_CORP_BANK_EDIT()
        {
            ViewBag.status = HttpContext.Request.Query["status"].ToString();
            ViewBag.corp_bank_id = ViewBag.status == "add" ? "" : HttpContext.Request.Query["Rowid"].ToString();
            ViewBag.CORP_ID = HttpContext.Session.GetString("CORP_ID");
            return View();
        }

        #region 获取全部：分页
        [HttpGet]
        public async Task<IActionResult> GetAPP_CORP_BANK(string search_text, int page, int limit)
        {
            string sql= @"SELECT * FROM(SELECT ROWNUM AS rowno, r.* FROM(
                                select (Select Count(*) From  APP_CORP_BANK b,app_corp c where b.corp_id=c.corp_id
                            and c.corp_name like '%'|| :search_text ||'%') totalPage,
                                b.CORP_BANK_ID
                                ,b.CORP_ID,c.corp_name
                                ,b.BANK_PROVINCE
                                ,b.BANK_CITY
                                ,b.BANK_NAME
                                ,b.BANK_ACCOUNT
                                ,b.BANK_NO
                                ,to_char(b.START_DATE,'yyyy-MM-dd') START_DATE
                                ,to_char(b.END_DATE,'yyyy-MM-dd') END_DATE
                                ,decode(b.STATUS,0,'编辑',1,'确定',3,'取消') STATUS
                                ,b.NOTE
                                 from APP_CORP_BANK b,app_corp c where b.corp_id=c.corp_id
                                and c.corp_name like '%' || :search_text1  || '%' order by c.corp_name) r
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
        public async Task<IActionResult> GetAPP_CORP_BANKById(string corp_bank_id)
        {
                string sql= @"Select CORP_BANK_ID
                                ,CORP_ID
                                ,BANK_PROVINCE
                                ,BANK_CITY
                                ,BANK_NAME
                                ,BANK_ACCOUNT
                                ,BANK_NO
                                ,to_char(START_DATE,'yyyy-MM-dd') START_DATE
                                ,to_char(END_DATE,'yyyy-MM-dd') END_DATE
                                ,STATUS
                                ,NOTE
                                 from APP_CORP_BANK where CORP_BANK_ID = :corp_bank_id ";
            OracleParameter[] sp = {data.MakeInParam(":corp_bank_id", corp_bank_id ??"")};
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
        public async Task<IActionResult> Insert(string corp_id,string bank_province,string bank_city,string bank_name,string bank_account,string bank_no,string start_date,string end_date,string note)
        {
            string sql = @"insert into APP_CORP_BANK( corp_bank_id
                                                     ,corp_id
                                                     ,bank_province
                                                     ,bank_city
                                                     ,bank_name
                                                     ,bank_account
                                                     ,bank_no
                                                     ,start_date
                                                     ,end_date
                                                     ,status
                                                     ,note
                                                     ,creation_date
                                                     ,created_by
                                                     )
                                                     values( APP_CORP_BANK_s.nextval 
                                                     ,:corp_id
                                                     ,:bank_province
                                                     ,:bank_city
                                                     ,:bank_name
                                                     ,:bank_account
                                                     ,:bank_no
                                                     ,to_date(:start_date,'yyyy-MM-dd')
                                                     ,to_date(:end_date,'yyyy-MM-dd')
                                                     ,1
                                                     ,:note
                                                     ,sysdate
                                                     ,:created_by
                                                      ) ";
            OracleParameter[] sp = {
                        data.MakeInParam("corp_id",corp_id??"")
                        ,data.MakeInParam("bank_province",bank_province??"")
                        ,data.MakeInParam("bank_city",bank_city??"")
                        ,data.MakeInParam("bank_name",bank_name??"")
                        ,data.MakeInParam("bank_account",bank_account??"")
                        ,data.MakeInParam("bank_no",bank_no??"")
                        ,data.MakeInParam("start_date",start_date??"")
                        ,data.MakeInParam("end_date",end_date??"")
                        ,data.MakeInParam("note",note??"")
                        ,data.MakeInParam("created_by",HttpContext.Session.GetString("USER_ID")??"")
                        ,};
            bool flag = await data.DoSqlByParam(sql, sp);
            return flag ? Json(new { code = 200, msg = "保存成功" }) : Json(new { code = 300, msg = "保存失败，请联系管理员" });
        }
        #endregion

        #region 编辑
        [HttpPost]
        public async Task<IActionResult> Modify(string corp_bank_id,string corp_id,string bank_province,string bank_city,string bank_name,string bank_account,string bank_no,string start_date,string end_date,string note)
        {
            if (corp_bank_id==null||corp_bank_id=="")
            {
                return Json(new { code = 300, msg = "请传入主键" });
            }
            string sql= @"Update APP_CORP_BANK set 
                                 corp_id = :corp_id
                                ,bank_province = :bank_province
                                ,bank_city = :bank_city
                                ,bank_name = :bank_name
                                ,bank_account = :bank_account
                                ,bank_no = :bank_no
                                ,start_date = to_date(:start_date,'yyyy-MM-dd')
                                ,end_date = to_date(:end_date,'yyyy-MM-dd')
                                ,note = :note
                                ,last_update_date = sysdate
                                ,last_updated_by = :last_updated_by
             where corp_bank_id = :corp_bank_id ";
            OracleParameter[] sp = {
                            data.MakeInParam(":corp_id",corp_id??""),
                            data.MakeInParam(":bank_province",bank_province??""),
                            data.MakeInParam(":bank_city",bank_city??""),
                            data.MakeInParam(":bank_name",bank_name??""),
                            data.MakeInParam(":bank_account",bank_account??""),
                            data.MakeInParam(":bank_no",bank_no??""),
                            data.MakeInParam(":start_date",start_date??""),
                            data.MakeInParam(":end_date",end_date??""),
                            data.MakeInParam(":note",note??""),
                            data.MakeInParam(":last_updated_by",HttpContext.Session.GetString("USER_ID")??""),
                            data.MakeInParam(":corp_bank_id",corp_bank_id??"")
                            };
            bool flag = await data.DoSqlByParam(sql, sp);
            return flag ? Json(new { code = 200, msg = "保存成功" }) : Json(new { code = 300, msg = "保存失败，请联系管理员" });
        }
        #endregion

        #region 删除
        [HttpPost]
        public async Task<IActionResult> Delete(string[] corp_bank_id)
        {
            Hashtable ht = new Hashtable();
            for (int i = 0; i < corp_bank_id.Length; i++)
            {
            ht.Add(@"delete from APP_CORP_BANK where CORP_BANK_ID =:corp_bank_id"+i,new OracleParameter[] { data.MakeInParam(":corp_bank_id"+i,corp_bank_id[i].ToString()??"")});
            }
            bool flag = await data.DoSqlList(ht);
            return flag ? Json(new { code = 200, msg = "删除成功" }) : Json(new { code = 300, msg = "删除失败，请联系管理员" });
        }
        #endregion

        #region 导出
        [HttpPost]
        public async Task<IActionResult> GetAPP_CORP_BANKForExport(string search_text, string status)
        {
            string sql = @"select c.corp_name
                            ,b.BANK_PROVINCE
                            ,b.BANK_CITY
                            ,b.BANK_NAME
                            ,b.BANK_ACCOUNT
                            ,b.BANK_NO
                            ,to_char(b.START_DATE,'yyyy-MM-dd') START_DATE
                            ,to_char(b.END_DATE,'yyyy-MM-dd') END_DATE
                            ,decode(b.STATUS,0,'编辑',1,'确定',3,'取消') STATUS
                            ,b.NOTE
                             from APP_CORP_BANK b,app_corp c where b.corp_id=c.corp_id and c.corp_name like '%'|| :search_text || '%' order by c.corp_name";
            OracleParameter[] sp = { data.MakeInParam(":search_text", search_text ?? "") };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            IList<Excelapp_corp_bank> list = data.DataSetToIList1<Excelapp_corp_bank>(ds, 0);
            byte[] buffer = new byte[0]; if (list == null) { }
            else
            {
                List<Excelapp_corp_bank> import = list.ToList(); if (import == null) { }
                else
                {
                    buffer = ExcelHelper.Export(import, "数据导出", ExcelTitle.app_corp_bank).GetBuffer();
                }
            }
            return File(buffer, "application/ms-excel", "数据导出(" + DateTime.Now + ").xls");
        }
        #endregion

        #region 导入
        [HttpPost]
        public async Task<IActionResult> FileUpload_APP_CORP_BANK([FromForm]IFormFile file)
        {
            var json = new { code = 1, msg = "请求失败", returnMsg = "" };
            string returnMsg = "";
            if (file.Length > 0)
            {
                long fileSize = file.Length / 5242880;
                if (fileSize > 15)
                {
                    json = new { code = 1, msg = "文件不能大于15M", returnMsg = "" };
                }
                else
                {
                    string path = Startup.HostingEnvironment.WebRootPath + "\\" + Guid.NewGuid() + file.FileName;
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    ExcelHelper excel = new ExcelHelper();
                    List<Excelapp_corp_bankImport> ec = excel.GetList<Excelapp_corp_bankImport>(path).ToList();
                    if (ec.Count > 0)
                    {
                        for (int i = 0; i < ec.Count; i++)
                        {
                            if (ec[i].CORP_NAME == null || ec[i].CORP_NAME == "")
                            {
                                returnMsg +="公司名称不能为空。";
                            }
                            else
                            {
                                if (ec[i].BANK_ACCOUNT == null || ec[i].BANK_ACCOUNT == "")
                                {
                                    returnMsg += "【" + ec[i].CORP_NAME + "】：银行账号不能为空。";
                                }
                                else
                                {

                                    string isExist = await data.GetStringByParam(@"select corp_id from app_corp where corp_name=:corp_name",
                                    new OracleParameter[] { data.MakeInParam(":corp_name", ec[i].CORP_NAME ?? "") });
                                    if (isExist == "")
                                    {
                                        returnMsg += "【" + ec[i].CORP_NAME + "】：在公司表中不存在，请先添加公司信息。";
                                    }
                                    else
                                    {
                                        string sql = @"Insert Into App_Corp_Bank
                                                                  (Corp_Bank_Id,
                                                                   Corp_Id,
                                                                   Bank_Province,
                                                                   Bank_City,
                                                                   Bank_Name,
                                                                   Bank_Account,
                                                                   Bank_No,
                                                                   Start_Date,
                                                                   End_Date,
                                                                   Status,
                                                                   Note,
                                                                   Creation_Date,
                                                                   Created_By)
                                                                Values
                                                                  (App_Corp_Bank_s.Nextval,
                                                                   :Corp_Id,
                                                                   :Bank_Province,
                                                                   :Bank_City,
                                                                   :Bank_Name,
                                                                   :Bank_Account,
                                                                   :Bank_No,
                                                                   to_date(:Start_Date,'yyyy-MM-dd'),
                                                                   to_date(:End_Date,'yyyy-MM-dd'),
                                                                   1,
                                                                   :Note,
                                                                   Sysdate,
                                                                   :Created_By)";
                                        OracleParameter[] sp = {
                                            data.MakeInParam(":Corp_Id", isExist??""),
                                            data.MakeInParam(":Bank_Province", ec[i].BANK_PROVINCE??""),
                                            data.MakeInParam(":Bank_City", ec[i].BANK_CITY??""),
                                            data.MakeInParam(":Bank_Name", ec[i].BANK_NAME??""),
                                            data.MakeInParam(":Bank_Account", ec[i].BANK_ACCOUNT??""),
                                            data.MakeInParam(":Bank_No",ec[i].BANK_NO??""),
                                            data.MakeInParam(":Start_Date", ec[i].START_DATE??""),
                                            data.MakeInParam(":End_Date", ec[i].END_DATE??""),
                                            data.MakeInParam(":Note", ec[i].NOTE??""),
                                            data.MakeInParam(":Created_By", HttpContext.Session.GetString("USER_ID")),
                                        };
                                        bool flag = await data.DoSqlByParam(sql, sp);
                                        if (flag)
                                        {
                                            returnMsg += "【" + ec[i].CORP_NAME + "】" + ec[i].BANK_ACCOUNT + "：导入成功。";
                                        }
                                        else
                                        {
                                            returnMsg += "【" + ec[i].CORP_NAME + "】" + ec[i].BANK_ACCOUNT + "：在导入数据时发生错误，请联系管理员。";
                                        }
                                    }
                                }
                            }              
                        }
                        json = new { code = 0, msg = "执行完成", returnMsg = returnMsg };
                    }
                    else
                    {
                        json = new { code = 1, msg = "Excel中没有数据", returnMsg = "" };
                    }
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
            }
            return Json(json);
        }
        #endregion

    }
}