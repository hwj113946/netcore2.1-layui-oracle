using System;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

namespace Common
{
    public class DBHelper
    {
        protected OracleConnection Connection;
        //private string ConStr = "User ID=system;Password=113946;Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = LAYUI)))";
        private string ConStr = @"User ID=TXRYHD;Password=txryhd;Data Source=(DESCRIPTION =
                                    (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.113.12)(PORT = 1521))
                                    (CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = sgxdwz)))";

        public DBHelper()
        {            
            Connection = new OracleConnection(ConStr);
        }

        #region 打开连接
        public void OpenConn()
        {
            if (this.Connection.State != ConnectionState.Open)            
                this.Connection.Open();            
        }
        #endregion

        #region 关闭数据库连接
        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void CloseConn()
        {
            if (Connection != null)
                Connection.Close();
        }
        #endregion

        #region 释放
        public void DisposeConn()
        {
            if (Connection != null)
                Connection.Dispose();
        }
        #endregion

        #region   传入参数并且转换为OracleParameter类型
        /// <summary>
        /// 转换参数
        /// </summary>
        /// <param name="ParamName">存储过程名称或命令文本</param>
        /// <param name="DbType">参数类型</param></param>
        /// <param name="Size">参数大小</param>
        /// <param name="Value">参数值</param>
        /// <returns>新的 parameter 对象</returns>
        public OracleParameter MakeInParam(string ParamName, OracleDbType DbType, int Size, object Value)
        {
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Input, Value);
        }
        public OracleParameter MakeInParam(string ParaName, Object Value)
        {
            OracleParameter param;

            param = new OracleParameter(ParaName, Value);
            param.Direction = ParameterDirection.Input;
            return param;
        }
        public OracleParameter MakeInParam(string ParaName, OracleDbType DbType, Int32 Size, ParameterDirection pd)
        {
            OracleParameter param;

            if (Size > 0)
                param = new OracleParameter(ParaName, DbType, Size);
            else
                param = new OracleParameter(ParaName, DbType);

            param.Direction = pd;
            return param;
        }


        /// <summary>
        /// 初始化参数值

        /// </summary>
        /// <param name="ParamName">存储过程名称或命令文本</param>
        /// <param name="DbType">参数类型</param>
        /// <param name="Size">参数大小</param>
        /// <param name="Direction">参数方向</param>
        /// <param name="Value">参数值</param>
        /// <returns>新的 parameter 对象</returns>
        /// 

        public OracleParameter MakeParam(string ParamName, OracleDbType DbType, Int32 Size, ParameterDirection Direction, object Value)
        {
            OracleParameter param;

            if (Size > 0)
                param = new OracleParameter(ParamName, DbType, Size);
            else
                param = new OracleParameter(ParamName, DbType);

            param.Direction = Direction;
            if (!(Direction == ParameterDirection.Output && Value == null))
                param.Value = Value;
            return param;
        }
        #endregion

        #region 获取DataSet：根据Oracle参数组
        /// <summary>
        /// 获取DataSet：根据Oracle参数组，传惨时，顺序要对应sql语句中的位置
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="sp">oracle参数组</param>
        /// <returns></returns>
        public DataSet GetDataSetByParamAsNoAsync(string sql, OracleParameter[] sp)
        {
            try
            {
                OpenConn();
                OracleCommand cmd = new OracleCommand(sql, Connection);
                cmd.Parameters.AddRange(sp);
                OracleDataAdapter oda = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                oda.Fill(ds);
                cmd.Parameters.Clear();
                cmd.Dispose();
                oda.Dispose();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseConn();
            }
        }
        #endregion

        #region 获取DataSet：根据Oracle参数组
        /// <summary>
        /// 获取DataSet：根据Oracle参数组，传惨时，顺序要对应sql语句中的位置
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="sp">oracle参数组</param>
        /// <returns></returns>
        public async Task<DataSet> GetDataSetByParam(string sql, OracleParameter[] sp)
        {
            return await Task.Run(() =>
            {
                try
                {
                    OpenConn();
                    OracleCommand cmd = new OracleCommand(sql, Connection);
                    cmd.Parameters.AddRange(sp);
                    OracleDataAdapter oda = new OracleDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    oda.Fill(ds);
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                    oda.Dispose();
                    return ds;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    CloseConn();
                }
            });
        }
        #endregion

        #region 获取DataSet
        /// <summary>
        /// 获取DataSet：根据Oracle参数组
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public async Task<DataSet> GetDataSet(string sql)
        {
            return await Task.Run(() =>
            {
                try
                {
                    OpenConn();
                    OracleCommand cmd = new OracleCommand(sql, Connection);
                    OracleDataAdapter oda = new OracleDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    oda.Fill(ds);
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                    oda.Dispose();
                    return ds;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    CloseConn();
                }
            });
        }
        #endregion

        #region 执行带参数的sql返回string
        /// <summary>
        /// 执行带参数的sql返回string
        ///  [where条件时绑定数据，防止注入式攻击]
        /// </summary>
        /// <param name="Oracle"></param>
        /// <returns></returns>
        public async Task<string> GetStringByParam(string Oracle, OracleParameter[] sp)
        {
            return await Task.Run(() =>
            {
                string getString = "";
                try
                {
                    OpenConn();
                    OracleCommand cmm = new OracleCommand(Oracle, Connection);
                    cmm.Parameters.AddRange(sp);
                    OracleDataReader odr = cmm.ExecuteReader();
                    if (odr.HasRows)
                    {
                        odr.Read();
                        getString = Convert.ToString(odr[0]);

                    }
                    odr.Dispose();
                    odr.Close();
                    cmm.Parameters.Clear();
                }
                catch (Exception)
                {
                    return "";
                    throw;
                }
                finally
                {
                    CloseConn();
                }
                return getString;
            });
        }
        #endregion

        #region 执行不带参数的sql返回string
        /// <summary>
        /// 执行不带参数的sql返回string
        ///  [where条件时绑定数据，防止注入式攻击]
        /// </summary>
        /// <param name="Oracle"></param>
        /// <returns></returns>
        public async Task<string> GetString(string Oracle)
        {
            return await Task.Run(() =>
            {
                string getString = "";
                try
                {
                    OpenConn();
                    OracleCommand cmm = new OracleCommand(Oracle, Connection);
                    OracleDataReader odr = cmm.ExecuteReader();
                    if (odr.HasRows)
                    {
                        odr.Read();
                        getString = Convert.ToString(odr[0]);

                    }
                    odr.Dispose();
                    odr.Close();
                    cmm.Parameters.Clear();
                }
                catch (Exception)
                {
                    return "";
                    throw;
                }
                finally
                {
                    CloseConn();
                }
                return getString;
            });
        }
        #endregion

        #region 执行单个SQL语句,带参数
        public async Task<bool> DoSqlByParam(string sql, OracleParameter[] sp)
        {
            return await Task.Run(() =>
            {
                int rows;
                try
                {
                    OpenConn();
                    OracleCommand oc = new OracleCommand(sql, Connection);
                    oc.Parameters.AddRange(sp);
                    rows = oc.ExecuteNonQuery();
                    oc.Parameters.Clear();
                    oc.Dispose();
                    return (rows==-1?true:(rows>0?true:false));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    CloseConn();
                }
            });
        }
        #endregion

        #region 执行单个SQL语句,不带参数
        public async Task<bool> DoSql(string sql)
        {
            return await Task.Run(() =>
            {
                int rows;
                try
                {
                    OpenConn();
                    OracleCommand oc = new OracleCommand(sql, Connection);
                    rows = oc.ExecuteNonQuery();
                    oc.Parameters.Clear();
                    oc.Dispose();
                    return (rows > 0);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    CloseConn();
                }
            });
        }
        #endregion

        #region 根据相关条件对数据库进行更新操作 用法：Update("test","Id=:Id",ht);
        public async Task<int> DoSql(string sql, Hashtable ht)
        {
            return await Task.Run(() =>
            {
                try
                {
                    int val = 0;
                    OracleParameter[] Parms = new OracleParameter[ht.Count];
                    IDictionaryEnumerator et = ht.GetEnumerator();
                    int i = 0;
                    // 作哈希表循环
                    while (et.MoveNext())
                    {
                        OracleParameter op = MakeParam(":" + et.Key.ToString(), et.Value.ToString());
                        Parms[i] = op; // 添加SqlParameter对象
                        i = i + 1;
                    }
                    val = ExecuteNonQuery(sql, Parms);
                    return val;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    CloseConn();
                }
            });
        }
        #endregion

        #region 执行SQL语句，返回数据到DataSet中
        /// <summary>
        /// 执行SQL语句，返回数据到DataSet中:无参
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回DataSet</returns>
        public async Task<DataSet> ReturnDataSet(string sql)
        {
            return await Task.Run(() =>
            {
                try
                {
                    OpenConn();
                    DataSet dataSet = new DataSet();
                    OracleDataAdapter OraDA = new OracleDataAdapter(sql, Connection);
                    OraDA.Fill(dataSet);
                    OraDA.Dispose();
                    return dataSet;
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    CloseConn();
                }
            });
        }
        #endregion

        #region 执行SQL语句，返回数据到DataSet中
        /// <summary>
        /// 执行SQL语句，返回数据到DataSet中
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="DataSetName">自定义返回的DataSet表名</param>
        /// <returns>返回DataSet</returns>
        public DataSet ReturnDataSet(string sql, string DataSetName)
        {
            try
            {
                OpenConn();
                DataSet dataSet = new DataSet();
                OracleDataAdapter OraDA = new OracleDataAdapter(sql, Connection);
                OraDA.Fill(dataSet, DataSetName);
                OraDA.Dispose();
                return dataSet;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                CloseConn();
            }
        }
        #endregion

        #region 执行SQL语句，返回数据到DataSet中
        /// <summary>
        /// 执行SQL语句，返回数据到DataSet中
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="ht">Hashtable，查询参数</param>
        /// <returns>返回DataSet</returns>
        public async Task<DataSet> ReturnDataSet(string sql, Hashtable ht)
        {
            return await Task.Run(() =>
            {
                try
                {
                    OpenConn();
                    DataSet dataSet = new DataSet();
                    OracleCommand cmd = new OracleCommand(sql, Connection);
                    if (ht.Count > 0)
                    {
                        IDictionaryEnumerator et = ht.GetEnumerator();
                        while (et.MoveNext()) // 作哈希表循环
                       {
                            OracleParameter op = MakeParam(":" + et.Key.ToString(), et.Value);
                            cmd.Parameters.Add(op);
                        }
                    }
                    OracleDataAdapter OraDA = new OracleDataAdapter(cmd);
                    OraDA.Fill(dataSet);
                    OraDA.Dispose();
                    cmd.Dispose();
                    return dataSet;
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    CloseConn();
                }
            });
        }
        #endregion

        #region 执行Sql语句,返回带分页功能的dataset
        /// <summary>
        /// 执行Sql语句,返回带分页功能的dataset
        /// </summary>
        /// <param name="sql">Sql语句</param>
        /// <param name="PageSize">每页显示记录数</param>
        /// <param name="CurrPageIndex"><当前页/param>
        /// <param name="DataSetName">返回dataset表名</param>
        /// <returns>返回DataSet</returns>
        public DataSet ReturnDataSet(string sql, int PageSize, int CurrPageIndex, string DataSetName)
        {
            try
            {
                DataSet dataSet = new DataSet();
                OpenConn();
                OracleDataAdapter OraDA = new OracleDataAdapter(sql, Connection);
                OraDA.Fill(dataSet, PageSize * (CurrPageIndex - 1), PageSize, DataSetName);
                OraDA.Dispose();
                return dataSet;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                CloseConn();
            }
        }
        #endregion

        #region 执行SQL语句，返回 DataReader,用之前一定要先.read()打开,然后才能读到数据
        /// <summary>
        /// 执行SQL语句，返回 DataReader,用之前一定要先.read()打开,然后才能读到数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回一个OracleDataReader</returns>
        public OracleDataReader ReturnDataReader(String sql)
        {
            OpenConn();
            OracleCommand command = new OracleCommand(sql, Connection);
            return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }
        #endregion

        #region 执行SQL语句，返回记录总数数
        /// <summary>
        /// 执行SQL语句，返回记录总数数
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回记录总条数</returns>
        public int GetRecordCount(string sql)
        {
            try
            {
                int recordCount = 0;
                OpenConn();
                OracleCommand command = new OracleCommand(sql, Connection);
                OracleDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    recordCount++;
                }
                dataReader.Close();
                command.Dispose();
                return recordCount;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                CloseConn();
            }
        }
        #endregion

        #region 取当前序列,条件为seq.nextval或seq.currval
        /// <summary>
        /// 取当前序列
        /// </summary>
        /// <param name="seqstr"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public async Task<decimal> GetSeq(string seqstr)
        {
            return await Task.Run(() =>
            {
                try
                {
                    decimal seqnum = 0;
                    string sql = "select " + seqstr + " from dual";
                    OpenConn();
                    OracleCommand command = new OracleCommand(sql, Connection);
                    OracleDataReader dataReader = command.ExecuteReader();
                    if (dataReader.Read())
                    {
                        seqnum = decimal.Parse(dataReader[0].ToString());
                    }
                    dataReader.Close();
                    command.Dispose();
                    return seqnum;
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    CloseConn();
                }
            });
        }
        #endregion

        #region 执行SQL语句,返回所影响的行数
        /// <summary>
        /// 执行SQL语句,返回所影响的行数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int ExecuteSQL(string sql)
        {
            int Cmd = 0;
            OpenConn();
            OracleCommand command = new OracleCommand(sql, Connection);
            try
            {
                Cmd = command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                CloseConn();
            }

            return Cmd;
        }
        #endregion

        //　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝ 
        //　＝＝用hashTable对数据库进行insert,update,del操作,注意此时只能用默认的数据库连接"connstr"＝＝
        //　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

        #region 根据表名及哈稀表自动插入数据库 用法：Insert("test",ht)
        public async Task<int> Insert(string TableName, Hashtable ht)
        {
            return await Task.Run(() =>
            {
                try
                {
                    int val = 0;
                    OracleParameter[] Parms = new OracleParameter[ht.Count];
                    IDictionaryEnumerator et = ht.GetEnumerator();
                    DataTable dt = GetTabType(TableName);
                    OracleDbType otype;
                    int size = 0;
                    int i = 0;

                    while (et.MoveNext()) // 作哈希表循环
                    {
                        GetoType(et.Key.ToString().ToUpper(), dt, out otype, out size);
                        OracleParameter op = MakeParam(":" + et.Key.ToString(), otype, size, et.Value.ToString());
                        Parms[i] = op; // 添加SqlParameter对象
                        i = i + 1;
                    }
                    string str_Sql = GetInsertSqlbyHt(TableName, ht); // 获得插入sql语句
                    val = ExecuteNonQuery(str_Sql, Parms);
                    return val;
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    CloseConn();
                }
            });
        }
        #endregion

        #region 根据相关条件对数据库进行更新操作 用法：Update("test","Id=:Id",ht);
        public async Task<int> Update(string TableName, string ht_Where, Hashtable ht)
        {
            return await Task.Run(() =>
            {
                try
                {
                    int val = 0;
                    OracleParameter[] Parms = new OracleParameter[ht.Count];
                    IDictionaryEnumerator et = ht.GetEnumerator();
                    DataTable dt = GetTabType(TableName);
                    OracleDbType otype;
                    int size = 0;
                    int i = 0;
                    // 作哈希表循环
                    while (et.MoveNext())
                    {
                        GetoType(et.Key.ToString().ToUpper(), dt, out otype, out size);
                        OracleParameter op = MakeParam(":" + et.Key.ToString(), otype, size, et.Value.ToString());
                        Parms[i] = op; // 添加SqlParameter对象
                        i = i + 1;
                    }
                    string str_Sql = GetUpdateSqlbyHt(TableName, ht_Where, ht); // 获得插入sql语句
                    val = ExecuteNonQuery(str_Sql, Parms);
                    return val;
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    CloseConn();
                }
            });
        }
        #endregion

        #region del操作,注意此处条件个数与hash里参数个数应该一致 用法：Del("test","Id=:Id",ht)
        public async Task<int> Del(string TableName, string ht_Where, Hashtable ht)
        {
            return await Task.Run(() =>
            {
                try
                {
                    int val = 0;
                    OracleParameter[] Parms = new OracleParameter[ht.Count];
                    IDictionaryEnumerator et = ht.GetEnumerator();
                    DataTable dt = GetTabType(TableName);
                    OracleDbType otype;
                    int i = 0;
                    int size = 0;
                    // 作哈希表循环
                    while (et.MoveNext())
                    {
                        GetoType(et.Key.ToString().ToUpper(), dt, out otype, out size);
                        OracleParameter op = MakeParam(":" + et.Key.ToString(), et.Value.ToString());
                        Parms[i] = op; // 添加SqlParameter对象
                        i = i + 1;
                    }
                    string str_Sql = GetDelSqlbyHt(TableName, ht_Where, ht); // 获得删除sql语句
                    val = ExecuteNonQuery(str_Sql, Parms);
                    return val;
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    CloseConn();
                }
            });
        }
        #endregion

        //　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
        //　＝＝＝＝＝＝＝＝上面三个操作的内部调用函数＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
        //　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝ 

        #region 根据哈稀表及表名自动生成相应insert语句(参数类型的)
        /// <summary>
        /// 根据哈稀表及表名自动生成相应insert语句
        /// </summary>
        /// <param name="TableName">要插入的表名</param>
        /// <param name="ht">哈稀表</param>
        /// <returns>返回sql语句</returns>
        public static string GetInsertSqlbyHt(string TableName, Hashtable ht)
        {
            string str_Sql = "";
            int i = 0;
            int ht_Count = ht.Count; // 哈希表个数
            IDictionaryEnumerator myEnumerator = ht.GetEnumerator();
            string before = "";
            string behide = "";
            while (myEnumerator.MoveNext())
            {
                if (i == 0)
                {
                    before = "(" + myEnumerator.Key;
                }
                else if (i + 1 == ht_Count)
                {
                    before = before + "," + myEnumerator.Key + ")";
                }
                else
                {
                    before = before + "," + myEnumerator.Key;
                }
                i = i + 1;
            }
            behide = " Values" + before.Replace(",", ",:").Replace("(", "(:");
            str_Sql = "Insert into " + TableName + before + behide;
            return str_Sql;
        }
        #endregion

        #region 根据表名，where条件，哈稀表自动生成更新语句(参数类型的)
        public static string GetUpdateSqlbyHt(string Table, string ht_Where, Hashtable ht)
        {
            string str_Sql = "";
            int i = 0;
            int ht_Count = ht.Count; // 哈希表个数
            IDictionaryEnumerator myEnumerator = ht.GetEnumerator();
            while (myEnumerator.MoveNext())
            {
                if (i == 0)
                {
                    if (ht_Where.ToString().ToLower().IndexOf((myEnumerator.Key + "=:" + myEnumerator.Key).ToLower()) == -1)
                    {
                        str_Sql = myEnumerator.Key + "=:" + myEnumerator.Key;
                    }
                }
                else
                {
                    if (ht_Where.ToString().ToLower().IndexOf((":" + myEnumerator.Key + " ").ToLower()) == -1)
                    {
                        str_Sql = str_Sql + "," + myEnumerator.Key + "=:" + myEnumerator.Key;
                    }

                }
                i = i + 1;
            }
            if (ht_Where == null || ht_Where.Replace(" ", "") == "")  // 更新时候没有条件
            {
                str_Sql = "update " + Table + " set " + str_Sql;
            }
            else
            {
                str_Sql = "update " + Table + " set " + str_Sql + " where " + ht_Where;
            }
            str_Sql = str_Sql.Replace("set ,", "set ").Replace("update ,", "update ");
            return str_Sql;
        }
        #endregion

        #region 根据表名，where条件，哈稀表自动生成del语句(参数类型的)
        public static string GetDelSqlbyHt(string Table, string ht_Where, Hashtable ht)
        {
            string str_Sql = "";
            int i = 0;

            int ht_Count = ht.Count; // 哈希表个数
            IDictionaryEnumerator myEnumerator = ht.GetEnumerator();
            while (myEnumerator.MoveNext())
            {
                if (i == 0)
                {
                    if (ht_Where.ToString().ToLower().IndexOf((myEnumerator.Key + "=:" + myEnumerator.Key).ToLower()) == -1)
                    {
                        str_Sql = myEnumerator.Key + "=:" + myEnumerator.Key;
                    }
                }
                else
                {
                    if (ht_Where.ToString().ToLower().IndexOf((":" + myEnumerator.Key + " ").ToLower()) == -1)
                    {
                        str_Sql = str_Sql + "," + myEnumerator.Key + "=:" + myEnumerator.Key;
                    }

                }
                i = i + 1;
            }
            if (ht_Where == null || ht_Where.Replace(" ", "") == "")  // 更新时候没有条件
            {
                str_Sql = "Delete " + Table;
            }
            else
            {
                str_Sql = "Delete " + Table + " where " + ht_Where;
            }
            return str_Sql;
        }
        #endregion

        #region 生成oracle参数
        /// <summary>
        /// 生成oracle参数
        /// </summary>
        /// <param name="ParamName">字段名</param>
        /// <param name="otype">数据类型</param>
        /// <param name="size">数据大小</param>
        /// <param name="Value">值</param>
        /// <returns></returns>
        public static OracleParameter MakeParam(string ParamName, OracleDbType otype, int size, Object Value)
        {
            OracleParameter para = new OracleParameter(ParamName, Value);
            para.OracleDbType = otype;
            para.Size = size;
            return para;
        }
        #endregion

        #region 生成oracle参数
        public static OracleParameter MakeParam(string ParamName, object Value)
        {
            return new OracleParameter(ParamName, Value);
        }
        #endregion

        #region 根据表结构字段的类型和长度拼装oracle sql语句参数
        public static void GetoType(string key, DataTable dt, out OracleDbType otype, out int size)
        {

            DataView dv = dt.DefaultView;
            dv.RowFilter = "column_name='" + key + "'";
            string fType = dv[0]["data_type"].ToString().ToUpper();
            switch (fType)
            {
                case "DATE":
                    otype = OracleDbType.Date;
                    size = int.Parse(dv[0]["data_length"].ToString());
                    break;
                case "CHAR":
                    otype = OracleDbType.Char;
                    size = int.Parse(dv[0]["data_length"].ToString());
                    break;
                case "LONG":
                    otype = OracleDbType.Double;
                    size = int.Parse(dv[0]["data_length"].ToString());
                    break;
                case "NVARCHAR2":
                    otype = OracleDbType.NVarchar2;
                    size = int.Parse(dv[0]["data_length"].ToString());
                    break;
                case "VARCHAR2":
                    otype = OracleDbType.Varchar2;
                    size = int.Parse(dv[0]["data_length"].ToString());
                    break;
                default:
                    otype = OracleDbType.Varchar2;
                    size = 100;
                    break;
            }
        }
        #endregion

        #region 动态取表里字段的类型和长度,此处没有动态用到connstr,是默认的！
        public System.Data.DataTable GetTabType(string tabnale)
        {
            string sql = "select  column_name,data_type,data_length from all_tab_columns where table_name='" + tabnale.ToUpper() + "'";
            OpenConn();
            return (ReturnDataSet(sql, "dv")).Tables[0];

        }
        #endregion

        #region 执行sql语句
        public int ExecuteNonQuery(string cmdText, params OracleParameter[] cmdParms)
        {

            try
            {
                int val = 0;
                OracleCommand cmd = new OracleCommand();
                OpenConn();
                cmd.Connection = Connection;
                cmd.CommandText = cmdText;
                if (cmdParms != null)
                {
                    foreach (OracleParameter parm in cmdParms)
                        cmd.Parameters.Add(parm);
                }
                val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                cmd.Dispose();
                return val;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                CloseConn();
            }
        }
        #endregion

        #region 执行多个带参sql
        /// <summary>
        /// 使用事务的方式执行，可同时执行多个
        /// </summary>
        /// <param name="ht">key为sql，value为OracleParameter[]参数</param>
        /// <returns></returns>
        public async Task<bool> DoSqlList(Hashtable ht)
        {
            return await Task.Run(() =>
            {
                OpenConn();
                OracleCommand cmd = Connection.CreateCommand();
                OracleTransaction tran = Connection.BeginTransaction();
                try
                {
                    int n = 0;
                    foreach (DictionaryEntry item in ht)
                    {
                        string sqlText = item.Key.ToString();
                        OracleParameter[] sp = (OracleParameter[])item.Value;
                        PrepareCommand(cmd, Connection, tran, sqlText, sp);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        n++;
                    }
                    if (n == ht.Count)
                    {
                        tran.Commit();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return false;
                }
                finally
                {
                    CloseConn();
                }
            });
        }

        public static void PrepareCommand(OracleCommand oc, OracleConnection conn, OracleTransaction trans, string ocText, OracleParameter[] Parms)
        {
            oc.CommandText = ocText;
            oc.CommandType = CommandType.Text;
            if (oc != null)
            {
                foreach (OracleParameter item in Parms)
                {
                    if ((item.Direction == ParameterDirection.InputOutput || item.Direction == ParameterDirection.Input) && (item.Value == null))
                    {
                        item.Value = DBNull.Value;
                    }
                    oc.Parameters.Add(item);
                }
            }
        }
        #endregion

        #region 执行SQL语句，返回数据到List中
        /// <summary>
        /// 执行SQL语句，返回数据到DataSet中
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="ht">Hashtable，查询参数（col1，"ABC123"）</param>
        /// <returns>返回DataSet</returns>
        public async Task<List<T>> GetList<T>(string sql, Hashtable ht) where T : class
        {
            return await Task.Run(async () =>
            {
                try
                {
                    OpenConn();
                    DataSet dataSet = new DataSet();
                    OracleCommand cmd = new OracleCommand(sql, Connection);
                    if (ht.Count > 0)
                    {
                        //OracleParameter[] sp = null;
                        IDictionaryEnumerator et = ht.GetEnumerator();
                        //int i = 0;
                        while (et.MoveNext()) // 作哈希表循环
                        {
                            OracleParameter op = MakeParam(":" + et.Key.ToString(), et.Value.ToString());
                            cmd.Parameters.Add(op);

                        }
                        //cmd.Parameters.AddRange(sp);
                    }
                    OracleDataAdapter OraDA = new OracleDataAdapter(cmd);
                    OraDA.Fill(dataSet);
                    OraDA.Dispose();
                    cmd.Dispose();
                    IList<T> gbList = await DataSetToIList<T>(dataSet, 0);
                    if (gbList != null && gbList.Count > 0)
                    {
                        List<T> list = new List<T>();
                        for (int i = 0; i < gbList.Count; i++)
                        {
                            T temp = gbList[i] as T;
                            if (temp != null)
                                list.Add(temp);
                        }
                        return list;
                    }
                    return null;
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    CloseConn();
                }
            });
        }

        #region DataSetToIList
        /// <summary>
        /// DataSetToList
        /// </summary>
        /// <typeparam name="T">转换类型</typeparam>
        /// <param name="dataSet">数据源</param>
        /// <param name="tableIndex">需要转换表的索引</param>
        /// <returns></returns>
        public async Task<IList<T>> DataSetToIList<T>(DataSet dataSet, int tableIndex)
        {
            return await Task.Run(() =>
            {
                //确认参数有效
                if (dataSet == null || dataSet.Tables.Count <= 0 || tableIndex < 0)
                    return null;

                DataTable dt = dataSet.Tables[tableIndex];

                IList<T> list = new List<T>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //创建泛型对象
                    T _t = Activator.CreateInstance<T>();
                    //获取对象所有属性
                    PropertyInfo[] propertyInfo = _t.GetType().GetProperties();
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        foreach (PropertyInfo info in propertyInfo)
                        {
                            //属性名称和列名相同时赋值
                            if (dt.Columns[j].ColumnName.ToUpper().Equals(info.Name.ToUpper()))
                            {
                                if (dt.Rows[i][j] != DBNull.Value)
                                {
                                    info.SetValue(_t, dt.Rows[i][j], null);
                                }
                                else
                                {
                                    info.SetValue(_t, null, null);
                                }
                                break;
                            }
                        }
                    }
                    list.Add(_t);
                }
                return list;
            });
        }
        #endregion
        #endregion

        #region 执行SQL语句，返回数据到List中
        /// <summary>
        /// 执行SQL语句，返回数据到DataSet中
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="ht">Hashtable，查询参数（col1，"ABC123"）</param>
        /// <returns>返回DataSet</returns>
        public List<T> GetList1<T>(string sql, Hashtable ht) where T : class
        {
            try
            {
                OpenConn();
                DataSet dataSet = new DataSet();
                OracleCommand cmd = new OracleCommand(sql, Connection);
                if (ht.Count > 0)
                {
                    //OracleParameter[] sp = null;
                    IDictionaryEnumerator et = ht.GetEnumerator();
                    //int i = 0;
                    while (et.MoveNext()) // 作哈希表循环
                    {
                        OracleParameter op = MakeParam(":" + et.Key.ToString(), et.Value.ToString());
                        cmd.Parameters.Add(op);

                    }
                    //cmd.Parameters.AddRange(sp);
                }
                OracleDataAdapter OraDA = new OracleDataAdapter(cmd);
                OraDA.Fill(dataSet);
                OraDA.Dispose();
                cmd.Dispose();
                IList<T> gbList = DataSetToIList1<T>(dataSet, 0);
                if (gbList != null && gbList.Count > 0)
                {
                    List<T> list = new List<T>();
                    for (int i = 0; i < gbList.Count; i++)
                    {
                        T temp = gbList[i] as T;
                        if (temp != null)
                            list.Add(temp);
                    }
                    return list;
                }
                return null;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                CloseConn();
            }
        }

        #region DataSetToIList
        /// <summary>
        /// DataSetToList
        /// </summary>
        /// <typeparam name="T">转换类型</typeparam>
        /// <param name="dataSet">数据源</param>
        /// <param name="tableIndex">需要转换表的索引</param>
        /// <returns></returns>
        public IList<T> DataSetToIList1<T>(DataSet dataSet, int tableIndex)
        {
            //确认参数有效
            if (dataSet == null || dataSet.Tables.Count <= 0 || tableIndex < 0)
                return null;

            DataTable dt = dataSet.Tables[tableIndex];

            IList<T> list = new List<T>();

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //创建泛型对象
                    T _t = Activator.CreateInstance<T>();
                    //获取对象所有属性
                    PropertyInfo[] propertyInfo = _t.GetType().GetProperties();
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        foreach (PropertyInfo info in propertyInfo)
                        {
                            //属性名称和列名相同时赋值
                            if (dt.Columns[j].ColumnName.ToUpper().Equals(info.Name.ToUpper()))
                            {
                                if (dt.Rows[i][j] != DBNull.Value)
                                {
                                    info.SetValue(_t, dt.Rows[i][j], null);
                                }
                                else
                                {
                                    info.SetValue(_t, null, null);
                                }
                                break;
                            }
                        }
                    }
                    list.Add(_t);
                }
            }
            catch (Exception ex)
            {
                list = null;
            }
            return list;
        }
        #endregion
        #endregion

        #region 执行SQL语句，返回数据到List中
        /// <summary>
        /// 执行SQL语句，返回数据到DataSet中
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回DataSet</returns>
        public List<T> GetList1<T>(string sql) where T : class
        {
            try
            {
                OpenConn();
                DataSet dataSet = new DataSet();
                OracleCommand cmd = new OracleCommand(sql, Connection);
                OracleDataAdapter OraDA = new OracleDataAdapter(cmd);
                OraDA.Fill(dataSet);
                OraDA.Dispose();
                cmd.Dispose();
                IList<T> gbList = DataSetToIList1<T>(dataSet, 0);
                if (gbList != null && gbList.Count > 0)
                {
                    List<T> list = new List<T>();
                    for (int i = 0; i < gbList.Count; i++)
                    {
                        T temp = gbList[i] as T;
                        if (temp != null)
                            list.Add(temp);
                    }
                    return list;
                }
                return null;
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                CloseConn();
            }
        }
        #endregion

        #region 执行Oracle过程返回文本
        public async Task<string> RunProcStr(string procName, OracleParameter[] prams)
        {
            return await Task.Run(() => {
                try
                {
                    OpenConn();
                    OracleCommand oc = new OracleCommand(procName, Connection);
                    oc.CommandType = CommandType.StoredProcedure;
                    OracleTransaction Trans = Connection.BeginTransaction();
                    oc.Transaction = Trans;
                    OracleParameter pars = prams[prams.Length - 1];
                    if (prams != null)
                    {
                        foreach (OracleParameter parameter in prams)
                        {
                            oc.Parameters.Add(parameter);
                        }
                    }
                    try
                    {
                        oc.ExecuteNonQuery();
                        Trans.Commit();
                        return pars.Value.ToString();

                    }
                    catch (Exception e)
                    {
                        Trans.Rollback();//回滚
                        return "调用" + procName + "存储过程错误，回滚";
                        throw;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    CloseConn();
                }
            });
        }
        #endregion

        #region 执行Oracle过程返回游标Dataset
        public async Task<DataSet> RunProcCur(string procName, OracleParameter[] prams)
        {
            return await Task.Run(() =>
            {
                string cs = "";
                if (prams != null)
                {
                    foreach (OracleParameter parameter in prams)
                    {
                        if (parameter.Direction == ParameterDirection.Input)
                        {
                            cs = cs + "Para:" + parameter.ParameterName.ToString() + "value:" + parameter.Value.ToString() + @"
";
                        }
                    }
                }
                try
                {
                    OpenConn();
                    OracleCommand oc = new OracleCommand(procName, Connection);
                    oc.CommandType = CommandType.StoredProcedure;
                    if (prams != null)
                    {
                        foreach (OracleParameter parameter in prams)
                        {
                            oc.Parameters.Add(parameter);
                        }
                    }
                    OracleDataAdapter dap = new OracleDataAdapter(oc);
                    DataSet ds = new DataSet();
                    dap.Fill(ds);
                    dap.Dispose();
                    return ds;
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally
                {
                    CloseConn();
                }
            });
        }

        #endregion

        #region Excel导入
        public async Task<bool> DoSqlList2<T>(string sql, List<T> list) where T : class
        {
            return await Task.Run(() =>
            {
                OpenConn();
                OracleCommand cmd = Connection.CreateCommand();
                OracleTransaction tran = Connection.BeginTransaction();
                try
                {
                    int n = 0;
                    cmd.CommandText = sql;
                    Type EntityType = list[0].GetType();
                    PropertyInfo[] EntityPro = EntityType.GetProperties();
                    foreach (object item in list)
                    {
                        for (int i = 0; i < EntityPro.Length; i++)
                        {
                            OracleParameter sp = MakeInParam(":" + EntityPro[i].Name, EntityPro[i].GetValue(item, null)??"");
                            cmd.Parameters.Add(sp);
                        }
                        cmd.CommandText = sql;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        n++;
                    }
                    if (n == list.Count)
                    {
                        tran.Commit();
                        return true;
                    }
                    else
                    {
                        tran.Rollback();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return false;
                }
                finally
                {
                    CloseConn();
                }
            });
        }
        #endregion

        #region 执行Oracle过程返回游标Dataset
        public DataSet RunProcCurNoAsync(string procName, OracleParameter[] prams)
        {
            string cs = "";
            if (prams != null)
            {
                foreach (OracleParameter parameter in prams)
                {
                    if (parameter.Direction == ParameterDirection.Input)
                    {
                        cs = cs + "Para:" + parameter.ParameterName.ToString() + "value:" + parameter.Value.ToString() + @"
";
                    }
                }
            }
            try
            {
                OpenConn();
                OracleCommand oc = new OracleCommand(procName, Connection);
                oc.CommandType = CommandType.StoredProcedure;
                if (prams != null)
                {
                    foreach (OracleParameter parameter in prams)
                    {
                        oc.Parameters.Add(parameter);
                    }
                }
                OracleDataAdapter dap = new OracleDataAdapter(oc);
                DataSet ds = new DataSet();
                dap.Fill(ds);
                dap.Dispose();
                return ds;
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                CloseConn();
            }
        }
        #endregion
    }
}
