using System;
using System.Data.OracleClient;
//using System.Data.Common;

namespace MiniORM
{
    public class OracleDB : DB
    {
        protected ISqlBuilder _sqlbuilder;
        public override ISqlBuilder SqlBuilder
        {
            get
            {
                if (_sqlbuilder == null)
                    _sqlbuilder = new OracleSqlBuilder(this._dbProviderFactory);
                return _sqlbuilder;
            }
        }

        public OracleDB(String connectionName)
            : base(new ConnectionStringProvider(connectionName), OracleClientFactory.Instance)
        { }

        public OracleDB(IConnectionStringProvider connectionProvider)
            : base(connectionProvider, OracleClientFactory.Instance)
        { }
    }
}
