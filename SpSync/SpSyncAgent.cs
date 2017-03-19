namespace Navicon.SP.Components.SqlCache.SpSync
{
    using System;
    using System.Collections.Generic;

    using Microsoft.SharePoint;

    using Navicon.SP.Common;
    using Navicon.SP.Common.Utility;
    using Navicon.SP.Components.SqlCache.EventReceivers;
    using Navicon.SP.Components.SqlCache.ViewInterpritator;

    public sealed class SpSyncAgent : ISpSyncAgent
    {
        private const string FieldAddingName = "NaviconArchive_CacheDb_ListChangeReceiver_FieldAdding";
        private const string FieldUpdatingName = "NaviconArchive_CacheDb_ListChangeReceiver_FieldUpdating";
        private const string FieldDeletingName = "NaviconArchive_CacheDb_ListChangeReceiver_FieldDeleting";
        private const string ItemAddedName = "NaviconArchive_CacheDb_ItemChangeReceiver_ItemAdded";
        private const string ItemUpdatedName = "NaviconArchive_CacheDb_ItemChangeReceiver_ItemUpdated";
        private const string ItemDeletingName = "NaviconArchive_CacheDb_ItemChangeReceiver_ItemDeleting";
        private const string ClassicItemUpdatedName = "NaviconArchive_ClassicRegisterEventReceiver_ItemUpdated";
        private ISpSyncProvider _syncProvider;

        public bool RegisterListToSync(SPList spList, bool ovverideView, bool forceCreating, bool hidden = false)
        {
            this._syncProvider = new SpSyncProvider(SyncType.SpList);

            bool resultFieldAdding =
                UtilityEventReceiver.CreateRecieverWithDeletingPrevious(FieldAddingName,
                    SPEventReceiverType.FieldAdding, spList, typeof (ListChangeReceiver), 10001, SPEventReceiverSynchronization.Synchronous);

            bool resultFieldUpdating =
                UtilityEventReceiver.CreateRecieverWithDeletingPrevious(FieldUpdatingName,
                    SPEventReceiverType.FieldUpdating, spList, typeof (ListChangeReceiver), 10001, SPEventReceiverSynchronization.Synchronous);

            bool resultFieldDeleting =
                UtilityEventReceiver.CreateRecieverWithDeletingPrevious(FieldDeletingName,
                    SPEventReceiverType.FieldDeleting, spList, typeof (ListChangeReceiver), 10001, SPEventReceiverSynchronization.Synchronous);

            bool resultItemAdded =
                UtilityEventReceiver.CreateRecieverWithDeletingPrevious(ItemAddedName,
                    SPEventReceiverType.ItemAdded, spList, typeof (ItemChangeReceiver), 10001, SPEventReceiverSynchronization.Synchronous);

            bool resultItemUpdated =
                UtilityEventReceiver.CreateRecieverWithDeletingPrevious(ItemUpdatedName,
                    SPEventReceiverType.ItemUpdated, spList, typeof (ItemChangeReceiver), 10001, SPEventReceiverSynchronization.Synchronous);

            bool resultItemDeleting =
                UtilityEventReceiver.CreateRecieverWithDeletingPrevious(ItemDeletingName,
                    SPEventReceiverType.ItemDeleting, spList, typeof (ItemChangeReceiver), 10001, SPEventReceiverSynchronization.Synchronous);

            bool resultCreateCachDb = this._syncProvider.CreateDataBaseTable(spList.ParentWeb.Site, spList.RootFolder.ServerRelativeUrl, spList.Fields);

            if (resultFieldAdding && resultFieldUpdating && resultFieldDeleting && resultItemAdded && resultItemUpdated && resultItemDeleting && resultCreateCachDb)
            {
                if (hidden)
                {
                    return true;
                }

                SettingsProvider.Instance(spList.ParentWeb).SetIsCacheDb(spList, true);
                SettingsProvider.Instance(spList.ParentWeb).SetIsOverrideView(spList, ovverideView);
                SettingsProvider.Instance(spList.ParentWeb).SetForceCreating(spList, forceCreating);
                SettingsProvider.Instance(spList.ParentWeb).SetCacheTypeDb(spList, SyncType.SpList);
                SettingsProvider.Instance(spList.ParentWeb).SetTableName(spList, spList.RootFolder.ServerRelativeUrl);
                return true;
            }

            // что то упало, удаляем то что получилось создать
            if (resultFieldAdding)
            {
                UtilityEventReceiver.DeleteReciever(FieldAddingName, spList);
            }

            if (resultFieldUpdating)
            {
                UtilityEventReceiver.DeleteReciever(FieldUpdatingName, spList);
            }

            if (resultFieldDeleting)
            {
                UtilityEventReceiver.DeleteReciever(FieldDeletingName, spList);
            }

            if (resultItemAdded)
            {
                UtilityEventReceiver.DeleteReciever(ItemAddedName, spList);
            }

            if (resultItemUpdated)
            {
                UtilityEventReceiver.DeleteReciever(ItemUpdatedName, spList);
            }

            if (resultItemDeleting)
            {
                UtilityEventReceiver.DeleteReciever(ItemDeletingName, spList);
            }

            SettingsProvider.Instance(spList.ParentWeb).SetIsCacheDb(spList, false);
            return false;
        }

        public bool RegisterCtToSync(SPContentType contentType, bool overrideView, bool forceCreating, bool tableNameAsParentList = true, bool hidden = false)
        {
            this._syncProvider = new SpSyncProvider(SyncType.SpContentType);

            bool resultFieldAdding =
                UtilityEventReceiver.CreateRecieverWithDeletingPrevious(FieldAddingName,
                    SPEventReceiverType.FieldAdding, contentType, typeof (ListChangeReceiver), 10001, SPEventReceiverSynchronization.Synchronous);

            bool resultFieldUpdating =
                UtilityEventReceiver.CreateRecieverWithDeletingPrevious(FieldUpdatingName,
                    SPEventReceiverType.FieldUpdating, contentType, typeof (ListChangeReceiver), 10001, SPEventReceiverSynchronization.Synchronous);

            bool resultFieldDeleting =
                UtilityEventReceiver.CreateRecieverWithDeletingPrevious(FieldDeletingName,
                    SPEventReceiverType.FieldDeleting, contentType, typeof (ListChangeReceiver), 10001, SPEventReceiverSynchronization.Synchronous);

            bool resultItemAdded =
                UtilityEventReceiver.CreateRecieverWithDeletingPrevious(ItemAddedName,
                    SPEventReceiverType.ItemAdded, contentType, typeof (ItemChangeReceiver), 10001, SPEventReceiverSynchronization.Synchronous);

            bool resultItemUpdated =
                UtilityEventReceiver.CreateRecieverWithDeletingPrevious(ItemUpdatedName,
                    SPEventReceiverType.ItemUpdated, contentType, typeof (ItemChangeReceiver), 10001, SPEventReceiverSynchronization.Synchronous);

            bool resultItemDeleting =
                UtilityEventReceiver.CreateRecieverWithDeletingPrevious(ItemDeletingName,
                    SPEventReceiverType.ItemDeleting, contentType, typeof (ItemChangeReceiver), 10001, SPEventReceiverSynchronization.Synchronous);

            string tableName = tableNameAsParentList ? contentType.ParentList.RootFolder.ServerRelativeUrl : contentType.Name;
            bool resultCreateCachDb = this._syncProvider.CreateDataBaseTable(contentType.ParentWeb.Site, tableName, contentType.Fields);

            if (resultFieldAdding && resultFieldUpdating && resultFieldDeleting && resultItemAdded && resultItemUpdated && resultItemDeleting && resultCreateCachDb)
            {
                if (hidden)
                {
                    return true;
                }

                IList<SPContentTypeUsage> usages = SPContentTypeUsage.GetUsages(contentType);
                foreach (SPContentTypeUsage usage in usages)
                {
                    if (!usage.IsUrlToList)
                    {
                        continue;
                    }

                    SPList usageList = contentType.ParentWeb.GetList(usage.Url);

                    SettingsProvider.Instance(contentType.ParentWeb).SetIsCacheDb(usageList, true);
                    SettingsProvider.Instance(contentType.ParentWeb).SetIsOverrideView(usageList, overrideView);
                    SettingsProvider.Instance(contentType.ParentWeb).SetForceCreating(usageList, forceCreating);
                    SettingsProvider.Instance(contentType.ParentWeb).SetCacheTypeDb(usageList, SyncType.SpContentType);
                    SettingsProvider.Instance(contentType.ParentWeb).SetTableName(usageList, tableName);
                }

                return true;
            }

            // что то упало, удаляем то что получилось создать
            if (resultFieldAdding)
            {
                UtilityEventReceiver.DeleteReciever(FieldAddingName, contentType);
            }

            if (resultFieldUpdating)
            {
                UtilityEventReceiver.DeleteReciever(FieldUpdatingName, contentType);
            }

            if (resultFieldDeleting)
            {
                UtilityEventReceiver.DeleteReciever(FieldDeletingName, contentType);
            }

            if (resultItemAdded)
            {
                UtilityEventReceiver.DeleteReciever(ItemAddedName, contentType);
            }

            if (resultItemUpdated)
            {
                UtilityEventReceiver.DeleteReciever(ItemUpdatedName, contentType);
            }

            if (resultItemDeleting)
            {
                UtilityEventReceiver.DeleteReciever(ItemDeletingName, contentType);
            }

            SettingsProvider.Instance(contentType.ParentList.ParentWeb).SetIsCacheDb(contentType.ParentList, false);
            return false;
        }

        public bool Disable(dynamic ctOrList)
        {
            try
            {
                SPList parentList = null;
                if (ctOrList is SPContentType)
                {
                    parentList = ctOrList.ParentList;
                }
                else if (ctOrList is SPList)
                {
                    parentList = ctOrList;
                }
                else
                {
                    throw new Exception("Переменнная ctOrList может быть или SPContentType или SPList");
                }

                UtilityEventReceiver.DeleteReciever(FieldAddingName, ctOrList);
                UtilityEventReceiver.DeleteReciever(FieldUpdatingName, ctOrList);
                UtilityEventReceiver.DeleteReciever(FieldDeletingName, ctOrList);
                UtilityEventReceiver.DeleteReciever(ItemAddedName, ctOrList);
                UtilityEventReceiver.DeleteReciever(ItemUpdatedName, ctOrList);
                UtilityEventReceiver.DeleteReciever(ItemDeletingName, ctOrList);

                SettingsProvider.Instance(parentList.ParentWeb).SetIsCacheDb(parentList, false);
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message, ex);
                return false;
            }
        }

        public void Dispose()
        {
            if (this._syncProvider != null)
            {
                this._syncProvider.Dispose();
            }
        }
    }
}