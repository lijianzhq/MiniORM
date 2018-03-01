using System;
using System.Text;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;

namespace MiniORM
{
    /// <summary>
    /// 公开给外部调用（扩展方法针对此接口实现扩展方法即可）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDBQuery<T>
    { }

    /// <summary>
    /// 实现类实现的接口，这样可以与扩展方法进行隔离，外部用户看不到这个接口的定义
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDBQuery_Implement<T> : IDBQuery<T>
    {
        IDB DB { get; }
        /// <summary>
        /// 开始行号（用于分页，序号从1开始）
        /// </summary>
        UInt16? StartRowNum { get; set; }
        /// <summary>
        /// 结束行号（用于分页，序号从1开始）
        /// </summary>
        UInt16? EndRowNum { get; set; }
        List<Expression> WhereExpression { get; }
        List<OrderByItem> OrderByItem { get; }
        List<JoinItem> JoinItem { get; }
    }
}
