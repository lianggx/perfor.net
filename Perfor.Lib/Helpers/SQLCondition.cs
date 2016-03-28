using Perfor.Lib.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Perfor.Lib.Helpers
{
    /**
     * @ SQL条件集合类
     * */
    public partial class SQLCondition
    {
        #region Identity
        /**
         * @ 默认构造函数
         * */
        public SQLCondition() { }

        /**
         * @ 构造函数第一次重载
         * @ name 字段名称
         * @ value 参数值
         * */
        public SQLCondition(string name, object value)
            : this(name, value, SQLExpression.ExprOperator.Eq)
        {
        }

        /**
       * @ 构造函数第一次重载
       * @ name 字段名称
       * @ value 参数值
       * @ expr 表达式运算符号
       * */
        public SQLCondition(string name, object value, SQLExpression.ExprOperator expr)
            : this(name, value, expr, SQLExpression.JoinType.AND)
        {
        }

        /**
         * @ 构造函数第一次重载
         * @ name 字段名称
         * @ value 参数值
         * @ joinType 该参数连接上一个参数的连接符号
         * */
        public SQLCondition(string name, object value, SQLExpression.JoinType joinType)
            : this(name, value, SQLExpression.ExprOperator.Eq, joinType)
        {
        }

        /**
          * @ 构造函数第一次重载
          * @ name 字段名称
          * @ value 参数值
          * @ expr 表达式运算符号
          * @ joinType 该参数连接上一个参数的连接符号
          * */
        public SQLCondition(string name, object value, SQLExpression.ExprOperator expr, SQLExpression.JoinType joinType)
        {
            this.name = name;
            this.val = value;
            this.expr = expr;
            this.joinType = joinType;
        }
        #endregion

        private string name = string.Empty;
        /**
         * @ 字段名称
         * */
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private object val = null;
        /**
         * @ 参数值
         * */
        public object Value
        {
            get { return val; }
            set { val = value; }
        }

        private SQLExpression.ExprOperator expr = SQLExpression.ExprOperator.Eq;
        /**
         * @ 表达式选项，默认Eq：=，等号
         * */
        public SQLExpression.ExprOperator Expression
        {
            get { return expr; }
            set { expr = value; }
        }

        private SQLExpression.JoinType joinType = SQLExpression.JoinType.AND;
        /**
         * @连接上一个条件选项，默认AND：and
         * */
        public SQLExpression.JoinType JoinType
        {
            get { return joinType; }
            set { joinType = value; }
        }
    }

    /**
     * @ 对条件增加左括号
     * */
    public partial class BracketLeft : SQLCondition
    {

    }

    /**
     * @ 对条件增加右括号
     * */
    public partial class BracketRight : SQLCondition
    {

    }
}
