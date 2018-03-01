using System;
using System.Data.Common;

namespace MiniORM
{
    public interface IConnectionStringProvider
    {
        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <returns></returns>
        String GetConnectionStr();
    }
}
