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
using System.Net.Http;

namespace WebApplication1.Controllers.MEDICAL_FUND
{
    public class MEDICAL_FUNDController : Controller
    {
        DBHelper data = new DBHelper();
        DataBaseContext context;
        private IHttpClientFactory httpClient;
        public MEDICAL_FUNDController(DataBaseContext context, IHttpClientFactory _httpClient)
        {
            this.context = context;
            this.httpClient = _httpClient;
        }

        [CheckCustomer]
        public IActionResult MEDICAL_FUND()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult MEDICAL_FUND_EDIT()
        {
            ViewBag.status = HttpContext.Request.Query["status"].ToString();
            ViewBag.fund_id = ViewBag.status == "add" ? "" : HttpContext.Request.Query["Rowid"].ToString();
            return View();
        }

        #region 获取全部：分页
        [HttpGet]
        public async Task<IActionResult> GetMEDICAL_FUND(string search_text, int page, int limit)
        {
            string sql= @"SELECT * FROM(SELECT ROWNUM AS rowno, r.* FROM(select (Select Count(*) From MEDICAL_FUND where  PERSON_NAME like '%'|| :search_text ||'%') totalPage,
                                                    FUND_ID
                                                    , PERSON_NAME
                                                    , ID_CARD_NUMBER
                                                    , BANK_NAME
                                                    , BANK_ACCOUNT
                                                    , MOHTN_AMT
                                                    , FUND_AMT
                                                    , NOTE
                                                    ,to_char(CREATION_DATE,'yyyy-MM-dd hh24:mi:ss') as CREATION_DATE
                                                    , (select user_name from app_user where user_id=CREATED_BY) as CREATED_BY
                                                    , PHONE
                         from MEDICAL_FUND where PERSON_NAME like '%' || :search_text1  || '%') r
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
        public async Task<IActionResult> GetMEDICAL_FUNDById(string fund_id)
        {
            string sql=@"Select FUND_ID
                        ,PERSON_NAME
                        ,ID_CARD_NUMBER
                        ,BANK_NAME
                        ,BANK_ACCOUNT
                        ,MOHTN_AMT
                        ,FUND_AMT
                        ,NOTE
                        ,CREATION_DATE
                        ,CREATED_BY
                        ,PHONE
                     from MEDICAL_FUND where FUND_ID = :fund_id ";
            OracleParameter[] sp = {data.MakeInParam(":fund_id", fund_id ??"")};
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
        public async Task<IActionResult> Insert(string person_name,string id_card_number,string bank_name,string bank_account,string mohtn_amt,string fund_amt,string note,string creation_date,string created_by,string phone)
        {
            string sql = @"insert into MEDICAL_FUND(                                    fund_id
,                                   person_name
,                                   id_card_number
,                                   bank_name
,                                   bank_account
,                                   mohtn_amt
,                                   fund_amt
,                                   note
,                                   creation_date
,                                   created_by
,                                   phone
)
                                values(                                    fund_id
,                                   person_name
,                                   id_card_number
,                                   bank_name
,                                   bank_account
,                                   mohtn_amt
,                                   fund_amt
,                                   note
,                                   creation_date
,                                   created_by
,                                   phone
 ) ";
            OracleParameter[] sp = {
                 data.MakeInParam("person_name",person_name??"")
,                data.MakeInParam("id_card_number",id_card_number??"")
,                data.MakeInParam("bank_name",bank_name??"")
,                data.MakeInParam("bank_account",bank_account??"")
,                data.MakeInParam("mohtn_amt",mohtn_amt??"")
,                data.MakeInParam("fund_amt",fund_amt??"")
,                data.MakeInParam("note",note??"")
,                data.MakeInParam("creation_date",creation_date??"")
,                data.MakeInParam("created_by",created_by??"")
,                data.MakeInParam("phone",phone??"")
,};
            bool flag = await data.DoSqlByParam(sql, sp);
            return flag ? Json(new { code = 200, msg = "保存成功" }) : Json(new { code = 300, msg = "保存失败，请联系管理员" });
        }
        #endregion

