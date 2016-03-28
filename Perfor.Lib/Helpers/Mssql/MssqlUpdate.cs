using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using Perfor.Lib.Extension;

namespace Perfor.Lib.Helpers.Mssql
{
    /**
     * @ 更新数据的入口
     * */
    public class MssqlUpdate : SQLHelper, SQLIDataSave
    {
        /**
         * @ 默认构造函数
         * */
        public MssqlUpdate() : base() { }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        public MssqlUpdate(string tableName)
        {
            SetTableName(tableName);
        }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        public MssqlUpdate(string tableName, SQLContext context)
            : base(context)
        {
            SetTableName(tableName);
        }

        /**
         * @ 初始化SQL插入语句，fields和values数组长度必须一致
         * @ fields 要更新的字段名称
         * @ values 字段对应的值 
         * */
        public void AddUpdate(string field, object value)
        {
            ArrayUpdate.Add(field, value);
        }

        /**
          * @ 初始化SQL插入语句，该方法使用反射，性能有所降低，请酌情使用
          * @  obj 要删除对象，必须给主键配置特性LdfSQLEntityKey.PrimaryKey=true
          * */
        public void UpdateObject<T>(T obj, params string[] filterFields) where T : class,new()
        {
            PropertyInfo[] piArray = obj.GetType().GetProperties();
            int len = piArray.Length;

            for (int i = 0; i < len; i++)
            {
                PropertyInfo pi = piArray[i];
                if (filterFields.Contains<string>(pi.Name))
                    continue;

                ArrayUpdate.Add(pi.Name, pi.GetValue(obj, null));
            }
        }

        /**
         * @ 提交保存
         * */
        public bool SaveChange()
        {
            Succeed = false;
            int result = 0;
            if (InitSQLWithCmdText())
            {
                result = ExecuteNonQuery();
            }
            // 清空数据
            Parameters.Clear();
            Succeed = result > 0;

            return Succeed;
        }

        /**
         * @ 初始化数据库命令
         * */
        protected override bool InitSQLWithCmdText()
        {
            if (TableName.IsNullOrEmpty())
            {
                throw new ArgumentException("更新目标数据库表名：tablename不能为空！");
            }
            string updateString = GetUpdateFields();
            string whereString = GetCondition();

            SQLCmdText = string.Format("UPDATE {0} SET {1} {2}", TableName, updateString, whereString);
            return SQLCmdText.IsNotNullOrEmpty();
        }

        /**
         * @ 获取要更新的字段
         * */
        private string GetUpdateFields()
        {
            int len = ArrayUpdate.Count;
            if (len == 0)
                return string.Empty;

            int index = 0;
            StringBuilder updateBuilder = new StringBuilder();
            foreach (var key in ArrayUpdate.Keys)
            {
                object value = ArrayUpdate[key];
                DbParameter par = AddParameter(key, value);
                updateBuilder.AppendFormat(" {0}={1}{2}", key, par.ParameterName, index + 1 == len ? "" : ",");
                index++;
            }
            return updateBuilder.ToString();
        }

        #region Properties
        private Dictionary<string, object> arrayUpdate = null;
        /**
         * @ 要更新的字段
         * */
        public Dictionary<string, object> ArrayUpdate
        {
            get
            {
                if (arrayUpdate == null)
                    arrayUpdate = new Dictionary<string, object>();

                return arrayUpdate;
            }
            set { arrayUpdate = value; }
        }
        #endregion
    }
}
