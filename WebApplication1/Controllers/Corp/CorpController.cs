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

namespace WebApplication1.Controllers.Corp
{
    public class CorpController : Controller
    {
        DBHelper data = new DBHelper();

        [CheckCustomer]
        public IActionResult Corp()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult EditCorp()
        {
            ViewBag.status = HttpContext.Request.Query["status"];
            ViewBag.corp_id = ViewBag.status == "add" ? "" : HttpContext.Request.Query["Rowid"].ToString();
            return View();
        }

        #region 获取公司
        [HttpPost]
        public async Task<IActionResult> GetCorp(string corp_name, string status, int limit, int page)
        {
            string Msg = "";
            string sql = @"SELECT *
  FROM (SELECT ROWNUM AS rowno, r.*
          FROM (select corp_id,
                       corp_code,
                       corp_name,
                       detailed_address,
                       law_person_name,
                       fax,
                       zip,
                       tax_rq_number,
                       e_mail,
                       status,
                       note,
                       attribute1
                  from app_corp
                 where (corp_name like '%' || :corp_name || '%' or
                       corp_code like '%' || :corp_code || '%' or
                       attribute1 like '%' || :attribute1 || '%')
                   and status = :status) r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (:page - 1) * :limit";
            string sql1 = @"select count(*)
                  from app_corp
                 where (corp_name like '%' || :corp_name || '%' or
                       corp_code like '%' || :corp_code || '%' or
                       attribute1 like '%' || :attribute1 || '%')
                   and status = :status";
            OracleParameter[] sp1 = { data.MakeInParam(":corp_name", corp_name ?? ""), data.MakeInParam(":corp_code", corp_name ?? ""), data.MakeInParam(":attribute1", corp_name ?? ""), data.MakeInParam(":status", status) };
            string n = await data.GetStringByParam(sql1,sp1);
            OracleParameter[] sp = { data.MakeInParam(":corp_name", corp_name ?? ""), data.MakeInParam(":corp_code", corp_name ?? ""), data.MakeInParam(":attribute1", corp_name ?? ""), data.MakeInParam(":status", status), data.MakeInParam(":page", page), data.MakeInParam(":limit", limit) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + n + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 删除公司
        [HttpPost]
        public async Task<IActionResult> DeleteCorp(string[] id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"delete from app_corp where corp_id=:corp_id"+i,new OracleParameter[]
                    {
                        data.MakeInParam(":corp_id"+i,  id[i])
                    });
            }
            if (ht.Count>0)
            {
                //string sql = "declare str varchar2(1000);str2 varchar2(3000); begin str:=:corp_id;str2:='delete from app_corp where corp_id in('||str||')'; execute immediate str2; end;";
                //OracleParameter[] sp = { data.MakeInParam(":corp_id", OracleDbType.Varchar2, 3000, id) };
                bool flag = await data.DoSqlList(ht); //data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"删除成功\"}" : "{\"code\":300,\"msg\":\"删除失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中公司\"}");
            }
            
        }
        #endregion

        #region 启用公司
        [HttpPost]
        public async Task<IActionResult> EnableStatusForCorp(string[] id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"Update App_Corp
                           Set Status           = 1,
                               Last_Update_Date = Sysdate,
                               Last_Updated_By  = :Last_Updated_By
                         Where Corp_Id = :Corp_Id" + i, new OracleParameter[]
                    {
                        data.MakeInParam(":Last_Updated_By",HttpContext.Session.GetString("USER_ID")),
                        data.MakeInParam(":Corp_Id"+i,  id[i])
                    });
            }
            if (ht.Count>0)
            {
                //string sql = "declare str varchar2(1000);user_id varchar2(100);str2 varchar2(3000); begin str:=:corp_id;user_id:=:user_id;str2:='update app_corp set status=1,last_update_date=sysdate,LAST_UPDATED_BY='||user_id||' where corp_id in('||str||')'; execute immediate str2; end;";
                //OracleParameter[] sp = { data.MakeInParam(":corp_id", OracleDbType.Varchar2, 3000, id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
                bool flag = await data.DoSqlList(ht); //data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"启用成功\"}" : "{\"code\":300,\"msg\":\"启用失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中公司\"}");
            }
           
        }
        #endregion

