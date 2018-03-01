using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

using DreamCube.Foundation.Basic.Utility;

namespace MiniORM
{
    public class OracleSqlBuilder : BasicSqlBuilder
    {
        public OracleSqlBuilder(DbProviderFactory _dbProviderFactory)
            : base(_dbProviderFactory)
        { }

        protected override string GetParamPrefix()
        {
            return ":";
        }

        public override void BuildInsert<TEntity>(object entity, Expression<Func<TEntity, dynamic>> returns, out string sql, out List<DbParameter> dbParams)
        {
            sql = String.Empty;
            String columnsStr;
            String paramsStr;
            String returnsStr;
            //构造插入语句的每一部分
            BuildInsert<TEntity>(entity, returns, out columnsStr, out paramsStr, out returnsStr, out dbParams);
            if (!String.IsNullOrWhiteSpace(returnsStr))
                returnsStr = "returning " + returnsStr;
            //根据不同的数据库组装成不同的sql语句
            sql = String.Format("INSERT INTO {0}({1}) VALUES({2}) {3}", GetTableName(entity), columnsStr, paramsStr, returnsStr);
        }

        public override void BuildUpdate<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> predicate, out string sql, out List<DbParameter> dbParams)
        {
            sql = String.Empty;
            String columnsStr;
            String whereStr;
            BuildUpdate<TEntity>(entity, predicate, out columnsStr, out whereStr, out dbParams);
            if (!String.IsNullOrWhiteSpace(whereStr))
                whereStr = "where " + whereStr;
            sql = String.Format("UPDATE {0} SET {1} {2}", GetTableName(entity), columnsStr, whereStr);
        }

        public override void BuildSelect<TEntity>(IDBQuery<TEntity> query, out string sql, out List<DbParameter> dbParams)
        {
            String columnsStr;
            String whereStr;
            String orderByStr;
            //表名或者子查询
            String sourcePart;
            BuildSelect<TEntity>(query, out columnsStr, out whereStr, out orderByStr, out dbParams);
            var implementIf = DBQueryExtension.ChangeInterface(query);
            //处理分页的情况
            if (implementIf.StartRowNum.HasValue || implementIf.EndRowNum.HasValue)
            {
                //起始行号不能为空，最小是1
                Int32 start = Math.Max(1, implementIf.StartRowNum.HasValue ? implementIf.StartRowNum.Value : 1);
                String tableAlias = tableAlias = "t1";
                columnsStr = GetColumnStrWithTableAlias(columnsStr, tableAlias);
                //增加列序号
                columnsStr += ",rownum rn";
                sourcePart = String.Format("(SELECT {0} FROM {1} {2} {3} {4})", columnsStr, GetTableName<TEntity>(), tableAlias, whereStr, orderByStr);
                //重新更新各个查询语句的构造部分
                columnsStr = "*";//列改为所有列
                //where语句更新为页码的控制
                whereStr = String.Format("where rn >={0}", start.ToString());
                if (implementIf.EndRowNum.HasValue)
                    whereStr += String.Format(" and rn <= {0}", implementIf.EndRowNum.Value.ToString());
                orderByStr = "";//外层不需要orderby了
            }
            else
            {
                sourcePart = GetTableName<TEntity>();
            }
            sql = String.Format("SELECT {0} FROM {1} {2} {3}", columnsStr, sourcePart, whereStr, orderByStr);
        }

        /// <summary>
        /// 每一列增加表别名前缀
        /// </summary>
        /// <param name="columnStr"></param>
        /// <param name="tableAlias"></param>
        /// <returns></returns>
        protected String GetColumnStrWithTableAlias(String columnStr, String tableAlias)
        {
            if (String.IsNullOrWhiteSpace(columnStr) || String.IsNullOrWhiteSpace(tableAlias)) return columnStr;
            return columnStr.Split(',')
                            .Select(it => String.Format("{0}.{1}", tableAlias, it))
                            .JoinEx(",");
        }
    }
}
