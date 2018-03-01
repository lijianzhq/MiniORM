using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MiniORM
{
    /// <summary>
    /// 需要传入事务对象的方法参数，可以把事务对象参数移除的，以后再优化
    /// </summary>
    public interface IDB
    {
        ISqlBuilder SqlBuilder
        {
            get;
        }

        /// <summary>
        /// 添加记录，并返回影响行数
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        Int32 Add<TEntity>(TEntity t) where TEntity : class;

        ///// <summary>
        ///// 添加记录，并返回影响行数（以事务的方式执行）
        ///// </summary>
        ///// <typeparam name="TEntity"></typeparam>
        ///// <param name="t"></param>
        ///// <param name="trans"></param>
        ///// <returns></returns>
        //Int32 Add<TEntity>(TEntity t, DbTransaction trans) where TEntity : class;

        /// <summary>
        /// 添加记录，并返回影响行数
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="t"></param>
        /// <param name="returns"></param>
        /// <returns></returns>
        Int32 AddWithReturn<TEntity>(TEntity t, Expression<Func<TEntity, dynamic>> returns) where TEntity : class;

        ///// <summary>
        ///// 添加记录，并返回影响行数
        ///// </summary>
        ///// <typeparam name="TEntity"></typeparam>
        ///// <param name="t"></param>
        ///// <param name="returns"></param>
        ///// <returns></returns>
        //Int32 AddWithReturn<TEntity>(TEntity t, Expression<Func<TEntity, dynamic>> returns, DbTransaction trans) where TEntity : class;

        /// <summary>
        /// 更新记录，并返回影响行数
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="predicate">where条件语句（update语句默认都需要添加条件语句）</typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        Int32 Update<TEntity>(TEntity t, Expression<Func<TEntity, bool>> predicate) where TEntity : class;

        ///// <summary>
        ///// 更新记录，并返回影响行数（以事务的方式执行）
        ///// </summary>
        ///// <typeparam name="TEntity"></typeparam>
        ///// <param name="t"></param>
        ///// <typeparam name="predicate">where条件语句（update语句默认都需要添加条件语句）</typeparam>
        ///// <param name="trans"></param>
        ///// <returns></returns>
        //Int32 Update<TEntity>(TEntity t, Expression<Func<TEntity, bool>> predicate, DbTransaction trans) where TEntity : class;

        /// <summary>
        /// 执行命令并返回影响的行数
        /// </summary>
        /// <param name="command"></param>
        /// <param name="autoCloseConnection">
        /// 指示是否自动关闭连接，如果指定自动关闭连接，则无论是否在事务中，都会直接关闭连接对象，关闭连接对象事务会直接commit；
        /// 如果不指定，则会根据上下文来自动判断，如果当前是处于事务上下文中，则不会自动关闭连接，由事务最终关闭。
        /// </param>
        /// <returns></returns>
        Int32 ExecuteNonQuery(DbCommand command, Boolean? autoCloseConnection = null);

        /// <summary>
        /// 在事务上下文中执行指令（内部会自动提交事务；如果方法发生异常，内部自动回滚事务）
        /// 这个只支持使用IDB的操作来执行数据库才有效的，自己创建数据库命令来执行等等是无效的。
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        void ExcuteInTransaction(Action a);

        /// <summary>
        /// 查询对象（整表查询）
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IDBQuery<TEntity> Select<TEntity>() where TEntity : class;

        /// <summary>
        /// 查询对象（指定查询列）
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector">指定需要返回的列</param>
        /// <returns></returns>
        IDBQuery<TResult> Select<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
            where TSource : class
            where TResult : class;

        ///// <summary>
        ///// 查询对象（指定查询列）
        ///// </summary>
        ///// <typeparam name="TSource"></typeparam>
        ///// <typeparam name="TResult"></typeparam>
        ///// <param name="deletion">指定不要返回的列</param>
        ///// <returns></returns>
        //IDBQuery<TResult> SelectByDeletion<TSource, TResult>(Expression<Func<TSource, TResult>> deletion)
        //    where TSource : class
        //    where TResult : class;

        ///// <summary>
        ///// 查询对象（指定查询列）
        ///// </summary>
        ///// <typeparam name="TSource"></typeparam>
        ///// <param name="selector">指定需要返回的列</param>
        ///// <returns></returns>
        //IDBQuery<dynamic> Select<TSource>(Expression<Func<TSource, dynamic>> selector)
        //    where TSource : class;

        ///// <summary>
        ///// 查询对象（指定查询列）
        ///// </summary>
        ///// <typeparam name="TSource"></typeparam>
        ///// <param name="deletion">指定不要返回的列</param>
        ///// <returns></returns>
        //IDBQuery<dynamic> SelectByDeletion<TSource>(Expression<Func<TSource, dynamic>> deletion)
        //    where TSource : class;

        IList<TEntity> GetEntities<TEntity>(String sql, List<DbParameter> paramList) where TEntity : class,new();
        //TEntity GetTopOneEntity<TEntity>(String sql, List<DbParameter> paramList) where TEntity : class,new();

        ///// <summary>
        ///// 创建一个命令对象
        ///// </summary>
        ///// <param name="commandText">命令文本</param>
        ///// <param name="commandType">命令类型</param>
        ///// <returns></returns>
        //DbCommand CreateCommand(String commandText, CommandType commandType = CommandType.Text);

        ///// <summary>
        ///// 创建一个命令参数对象
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="dbType"></param>
        ///// <param name="size"></param>
        ///// <param name="direction"></param>
        ///// <param name="nullable">该值指示参数是否接受 null 值</param>
        ///// <param name="precision">
        ///// 获取或设置用来表示 Value 属性的最大位数。 
        ///// Precision 属性由 SqlDbType 为 Decimal 的参数使用。 
        ///// 不需要为输入参数指定 Precision 和 Scale 属性的值，因为可以从参数值推断它们的值。 
        ///// 输出参数以及在您需要指定参数的完整元数据而不指示值时，
        ///// 需要 Precision 和 Scale，例如指定一个具有特定精度和小数位数的空值。 
        ///// </param>
        ///// <param name="scale">获取或设置 Value 解析为的小数位数。 </param>
        ///// <param name="sourceColumn">获取或设置源列的名称，该源列映射到 DataSet 并用于加载或返回 Value </param>
        ///// <param name="sourceVersion">获取或设置在加载 Value 时要使用的 DataRowVersion。</param>
        ///// <param name="value">参数值</param>
        ///// <returns></returns>
        //DbParameter CreateParameter(String name,
        //                            DbType? dbType,
        //                            Int32? size,
        //                            ParameterDirection direction,
        //                            Boolean nullable,
        //                            Byte precision,
        //                            Byte scale,
        //                            String sourceColumn,
        //                            DataRowVersion sourceVersion,
        //                            Object value);

        ///// <summary>
        ///// 创建命令对象
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="value"></param>
        ///// <param name="direction"></param>
        ///// <returns></returns>
        //DbParameter CreateParameter(String name,
        //                            Object value,
        //                            ParameterDirection direction = ParameterDirection.Input);



        ///// <summary>
        ///// 创建一个数据库连接对象
        ///// </summary>
        ///// <param name="open">标志是否打开连接；true打开；false不打开</param>
        ///// <returns></returns>
        //DbConnection CreateConnection(Boolean open);

        ///// <summary>
        ///// 获取连接字符串
        ///// </summary>
        ///// <returns></returns>
        //String GetConnectionString();

        ///// <summary>
        ///// 获取连接字符串
        ///// </summary>
        ///// <returns></returns>
        //DbProviderFactory GetDBProviderFactory();
    }
}
