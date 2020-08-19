using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace WebApplication1.Controllers.Post
{
    public class PostsController : Controller
    {
        DBHelper data = new DBHelper();

        [CheckCustomer]
        public IActionResult Posts()
        {
            ViewBag.CORP_ID = HttpContext.Session.GetString("CORP_ID");
            ViewBag.DEPT_ID = HttpContext.Session.GetString("DEPT_ID");
            return View();
        }

        [CheckCustomer]
        public IActionResult EditPost()
        {
            ViewBag.status = HttpContext.Request.Query["status"];
            ViewBag.post_id = ViewBag.status == "add" ? "" : HttpContext.Request.Query["Rowid"].ToString();
            return View();
        }

        #region 根据部门获取岗位
        [HttpPost]
        public async Task<IActionResult> GetPostByDept(int limit, int page, string post_name, string dept_id, string status)
        {
            dept_id = dept_id ?? HttpContext.Session.GetString("DEPT_ID");
            string msg = "";
            string sql = @"SELECT *
                  FROM (SELECT ROWNUM AS rowno, r.*
                          FROM (select t.Post_Id,
                               t.Post_Code,
                               t.Post_Name,
                               t.Parent_Post_Id,
                               Decode(t.Status, 0, '有效', 1, '无效') Status,
                               Ps.Post_Code parent_post_code,
                               Ps.Post_Name parent_post_name,
                               d.Dept_Id,
                               d.Dept_Code,
                               d.Dept_Name,
                               c.Corp_Id,
                               c.Corp_Code,
                               c.Corp_Name
                         From App_Posts t, App_Posts Ps, App_Dept d, App_Corp c
                        Where t.Parent_Post_Id = Ps.Post_Id(+)
                          And t.Dept_Id = d.Dept_Id
                          And d.Corp_Id = c.Corp_Id
                          And (t.post_code Like '%'|| :post_code ||'%' Or t.post_name Like '%'|| :post_name ||'%')
                          And t.status=:status
                          And t.dept_id=:dept_id) r
                 where ROWNUM <= :page * :limit) table_alias
             WHERE table_alias.rowno > (:page - 1) * :limit";
            string sql1 = @"select count(*)
                         From App_Posts t, App_Posts Ps, App_Dept d, App_Corp c
                        Where t.Parent_Post_Id = Ps.Post_Id(+)
                          And t.Dept_Id = d.Dept_Id
                          And d.Corp_Id = c.Corp_Id
                          And (t.post_code Like '%'|| :post_code ||'%' Or t.post_name Like '%'|| :post_name ||'%')
                          And t.status=:status
                          And t.dept_id=:dept_id";
            OracleParameter[] sp1 = {
                                    data.MakeInParam(":post_code",post_name??""),
                                    data.MakeInParam(":post_name",post_name??""),
                                    data.MakeInParam(":status",status),
                                    data.MakeInParam(":dept_id",dept_id??"")};
            string n = await data.GetStringByParam(sql1,sp1);
            OracleParameter[] sp = {
                                    data.MakeInParam(":post_code",post_name??""),
                                    data.MakeInParam(":post_name",post_name??""),
                                    data.MakeInParam(":status",status),
                                    data.MakeInParam(":dept_id",dept_id??""),
                                    data.MakeInParam(":page",page),data.MakeInParam(":limit",limit)};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + n + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(msg);
        }
        #endregion

        #region 删除岗位
        [HttpPost]
        public async Task<IActionResult> DeletePost(string[] id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"delete from app_posts where Post_id=:post_id"+i,new OracleParameter[] { data.MakeInParam(":post_id"+1,id[i])});
            }
            if (ht.Count>0)
            {
                //string sql = "declare str varchar2(1000);str2 varchar2(3000); begin str:=:Post_id;str2:='delete from app_posts where Post_id in('||str||')'; execute immediate str2; end;";
                //OracleParameter[] sp = { data.MakeInParam(":Post_id", OracleDbType.Varchar2, 3000, id) };
                bool flag = await data.DoSqlList(ht);// data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"删除成功\"}" : "{\"code\":300,\"msg\":\"删除失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中岗位\"}");
            }
           
        }
        #endregion

        #region 启用岗位
        [HttpPost]
        public async Task<IActionResult> EnableStatusForPost(string[] id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"Update App_Posts
                              Set Status           = 0,
                                  Last_Update_Date = Sysdate,
                                  Last_Updated_By  = :Last_Updated_By
                            Where Post_Id = :Post_Id" + i, new OracleParameter[] 
                       {
                           data.MakeInParam(":Last_Updated_By",HttpContext.Session.GetString("USER_ID")),
                           data.MakeInParam(":post_id" + 1, id[i])
                       });
            }
            if (ht.Count>0)
            {
                //string sql = "declare str varchar2(1000);user_id varchar2(100);str2 varchar2(3000); begin str:=:Post_id;user_id:=:user_id;str2:='update app_posts set status=1,last_update_date=sysdate,LAST_UPDATED_BY='||user_id||' where Post_id in('||str||')'; execute immediate str2; end;";
                //OracleParameter[] sp = { data.MakeInParam(":Post_id", OracleDbType.Varchar2, 3000, id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
                bool flag = await data.DoSqlList(ht); //data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"启用成功\"}" : "{\"code\":300,\"msg\":\"启用失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中岗位\"}");
            }
            
        }
        #endregion

        #region 失效岗位
        [HttpPost]
        public async Task<IActionResult> FailureStatusForPost(string id)
        {
            string Msg = "";
            if (id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            Hashtable ht = new Hashtable();
            for (int i = 0; i < id.Length; i++)
            {
                ht.Add(@"Update App_Posts
                              Set Status           = 1,
                                  Last_Update_Date = Sysdate,
                                  Last_Updated_By  = :Last_Updated_By
                            Where Post_Id = :Post_Id" + i, new OracleParameter[]
                       {
                           data.MakeInParam(":Last_Updated_By",HttpContext.Session.GetString("USER_ID")),
                           data.MakeInParam(":post_id" + 1, id[i])
                       });
            }
            if (ht.Count>0)
            {
                //string sql = "declare str varchar2(1000);user_id varchar2(100);str2 varchar2(3000); begin str:=:Post_id;user_id:=:user_id;str2:='update app_posts set status=1,last_update_date=sysdate,LAST_UPDATED_BY='||user_id||' where Post_id in('||str||')'; execute immediate str2; end;";
                //OracleParameter[] sp = { data.MakeInParam(":Post_id", OracleDbType.Varchar2, 3000, id), data.MakeInParam(":user_id", HttpContext.Session.GetString("USER_ID")) };
                bool flag = await data.DoSqlList(ht); //data.DoSqlByParam(sql, sp);
                Msg = flag ? "{\"code\":200,\"msg\":\"失效成功\"}" : "{\"code\":300,\"msg\":\"失效失败,请联系管理员\"}";
                return Content(Msg);
            }
            else
            {
                return Content("{\"code\":300,\"msg\":\"未选中岗位\"}");
            }

        }
        #endregion

        #region 根据岗位ID获取岗位
        [HttpPost]
        public async Task<IActionResult> GetPostByPostId(string post_id)
        {
            string Msg = "";
            if (post_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"Select t.Post_Id,
                                   t.Post_Code,
                                   t.Post_Name,
                                   t.Parent_Post_Id,
                                   Ps.Post_Code parent_post_code,
                                   Ps.Post_Name parent_post_name,
                                   d.Dept_Id,
                                   d.Dept_Code,
                                   d.Dept_Name,
                                   c.Corp_Id,
                                   c.Corp_Code,
                                   c.Corp_Name
                              From App_Posts t, App_Posts Ps, App_Dept d, App_Corp c
                             Where t.Parent_Post_Id = Ps.Post_Id(+)
                               And t.Dept_Id = d.Dept_Id
                               And d.Corp_Id = c.Corp_Id
                               And t.post_id=:post_id";
            OracleParameter[] sp = { data.MakeInParam(":post_id", post_id) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            Msg = ds.Tables[0].Rows.Count > 0 ? ("{\"code\":0,\"msg\":\"已查询到数据\",\"count\":" + ds.Tables[0].Rows.Count + ",\"data\":[" + JsonTools.DataTableToJson(ds.Tables[0]) + "]}") : "{\"code\":1,\"msg\":\"未查询到数据\",\"count\":0,\"data\":[]}";
            return Content(Msg);
        }
        #endregion

        #region 根据部门ID获取岗位树
        [HttpPost]
        public async Task<IActionResult> GetPostByDeptToTree(string dept_id)
        {
            string sql = @"Select t.Post_Id value, Nvl(t.Parent_Post_Id, 0) Parent_Post_Id, t.Post_Name name From App_Posts t Where t.dept_id=:dept_id";
            OracleParameter[] sp = { data.MakeInParam(":dept_id", dept_id) };
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            string json = "";
            try
            {
                var list = DataSetHelper.DataSetToList<Models.PostTree>(ds,0).ToList();
                json = ToMenuJson(list, 0);
                json = "{\"code\":0,\"msg\":\"success\",\"data\":" + json + "}";
            }
            catch (Exception)
            {
                json = "{\"code\":1,\"msg\":\"false\",\"data\":[]}";
                return Content(json);
            }            
            return Content(json);
        }
        #endregion

        #region 转换成json树结构
        private string ToMenuJson(List<Models.PostTree> data, decimal parentId)
        {
            StringBuilder sbJson = new StringBuilder();
            sbJson.Append("[");
            List<Models.PostTree> entitys = data.FindAll(t => t.Parent_Post_Id == parentId);
            if (entitys.Count > 0)
            {
                foreach (var item in entitys)
                {
                    string strJson = item.ToJson();
                    strJson = strJson.Insert(strJson.Length - 1, ",\"children\":" + ToMenuJson(data, item.value) + "");
                    sbJson.Append(strJson + ",");
                }
                sbJson = sbJson.Remove(sbJson.Length - 1, 1);
            }
            sbJson.Append("]");
            return sbJson.ToString();
        }
        #endregion

        #region 新增岗位
        [HttpPost]
        public async Task<IActionResult> Insert(string dept_id, string post_code, string post_name,string parent_post_id)
        {
            string Msg = "";
            string sql = @"Insert Into App_Posts
                                      (Post_Id,
                                       Parent_Post_Id,
                                       Post_Code,
                                       Post_Name,
                                       Dept_Id,
                                       Status,
                                       Creation_Date,
                                       Created_By)
                                    Values
                                      (App_Posts_s.Nextval,
                                       :parent_post_id,
                                       :post_code,
                                       :post_name,
                                       :dept_id,
                                       0,
                                       Sysdate,
                                       :user_id)";
            OracleParameter[] sp = {    data.MakeInParam(":parent_post_id", parent_post_id??"0"),
                                        data.MakeInParam(":post_code",post_code??"" ),
                                        data.MakeInParam(":post_name",post_name??"" ),
                                        data.MakeInParam(":dept_id",dept_id??"" ),
                                        data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID") )};
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 修改
        [HttpPost]
        public async Task<IActionResult> Modify(string dept_id, string post_code, string post_name, string parent_post_id,string post_id)
        {
            string Msg = "";
            if (post_id == null)
            {
                return Content("{\"code\":300,\"msg\":\"请传入关键值\"}");
            }
            string sql = @"Update App_Posts
                                   Set Parent_Post_Id   = :parent_post_id,
                                       Post_Code        = :post_code,
                                       Post_Name        = :post_name,
                                       Dept_Id          = :dept_id,
                                       Last_Update_Date = sysdate,
                                       Last_Updated_By  = :user_id
                                 Where Post_Id = :post_id";
            OracleParameter[] sp = { data.MakeInParam(":parent_post_id", parent_post_id??"0"),
                                     data.MakeInParam(":post_code",post_code??"" ),
                                     data.MakeInParam(":post_name",post_name??"" ),
                                     data.MakeInParam(":dept_id",dept_id??"" ),
                                     data.MakeInParam(":user_id",HttpContext.Session.GetString("USER_ID")),
                                     data.MakeInParam(":post_id",post_id)};
            bool flag = await data.DoSqlByParam(sql, sp);
            Msg = flag ? "{\"code\":200,\"msg\":\"保存成功\"}" : "{\"code\":300,\"msg\":\"保存失败,请联系管理员\"}";
            return Content(Msg);
        }
        #endregion

        #region 根据部门获取岗位
        [HttpPost]
        public async Task<IActionResult> GetPostForExport(string post_name, string dept_id, string status)
        {
            string msg = "";
            string sql = @"select 
                               t.Post_Code,
                               t.Post_Name,                               
                               Ps.Post_Code parent_post_code,
                               Ps.Post_Name parent_post_name,
                               c.Corp_Code,
                               c.Corp_Name,
                               d.Dept_Code,
                               d.Dept_Name,
                               Decode(t.Status, 0, '有效', 1, '无效') Status
                         From App_Posts t, App_Posts Ps, App_Dept d, App_Corp c
                        Where t.Parent_Post_Id = Ps.Post_Id(+)
                          And t.Dept_Id = d.Dept_Id
                          And d.Corp_Id = c.Corp_Id
                          And (t.post_code Like '%'|| :post_code ||'%' Or t.post_name Like '%'|| :post_name ||'%')
                          And t.status=:status
                          And t.dept_id=:dept_id";
            OracleParameter[] sp = {
                                    data.MakeInParam(":post_code",post_name??""),
                                    data.MakeInParam(":post_name",post_name??""),
                                    data.MakeInParam(":status",status),
                                    data.MakeInParam(":dept_id",dept_id??"")};
            DataSet ds = await data.GetDataSetByParam(sql, sp);
            List<Models.ExcelPost> post = data.DataSetToIList1<Models.ExcelPost>(ds, 0).ToList();
            byte[] buffer = Models.ExcelHelper.Export(post, "岗位", Models.ExcelTitle.Post).GetBuffer();
            return File(buffer, "application/ms-excel", "岗位数据导出.xls");
        }
        #endregion

        #region 获取菜单树
        [HttpGet]
        public async Task<IActionResult> GetMenuTree()
        {
            string sql = @"select m.menu_id value,
                           m.parent_menu_id pid,
                           m.menu_name name
                      from app_menu m  Where m.Menu_Type = 0
                      order by m.menu_sort asc";
            var ds = await data.GetDataSet(sql);
            string json = "";
            try
            {
                var list = DataSetHelper.DataSetToList<Models.MenuTree>(ds, 0).ToList();
                json = ToMenuJson(list, 0);
                json = "{\"code\":0,\"msg\":\"success\",\"data\":" + json + "}";
            }
            catch (Exception)
            {
                json = "{\"code\":1,\"msg\":\"false\",\"data\":[]}";
            }
            return Content(json);
        }
        #endregion

        #region 转换菜单树
        private string ToMenuJson(List<Models.MenuTree> data, decimal parentId)
        {
            StringBuilder sbJson = new StringBuilder();
            sbJson.Append("[");
            List<Models.MenuTree> entitys = data.FindAll(t => t.pid == parentId);
            if (entitys.Count > 0)
            {
                foreach (var item in entitys)
                {
                    string strJson = item.ToJson();
                    strJson = strJson.Insert(strJson.Length - 1, ",\"children\":" + ToMenuJson(data, item.value) + "");
                    sbJson.Append(strJson + ",");
                }
                sbJson = sbJson.Remove(sbJson.Length - 1, 1);
            }
            sbJson.Append("]");
            return sbJson.ToString();
        } 
        #endregion
    }
}