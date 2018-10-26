using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace ID_Maker_3_Utilities
{
    class SqliteDatabase
    {
        private string dbPath;

        public string DbPath
        {
            get { return dbPath; }
            set { dbPath = value; }
        }
    }
}
