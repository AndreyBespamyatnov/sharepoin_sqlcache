using System.Globalization;

namespace Navicon.SP.Components.SqlCache.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;

    using Dapper;

    using DapperExtensions;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;

    using Navicon.SP.Common;
    using System.Data;

    public enum ErrorCode
    {
        NoError = 0,
        UnknownError = 1,
        EntityDoesNotExist = 2
    }

    public class BaseCRUD
    {
        #region private
        private readonly string _connectionString;
        private readonly SPContentDatabase _contentDatabase;
        private string _databaseName;

        public string GetConnectionString()
        {
            string server = this._contentDatabase.Server;
            const string connectionStringTemplate = "Data Source={0};Initial Catalog={1};Integrated Security=True";

            if (string.IsNullOrEmpty(this._databaseName))
            {
                this._databaseName = Constants.DataBaseName;
            }

            return string.Format(connectionStringTemplate, server, this._databaseName);
        }

        protected BaseCRUD(string databaseName = "")
        {
            this._contentDatabase = SPContext.Current.Site.ContentDatabase;
            this._databaseName = databaseName;
            this._connectionString = this.GetConnectionString();
        }

        protected BaseCRUD(SPSite spSite, string databaseName = "")
        {
            this._contentDatabase = spSite.ContentDatabase;
            this._databaseName = databaseName;
            this._connectionString = this.GetConnectionString();
        }

        protected BaseCRUD(string connectionString, bool useConnectionString = true)
        {
            this._connectionString = connectionString;
        }

        private void Run(Action<SqlConnection> action)
        {
            using (SqlConnection connection = new SqlConnection(this._connectionString))
            {
                connection.Open();
                action(connection);
            }
        }
        #endregion

        #region dapper
        protected IEnumerable<dynamic> GetDynamicList(string sql)
        {
            IEnumerable<dynamic> result = null;

            try
            {
                this.Run(connection => result = connection.Query(sql , commandTimeout: 5000));
            }
            catch (SqlException sqlException)
            {

                Logger.WriteError(sqlException.Message, "BaseCRUD.StoredProcedureList<T>");

                Trace.TraceInformation(sqlException.Message);
                return null;
            }

            return result;
        }

        public ErrorCode Execute(string sql)
        {
            try
            {
                this.Run(connection => connection.Execute(sql));
            }
            catch (SqlException sqlException)
            {

                Logger.WriteError(sqlException.Message, "BaseCRUD.Execute");

                return sqlException.State == 0 ? ErrorCode.UnknownError : (ErrorCode)sqlException.State;
            }

            return ErrorCode.NoError;
        }

        protected ErrorCode StoredProcedureVoid(string sql, dynamic param = null)
        {
            try
            {
                this.Run(connection => SqlMapper.Execute(connection, sql, param));
            }
            catch (SqlException sqlException)
            {

                Logger.WriteError(sqlException.Message, "BaseCRUD.StoredProcedureVoid");

                return sqlException.State == 0 ? ErrorCode.UnknownError : (ErrorCode)sqlException.State;
            }

            return ErrorCode.NoError;
        }

        protected StatusValuePair<T> StoredProcedureSingle<T>(string sql, dynamic param = null)
        {
            StatusValuePair<List<T>> executeDapperList = StoredProcedureList<T>(sql, param);
            if (executeDapperList.HasValue && executeDapperList.Value.Count > 0)
            {
                Type itemType = typeof(T);
                if (itemType == typeof(int))
                {
                    int integerValue = Convert.ToInt32(executeDapperList.Value[0]);
                    bool valueIsZero = integerValue == 0;
                    return new StatusValuePair<T>(executeDapperList.Value[0], ErrorCode.NoError, valueIsZero);
                }

                return executeDapperList.Value.Single();
            }

            return new StatusValuePair<T>(default(T), executeDapperList.ErrorCode);
        }

        protected StatusValuePair<List<T>> StoredProcedureList<T>(string sql, dynamic param = null)
        {
            List<T> result = new List<T>();

            try
            {
                this.Run(connection => result = SqlMapper.Query<T>(connection, sql, param));
            }
            catch (SqlException sqlException)
            {

                Logger.WriteError(sqlException.Message, "BaseCRUD.StoredProcedureList<T>");

                Trace.TraceInformation(sqlException.Message);
                return new StatusValuePair<List<T>>(null, (ErrorCode)sqlException.State);
            }

            return result;
        }
        #endregion

        #region dapper ext
        protected StatusValuePair<T> SingleByID<T>(dynamic id) where T : class
        {
            T result = null;
            try
            {
                this.Run(cn => result = global::DapperExtensions.DapperExtensions.Get<T>(cn, id));
            }
            catch (SqlException sqlException)
            {

                Logger.WriteError(sqlException.Message, "BaseCRUD.SingleByID<T>");

                Trace.TraceInformation(sqlException.Message);
                return new StatusValuePair<T>(default(T), (ErrorCode)sqlException.State);
            }

            if (result == null)
            {
                return new StatusValuePair<T>(default(T), ErrorCode.EntityDoesNotExist);
            }

            return result;
        }

        protected StatusValuePair<T> Create<T>(T entity) where T : class
        {
            try
            {
                this.Run(cn => cn.Insert(entity));
                return entity;
            }
            catch (SqlException sqlException)
            {

                Logger.WriteError(sqlException.Message, "BaseCRUD.Create<T>");

                Trace.TraceInformation(sqlException.Message);
                return new StatusValuePair<T>(default(T), (ErrorCode)sqlException.State);
            }
        }

        protected StatusValuePair<T> Delete<T>(T entity) where T : class
        {
            try
            {
                this.Run(cn => cn.Delete(entity));
                return new StatusValuePair<T>(default(T), ErrorCode.NoError);
            }
            catch (SqlException sqlException)
            {

                Logger.WriteError(sqlException.Message, "BaseCRUD.Delete<T>");

                Trace.TraceInformation(sqlException.Message);
                return new StatusValuePair<T>(default(T), (ErrorCode)sqlException.State);
            }
        }

        protected StatusValuePair<T> Delete<T>(IPredicate predicate) where T : class
        {
            try
            {
                this.Run(cn => cn.Delete<T>(predicate));
                return new StatusValuePair<T>(default(T), ErrorCode.NoError);
            }
            catch (SqlException sqlException)
            {

                Logger.WriteError(sqlException.Message, "BaseCRUD.Delete<T>");

                Trace.TraceInformation(sqlException.Message);
                return new StatusValuePair<T>(default(T), (ErrorCode)sqlException.State);
            }
        }

        protected void Insert<T>(T entity) where T : class
        {
            try
            {
                this.Run(cn => cn.Insert(entity));
            }
            catch (SqlException sqlException)
            {

                Logger.WriteError(sqlException.Message, "BaseCRUD.Insert<T>");

                Trace.TraceInformation(sqlException.Message);
            }
        }

        protected StatusValuePair<T> Update<T>(T entity) where T : class
        {
            try
            {
                this.Run(cn => cn.Update(entity));
                return entity;
            }
            catch (SqlException sqlException)
            {

                Logger.WriteError(sqlException.Message, "BaseCRUD.Update<T>");

                Trace.TraceInformation(sqlException.Message);
                return new StatusValuePair<T>(default(T), (ErrorCode)sqlException.State);
            }
        }

        protected StatusValuePair<List<T>> GetList<T>(IPredicate predicate) where T : class
        {
            try
            {
                List<T> list = null;
                this.Run(cn => list = cn.GetList<T>(predicate).ToList());
                return list;
            }
            catch (SqlException sqlException)
            {

                Logger.WriteError(sqlException.Message, "BaseCRUD.GetList<T>(IPredicate predicate)");

                Trace.TraceInformation(sqlException.Message);
                return new StatusValuePair<List<T>>(null, (ErrorCode)sqlException.State);
            }
        }

        protected List<T> GetListSafe<T>(IPredicate predicate) where T : class
        {
            StatusValuePair<List<T>> listStatusValuePair = this.GetList<T>(predicate);
            return listStatusValuePair.HasValue ? listStatusValuePair.Value : new List<T>();
        }

        protected StatusValuePair<List<T>> GetList<T>() where T : class
        {
            try
            {
                List<T> list = null;
                this.Run(cn => list = cn.GetList<T>().ToList());
                return list;
            }
            catch (SqlException sqlException)
            {
                Logger.WriteError(sqlException.Message, "BaseCRUD.GetList<T>");
                Trace.TraceInformation(sqlException.Message);
                return new StatusValuePair<List<T>>(null, (ErrorCode)sqlException.State);
            }
        }

        protected int GetCount<T>(IPredicate predicate)
             where T : class
        {
            int result = 0;

            try
            {
                this.Run(cn => result = cn.Count<T>(predicate));
            }
            catch (SqlException sqlException)
            {
                Logger.WriteError(sqlException.Message, "GetCount.GetList<T>");
                Trace.TraceInformation(sqlException.Message);
            }

            return result;
        }

        protected StatusValuePair<List<T>> GetPage<T>(IPredicate predicate, List<ISort> sort, int pageNumber, int resultsPerPage) 
            where T : class
        {
            try
            {
                List<T> list = null;
                this.Run(cn => list = cn.GetPage<T>(predicate, sort, pageNumber, resultsPerPage).ToList());
                return list;
            }
            catch (SqlException sqlException)
            {
                Logger.WriteError(sqlException.Message, "BaseCRUD.GetPage<T>");
                Trace.TraceInformation(sqlException.Message);
                return new StatusValuePair<List<T>>(null, (ErrorCode)sqlException.State);
            }
        }

        protected StatusValuePair<T> GetSingle<T>(IPredicate predicate) where T : class
        {
            try
            {
                List<T> list = null;
                this.Run(cn => list = cn.GetList<T>(predicate).ToList());
                T single = list.SingleOrDefault();
                if (single == null)
                {
                    return new StatusValuePair<T>(null, ErrorCode.EntityDoesNotExist);
                }

                return single;
            }
            catch (SqlException sqlException)
            {

                Logger.WriteError(sqlException.Message, "BaseCRUD.GetSingle<T>(IPredicate predicate)");

                return new StatusValuePair<T>(null, (ErrorCode)sqlException.State);
            }
        }

        protected StatusValuePair<bool?> Exists<T>(IPredicate predicate) where T : class
        {
            try
            {
                List<T> list = null;
                this.Run(cn => list = cn.GetList<T>(predicate).ToList());
                int count = list.Count();
                if (count == 0)
                {
                    StatusValuePair<bool?> statusValuePair = new StatusValuePair<bool?>(false, ErrorCode.NoError);
                    return statusValuePair;
                }

                return new StatusValuePair<bool?>(true, ErrorCode.NoError);
            }
            catch (SqlException sqlException)
            {

                Logger.WriteError(sqlException.Message, "BaseCRUD.GetSingle<T>(IPredicate predicate)");

                return new StatusValuePair<bool?>(null, (ErrorCode)sqlException.State);
            }
        }
        #endregion
    }
}