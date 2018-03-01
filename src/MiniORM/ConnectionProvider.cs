using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

using DreamCube.Foundation.Basic.Objects;

namespace MiniORM
{
    /// <summary>
    /// 连接provider（同一个线程上下文会共有一个连接对象）
    /// </summary>
    public class ConnectionProvider : IConnectionProvider
    {
        protected const String CONNECTION_CACHE_KEY = "Mini.DB.CONNECTION";
        //这样总是感觉在循环引用，所以去掉这种方式，冗余实现方法
        //protected IDB _dbInstance = null;
        protected DbProviderFactory _dbProviderFactory;
        protected String _connectionStr;

        //public ConnectionProvider(IDB dbInstance)
        //{
        //    _dbInstance = dbInstance;
        //}

        public ConnectionProvider(String connectionStr, DbProviderFactory dbProviderFactory)
        {
            _dbProviderFactory = dbProviderFactory;
            _connectionStr = connectionStr;
        }

        //public DbConnection GetConnection(bool open = false)
        //{
        //    //因为CurrentContext是线程安全的，线程唯一性，所以Key值是可以一致的，不需要不同的连接字符串作为key值
        //    DbConnection conn = CurrentContext.GetCacheItem<DbConnection>(connectionCacheKey);
        //    if (conn == null)
        //    {
        //        conn = _dbInstance.GetDBProviderFactory().CreateConnection();
        //        conn.ConnectionString = _dbInstance.GetConnectionString();
        //        CurrentContext.TryCacheItem(connectionCacheKey, conn);
        //    }
        //    if (open && conn.Connection.State != System.Data.ConnectionState.Open)
        //        conn.Open();
        //    return conn;
        //}

        public virtual DbConnection GetConnection(bool open = false)
        {
            //因为CurrentContext是线程安全的，线程唯一性，所以Key值是可以一致的，不需要不同的连接字符串作为key值
            DbConnection conn = CurrentContext.GetCacheItem<DbConnection>(CONNECTION_CACHE_KEY);
            if (conn == null)
            {
                conn = _dbProviderFactory.CreateConnection();
                conn.ConnectionString = _connectionStr;
                CurrentContext.CacheItem(CONNECTION_CACHE_KEY, conn);
            }
            if (open && conn.State != ConnectionState.Open)
                conn.Open();
            return conn;
        }

        public void TryCloseAllConnection()
        {
            try
            {
                DbConnection conn = CurrentContext.GetCacheItem<DbConnection>(CONNECTION_CACHE_KEY);
                if (conn != null && conn.State != ConnectionState.Closed) conn.Close();
            }
            catch (Exception)
            {
                //忽略异常
            }
        }

        public void CloseAllConnection()
        {
            DbConnection conn = CurrentContext.GetCacheItem<DbConnection>(CONNECTION_CACHE_KEY);
            if (conn != null && conn.State != ConnectionState.Closed) conn.Close();
        }
    }
}
