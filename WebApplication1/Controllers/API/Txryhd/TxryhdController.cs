using System.Data;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using WebApplication1.Models;

namespace WebApplication1.Controllers.API.Txryhd
{
    [Route("API/Txryhd")]
    [Produces("application/json")]
    [EnableCors("AllowSpecificOrigin")]//跨域
    public class TxryhdController : Controller
    {
        DBHelper data = new DBHelper();

        #region 登录
        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login(Txry_Person person)
        {
            #region sql
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
 from txry_person t Where t.person_name=:person_name And t.id_card_number=:id_card_number"; 
            #endregion
            OracleParameter[] p = {
                data.MakeInParam(":person_name",person.PERSON_NAME??""),
                data.MakeInParam(":id_card_number",person.ID_CARD_NUMBER??"") };
            try
            {
                DataSet ds = await data.GetDataSetByParam(sql, p);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    //if (ds.Tables[0].Rows[0]["STATUS"].ToString() !="2")
                    //{
                        string json = "[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]";
                        return Json(new { code = 0, msg = "已查询到数据" ,count=ds.Tables[0].Rows.Count,data= json.ToJson()  });
                    //}
                    //else
                    //{
                    //    return Json(new { code = 2, msg = "信息已通过复核，无法再进行修改" });
                    //}
                }
                else
                {
                    return Json(new { code=1,msg= "检索不到你的身份信息，请与员工服务中心联系，联系电话：8798509、8788390。" });
                }
            }
            catch (System.Exception ex)
            {
                return Json(new { code=1,msg=ex.Message});
            }
        }
        #endregion

        #region 本人核验
        [Route("Modify")]
        [HttpPost]
        public async Task<IActionResult> Modify(Txry_Person person)
        {
            string sql = @"Update Txry_Person
                               Set Person_Name                 = :person_name,
                                   Sex                         = :sex,
                                   Age                         = :age,
                                   Phone                       = :phone,
                                   Id_Card_Number              = :id_card_number,
                                   National                    = :national,
                                   Political_Landscape         = :political_landscape,
                                   Long_Term_Residence         = :long_term_residence,
                                   Domicile_Place              = :domicile_place,
                                   Special_Person              = :special_person,
                                   Health                      = :health,
                                   e_i_Address                 = :e_i_address,
                                   Medical_i_Address           = :medical_i_address,
                                   Is_Gsbx                     = :is_gsbx,
                                   Living_Situation            = :living_situation,
                                   Spouse_Name                 = :spouse_name,
                                   Spouse_Health               = :spouse_health,
                                   Spouse_Phone                = :spouse_phone,
                                   Family_Major_Person_Name    = :family_major_person_name,
                                   Family_Major_p_Relationship = :family_major_p_relationship,
                                   Family_Major_Person_Address = :family_major_person_address,
                                   Family_Major_Person_Phone   = :family_major_person_phone,
                                   Registered_Image_First      = :registered_image_first,
                                   Registered_Image_Self       = :registered_image_self,
                                   Status                      =  1,
                                   Emergency_Person            = :emergency_person,
                                   Emergency_Phone             = :emergency_phone,
                                   Emergency_Address           = :emergency_address,
                                   Transfer_Type               = :transfer_type
                             Where Person_Id = :person_id";
            OracleParameter[] sp = {
                data.MakeInParam(":person_name",person.PERSON_NAME??""),
                data.MakeInParam(":sex",person.SEX??""),
                data.MakeInParam(":age",person.AGE??""),
                data.MakeInParam(":phone",person.PHONE??""),
                data.MakeInParam(":id_card_number",person.ID_CARD_NUMBER??""),
                data.MakeInParam(":national",person.NATIONAL??""),
                data.MakeInParam(":political_landscape",person.POLITICAL_LANDSCAPE??""),
                data.MakeInParam(":long_term_residence",person.LONG_TERM_RESIDENCE??""),
                data.MakeInParam(":domicile_place",person.DOMICILE_PLACE??""),
                data.MakeInParam(":special_person",person.SPECIAL_PERSON??""),
                data.MakeInParam(":health",person.HEALTH??""),
                data.MakeInParam(":e_i_address",person.E_I_ADDRESS??""),
                data.MakeInParam(":medical_i_address",person.MEDICAL_I_ADDRESS??""),
                data.MakeInParam(":is_gsbx",person.IS_GSBX??""),
                data.MakeInParam(":living_situation",person.LIVING_SITUATION??""),
                data.MakeInParam(":spouse_name",person.SPOUSE_NAME??""),
                data.MakeInParam(":spouse_health",person.SPOUSE_HEALTH??""),
                data.MakeInParam(":spouse_phone",person.SPOUSE_PHONE??""),
                data.MakeInParam(":family_major_person_name",person.FAMILY_MAJOR_PERSON_NAME??""),
                data.MakeInParam(":family_major_p_relationship",person.FAMILY_MAJOR_P_RELATIONSHIP??""),
                data.MakeInParam(":family_major_person_address",person.FAMILY_MAJOR_PERSON_ADDRESS??""),
                data.MakeInParam(":family_major_person_phone",person.FAMILY_MAJOR_PERSON_PHONE??""),
                data.MakeInParam(":registered_image_first",person.REGISTERED_IMAGE_FIRST??""),
                data.MakeInParam(":registered_image_self",person.REGISTERED_IMAGE_SELF??""),
                data.MakeInParam(":emergency_person",person.EMERGENCY_PERSON??""),
                data.MakeInParam(":emergency_phone",person.EMERGENCY_PHONE??""),
                data.MakeInParam(":emergency_address",person.EMERGENCY_ADDRESS??""),
                data.MakeInParam(":transfer_type",person.TRANSFER_TYPE??""),
                data.MakeInParam(":person_id",person.PERSON_ID??"")};
            bool flag = await data.DoSqlByParam(sql, sp);
            //var json = flag == true ? new { code = 0, msg = "核验成功" } : new { code = 1, msg = "核验失败，请联系管理员" };
            return Json(flag == true ? new { code = 0, msg = "核验成功" } : new { code = 1, msg = "核验失败，请联系管理员" });
        } 
        #endregion
    }
}