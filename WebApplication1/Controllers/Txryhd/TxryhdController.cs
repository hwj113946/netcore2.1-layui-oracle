using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using WebApplication1.Models;
using WebApplication1.Models.ApiModels;

namespace WebApplication1.Controllers.Txryhd
{
    public class TxryhdController : Controller
    {
        DBHelper data = new DBHelper();
        DataBaseContext context;
        private IHttpClientFactory httpClient;
        public TxryhdController(DataBaseContext context, IHttpClientFactory _httpClient)
        {
            this.context = context;
            this.httpClient = _httpClient;
        }
        [CheckCustomer]
        public IActionResult txryhd_index()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult txryhd_fh()
        {
            ViewBag.status= HttpContext.Request.Query["status"].ToString();
            ViewBag.person_id = HttpContext.Request.Query["Rowid"].ToString();
            return View();
        }

        [CheckCustomer]
        public IActionResult TXRY_PERSONEDIT()
        {
            ViewBag.status = HttpContext.Request.Query["status"].ToString();
            ViewBag.person_id = ViewBag.status == "update" ? HttpContext.Request.Query["Rowid"].ToString() : "";
            return View();
        }

        [CheckCustomer]
        public IActionResult ydjybx_sgybb()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult ydjybx_pjcljs_edit()
        {
            ViewBag.status = HttpContext.Request.Query["status"].ToString();
            ViewBag.person_id = HttpContext.Request.Query["person_id"].ToString();
            ViewBag.Rowid = ViewBag.status == "update" ? HttpContext.Request.Query["Rowid"].ToString() : "";
            return View();
        }

        [CheckCustomer]
        public IActionResult ydjybx_sgybb_sh()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult ydjybx_sybj()
        {
            return View();
        }
        [CheckCustomer]
        public IActionResult ydjybx_sqzf()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult ydjybx_query()
        {
            return View();
        }

