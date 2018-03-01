using System;
using System.Data.Common;

namespace MiniORM
{
    public interface IConnectionProvider
    {
        /// <summary>
        /// 创建一个连接对象
        /// </summary>
        /// <param name="connStr">连接字符串</param>
        /// <param name="factory">数据库提供程序</param>
        /// <param name="open">是否打开连接；true：打开连接；false：关闭连接</param>
        /// <returns></returns>
        DbConnection GetConnection(Boolean open = false);
        
        /// <summary>
        /// 删除当前上下文的所有数据库连接
        /// </summary>
        void CloseAllConnection();
    }
}
