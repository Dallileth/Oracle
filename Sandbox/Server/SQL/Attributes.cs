using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Sandbox.Server.Data
{
    
    public class ForeignKeyAttribute : SQLAttribute
    {
        public string Table { get; private set; }
        //todo: allow override of keynames?
        public ForeignKeyAttribute(string table,[CallerMemberName]string column=null) : base(column)
        {
            Table = table;
        }
    }
    public class ForeignTableAttribute : SQLAttribute
    {
        public string Table { get; private set; } = null;

        /// <summary>
        /// Used when the database is storing a basic datatype (e.g. just an ID,String pair)
        /// </summary>
        public string[] TableKeys { get; private set; } = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="table_override">When describing a collection of basic datatypes, note what table it should go into</param>
        public ForeignTableAttribute(string table_override = null,string[]table_keys=null, [CallerMemberName] string column = null) : base(column)
        {
            Table = table_override;
            TableKeys = table_keys;
        }


    }

    public abstract class SQLAttribute : Attribute
    {
        public string Column { get; private set; }
        public string[] JsonPath { get; private set; }
        public SQLAttribute([CallerMemberName] string column = "")
        {
            Column = column;
        }
    }
    public class KeyAttribute : SQLAttribute
    {
        public KeyAttribute([CallerMemberName] string column = null) : base(column) { }
    }
    

    public class TableAttribute : Attribute
    {
        public string TableName { get; private set; }
        public TableAttribute(string tablename)
        {
            TableName = tablename;
        }
    }
}
