using System;
using System.Configuration;
using System.Data.Common;

namespace MiniORM
{
    /// <summary>
    /// 从app.config中去读取数据库连接
    /// </summary>
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        protected String _name;

        public ConnectionStringProvider(String name)
        {
            _name = name;
        }

        public virtual string GetConnectionStr()
        {
            foreach (ConnectionStringSettings setting in ConfigurationManager.ConnectionStrings)
            {
                if (setting.Name == _name)
                    return setting.ConnectionString;
            }
            return String.Empty;
        }
    }
}
