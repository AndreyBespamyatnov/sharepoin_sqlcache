namespace Navicon.SP.Components.SqlCache.ViewInterpritator
{
    using System;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    using DevExpress.Web.ASPxClasses;
    using DevExpress.Web.ASPxTreeView;

    using Microsoft.SharePoint;

    using Navicon.SP.Common;
    using Navicon.SP.Common.Utility;
    using Navicon.SP.CONTROLTEMPLATES.Navicon.SqlCache;

    public class PersonalViewMenuBuilder
    {
        public const string ViewHrefFormat = "/_layouts/15/Navicon/SqlCache/PersonalViews.aspx?List={0}&PersonalView={1}";
        private const string PersonalViewHeadText = "Личные представления";
        public void BuildPersonalViewMenu(SPView view, Page page, SPList spList = null)
        {
            try
            {
                if (view == null && spList == null)
                {
                    return;
                }

                HtmlGenericControl personalViewDiv = UtilityControl.FindControl<HtmlGenericControl>(page, "divForPersonalView", false).FirstOrDefault();
                if (personalViewDiv == null)
                {
                    return;
                }

                HtmlGenericControl control = new HtmlGenericControl(HtmlTextWriterTag.Div.ToString());
                control.Attributes.Add(HtmlTextWriterAttribute.Class.ToString(), "nav-leftNavigationContainer");

                ASPxTreeView treeControl = new ASPxTreeView
                {
                    EnableViewState = false,
                    TextField = ListViewDelegateControls.NavigationElementDisplayNameAttr,
                    NodeLinkMode = ItemLinkMode.TextOnly,
                    NavigateUrlField = ListViewDelegateControls.NavigationElementLinkAttr,
                    ID = "treePersonalView"
                };

                treeControl.Images.CollapseButton.Url = "/_layouts/15/images/Navicon/LeftNavigation/minus.png";
                treeControl.Images.ExpandButton.Url = "/_layouts/15/images/Navicon/LeftNavigation/plus.png";

                PersonalViewsStructs personalViewsStructs = new PersonalViewsStructs();
                PersonalViewsStructs childrensStructs = new PersonalViewsStructs();
                PersonalViewsStruct rootPersonalView = new PersonalViewsStruct
                {
                    DisplayName = PersonalViewHeadText,
                    Id = Guid.NewGuid().ToString().Replace("-", "_"),
                    Link = "javascript:void(0)",
                    Children = childrensStructs
                };
                personalViewsStructs.Add(rootPersonalView);

                SPViewCollection spViewCollection = spList != null ? spList.Views : view.ParentList.Views;

                childrensStructs.AddRange(from SPView spView in spViewCollection
                                          where spView.PersonalView
                                          select new PersonalViewsStruct
                                          {
                                              DisplayName = spView.Title,
                                              Link = string.Format(ViewHrefFormat, spView.ParentList.ID, spView.ID),
                                              Id = spList != null ? spList.ID.ToString().Replace("-", "_") : view.ID.ToString().Replace("-", "_")
                                          });

                if (personalViewsStructs.Count == 1 && personalViewsStructs[0].HasChildren)
                {
                    control.Controls.Add(treeControl);
                    personalViewDiv.Controls.Add(control);
                    personalViewDiv.Visible = true;
                    treeControl.DataSource = personalViewsStructs;
                    treeControl.DataBind();
                    treeControl.ExpandAll();
                }
            }
            catch (Exception ex)
            {
                Logger.ShowErrorOnPage("Не удалось изменить/сохранить отображение ", ex);
            }
        }
    }
}