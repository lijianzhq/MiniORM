using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.Common;

namespace MiniORM
{
    public abstract class BasicSqlBuilder : ISqlBuilder
    {
        protected const String _sqlSegmentPackKeyt = "63F4A04F0DED7E6CE0530100007FE980";
        protected const String _sqlSegmentPrefix = "sqlSegment:";

        #region "属性"

        //不用缓存的方式，没办法确保已丢弃的数据都会被移除
        //protected Dictionary<String, String> _sqlSegmentDic = new Dictionary<String, String>();
        //这样总是感觉在循环引用，所以去掉这种方式，冗余实现方法
        //protected IDB _dbInstance = null;
        protected DbProviderFactory _dbProviderFactory;

        /// <summary>
        /// 加密器
        /// </summary>
        private IEncryptWorker _encryptWorker;
        protected virtual IEncryptWorker EncryptWorker
        {
            get
            {
                if (_encryptWorker == null)
                    _encryptWorker = new EncryptWorker(_sqlSegmentPackKeyt);
                return _encryptWorker;
            }
        }

        /// <summary>
        /// 表达式转换器
        /// </summary>
        private IExpressionToSql _expressionToSql;
        public IExpressionToSql ExpressionToSql
        {
            get
            {
                if (_expressionToSql == null)
                    _expressionToSql = CreateExpressionToSql();
                return _expressionToSql;
            }
        }

        #endregion

        #region "构造方法"

        //public BasicSqlBuilder(IDB dbInstance)
        //{
        //    _dbInstance = dbInstance;
        //    _encryptWorker = new EncryptWorker(_sqlSegmentPackKeyt);
        //}

        public BasicSqlBuilder(DbProviderFactory dbProviderFactory)
        {
            _dbProviderFactory = dbProviderFactory;
            //_encryptWorker = new EncryptWorker(_sqlSegmentPackKeyt);
        }

        #endregion

        #region "抽象方法"

        /// <summary>
        /// 获取变量的前缀
        /// </summary>
        /// <returns></returns>
        protected abstract String GetParamPrefix();

        public abstract void BuildUpdate<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> predicate, out string sql, out List<DbParameter> dbParams);


        public abstract void BuildInsert<TEntity>(object entity,
                                                  Expression<Func<TEntity, dynamic>> returns,
                                                  out string sql,
                                                  out List<DbParameter> dbParams) where TEntity : class;

        public abstract void BuildSelect<TEntity>(IDBQuery<TEntity> query, out string sql, out List<DbParameter> dbParams) where TEntity : class,new();

        #endregion

        #region "公共方法"

        /// <summary>
        /// 把实体属性名转换为数据库returnvalue类型的参数名
        /// 重写此方法必须同时重写方法：ReturnParamNameToPropertyName
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual String PropertyNameToReturnParamName(String propertyName)
        {
            propertyName = "r_" + propertyName;
            return FormatToParamName(propertyName);
        }

        /// <summary>
        /// 把数据库returnvalue类型的参数名转换为实体属性名
        /// 重写此方法必须同时重写方法：PropertyNameToReturnParamName
        /// </summary>
        /// <param name="returnParamName"></param>
        /// <returns></returns>
        public virtual String ReturnParamNameToPropertyName(String returnParamName)
        {
            String prefix = GetParamPrefix();
            return returnParamName.Substring("r_".Length + (prefix == null ? 0 : prefix.Length));
        }

        /// <summary>
        /// 打包sql（底层不考虑注入，业务层自己处理，不要随便用用户的输入直接调用此方法即可）
        /// </summary>
        /// <param name="sqlSegment"></param>
        /// <returns></returns>
        public virtual String PackSqlSegment(String sqlSegment)
        {
            if (!String.IsNullOrWhiteSpace(sqlSegment))
            {
                String key = String.Format("{0}{1}", _sqlSegmentPrefix, EncryptWorker.Encrypt(sqlSegment));
                return key;
            }
            return sqlSegment;
        }

