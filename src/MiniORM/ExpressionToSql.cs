using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace MiniORM
{
    public enum ExpressionNodeValueType
    {
        /// <summary>
        /// 变量名称
        /// </summary>
        ParamName,
        /// <summary>
        /// 常量值
        /// </summary>
        Constant,
        /// <summary>
        /// 表达式串
        /// </summary>
        Expression
    }
    public class ExpressionNodeValue
    {
        public Object Value;
        public ExpressionNodeValueType ValueType;
    }

    /// <summary>
    /// 此类算法需要重新设计
    /// </summary>
    public class ExpressionToSql : IExpressionToSql
    {
        protected BasicSqlBuilder _sqlBuilder;

        public ExpressionToSql(BasicSqlBuilder sqlBuilder)
        {
            _sqlBuilder = sqlBuilder;
        }

        public virtual void BuildReturn(Expression exp, ref StringBuilder sqlStrBuilder, ref List<DbParameter> paramList)
        {
            if (exp == null) return;
            ExpressionNodeValue nodeValue = null;
            if (exp is LambdaExpression)
            {
                LambdaExpression ldExp = exp as LambdaExpression;
                if (ldExp.Parameters.Count == 0)
                    throw new Exception(String.Format("Expression[{0}] is not right,the source object is in need!", exp.ToString()));
                DoBuildReturn(ldExp.Body, ldExp.Parameters[0], ref sqlStrBuilder, ref paramList, out nodeValue);
            }
            else
            {
                ThrowExpressionError(exp);
                //throw new Exception(String.Format("Expression[{0}] not support!", exp.ToString()));
            }
        }

        public virtual void BuildWhere(Expression exp, ref StringBuilder sqlStrBuilder, ref List<DbParameter> paramList)
        {
            if (exp == null) return;
            ExpressionNodeValue nodeValue = null;
            if (exp is LambdaExpression)
            {
                LambdaExpression ldExp = exp as LambdaExpression;
                if (ldExp.Parameters.Count == 0)
                    throw new Exception(String.Format("Expression[{0}] is not right,the source object is in need!", exp.ToString()));
                DoBuildWhere(ldExp.Body, ldExp.Parameters[0], ref sqlStrBuilder, ref paramList, out nodeValue);
            }
            else
            {
                ThrowExpressionError(exp);
                //throw new Exception(String.Format("Expression[{0}] not support!", exp.ToString()));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="sqlStrBuilder"></param>
        /// <param name="desc">true：降序；false：升序</param>
        public virtual void BuildOrderBy(Expression exp, ref StringBuilder sqlStrBuilder, Boolean desc = true)
        {
            if (exp == null) return;
            ExpressionNodeValue nodeValue = null;
            if (exp is LambdaExpression)
            {
                LambdaExpression ldExp = exp as LambdaExpression;
                if (ldExp.Parameters.Count == 0)
                    throw new Exception(String.Format("Expression[{0}] is not right,the source object is in need!", exp.ToString()));
                DoBuildOrderBy(ldExp.Body, ldExp.Parameters[0], desc, ref sqlStrBuilder, out nodeValue);
            }
            else
            {
                ThrowExpressionError(exp);
                //throw new Exception(String.Format("Expression[{0}] not support!", exp.ToString()));
            }
        }

        protected virtual void DoBuildOrderBy(Expression exp,
                                              Object sourceObj,
                                              Boolean desc,
                                              ref StringBuilder sqlStrBuilder,
                                              out ExpressionNodeValue nodeValue)
        {
            nodeValue = null;
            if (exp is NewExpression)
            {
                NewExpression newExp = exp as NewExpression;
                foreach (Expression arg in newExp.Arguments)
                {
                    DoBuildOrderBy(arg, sourceObj, desc, ref sqlStrBuilder, out nodeValue);
                }
                //因为直接传入了最原始的的sqlStrBuilder对象，所以必须要记得return，要不就会重复添加了
                return;
            }
            else if (exp is MemberExpression)
            {
                MemberExpression(exp as MemberExpression, sourceObj, out nodeValue);
                if (nodeValue.ValueType == ExpressionNodeValueType.ParamName)
                {
                    String memberName = nodeValue.Value.ToString();
                    String paramName = GetReturnParamName(memberName);
                    ExpressionNodeValue new_nodeValue = new ExpressionNodeValue()
                    {
                        Value = String.Format("{0} {1}", memberName, desc ? "DESC" : "ASC"),
                        ValueType = ExpressionNodeValueType.Expression
                    };
                    nodeValue = new_nodeValue;
                }
            }
            else if (exp is UnaryExpression)
            {
                //因为直接传入了最原始的的sqlStrBuilder对象，所以必须要记得return，要不就会重复添加了
                DoBuildOrderBy((exp as UnaryExpression).Operand, sourceObj, desc, ref sqlStrBuilder, out nodeValue);
                return;
            }
            else
            {
                ThrowExpressionError(exp);
            }
            //需要解析nodeValue值
            if (nodeValue.ValueType == ExpressionNodeValueType.Expression)
            {
                if (sqlStrBuilder.Length > 0) sqlStrBuilder.Append(",");
                sqlStrBuilder.Append(nodeValue.Value.ToString());
            }
        }

        protected virtual void DoBuildReturn(Expression exp,
                                             Object sourceObj,
                                             ref StringBuilder sqlStrBuilder,
                                             ref List<DbParameter> paramList,
                                             out ExpressionNodeValue nodeValue)
        {
            nodeValue = null;
            if (exp is NewExpression)
            {
                NewExpression newExp = exp as NewExpression;
                foreach (Expression arg in newExp.Arguments)
                {
                    DoBuildReturn(arg, sourceObj, ref sqlStrBuilder, ref paramList, out nodeValue);
                }
                return;
            }
            else if (exp is MemberExpression)
            {
                MemberExpression(exp as MemberExpression, sourceObj, out nodeValue);
                if (nodeValue.ValueType == ExpressionNodeValueType.ParamName)
                {
                    String memberName = nodeValue.Value.ToString();
                    String paramName = GetReturnParamName(memberName);
                    if (paramList.SingleOrDefault(it => it.ParameterName == paramName) != null)
                        throw new Exception(String.Format("return memberName[{0}] define repeat!", memberName));
                    paramList.Add(CreateParam(paramName, null, ParameterDirection.ReturnValue, GetReturnParamSize()));
                    ExpressionNodeValue new_nodeValue = new ExpressionNodeValue()
                    {
                        Value = String.Format("{0} into {1}", memberName, paramName),
                        ValueType = ExpressionNodeValueType.Expression
                    };
                    nodeValue = new_nodeValue;
                }
            }
            else
            {
                ThrowExpressionError(exp);
            }
            //需要解析nodeValue值
            if (nodeValue.ValueType == ExpressionNodeValueType.Expression)
            {
                if (sqlStrBuilder.Length > 0) sqlStrBuilder.Append(",");
                sqlStrBuilder.Append(nodeValue.Value.ToString());
            }
        }

        protected virtual void DoBuildWhere(Expression exp,
                                            Object sourceObj,
                                            ref StringBuilder whereSqlStrBuilder,
                                            ref List<DbParameter> paramList,
                                            out ExpressionNodeValue nodeValue)
        {
            nodeValue = null;
            if (exp is BinaryExpression)
            {
                //二元表达式
                BinaryExpression binaryNode = exp as BinaryExpression;
                StringBuilder leftStrBuilder = new StringBuilder();
                StringBuilder rightStrBuilder = new StringBuilder();
                ExpressionNodeValue leftNodeValue = null;
                ExpressionNodeValue rightNodeValue = null;
                //先左边遍历
                DoBuildWhere(binaryNode.Left, sourceObj, ref leftStrBuilder, ref paramList, out leftNodeValue);
                //再右边遍历
                DoBuildWhere(binaryNode.Right, sourceObj, ref rightStrBuilder, ref paramList, out rightNodeValue);

                String paramName = String.Empty;
                String left = String.Empty;
                String right = String.Empty;
                String operStr = GetOperStr(exp.NodeType);
                //处理结果
                if (leftNodeValue.ValueType == ExpressionNodeValueType.ParamName
                    || rightNodeValue.ValueType == ExpressionNodeValueType.ParamName)
                {
                    //有属性名的情况
                    left = leftNodeValue.ValueType == ExpressionNodeValueType.ParamName ? leftNodeValue.Value.ToString() : rightNodeValue.Value.ToString();
                    Object value = leftNodeValue.ValueType != ExpressionNodeValueType.ParamName ? leftNodeValue.Value : rightNodeValue.Value;
                    if (value != null)
                    {
                        //right = GetWhereParamName(left);
                        //if (paramList.SingleOrDefault(it => it.ParameterName == right) != null)
                        //    throw new Exception(String.Format("parameter name[{0}] define repeat!", right));
                        //paramList.Add(CreateParam(right, value));
                        right = AddDBParam(ref paramList, value);
                    }
                    else
                    {
                        operStr = FormatNULLOperStr(operStr);
                        right = "NULL";
                    }
                    nodeValue = new ExpressionNodeValue()
                    {
                        Value = String.Format("({0} {1} {2})", left, operStr, right),
                        ValueType = ExpressionNodeValueType.Expression
                    };
                }
                else if (leftNodeValue.ValueType == ExpressionNodeValueType.Constant && rightNodeValue.ValueType == ExpressionNodeValueType.Constant)
                {
                    //都是常量的情况
                    LambdaExpression ldExp = Expression.Lambda(exp);
                    Object value = ldExp.Compile().DynamicInvoke();
                    Boolean boolValue;
                    if (Boolean.TryParse(Convert.ToString(value), out boolValue))
                    {
                        nodeValue = new ExpressionNodeValue();
                        nodeValue.ValueType = ExpressionNodeValueType.Expression;
                        nodeValue.Value = String.Format("(1 = {0})", boolValue ? "1" : "0");
                    }
                    else
                    {
                        nodeValue = new ExpressionNodeValue()
                        {
                            Value = value,
                            ValueType = ExpressionNodeValueType.Constant
                        };
                    }
                }
                //else if (leftNodeValue.ValueType == ExpressionNodeValueType.Constant || rightNodeValue.ValueType == ExpressionNodeValueType.Constant)
                //{
                //    //只要两个节点有一个计算出的是常量，则根据节点类型来推断，看看能否得出常量
                //    LambdaExpression ldExp = Expression.Lambda(exp);
                //    Object value = ldExp.Compile().DynamicInvoke();
                //    Boolean boolValue;
                //    if (Boolean.TryParse(Convert.ToString(value), out boolValue))
                //    {
                //        nodeValue = new ExpressionNodeValue();
                //        nodeValue.ValueType = ExpressionNodeValueType.Expression;
                //        //明显能推断出 表达式的值就是true
                //        if (boolValue && exp.NodeType == ExpressionType.OrElse)
                //        {
                //            nodeValue.ValueType = ExpressionNodeValueType.Constant;
                //            nodeValue.Value = true;
                //        }
                //        else if (!boolValue && exp.NodeType == ExpressionType.AndAlso)
                //        {
                //            //明显能推断出表达式的值就是false
                //            nodeValue.ValueType = ExpressionNodeValueType.Constant;
                //            nodeValue.Value = false;
                //        }
                //    }
                //    else
                //    {
                //        //省略掉已经没用的值

                //    }
                //}
                else
                {
                    left = leftNodeValue.Value == null ? "NULL" :
                            (leftNodeValue.ValueType == ExpressionNodeValueType.Constant ? GetValueFormat(leftNodeValue.Value) :
                                                                                            leftNodeValue.Value.ToString());
                    right = rightNodeValue.Value == null ? "NULL" :
                        (rightNodeValue.ValueType == ExpressionNodeValueType.Constant ? GetValueFormat(rightNodeValue.Value) :
                                                                                            rightNodeValue.Value.ToString());
                    if (left == "NULL")
                    {
                        operStr = FormatNULLOperStr(operStr);
                        String temp = left;
                        left = right;
                        right = temp;
                    }
                    else if (right == "NULL")
                    {
                        operStr = FormatNULLOperStr(operStr);
                    }
                    nodeValue = new ExpressionNodeValue()
                    {
                        Value = String.Format("({0} {1} {2})", left, operStr, right),
                        ValueType = ExpressionNodeValueType.Expression
                    };
                }
            }
            else if (exp is ConstantExpression)
            {
                ConstantExpression(exp as ConstantExpression, sourceObj, out nodeValue);
            }
            else if (exp is MemberExpression)
            {
                MemberExpression(exp as MemberExpression, sourceObj, out nodeValue);
            }
            else if (exp is UnaryExpression)
            {
                DoBuildWhere((exp as UnaryExpression).Operand, sourceObj, ref whereSqlStrBuilder, ref paramList, out nodeValue);
                //UnaryExpression(exp as UnaryExpression, sourceObj, out nodeValue);
            }
            else
            {
                //throw new Exception(String.Format("Expression[{0}] not support!", exp.ToString()));
                ThrowExpressionError(exp);
            }
            //需要解析nodeValue值
            if (nodeValue.ValueType == ExpressionNodeValueType.Expression)
            {
                whereSqlStrBuilder.Append(nodeValue.Value.ToString());
            }
        }

        protected virtual DbParameter CreateParam(String name,
                                                  Object value,
                                                  ParameterDirection direction = ParameterDirection.Input,
                                                  Int32? size = null)
        {
            return _sqlBuilder.CreateParameter(name, value, direction, size);
        }

        protected virtual Int32 GetReturnParamSize()
        {
            return 2000;
        }

        protected virtual String GetReturnParamName(String name)
        {
            return _sqlBuilder.PropertyNameToReturnParamName(name);
        }

        /// <summary>
        /// 添加一个数据库变量，并返回变量名
        /// </summary>
        /// <param name="paramList"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        protected virtual String AddDBParam(ref List<DbParameter> paramList,
                                            Object value,
                                            ParameterDirection direction = ParameterDirection.Input,
                                            Int32? size = null)
        {
            String paramName = _sqlBuilder.FormatToParamName(String.Format("p_{0}", paramList.Count));
            DbParameter param = CreateParam(paramName, value, direction, size);
            paramList.Add(param);
            return paramName;
        }

        /// <summary>
        /// 格式化空值的操作符（对于空值，同一种操作，数据库有时候会采用不同的操作符）
        /// </summary>
        /// <param name="oprStr"></param>
        /// <returns></returns>
        protected virtual String FormatNULLOperStr(String oprStr)
        {
            if (oprStr == "=") oprStr = "IS";
            else oprStr = "IS NOT";
            return oprStr;
        }

        /// <summary>
        /// 获取操作符
        /// </summary>
        /// <param name="type">表达式类型</param>
        /// <returns></returns>
        protected virtual String GetOperStr(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Subtract:
                    return "-";
            }
            throw new Exception(String.Format("ExpressionType[{0}] not support!", type.ToString()));
        }


        protected virtual void MemberExpression(MemberExpression exp,
                                                Object sourceObj,
                                                out ExpressionNodeValue nodeValue)
        {
            nodeValue = new ExpressionNodeValue();
            //如果是原始对象属性访问，则应该是作为变量名的
            if (exp.Expression.Equals(sourceObj))
            {
                nodeValue.Value = exp.Member.Name;
                nodeValue.ValueType = ExpressionNodeValueType.ParamName;
            }
            else
            {
                //如果是访问其他的对象属性，则马上求对象属性值，值作为匹配条件
                LambdaExpression ldExp = Expression.Lambda(exp);
                nodeValue.Value = ldExp.Compile().DynamicInvoke();
                nodeValue.ValueType = ExpressionNodeValueType.Constant;
            }
        }

        protected virtual void ConstantExpression(ConstantExpression exp, Object sourceObj, out ExpressionNodeValue nodeValue)
        {
            nodeValue = new ExpressionNodeValue()
            {
                Value = exp.Value,
                ValueType = ExpressionNodeValueType.Constant
            };
        }

        protected virtual string GetValueFormat(object obj)
        {
            var type = obj.GetType();
            if (type.Name == "List`1") //list集合
            {
                List<string> data = new List<string>();
                var list = obj as IEnumerable;
                string sql = string.Empty;
                foreach (var item in list)
                {
                    data.Add(GetValueFormat(item));
                }
                sql = "(" + string.Join(",", data) + ")";
                return sql;
            }

            if (type == typeof(string))// 
            {
                return string.Format("'{0}'", obj.ToString());
            }
            return obj.ToString();
        }

        protected virtual void ThrowExpressionError(Expression exp)
        {
            throw new Exception(String.Format("Expression[{0}] not support!", exp.ToString()));
        }

        //protected virtual String GetWhereParamName(String name)
        //{
        //    name = "q_" + name;
        //    return _sqlBuilder.FormatToParamName(name);
        //}
    }
}
