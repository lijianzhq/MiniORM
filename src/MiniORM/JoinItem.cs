using System;
using System.Linq.Expressions;

namespace MiniORM
{
    public enum JoinType
    {
        InnerJoin,
        OuterJoin
    }

    public class JoinItem
    {
        public Expression Expression;

        /// <summary>
        /// join的类型
        /// </summary>
        public JoinType JoinType;
    }
}