        /// <summary>
        /// 构造查询语句
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="sql"></param>
        /// <param name="dbParams"></param>
        //public virtual void BuildSelect<TEntity>(IDBQuery<TEntity> query, out string sql, out List<DbParameter> dbParams) where TEntity : class,new()
        //{
        //    dbParams = new List<DbParameter>();
        //    StringBuilder whereStrBuilder = new StringBuilder();
        //    foreach (Expression exp in query.WhereExpression)
        //    {
        //        StringBuilder sqlStrBuilder = new StringBuilder();
        //        List<DbParameter> paramList = new List<DbParameter>();
        //        ExpressionToSql.BuildWhere(exp, ref sqlStrBuilder, ref paramList);
        //        if (paramList != null) dbParams.AddRange(paramList);
        //        if (whereStrBuilder.Length > 0) whereStrBuilder.Append(" AND ");
        //        whereStrBuilder.AppendFormat("({0})", sqlStrBuilder);
        //    }
        //    if (whereStrBuilder.Length > 0) whereStrBuilder.Insert(0, "where ");
        //    sql = String.Format("SELECT * FROM {0} {1}", GetTableName<TEntity>(), whereStrBuilder.ToString());
        //}

        ///// <summary>
        ///// 打包sql
        ///// </summary>
        ///// <param name="packKeyt">传入的方法密钥，密钥对了才会对sql进行打包（此方法可以注入sql的，所以要做控制，内部使用）</param>
        ///// <param name="sqlSegment"></param>
        ///// <returns></returns>
        //public virtual String PackSqlSegment(String packKeyt, String sqlSegment)
        //{
        //    //if (packKeyt == _sqlSegmentPackKeyt && !String.IsNullOrWhiteSpace(sqlSegment))
        //    //{
        //    //    String md5 = MD5Helper.EncryptMD5(sqlSegment.GetHashCode() + DateTime.Now.ToString("yyyyMMddHHmmssffff") + _sqlSegmentDic.Count);
        //    //    String key = String.Format("{0}:{1}", _sqlSegmentPrefix, md5);
        //    //    _sqlSegmentDic.Add(key, sqlSegment);
        //    //    return key;
        //    //}
        //    //return sqlSegment;
        //    if (packKeyt == _sqlSegmentPackKeyt && !String.IsNullOrWhiteSpace(sqlSegment))
        //    {
        //        String key = String.Format("{0}:{1}", _sqlSegmentPrefix, EncryptWorker.Encrypt(sqlSegment));
        //        return key;
        //    }
        //    return sqlSegment;
        //}

        #endregion

        #region "protected方法"

        /// <summary>
        /// 构造表达式转换为sql的实例对象
        /// </summary>
        /// <returns></returns>
        protected virtual IExpressionToSql CreateExpressionToSql()
        {
            return new ExpressionToSql(this);
        }

        /// <summary>
        /// 解包sql片段
        /// </summary>
        /// <param name="sqlSegment"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected virtual Boolean TryUnPackSqlSegment(String sqlSegment, out String sql)
        {
            //if (sqlSegment != null && sqlSegment.StartsWith(_sqlSegmentPrefix))
            //{
            //    if (_sqlSegmentDic.TryGetValue(sqlSegment, out sql))
            //    {
            //        _sqlSegmentDic.Remove(_sqlSegmentPackKeyt);
            //        return true;
            //    }
            //}
            //sql = sqlSegment;
            //return false;

            if (sqlSegment != null && sqlSegment.StartsWith(_sqlSegmentPrefix))
            {
                String sqlSegmentEncryptStr = sqlSegment.Substring(_sqlSegmentPrefix.Length);
                if (!String.IsNullOrWhiteSpace(sqlSegmentEncryptStr))
                {
                    sql = _encryptWorker.Decrypt(sqlSegmentEncryptStr);
                    return true;
                }
            }
            sql = sqlSegment;
            return false;
        }

        /// <summary>
        /// 获取实体映射到数据表的所有属性集合
        /// 可以重写此方法实现忽略实体的某些属性
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual PropertyInfo[] GetEntityMappingToDBColumnProperty(Object entity)
        {
            if (entity == null) return null;
            Type t = entity.GetType();
            return t.GetProperties();
        }

