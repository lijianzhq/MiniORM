using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using DreamCube.Foundation.Basic.Utility;
using DreamCube.Foundation.Basic.Objects;

namespace MiniORM
{
    public abstract class DB : IDB
    {
        protected IConnectionStringProvider _connStrProvider;
        protected DbProviderFactory _dbProviderFactory;
        protected String _currentContextTransCacheKey = "_currentContextTransCacheKey";

        #region "属性"

        public abstract ISqlBuilder SqlBuilder
        {
            get;
        }

        protected IConnectionProvider _connectionProvider;
        protected IConnectionProvider ConnectionProvider
        {
            get
            {
                if (_connectionProvider == null)
                    _connectionProvider = CreateConnectionProvider();
                return _connectionProvider;
            }
        }

        public DbProviderFactory DbProviderFactory
        {
            get { return _dbProviderFactory; }
        }

        #endregion

        #region "构造函数"

        //public BasicDB(IConnectionProvider connProvider, ISqlBuilder sqlbuilder)
        //{
        //    _connProvider = connProvider;
        //    _sqlbuilder = sqlbuilder;
        //}

        public DB(IConnectionStringProvider connStrProvider, DbProviderFactory dbProviderFactory)
        {
            _connStrProvider = connStrProvider;
            _dbProviderFactory = dbProviderFactory;
        }

        #endregion

        #region "公共方法"

        /// <summary>
        /// 添加记录到数据库
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public virtual int Add<TEntity>(TEntity t) where TEntity : class
        {
            return AddWithReturn<TEntity>(t, null);
        }

        /// <summary>
        /// 新增记录（支持数据库事务过程）
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="t"></param>
        /// <param name="returns"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int AddWithReturn<TEntity>(TEntity t, Expression<Func<TEntity, dynamic>> returns) where TEntity : class
        {
            String sql;
            List<DbParameter> paramList;
            //构造sql语句
            SqlBuilder.BuildInsert<TEntity>(t, returns, out sql, out paramList);
            WriteLog(sql, paramList);
            if (!String.IsNullOrEmpty(sql))
            {
                DbCommand command = CreateCommand(sql, CommandType.Text);
                if (paramList != null && paramList.Count > 0)
                    command.Parameters.AddRange(paramList.ToArray());
                //return command.ExecuteNonQuery();
                Int32 rowCount = ExecuteNonQuery(command);
                if (rowCount > 0 && returns != null) SetReturnValueToEntity(t, paramList);
                return rowCount;
            }
            return 0;
        }

        /// <summary>
        /// 更新记录（支持数据库事务过程）
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="t"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public int Update<TEntity>(TEntity t, Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            String sql;
            List<DbParameter> paramList;
            //构造sql语句
            SqlBuilder.BuildUpdate(t, predicate, out sql, out paramList);
            WriteLog(sql, paramList);
            if (!String.IsNullOrEmpty(sql))
            {
                DbCommand command = CreateCommand(sql, CommandType.Text);
                if (paramList != null && paramList.Count > 0)
                    command.Parameters.AddRange(paramList.ToArray());
                //return command.ExecuteNonQuery();
                return ExecuteNonQuery(command);
            }
            return 0;
        }

        /// <summary>
        /// 执行command（内部会自动创建数据连接，外部只要配置好commmandtype和commandtext即可）
        /// </summary>
        /// <param name="command"></param>
        /// <param name="autoCloseConnection">
        /// 指示是否自动关闭连接，如果指定自动关闭连接，则无论是否在事务中，都会直接关闭连接对象，关闭连接对象事务会直接commit；
        /// 如果不指定，则会根据上下文来自动判断，如果当前是处于事务上下文中，则不会自动关闭连接，由事务最终关闭。
        /// </param>
        /// <returns></returns>
        public virtual int ExecuteNonQuery(DbCommand command, Boolean? autoCloseConnection = null)
        {
            try
            {
                if (!autoCloseConnection.HasValue)
                    autoCloseConnection = CurrentContext.GetCacheItem(_currentContextTransCacheKey) == null;
                InitalCommand(command);
                return command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //关闭数据库连接
                if (autoCloseConnection.HasValue && autoCloseConnection.Value)
                    ConnectionProvider.CloseAllConnection();
            }
        }

        /// <summary>
        /// 执行command（内部会自动创建数据连接，外部只要配置好commmandtype和commandtext即可）
        /// </summary>
        /// <param name="command"></param>
        /// <param name="autoCloseConnection"></param>
        /// <returns></returns>
        public virtual DataTable ExecuteDatatable(DbCommand command, Boolean? autoCloseConnection = null)
        {
            try
            {
                InitalCommand(command);
                if (!autoCloseConnection.HasValue)
                    autoCloseConnection = CurrentContext.GetCacheItem(_currentContextTransCacheKey) == null;
                DbDataAdapter adapter = DbProviderFactory.CreateDataAdapter();
                adapter.SelectCommand = command;
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                if (ds != null && ds.Tables.Count > 0) return ds.Tables[0];
                return null;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //关闭数据库连接
                if (autoCloseConnection.HasValue && autoCloseConnection.Value)
                    ConnectionProvider.CloseAllConnection();
            }
        }

