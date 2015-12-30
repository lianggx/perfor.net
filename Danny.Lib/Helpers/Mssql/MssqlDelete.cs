using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Danny.Lib.Extension;
using System.Reflection;
using System.Data.Common;

namespace Danny.Lib.Helpers.Mssql
{
    /**
     * @ 删除数据的入口
     * */
    public partial class MssqlDelete : SQLHelper, SQLIDataSave
    {
        /**
 * @ 默认构造函数
 * */
        public MssqlDelete() : base() { }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        public MssqlDelete(string tableName)
        {
            SetTableName(tableName);
        }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        public MssqlDelete(string tableName, SQLContext context)
            : base(context)
        {
            SetTableName(tableName);
        }

        /**
         * @ 初始化SQL删除语句，fields和values数组长度必须一致
         * @ field 删除条件字段名称
         * @ values 字段对应的值 
         * @ tableName 要删除的表名
         * */
        public void DeleteObject(string field, object value, string tableName = null)
        {
            string[] fields = { field };
            object[] values = { value };
            int primaryKeyIndex = 0;

            SQLParameter item = AddObject(fields, values, primaryKeyIndex);
            if (tableName.IsNotNull())
            {
                item.TableName = tableName;
            }
            else
                item.TableName = this.TableName;
        }

        /**
          * @ 初始化SQL插入语句，该方法使用反射，性能有所降低，请酌情使用
          * @ obj 要删除对象，必须给主键配置特性LdfSQLEntityKey.PrimaryKey=true
          * */
        public void DeleteObject<T>(T obj) where T : class,new()
        {
            if (obj == null)
                throw new ArgumentNullException("不能删除空对象");

            if (TableName.IsNullOrEmpty())
            {
                TableName = obj.GetType().Name;
            }

            PropertyInfo[] piArray = obj.GetType().GetProperties();
            int len = piArray.Length;
            if (len == 0)
                throw new ArgumentException("没有可以使用的删除条件");

            PropertyInfo pi = null;
            for (int i = 0; i < len; i++)
            {
                if (CheckPrimaryKey(piArray[i]) == false) continue;

                pi = piArray[i];
                break;
            }
            if (pi == null)
            {
                throw new ArgumentException(string.Format("必须对实体类 {0} 进行实体键 LdfSQLEntityKey 的配置，且只能出现一次，请勿使用复合主键", obj.ToString()));
            }
            string field = pi.Name;
            object value = pi.GetValue(obj, null);
            DeleteObject(field, value, TableName);
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
            if (Parameters.Count == 0)
                return false;

            int index = 0;
            StringBuilder delBuilder = new StringBuilder();
            foreach (var item in Parameters)
            {
                DbParameter para = AddParameter(item.Fields[index], item.Values[index]);
                if (item.TableName.IsNullOrEmpty())
                    throw new ArgumentException("参数  item.TableName 不能为空！如果您是使用 DeleteObject(string field, object value, string tableName =null) ，建议您传入表名");
                delBuilder.AppendFormat(" DELETE {0} WHERE {1}={2}", item.TableName, item.Fields[index], para.ParameterName);
                delBuilder.AppendLine();
            }
            SQLCmdText = delBuilder.ToString().ToTrimSpace();
            return true;
        }
    }
}
