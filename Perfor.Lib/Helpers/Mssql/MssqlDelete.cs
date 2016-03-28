using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Perfor.Lib.Extension;
using System.Reflection;
using System.Data.Common;

namespace Perfor.Lib.Helpers.Mssql
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
            string whereString = GetCondition();
            if (TableName.IsNullOrEmpty())
                throw new ArgumentException("参数  TableName 不能为空！");
            SQLCmdText = string.Format(" DELETE {0} {1}", TableName, whereString);
            return true;
        }
    }
}
