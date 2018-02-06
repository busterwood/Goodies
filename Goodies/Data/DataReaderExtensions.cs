using BusterWood.Contracts;
using System;
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

    }
}
