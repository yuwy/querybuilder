using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers.Bindings;

namespace SqlKata
{
    public class SqlResult
    {
        public Query Query { get; set; }
        public string RawSql { get; set; } = "";
        public List<object> Bindings { get; set; } = new List<object>();
        
        public string Sql { get; set; } = "";
        public Dictionary<string, object> NamedBindings = new Dictionary<string, object>();

        private static readonly Type[] NumberTypes =
        {
            typeof(int),
            typeof(long),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(short),
            typeof(ushort),
            typeof(ulong),
        };

        public override string ToString()
        {
            return Helper.ReplaceAll(RawSql, "?", i =>
            {
                if (i >= Bindings.Count)
                {
                    throw new Exception(
                        $"Failed to retrieve a binding at the index {i}, the total bindings count is {Bindings.Count}");
                }

                var value = Bindings[i];

                if (value == null)
                {
                    return "NULL";
                }

                if (NumberTypes.Contains(value.GetType()))
                {
                    return value.ToString();
                }

                if (value is DateTime date)
                {
                    return "'" + date.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }

                if (value is bool vBool)
                {
                    return vBool ? "true" : "false";
                }

                if (value is Enum vEnum)
                {
                    return Convert.ToInt32(vEnum) + $" /* {vEnum} */";
                }

                // fallback to string
                return "'" + value.ToString() + "'";
            });
        }

        [Obsolete("Please use Compiler.Compile(IEnumerable<Query>)")]
        public static SqlResult operator +(SqlResult a, SqlResult b)
        {
            var sql = a.RawSql + ";" + b.RawSql;

            var bindings = a.Bindings.Concat(b.Bindings).ToList();

            var result = new SqlResult
            {
                RawSql = sql,
                Bindings = bindings
            };
            
            //@hack Won't work for results generated by compilers that use custom binder
            var binder = new SqlResultBinder();
            binder.BindNamedParameters(result);

            return result;
        }
    }
}