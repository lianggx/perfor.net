using Danny.Authority.Data;
using Danny.Lib.Helpers;
using Danny.Lib.Helpers.Mssql;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Danny.Authority.Services
{
    public class MenuDataService
    {
        public MenuDataService() { }

        public bool Put(MenuData menu)
        {
            bool succeess = true;
            if (menu == null)
                return succeess;
            menu.ID = Guid.NewGuid().ToString("N");

            MssqlInsert insert = new MssqlInsert("MenuData");
            insert.InsertObject<MenuData>(menu);
            insert.SaveChange();

            return succeess;
        }

        public bool Delete(string id)
        {
            return true;
        }

        public List<MenuData> Get()
        {
            return null;
        }

        public bool Post(MenuData menu)
        {
            return true;
        }
    }
}
