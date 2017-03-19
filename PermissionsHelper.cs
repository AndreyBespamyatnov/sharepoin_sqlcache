using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;
using Navicon.SP.Common;
using Navicon.SP.Common.Extensions;
using Navicon.SP.Components.SqlCache.Models;
using Navicon.SP.Components.SqlCache.DAL;

namespace Navicon.SP.Components.SqlCache
{
    public class PermissionsHelper
    {
        public static List<RoleSetup> GetRoleSetup(SPUser user)
        {
            List<RoleSetup> roleSetups = new List<RoleSetup>();

            List<int> groups = user.Groups.Cast<SPGroup>().Select(g => g.ID).ToList();
            groups.Add(user.ID);

            ArchiveCacheCRUD crud = new ArchiveCacheCRUD(user.ParentWeb.Site);
            foreach (int griupOrUserId in groups)
            {
                StatusValuePair<List<RoleSetupDb>> result = crud.GetRoleSetup(griupOrUserId);
                if (!result.HasValue || result.ErrorCode != ErrorCode.NoError)
                {
                    continue;
                }

                IEnumerable<RoleSetup> setups = result.Value.Select(v =>
                {
                    RoleSetup rs = new RoleSetup();

                    if (!string.IsNullOrEmpty(v.Lists))
                    {
                        rs.Lists = v.Lists.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                    rs.ListRight = v.ListsRights;
                    rs.Services = new SPFieldLookupValueCollection(v.Service);
                    rs.ServicesRight = v.ServiceRights;
                    rs.Legals = new SPFieldLookupValueCollection(v.Legals);
                    rs.LegalsRight = v.LegalsRights;
                    return rs;
                });

                roleSetups.AddRange(setups);
            }

            return roleSetups;
        }

        public static HashSet<string> GetMyServices(SPUser user)
        {
            List<int> groups = user.Groups.Cast<SPGroup>().Select(g => g.ID).ToList();
            groups.Add(user.ID);

            HashSet<string> myServices = new HashSet<string>();
            ArchiveCacheCRUD crud = new ArchiveCacheCRUD(user.ParentWeb.Site);
            foreach (int griupOrUserId in groups)
            {
                StatusValuePair<List<string>> result = crud.GetMyServices(griupOrUserId);
                if (!result.HasValue || result.ErrorCode != ErrorCode.NoError)
                {
                    continue;
                }
                foreach (string serviceName in  result.Value)
                {
                    myServices.Add(serviceName);
                }
            }

            return myServices;
        }

        public static List<Permissions> GetPermissions(string barCode, SPListItem listItem)
        {
            List<Permissions> permissions = new List<Permissions>();

            string author = listItem.TryGetFieldValue(SPBuiltInFieldId.Author, string.Empty);
            if (!string.IsNullOrWhiteSpace(author))
            {
                SPFieldUserValue authorUserValue = new SPFieldUserValue(listItem.Web, author);
                Permissions authorPermission = new Permissions
                {
                    Barcode = barCode,
                    CanWrite = true,
                    UserGroupId = authorUserValue.User.ID
                };
                permissions.Add(authorPermission);
            }

            try
            {
                using (SPSite evaluatedSite = new SPSite(listItem.ParentList.ParentWeb.Site.ID, SPUserToken.SystemAccount))
                {
                    using (SPWeb evaluatedWeb = evaluatedSite.OpenWeb(listItem.ParentList.ParentWeb.ID))
                    {
                        SPList spList = evaluatedWeb.Lists[listItem.ParentList.ID];
                        SPListItem spListItem = spList.GetItemById(listItem.ID);

                        foreach (SPRoleAssignment assignment in spListItem.RoleAssignments)
                        {
                            bool isAgroup = true;
                            SPGroup aGroup =
                                evaluatedWeb.Groups.Cast<SPGroup>().FirstOrDefault(g => g.ID == assignment.Member.ID);
                            if (aGroup == null)
                            {
                                isAgroup = false;
                            }

                            if (isAgroup)
                            {
                                bool groupCanEdit = false;
                                SPRoleAssignment spRoleAssignment = new SPRoleAssignment(aGroup);
                                SPRoleDefinitionBindingCollection roleDefinitionBindings = spRoleAssignment.RoleDefinitionBindings;
                                foreach (SPRoleDefinition roleDefinitionBinding in roleDefinitionBindings)
                                {
                                    groupCanEdit =
                                        roleDefinitionBinding.BasePermissions.HasFlag(SPBasePermissions.EditListItems);
                                }
                                Permissions newGroupPermission = new Permissions
                                {
                                    Barcode = barCode,
                                    CanWrite = groupCanEdit,
                                    UserGroupId = aGroup.ID
                                };
                                if (
                                    !permissions.Any(
                                        p =>
                                            p.UserGroupId == newGroupPermission.UserGroupId &&
                                            p.CanWrite == newGroupPermission.CanWrite))
                                {
                                    permissions.Add(newGroupPermission);
                                }

                                if (aGroup.Users.Count <= 0)
                                {
                                    continue;
                                }

                                IEnumerable<Permissions> newPermissions = from SPUser aUser in aGroup.Users
                                                                          let canEdit =
                                                                              spListItem.DoesUserHavePermissions(aUser, SPBasePermissions.EditListItems)
                                                                          select new Permissions
                                                                          {
                                                                              Barcode = barCode,
                                                                              CanWrite = canEdit,
                                                                              UserGroupId = aUser.ID
                                                                          };

                                foreach (Permissions newPermission in newPermissions
                                    .Where(
                                        permissionse =>
                                            !permissions.Any(
                                                p =>
                                                    p.UserGroupId == permissionse.UserGroupId &&
                                                    p.CanWrite == permissionse.CanWrite)))
                                {
                                    permissions.Add(newPermission);
                                }
                            }
                            else
                            {
                                SPUser spUser = spListItem.Web.Users.GetByID(assignment.Member.ID);
                                bool canEdit = spListItem.DoesUserHavePermissions(spUser, SPBasePermissions.EditListItems);
                                Permissions newPermission = new Permissions
                                {
                                    Barcode = barCode,
                                    CanWrite = canEdit,
                                    UserGroupId = spUser.ID
                                };
                                if (
                                    !permissions.Any(
                                        p =>
                                            p.UserGroupId == newPermission.UserGroupId &&
                                            p.CanWrite == newPermission.CanWrite))
                                {
                                    permissions.Add(newPermission);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message, ex);
            }

            return permissions;
        }
    }
}