using System;
using System.Linq.Expressions;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;

namespace MiniORM
{
    public interface IExpressionToSql
    {
        void BuildWhere(Expression exp, ref StringBuilder sqlStrBuilder, ref List<DbParameter> paramList);
        void BuildReturn(Expression exp, ref StringBuilder sqlStrBuilder, ref List<DbParameter> paramList);
        void BuildOrderBy(Expression exp, ref StringBuilder sqlStrBuilder, Boolean desc = true);
    }
}
