using BusterWood.Collections;
using BusterWood.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace BusterWood.Data
{
    public static class DataReaderExtensions
    {
        public static bool? GetNullableBoolean(this DbDataReader reader, int ordinal)
        {
            Contract.RequiresNotNull(reader);
            return reader.IsDBNull(ordinal) ? default(bool?) : reader.GetBoolean(ordinal);
        }

        public static byte? GetNullableByte(this DbDataReader reader, int ordinal)
        {
            Contract.RequiresNotNull(reader);
            return reader.IsDBNull(ordinal) ? default(byte?) : reader.GetByte(ordinal);
        }

        public static short? GetNullableInt16(this DbDataReader reader, int ordinal)
        {
            Contract.RequiresNotNull(reader);
            return reader.IsDBNull(ordinal) ? default(short?) : reader.GetInt16(ordinal);
        }

        public static int? GetNullableInt32(this DbDataReader reader, int ordinal)
        {
            Contract.RequiresNotNull(reader);
            return reader.IsDBNull(ordinal) ? default(int?) : reader.GetInt32(ordinal);
        }

        public static long? GetNullableInt64(this DbDataReader reader, int ordinal)
        {
            Contract.RequiresNotNull(reader);
            return reader.IsDBNull(ordinal) ? default(long?) : reader.GetInt64(ordinal);
        }

        public static DateTime? GetNullableDateTime(this DbDataReader reader, int ordinal)
        {
            Contract.RequiresNotNull(reader);
            return reader.IsDBNull(ordinal) ? default(DateTime?) : reader.GetDateTime(ordinal);
        }

        public static float? GetNullableFloat(this DbDataReader reader, int ordinal)
        {
            Contract.RequiresNotNull(reader);
            return reader.IsDBNull(ordinal) ? default(float?) : reader.GetFloat(ordinal);
        }

        public static double? GetNullableDouble(this DbDataReader reader, int ordinal)
        {
            Contract.RequiresNotNull(reader);
            return reader.IsDBNull(ordinal) ? default(double?) : reader.GetDouble(ordinal);
        }

        public static decimal? GetNullableDecimal(this DbDataReader reader, int ordinal)
        {
            Contract.RequiresNotNull(reader);
            return reader.IsDBNull(ordinal) ? default(decimal?) : reader.GetDecimal(ordinal);
        }

        public static Guid? GetNullableGuid(this DbDataReader reader, int ordinal)
        {
            Contract.RequiresNotNull(reader);
            return reader.IsDBNull(ordinal) ? default(Guid?) : reader.GetGuid(ordinal);
        }

        public static char? GetNullableChar(this DbDataReader reader, int ordinal)
        {
            Contract.RequiresNotNull(reader);
            return reader.IsDBNull(ordinal) ? default(char?) : reader.GetChar(ordinal);
        }

        public static string GetNullableString(this DbDataReader reader, int ordinal)
        {
            Contract.RequiresNotNull(reader);
            return reader.IsDBNull(ordinal) ? default(string) : reader.GetString(ordinal);
        }

        public static DbCommand NewStoredProc(this DbConnection connection, string procName)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = procName;
            cmd.CommandType = CommandType.StoredProcedure;
            return cmd;
        }

        public static DbCommand NewQuery(this DbConnection connection, string sql)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            return cmd;
        }

        public static DbParameter AddParameter(this DbCommand command, string name, bool value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.DbType = DbType.Boolean;
            p.Value = value;
            command.Parameters.Add(p);
            return p;
        }

        public static DbParameter AddParameter(this DbCommand command, string name, byte value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.DbType = DbType.Byte;
            p.Value = value;
            command.Parameters.Add(p);
            return p;
        }

        public static DbParameter AddParameter(this DbCommand command, string name, short value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.DbType = DbType.Int16;
            p.Value = value;
            command.Parameters.Add(p);
            return p;
        }

        public static DbParameter AddParameter(this DbCommand command, string name, int value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.DbType = DbType.Int32;
            p.Value = value;
            command.Parameters.Add(p);
            return p;
        }

        public static DbParameter AddParameter(this DbCommand command, string name, long value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.DbType = DbType.Int64;
            p.Value = value;
            command.Parameters.Add(p);
            return p;
        }

        public static DbParameter AddParameter(this DbCommand command, string name, decimal value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.DbType = DbType.Decimal;
            p.Value = value;
            command.Parameters.Add(p);
            return p;
        }

        public static DbParameter AddParameter(this DbCommand command, string name, float value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.DbType = DbType.Single;
            p.Value = value;
            command.Parameters.Add(p);
            return p;
        }

        public static DbParameter AddParameter(this DbCommand command, string name, double value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.DbType = DbType.Double;
            p.Value = value;
            command.Parameters.Add(p);
            return p;
        }

        public static DbParameter AddParameter(this DbCommand command, string name, DateTime value, DbType type = DbType.DateTime2)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.DbType = type;
            p.Value = value;
            command.Parameters.Add(p);
            return p;
        }

        public static DbParameter AddParameter(this DbCommand command, string name, string value, int? length = -1, DbType type = DbType.String)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.DbType = type;
            p.Size = length ?? value?.Length ?? -1;
            p.Value = value;
            command.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Returns a <see cref="List{T}"/> containing a <typeparamref name="T"/> for each record in the <paramref name="reader"/>
        /// </summary>
        public static List<T> ToList<T>(this DbDataReader reader, Func<DbDataReader, T> valueSelector)
        {
            var list = new List<T>();
            while (reader.Read())
            {
                var value = valueSelector(reader);
                list.Add(value);
            }
            return list;
        }

        /// <summary>
        /// Returns a <see cref="ILookup{TKey, TElement}"/> that maps a <typeparamref name="TKey"/> to zero or more <typeparamref name="TValue"/>
        /// </summary>
        public static HashLookup<TKey, TValue> ToLookup<TKey, TValue>(this DbDataReader reader, Func<DbDataReader, TKey> keySelector, Func<DbDataReader, TValue> valueSelector)
        {
            var lookup = new HashLookup<TKey, TValue>();
            while (reader.Read())
            {
                var key = keySelector(reader);
                var value = valueSelector(reader);
                lookup.Add(key, value);
            }
            return lookup;
        }

        /// <summary>
        /// Returns a <see cref="Dictionary{TKey, TValue}"/> containing a key-value for each row of the input <paramref name="reader"/>
        /// </summary>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this DbDataReader reader, Func<DbDataReader, TKey> keySelector, Func<DbDataReader, TKey, TValue> valueSelector)
        {
            var map = new Dictionary<TKey, TValue>();
            while (reader.Read())
            {
                var key = keySelector(reader);
                var value = valueSelector(reader, key);
                map.Add(key, value);
            }
            return map;
        }
    }
}
