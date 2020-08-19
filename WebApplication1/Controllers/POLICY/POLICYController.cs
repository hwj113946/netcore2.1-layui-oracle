using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace WebApplication1.Controllers.POLICY
{
    [EnableCors("AllowSpecificOrigin")]//跨域
    public class POLICYController : Controller
    {
        DBHelper data = new DBHelper();
        private readonly IHostingEnvironment _hostingEnvironment;
        public POLICYController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        
        [CheckCustomer]
        public IActionResult POLICY()
        {
            return View();
        }

        [CheckCustomer]
        public IActionResult PolicyFileUpload()
        {
            return View();
        }

        #region 获取政策
        [HttpGet]
        public async Task<IActionResult> GetPolicy(string search_text, int page, int limit)
        {
            string Msg = "";
            string sql = @"SELECT *
                      FROM(SELECT ROWNUM AS rowno, r.*
                              FROM(select (Select Count(*) From Policy p, App_User u
                                    Where p.Creation_By = u.User_Id(+)
                                      And p.Title Like '%' || :Title || '%') totalPage,
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
                                     data.MakeInParam(":Title", search_text ?? ""),
                                     data.MakeInParam(":Title1", search_text ?? ""),
                                     data.MakeInParam(":page", page),
                                    data.MakeInParam(":limit", limit)};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows[0]["totalPage"] + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion
        
        #region 上传文件
        public async Task<IActionResult> UploadFile()
        {
            //var date = Request;
            try
            {
                var files = HttpContext.Request.Form.Files;// Request.Form.Files;
                long size = files.Sum(f => f.Length);
                string shortTime = DateTime.Now.ToString("yyyyMMdd") + "/";
                string filePhysicalPath = Startup.HostingEnvironment.WebRootPath.Replace("\\", "/") + "/PolicyFiles/" + shortTime; //文件路径  可以通过注入 IHostingEnvironment 服务对象来取得Web根目录和内容根目录的物理路径
                if (!Directory.Exists(filePhysicalPath)) //判断上传文件夹是否存在，若不存在，则创建
                {
                    Directory.CreateDirectory(filePhysicalPath); //创建文件夹
                }
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var title = Path.GetFileName(file.FileName);
                        var fileName = file.FileName;//Path.GetExtension(file.FileName);//文件名+文件后缀名
                        var file_link = "https://sgtxryxx.sgis.com.cn/PolicyFiles/" + shortTime + file.FileName;
                        using (var stream = new FileStream(filePhysicalPath + fileName, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                            string sql = @"Insert Into Policy
                                      (Policy_Id, Title, Upload_Time, File_Link, Creation_Date, Creation_By)
                                    Values
                                      (Policy_s.Nextval, :Title, Sysdate, :File_Link, Sysdate, :User_Id)";
                            OracleParameter[] sp = {
                            data.MakeInParam(":Title",title??""),
                            data.MakeInParam(":File_Link",file_link),
                            data.MakeInParam(":User_Id",HttpContext.Session.GetString("USER_ID"))
                        };
                            await data.DoSqlByParam(sql, sp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { code = 1, msg = "上传失败,"+ex.Message, data = new { } });
            }
            return Json(new { code = 0, msg = "上传成功", data = new { } });
        } 
        #endregion

    }
}