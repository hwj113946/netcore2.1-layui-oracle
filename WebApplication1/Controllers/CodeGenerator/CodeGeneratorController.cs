using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using WebApplication1.Models;

namespace WebApplication1.Controllers.CodeGenerator
{
    public class CodeGeneratorController : Controller
    {
        DBHelper data = new DBHelper();
        [CheckCustomer]
        public IActionResult CodeGenerator()
        {
            return View();
        }

        #region 生成代码
        [HttpPost]
        public async Task<IActionResult> Generator(string TableName, string ControllerName,string ApiControllerName, string ModelName, string ViewName, string EditViewName)
        {
            try
            {
                var sql = @"Select b.Column_Name    As 字段名称,
                                               b.Data_Type      As 数据类型,
                                               to_char(b.Data_Length)    As 字段长度,
                                               to_char(b.Data_Precision) As 长度,
                                               to_char(b.Data_Scale)     As 小数位,
                                               c.Comments       As 注释
                                          From User_Tables a, User_Tab_Cols b, User_Col_Comments c
                                         Where a.Table_Name = b.Table_Name
                                           And b.Column_Name = c.Column_Name
                                           And b.Table_Name = c.Table_Name
                                           And a.Table_Name =:TableName  Order By b.Column_Id Asc";
                DataSet ds = await data.GetDataSetByParam(sql, new OracleParameter[] { data.MakeInParam(":TableName", TableName ?? "") });
                var pk = await data.GetStringByParam(@"Select Column_Name From User_Cons_Columns
                        Where Constraint_Name = (Select Constraint_Name  From User_Constraints Where Table_Name = :TableName And Constraint_Type = 'P')", new OracleParameter[] { data.MakeInParam(":TableName", TableName ?? "") });
                await Model(ds, ModelName);
                await View(ds, TableName, ControllerName, ViewName, EditViewName, pk);
                await EditView(ds, EditViewName, pk, ControllerName);
            }
            catch (Exception ex)
            {
                return Json(new { code = 300, msg = "执行失败。" });
            }
            return Json(new { code = 200, msg = "执行成功，请注意查看。" });
        }
        #endregion

        #region 读取模板内容
        /// <summary>
        /// 读取模板内容
        /// </summary>
        /// <param name="fileName">模板文件名称</param>
        /// <returns></returns>
        private async Task<string> ReadTemp(string fileName)
        {
            string filepath = Startup.HostingEnvironment.WebRootPath + "\\CodeTemplate\\" + fileName;
            FileStream fileStream = new FileStream(filepath, FileMode.Open);
            var line = "";
            using (StreamReader reader = new StreamReader(fileStream))
            {
                line = await reader.ReadToEndAsync();
            }
            return line;
        }
        #endregion

        #region 写入保存
        /// <summary>
        /// 写入保存
        /// </summary>
        /// <param name="fileName">路径+名称</param>
        /// <param name="content">内容</param>
        private void WriteAndSave(string fileName, string content)
        {
            //实例化一个文件流--->与写入文件相关联
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                //实例化一个StreamWriter-->与fs相关联
                using (var sw = new StreamWriter(fs))
                {
                    //开始写入
                    sw.Write(content);
                    //清空缓冲区
                    sw.Flush();
                    //关闭流
                    sw.Close();
                    fs.Close();
                }
            }
        }
        #endregion

