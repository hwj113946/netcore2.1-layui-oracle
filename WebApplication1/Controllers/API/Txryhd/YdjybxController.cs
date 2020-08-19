using System.Threading.Tasks;
//using Common;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using WebApplication1.Models.ApiModels;
using System;
using System.Linq;
using System.Collections;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace WebApplication1.Controllers.API.Txryhd
{
    [Route("api/Ydjybx")]
    [Produces("application/json")]
    [EnableCors("AllowSpecificOrigin")]//跨域
    public class YdjybxController : Controller
    {
        Common.DBHelper data = new Common.DBHelper();
        DataBaseContext context;
        private IHttpClientFactory httpClient;
        public YdjybxController(DataBaseContext context, IHttpClientFactory _httpClient)
        {
            this.context = context;
            this.httpClient = _httpClient;
        }

        #region 获取角色权限
        [HttpGet]
        [Route("GetRole")]
        public async Task<IActionResult> GetRole(pjcljs_param param)
        {
            if (param.USER_ID==null&&param.USER_ID=="")
            {
                return Json(new { code = 1, msg = "请传入用户主键",count=0,data=new { } });
            }
            try
            {
                string sql = @"Select Role_Name
                                  From (Select Distinct r.Role_Name, t.Role_Id
                                          From App_User_Role t, App_Role r
                                         Where t.Role_Id = r.Role_Id
                                           And t.User_Id = :user_id
                                           And t.Role_Id In ('46', '86', '87', '88', '89'))
                                 Order By Role_Id ";
                OracleParameter[] sp = { data.MakeInParam(":user_id", param.USER_ID ?? "") };
                DataSet ds = await data.GetDataSetByParam(sql, sp);
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

        #region 获取退休人员信息
        [Route("GetPerson")]
        [HttpGet]
        public async Task<IActionResult> GetPerson(GetPerson person)
        {
            if (person.PAGE > 0 && person.LIMIT > 0)
            {
                //string Msg = "";
                #region sql
                string sql = @"SELECT *
  FROM(SELECT ROWNUM AS rowno, r.*
          FROM(select to_char(ceil(COUNT(*) OVER ()/ :limit1)) totalPage,
To_Char(Person_Id) Person_Id,
       Person_Name,
       Sex,
       To_Char(Age) Age,
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
       To_Char(Status) Status,
       Emergency_Person,
       Emergency_Phone,
       Emergency_Address,
       Transfer_Type,
       Open_Id
                      From Txry_Person tp
                     Where Person_Name Like '%' || :Person_Name1 || '%') r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (: page - 1) * :limit";
                #endregion

                OracleParameter[] sp = {
                                     data.MakeInParam(":limit1", person.LIMIT),
                                     data.MakeInParam(":Person_Name1", person.PERSON_NAME ?? ""),
                                     data.MakeInParam(":page", person.PAGE),
                                    data.MakeInParam(":limit", person.LIMIT)};
                DataSet ds = await data.GetDataSetByParam(sql, sp);
                //Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
                //return Content(Msg);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    return Json(new { code = 0, msg = "已查询到数据", count = ds.Tables[0].Rows[0]["totalPage"], data = ds.Tables[0] });
                }
                else
                {
                    return Json(new { code = 1, msg = "未查询到数据", count = 0, data = ds.Tables[0] });
                }
                #region EFCORE
                //try
                //{
                //    var list = await Task.FromResult(context.TXRY_PERSON.ToList().Where(u => u.PERSON_NAME.Contains(person.PERSON_NAME ?? "")).ToList());
                //    if (list.Count > 0)
                //    {
                //        var newlist = list.Skip((person.PAGE - 1) * person.LIMIT).Take(person.LIMIT).ToList();
                //        return Json(new { code = 0, msg = "查询到数据", count = list.Count, data = newlist });
                //    }
                //    else
                //    {
                //        return Json(new { code = 1, msg = "暂无数据", count = 0, data = new { } });
                //    }
                //}
                //catch (Exception ex)
                //{
                //    return Json(new { code = 1, msg = "接口出错，请联系管理员", count = 0, data = new { } });
                //} 
                #endregion
            }
            else
            {
                return Json(new { code = 1, msg = "请传入参数值", count = 0 });
            }
        }
        #endregion

        #region 异地就医：票据材料接收：根据afr_id获取单据信息
        [HttpGet]
        [Route("GetInfo_ydjybx_pjcljs")]
        public async Task<IActionResult> GetInfo_ydjybx_pjcljs(pjcljs_param param)
        {
            if (param.AFR_ID == null || param.AFR_ID == "")
            {
                return Json(new { code = 1, msg = "请传入主键",count=0,data=new { } });
            }
            try
            {
                string sql = @"select To_Char(Afr_Id) Afr_Id,
                                      Flow_No,
                                      To_Char(Deal_With_Number) Deal_With_Number,
                                      Flow_Type,
                                      Id_Card_Number,
                                      Person_Name,
                                      To_Char(Tran_Date, 'yyyy-MM-dd hh24:mi:ss') Tran_Date,
                                      Tran_Person,
                                      Access_Msg,
                                      Fail_Msg,
                                      Fail_Reason,
                                      To_Char(Status) Status,
                                      Attribute1,
                                      Attribute2,
                                      To_Char(Person_Id) Person_Id,to_char(pay_amt) pay_amt,phone from anotherplaceafr t Where t.afr_id=:afr_id";
                OracleParameter[] sp = { data.MakeInParam(":afr_id", param.AFR_ID ?? "") };
                DataSet ds = await data.GetDataSetByParam(sql, sp);
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
                return Json(new { code = 1, msg = "查询出错，请联系管理员", count = 0, data = new { } });
            }
        }
        #endregion

        #region 票据材料接收：根据人员id获取处理进度
        [HttpGet]
        [Route("GetYdjybxByPerson")]
        public async Task<IActionResult> GetYdjybxByPerson(pjcljs_param param)
        {
            string pj = "";
            switch (param.STATUS)
            {
                case "全部":pj=" and status in(0,1,2) ";break;
                case "编辑": pj = " and status =0 "; break;
                case "待补充材料": pj = " and status =1 "; break;
                case "票据材料已接收": pj = " and status =2 "; break;
            }
            string sql = @"SELECT *
  FROM(SELECT ROWNUM AS rowno, r.*
          FROM( Select to_char(ceil(COUNT(*) OVER ()/ :limit1)) totalPage,
                       to_char(Afr_Id) Afr_Id,
                       Flow_No,
                       to_char(Deal_With_Number) Deal_With_Number,
                       Flow_Type,
                       Id_Card_Number,
                       Person_Name,
                       To_Char(Tran_Date, 'yyyy-MM-dd hh24:mi:ss') Tran_Date,
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
                       to_char(Person_Id) Person_Id,attribute2
                  From Anotherplaceafr
                 Where Person_Id = :Person_Id1 and flow_no like '%' ||:flow_no1 || '%'
                   And Flow_Type = '票据材料接收' " + pj + @"
                 Order By Flow_No,Deal_With_Number Asc) r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (: page - 1) * :limit";
            OracleParameter[] sp = { data.MakeInParam(":limit1", param.LIMIT),
                                     data.MakeInParam(":Person_Id1",param.PERSON_ID??""),
                                     data.MakeInParam(":flow_no1",param.FLOW_NO??""),
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
        #endregion

        #region 票据材料接收：新增：保存
        [HttpPost]
        [Route("Insert_ydjybx")]
        public async Task<IActionResult> Insert_ydjybx(pjcljs_param param)
        {
            bool flag = false;
            string msg = "";
            try
            {
                #region 数据库操作
                OracleParameter[] p = { data.MakeInParam(":p_Inf", OracleDbType.Varchar2, 1000, ParameterDirection.Output) };
                string flow_no = await data.RunProcStr("Ydjybx.Get_Flow_Number", p);
                string afr_id =await data.GetString(@"select Anotherplaceafr_s.Nextval from dual");
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
                    data.MakeInParam(":afr_id",afr_id??""),
                    data.MakeInParam(":flow_no",flow_no??""),
                    data.MakeInParam(":id_card_number",param.ID_CARD_NUMBER??""),
                    data.MakeInParam(":person_name",param.PERSON_NAME??""),
                    data.MakeInParam(":user_name",param.USER_NAME??""),
                    data.MakeInParam(":attribute2",param.ATTRIBUTE2??""),
                    data.MakeInParam(":user_id",param.USER_ID??""),
                    data.MakeInParam(":person_id",param.PERSON_ID??""),
                    data.MakeInParam(":phone",param.PHONE??"")};
                flag = await data.DoSqlByParam(sql, sp); 
                #endregion
                if (flag)
                {
                    msg = await SendMsg2(msg,  afr_id);
                }
            }
            catch (Exception ex)
            {
                flag = false;
            }
            return Json(flag ? new { code = 200, msg = "保存成功,"+ msg } : new { code = 300, msg = "保存失败，请联系管理员" });
        }
        #endregion

        #region 票据材料接收：修改：保存
        [HttpPost]
        [Route("Modify_ydjybx")]
        public async Task<IActionResult> Modify_ydjybx(pjcljs_param param)
        {
            if (param.AFR_ID == null || param.AFR_ID == "")
            {
                return Json(new { code = 300, msg = "请传入主键" });
            }
            bool flag = false;
            try
            {
                string sql = @"Update Anotherplaceafr
                                   Set Id_Card_Number   = :id_card_number,
                                       Person_Name      = :person_name,
                                       Attribute2       = :attribute2,
                                       Last_Update_Date = Sysdate,
                                       Last_Update_By   = :user_id,
                                       Person_Id        = :person_id,phone=:phone
                                 Where Afr_Id = :Afr_Id";
                OracleParameter[] sp = {
                data.MakeInParam(":id_card_number",param.ID_CARD_NUMBER??""),
                data.MakeInParam(":person_name",param.PERSON_NAME??""),
                data.MakeInParam(":attribute2",param.ATTRIBUTE2??""),
                data.MakeInParam(":user_id",param.USER_ID??""),
                data.MakeInParam(":person_id",param.PERSON_ID??""),data.MakeInParam(":phone",param.PHONE??""),
                data.MakeInParam(":Afr_Id",param.AFR_ID??"")};
                flag = await data.DoSqlByParam(sql, sp);
            }
            catch (Exception ex)
            {
                flag = false;
            }
            return Json(flag ? new { code = 200, msg = "保存成功" } : new { code = 300, msg = "保存失败，请联系管理员" });
        }
        #endregion

        #region 票据材料接收：转回原节点
        [HttpPost]
        [Route("ReturnNode_ydjybx")]
        public async Task<IActionResult> ReturnNode_ydjybx(pjcljs_param param)
        {
            if (param.AFR_ID == null || param.AFR_ID == "")
            {
                return Json(new { code=300,msg= "请传入主键" });
            }
            var n = await data.GetStringByParam(@"select count(*) from ANOTHERPLACEAFR where AFR_ID=:afr_id", new OracleParameter[] { data.MakeInParam(":afr_id", param.AFR_ID) });
            if (n == "0")
            {
                return Json(new { code = 300, msg = "传入的主键找不到相应单据" });
            }
            var attribute2 = await data.GetStringByParam(@"select nvl(attribute2,0) from ANOTHERPLACEAFR where AFR_ID=:afr_id", new OracleParameter[] { data.MakeInParam(":afr_id", param.AFR_ID) });
            if (attribute2=="0")
            {
                return Json(new { code = 300, msg = "请先填写材料接收时间" });
            }
            string msg = "";
            try
            {
                OracleParameter[] sp = {
                data.MakeInParam(":v_Afr_Id",param.AFR_ID??""),
                data.MakeInParam(":v_User_Id",param.USER_ID??""),
                data.MakeInParam(":p_Inf",OracleDbType.Varchar2,1000, ParameterDirection.Output)};
                msg = await data.RunProcStr("Ydjybx.Returnnode", sp);
            }
            catch (Exception ex)
            {
                
            }
            return Json(msg == "Y" ? new { code = 200, msg = "转交成功" } : new { code = 300, msg = ("转交失败，请联系管理员：" + msg) });
        }
        #endregion

        #region 票据材料接收：提交
        [HttpPost]
        [Route("CommitToYbb_ydjybx")]
        public async Task<IActionResult> CommitToYbb_ydjybx(pjcljs_param param)
        {
            if (param.AFR_ID == null || param.AFR_ID == "")
            {
                return Json(new { code = 300, msg = "请传入主键" });
            }
            bool flag = false;
            string msg = "";
            #region 数据库操作
            string sql = @"Update Anotherplaceafr
                                   Set 
                                       TRAN_PERSON       = :tran_person,
                                       TRAN_DATE         = sysdate,
                                       status            = 2,
                                       Last_Update_Date = Sysdate,
                                       Last_Update_By   = :user_id
                                 Where Afr_Id = :Afr_Id";
            OracleParameter[] sp = {
                data.MakeInParam(":tran_person",param.USER_NAME??""),
                data.MakeInParam(":user_id",param.USER_ID??""),
                data.MakeInParam(":Afr_Id",param.AFR_ID??"")};
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
            OracleParameter[] sp1 = { data.MakeInParam(":user_id", param.USER_ID ?? ""), data.MakeInParam(":Afr_Id", param.AFR_ID ?? "") };
            Hashtable ht = new Hashtable();
            ht.Add(sql1, sp1);
            ht.Add(sql, sp);
            flag = await data.DoSqlList(ht);
            #endregion
            if (flag)
            {
                msg = await SendMsg(msg, "step2", param.AFR_ID);
            }
            return Json(flag ? new
            {
                code = 200,
                msg = "提交成功，" + msg
            } : new { code = 300, msg = "提交失败，请联系管理员，" + msg });
        }
        #endregion

        #region 票据材料接收：查询
        [HttpGet]
        [Route("GetPjjscl_Ydjybx")]
        public async Task<IActionResult> GetPjjscl_Ydjybx(pjcljs_param param)
        {
            string pj = "";
            switch (param.STATUS)
            {
                case "全部": pj = " and status in(0,1,2) "; break;
                case "编辑": pj = " and status =0 "; break;
                case "待补充材料": pj = " and status =1 "; break;
                case "票据材料已接收": pj = " and status =2 "; break;
            }
            string sql = @"SELECT *
  FROM(SELECT ROWNUM AS rowno, r.*
          FROM( Select to_char(ceil(COUNT(*) OVER ()/ :limit1)) totalPage,
                       to_char(Afr_Id) Afr_Id,
                       Flow_No,
                       to_char(Deal_With_Number) Deal_With_Number,
                       Flow_Type,
                       Id_Card_Number,
                       Person_Name,
                       To_Char(Tran_Date, 'yyyy-MM-dd hh24:mi:ss') Tran_Date,
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
                       to_char(Person_Id) Person_Id,attribute2,to_char(pay_amt) pay_amt,phone
                  From Anotherplaceafr
                 Where person_name like '%' ||:person_name || '%'
                   And Flow_Type = '票据材料接收' " + pj + @"
                 Order By creation_date desc) r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (: page - 1) * :limit";
            try
            {
                OracleParameter[] sp = { data.MakeInParam(":limit1", param.LIMIT),
                                     data.MakeInParam(":person_name",param.PERSON_NAME??""),
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
                return Json(new { code = 1, msg = "请求出错，请联系管理员", count = 0, data = new { } });
            }
        }
        #endregion

        #region 韶钢医保办审核：查询
        [HttpGet]
        [Route("GetYdjybxByStatusYBB")]
        public async Task<IActionResult> GetYdjybxByStatusYBB(pjcljs_param param)
        {
            string pj = "";
            switch (param.STATUS)
            {
                case "全部": pj = " and status in(3,4,5) "; break;
                case "待审核": pj = " and status =3 "; break;
                case "审核通过": pj = " and status =4 "; break;
                case "退回": pj = " and status =5 "; break;
            }
            string sql = @"SELECT *
  FROM(SELECT ROWNUM AS rowno, r.*
          FROM( Select to_char(ceil(COUNT(*) OVER ()/ :limit1)) totalPage,
                       to_char(Afr_Id) Afr_Id,
                       Flow_No,
                       to_char(Deal_With_Number) Deal_With_Number,
                       Flow_Type,
                       Id_Card_Number,
                       Person_Name,
                       To_Char(Tran_Date, 'yyyy-MM-dd hh24:mi:ss') Tran_Date,
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
                       to_char(Person_Id) Person_Id,attribute2,to_char(pay_amt) pay_amt,phone
                  From Anotherplaceafr
                 Where person_name like '%'|| :person_name1 ||'%'
                   And Flow_Type = '韶钢医保办审核' " + pj + @"
                 Order By creation_date desc) r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (: page - 1) * :limit";
            try
            {
                OracleParameter[] sp = {  data.MakeInParam(":limit", param.LIMIT),
                                     data.MakeInParam(":person_name1",param.PERSON_NAME??""),
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
                return Json(new { code = 1, msg = "请求出错，请联系管理员", count = 0, data = new { } });
            }
        }
        #endregion

        #region 市医保局：查询
        [HttpGet]
        [Route("GetYdjybxByStatusSybj")]
        public async Task<IActionResult> GetYdjybxByStatusSybj(pjcljs_param param)
        {
            string pj = "";
            switch (param.STATUS)
            {
                case "全部": pj = " and status in(6,7,8) "; break;
                case "待审核": pj = " and status =6 "; break;
                case "审核通过": pj = " and status =7 "; break;
                case "退回": pj = " and status =8 "; break;
            }
            string sql = @"SELECT *
  FROM(SELECT ROWNUM AS rowno, r.*
          FROM( Select to_char(ceil(COUNT(*) OVER ()/ :limit1)) totalPage,
                       to_char(Afr_Id) Afr_Id,
                       Flow_No,
                       to_char(Deal_With_Number) Deal_With_Number,
                       Flow_Type,
                       Id_Card_Number,
                       Person_Name,
                       To_Char(Tran_Date, 'yyyy-MM-dd hh24:mi:ss') Tran_Date,
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
                       to_char(Person_Id) Person_Id,attribute2,to_char(pay_amt) pay_amt,phone
                  From Anotherplaceafr
                 Where person_name like '%'|| :person_name1 ||'%'
                   And Flow_Type = '市医保局复核' " + pj+ @"
                 Order By creation_date desc) r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (: page - 1) * :limit";
            try
            {
                OracleParameter[] sp = { data.MakeInParam(":limit1",param.LIMIT),
                                     data.MakeInParam(":person_name1",param.PERSON_NAME??""),
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
                return Json(new { code = 1, msg = "请求出错，请联系管理员", count = 0, data = new { } });
            }
        }
        #endregion

        #region 申请支付：查询
        [HttpGet]
        [Route("GetYdjybxByStatusSqzf")]
        public async Task<IActionResult> GetYdjybxByStatusSqzf(pjcljs_param param)
        {
            string pj = "";
            switch (param.STATUS)
            {
                case "待申请支付": pj = " and status =9 "; break;
                case "已申请支付": pj = " and status =10 "; break;
            }
            string sql = @"SELECT *
  FROM(SELECT ROWNUM AS rowno, r.*
          FROM( Select to_char(ceil(COUNT(*) OVER ()/ :limit1)) totalPage,
                       to_char(Afr_Id) Afr_Id,
                       Flow_No,
                       to_char(Deal_With_Number) Deal_With_Number,
                       Flow_Type,
                       Id_Card_Number,
                       Person_Name,
                       To_Char(Tran_Date, 'yyyy-MM-dd hh24:mi:ss') Tran_Date,
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
                       to_char(Person_Id) Person_Id,attribute2,to_char(pay_amt) pay_amt,phone
                  From Anotherplaceafr
                 Where person_name like '%'|| :person_name1 ||'%'
                   And Flow_Type = '申请支付' " + pj+ @"
                 Order By creation_date desc) r
         where ROWNUM <= :page * :limit) table_alias
 WHERE table_alias.rowno > (: page - 1) * :limit";
            try
            {
                OracleParameter[] sp = { data.MakeInParam(":limit1",param.LIMIT),
                                     data.MakeInParam(":person_name1",param.PERSON_NAME??""),
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
                return Json(new { code = 1, msg = "请求出错，请联系管理员", count = 0, data = new { } });
            }
        }
        #endregion

        #region 医保办审核：退回到票据材料接收
        [HttpPost]
        [Route("BackPjjs_ydjybx")]
        public async Task<IActionResult> BackPjjs_ydjybx(Sgybb_param param)
        {
            if (param.AFR_ID==null||param.AFR_ID=="")
            {
                return Json(new { code=300,msg="请传入主键"});
            }
            var n = await data.GetStringByParam(@"select count(*) from ANOTHERPLACEAFR where AFR_ID=:afr_id",new OracleParameter[] { data.MakeInParam(":afr_id",param.AFR_ID)});
            if (n=="0")
            {
                return Json(new { code = 300, msg = "传入的主键找不到相应单据" });
            }
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
                                       Person_Id,phone)
                                      Select Anotherplaceafr_s.Nextval,
                                             Flow_No,
                                             Deal_With_Number + 1,
                                             '票据材料接收',
                                             Id_Card_Number,
                                             Person_Name,
                                             1,
                                             null,
                                             sysdate,
                                             :user_id,
                                             Person_Id,phone
                                        From Anotherplaceafr
                                       Where Afr_Id = :Afr_Id";
            OracleParameter[] sp = {data.MakeInParam(":user_id",param.USER_ID),data.MakeInParam(":Afr_Id", param.AFR_ID) };
            string sql1 = @"Update Anotherplaceafr
                               Set Tran_Date        = Sysdate,
                                   Tran_Person     =
                                   (Select User_Name From App_User Where User_Id = :User_Id),
                                   Fail_Reason      = :Fail_Reason,
                                   Status           = 5,
                                   Last_Update_Date = Sysdate,
                                   Last_Update_By   = :User_Id1
                             Where Afr_Id = :Afr_Id";
            OracleParameter[] sp1 = { data.MakeInParam(":User_Id", param.USER_ID),
                                      data.MakeInParam(":Fail_Reason", param.BACKREASON??""),
                                      data.MakeInParam(":User_Id1", param.USER_ID),
                                      data.MakeInParam(":Afr_Id", param.AFR_ID) };
            Hashtable ht = new Hashtable();
            ht.Add(sql,sp);
            ht.Add(sql1,sp1);
            bool flag = await data.DoSqlList(ht);
            string msg = "";
            if (flag)
            {
                //发送短信
                msg = await SendMsg(msg, "fail_msg", param.AFR_ID);
                return Json(new { code = 200, msg = "退回成功，"+msg });
            }
            else
            {
                return Json(new { code = 300, msg = "退回失败，请联系管理员" });
            }
        }
        #endregion

        #region 医保办审核：复核通过并转交到市医保局
        [HttpPost]
        [Route("CommitToSybj_ydjybx")]
        public async Task<IActionResult> CommitToSybj_ydjybx(Sgybb_param param)
        {
            if (param.AFR_ID == null || param.AFR_ID == "")
            {
                return Json(new { code = 300, msg = "请传入主键" });
            }
            var n = await data.GetStringByParam(@"select count(*) from ANOTHERPLACEAFR where AFR_ID=:afr_id", new OracleParameter[] { data.MakeInParam(":afr_id", param.AFR_ID) });
            if (n == "0")
            {
                return Json(new { code = 300, msg = "传入的主键找不到相应单据" });
            }
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
                                       Person_Id,phone)
                                      Select Anotherplaceafr_s.Nextval,
                                             Flow_No,
                                             Deal_With_Number + 1,
                                             '市医保局复核',
                                             Id_Card_Number,
                                             Person_Name,
                                             6,
                                             to_char(sysdate,'yyyy-MM-dd hh24:mi:ss'),
                                             sysdate,
                                             :user_id,
                                             Person_Id,phone
                                        From Anotherplaceafr
                                       Where Afr_Id = :Afr_Id";
            OracleParameter[] sp = { data.MakeInParam(":user_id", param.USER_ID), data.MakeInParam(":Afr_Id", param.AFR_ID) };
            string sql1 = @"Update Anotherplaceafr
                               Set Tran_Date        = Sysdate,
                                   Tran_Person     = :User_Name,
                                   Fail_Reason      = :Fail_Reason,
                                   Status           = 4,
                                   Last_Update_Date = Sysdate,
                                   Last_Update_By   = :User_Id1
                             Where Afr_Id = :Afr_Id";
            OracleParameter[] sp1 = { data.MakeInParam(":User_Name", param.USER_NAME??""),
                                      data.MakeInParam(":Fail_Reason", param.BACKREASON??""),
                                      data.MakeInParam(":User_Id1", param.USER_ID),
                                      data.MakeInParam(":Afr_Id", param.AFR_ID) };
            Hashtable ht = new Hashtable();
            ht.Add(sql, sp);
            ht.Add(sql1, sp1);
            bool flag = await data.DoSqlList(ht);
            string msg = "";
            if (flag)
            {
                //发送短信
                msg = await SendMsg(msg, "step3", param.AFR_ID);
                return Json(new { code = 200, msg = "审核成功，"+msg });
            }
            else
            {
                return Json(new { code = 300, msg = "审核失败，请联系管理员" });
            }            
        }
        #endregion

        #region 市医保局审核：退回到票据材料接收
        [HttpPost]
        [Route("SybjBackPjjs_ydjybx")]
        public async Task<IActionResult> SybjBackPjjs_ydjybx(Sgybb_param param)
        {
            if (param.AFR_ID == null || param.AFR_ID == "")
            {
                return Json(new { code = 300, msg = "请传入主键" });
            }
            var n = await data.GetStringByParam(@"select count(*) from ANOTHERPLACEAFR where AFR_ID=:afr_id", new OracleParameter[] { data.MakeInParam(":afr_id", param.AFR_ID) });
            if (n == "0")
            {
                return Json(new { code = 300, msg = "传入的主键找不到相应单据" });
            }
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
                                       Person_Id,phone)
                                      Select Anotherplaceafr_s.Nextval,
                                             Flow_No,
                                             Deal_With_Number + 1,
                                             '票据材料接收',
                                             Id_Card_Number,
                                             Person_Name,
                                             1,
                                             null,
                                             sysdate,
                                             :user_id,
                                             Person_Id,phone
                                        From Anotherplaceafr
                                       Where Afr_Id = :Afr_Id";
            OracleParameter[] sp = { data.MakeInParam(":user_id", param.USER_ID), data.MakeInParam(":Afr_Id", param.AFR_ID) };
            string sql1 = @"Update Anotherplaceafr
                               Set Tran_Date        = Sysdate,
                                   Tran_Person     =
                                   (Select User_Name From App_User Where User_Id = :User_Id),
                                   Fail_Reason      = :Fail_Reason,
                                   Status           = 8,
                                   Last_Update_Date = Sysdate,
                                   Last_Update_By   = :User_Id1
                             Where Afr_Id = :Afr_Id";
            OracleParameter[] sp1 = { data.MakeInParam(":User_Id", param.USER_ID),
                                      data.MakeInParam(":Fail_Reason", param.BACKREASON??""),
                                      data.MakeInParam(":User_Id1", param.USER_ID),
                                      data.MakeInParam(":Afr_Id", param.AFR_ID) };
            Hashtable ht = new Hashtable();
            ht.Add(sql, sp);
            ht.Add(sql1, sp1);
            bool flag = await data.DoSqlList(ht);
            string msg = "";
            if (flag)
            {
                //发送短信
                msg = await SendMsg(msg, "fail_msg", param.AFR_ID);
                return Json(new { code = 200, msg = "退回成功，" + msg });
            }
            else
            {
                return Json(new { code = 300, msg = "退回失败，请联系管理员" });
            }
        }
        #endregion

        #region 市医保局审核：复核通过并转交到财务申请支付
        [HttpPost]
        [Route("CommitToPay_ydjybx")]
        public async Task<IActionResult> CommitToPay_ydjybx(Sgybb_param param)
        {
            if (param.AFR_ID == null || param.AFR_ID == "")
            {
                return Json(new { code = 300, msg = "请传入主键" });
            }
            var n = await data.GetStringByParam(@"select count(*) from ANOTHERPLACEAFR where AFR_ID=:afr_id", new OracleParameter[] { data.MakeInParam(":afr_id", param.AFR_ID) });
            if (n == "0")
            {
                return Json(new { code = 300, msg = "传入的主键找不到相应单据" });
            }
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
                                       Person_Id,phone)
                                      Select Anotherplaceafr_s.Nextval,
                                             Flow_No,
                                             Deal_With_Number + 1,
                                             '申请支付',
                                             Id_Card_Number,
                                             Person_Name,
                                             9,
                                             to_char(sysdate,'yyyy-MM-dd hh24:mi:ss'),
                                             sysdate,
                                             :user_id,
                                             Person_Id,phone
                                        From Anotherplaceafr
                                       Where Afr_Id = :Afr_Id";
            OracleParameter[] sp = { data.MakeInParam(":user_id", param.USER_ID), data.MakeInParam(":Afr_Id", param.AFR_ID) };
            string sql1 = @"Update Anotherplaceafr
                               Set Tran_Date        = Sysdate,
                                   Tran_Person     = :User_Name,
                                   Fail_Reason      = :Fail_Reason,
                                   Status           = 7,
                                   Last_Update_Date = Sysdate,
                                   Last_Update_By   = :User_Id1
                             Where Afr_Id = :Afr_Id";
            OracleParameter[] sp1 = { data.MakeInParam(":User_Name", param.USER_NAME??""),
                                      data.MakeInParam(":Fail_Reason", param.BACKREASON??""),
                                      data.MakeInParam(":User_Id1", param.USER_ID),
                                      data.MakeInParam(":Afr_Id", param.AFR_ID) };
            Hashtable ht = new Hashtable();
            ht.Add(sql, sp);
            ht.Add(sql1, sp1);
            bool flag = await data.DoSqlList(ht);
            string msg = "";
            if (flag)
            {
                //发送短信
                //msg = await SendMsg(msg, "step3", param.AFR_ID);
                return Json(new { code = 200, msg = "审核成功"  });
            }
            else
            {
                return Json(new { code = 300, msg = "审核失败，请联系管理员" });
            }            
        }
        #endregion

        #region 申请支付：发起申请支付
        [HttpPost]
        [Route("CommitPay_ydjybx")]
        public async Task<IActionResult> CommitPay_ydjybx(Sgybb_param param)
        {
            if (param.AFR_ID == null || param.AFR_ID == "")
            {
                return Json(new { code = 300, msg = "请传入主键" });
            }
            var n = await data.GetStringByParam(@"select count(*) from ANOTHERPLACEAFR where AFR_ID=:afr_id", new OracleParameter[] { data.MakeInParam(":afr_id", param.AFR_ID) });
            if (n == "0")
            {
                return Json(new { code = 300, msg = "传入的主键找不到相应单据" });
            }
            string sql = @"Update Anotherplaceafr
                               Set Tran_Date        = Sysdate,
                                   Tran_Person     = :User_Name,
                                   Fail_Reason      = :Fail_Reason,
                                   PAY_AMT          = :Pay_Amt,
                                   Status           = 10,
                                   Last_Update_Date = Sysdate,
                                   Last_Update_By   = :User_Id1
                             Where Afr_Id = :Afr_Id";
            OracleParameter[] sp = { data.MakeInParam(":User_Name", param.USER_NAME??""),
                                      data.MakeInParam(":Fail_Reason", param.BACKREASON??""),
                                      data.MakeInParam(":Pay_Amt", param.PAY_AMT??""),
                                      data.MakeInParam(":User_Id1", param.USER_ID),
                                      data.MakeInParam(":Afr_Id", param.AFR_ID) };
            bool flag = await data.DoSqlByParam(sql,sp);
            string msg = "";
            if (flag)
            {
                //发送短信
                msg = await SendMsg(msg, "step4", param.AFR_ID);
                return Json(new { code = 200, msg = "发起成功，" + msg });
            }
            else
            {
                return Json(new { code = 300, msg = "发起失败，请联系管理员" });
            }
        }
        #endregion

        #region 退休人员总查询界面
        [HttpGet]
        [Route("GetProcess")]
        public async Task<IActionResult> GetProcess(Query_param param)
        {
            string pj = param.STATUS == "未完成" ? " Where Status = '未完成' " : " Where Status = '完成'";
            string sql = @"SELECT *
                           FROM(SELECT ROWNUM AS rowno, r.*
                                   FROM( Select Flow_No,
                                                Person_Name,
                                                Id_Card_Number,
                                                Status,
                                                To_Char(Ceil(Count(*) Over() / :limit1)) Totalpage
                                           From (Select Distinct t.Flow_No,
                                                                 t.Person_Name,
                                                                 t.Id_Card_Number,
                                                                 (Case
                                                                   When (Select fr.Status
                                                                           From Anotherplaceafr fr
                                                                          Where fr.Flow_Type = '申请支付'
                                                                            And fr.Flow_No = t.Flow_No) = '10' Then
                                                                    '完成'
                                                                   Else
                                                                    '未完成'
                                                                 End) Status
                                                   From Anotherplaceafr t
                                                  Where t.Person_Id = :person_id order by flow_no asc)
                                                  " + pj + @" ) r
                                  where ROWNUM <= :page * :limit) table_alias
                          WHERE table_alias.rowno > (: page - 1) * :limit ";
            try
            {
                OracleParameter[] p = { data.MakeInParam(":limit1", param.LIMIT),
                                        data.MakeInParam(":person_id", param.PERSON_ID),
                                        data.MakeInParam(":page", param.PAGE),
                                        data.MakeInParam(":limit",param.LIMIT)};
                DataSet ds = await data.GetDataSetByParam(sql, p);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    return Json(new { code = 0, msg = "已查询到数据", count = ds.Tables[0].Rows[0]["Totalpage"], data = ds.Tables[0] });
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

        #region 退休人员单个流程查询
        [HttpGet]
        [Route("GetAllProcess")]
        public async Task<IActionResult> GetAllProcess(Query_param param)
        {
            if (param.FLOW_NO==null||param.FLOW_NO=="")
            {
                return Json(new
                {
                    code = 1,
                    msg = "请传入流程号",
                    count = 0,
                    data =
                    new { }
                });
            }
            string sql = @"Select To_Char(Afr_Id) Afr_Id,
                               Flow_No,
                               To_Char(Deal_With_Number) Deal_With_Number,
                               Flow_Type,
                               Id_Card_Number,
                               Person_Name,
                               To_Char(Tran_Date, 'yyyy-MM-dd hh24:mi:ss') Tran_Date,
                               Tran_Person,
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
                               Attribute2,
                               To_Char(Person_Id) Person_Id,to_char(pay_amt) pay_amt
                          From Anotherplaceafr
                         Where Flow_No = :flow_no
                         Order By Deal_With_Number desc";
            try
            {
                OracleParameter[] p = { data.MakeInParam(":flow_no", param.FLOW_NO) };
                DataSet ds = await data.GetDataSetByParam(sql, p);
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

        #region 发送短信2
        private async Task<string> SendMsg2(string msg, string afr_id)
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
            var content = "";
            if (int.Parse(day) >= 20)
            {
                //25个工作日
                content = await data.GetString(@"Select f.Fixvalue_Name
                                              From App_Fixvalue f, App_Fixvalue_Type Ft
                                             Where f.Fixvalue_Type_Id = Ft.Fixvalue_Type_Id
                                               And Ft.Fixvalue_Type_Code = 'ydjybx'
                                               And f.Fixvalue_Code = 'step1'");
            }
            else if (int.Parse(day) < 20)
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
    }
}