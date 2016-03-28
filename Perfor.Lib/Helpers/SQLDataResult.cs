using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Perfor.Lib.Helpers
{
    /**
     * @ 执行SQL查询返回的结果集对象
     * */
    [Serializable]
    public partial class SQLDataResult : Dictionary<string, object>
    {
        /*
         * @ 默认构造函数
         * */
        public SQLDataResult() { }

        /**
         * @ 序列化参数构造函数
         * */
        protected SQLDataResult(SerializationInfo info, StreamingContext contex)
            : base(info, contex) { }

        /**
         * @ 按索引编号查询对象
         * */
        public object this[int index]
        {
            get
            {
                int len = this.Count;
                if (index < 0 || index >= len)
                    return null;

                object result = null;

                int i = 0;
                foreach (var item in this)
                {
                    if (i < index)
                    {
                        i++;
                        continue;
                    }
                    result = item.Value;
                    break;
                }
                return result;
            }
        }

        /**
         * @ 获取一个键值对象
         * @ key 要查询的键名称
         * */
        public object GetValue(string key)
        {
            object result = null;
            if (this.ContainsKey(key))
            {
                this.TryGetValue(key, out result);
            }
            return result;
        }
    }
}
