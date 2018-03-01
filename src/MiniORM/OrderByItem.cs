using System;
using System.Linq.Expressions;

namespace MiniORM
{
    public class OrderByItem
    {
        public Expression Expression;

        /// <summary>
        /// 是否降序
        /// </summary>
        public Boolean Desc;
    }
}
