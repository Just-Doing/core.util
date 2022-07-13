using System.Data;
using System.Data.SqlClient;

namespace CrossSellingApi.Common
{
    /// <summary>
    /// EF 执行sql 查询
    /// </summary>
    public class EfExcuteQueryBySql
    {
        /// <summary>
        /// 使用EF-DB 查询sql
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public DataTable Query(string connStr, string sql, SqlParameter[] paras) {
            if(string.IsNullOrEmpty(sql)) return null;

            using (var conn = new SqlConnection("Server=yanglinqq163.eicp.net,2435;Database=CrossSellingDB;Integrated Security=False;User Id=sa;Password=1qaz!QAZ;")) {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.Parameters.Clear();
                cmd.Parameters.AddRange(paras);
                cmd.CommandText = sql;
                var dataAdapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                dataAdapter.Fill(dt);
                return dt;
            }
            

        }


    }
}