        #region 编辑
        [HttpPost]
        public async Task<IActionResult> Modify(string fund_id,string person_name,string id_card_number,string bank_name,string bank_account,string mohtn_amt,string fund_amt,string note,string creation_date,string created_by,string phone)
        {
            string sql=@"Update MEDICAL_FUND set 
                                    person_name = :person_name
,                                   id_card_number = :id_card_number
,                                   bank_name = :bank_name
,                                   bank_account = :bank_account
,                                   mohtn_amt = :mohtn_amt
,                                   fund_amt = :fund_amt
,                                   note = :note
,                                   created_by = :created_by
,                                   phone = :phone
             where fund_id = :fund_id ";
            OracleParameter[] sp = {
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":id_card_number",id_card_number??""),
                data.MakeInParam(":bank_name",bank_name??""),
                data.MakeInParam(":bank_account",bank_account??""),
                data.MakeInParam(":mohtn_amt",mohtn_amt??""),
                data.MakeInParam(":fund_amt",fund_amt??""),
                data.MakeInParam(":note",note??""),
                data.MakeInParam(":created_by",created_by??""),
                data.MakeInParam(":phone",phone??"")};
            bool flag = await data.DoSqlByParam(sql, sp);
            return flag ? Json(new { code = 200, msg = "保存成功" }) : Json(new { code = 300, msg = "保存失败，请联系管理员" });
        }
        #endregion

        #region 删除
        [HttpPost]
        public async Task<IActionResult> Delete(string[] fund_id)
        {
            Hashtable ht = new Hashtable();
            for (int i = 0; i < fund_id.Length; i++)
            {
            ht.Add(@"delete from MEDICAL_FUND where FUND_ID =:fund_id"+i,new OracleParameter[] { data.MakeInParam(":fund_id"+i,fund_id[i].ToString()??"")});
            }
            bool flag = await data.DoSqlList(ht);
            return flag ? Json(new { code = 200, msg = "删除成功" }) : Json(new { code = 300, msg = "删除失败，请联系管理员" });
        }
        #endregion

        #region 导出
        [HttpPost]
        public async Task<IActionResult> GetMEDICAL_FUNDForExport(string search_text, string status)
        {
            string sql = @"select PERSON_NAME
                                    ,ID_CARD_NUMBER
                                    ,BANK_NAME
                                    ,BANK_ACCOUNT
                                    ,to_char(MOHTN_AMT) MOHTN_AMT
                                    ,to_char(FUND_AMT) FUND_AMT
                                    ,NOTE
                                    ,to_char(CREATION_DATE,'yyyy-MM-dd hh24:mi:ss') as CREATION_DATE
                                    ,(select user_name from app_user where user_id= CREATED_BY) as CREATED_BY
                                    ,PHONE
                                     from MEDICAL_FUND where PERSON_NAME like '%'|| :search_text || '%'";
            OracleParameter[] sp = { data.MakeInParam(":search_text", search_text ?? "") };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            IList<Excelmedical_fundExport> list = data.DataSetToIList1<Excelmedical_fundExport>(ds, 0);
            byte[] buffer = new byte[0]; if (list == null) { }
            else
            {
                List<Excelmedical_fundExport> import = list.ToList();
                if (import == null) { }
                else
                {
                    buffer = ExcelHelper.Export(import, "数据导出", ExcelTitle.medical_fund).GetBuffer();
                }
            }
            return File(buffer, "application/ms-excel", "数据导出(" + DateTime.Now + ").xls");
        }
        #endregion

