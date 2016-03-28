using Perfor.Authority.Data;
using Perfor.Lib.Helpers;
using Perfor.Lib.Helpers.Mssql;
using Perfor.Lib.Extension;
using Perfor.Lib.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Authority.Services
{
    /**
     * @ 菜单数据管理
     * */
    public class MenuDataService : DataOperation<MenuData>
    {
        #region Identity
        public MenuDataService() { }
        #endregion

        /**
         * @ 修改菜单
         * */
        public override bool Update(IEnumerable<MenuData> menu)
        {
            bool succeess = true;
            if (menu.IsNullOrEmpty())
                return succeess;

            MssqlUpdate update = new MssqlUpdate(TableName);
            foreach (var item in menu)
            {
                CreatePath(item);
                update.UpdateObject<MenuData>(item);
                update.AddWhere(Primarykey, item.ID);
                update.SaveChange();
            }

            return succeess;
        }

        /**
         * @ 检查菜单是否存在下级
         * */
        public bool HasChildren(string id)
        {
            if (id.IsNullOrEmpty())
                return false;

            MssqlGetSomeOne getsome = new MssqlGetSomeOne(TableName);
            getsome.AddWhere("PID", id);
            getsome.AddWhere("ParentPath", SQLExpression.ExprOperator.Like, id, SQLExpression.JoinType.OR);
            string[] fields = { "TOP 1 ID" };
            List<SQLDataResult> list = getsome.Select(fields, "");
            bool has = list.Count > 0 && (list[0]["ID"] as string).IsNotNullOrEmpty();
            return has;
        }

        /**
         * @ 新增菜单项
         * */
        public override bool Add(IEnumerable<MenuData> menu)
        {
            bool succeess = true;
            if (menu == null)
                return succeess;

            MssqlInsert insert = new MssqlInsert(TableName);
            foreach (var item in menu)
            {
                item.ID = Guid.NewGuid().ToString("N");
                CreatePath(item);
                insert.InsertObject<MenuData>(item);
            }
            insert.SaveChange();

            return succeess;
        }

        /**
         * @ 创建菜单的路径
         * */
        private void CreatePath(MenuData menu)
        {
            if (menu.PID.IsNullOrEmpty())
            {
                menu.PID = "";
                menu.ParentPath = "";
                menu.Level = 1;
                return;
            }

            MssqlGetSomeOne pager = new MssqlGetSomeOne(TableName);
            pager.AddWhere("ID", menu.PID);
            List<MenuData> list = pager.Select<MenuData>();
            if (list.Count == 0)
            {
                menu.PID = "";
                menu.ParentPath = "";
                menu.Level = 1;
                return;
            }
            string joinchar = list[0].ParentPath.IsNullOrEmpty() ? "" : ".";
            menu.ParentPath = string.Format("{0}{1}{2}", list[0].ParentPath, joinchar, menu.PID);
            menu.Level = list[0].Level + 1;
        }

        /**
         * @ 表名称
         * */
        public override string TableName
        {
            get { return "MenuData"; }
        }
    }
}
