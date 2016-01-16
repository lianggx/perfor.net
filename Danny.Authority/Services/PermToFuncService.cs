using Danny.Authority.Data;
using Danny.Lib.Helpers;
using Danny.Lib.Helpers.Mssql;
using Danny.Lib.Extension;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Danny.Authority.Services
{
    /**
     * @ 数据管理
     * */
    public class PermToFuncService : DataOperation<PermToFunc>
    {
        #region Identity
        public PermToFuncService() { }
        #endregion

        /**
         * @ 修改
         * */
        public override bool Update(IEnumerable<PermToFunc> menu)
        {
            throw new NotImplementedException("不允许修改");
        }

        /**
         * @ 删除
         * */
        public override bool Delete(IEnumerable<string> id)
        {
            throw new NotImplementedException("未实现，请调用重载方法");
        }

        /**
         * @ 删除
         * */
        public bool Delete(IEnumerable<PermToFunc> id)
        {
            if (id.IsNullOrEmpty())
                return true;

            MssqlDelete delete = new MssqlDelete(TableName);
            foreach (var item in id)
            {
                delete.AddWhere("Perm_ID", item.Perm_ID);
                delete.AddWhere("Func_ID", item.Func_ID);
                delete.SaveChange();
            }
            return true;
        }

        /**
         * @ 获取单条记录
         * */
        public override PermToFunc Get(string id)
        {
            throw new NotImplementedException();
        }

        /**
         * @ 获取单条记录
         * @ id 权限类型编号
         * */
        public List<SQLDataResult> GetMeumByPermId(string id)
        {
            MssqlGetSomeOne pager = new MssqlGetSomeOne(TableName);
            pager.AddWhere("Perm_ID", id);
            pager.AddWhere("DataType", FuncDataType.Menu.ToInt());
            pager.SetOrderBy("DataType", Lib.Enums.SQLExpression.Order.DESC);
            pager.TableAlias = "A";
            string[] fields = { "B.ID", "B.PID", "B.Name", "B.Url", "B.ParentPath", "B.Level", "B.Sort", "A.Access" };
            string leftJoin = "LEFT JOIN MenuData AS B ON B.ID = A.Func_ID";
            List<SQLDataResult> list = pager.Select(fields, leftJoin);

            return list;
        }

        /**
         * @ 新增项
         * */
        public override bool Add(IEnumerable<PermToFunc> menu)
        {
            bool succeess = true;
            if (menu == null)
                return succeess;

            MssqlInsert insert = new MssqlInsert(TableName);
            foreach (var item in menu)
            {
                if (item.Perm_ID.IsNullOrEmpty() || item.Func_ID.IsNullOrEmpty())
                    throw new ArgumentNullException("不能将空数据插入 PermToFunc 表中");
                insert.InsertObject<PermToFunc>(item);
            }
            insert.SaveChange();

            return succeess;
        }

        /**
         * @ 表名称
         * */
        public override string TableName
        {
            get { return "PermToFunc"; }
        }
    }
}
