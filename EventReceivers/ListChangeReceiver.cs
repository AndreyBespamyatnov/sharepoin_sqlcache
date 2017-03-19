namespace Navicon.SP.Components.SqlCache.EventReceivers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.SharePoint;

    using Navicon.SP.Common;
    using Navicon.SP.Components.SqlCache.SpSync;
    using Navicon.SP.Components.SqlCache.ViewInterpritator;

    public class ListChangeReceiver : SPListEventReceiver
    {
        public override void FieldAdding(SPListEventProperties properties)
        {
            if (this.UpdateCacheDataTable(properties))
            {
                base.FieldAdding(properties);
            }
        }

        public override void FieldUpdating(SPListEventProperties properties)
        {
            if (this.UpdateCacheDataTable(properties))
            {
                base.FieldUpdating(properties);
            }
        }

        public override void FieldDeleting(SPListEventProperties properties)
        {
            if (this.UpdateCacheDataTable(properties, true))
            {
                base.FieldDeleting(properties);
            }
        }

        private bool UpdateCacheDataTable(SPListEventProperties properties, bool isDeleted = false)
        {
            const string errorMessage = "Произошла ошибка при изменении кеша.";
            try
            {
                bool result = false;
                List<SyncType> syncTypes = SettingsProvider.Instance(properties.Web).CacheType(properties.List);

                ISpSyncProvider syncProvider;
                switch (syncTypes.Count)
                {
                    case 0:
                        return false;
                    case 1:
                        syncProvider = new SpSyncProvider(syncTypes.FirstOrDefault());
                        result = syncProvider.UpdateDataTableColumns(properties.List, properties.FieldXml, isDeleted);
                        break;
                    default:
                        foreach (SyncType syncType in syncTypes)
                        {
                            syncProvider = new SpSyncProvider(syncType);
                            result = syncProvider.UpdateDataTableColumns(properties.List, properties.FieldXml, isDeleted);
                        }
                        break;
                }

                if (result)
                {
                    return true;
                }

                throw new Exception(errorMessage);
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message, "ListChangeReceiver.UpdateCacheDataTable()");

                properties.Status = SPEventReceiverStatus.CancelWithError;
                properties.ErrorMessage = errorMessage;
                return false;
            }
        }
    }
}