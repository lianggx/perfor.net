using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Lib.Enums
{
    public partial class SQLExpression
    {
        /**
         * @ LdfSQLCondition连接上一个条件采用的连接符号
         * */
        public enum JoinType
        {
            AND = 10,
            OR = 20
        }

        /**
         * @ 排序方式
         * */
        public enum Order
        {
            /**
             * @ 顺序
             * */
            ASC = 0,
            /**
             * @ 倒序
             * */
            DESC = 1
        }

        /**
         * @ 条件表达式
         * */
        public enum ExprOperator
        {
            /**
             * @ 大于
             * */
            Gt = 10,
            /**
             * @ 小于
             * */
            Lt = 20,
            /**
             * @ 等于
             * */
            Eq = 30,
            /**
             * @ 大于等于
             * */
            GtEq = 40,
            /**
             * @ 小于等于
             * */
            LtEq = 50,
            /**
             * @ 模糊查询
             * */
            Like = 60,
            /**
             * @ 相当于is not null
             * */
            NotNull = 70,
            /**
             * @ 相当于 is not like
             * */
            NotLike = 80,
            /**
             * @ 在xxx中，相当于in
             * */
            In = 90,
            /**
             * @ 不存在在xxx中，相当于 not in
             * */
            NotIn = 100,
            /**
             * @ 相当于 is null
             * */
            IsNull = 110
        }
    }
}