        /// <summary>
        /// 根据属性值，转换为对应的sql
        /// </summary>
        /// <typeparam name="TDBParam"></typeparam>
        /// <param name="entity"></param>
        /// <param name="p"></param>
        /// <param name="val"></param>
        /// <param name="dbParam">参数（可能为空，如果方法返回的字符串是参数名，则此参数返回对应的参数对象）</param>
        /// <returns>sql语句或者参数名</returns>
        protected virtual String ParsePropertyToSql(object entity, PropertyInfo p, Object val, out DbParameter dbParam)
        {
            dbParam = null;
            if (p.PropertyType == typeof(String))
            {
                //如果是打包的sql，则直接返回sql
                String sql;
                if (TryUnPackSqlSegment(Convert.ToString(val), out sql))
                    return sql;
            }
            String paramName = FormatToParamName(p.Name);
            //dbParam = _dbInstance.CreateParameter(paramName, val);
            dbParam = CreateParameter(paramName, val);
            return paramName;
        }

        /// <summary>
        /// 根据实体获取表名（可以重写逻辑）
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual String GetTableName(object entity)
        {
            if (entity == null) return String.Empty;
            return entity.GetType().Name;
        }

        /// <summary>
        /// 根据实体获取表名（可以重写逻辑）
        /// </summary>
        /// <returns></returns>
        protected virtual String GetTableName<TEntity>()
        {
            return typeof(TEntity).Name;
        }

        /// <summary>
        /// 根据实体和属性获取列名（可以重写逻辑）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="p"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        protected virtual String GetColumnName(object entity, PropertyInfo p, Object val)
        {
            return p.Name;
        }

        /// <summary>
        /// 把名字格式化成参数名的格式，例如 :xxx @xxxx
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal protected virtual String FormatToParamName(String name)
        {
            return String.Format("{0}{1}", GetParamPrefix(), name);
        }

        /// <summary>
        /// 创建变量
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        internal protected virtual DbParameter CreateParameter(string name,
                                                               Object value,
                                                               ParameterDirection direction = ParameterDirection.Input,
                                                               Int32? size = null)
        {
            DbParameter parameter = _dbProviderFactory.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            parameter.Direction = direction;
            if (size.HasValue) parameter.Size = size.Value;
            return parameter;
        }

        /// <summary>
        /// 构造查询语句
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="columnsStr"></param>
        /// <param name="whereStr"></param>
        /// <param name="dbParams"></param>
        protected virtual void BuildSelect<TEntity>(IDBQuery<TEntity> query,
                                                    out string columnsStr,
                                                    out string whereStr,
                                                    out string orderByStr,
                                                    out List<DbParameter> dbParams) where TEntity : class,new()
        {
            columnsStr = String.Empty;
            whereStr = String.Empty;
            orderByStr = String.Empty;
            dbParams = new List<DbParameter>();
            StringBuilder whereStrBuilder = new StringBuilder();
            StringBuilder orderByStrBuilder = new StringBuilder();
            var implementIf = DBQueryExtension.ChangeInterface(query);
            //构造where
            foreach (Expression exp in implementIf.WhereExpression)
            {
                StringBuilder sqlStrBuilder = new StringBuilder();
                List<DbParameter> paramList = new List<DbParameter>();
                ExpressionToSql.BuildWhere(exp, ref sqlStrBuilder, ref paramList);
                if (paramList != null) dbParams.AddRange(paramList);
                if (whereStrBuilder.Length > 0) whereStrBuilder.Append(" AND ");
                whereStrBuilder.AppendFormat("({0})", sqlStrBuilder);
            }
            //构造orderby
            foreach (OrderByItem orderby in implementIf.OrderByItem)
            {
                StringBuilder sqlStrBuilder = new StringBuilder();
                List<DbParameter> paramList = new List<DbParameter>();
                ExpressionToSql.BuildOrderBy(orderby.Expression, ref sqlStrBuilder, orderby.Desc);
                if (paramList != null) dbParams.AddRange(paramList);
                if (orderByStrBuilder.Length > 0) orderByStrBuilder.Append(",");
                orderByStrBuilder.AppendFormat("{0}", sqlStrBuilder);
            }
            //构造left join
            if (whereStrBuilder.Length > 0) whereStrBuilder.Insert(0, "WHERE ");
            if (orderByStrBuilder.Length > 0) orderByStrBuilder.Insert(0, "ORDER BY ");
            columnsStr = "*";
            whereStr = whereStrBuilder.ToString();
            orderByStr = orderByStrBuilder.ToString();
            //sql = String.Format("SELECT * FROM {0} {1}", GetTableName<TEntity>(), whereStrBuilder.ToString());
        }