        #region 生成Model
        private async Task<bool> Model(DataSet ds, string ModelName)
        {
            try
            {
                var sb = new StringBuilder();
                #region 遍历列
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    var str = "";
                    switch (ds.Tables[0].Rows[i]["数据类型"].ToString())
                    {
                        case "BFILE": str = "byte[]"; break;
                        case "BLOB": str = "byte[]"; break;
                        case "CHAR": str = "string"; break;
                        case "CLOB": str = "string"; break;
                        case "DATE": str = "DateTime?"; break;
                        case "FLOAT": str = "decimal?"; break;
                        case "INTEGER": str = "decimal?"; break;
                        case "INTERVAL YEAR TO  MONTH": str = "Int32"; break;
                        case "INTERVAL DAY TO  SECOND": str = "TimeSpan"; break;
                        case "LONG": str = "string"; break;
                        case "LONG RAW": str = "byte[]"; break;
                        case "NCHAR": str = "string"; break;
                        case "NCLOB": str = "string"; break;
                        case "NUMBER":
                            if (ds.Tables[0].Rows[i]["长度"].ToString() == "")
                            {
                                str = "decimal?";
                            }
                            else
                            {
                                if (int.Parse(ds.Tables[0].Rows[i]["长度"].ToString()==""?"0": ds.Tables[0].Rows[i]["长度"].ToString()) <= 4)
                                {
                                    str = "Int32";
                                }
                                else if (int.Parse(ds.Tables[0].Rows[i]["长度"].ToString() == "" ? "0" : ds.Tables[0].Rows[i]["长度"].ToString()) >= 5 && int.Parse(ds.Tables[0].Rows[i]["长度"].ToString() == "" ? "0" : ds.Tables[0].Rows[i]["长度"].ToString()) <= 9)
                                {
                                    str = "Int32";
                                }
                                else if (int.Parse(ds.Tables[0].Rows[i]["长度"].ToString() == "" ? "0" : ds.Tables[0].Rows[i]["长度"].ToString()) >= 10 && int.Parse(ds.Tables[0].Rows[i]["长度"].ToString() == "" ? "0" : ds.Tables[0].Rows[i]["长度"].ToString()) <= 19)
                                {
                                    str = "long?";
                                }
                                else if (int.Parse(ds.Tables[0].Rows[i]["长度"].ToString() == "" ? "0" : ds.Tables[0].Rows[i]["长度"].ToString()) >= 20)
                                {
                                    str = "decimal?";
                                }
                            }

                            break;
                        case "NVARCHAR2": str = "string"; break;
                        case "RAW": str = "byte[]"; break;
                        case "ROWID": str = "byte[]"; break;
                        case "TIMESTAMP": str = "DateTime?"; break;
                        case "VARCHAR2": str = "string"; break;
                    }
                    sb.Append("        [Column(" + i + ")] public " + str + " " + ds.Tables[0].Rows[i]["字段名称"] + " {get;set;} //" + ds.Tables[0].Rows[i]["注释"] + "\r\n");
                }
                #endregion
                var content = await ReadTemp("ModelTemp.txt");
                content = content.Replace("{ModelName}", ModelName).Replace("{ColName}", sb.ToString());
                WriteAndSave(Startup.HostingEnvironment.WebRootPath + "\\Model\\" + ModelName + ".cs", content);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region 生成主界面
        private async Task<bool> View(DataSet ds,string TableName,string ControllerName, string ViewName,string EditViewName,string pk)
        {
            try
            {
                var sb = new StringBuilder();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (ds.Tables[0].Rows[i]["字段名称"].ToString() == pk)
                    {
                        sb.Append("\r\n                    {field:'" + ds.Tables[0].Rows[i]["字段名称"] + "', title:'" + ds.Tables[0].Rows[i]["注释"] + "',hide: true },");
                    }
                    else
                    {
                        sb.Append("\r\n                    {field:'" + ds.Tables[0].Rows[i]["字段名称"] + "', title:'" + ds.Tables[0].Rows[i]["注释"] + "',sort: true ,align : 'center' },");
                    }
                }
                var content = await ReadTemp("ViewTemp.txt");
                content = content.Replace("{TableName}", TableName)
                    .Replace("{ControllerName}", ControllerName)
                    .Replace("{ColName}", sb.ToString().Remove(sb.ToString().Length - 1, 1))
                    .Replace("{EditViewName}", EditViewName)
                    .Replace("{PK}", pk);
                WriteAndSave(Startup.HostingEnvironment.WebRootPath + "\\View\\" + ViewName + ".cshtml",content);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region 生成编辑界面
        private async Task<bool> EditView(DataSet ds, string EditViewName,string pk,string ControllerName)
        {
            try
            {
                #region 遍历列
                var table_colum = "";
                var item_head = "        <div class=\"layui-form-item\">\r\n            <div class=\"layui-inline\">\r\n";
                var item_end = "            </div>\r\n        </div>\r\n";
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (i == 0)
                    {
                        if (ds.Tables[0].Rows[i]["字段名称"].ToString() == pk)
                        {

                        }
                    }
                    else
                    {
                        if (i % 2 == 1)
                        {
                            table_colum += item_head + "                <label class=\"layui-form-label\">" + ds.Tables[0].Rows[i]["注释"] + "</label>\r\n                <div class=\"layui-input-inline\">\r\n                    <input type=\"text\" class=\"layui-input\" id=\"" + ds.Tables[0].Rows[i]["字段名称"].ToString().ToLower() + "\" placeholder=\"" + ds.Tables[0].Rows[i]["注释"] + "\">\r\n                </div>\r\n";
                            if ((ds.Tables[0].Rows.Count - 1) == i)
                            {
                                table_colum = table_colum + item_end;
                            }
                        }
                        else
                        {
                            table_colum += "                <label class=\"layui-form-label\">" + ds.Tables[0].Rows[i]["注释"] + "</label>\r\n                <div class=\"layui-input-inline\">\r\n                    <input type=\"text\" class=\"layui-input\" id=\"" + ds.Tables[0].Rows[i]["字段名称"].ToString().ToLower() + "\" placeholder=\"" + ds.Tables[0].Rows[i]["注释"] + "\">\r\n                </div>\r\n" + item_end;
                        }
                    }
                }
                #endregion

                #region 参数
                var table_col = new StringBuilder();
                var data_param = new StringBuilder();
                var data_param_ready = new StringBuilder();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    table_col .Append("\r\n                var " + ds.Tables[0].Rows[i]["字段名称"].ToString().ToLower() + " = $(\"#" + ds.Tables[0].Rows[i]["字段名称"].ToString().ToLower() + "\").val();");
                    if (ds.Tables[0].Rows[i]["字段名称"].ToString() == pk)
                    {
                        data_param .Append("\r\n                        " + ds.Tables[0].Rows[i]["字段名称"].ToString().ToLower() + ":'@ViewBag." + ds.Tables[0].Rows[i]["字段名称"].ToString().ToLower() + "',");
                    }
                    else
                    {
                        data_param .Append("\r\n                        " + ds.Tables[0].Rows[i]["字段名称"].ToString().ToLower() + ":" + ds.Tables[0].Rows[i]["字段名称"].ToString().ToLower() + " ,");
                        data_param_ready .Append("\r\n                        $(\"#" + ds.Tables[0].Rows[i]["字段名称"].ToString().ToLower() + "\").val(res.data[0]." + ds.Tables[0].Rows[i]["字段名称"].ToString().ToUpper() + ");");
                    }
                }
                #endregion

                var content = await ReadTemp("EditViewTemp.txt");
                content = content.Replace("{ColName}", table_colum)
                    .Replace("{ControllerName}", ControllerName)
                    .Replace("{table_col}", table_col.ToString().Remove(table_col.ToString().Length - 1, 1))
                    .Replace("{data_param}", data_param.ToString().Remove(data_param.ToString().Length-1,1))
                    .Replace("{data_param_ready}", data_param_ready.ToString())
                    .Replace("{PK}", pk.ToLower());
                WriteAndSave(Startup.HostingEnvironment.WebRootPath + "\\View\\" + EditViewName + ".cshtml", content);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        } 
        #endregion
    }
}