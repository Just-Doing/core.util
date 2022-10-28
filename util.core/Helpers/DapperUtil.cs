using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System;

namespace Util.Core.Helpers
{
    public class DapperUtil
    {
        private string _connStr = "";

        public DapperUtil(string connStr) {
            _connStr = connStr;
        }

        public IDbConnection DbConnection
        {
            get
            {
                return new SqlConnection(_connStr);
            }
        }




        public IEnumerable<T> Query<T>(string sql, object? param = null, IDbTransaction? transaction = null,
             bool buffered = true, int? commandTimeout = null, CommandType? commandType = null, IDbConnection connection = null)
        {
            IDbConnection conn = transaction?.Connection != null ? transaction.Connection : DbConnection;
            var res = conn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
            if (transaction?.Connection == null)
            {
                conn.Dispose();
            }
            return res;
            
        }


        public DataTable QueryData(string sql, object param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            IDbConnection conn = transaction?.Connection != null ? transaction.Connection : DbConnection;
            DataTable table = new DataTable("Table");
            var reader = conn.ExecuteReader(sql, param, transaction, commandTimeout, commandType);
            table.Load(reader);
            if (transaction?.Connection == null)
            {
                conn.Dispose();
            }
            return table;
            
        }

        public T QuerySingleOrDefault<T>(string sql, object param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            IDbConnection conn = transaction?.Connection != null ? transaction.Connection : DbConnection;
            var res = conn.QuerySingleOrDefault<T>(sql, param, transaction, commandTimeout, commandType);
            if (transaction?.Connection == null)
            {
                conn.Dispose();
            }
            return res;
            
        }

        public T QueryFirstOrDefault<T>(string sql, object param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            IDbConnection conn = transaction?.Connection != null ? transaction.Connection : DbConnection;
            var res = conn.QueryFirstOrDefault<T>(sql, param, transaction, commandTimeout, commandType);
            if (transaction?.Connection == null)
            {
                conn.Dispose();
            }
            return res;
            
        }
        //将 sql 语句的返回值分页输出
        public IEnumerable<T> SqlPageQuery<T>(string sql, string orderByName, string orderType, int pageIndex, int pageSize, out int totalCount, object param = null, IDbTransaction transaction = null,
          bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            IDbConnection conn = transaction?.Connection != null ? transaction.Connection : DbConnection;
            totalCount = conn.QuerySingleOrDefault<int>("select count(1) from (" + sql + ") t0", param, transaction);
            var startIx = (pageIndex - 1) * pageSize;
            var endIx = startIx + pageSize;
            var pageSql = @"
                    SELECT * FROM(
	                    SELECT row_number() OVER(ORDER BY userid) AS no,* FROM (" + sql + @") t0
                    ) t1
                    WHERE t1.no > " + startIx + " and t1.no <= " + endIx + "";
            var res = conn.Query<T>(pageSql, param, transaction, buffered, commandTimeout, commandType);
            if (transaction?.Connection == null)
            {
                conn.Dispose();
            }
            return res;
            
        }
        //将 单表数据按条件 分页输出
        public IEnumerable<T> SingleTablePageQuery<T>(string tableOrViewName, string fileds, string where, string orderByName, string orderType, int pageIndex, int pageSize, out int totalCount, object param = null, IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            IDbConnection conn = transaction?.Connection != null ? transaction.Connection : DbConnection;
            totalCount = conn.QuerySingleOrDefault<int>(string.Format("select count(1) from {0} {1}", tableOrViewName, where), param, transaction);
            var startIx = (pageIndex - 1) * pageSize;
            var endIx = startIx + pageSize;
            var pageSql = @"
                    SELECT * FROM(
	                    SELECT row_number() OVER(ORDER BY userid) AS no," + fileds + " FROM " + tableOrViewName + @"
                    ) t0
                    WHERE t0.no > " + startIx + " and t0.no <= " + endIx + "";
            var res = conn.Query<T>(pageSql, param, transaction, buffered, commandTimeout, commandType);
            if (transaction?.Connection == null)
            {
                conn.Dispose();
            }
            return res;
            
        }

        public int Execute(string sql, object param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            IDbConnection conn = transaction?.Connection != null ? transaction.Connection : DbConnection;
            var res = conn.Execute(sql, param, transaction, commandTimeout, commandType);
            if (transaction?.Connection == null) {
                conn.Dispose();
            }
            return res;
        }
    }
}
