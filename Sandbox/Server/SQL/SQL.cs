using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Server.Data
{
    public interface ISQL
    {
        IEnumerable<Dictionary<string, object>> Command(string sql, object input);
        IEnumerable<T> Command<T>(string sql, object input) where T : new();
        IEnumerable<Dictionary<string, object>> StoredProc(string sql, object input);
        IEnumerable<T> StoredProc<T>(string sql, object input);
    }
    public class SQL : ISQL
    {
        public string _connection_string;
        public SQL(string connection_string)
        {
            _connection_string = connection_string;
        }

        static void GenerateSQLParameters(SqlParameterCollection parameters, object model=null)
        {
            if (model == null) return;

            Type modelType = model.GetType();
            var properties = modelType.GetProperties();
            foreach (var property in properties)
            {
                var val = property.GetValue(model);
                /*if (property.PropertyType.IsArray)
                {
                    DataTable table = new DataTable();
                    table.Columns.Add("ID", typeof(int));
                    if (!(val is null))
                        foreach (var id in (int[])val)
                        {
                            table.Rows.Add(id);
                        }
                    val = table;

                    parameters.Add(
                        new SqlParameter
                        {
                            ParameterName = $"@{property.Name}",
                            Value = val ?? DBNull.Value,
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "cam.IDList"
                        });
                }
                else*/
                {
                    if (val is string && (string)val == "undefined")
                        val = null;

                    parameters.AddWithValue($"@{property.Name}", val ?? DBNull.Value);

                }
            }


        }
        
        public IEnumerable<Dictionary<string,object>> Command(string sql, object input=null)
        {
            // before/after
            //      insert into People(!COLS!) values(!VALS!)
            //      insert into People(Name,Password) values(@Name,@Password)
            //
            //      select !COLS! from People where MemberID=3
            //      select Name,Password from People where MemberID=3
            /*if (input != null)
            {
                Type modelType = input.GetType();
                var properties = modelType.GetProperties();
                var names = properties.Select(p => p.Name);

                var cols = string.Join(",", names);
                var vals = string.Join(",", names.Select(n => $"@{n}"));
                sql = sql.Replace("!COLS!", cols);
                sql = sql.Replace("!VALS!", vals);
            }*/
            return Run(CommandType.Text, sql, input);
        }

        /*public IEnumerable<Dictionary<string,object>> PagedCommand()
        {
            DECLARE @PageNumber AS INT, @RowspPage AS INT
            set @PageNumber=2;
            set @RowspPage=3;
            select	*
            from	cam.box_files
            where	1=1
            order by fileid 
            offset ((@PageNumber-1)*@RowspPage) rows
            fetch next @RowspPage rows only

        SELECT * 
            FROM (
               SELECT ROW_NUMBER() OVER (ORDER BY DateTime) As RowNum, * 
               FROM Topic) As a 
        WHERE RowNum BETWEEN 1+(@recsPerPage)*(@page-1) AND @recsPerPage*(@page)


        }*/
        /// <summary>
        /// e.g. Get<(string name,string password)>("select top 5 name,password from users")
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public IEnumerable<T> Command<T>(string sql, object input=null) where T:new()
        {
            foreach(var dict in Run(CommandType.Text, sql, input))
            {
                yield return dict.ToObject<T>();
            }
        }

        public IEnumerable<Dictionary<string, object>> Run(CommandType command,string sql,object input)
        {
            using var conn = new SqlConnection(_connection_string);
            using var cmd = new SqlCommand();

            cmd.Connection = conn;
            cmd.CommandType = command;
            cmd.CommandText = sql;


            if (input!=null)
                GenerateSQLParameters(cmd.Parameters, input);
            conn.Open();
            var reader = cmd.ExecuteReader();
            do
            {
                while (reader.Read())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.GetName(i), reader[i] is DBNull ? null : reader[i]);
                    }

                    yield return row;
                }
            }
            while (reader.NextResult());
            //StopMeasuring();

        }

        public IEnumerable<Dictionary<string, object>> StoredProc(string sql, object input)
            => Run(CommandType.StoredProcedure, sql, input);
        public IEnumerable<T> StoredProc<T>(string sql, object input)
            => Run(CommandType.StoredProcedure, sql, input).Select(o => o.ToObject<T>());

    }

    public static class SQLExtensions
    {
        public static T ToObject<T>(this Dictionary<string, object> row)// where T : new()
        {
            Dictionary<string, PropertyInfo> props = new Dictionary<string, PropertyInfo>();
            var ret = Activator.CreateInstance<T>();
            foreach (var prop in typeof(T).GetProperties())
            {
                props.Add(prop.Name, prop);
            }
            foreach (var key in row.Keys)
            {
                if (props.ContainsKey(key))
                    props[key].SetValue(ret, row[key]);
            }
            return ret;
        }
    }
}
