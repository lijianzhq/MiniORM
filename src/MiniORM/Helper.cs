using System;
using System.Data.Common;
using System.Text;
using System.Collections.Generic;

namespace MiniORM
{
    public static class Helper
    {
        /// <summary>
        /// 写日记
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        public static void WriteSql(String sql, List<DbParameter> paramList)
        {
            StringBuilder logStrBuilder = new StringBuilder();
            logStrBuilder.AppendLine(sql);
            foreach (DbParameter p in paramList)
            {
                logStrBuilder.AppendFormat("{0}:{1}", p.ParameterName, Convert.ToString(p.Value));
                logStrBuilder.AppendLine();
            }
            //Log.SQL.LogInfo(logStrBuilder.ToString());
        }
    }
}
