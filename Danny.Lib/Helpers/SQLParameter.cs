using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Danny.Lib.Helpers
{
    /**
         * @ 插入数据参数类
         * */
    public class SQLParameter
    {
        /**
         * @ 默认构造函数
         * */
        public SQLParameter() { }
        /**
         * @ 构造函数第一次重载
         * @ fields 字段列表
         * @ values 值列表
         * */
        public SQLParameter(string[] fields, object[] values)
        {
            this.fields = fields;
            this.values = values;
        }

        /**
         * @ 构造函数第二次重载
         * @ fields 字段列表
         * @ values 值列表
         * @ primaryKeyIndex 实体键在fields参数列表中的索引
         * */
        public SQLParameter(string[] fields, object[] values, int primaryKeyIndex)
        {
            this.fields = fields;
            this.values = values;
            this.primarykeyindex = primaryKeyIndex;
        }

        private string[] fields = null;
        /**
         * @ 字段名称
         * */
        public string[] Fields
        {
            get { return fields; }
            set { fields = value; }
        }

        private object[] values = null;
        /**
         * @ 字段对应的值
         * */
        public object[] Values
        {
            get { return values; }
            set { values = value; }
        }

        private int primarykeyindex = -1;
        /**
         * @ 主键在fields参数列表的索引位置
         * */
        public int PrimaryKeyIndex
        {
            get { return primarykeyindex; }
            set { primarykeyindex = value; }
        }

        private string tablename = string.Empty;
        /**
         *  @ 重写父类的属性
         * */
        public virtual string TableName
        {
            get
            {
                return tablename;
            }
            set
            {
                tablename = value;
            }
        }
    }
}