        #region 获取人员
        [HttpPost]
        public async Task<IActionResult> GetPerson(string person_name, string id_card_number, string status, int limit, int page)
        {
            string Msg = "";
            #region sql
            string sql = @"SELECT *
  FROM(SELECT ROWNUM AS rowno, r.*
          FROM(select (Select Count(*) From Txry_Person Where status=:Status1) totalPage,Person_Id,
                           Person_Name,
                           Sex,
                           Age,
                           Phone,
                           Id_Card_Number,
                           National,
                           Political_Landscape,
                           Long_Term_Residence,
                           Domicile_Place,
                           Special_Person,
                           Health,
                           e_i_Address,
                           Medical_i_Address,
                           Is_Gsbx,
                           Living_Situation,
                           Spouse_Name,
                           Spouse_Health,
                           Spouse_Phone,
                           Family_Major_Person_Name,
                           Family_Major_p_Relationship,
                           Family_Major_Person_Address,
                           Family_Major_Person_Phone,
                           Registered_Image_First,
                           Registered_Image_Self,
                           Decode(Status, 0, '未核验', 1, '已核验',2,'复核通过') Status,
                           Emergency_Person,
                           Emergency_Phone,
                           Emergency_Address,
                           /*REGISTERED_IMAGE_FIRST image_first,
                           REGISTERED_IMAGE_SELF image_self,*/
                           EMERGENCY_PERSON e_person,
                           EMERGENCY_PHONE ep，
                           EMERGENCY_ADDRESS ea,tp.TRANSFER_TYPE
                      From Txry_Person tp
                     Where Person_Name Like '%' || :Person_Name || '%'
                       And Id_Card_Number Like '%' || :Id_Card_Number || '%'
                       And Status = :Status) r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (: page - 1) * :limit";
            #endregion
            OracleParameter[] sp = { 
                                     data.MakeInParam(":Status1", status ?? ""),
                                     data.MakeInParam(":Person_Name", person_name ?? ""),
                                     data.MakeInParam(":Id_Card_Number", id_card_number ?? ""),
                                     data.MakeInParam(":Status", status ?? ""),
                                     data.MakeInParam(":page", page),
                                    data.MakeInParam(":limit", limit)};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 导出：获取退休人员核定
        [HttpPost]
        public async Task<IActionResult> GetTxryForExport(string person_name, string id_card_number, string status)
        {
            #region sql
            string sql = @"Select 
       Person_Name,
       Sex,
       Age,
       Phone,
       Id_Card_Number,
       National,
       Political_Landscape,
       Long_Term_Residence,
       Domicile_Place,
       Special_Person,
       Health,
       e_i_Address,
       Medical_i_Address,
       Is_Gsbx,
       Living_Situation,
       Spouse_Name,
       Spouse_Health,
       Spouse_Phone,
       Family_Major_Person_Name,
       Family_Major_p_Relationship,
       Family_Major_Person_Address,
       Family_Major_Person_Phone,
       Decode(Status, 0, '未核验', 1, '已核验',2,'复核通过') Status,
       Emergency_Person,
       Emergency_Phone,
       Emergency_Address，EMERGENCY_PERSON,EMERGENCY_PHONE，EMERGENCY_ADDRESS,TRANSFER_TYPE
  From Txry_Person Tp
 Where Person_Name Like '%' || :Person_Name || '%'
                       And Id_Card_Number Like '%' || :Id_Card_Number || '%'
                       And Status = :Status"; 
            #endregion
            OracleParameter[] sp = { data.MakeInParam(":Person_Name", person_name ?? ""),
                                     data.MakeInParam(":Id_Card_Number", id_card_number ?? ""),
                                     data.MakeInParam(":Status", status ?? "") };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            List<ExcelTxryhd> txryhd = data.DataSetToIList1<ExcelTxryhd>(ds, 0).ToList();
            byte[] buffer = ExcelHelper.Export(txryhd, "退休人员核定", ExcelTitle.Txryhd).GetBuffer();
            return File(buffer, "application/ms-excel", "退休人员核定数据导出.xls");
        }
        #endregion

        #region 核定
        [HttpPost]
        public async Task<IActionResult> PersonConfirm(string id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"Declare Str Varchar2(1000); Str2 Varchar2(3000);
            Begin Str     := :person_id; Str2:= 'update txry_person set status=1 where person_id in(' || Str || ')';
            Execute Immediate Str2; End; ";
            OracleParameter[] sp = { data.MakeInParam(":person_id", OracleDbType.Varchar2, 3000, id) };
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"核定成功\"}" : "{\"code\":300,\"msg\":\"核定失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 退休人员：新增
        [HttpPost]
        public async Task<IActionResult> Insert( string id_card_number, string person_name, string phone,string spouse_name,string spouse_health,string spouse_phone,string emergency_person,string emergency_phone,string emergency_address,string domicile_place,string long_term_residence,string transfer_type)
        {
            
            string sql = @"Insert Into Txry_Person
                                      (Person_Id,
                                       Person_Name,
                                       Phone,
                                       Id_Card_Number,
                                       Long_Term_Residence,
                                       Domicile_Place,
                                       Spouse_Name,
                                       Spouse_Health,
                                       Spouse_Phone,
                                       Status,
                                       Emergency_Person,
                                       Emergency_Phone,
                                       Emergency_Address,
                                       Transfer_Type)
                                    Values
                                      (Txry_Person_s.Nextval,
                                       :Person_Name,
                                       :Phone,
                                       :Id_Card_Number,
                                       :Long_Term_Residence,
                                       :Domicile_Place,
                                       :Spouse_Name,
                                       :Spouse_Health,
                                       :Spouse_Phone,
                                       0,
                                       :Emergency_Person,
                                       :Emergency_Phone,
                                       :Emergency_Address,
                                       :Transfer_Type)";
            OracleParameter[] sp = {
                data.MakeInParam(":Person_Name",person_name??""),
                data.MakeInParam(":Phone",phone??""),
                data.MakeInParam(":Id_Card_Number",id_card_number??""),
                data.MakeInParam(":Long_Term_Residence",long_term_residence??""),
                data.MakeInParam(":Domicile_Place",domicile_place??""),
                data.MakeInParam(":Spouse_Name",spouse_name??""),
                data.MakeInParam(":Spouse_Health",spouse_health??""),
                data.MakeInParam(":Spouse_Phone",spouse_phone??""),
                data.MakeInParam(":Emergency_Person",emergency_person??""),
                data.MakeInParam(":Emergency_Phone",emergency_phone??""),
                data.MakeInParam(":Emergency_Address",emergency_address??""),
                data.MakeInParam(":Transfer_Type",transfer_type??"")};
            bool flag = await data.DoSqlByParam(sql, sp);
            string Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 退休人员：修改
        [HttpPost]
        public async Task<IActionResult> Modifys(string person_id,string id_card_number, string person_name, string phone, string spouse_name, string spouse_health, string spouse_phone, string emergency_person, string emergency_phone, string emergency_address, string domicile_place, string long_term_residence, string transfer_type)
        {

            string sql = @"Update Txry_Person
                                   Set Person_Name         = :Person_Name,
                                       Phone               = :Phone,
                                       Id_Card_Number      = :Id_Card_Number,
                                       Long_Term_Residence = :Long_Term_Residence,
                                       Domicile_Place      = :Domicile_Place,
                                       Spouse_Name         = :Spouse_Name,
                                       Spouse_Health       = :Spouse_Health,
                                       Spouse_Phone        = :Spouse_Phone,
                                       Emergency_Person    = :Emergency_Person,
                                       Emergency_Phone     = :Emergency_Phone,
                                       Emergency_Address   = :Emergency_Address,
                                       Transfer_Type       = :Transfer_Type
                                 Where Person_Id = :Person_Id";
            OracleParameter[] sp = {
                data.MakeInParam(":Person_Name",person_name??""),
                data.MakeInParam(":Phone",phone??""),
                data.MakeInParam(":Id_Card_Number",id_card_number??""),
                data.MakeInParam(":Long_Term_Residence",long_term_residence??""),
                data.MakeInParam(":Domicile_Place",domicile_place??""),
                data.MakeInParam(":Spouse_Name",spouse_name??""),
                data.MakeInParam(":Spouse_Health",spouse_health??""),
                data.MakeInParam(":Spouse_Phone",spouse_phone??""),
                data.MakeInParam(":Emergency_Person",emergency_person??""),
                data.MakeInParam(":Emergency_Phone",emergency_phone??""),
                data.MakeInParam(":Emergency_Address",emergency_address??""),
                data.MakeInParam(":Transfer_Type",transfer_type??""),
                data.MakeInParam(":Person_Id",person_id??"")};
            bool flag = await data.DoSqlByParam(sql, sp);
            string Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 退休人员信息导入
        [HttpPost]
        public async Task<IActionResult> FileUpload_Txry([FromForm]IFormFile file)
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
                    List<ExcelTxryAdd> ec = excel.GetList<ExcelTxryAdd>(path).ToList();
                    if (ec.Count > 0)
                    {
                        for (int i = 0; i < ec.Count; i++)
                        {
                            if ((ec[i].PERSON_NAME == null || ec[i].PERSON_NAME == "")&& (ec[i].ID_CARD_NUMBER == null || ec[i].ID_CARD_NUMBER == ""))
                            {
                                returnMsg += "姓名和身份证不能为空。";
                            }
                            else
                            {
                                if (ec[i].PERSON_NAME == null || ec[i].PERSON_NAME == "")
                                {
                                    returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + "：姓名不能为空。";
                                }
                                else
                                {
                                    if (ec[i].ID_CARD_NUMBER == null || ec[i].ID_CARD_NUMBER == "")
                                    {
                                        returnMsg += "【" + ec[i].PERSON_NAME + "】" + "：身份证号码不能为空。";
                                    }
                                    else
                                    {
                                        OracleParameter[] p = { data.MakeInParam(":Person_Name", ec[i].PERSON_NAME??""),
                                                                data.MakeInParam(":Id_Card_Number",ec[i].ID_CARD_NUMBER??"")};
                                        string isexist = await data.GetStringByParam(@"Select Count(*) From Txry_Person Where Person_Name = :Person_Name  And Id_Card_Number = :Id_Card_Number", p);
                                        //验证是否存在
                                        if (int.Parse(isexist) >0)
                                        {
                                            returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + ec[i].PERSON_NAME + "：已经在退休人员信息表中存在。";
                                        }
                                        else
                                        {
                                            #region InsertSql
                                            string sql = @"Insert Into Txry_Person
                                                                          (Person_Id,
                                                                           Person_Name,
                                                                           Sex,
                                                                           Age,
                                                                           Phone,
                                                                           Id_Card_Number,
                                                                           National,
                                                                           Political_Landscape,
                                                                           Long_Term_Residence,
                                                                           Domicile_Place,
                                                                           Status)
                                                                        Values
                                                                          (Txry_Person_s.Nextval,
                                                                           :Person_Name,
                                                                           :Sex,
                                                                           :Age,
                                                                           :Phone,
                                                                           :Id_Card_Number,
                                                                           :National,
                                                                           :Political_Landscape,
                                                                           :Long_Term_Residence,
                                                                           :Domicile_Place,
                                                                           0)";
                                            #endregion

                                            #region 数据库操作
                                            bool flag = await data.DoSqlByParam(sql, new OracleParameter[]
                                                                                {
                                                 data.MakeInParam(":Person_Name",ec[i].PERSON_NAME??""),
                                                 data.MakeInParam(":Sex",ec[i].SEX??""),
                                                 data.MakeInParam(":Age",ec[i].AGE??""),
                                                data.MakeInParam(":Phone",ec[i].PHONE??""),
                                                data.MakeInParam(":Id_Card_Number",ec[i].ID_CARD_NUMBER??""),
                                                data.MakeInParam(":National",ec[i].NATIONAL??""),
                                                data.MakeInParam(":Political_Landscape",ec[i].POLITICAL_LANDSCAPE??""),
                                                data.MakeInParam(":Long_Term_Residence",ec[i].LONG_TERM_RESIDENCE??""),
                                                data.MakeInParam(":Domicile_Place",ec[i].DOMICILE_PLACE??"")
                                                                                });
                                            if (flag)
                                            {
                                                returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + ec[i].PERSON_NAME + "：导入成功。";
                                            }
                                            else
                                            {
                                                returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + ec[i].PERSON_NAME + "：在导入数据时发生错误，请联系管理员。";
                                            } 
                                            #endregion
                                        }
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

        #region 复核：单个
        [HttpPost]
        public async Task<IActionResult> ModifyForSingle(string person_id)
        {
            string sql = @"Update Txry_Person
                               Set Status  =  2
                             Where Person_Id = :person_id";
            OracleParameter[] sp = {data.MakeInParam(":person_id",person_id??"")};
            bool flag = await data.DoSqlByParam(sql, sp);
            var json = flag == true ? new { code = 200, msg = "复核成功" } : new { code = 300, msg = "复核失败，请联系管理员" };
            return Json(json.ToJson());
        }
        #endregion

        #region 根据ID获取人员信息
        [HttpPost]
        public async Task<IActionResult> GetPersonById(string person_id)
        {
            string Msg = "";
            string sql = @"select PERSON_ID,
                    PERSON_NAME,
                    SEX,
                    AGE,
                    PHONE,
                    ID_CARD_NUMBER,
                    NATIONAL,
                    POLITICAL_LANDSCAPE,
                    LONG_TERM_RESIDENCE,
                    DOMICILE_PLACE,
                    SPECIAL_PERSON,
                    HEALTH,
                    E_I_ADDRESS,
                    MEDICAL_I_ADDRESS,
                    IS_GSBX,
                    LIVING_SITUATION,
                    SPOUSE_NAME,
                    SPOUSE_HEALTH,
                    SPOUSE_PHONE,
                    FAMILY_MAJOR_PERSON_NAME,
                    FAMILY_MAJOR_P_RELATIONSHIP,
                    FAMILY_MAJOR_PERSON_ADDRESS,
                    FAMILY_MAJOR_PERSON_PHONE,
                    STATUS,
                    EMERGENCY_PERSON,
                    EMERGENCY_PHONE,
                    EMERGENCY_ADDRESS,
                    TRANSFER_TYPE
                    from txry_person t Where t.person_id=:person_id";
            OracleParameter[] sp = { data.MakeInParam(":person_id", person_id ?? "") };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 异地就医：根据姓名或身份证获取退休人员
        [HttpPost]
        public async Task<IActionResult> GetPerson_ydjybx(string person_name, int page, int limit)
        {
            string Msg = "";
            string sql = @"SELECT *
  FROM(SELECT ROWNUM AS rowno, r.*
          FROM(select (Select Count(*) From Txry_Person Where (Person_Name Like '%' || :Person_Name || '%'
                       or Id_Card_Number Like '%' || :Id_Card_Number || '%')) totalPage,Person_Id,
                           Person_Name,
                           Id_Card_Number
                      From Txry_Person tp
                     Where (Person_Name Like '%' || :Person_Name1 || '%'
                       or Id_Card_Number Like '%' || :Id_Card_Number1 || '%')) r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (: page - 1) * :limit";
            
            OracleParameter[] sp = {
                                     data.MakeInParam(":Person_Name", person_name ?? ""),
                                     data.MakeInParam(":Id_Card_Number", person_name ?? ""),
                                     data.MakeInParam(":Person_Name1", person_name ?? ""),
                                     data.MakeInParam(":Id_Card_Number1", person_name ?? ""),
                                     data.MakeInParam(":page", page),
                                    data.MakeInParam(":limit", limit)};
            DataSet ds = await data.GetDataSetByParam(sql,sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 异地就医：根据人员id获取处理进度(票据材料接收)
        [HttpPost]
        public async Task<IActionResult> GetYdjybxByPerson(string person_id,string flow_no, int page, int limit)
        {
            string Msg = "";
            string sql = @"SELECT *
  FROM(SELECT ROWNUM AS rowno, r.*
          FROM( Select (Select Count(*)
                          From Anotherplaceafr
                         Where Person_Id = :Person_Id and flow_no like '%' ||:flow_no || '%'
                           And Flow_Type = '票据材料接收') Totalpage,
                       Afr_Id,
                       Flow_No,
                       Deal_With_Number,
                       Flow_Type,
                       Id_Card_Number,
                       Person_Name,
                       Tran_Date,
                       Tran_Person,
                       Access_Msg,
                       Fail_Msg,
                       Fail_Reason,
                       Decode(Status,
                              0,
                              '编辑',
                              1,
                              '待补充材料',
                              2,
                              '票据材料已接收',
                              3,
                              '韶钢医保办待审核',
                              4,
                              '韶钢医保办复核通过',
                              5,
                              '韶钢医保办复核退回',
                              6,
                              '市医保局待复核',
                              7,
                              '市医保局复核通过',
                              8,
                              '市医保局退回',
                              9,
                              '待申请支付',
                              10,
                              '已申请支付') Status,
                       Person_Id,attribute2
                  From Anotherplaceafr
                 Where Person_Id = :Person_Id1 and flow_no like '%' ||:flow_no1 || '%'
                   And Flow_Type = '票据材料接收'
                 Order By Flow_No,Deal_With_Number Asc) r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (: page - 1) * :limit";
            OracleParameter[] sp = { data.MakeInParam(":Person_Id",person_id??""),
                                     data.MakeInParam(":flow_no",flow_no??""),
                                     data.MakeInParam(":Person_Id1",person_id??""),
                                     data.MakeInParam(":flow_no1",flow_no??""),
                                     data.MakeInParam(":page", page),
                                     data.MakeInParam(":limit", limit)};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 异地就医：票据材料接收：获取：编辑
        [HttpPost]
        public async Task<IActionResult> GetPerson_ydjybx_pjcljs(string person_id, string Rowid)
        {
            string Msg = "";

            OracleParameter[] sp1 = { data.MakeInParam(":person_id", person_id ?? "") };
            DataSet ds1 = await data.GetDataSetByParam(@"select Person_Name, Id_Card_Number,PHONE from Txry_Person where person_id=:person_id", sp1);
            string sql = @"select t.* from anotherplaceafr t Where t.person_id=:person_id and t.afr_id=:afr_id";
            OracleParameter[] sp = { data.MakeInParam(":person_id", person_id ?? ""),
                data.MakeInParam(":afr_id", Rowid ?? "")};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "],\"person\":[ " + JsonTools.DataTableToJson(ds1.Tables[0]) + " ]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[],\"person\":[" + JsonTools.DataTableToJson(ds1.Tables[0]) + "]}";
            return Content(Msg);
        }
        #endregion

        #region 票据材料接收：新增：保存
        [HttpPost]
        public async Task<IActionResult> Insert_ydjybx(string person_id,string id_card_number,string person_name,string attribute2,string phone)
        {
            OracleParameter[] p = { data.MakeInParam(":p_Inf",OracleDbType.Varchar2,1000, ParameterDirection.Output) };
            string flow_no = await data.RunProcStr("Ydjybx.Get_Flow_Number", p);
            string id = await data.GetString(@"select Anotherplaceafr_s.Nextval from dual");
            string sql = @"Insert Into Anotherplaceafr
                                                  (Afr_Id,
                                                   Flow_No,
                                                   Deal_With_Number,
                                                   Flow_Type,
                                                   Id_Card_Number,
                                                   Person_Name,
                                                   Tran_Date,
                                                   Tran_Person,
                                                   Access_Msg,
                                                   Fail_Msg,
                                                   Fail_Reason,
                                                   Status,
                                                   Attribute2,
                                                   Creation_Date,
                                                   Creation_By,
                                                   Person_Id,phone)
                                                Values
                                                  (:afr_id,
                                                   :flow_no,
                                                   1,
                                                   '票据材料接收',
                                                   :id_card_number,
                                                   :person_name,
                                                   sysdate,
                                                   :user_name,
                                                   '',
                                                   '',
                                                   '',
                                                   0,
                                                   :attribute2,
                                                   Sysdate,
                                                   :user_id,
                                                   :person_id,:phone)";
            OracleParameter[] sp = {
                data.MakeInParam(":afr_id",id??""),
                data.MakeInParam(":flow_no",flow_no??""),
                data.MakeInParam(":id_card_number",id_card_number??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":user_name",HttpContext.Session.GetString("USER_NAME")??""),
                data.MakeInParam(":attribute2",attribute2??""),
                data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")??""),
                data.MakeInParam(":person_id",person_id??""),data.MakeInParam(":phone",phone??"")};
            bool flag = await data.DoSqlByParam(sql, sp);
            if (flag)
            {
                string msg = "";
                //发送短信
                msg = await SendMsg2(msg, id);
            }
            string Msg =flag? "{\"code\":200,\"msg\":\"保存成功\"}":"{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 票据材料接收：修改：保存
        [HttpPost]
        public async Task<IActionResult> Modify_ydjybx(string person_id, string id_card_number, string person_name, string attribute2,string afr_id,string phone)
        {
            string sql = @"Update Anotherplaceafr
                                   Set Id_Card_Number   = :id_card_number,
                                       Person_Name      = :person_name,
                                       Attribute2       = :attribute2,
                                       Last_Update_Date = Sysdate,
                                       Last_Update_By   = :user_id,
                                       Person_Id        = :person_id, phone=:phone
                                 Where Afr_Id = :Afr_Id";
            OracleParameter[] sp = {
                data.MakeInParam(":id_card_number",id_card_number??""),
                data.MakeInParam(":person_name",person_name??""),
                data.MakeInParam(":attribute2",attribute2??""),
                data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")??""),
                data.MakeInParam(":person_id",person_id??""),
                data.MakeInParam(":phone",phone??""),
                data.MakeInParam(":Afr_Id",afr_id??"")};
            bool flag = await data.DoSqlByParam(sql, sp);
            string Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 票据材料接收：转回原节点
        [HttpPost]
        public async Task<IActionResult> ReturnNode_ydjybx(string afr_id)
        {
            if (afr_id==null||afr_id=="")
            {
                return Content("{\"code\":300,\"msg\":\"请传入主键\"}");
            }
            OracleParameter[] sp = {
                data.MakeInParam(":v_Afr_Id",afr_id??""),
                data.MakeInParam(":v_User_Id",HttpContext.Session.GetString("USER_ID")??""),
                data.MakeInParam(":p_Inf",OracleDbType.Varchar2,1000, ParameterDirection.Output)};
            string msg = await data.RunProcStr("Ydjybx.Returnnode", sp);
            string Msg = msg == "Y" ? "{\"code\":200,\"msg\":\"转交成功\"}" : "{\"code\":300,\"msg\":\"转交失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 票据材料接收：提交
        [HttpPost]
        public async Task<IActionResult> CommitToYbb_ydjybx(string afr_id)
        {
            if (afr_id==null||afr_id=="")
            {
                return Content("{\"code\":300,\"msg\":\"请传入主键\"}");
            }
            string Msg = "";
            string sql = @"Update Anotherplaceafr
                                   Set 
                                       TRAN_PERSON       = :tran_person,
                                       TRAN_DATE         = sysdate,
                                       status            = 2,
                                       Last_Update_Date = Sysdate,
                                       Last_Update_By   = :user_id
                                 Where Afr_Id = :Afr_Id";
            OracleParameter[] sp = {
                data.MakeInParam(":tran_person",HttpContext.Session.GetString("USER_NAME")??""),
                data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")??""),
                data.MakeInParam(":Afr_Id",afr_id??"")};
            bool flag = await data.DoSqlByParam(sql, sp);
            if (flag)
            {
                string sql1 = @"Insert Into Anotherplaceafr
                                          (Afr_Id,
                                           Flow_No,
                                           Deal_With_Number,
                                           Flow_Type,
                                           Id_Card_Number,
                                           Person_Name,
                                           Status,
                                           Attribute2,
                                           Creation_Date,
                                           Creation_By,
                                           Person_Id,phone)
                                          Select Anotherplaceafr_s.Nextval,
                                                 Flow_No,
                                                 Deal_With_Number + 1,
                                                 '韶钢医保办审核',
                                                 Id_Card_Number,
                                                 Person_Name,
                                                 3,
                                                 To_Char(Sysdate, 'yyyy-MM-dd hh24:mi:ss'),
                                                 Sysdate,
                                                 :User_Id,
                                                 Person_Id,phone
                                            From Anotherplaceafr
                                           Where Afr_Id =:Afr_Id";
                OracleParameter[] sp1 = {
                data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")??""),
                data.MakeInParam(":Afr_Id",afr_id??"")};
                flag = await data.DoSqlByParam(sql1, sp1);
                string msg = "";
                //发送短信
                msg = await SendMsg(msg, "step2", afr_id);
            }
            Msg = flag ? "{\"code\":200,\"msg\":\"提交成功\"}" : "{\"code\":300,\"msg\":\"提交失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 异地就医：(韶钢医保办审核)
        [HttpPost]
        public async Task<IActionResult> GetYdjybxByStatusYBB(string status,string person_name, int page, int limit)
        {
            string Msg = "";
            string sql = @"SELECT *
  FROM(SELECT ROWNUM AS rowno, r.*
          FROM( Select (Select Count(*)
                          From Anotherplaceafr
                         Where status = :status and person_name like '%'|| :person_name ||'%'
                           And Flow_Type = '韶钢医保办审核') Totalpage,
                       Afr_Id,
                       Flow_No,
                       Deal_With_Number,
                       Flow_Type,
                       Id_Card_Number,
                       Person_Name,
                       Tran_Date,
                       Tran_Person,
                       Access_Msg,
                       Fail_Msg,
                       Fail_Reason,
                       Decode(Status,
                              0,
                              '编辑',
                              1,
                              '待补充材料',
                              2,
                              '票据材料已接收',
                              3,
                              '韶钢医保办待审核',
                              4,
                              '韶钢医保办复核通过',
                              5,
                              '韶钢医保办复核退回',
                              6,
                              '市医保局待复核',
                              7,
                              '市医保局复核通过',
                              8,
                              '市医保局退回',
                              9,
                              '待申请支付',
                              10,
                              '已申请支付') Status,
                       Person_Id,attribute2
                  From Anotherplaceafr
                 Where status = :status1 and person_name like '%'|| :person_name1 ||'%'
                   And Flow_Type = '韶钢医保办审核'
                 Order By Deal_With_Number Asc) r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (: page - 1) * :limit";
            OracleParameter[] sp = { data.MakeInParam(":status",status??""), data.MakeInParam(":person_name",person_name??""),
                                     data.MakeInParam(":status1",status??""),data.MakeInParam(":person_name1",person_name??""),
                                     data.MakeInParam(":page", page),
                                     data.MakeInParam(":limit", limit)};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 医保办审核：退回到票据材料接收
        [HttpPost]
        public async Task<IActionResult> BackPjjs_ydjybx(long[] id, string backReason)
        {
            string Msg = "";
            using (var tran = context.Database.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < id.Length; i++)
                    {
                        var rm = await Task.FromResult(context.AFR.ToList().Where(u => u.AFR_ID.Equals(id[i])).FirstOrDefault());
                        ANOTHERPLACEAFR afr = new ANOTHERPLACEAFR()
                        {
                            FLOW_NO = rm.FLOW_NO,
                            FLOW_TYPE = "票据材料接收",
                            ID_CARD_NUMBER = rm.ID_CARD_NUMBER,
                            PERSON_NAME = rm.PERSON_NAME,
                            STATUS = 1,
                            PERSON_ID = rm.PERSON_ID,
                            CREATION_DATE = DateTime.Now,
                            CREATION_BY = int.Parse(HttpContext.Session.GetString("USER_ID")),
                            DEAL_WITH_NUMBER = rm.DEAL_WITH_NUMBER + 1,
                            PHONE=rm.PHONE
                        };
                        await context.AFR.AddAsync(afr);
                        rm.STATUS = 5;
                        rm.FAIL_REASON = backReason ?? "";
                        rm.LAST_UPDATE_BY = int.Parse(HttpContext.Session.GetString("USER_ID"));
                        rm.LAST_UPDATE_DATE = DateTime.Now;
                        context.AFR.Update(rm);
                    }
                    await context.SaveChangesAsync();
                    string msg = "";
                    for (int i = 0; i < id.Length; i++)
                    {
                        //发送短信
                        msg = await SendMsg(msg, "fail_msg", id[i].ToString());
                    }
                    tran.Commit();
                    Msg = "{\"code\":200,\"msg\":\"退回成功\"}";
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Msg = "{\"code\":300,\"msg\":\"退回失败，请联系管理员\"}";
                }
            }
            return Content(Msg);
        }
        #endregion

        #region 医保办审核：复核通过并转交到市医保局
        [HttpPost]
        public async Task<IActionResult> CommitToSybj_ydjybx(long[] id)
        {
            string Msg = "";
            using (var tran = context.Database.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < id.Length; i++)
                    {
                        var rm = await Task.FromResult(context.AFR.ToList().Where(u => u.AFR_ID.Equals(id[i])).FirstOrDefault());
                        ANOTHERPLACEAFR afr = new ANOTHERPLACEAFR()
                        {
                            FLOW_NO = rm.FLOW_NO,
                            FLOW_TYPE = "市医保局复核",
                            ID_CARD_NUMBER = rm.ID_CARD_NUMBER,
                            PERSON_NAME = rm.PERSON_NAME,
                            STATUS = 6,
                            PERSON_ID = rm.PERSON_ID,
                            CREATION_DATE = DateTime.Now,
                            ATTRIBUTE2=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            CREATION_BY = int.Parse(HttpContext.Session.GetString("USER_ID")),
                            DEAL_WITH_NUMBER = rm.DEAL_WITH_NUMBER + 1,
                            PHONE = rm.PHONE
                        };
                        await context.AFR.AddAsync(afr);
                        rm.STATUS = 4;
                        rm.TRAN_DATE = DateTime.Now;
                        rm.TRAN_PERSON = HttpContext.Session.GetString("USER_NAME");
                        rm.LAST_UPDATE_BY = int.Parse(HttpContext.Session.GetString("USER_ID"));
                        rm.LAST_UPDATE_DATE = DateTime.Now;
                        context.AFR.Update(rm);
                    }
                    await context.SaveChangesAsync();
                    string msg = "";
                    for (int i = 0; i < id.Length; i++)
                    {
                        //发送短信
                        msg = await SendMsg(msg, "step3", id[i].ToString());
                    }
                    tran.Commit();
                    Msg = "{\"code\":200,\"msg\":\"审核成功\"}";
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Msg = "{\"code\":300,\"msg\":\"审核失败，请联系管理员\"}";
                }
            }
            return Content(Msg);
        }
        #endregion

        #region 异地就医：(市医保局)：查询
        [HttpPost]
        public async Task<IActionResult> GetYdjybxByStatusSybj(string status,string person_name, int page, int limit)
        {
            string Msg = "";
            string sql = @"SELECT *
  FROM(SELECT ROWNUM AS rowno, r.*
          FROM( Select (Select Count(*)
                          From Anotherplaceafr
                         Where status = :status and person_name like '%'|| :person_name ||'%'
                           And Flow_Type = '市医保局复核') Totalpage,
                       Afr_Id,
                       Flow_No,
                       Deal_With_Number,
                       Flow_Type,
                       Id_Card_Number,
                       Person_Name,
                       Tran_Date,
                       Tran_Person,
                       Access_Msg,
                       Fail_Msg,
                       Fail_Reason,
                       Decode(Status,
                              0,
                              '编辑',
                              1,
                              '待补充材料',
                              2,
                              '票据材料已接收',
                              3,
                              '韶钢医保办待审核',
                              4,
                              '韶钢医保办复核通过',
                              5,
                              '韶钢医保办复核退回',
                              6,
                              '市医保局待复核',
                              7,
                              '市医保局复核通过',
                              8,
                              '市医保局退回',
                              9,
                              '待申请支付',
                              10,
                              '已申请支付') Status,
                       Person_Id,attribute2
                  From Anotherplaceafr
                 Where status = :status1  and person_name like '%'|| :person_name1 ||'%'
                   And Flow_Type = '市医保局复核'
                 Order By Deal_With_Number Asc) r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (: page - 1) * :limit";
            OracleParameter[] sp = { data.MakeInParam(":status",status??""), data.MakeInParam(":person_name",person_name??""),
                                     data.MakeInParam(":status1",status??""),data.MakeInParam(":person_name1",person_name??""),
                                     data.MakeInParam(":page", page),
                                     data.MakeInParam(":limit", limit)};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 市医保局审核：退回到票据材料接收
        [HttpPost]
        public async Task<IActionResult> SybjBackPjjs_ydjybx(long[] id, string backReason)
        {
            string Msg = "";
            using (var tran = context.Database.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < id.Length; i++)
                    {
                        var rm = await Task.FromResult(context.AFR.ToList().Where(u => u.AFR_ID.Equals(id[i])).FirstOrDefault());
                        ANOTHERPLACEAFR afr = new ANOTHERPLACEAFR()
                        {
                            FLOW_NO = rm.FLOW_NO,
                            FLOW_TYPE = "票据材料接收",
                            ID_CARD_NUMBER = rm.ID_CARD_NUMBER,
                            PERSON_NAME = rm.PERSON_NAME,
                            STATUS = 1,
                            PERSON_ID = rm.PERSON_ID,
                            CREATION_DATE = DateTime.Now,
                            CREATION_BY = int.Parse(HttpContext.Session.GetString("USER_ID")),
                            DEAL_WITH_NUMBER = rm.DEAL_WITH_NUMBER + 1,
                            PHONE = rm.PHONE
                        };
                        await context.AFR.AddAsync(afr);
                        rm.STATUS = 8;
                        rm.FAIL_REASON = backReason ?? "";
                        rm.LAST_UPDATE_BY = int.Parse(HttpContext.Session.GetString("USER_ID"));
                        rm.LAST_UPDATE_DATE = DateTime.Now;
                        context.AFR.Update(rm);
                    }
                    await context.SaveChangesAsync();
                    string msg = "";
                    for (int i = 0; i < id.Length; i++)
                    {
                        //发送短信
                        msg = await SendMsg(msg, "fail_msg", id[i].ToString());
                    }
                    tran.Commit();
                    Msg = "{\"code\":200,\"msg\":\"退回成功\"}";
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Msg = "{\"code\":300,\"msg\":\"退回失败，请联系管理员\"}";
                }
            }
            return Content(Msg);
        }
        #endregion

        #region 市医保局审核：复核通过并转交到财务申请支付
        [HttpPost]
        public async Task<IActionResult> CommitToPay_ydjybx(long[] id)
        {
            string Msg = "";
            using (var tran = context.Database.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < id.Length; i++)
                    {
                        var rm = await Task.FromResult(context.AFR.ToList().Where(u => u.AFR_ID.Equals(id[i])).FirstOrDefault());
                        ANOTHERPLACEAFR afr = new ANOTHERPLACEAFR()
                        {
                            FLOW_NO = rm.FLOW_NO,
                            FLOW_TYPE = "申请支付",
                            ID_CARD_NUMBER = rm.ID_CARD_NUMBER,
                            PERSON_NAME = rm.PERSON_NAME,
                            STATUS = 9,
                            PERSON_ID = rm.PERSON_ID,
                            CREATION_DATE = DateTime.Now,
                            ATTRIBUTE2 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            CREATION_BY = int.Parse(HttpContext.Session.GetString("USER_ID")),
                            DEAL_WITH_NUMBER = rm.DEAL_WITH_NUMBER + 1,
                            PHONE = rm.PHONE
                        };
                        await context.AFR.AddAsync(afr);
                        rm.STATUS = 7;
                        rm.TRAN_DATE = DateTime.Now;
                        rm.TRAN_PERSON = HttpContext.Session.GetString("USER_NAME");
                        rm.LAST_UPDATE_BY = int.Parse(HttpContext.Session.GetString("USER_ID"));
                        rm.LAST_UPDATE_DATE = DateTime.Now;
                        context.AFR.Update(rm);
                    }
                    await context.SaveChangesAsync();
                    for (int i = 0; i < id.Length; i++)
                    {
                        //发送短信
                    }
                    tran.Commit();
                    Msg = "{\"code\":200,\"msg\":\"审核成功\"}";
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Msg = "{\"code\":300,\"msg\":\"审核失败，请联系管理员\"}";
                }
            }
            return Content(Msg);
        }
        #endregion

        #region 异地就医：(申请支付)：查询
        [HttpPost]
        public async Task<IActionResult> GetYdjybxByStatusSqzf(string status,string person_name, int page, int limit)
        {
            string Msg = "";
            string sql = @"SELECT *
  FROM(SELECT ROWNUM AS rowno, r.*
          FROM( Select (Select Count(*)
                          From Anotherplaceafr
                         Where status = :status and person_name like '%'|| :person_name ||'%'
                           And Flow_Type = '申请支付') Totalpage,
                       Afr_Id,
                       Flow_No,
                       Deal_With_Number,
                       Flow_Type,
                       Id_Card_Number,
                       Person_Name,
                       Tran_Date,
                       Tran_Person,
                       Access_Msg,
                       Fail_Msg,
                       Fail_Reason,
                       Decode(Status,
                              0,
                              '编辑',
                              1,
                              '待补充材料',
                              2,
                              '票据材料已接收',
                              3,
                              '韶钢医保办待审核',
                              4,
                              '韶钢医保办复核通过',
                              5,
                              '韶钢医保办复核退回',
                              6,
                              '市医保局待复核',
                              7,
                              '市医保局复核通过',
                              8,
                              '市医保局退回',
                              9,
                              '待申请支付',
                              10,
                              '已申请支付') Status,
                       Person_Id,attribute2,nvl(pay_amt,0) pay_amt
                  From Anotherplaceafr
                 Where status = :status1 and person_name like '%'|| :person_name1 ||'%'
                   And Flow_Type = '申请支付'
                 Order By Deal_With_Number Asc) r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (: page - 1) * :limit";
            OracleParameter[] sp = { data.MakeInParam(":status",status??""), data.MakeInParam(":person_name",person_name??""),
                                     data.MakeInParam(":status1",status??""),data.MakeInParam(":person_name1",person_name??""),
                                     data.MakeInParam(":page", page),
                                     data.MakeInParam(":limit", limit)};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 申请支付：发起申请支付
        [HttpPost]
        public async Task<IActionResult> CommitPay_ydjybx(long[] id)
        {
            string Msg = "";
            var err = 0;
            using (var tran = context.Database.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < id.Length; i++)
                    {
                        var rm = await Task.FromResult(context.AFR.ToList().Where(u => u.AFR_ID.Equals(id[i])).FirstOrDefault());
                        if (rm.PAY_AMT==null)
                        {
                            Msg = "{\"code\":300,\"msg\":\""+rm.PERSON_NAME+"未填写支付金额，请先单击支付金额单元格进行金额填写\"}";
                            tran.Rollback();
                            err++;
                            break;
                        }
                        else
                        {
                            rm.STATUS = 10;
                            rm.TRAN_DATE = DateTime.Now;
                            rm.TRAN_PERSON = HttpContext.Session.GetString("USER_NAME");
                            rm.LAST_UPDATE_BY = int.Parse(HttpContext.Session.GetString("USER_ID"));
                            rm.LAST_UPDATE_DATE = DateTime.Now;
                            context.AFR.Update(rm);
                        }                        
                    }
                    if (err==0)
                    {
                        await context.SaveChangesAsync();
                        string msg = "";
                        for (int i = 0; i < id.Length; i++)
                        {
                            //发送短信
                            msg = await SendMsg3(msg,  id[i].ToString());
                        }
                        tran.Commit();
                        Msg = "{\"code\":200,\"msg\":\"发起成功\"}";
                    }                    
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Msg = "{\"code\":300,\"msg\":\"发起失败，请联系管理员\"}";
                }
            }
            return Content(Msg);
        }
        #endregion

        #region 流程查询
        [HttpPost]
        public async Task<IActionResult> GetYdjybxQuery(string person_id, string flow_no,int page, int limit)
        {
            string Msg = "";
            string sql = @"SELECT *
  FROM(SELECT ROWNUM AS rowno, r.*
          FROM( Select (Select Count(*)
                          From Anotherplaceafr
                         Where person_id=:person_id  and Flow_No like '%' ||:flow_no || '%') Totalpage,
                       Afr_Id,
                       Flow_No,
                       Deal_With_Number,
                       Flow_Type,
                       Id_Card_Number,
                       Person_Name,
                       Tran_Date,
                       Tran_Person,
                       Access_Msg,
                       Fail_Msg,
                       Fail_Reason,
                       Decode(Status,
                              0,
                              '编辑',
                              1,
                              '待补充材料',
                              2,
                              '票据材料已接收',
                              3,
                              '韶钢医保办待审核',
                              4,
                              '韶钢医保办复核通过',
                              5,
                              '韶钢医保办复核退回',
                              6,
                              '市医保局待复核',
                              7,
                              '市医保局复核通过',
                              8,
                              '市医保局退回',
                              9,
                              '待申请支付',
                              10,
                              '已申请支付') Status,
                       Person_Id,attribute2,pay_amt
                  From Anotherplaceafr
                  Where person_id=:person_id1  and Flow_No like '%' ||:flow_no1 || '%'
                 Order By Flow_No,Deal_With_Number Asc) r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (: page - 1) * :limit";
            OracleParameter[] sp = { data.MakeInParam(":person_id",person_id??""),
                                     data.MakeInParam(":flow_no",flow_no??""),
                                     data.MakeInParam(":person_id1",person_id??""),
                                     data.MakeInParam(":flow_no1",flow_no??""),
                                     data.MakeInParam(":page", page),
                                     data.MakeInParam(":limit", limit)};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 申请支付金额：修改
        [HttpPost]
        public async Task<IActionResult> SavePayAmt(string afr_id,string pay_amt)
        {
            string sql = @"Update Anotherplaceafr
                                   Set Pay_Amt=:Pay_Amt,
                                       Last_Update_Date = Sysdate,
                                       Last_Update_By   = :user_id
                                 Where Afr_Id = :Afr_Id";
            OracleParameter[] sp = {
                data.MakeInParam(":Pay_Amt",pay_amt??""),
                data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")??""),
                data.MakeInParam(":Afr_Id",afr_id??"")};
            bool flag = await data.DoSqlByParam(sql, sp);
            string Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败，请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 发送短信
        private async Task<string> SendMsg(string msg, string step, string afr_id)
        {
            var phone = "";
            //优先使用流程表中的手机号码进行发送短信
            phone = await data.GetStringByParam(@"select phone from Anotherplaceafr where afr_id=:afr_id", new OracleParameter[] { data.MakeInParam(":afr_id", afr_id) });
            if (phone == "" || phone == null)
            {
                phone = await data.GetStringByParam(@"select PHONE from TXRY_PERSON where person_id=(select person_id from Anotherplaceafr where afr_id=:afr_id)", new OracleParameter[] { data.MakeInParam(":afr_id", afr_id) });
            }
            var url = "http://sms.106jiekou.com/utf8/sms.aspx";
            var content = await data.GetString(@"Select f.Fixvalue_Name
                                              From App_Fixvalue f, App_Fixvalue_Type Ft
                                             Where f.Fixvalue_Type_Id = Ft.Fixvalue_Type_Id
                                               And Ft.Fixvalue_Type_Code = 'ydjybx'
                                               And f.Fixvalue_Code = '" + step + @"'");
            try
            {
                var client = httpClient.CreateClient();
                content = "您好！" + content + "具体流程办理进度，请进入“韶钢e退管”小程序查看。祝您身体安康。";
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

        #region 票据材料接收导入
        [HttpPost]
        public async Task<IActionResult> FileUpload_Pjcljs([FromForm]IFormFile file)
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
                    List<ExcelPjcljs> ec = excel.GetList<ExcelPjcljs>(path).ToList();
                    if (ec.Count > 0)
                    {
                        for (int i = 0; i < ec.Count; i++)
                        {
                            if (ec[i].ATTRIBUTE2 == null || ec[i].ATTRIBUTE2 == "")
                            {
                                returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + ec[i].PERSON_NAME + "：接收时间不能为空。";
                            }
                            else
                            {
                                string person_id = await data.GetStringByParam(@"select person_id from TXRY_PERSON where person_name=:PERSON_NAME and id_card_number=:ID_CARD_NUMBER", new OracleParameter[]
                                            { data.MakeInParam(":PERSON_NAME",ec[i].PERSON_NAME),
                                                data.MakeInParam(":ID_CARD_NUMBER",ec[i].ID_CARD_NUMBER)});
                                if (person_id == "" || person_id == null)
                                {
                                    returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + ec[i].PERSON_NAME + "：在退休人员表中不存在。";
                                }
                                else
                                {
                                    string phone = "";
                                    if (ec[i].PHONE == null || ec[i].PHONE == "")
                                    {
                                        phone = await data.GetStringByParam(@"select person_id from TXRY_PERSON where person_id=:person_id", new OracleParameter[]
                                            { data.MakeInParam(":person_id",person_id)});
                                    }
                                    else
                                    {
                                        phone = ec[i].PHONE.Trim();
                                    }
                                    OracleParameter[] p = { data.MakeInParam(":p_Inf", OracleDbType.Varchar2, 1000, ParameterDirection.Output) };
                                    string flow_no = await data.RunProcStr("Ydjybx.Get_Flow_Number", p);
                                    string afr_id = await data.GetString(@"select Anotherplaceafr_s.nextval from dual");
                                    string sql = @"Insert Into Anotherplaceafr
                                                      (Afr_Id,
                                                       Flow_No,
                                                       Deal_With_Number,
                                                       Flow_Type,
                                                       Id_Card_Number,
                                                       Person_Name,
                                                       Status,
                                                       Attribute2,
                                                       Creation_Date,
                                                       Creation_By,
                                                       Person_Id,Phone)
                                                    Values
                                                      (:afr_id,
                                                       '" + flow_no + @"',
                                                       1,
                                                       '票据材料接收',
                                                       :Id_Card_Number,
                                                       :Person_Name,
                                                       0,
                                                       :Attribute2,
                                                       Sysdate,
                                                       :Creation_By,
                                                       :Person_Id,:Phone)";
                                    bool flag = await data.DoSqlByParam(sql, new OracleParameter[]
                                    {
                                    data.MakeInParam(":afr_id",afr_id),
                                    data.MakeInParam(":Id_Card_Number",ec[i].ID_CARD_NUMBER),
                                    data.MakeInParam(":Person_Name",ec[i].PERSON_NAME),
                                    data.MakeInParam(":Attribute2",(ec[i].ATTRIBUTE2+" 08:00:00")),
                                    data.MakeInParam(":Creation_By",HttpContext.Session.GetString("USER_ID")),
                                    data.MakeInParam(":Person_Id",person_id),
                                    data.MakeInParam(":Phone",phone)
                                    });
                                    if (flag)
                                    {
                                        returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + ec[i].PERSON_NAME + "：导入成功。";
                                        returnMsg += await SendMsg2("", afr_id);
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

        #region 发送短信2
        private async Task<string> SendMsg2(string msg, string afr_id)
        {
            OracleParameter[] p = { data.MakeInParam(":afr_id", afr_id) };
            var phone = "";
            //优先使用流程表中的手机号码进行发送短信
            phone = await data.GetStringByParam(@"select phone from Anotherplaceafr where afr_id=:afr_id", p);
            if (phone==""||phone==null)
            {
                phone = await data.GetStringByParam(@"select PHONE from TXRY_PERSON where person_id=(select person_id from Anotherplaceafr where afr_id=:afr_id)", p);
            }            
            var day = await data.GetStringByParam(@"select extract(day from to_date(t.attribute2,'yyyy-MM-dd hh24:mi:ss')) from anotherplaceafr t Where t.afr_id=:afr_id",p);
            var url = "http://sms.106jiekou.com/utf8/sms.aspx";
            var content = "";
            if (int.Parse(day)>=20)
            {
                //25个工作日
                 content = await data.GetString(@"Select f.Fixvalue_Name
                                              From App_Fixvalue f, App_Fixvalue_Type Ft
                                             Where f.Fixvalue_Type_Id = Ft.Fixvalue_Type_Id
                                               And Ft.Fixvalue_Type_Code = 'ydjybx'
                                               And f.Fixvalue_Code = 'step1'");
            }
            else if (int.Parse(day) <20)
            {
                //15个工作日
                content = await data.GetString(@"Select f.Fixvalue_Name
                                              From App_Fixvalue f, App_Fixvalue_Type Ft
                                             Where f.Fixvalue_Type_Id = Ft.Fixvalue_Type_Id
                                               And Ft.Fixvalue_Type_Code = 'ydjybx'
                                               And f.Fixvalue_Code = 'step1_2'");
            }
            
            try
            {
                var client = httpClient.CreateClient();
                content = "您好！" + content + "具体流程办理进度，请进入“韶钢e退管”小程序查看。祝您身体安康。";
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

        #region 导出：申请支付
        [HttpPost]
        public async Task<IActionResult> GetSqzfForExport(string person_name,  string status)
        {
            #region sql
            string sql = @"Select Flow_No, Flow_Type,person_name, Id_Card_Number, to_char(nvl(Pay_Amt,0)) Pay_Amt
                              From Anotherplaceafr
                             Where Person_Name Like '%' || :Person_Name || '%'
                               And Status = :Status";
            #endregion
            OracleParameter[] sp = { data.MakeInParam(":Person_Name", person_name ?? ""),
                                     data.MakeInParam(":Status", status ?? "") };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            IList<ExcelSqzf> list = data.DataSetToIList1<ExcelSqzf>(ds, 0);
            byte[] buffer = new byte[0];
            if (list==null)
            {

            }
            else
            {
                List<ExcelSqzf> sqzf = list.ToList();                
                if (sqzf == null)
                {

                }
                else
                {
                    buffer = ExcelHelper.Export(sqzf, "申请支付数据导出", ExcelTitle.Sqzf).GetBuffer();
                }
            }                        
            return File(buffer, "application/ms-excel", "申请支付数据导出("+DateTime.Now+").xls");
        }
        #endregion

        #region 申请支付导入
        [HttpPost]
        public async Task<IActionResult> FileUpload_Sqzf([FromForm]IFormFile file)
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
                    List<ExcelSqzf> ec = excel.GetList<ExcelSqzf>(path).ToList();
                    if (ec.Count > 0)
                    {
                        for (int i = 0; i < ec.Count; i++)
                        {
                            if (ec[i].FLOW_NO == null || ec[i].FLOW_NO == "")
                            {
                                returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + ec[i].PERSON_NAME + "：流程号不能为空。";
                            }
                            else
                            {
                                OracleParameter[] p = { data.MakeInParam(":flow_no", ec[i].FLOW_NO) };
                                string isexist = await data.GetStringByParam(@"Select Afr_Id From Anotherplaceafr
                                            Where Flow_No = :Flow_No And Flow_Type = '申请支付' And Status = 9", p);
                                if (isexist == "")
                                {
                                    returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + ec[i].PERSON_NAME + "/" + ec[i].FLOW_NO + "：不存在待申请支付状态的单据或流程已走完。";
                                }
                                else
                                {
                                    string sql = @"Update Anotherplaceafr
                                                       Set status=10, Last_Update_Date = Sysdate, tran_date=sysdate,TRAN_PERSON=:user_name,
                                                           Last_Update_By   = :Last_Update_By,
                                                           Pay_Amt          = :Pay_Amt
                                                     Where Afr_Id = :afr_id";
                                    bool flag = await data.DoSqlByParam(sql, new OracleParameter[]
                                    {
                                        data.MakeInParam(":user_name",HttpContext.Session.GetString("USER_NAME")),
                                        data.MakeInParam(":Last_Update_By",HttpContext.Session.GetString("USER_ID")),
                                        data.MakeInParam(":Pay_Amt",ec[i].PAY_AMT??"0"),
                                        data.MakeInParam(":afr_id",isexist)
                                    });
                                    if (flag)
                                    {
                                        returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + ec[i].PERSON_NAME + "/" + ec[i].FLOW_NO+ "：导入成功。";
                                        returnMsg += await SendMsg3("", isexist);
                                    }
                                    else
                                    {
                                        returnMsg += "【" + ec[i].ID_CARD_NUMBER + "】" + ec[i].PERSON_NAME + "/" + ec[i].FLOW_NO+ "：在导入数据时发生错误，请联系管理员。";
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

        #region 发送短信3
        private async Task<string> SendMsg3(string msg, string afr_id)
        {
            OracleParameter[] p = { data.MakeInParam(":afr_id", afr_id) };
            var phone = "";
            //优先使用流程表中的手机号码进行发送短信
            phone = await data.GetStringByParam(@"select phone from Anotherplaceafr where afr_id=:afr_id", p);
            if (phone == "" || phone == null)
            {
                phone = await data.GetStringByParam(@"select PHONE from TXRY_PERSON where person_id=(select person_id from Anotherplaceafr where afr_id=:afr_id)", p);
            }
            var day = await data.GetStringByParam(@"select extract(day from to_date(t.attribute2,'yyyy-MM-dd hh24:mi:ss')) from anotherplaceafr t Where t.afr_id=:afr_id", p);
            var url = "http://sms.106jiekou.com/utf8/sms.aspx";
            var content = await data.GetString(@"Select f.Fixvalue_Name
                                              From App_Fixvalue f, App_Fixvalue_Type Ft
                                             Where f.Fixvalue_Type_Id = Ft.Fixvalue_Type_Id
                                               And Ft.Fixvalue_Type_Code = 'ydjybx'
                                               And f.Fixvalue_Code = 'step4'");
            try
            {
                var client = httpClient.CreateClient();
                content = "您好！" + content + "具体流程办理进度，请进入“韶钢e退管”小程序查看。祝您身体安康。";
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