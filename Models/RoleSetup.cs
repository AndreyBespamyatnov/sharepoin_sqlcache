namespace Navicon.SP.Components.SqlCache.Models
{
    using System.Collections.Generic;

    using Microsoft.SharePoint;

    public class RoleSetup
    {
        public List<string> Lists { get; set; }
        public string ListRight { get; set; }
        public SPFieldLookupValueCollection Services { get; set; }
        public string ServicesRight { get; set; }
        public SPFieldLookupValueCollection Legals { get; set; }
        public string LegalsRight { get; set; }
    }

    public class RoleSetupDb
    {
        public string Lists { get; set; }
        public string ListsRights { get; set; }
        public string Service { get; set; }
        public string ServiceRights { get; set; }
        public string Legals { get; set; }
        public string LegalsRights { get; set; }
    }
}