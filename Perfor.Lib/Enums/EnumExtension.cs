using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Lib.Enums
{
    public static partial class EnumExtension
    {
        /**
         * @ 获取枚举的字符串表现形式
         * */
        public static string GetEnumString(this SQLExpression.JoinType value)
        {
            if (value == SQLExpression.JoinType.AND)
                return "AND";

            return "OR";
        }

        /**
         * @ 获取枚举的字符串表现形式
         * */
        public static string GetEnumString(this SQLExpression.Order value)
        {
            if (value == SQLExpression.Order.ASC)
                return "ASC";

            return "DESC";
        }

        /**
        * @ 获取枚举的字符串表现形式
        * */
        public static string GetEnumString(this SQLExpression.ExprOperator value)
        {
            string defaultExpression = string.Empty;
            switch (value)
            {
                /**
             * @ 大于
             * */
                case SQLExpression.ExprOperator.Gt:
                    defaultExpression = ">";
                    break;
                /**
                 * @ 小于
                 * */
                case SQLExpression.ExprOperator.Lt:
                    defaultExpression = "<";
                    break;
                /**
                 * @ 大于等于
                 * */
                case SQLExpression.ExprOperator.GtEq:
                    defaultExpression = ">=";
                    break;
                /**
                 * @ 小于等于
                 * */
                case SQLExpression.ExprOperator.LtEq:
                    defaultExpression = "<=";
                    break;
                /**
                 * @ 模糊查询
                 * */
                case SQLExpression.ExprOperator.Like:
                    defaultExpression = "LIKE";
                    break;
                /**
                 * @ 相当于is not null
                 * */
                case SQLExpression.ExprOperator.NotNull:
                    defaultExpression = "IS NOT";
                    break;
                /**
                 * @ 相当于 is not like
                 * */
                case SQLExpression.ExprOperator.NotLike:
                    defaultExpression = "IS NOT LIKE";
                    break;
                /**
                 * @ 在xxx中，相当于in
                 * */
                case SQLExpression.ExprOperator.In:
                    defaultExpression = "IN";
                    break;
                /**
                 * @ 不存在在xxx中，相当于 not in
                 * */
                case SQLExpression.ExprOperator.NotIn:
                    defaultExpression = "NOT IN";
                    break;
                /**
                 * @ 相当于 is null
                 * */
                case SQLExpression.ExprOperator.IsNull:
                    defaultExpression = "IS";
                    break;
                /**
                 * @ 默认Eq
                 * */
                default:
                    defaultExpression = "=";
                    break;
            }
            return defaultExpression;
        }
    }
}
