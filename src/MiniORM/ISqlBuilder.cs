using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MiniORM
{
    public interface ISqlBuilder
    {
        IExpressionToSql ExpressionToSql
        {
            get;
        }

        /// <summary>
        /// 构造insert语句
        /// </summary>
        /// <typeparam name="TDBParam"></typeparam>
        /// <param name="entity"></param>
        /// <param name="returns"></param>
        /// <param name="sql"></param>
        /// <param name="dbParams"></param>
        /// <returns></returns>
        void BuildInsert<TEntity>(Object entity, Expression<Func<TEntity, dynamic>> returns, out String sql, out List<DbParameter> dbParams) where TEntity : class;

        /// <summary>
        /// 构造update语句
        /// </summary>
        /// <typeparam name="TDBParam"></typeparam>
        /// <param name="entity"></param>
        /// <param name="sql"></param>
        /// <param name="dbParams"></param>
        void BuildUpdate<TEntity>(TEntity entity, Expression<Func<TEntity, Boolean>> predicate, out String sql, out List<DbParameter> dbParams);

        /// <summary>
        /// 构造select语句
        /// </summary>
        /// <typeparam name="query"></typeparam>
        /// <param name="sql"></param>
        /// <param name="dbParams"></param>
        void BuildSelect<TEntity>(IDBQuery<TEntity> query, out String sql, out List<DbParameter> dbParams) where TEntity : class,new();

        /// <summary>
        /// 打包sql（底层不考虑注入，业务层自己处理，不要随便用用户的输入直接调用此方法即可）
        /// </summary>
        /// <param name="sqlSegment">sql方法或者是sql语句</param>
        /// <returns></returns>
        String PackSqlSegment(String sqlSegment);

        /// <summary>
        /// 把数据库returnvalue类型的参数名转换为实体属性名
        /// 重写此方法必须同时重写方法：PropertyNameToReturnParamName
        /// </summary>
        /// <param name="returnParamName"></param>
        /// <returns></returns>
        String ReturnParamNameToPropertyName(String returnParamName);

        /// <summary>
        /// 把实体属性名转换为数据库returnvalue类型的参数名
        /// 重写此方法必须同时重写方法：ReturnParamNameToPropertyName
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        String PropertyNameToReturnParamName(String propertyName);
    }
}