        protected virtual void BuildUpdate<TEntity>(TEntity entity,
                                                    Expression<Func<TEntity, bool>> predicate,
                                                    out string columnsStr,
                                                    out string whereStr,
                                                    out List<DbParameter> dbParams)
        {
            columnsStr = String.Empty;
            whereStr = String.Empty;
            dbParams = null;
            if (entity == null) return;
            Type t = entity.GetType();
            PropertyInfo[] properties = GetEntityMappingToDBColumnProperty(entity);
            if (properties.Length == 0) return;
            //StringBuilder sqlBuilder = new StringBuilder();
            StringBuilder columnsBuilder = new StringBuilder();
            dbParams = new List<DbParameter>();
            foreach (PropertyInfo p in properties)
            {
                Object pValue = p.GetValue(entity, null);
                //null值不作处理
                if (pValue == null) continue;
                DbParameter dbParam = null;
                String propertySql = ParsePropertyToSql(entity, p, pValue, out dbParam);
                //参数表
                if (dbParam != null)
                {
                    dbParams.Add(dbParam);
                    columnsBuilder.AppendFormat("{0}={1},", GetColumnName(entity, p, pValue), propertySql);
                }
                else
                {
                    columnsBuilder.AppendFormat("{0}=({1}),", GetColumnName(entity, p, pValue), propertySql);
                }
            }
            columnsStr = columnsBuilder.ToString();
            //去掉最后一个逗号
            columnsStr = columnsStr.Substring(0, columnsStr.Length - 1);
            //构造where条件语句
            StringBuilder whereStringBuilder = new StringBuilder();
            ExpressionToSql.BuildWhere(predicate, ref whereStringBuilder, ref dbParams);
            whereStr = whereStringBuilder.ToString();
        }

        protected virtual void BuildInsert<TEntity>(object entity,
                                                    Expression<Func<TEntity, dynamic>> returns,
                                                    out string columnsStr,
                                                    out string paramsStr,
                                                    out string returnStr,
                                                    out List<DbParameter> dbParams) where TEntity : class
        {
            columnsStr = String.Empty;
            paramsStr = String.Empty;
            returnStr = String.Empty;
            dbParams = null;
            if (entity == null) return;
            Type t = entity.GetType();
            PropertyInfo[] properties = GetEntityMappingToDBColumnProperty(entity);
            if (properties.Length == 0) return;
            //StringBuilder sqlBuilder = new StringBuilder();
            StringBuilder columnsBuilder = new StringBuilder();
            StringBuilder paramsBuilder = new StringBuilder();
            dbParams = new List<DbParameter>();
            foreach (PropertyInfo p in properties)
            {
                Object pValue = p.GetValue(entity, null);
                //null值不作处理
                if (pValue == null) continue;
                //插入列
                columnsBuilder.Append(GetColumnName(entity, p, pValue) + ",");
                DbParameter dbParam = null;
                //插入值
                paramsBuilder.Append(ParsePropertyToSql(entity, p, pValue, out dbParam) + ",");
                //添加参数到参数表
                if (dbParam != null)
                    dbParams.Add(dbParam);
            }
            //把构造的字符串返回去
            columnsStr = columnsBuilder.ToString();
            paramsStr = paramsBuilder.ToString();
            //去掉最后一个逗号
            columnsStr = columnsStr.Substring(0, columnsStr.Length - 1);
            paramsStr = paramsStr.Substring(0, paramsStr.Length - 1);
            //处理return语句
            StringBuilder returnSqlStrBuilder = new StringBuilder();
            ExpressionToSql.BuildReturn(returns, ref returnSqlStrBuilder, ref dbParams);
            if (returnSqlStrBuilder.Length > 0)
                returnStr = returnSqlStrBuilder.ToString();
        }

        #endregion

    }
}
