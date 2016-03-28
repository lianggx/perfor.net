using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Perfor.Lib.Extension;

namespace Perfor.Lib.Helpers
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
        public SQLParameter(IEnumerable<string> fields, IEnumerable<object> values)
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
        public SQLParameter(IEnumerable<string> fields, IEnumerable<object> values, int primaryKeyIndex)
        {
            this.fields = fields;
            this.values = values;
            this.primarykeyindex = primaryKeyIndex;
        }

        private IEnumerable<string> fields = null;
        /**
         * @ 字段名称
         * */
        public List<string> Fields
        {
            get
            {
                if (fields.IsNullOrEmpty())
                    fields = new List<string>();

                return fields.ToList();
            }
            set { fields = value; }
        }

        private IEnumerable<object> values = null;
        /**
         * @ 字段对应的值
         * */
        public List<object> Values
        {
            get
            {
                if (values.IsNullOrEmpty())
                    values = new List<object>();

                return values.ToList();
            }
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
