using System;
using System.Linq;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;
using Navicon.SP.Common;
using WebPart = System.Web.UI.WebControls.WebParts.WebPart;

namespace Navicon.SP.Components.SqlCache
{
    public static class WebPartHelper
    {
        public static void DeleteAllWebPartOfType<T>(SPWeb web) where T : WebPart
        {
            foreach (var list in web.Lists.Cast<SPList>().Where(list => !list.Hidden))
            {
                foreach (var view in list.Views.Cast<SPView>().Where(v => !v.PersonalView))
                {
                    try
                    {
                        using (SPLimitedWebPartManager manager = web.GetLimitedWebPartManager(view.Url, PersonalizationScope.Shared))
                        {
                            var webpartsToDelete = manager.WebParts.OfType<T>().ToArray();
                            for (int i = webpartsToDelete.Length; i != 0; i--)
                            {
                                manager.DeleteWebPart(webpartsToDelete[i - 1]);
                            }
                        } 
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError(string.Format("Ошибка при попытке удалить вебчать из списка: '{0}', из представления: '{1}'", list.RootFolder.Url, view.Url), ex);
                    }
                }
            }
        }
    }
}
