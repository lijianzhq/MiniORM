using System;
using System.Text;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;

namespace MiniORM
{
    public class DBQuery<TEntity> : IDBQuery_Implement<TEntity>
        where TEntity : class
    {
        protected List<Expression> _whereExpression = new List<Expression>();
        protected List<OrderByItem> _orderByItem = new List<OrderByItem>();
        protected List<JoinItem> _joinItem = new List<JoinItem>();

        public List<Expression> WhereExpression
        {
            get { return _whereExpression; }
        }

        public List<OrderByItem> OrderByItem
        {
            get { return _orderByItem; }
        }

        public List<JoinItem> JoinItem
        {
            get { return _joinItem; }
        }

        /// <summary>
        /// 开始行号（用于分页，序号从1开始）
        /// </summary>
        protected UInt16? _startRowNum = null;
        public UInt16? StartRowNum
        {
            get { return _startRowNum; }
            set { _startRowNum = value; }
        }

        /// <summary>
        /// 结束行号（用于分页，序号从1开始）
        /// </summary>
        protected UInt16? _endRowNum = null;
        public UInt16? EndRowNum
        {
            get { return _endRowNum; }
            set { _endRowNum = value; }
        }

        //protected ISqlBuilder _sqlBuilder;
        //public ISqlBuilder SqlBuilder
        //{
        //    get { return DB.SqlBuilder; }
        //}

        protected IDB _DB;
        public IDB DB
        {
            get { return _DB; }
        }

        public DBQuery(IDB DB)
        //: this(sqlBuilder, null)
        {
            _DB = DB;
        }

        public DBQuery(ISqlBuilder sqlBuilder, Expression<Func<TEntity, dynamic>> selector)
        {
            //_sqlBuilder = sqlBuilder;
            Init(selector);
        }

        /// <summary>
        /// 根据查询的指示，初始化sql字符串（暂不支持）
        /// select可以指定某些列以及列别名
        /// </summary>
        /// <param name="selector"></param>
        protected virtual void Init(Expression<Func<TEntity, dynamic>> selector)
        {
            throw new NotImplementedException();
        }

    }
}