        #region 导入
        [HttpPost]
        public async Task<IActionResult> FileUpload_MEDICAL_FUND([FromForm]IFormFile file)
        {
            string Msg = "";
            string returnMsg = "";
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
                    List<Excelmedical_fund> ec = excel.GetList<Excelmedical_fund>(path).ToList();
                    if (ec.Count > 0)
                    {
                        for (int i = 0; i < ec.Count; i++)
                        {
                            if (ec[i].PERSON_NAME == null || ec[i].PERSON_NAME == "")
                            {
                                returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + "：姓名不能为空。";
                            }
                            else
                            {
                                if (ec[i].ID_CARD_NUMBER == null || ec[i].ID_CARD_NUMBER == "")
                                {
                                    returnMsg += ec[i].PERSON_NAME + "：身份证不能为空。";
                                }
                                else
                                {
                                    string fund_id = await data.GetString(@"select MEDICAL_FUND_s.nextval from dual");
                                    string sql = @"insert into MEDICAL_FUND(fund_id
                                    ,person_name
                                    ,id_card_number
                                    ,bank_name
                                    ,bank_account
                                    ,mohtn_amt
                                    ,fund_amt
                                    ,note
                                    ,creation_date
                                    ,created_by
                                    ,phone
                                    )
                                values( :fund_id
                                            ,:person_name
                                            ,:id_card_number
                                            ,:bank_name
                                            ,:bank_account
                                            ,:mohtn_amt
                                            ,:fund_amt
                                            ,:note
                                            ,sysdate
                                            ,:created_by
                                            ,:phone
                                             ) ";
                                    OracleParameter[] sp = {
                                        data.MakeInParam(":fund_id",fund_id)
                                         ,data.MakeInParam(":person_name",ec[i].PERSON_NAME??"")
                                        ,data.MakeInParam(":id_card_number",ec[i].ID_CARD_NUMBER??"")
                                        ,data.MakeInParam(":bank_name",ec[i].BANK_NAME??"")
                                        ,data.MakeInParam(":bank_account",ec[i].BANK_ACCOUNT??"")
                                        ,data.MakeInParam(":mohtn_amt",ec[i].MOHTN_AMT??"")
                                        ,data.MakeInParam(":fund_amt",ec[i].FUND_AMT??"")
                                        ,data.MakeInParam(":note",ec[i].NOTE??"")
                                        ,data.MakeInParam(":created_by",HttpContext.Session.GetString("USER_ID")??"")
                                        ,data.MakeInParam(":phone",ec[i].PHONE??"")};
                                    bool flag = await data.DoSqlByParam(sql, sp);
                                    if (flag)
                                    {
                                        returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + ec[i].PERSON_NAME + "：导入成功。";
                                        returnMsg += await SendMsg("", fund_id);
                                    }
                                    else
                                    {
                                        returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + ec[i].PERSON_NAME + "：在导入数据时发生错误，请联系管理员。";
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

        #region 发送短信
        private async Task<string> SendMsg(string msg, string fund_id)
        {
            var phone= await data.GetStringByParam(@"select phone from MEDICAL_FUND where FUND_ID=:fund_id", new OracleParameter[] { data.MakeInParam(":fund_id", fund_id??"") });
            if (phone==""||phone==null)
            {
                phone = await data.GetStringByParam(@"Select distinct t.Phone
                                              From Txry_Person t, Medical_Fund f
                                             Where t.Person_Name = f.Person_Name
                                               And t.Id_Card_Number = f.Id_Card_Number
                                               And f.Fund_Id = :Fund_Id",
                    new OracleParameter[] { data.MakeInParam(":Fund_Id", fund_id??"")});
            }
            if (phone==""||phone==null)
            {
                msg = "手机号码为空，当前人员可能在退休人员名单中不存在，或手机号码并未填写。";
                return msg;
            }
            var amt = await data.GetStringByParam(@"select FUND_AMT from MEDICAL_FUND where FUND_ID=:fund_id",new OracleParameter[] { data.MakeInParam(":fund_id",fund_id??"")});
            var url = "http://sms.106jiekou.com/utf8/sms.aspx";
            var content = "您好！您当年可用于韶钢医院门诊治疗费用结算报销的医疗备用金额度为" + amt + "元。";
            try
            {
                var client = httpClient.CreateClient();
                url = url + "?account=huang_q_w&password=9h-9h-&mobile=" + phone + "&content=" + content + "&signName=韶钢e退管";
                client.BaseAddress = new Uri(url);
                var response = await client.GetAsync(url);
                //获取请求到数据，并转化为字符串
                var back = response.Content.ReadAsStringAsync().Result;
                switch (back)
                {
                    case "100": msg = "短信发送成功"; break;
                    case "101": msg = "短信接口验证失败"; break;
                    case "102": msg = "该人员手机号码格式不正确，无法短信通知"; break;
                    case "103": msg = "短信账户会员级别不够"; break;
                    case "104": msg = "短信内容未审核"; break;
                    case "105": msg = "短信内容过多"; break;
                    case "106": msg = "短信账户余额不足"; break;
                    case "107": msg = "Ip地址受限，无法发送短信"; break;
                    case "108": msg = "手机号码发送太频繁，请换号或隔天再发"; break;
                    case "109": msg = "短信帐号被锁定"; break;
                    case "110": msg = "手机号发送频率持续过高，黑名单屏蔽数日"; break;
                    case "120": msg = "短信接口系统升级中"; break;
                }
            }
            catch (Exception ex)
            {
                msg = "短信发送失败";
            }
            return msg;
        }
        #endregion
    }
}