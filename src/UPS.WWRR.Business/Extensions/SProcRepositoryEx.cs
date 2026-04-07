using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace UPS.WWRR.Business.Extensions
{
    /// <summary>
    /// Provides extension methods for DbContext to load and execute stored procedures.
    /// </summary>
    public static class SProcRepositoryEx
    {
        /// <summary>
        /// Creates a DbCommand for a given stored procedure.
        /// </summary>
        /// <param name="context">The DbContext instance.</param>
        /// <param name="storedProcName">The name of the stored procedure.</param>
        /// <returns>A DbCommand instance.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when the database connection is not set.</exception>
        [ExcludeFromCodeCoverage(Justification = "SQLite does not support CommandType.StoredProcedure")]
        public static DbCommand LoadStoreProcedure(this DbContext context, string storedProcName)
        {
            var cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = storedProcName;
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            return cmd;
        }

        /// <summary>
        /// Adds SQL parameters to a DbCommand.
        /// </summary>
        /// <param name="cmd">The DbCommand instance to which parameters will be added.</param>
        /// <param name="nameValues">An array of tuples, each containing a parameter name and its corresponding value.</param>
        /// <returns>The DbCommand instance with the added parameters.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when a parameter name is null.</exception>
        public static DbCommand WithSqlParams(this DbCommand cmd, params (string, object)[] nameValues)
        {
            foreach (var pair in nameValues)
            {
                var param = cmd.CreateParameter();
                param.ParameterName = pair.Item1;
                param.Value = pair.Item2 ?? DBNull.Value;
                cmd.Parameters.Add(param);
            }

            return cmd;
        }

        /// <summary>
        /// Executes a stored procedure and returns a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to return.</typeparam>
        /// <param name="command">The DbCommand to execute.</param>
        /// <returns>A list of objects of type T.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when the connection state is not closed or the reader is not set.</exception>
        public static IList<T> ExecuteStoredProcedure<T>(this DbCommand command)
            where T : class
        {
            using (command)
            {
                if (command.Connection.State == System.Data.ConnectionState.Closed)
                    command.Connection.Open();

                try
                {
                    using var reader = command.ExecuteReader();
                    return reader.MapToList<T>();
                }
                finally
                {
                    command.Connection.Close();
                }
            }
        }

        /// <summary>
        /// Executes a stored procedure and allows custom asynchronous handling of the DbDataReader.
        /// Ensures connection open/close lifecycle is managed around the handler.
        /// </summary>
        public static async Task<T> ExecuteStoredProcedureAsync<T>(this DbCommand command, Func<DbDataReader, Task<T>> readerHandler, CancellationToken ct = default)
        {
            using (command)
            {
                if (command.Connection.State == ConnectionState.Closed)
                    await command.Connection.OpenAsync(ct);

                try
                {
                    using var reader = await command.ExecuteReaderAsync(ct);
                    return await readerHandler(reader);
                }
                finally
                {
                    await command.Connection.CloseAsync();
                }
            }
        }

        /// <summary>
        /// Maps the results of a DbDataReader to a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to map to.</typeparam>
        /// <param name="dr">The DbDataReader instance.</param>
        /// <returns>A list of objects of type T.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when the reader is closed.</exception>
        /// <exception cref="System.Data.NoNullAllowedException">Thrown when a column contains no nulls.</exception>
        public static IList<T> MapToList<T>(this DbDataReader dr)
        {
            var objList = new List<T>();
            var props = typeof(T).GetRuntimeProperties();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    T obj = Activator.CreateInstance<T>();
                    foreach (var prop in props)
                    {
                        var val = dr.GetValue(prop.Name);
                        prop.SetValue(obj, val == DBNull.Value ? null : val);
                    }

                    objList.Add(obj);
                }
            }

            return objList;
        }

        /// <summary>
        /// Maps the results of a Async DbDataReader to a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to map to.</typeparam>
        /// <param name="dr">The DbDataReader instance.</param>
        /// <returns>A list of objects of type T.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when the reader is closed.</exception>
        /// <exception cref="System.Data.NoNullAllowedException">Thrown when a column contains no nulls.</exception>
        public static async Task<IList<T>> MapToListAsync<T>(this DbDataReader dr)
        {
            List<T> list = new List<T>();
            IEnumerable<PropertyInfo> props = typeof(T).GetRuntimeProperties();

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    T val = Activator.CreateInstance<T>();
                    foreach (PropertyInfo item in props)
                    {
                        object value = dr.GetValue(item.Name);
                        item.SetValue(val, (value == DBNull.Value) ? null : value);
                    }

                    list.Add(val);
                }
            }

            return list;
        }



    }
}
