namespace Navicon.SP.Components.SqlCache.ViewInterpritator
{
    using System.Web.UI;

    public class PersonalViewsStruct : IHierarchyData
    {
        #region Data
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Link { get; set; }

        public PersonalViewsStruct Parent { get; set; }

        public PersonalViewsStructs Children = new PersonalViewsStructs();
        #endregion

        #region IHierarchyData Members

        // TODO: GetChildren and GetParent actually embeds the 
        // real business logic of the hierarchical structure
        public IHierarchicalEnumerable GetChildren()
        {
            return this.Children;
        }

        public IHierarchyData GetParent()
        {
            return this.Parent;
        }

        public bool HasChildren
        {
            get
            {
                PersonalViewsStructs personalViewsStructs = this.GetChildren() as PersonalViewsStructs;
                return (personalViewsStructs != null) && (personalViewsStructs.Count > 0);
            }
        }

        public object Item
        {
            get { return this; }
        }

        public string Path
        {
            // Some unique id:
            // (1) Easy job if you have a Primary key in the entire hierarchical structure
            // (2) If you don't have uniqueness in the entire hierarchical structure, 
            //     then uniqueness in a certain level will also be enough. Then, you can
            //     achieve the uniqueness in the whole hierarchy something like:
            //     RootId.GrandGrandParentId.GrandParentId.ParentId.MyLevelId
            // But it does not matter really for the control to work properly. 
            // It is required only for you as an application developer to carry on with your 
            // business logic after certain node is selected by the end user and you want to 
            // know which one... something like that.
            get { return this.Id; }
        }

        public string Type
        {
            get { return this.GetType().ToString(); }
        }
        #endregion
    }
}