        /// <summary>
        /// 在事务上下文中执行指令（内部会自动提交事务；如果方法发生异常，内部自动回滚事务）
        /// </summary>
        /// <returns></returns>
        public virtual void ExcuteInTransaction(Action a)
        {
            using (DbTransaction trans = GetConnection().BeginTransaction())
            {
                try
                {
                    CurrentContext.CacheItem(_currentContextTransCacheKey, trans);
                    a.Invoke();
                    trans.Commit();
                    //移除对象
                    CurrentContext.RemoveItem(_currentContextTransCacheKey);
                }
                catch (Exception)
                {
                    MyUtility.CatchEx(new Action(() => trans.Rollback()));
                    throw;
                }
            }
        }

        /// <summary>
        /// 支持查询接口
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public virtual IDBQuery<TEntity> Select<TEntity>() where TEntity : class
        {
            return new DBQuery<TEntity>(this);
        }

        /// <summary>
        /// 根据sql获取实体集合
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <param name="onlyTopOne"></param>
        /// <returns></returns>
        public virtual IList<TEntity> GetEntities<TEntity>(String sql, List<DbParameter> paramList) where TEntity : class,new()
        //protected virtual IList<TEntity> GetEntities<TEntity>(String sql, List<DbParameter> paramList, Boolean onlyTopOne = false) where TEntity : class,new()
        {
            DataTable table = GetDataTable(sql, paramList);
            if (table != null && table.Rows.Count > 0)
            {
                List<TEntity> entityList = new List<TEntity>();
                for (var r = 0; r < table.Rows.Count; r++)
                {
                    TEntity entity = new TEntity();
                    PropertyInfo[] properties = GetEntityProperties(entity);
                    DataRow row = table.Rows[r];
                    for (var i = 0; i < table.Columns.Count; i++)
                    {
                        String colName = table.Columns[i].ColumnName;
                        PropertyInfo property = GetPropertyInfoByColumnName(colName, properties);
                        if (property == null) continue;
                        property.SetValue(entity, FormatColumnValue(property, row[i]), null);
                    }
                    entityList.Add(entity);
                    //if (onlyTopOne) return entityList;
                }
                return entityList;
            }
            return null;
        }

        /// <summary>
        /// 获取实体对象
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        //public virtual IList<TEntity> GetEntities<TEntity>(String sql, List<DbParameter> paramList) where TEntity : class,new()
        //{
        //    return GetEntities<TEntity>(sql, paramList, false);
        //}

        ///// <summary>
        ///// 获取实体对象
        ///// </summary>
        ///// <typeparam name="TEntity"></typeparam>
        ///// <param name="sql"></param>
        ///// <returns></returns>
        //public virtual TEntity GetTopOneEntity<TEntity>(String sql, List<DbParameter> paramList) where TEntity : class,new()
        //{
        //    IList<TEntity> entities = GetEntities<TEntity>(sql, paramList, true);
        //    if (entities != null && entities.Count > 0) return entities[0];
        //    return null;
        //}

        #endregion

        #region "protected方法"

        /// <summary>
        /// 根据sql获取datatable对象
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        protected virtual DataTable GetDataTable(String sql, List<DbParameter> paramList)
        {
            DbCommand command = CreateCommand(sql, CommandType.Text);
            if (paramList != null && paramList.Count > 0)
                command.Parameters.AddRange(paramList.ToArray());
            //return command.ExecuteNonQuery();
            return ExecuteDatatable(command);
        }

        /// <summary>
        /// 写日记
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        internal protected virtual void WriteLog(String sql, List<DbParameter> paramList)
        {
            Helper.WriteSql(sql, paramList);
        }

        /// <summary>
        /// 创建数据连接
        /// </summary>
        /// <returns></returns>
        protected virtual DbConnection GetConnection()
        {
            return ConnectionProvider.GetConnection(true);
        }

        /// <summary>
        /// 创建数据库连接provider对象
        /// </summary>
        /// <returns></returns>
        protected virtual ConnectionProvider CreateConnectionProvider()
        {
            return new ConnectionProvider(this._connStrProvider.GetConnectionStr(), this._dbProviderFactory);
        }

        /// <summary>
        /// 创建command对象
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        //protected virtual DbCommand CreateCommand(string commandText, CommandType commandType = CommandType.Text, DbTransaction trans = null)
        protected virtual DbCommand CreateCommand(string commandText, CommandType commandType = CommandType.Text)
        {
            DbCommand command = _dbProviderFactory.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            //if (trans != null) command.Transaction = trans;
            return command;
        }