        #region 失效公司
        [HttpPost]
        public async Task<IActionResult> FailureStatusForCorp(string[] id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"Update App_Corp
                           Set Status           = 3,
                               Last_Update_Date = Sysdate,
                               Last_Updated_By  = :Last_Updated_By
                         Where Corp_Id = :Corp_Id" + i, new OracleParameter[]
                    {
                        data.MakeInParam(":Last_Updated_By",HttpContext.Session.GetString("USER_ID")),
                        data.MakeInParam(":Corp_Id"+i,  id[i])
                    });
            }
            if (ht.Count>0)
            {
                //string sql = "declare str varchar2(1000);user_id varchar2(100);str2 varchar2(3000); begin str:=:corp_id;user_id:=:user_id;str2:='update app_corp set status=3,last_update_date=sysdate,LAST_UPDATED_BY='||user_id||' where corp_id in('||str||')'; execute immediate str2; end;";
                //OracleParameter[] sp = { data.MakeInParam(":corp_id", OracleDbType.Varchar2, 3000, id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
                bool flag = await data.DoSqlList(ht); //data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"失效成功\"}" : "{\"code\":300,\"msg\":\"失效失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中公司\"}");
            }
            
        }
        #endregion

        #region 获取根据ID公司
        [HttpPost]
        public async Task<IActionResult> GetCorpById(string corp_id)
        {
            string Msg = "";
            if (corp_id==null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"select corp_id,  corp_code,  corp_name,  detailed_address,  law_person_name,  fax,  zip,  tax_rq_number,  e_mail,  status,  note,  attribute1 from app_corp where corp_id=:corp_id";
            OracleParameter[] sp = { data.MakeInParam(":corp_id", corp_id) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 新增
        [HttpPost]
        public async Task<IActionResult> Insert(string corp_code, string corp_name, string attribute1, string law_person_name, string fax, string zip,
            string tax_rq_number, string detailed_address, string note, string e_mail)
        {
            string Msg = "";
            string sql = @"Insert Into App_Corp(Corp_Id, Corp_Code, Corp_Name, Detailed_Address, Law_Person_Name, Fax,Zip,  Tax_Rq_Number,   e_Mail,   Status,   Note,   Attribute1,   Creation_Date,   Created_By)
                            Values  (App_Corp_s.Nextval,   :corp_code,   :corp_name,   :detailed_address,   :law_person_name,   :fax,   :zip,  :tax_rq_number,   :e_mail,   1,   :note,   :attribute1,   Sysdate,   :user_id)";
            OracleParameter[] sp = { data.MakeInParam(":corp_code",corp_code??""),
                                     data.MakeInParam(":corp_name", corp_name??""),
                                     data.MakeInParam(":detailed_address", detailed_address??""),
                                     data.MakeInParam(":low_person_name", law_person_name??""),
                                     data.MakeInParam(":fax",fax??""),
                                     data.MakeInParam(":zip", zip??""),
                                     data.MakeInParam(":tax_rq_number",tax_rq_number??""),
                                     data.MakeInParam(":e_mail",e_mail??""),
                                     data.MakeInParam(":note", note??""),
                                     data.MakeInParam(":attribute1",attribute1??""),
                                     data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID"))};
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 修改
        [HttpPost]
        public async Task<IActionResult> Modify(string corp_code, string corp_name, string attribute1, string law_person_name, string fax, string zip,
            string tax_rq_number, string detailed_address, string note, string e_mail, string corp_id)
        {
            string Msg = "";
            if (corp_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"Update App_Corp
                                       Set Corp_Code        = :corp_code,
                                           Corp_Name        = :corp_name,
                                           Detailed_Address = :detailed_address,
                                           Law_Person_Name  = :law_person_name,
                                           Fax              = :fax,
                                           Zip              = :zip,
                                           Tax_Rq_Number    = :tax_rq_number,
                                           e_Mail           = :e_mail,
                                           Note             = :note,
                                           Attribute1       = :attribute1,
                                           Last_Update_Date = Sysdate,
                                           Last_Updated_By  = :user_id
                                     Where Corp_Id = :corp_id";
            OracleParameter[] sp = { data.MakeInParam(":corp_code",corp_code??""),
                                     data.MakeInParam(":corp_name", corp_name??""),
                                     data.MakeInParam(":detailed_address", detailed_address??""),
                                     data.MakeInParam(":low_person_name", law_person_name??""),
                                     data.MakeInParam(":fax",fax??""),
                                     data.MakeInParam(":zip", zip??""),
                                     data.MakeInParam(":tax_rq_number",tax_rq_number??""),
                                     data.MakeInParam(":e_mail",e_mail??""),
                                     data.MakeInParam(":note", note??""),
                                     data.MakeInParam(":attribute1",attribute1??""),
                                     data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")),
                                     data.MakeInParam(":corp_id",corp_id)};
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 导出：获取公司
        [HttpPost]
        public async Task<IActionResult> GetCorpForExport(string corp_name,string status)
        {
            string sql = @"select corp_code, corp_name, attribute1,detailed_address, law_person_name, fax, zip, tax_rq_number, e_mail, note from app_corp where (corp_name like '%' || :corp_name || '%' or corp_code like '%' || :corp_code || '%' or attribute1 like '%' || :attribute1 || '%')  and status = :status";
            OracleParameter[] sp = { data.MakeInParam(":corp_name", corp_name ?? ""), data.MakeInParam(":corp_code", corp_name ?? ""), data.MakeInParam(":attribute1", corp_name ?? ""), data.MakeInParam(":status", status) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            List<ExcelCorp> corp = data.DataSetToIList1<ExcelCorp>(ds,0).ToList();
            byte[] buffer = ExcelHelper.Export(corp,"公司",ExcelTitle.Corp).GetBuffer();
            return File(buffer, "application/ms-excel", "公司数据导出.xls");
        }
        #endregion

        #region 导入
        [HttpPost]
        public async Task<IActionResult> FileUpload([FromForm]IFormFile file)
        {
            string Msg = "";
            if (file.Length > 0)
            {
                long fileSize = file.Length / 5242880;
                if (fileSize > 15)
                {
                    Msg = "{\"code\":1,\"msg\":\"文件不能大于15M\",\"returnMsg\":\"\"}";
                }
                else
                {
                    string path = Startup.HostingEnvironment.WebRootPath + "\\" + Guid.NewGuid() + file.FileName;

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    ExcelHelper excel = new ExcelHelper();
                    List<ExcelCorp> ec = excel.GetList<ExcelCorp>(path).ToList();
                    string returnMsg = "";
                    if (ec.Count > 0)
                    {
                        for (int i = 0; i < ec.Count; i++)
                        {
                            if (ec[i].CORP_CODE == null || ec[i].CORP_CODE == "")
                            {
                                returnMsg += "公司代码不能为空。";
                            }
                            else
                            {
                                if (ec[i].CORP_NAME == null || ec[i].CORP_NAME == "")
                                {
                                    returnMsg += "【" + ec[i].CORP_CODE + "】：公司名称不能为空。";
                                }
                                else
                                {
                                    string codeIsexist = await data.GetStringByParam(@"select count(*) from app_corp where corp_code=:corp_code",
                                        new OracleParameter[] { data.MakeInParam(":corp_code", ec[i].CORP_CODE??"")});
                                    if (codeIsexist=="0")
                                    {
                                        string nameIsexist = await data.GetStringByParam(@"select count(*) from app_corp where Corp_Name=:corp_name",
                                        new OracleParameter[] { data.MakeInParam(":corp_name", ec[i].CORP_NAME ?? "") });
                                        if (nameIsexist=="0")
                                        {
                                            #region Insertsql
                                            string sql = @"Insert Into App_Corp
                                                                  (Corp_Id,
                                                                   Corp_Code,
                                                                   Corp_Name,
                                                                   Attribute1,
                                                                   Detailed_Address,
                                                                   Law_Person_Name,
                                                                   Fax,
                                                                   Zip,
                                                                   Tax_Rq_Number,
                                                                   e_Mail,
                                                                   Status,
                                                                   Note,
                                                                   Creation_Date,
                                                                   Created_By)
                                                                Values
                                                                  (App_Corp_s.Nextval,
                                                                   :Corp_Code,
                                                                   :Corp_Name,
                                                                   :Attribute1,
                                                                   :Detailed_Address,
                                                                   :Law_Person_Name,
                                                                   :Fax,
                                                                   :Zip,
                                                                   :Tax_Rq_Number,
                                                                   :e_Mail,
                                                                   1,
                                                                   :Note,
                                                                   Sysdate,
                                                                   :Created_By)";
                                            #endregion
                                            OracleParameter[] sp = {
                                                    data.MakeInParam(":Corp_Code",ec[i].CORP_CODE??""),
                                                    data.MakeInParam(":Corp_Name",ec[i].CORP_NAME??""),
                                                    data.MakeInParam(":Attribute1",ec[i].ATTRIBUTE1??""),
                                                    data.MakeInParam(":Detailed_Address",ec[i].DETAILED_ADDRESS??""),
                                                    data.MakeInParam(":Law_Person_Name",ec[i].LAW_PERSON_NAME??""),
                                                    data.MakeInParam(":Fax",ec[i].FAX??""),
                                                    data.MakeInParam(":Zip",ec[i].ZIP??""),
                                                    data.MakeInParam(":e_Mail",ec[i].E_MAIL??""),
                                                    data.MakeInParam(":Note",ec[i].NOTE??""),
                                                    data.MakeInParam(":Created_By",HttpContext.Session.GetString("USER_ID"))
                                                };
                                            bool flag = await data.DoSqlByParam(sql, sp);
                                            if (flag)
                                            {
                                                returnMsg += "【" + ec[i].CORP_CODE + "】" + ec[i].CORP_NAME + "：导入成功。";
                                            }
                                            else
                                            {
                                                returnMsg += "【" + ec[i].CORP_CODE + "】" + ec[i].CORP_NAME + "：导入失败，具体情况请联系管理员。";
                                            }
                                        }
                                        else
                                        {
                                            returnMsg += "【" + ec[i].CORP_CODE + "】" + ec[i].CORP_NAME + "：公司名称已存在，无法新增。";
                                        }
                                    }
                                    else
                                    {
                                        returnMsg += "【" + ec[i].CORP_CODE + "】" + ec[i].CORP_NAME + "：公司代码已存在，请另选编码。";
                                    }                                    
                                }
                            }
                        }
                        Msg = "{\"code\":0,\"msg\":\"执行完成\",\"returnMsg\":\"" + returnMsg + "\"}";
                    }
                    else
                    {
                        Msg = "{\"code\":1,\"msg\":\"Excel中没有数据\",\"returnMsg\":\"\"}";
                    }
                    //找到文件，删除
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
            }
            return Content(Msg);
        }
        #endregion

    }
}