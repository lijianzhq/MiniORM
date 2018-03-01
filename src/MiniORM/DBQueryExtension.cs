using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.Common;

namespace MiniORM
{
    public static class DBQueryExtension
    {
        /// <summary>
        /// 把外部接口转换实现接口
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        internal static IDBQuery_Implement<TEntity> ChangeInterface<TEntity>(IDBQuery<TEntity> query) where TEntity : class
        {
            return ((IDBQuery_Implement<TEntity>)query);
        }

        /// <summary>
        /// where语句
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IDBQuery<TEntity> Where<TEntity>(this IDBQuery<TEntity> query, Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            StringBuilder sqlBuilder = new StringBuilder();
            ChangeInterface(query).WhereExpression.Add(predicate);
            return query;
        }

        /// <summary>
        /// 升序排序
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IDBQuery<TEntity> OrderBy<TEntity>(this IDBQuery<TEntity> query, Expression<Func<TEntity, dynamic>> selector) where TEntity : class
        {
            StringBuilder sqlBuilder = new StringBuilder();
            ChangeInterface(query).OrderByItem.Add(new OrderByItem()
            {
                Expression = selector,
                Desc = false
            });
            return query;
        }

        /// <summary>
        /// 降序排序
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IDBQuery<TEntity> OrderByDesc<TEntity>(this IDBQuery<TEntity> query, Expression<Func<TEntity, dynamic>> selector) where TEntity : class
        {
            StringBuilder sqlBuilder = new StringBuilder();
            ChangeInterface(query).OrderByItem.Add(new OrderByItem()
            {
                Expression = selector,
                Desc = true
            });
            return query;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="startRowNum">起始行号（序号从1开始的）</param>
        /// <param name="endRowNum">结束行号（序号从1开始的）</param>
        /// <returns></returns>
        public static IList<TEntity> Take<TEntity>(this IDBQuery<TEntity> query, UInt16 startRowNum, UInt16 endRowNum) where TEntity : class,new()
        {
            if (startRowNum < 1) throw new ArgumentException("startRowNum must start with 1!");
            if (endRowNum < 1 || endRowNum < startRowNum) throw new ArgumentException("endRowNum must start with 1,and not less than startRowNum!");
            //配置分页的信息
            var implementIf = ChangeInterface(query);
            implementIf.StartRowNum = startRowNum;
            implementIf.EndRowNum = endRowNum;
            String sql = String.Empty;
            List<DbParameter> paramList;
            implementIf.DB.SqlBuilder.BuildSelect(query, out sql, out paramList);
            Helper.WriteSql(sql, paramList);
            return implementIf.DB.GetEntities<TEntity>(sql, paramList);
        }

        /// <summary>
        /// join查询（outer join）
        /// 
        /// 这里遇到一个情况，记录理解：innerKeySelector和outerKeySelector两个参数顺序一定不能调换，
        /// 因为TKey类型还没办法推测，所以如果调换的话，outerKeySelector这个参数类型没办法推测，代码提示会一直报错
        /// </summary>
        /// <typeparam name="TInner">查询对象</typeparam>
        /// <typeparam name="TOuter">join的表对象</typeparam>
        /// <typeparam name="TKey">比较列类型</typeparam>
        /// <typeparam name="TResult">返回的对象类型</typeparam>
        /// <param name="query">查询对象</param>
        /// <param name="outer">关联的对象</param>
        /// <param name="innerKeySelector">查询的表对象的比较列，也就是on列</param>
        /// <param name="outerKeySelector">joins的表对象的比较列，也就是on列</param>
        /// <param name="resultSelector">返回的结果</param>
        /// <returns></returns>
        public static IDBQuery<TResult> Join<TInner, TOuter, TKey, TResult>(this IDBQuery<TInner> query,
                                                                            TOuter outer,
                                                                            Expression<Func<TInner, TKey>> innerKeySelector,
                                                                            Expression<Func<TOuter, TKey>> outerKeySelector,
                                                                            Expression<Func<TInner, TOuter, TResult>> resultSelector)
            where TInner : class
            where TOuter : class
        {
            throw new NotImplementedException();
            var implementIf = ChangeInterface(query);
        }

        //public static ITable<TResult> Join<TOuter, TInner, TKey, TResult>(
        //                                this IDBQuery<TOuter> outer,
        //                                TInner inner,
        //                                Expression<Func<TOuter, TKey>> outerKeySelector,
        //                                Expression<Func<TInner, TKey>> innerKeySelector,
        //                                Expression<Func<TOuter, TInner, TResult>> resultSelector)
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// join查询（inner join）
        ///// </summary>
        ///// <typeparam name="TInner">查询对象</typeparam>
        ///// <typeparam name="TOuter">join的表对象</typeparam>
        ///// <typeparam name="TKey">比较列类型</typeparam>
        ///// <typeparam name="TResult">返回的对象类型</typeparam>
        ///// <param name="query">查询对象</param>
        ///// <param name="outerKeySelector">joins的表对象的比较列，也就是on列</param>
        ///// <param name="innerKeySelector">查询的表对象的比较列，也就是on列</param>
        ///// <param name="resultSelector">返回的结果</param>
        ///// <returns></returns>
        //public static IList<TResult> InnerJoin<TInner, TOuter, TKey, TResult>(this IDBQuery<TInner> query,
        //                                                                      Expression<Func<TOuter, TKey>> outerKeySelector,
        //                                                                      Expression<Func<TInner, TKey>> innerKeySelector,
        //                                                                      Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector) where TInner : class,new()
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// join查询（outer join）
        ///// </summary>
        ///// <typeparam name="TInner"></typeparam>
        ///// <typeparam name="TOuter"></typeparam>
        ///// <typeparam name="TOterResult"></typeparam>
        ///// <typeparam name="TKey"></typeparam>
        ///// <typeparam name="TResult"></typeparam>
        ///// <param name="query"></param>
        ///// <param name="outerSelector">join的表返回的列</param>
        ///// <param name="outerKeySelector"></param>
        ///// <param name="innerKeySelector"></param>
        ///// <param name="resultSelector"></param>
        ///// <returns></returns>
        //public static IList<TResult> Join<TInner, TOuter, TOuterResult, TKey, TResult>(this IDBQuery<TInner> query,
        //                                                                Expression<Func<TOuter, TOuterResult>> outerSelector,
        //                                                                Expression<Func<TOuter, TKey>> outerKeySelector,
        //                                                                Expression<Func<TInner, TKey>> innerKeySelector,
        //                                                                Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector) where TInner : class,new()
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// join查询（inner join）
        ///// </summary>
        ///// <typeparam name="TInner"></typeparam>
        ///// <typeparam name="TOuter"></typeparam>
        ///// <typeparam name="TOterResult"></typeparam>
        ///// <typeparam name="TKey"></typeparam>
        ///// <typeparam name="TResult"></typeparam>
        ///// <param name="query"></param>
        ///// <param name="outerSelector">join的表返回的列</param>
        ///// <param name="outerKeySelector"></param>
        ///// <param name="innerKeySelector"></param>
        ///// <param name="resultSelector"></param>
        ///// <returns></returns>
        //public static IList<TResult> InnerJoin<TInner, TOuter, TOuterResult, TKey, TResult>(this IDBQuery<TInner> query,
        //                                                                      Expression<Func<TOuter, TOuterResult>> outerSelector,
        //                                                                      Expression<Func<TOuter, TKey>> outerKeySelector,
        //                                                                      Expression<Func<TInner, TKey>> innerKeySelector,
        //                                                                      Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector) where TInner : class,new()
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// 执行查询操作（只返回一条记录）
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static TEntity SingleOrDefault<TEntity>(this IDBQuery<TEntity> query) where TEntity : class,new()
        {
            //配置分页的信息
            var implementIf = ChangeInterface(query);
            implementIf.StartRowNum = 1;
            implementIf.EndRowNum = 1;
            String sql = String.Empty;
            List<DbParameter> paramList;
            implementIf.DB.SqlBuilder.BuildSelect(query, out sql, out paramList);
            Helper.WriteSql(sql, paramList);
            var entities = implementIf.DB.GetEntities<TEntity>(sql, paramList);
            if (entities == null || entities.Count == 0) return default(TEntity);
            return entities[0];
        }

        /// <summary>
        /// 执行查询操作（返回所有查询结果）
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IList<TEntity> ToList<TEntity>(this IDBQuery<TEntity> query) where TEntity : class,new()
        {
            String sql = String.Empty;
            List<DbParameter> paramList;
            var implementIf = ChangeInterface(query);
            implementIf.DB.SqlBuilder.BuildSelect(query, out sql, out paramList);
            Helper.WriteSql(sql, paramList);
            return implementIf.DB.GetEntities<TEntity>(sql, paramList);
        }
    }
}
