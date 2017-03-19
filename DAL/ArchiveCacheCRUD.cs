namespace Navicon.SP.Components.SqlCache.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using DapperExtensions;

    using Microsoft.SharePoint;

    using Navicon.SP.Components.BuilderForm.Interfaces.Controllers;
    using Navicon.SP.Components.SqlCache.Models;

    public class ArchiveCacheCRUD : BaseCRUD
    {
        public ArchiveCacheCRUD(SPSite spSite)
            : base(spSite)
        {
            this.CurrentSite = spSite;
        }

        public ArchiveCacheCRUD()
        {
            this.CurrentSite = SPContext.Current.Site;
        }

        private SPSite CurrentSite { get; set; }

        public ErrorCode CreateOrUpdateTable(CacheTableStructXml cacheTableStructXml)
        {
            RemoveNotAvalibleColumns(cacheTableStructXml, new[] {"Id", Constants.BarCodeFieldName, "ItemUrl"});

            const string sql = "EXECUTE [dbo].[P_CacheDb_CreateTable] @CacheTableStructXml";
            string xmlString = cacheTableStructXml.ToString();

            ErrorCode result = this.StoredProcedureVoid(sql, new
            {
                CacheTableStructXml = xmlString
            });

            return result;
        }

        public ErrorCode CreateOrUpdateValues(CacheTableStructXml cacheTableStructXml)
        {
            RemoveNotAvalibleColumns(cacheTableStructXml, new[] {"Id"});

            const string sql = "EXECUTE [dbo].[P_CacheDb_UpdateValues] @CacheTableStructXml";
            string xmlString = cacheTableStructXml.ToString();

            ErrorCode result = this.StoredProcedureVoid(sql, new
            {
                CacheTableStructXml = xmlString
            });

            return result;
        }

        public ErrorCode DeleteExistingPermissions(SPListItem item)
        {
            ErrorCode errorCode = ErrorCode.NoError;

            IPredicate searchPredicate = Predicates.Field<Permissions>(f => f.ItemUniqueID, Operator.Eq, Convert.ToString(item[Constants.ItemUniqueIDFieldName]));
            StatusValuePair<List<Permissions>> foundPermissions = this.GetList<Permissions>(searchPredicate);
            errorCode = foundPermissions.ErrorCode;
            if (foundPermissions.HasValue)
            {
                foreach (Permissions permission in foundPermissions.Value)
                {
                    this.Delete(permission);
                }
            }

            return errorCode;
        }

        public ErrorCode CreateOrUpdatePermissions(IEnumerable<Permissions> permissions)
        {
            ErrorCode errorCode = ErrorCode.NoError;
            foreach (Permissions permission in permissions)
            {
                PredicateGroup searchPredicates = new PredicateGroup
                {
                    Operator = GroupOperator.And,
                    Predicates = new List<IPredicate>(1)
                };
                searchPredicates.Predicates.Add(Predicates.Field<Permissions>(f => f.Barcode, Operator.Eq, permission.Barcode));
                searchPredicates.Predicates.Add(Predicates.Field<Permissions>(f => f.ItemUniqueID, Operator.Eq, permission.ItemUniqueID));
                searchPredicates.Predicates.Add(Predicates.Field<Permissions>(f => f.UserGroupId, Operator.Eq, permission.UserGroupId));

                StatusValuePair<bool?> exists = this.Exists<Permissions>(searchPredicates);
                if (exists.HasValue && exists.Value.HasValue && !exists.Value.Value)
                {
                    StatusValuePair<Permissions> toCreateResult = this.Create(permission);
                    errorCode = toCreateResult.ErrorCode;
                }
                else
                {
                    StatusValuePair<Permissions> existingPermissions = this.GetSingle<Permissions>(searchPredicates);
                    permission.Id = existingPermissions.Value.Id;
                    StatusValuePair<Permissions> toUpdateResult = this.Update(permission);
                    errorCode = toUpdateResult.ErrorCode;
                }
            }
            return errorCode;
        }

        public IEnumerable<dynamic> GetCacheTable(List<int> userGroups, int userId, CacheTableStructXml cacheTableStructXml, string sqlFilterString = null, long rowLimit = 0, SPWeb currentWeb = null)
        {
            if (currentWeb == null)
            {
                currentWeb = SPContext.Current.Web;
            }
            if (!userGroups.Contains(userId))
            {
                userGroups.Add(userId);
            }

            // собираем колонки
            string sqlColums = "";
            foreach (Columns column in cacheTableStructXml.Columns)
            {
                if (!string.IsNullOrWhiteSpace(sqlColums))
                {
                    sqlColums += ",";
                }
                sqlColums += Constants.SqlScriptTablePrefix + "." + column.ColumnName + " as [" + column.ColumnName + "]";
            }

            // собираем запрос на пермишены по пользователю и его группам
            string permissionsFilter = string.Empty;
            if (userGroups.Count > 1)
            {
                foreach (int uId in userGroups)
                {
                    if (permissionsFilter.EndsWith(")"))
                    {
                        permissionsFilter += " OR ";
                    }

                    permissionsFilter += "([UserGroupId] = " + uId + ")";
                }
            }
            else
            {
                permissionsFilter = "[UserGroupId] = " + userGroups.FirstOrDefault();
            }

            StringBuilder sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append("SELECT");
            if (rowLimit > 0)
            {
                sqlStringBuilder.AppendFormat(" TOP {0} ", rowLimit);
            }
            sqlStringBuilder.AppendFormat(" {0} ", sqlColums);
            sqlStringBuilder.Append("FROM");
            sqlStringBuilder.AppendFormat(" {0} as {1} ", cacheTableStructXml.TableName, Constants.SqlScriptTablePrefix);
            sqlStringBuilder.AppendFormat("WHERE ([{0}]=1) ", SP.SqlCache.Common.Box.Fields.IsUpdated);

            if (!currentWeb.UserIsSiteAdmin && !currentWeb.UserIsWebAdmin)
            {
                //Если в талице Permissions не найдено строк для текущих групп пользователя, то считаем, что доступ есть.
                sqlStringBuilder.AppendFormat(" AND ([{0}] IN (SELECT [{0}] FROM [{1}] WHERE ({2})) OR NOT EXISTS(SELECT P.* FROM [{1}] as P WHERE P.[{0}] = D.[{0}])",
                    Constants.ItemUniqueIDFieldName,
                    Constants.PermissionsDataTableName,
                    permissionsFilter);
                sqlStringBuilder.Append(")");
            }

            string sqlString = sqlStringBuilder.ToString();

            // получаем баез доп. фильтров
            if (string.IsNullOrWhiteSpace(sqlFilterString))
            {
                IEnumerable<dynamic> rows = this.GetDynamicList(sqlString);
                return rows;
            }
                // получаем c доп. фильтрами
            else
            {
                IEnumerable<dynamic> rows = this.GetDynamicList(sqlString + " AND (" + sqlFilterString + ")");
                return rows;
            }
        }

        private static void RemoveNotAvalibleColumns(CacheTableStructXml cacheTableStructXml, IEnumerable<string> columnNotAvalibleNames)
        {
            foreach (
                Columns column in
                    columnNotAvalibleNames.Select(
                        s =>
                            cacheTableStructXml.Columns.Where(
                                c => string.Equals(c.ColumnName, s, StringComparison.InvariantCultureIgnoreCase))
                                               .ToList()).SelectMany(invalidColumns => invalidColumns))
            {
                cacheTableStructXml.Columns.Remove(column);
            }
        }

        public AccessType GetAccessRights(int userId, List<int> userGroups, string tableName, string barcode)
        {
            if (!userGroups.Contains(userId))
            {
                userGroups.Add(userId);
            }

            // собираем запрос на пермишенны по пользователю и его группам
            string permissionsFilter = string.Empty;
            if (userGroups.Count > 1)
            {
                foreach (int uId in userGroups)
                {
                    if (permissionsFilter.EndsWith(")"))
                    {
                        permissionsFilter += " OR ";
                    }

                    permissionsFilter += "(P.[UserGroupId] = " + uId + ")";
                }
            }
            else if (userGroups.Any())
            {
                permissionsFilter = "P.[UserGroupId] = " + userGroups.FirstOrDefault();
            }
            if (string.IsNullOrWhiteSpace(permissionsFilter))
            {
                return AccessType.NotAccessible;
            }

            string sqlString = string.Format(@"
                SELECT
	                P.[CanWrite]
                FROM 
	                {0} as {1} 
                inner join [{7}] as P on D.[{6}]=P.[{6}]
                Where 
                (
	                (({8}) AND (P.[{6}] = {9}))
                )
                ",
                tableName,
                Constants.SqlScriptTablePrefix,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                Constants.BarCodeFieldName,
                Constants.PermissionsDataTableName,
                permissionsFilter,
                barcode);

            List<dynamic> rows = this.GetDynamicList(sqlString).ToList();
            if (!rows.Any())
            {
                return AccessType.NotAccessible;
            }

            AccessType accessRights =
                rows.Cast<IDictionary<string, object>>().Any(item => bool.Parse(item["CanWrite"].ToString()))
                    ? AccessType.Write
                    : AccessType.Read;

            return accessRights;
        }

        public StatusValuePair<List<RoleSetupDb>> GetRoleSetup(int userOrGroupId)
        {
            const string sql = "EXECUTE [dbo].[P_CacheDb_GetRoleSetup] @UserId";
            StatusValuePair<List<RoleSetupDb>> result = this.StoredProcedureList<RoleSetupDb>(sql, new
            {
                @UserId = userOrGroupId
            });

            return result;
        }

        public StatusValuePair<List<string>> GetMyServices(int userOrGroupId)
        {
            const string sql = "EXECUTE [dbo].[P_CacheDb_GetMyServices] @UserId";
            StatusValuePair<List<string>> result = this.StoredProcedureList<string>(sql, new
            {
                @UserId = userOrGroupId
            });

            return result;
        }
    }
}