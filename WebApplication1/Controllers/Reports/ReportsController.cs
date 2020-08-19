using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Common;
using FastReport.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace WebApplication1.Controllers.Reports
{
    public class ReportsController : Controller
    {
        DBHelper data = new DBHelper();
        private readonly IHostingEnvironment _hostingEnvironment;
        public ReportsController( IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        
        [CheckCustomer]
        public IActionResult WorkFlow_Appr_Rep()
        {
            var report = new WebReport();
            report.Report.Load(_hostingEnvironment.WebRootPath + "/ReportTemp/Workflow_Appr_Rep.frx");
            #region sql
            string sql = @"Select Decode(a.Appr_Flow_Id,
              -99,
              '',
              (Select Nvl(Attribute1, Dept_Name)
                 From App_Dept
                Where Dept_Id = a.Appl_Dept_Id)) Dept,
       To_Char(Appl_Date, 'yyyy-mm-dd HH24:mi') As Appldate,
       (Select Person_Name
          From App_Person
         Where Person_Id = a.Appl_Person_Id) Person,
       Decode(a.Appr_Flow_Id, -99, '', a.Doc_Note) Note,
       a.Doc_Title Title
  From App_Workflow_Appr a
 Where a.Appr_Id = :Appr_Id";
            
            string sql2 = @"Select Rownum Rn,
       Wat.Appr_Tran_Id,
       Decode(Wat.Appr_Dept_Id,
              '',
              Attribute9,
              (Select Nvl(Attribute1, Dept_Name)
                 From App_Dept
                Where Dept_Id = Wat.Appr_Dept_Id)) Appl_Dept,
       Wat.Appr_Person_Id,
       Decode(Appr_Person_Id,
              '',
              Attribute8,
              (Select Person_Name
                 From App_Person
                Where Person_Id = Wat.Appr_Person_Id)) Appr_Person,
       Wat.Appr_Post_Name,
       To_Char(Wat.Appr_Date, 'yyyy-mm-dd hh24:mi:ss') Appr_Date,
       Decode(Wat.Status,
              '0',
              '发起',
              '1',
              '通过',
              '2',
              '流程结束',
              '3',
              '退回',
              '4',
              '收回') || '：' || Wat.Appr_Note Yj,
       Wat.Appr_Note,
       Wat.Status,
       Decode(Wat.Status,
              '0',
              '发起',
              '1',
              '通过',
              '2',
              '流程结束',
              '3',
              '退回',
              '4',
              '收回') St,
       Wat.Attribute1,
       Wat.Attribute2,
       Wat.Attribute3,
       Wat.Attribute4,
       Wat.Attribute5,
       Wat.Attribute6,
       Wat.Attribute7
  From App_Workflow_Appr_Tran Wat
 Where Wat.Appr_Id = :Appr_Id
 Order By Wat.Appr_Date";
            #endregion
            DataSet ds = data.GetDataSetByParamAsNoAsync(sql,new OracleParameter[] 
            {
                data.MakeInParam(":Appr_Id",HttpContext.Request.Query["Rowid"].ToString()??"")
            });
            DataSet ds2 = data.GetDataSetByParamAsNoAsync(sql2, new OracleParameter[]
            {
                data.MakeInParam(":Appr_Id",HttpContext.Request.Query["Rowid"].ToString()??"")
            });
            DataTable dt1 = ds.Tables[0].Copy();
            dt1.TableName = "Appr";
            //report.Report.RegisterData(dt1, "TABLE");
            DataTable dt2 = ds2.Tables[0].Copy();
            dt2.TableName = "Table";
            //report.Report.RegisterData(dt2, "TABLES");
            DataSet source = new DataSet();
            //source.Tables.AddRange(new DataTable[] { dt1, dt2 });
            source.Tables.Add(dt1);
            source.Tables.Add(dt2);
            source.Tables[0].TableName = "Appr";
            source.Tables[1].TableName = "Table";
            report.Report.RegisterData(source);//注册数据
            ViewBag.WebReport = report;
           
            return View();
        }
    }
}