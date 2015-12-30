using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using Danny.Lib.Extension;

namespace Danny.Lib.Helpers.Mssql
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
        public void UpdateObject(string[] fields, object[] values, int primaryKeyIndex = -1)
        {
            AddObject(fields, values, primaryKeyIndex);
        }

        /**
          * @ 初始化SQL插入语句，该方法使用反射，性能有所降低，请酌情使用
          * @  obj 要删除对象，必须给主键配置特性LdfSQLEntityKey.PrimaryKey=true
          * */
        public void UpdateObject<T>(T obj) where T : class,new()
        {
            AddObject<T>(obj, Enums.SQLOption.UPDATE);
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

            StringBuilder updateBuilder = new StringBuilder();
            foreach (var item in Parameters)
            {
                if (item.PrimaryKeyIndex == -1)
                    throw new ArgumentNullException("更新操作必须指定主键在参数列表中的索引：PrimaryKeyIndex");
                updateBuilder.AppendFormat(" UPDATE {0} SET ", TableName);

                string[] fields = item.Fields;
                object[] values = item.Values;
                int len = fields.Length;
                DbParameter pkParam = null;
                for (int i = 0; i < len; i++)
                {
                    string field = fields[i];
                    DbParameter para = AddParameter(field, values[i]);
                    updateBuilder.AppendFormat("{0}={1}", field, para.ParameterName);
                    if (i + 1 < len)
                    {
                        updateBuilder.Append(",");
                    }
                    if (i == item.PrimaryKeyIndex)
                        pkParam = para;
                }
                updateBuilder.AppendFormat(" WHERE {0}={1}", fields[item.PrimaryKeyIndex], pkParam.ParameterName);
                updateBuilder.AppendLine();
            }
            SQLCmdText = updateBuilder.ToString().ToTrimSpace();
            return SQLCmdText.IsNotNull();
        }

        #region Properties
        #endregion
    }
}