        /// <summary>
        /// 不同的数据库可以直接重写这个方法（其他逻辑基本上不用重写）
        /// </summary>
        /// <param name="paramList"></param>
        /// <returns></returns>
        protected virtual IEnumerable<DbParameter> GetReturnValueParam(List<DbParameter> paramList)
        {
            if (paramList == null) return null;
            return paramList.Where((it) => it.Direction == ParameterDirection.ReturnValue);
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual PropertyInfo[] GetEntityProperties(Object entity)
        {
            if (entity == null) return null;
            Type t = entity.GetType();
            return t.GetProperties();
        }

        /// <summary>
        /// 把returnvalue的参数值写回到实体对象
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="paramList"></param>
        protected virtual void SetReturnValueToEntity(Object entity, List<DbParameter> paramList)
        {
            if (paramList == null) return;
            IEnumerable<DbParameter> returnParams = GetReturnValueParam(paramList);
            PropertyInfo[] properties = GetEntityProperties(entity);
            if (properties == null || properties.Length == 0) return;
            foreach (DbParameter param in returnParams)
            {
                PropertyInfo property = GetPropertyInfoByReturnParamName(param.ParameterName, properties);
                if (property == null) continue;
                //SetParamValueToEntity(entity, property, param.Value);
                property.SetValue(entity, GetParamValueWithRightType(property, param), null);
            }
        }

        /// <summary>
        /// 根据变量名，获取变量对应的实体属性
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected virtual PropertyInfo GetPropertyInfoByColumnName(String colName, PropertyInfo[] properties)
        {
            if (String.IsNullOrWhiteSpace(colName) || properties == null || properties.Length == 0)
                return null;
            return properties.SingleOrDefault(it => String.Compare(it.Name, colName, StringComparison.CurrentCultureIgnoreCase) == 0);
        }

        /// <summary>
        /// 根据变量名，获取变量对应的实体属性
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected virtual PropertyInfo GetPropertyInfoByReturnParamName(String paramName, PropertyInfo[] properties)
        {
            String propertyName = SqlBuilder.ReturnParamNameToPropertyName(paramName);
            if (String.IsNullOrWhiteSpace(paramName) || properties == null || properties.Length == 0)
                return null;
            return properties.SingleOrDefault(it => String.Compare(it.Name, propertyName, StringComparison.CurrentCultureIgnoreCase) == 0);
        }

        /// <summary>
        /// 派生类可以重写此方法获得数据的转换
        /// </summary>
        /// <param name="property"></param>
        /// <param name="dbParam"></param>
        /// <returns></returns>
        protected virtual Object GetParamValueWithRightType(PropertyInfo property, DbParameter dbParam)
        {
            //最有可能是真的条件放在最前
            if (dbParam.Value == DBNull.Value || property == null || dbParam == null || dbParam.Value == null)
                return null;
            return dbParam.Value;
        }

        /// <summary>
        /// 派生类可以重写此方法获得数据的转换
        /// </summary>
        /// <param name="property"></param>
        /// <param name="columnValue"></param>
        /// <returns></returns>
        protected virtual Object FormatColumnValue(PropertyInfo property, Object columnValue)
        {
            //最有可能是真的条件放在最前
            if (columnValue == DBNull.Value || columnValue == null || property == null)
                return null;
            return columnValue;
        }

        /// <summary>
        /// 初始化命令对象（配置连接对象）
        /// </summary>
        /// <param name="command"></param>
        protected void InitalCommand(DbCommand command)
        {
            if (command.Connection == null)
                command.Connection = ConnectionProvider.GetConnection(true);
            if (command.Connection.State != ConnectionState.Open)
                command.Connection.Open();
            DbTransaction trans = CurrentContext.GetCacheItem<DbTransaction>(_currentContextTransCacheKey);
            if (trans != null)
                command.Transaction = trans;
        }

        /// <summary>
        /// 已丢弃
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <param name="dbParamValue"></param>
        [Obsolete]
        protected virtual void SetParamValueToEntity(Object entity, PropertyInfo property, Object dbParamValue)
        {
            if (entity == null || property == null || dbParamValue == null || dbParamValue == DBNull.Value)
                return;
            property.SetValue(entity, dbParamValue, null);
        }

        #endregion

        public IDBQuery<TResult> Select<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
            where TSource : class
            where TResult : class
        {
            throw new NotImplementedException();
        }

        public IDBQuery<TResult> SelectByDeletion<TSource, TResult>(Expression<Func<TSource, TResult>> deletion)
            where TSource : class
            where TResult : class
        {
            throw new NotImplementedException();
        }


        public IDBQuery<dynamic> Select<TSource>(Expression<Func<TSource, dynamic>> selector) where TSource : class
        {
            throw new NotImplementedException();
        }

        public IDBQuery<dynamic> SelectByDeletion<TSource>(Expression<Func<TSource, dynamic>> deletion) where TSource : class
        {
            throw new NotImplementedException();
        }
    }
}
