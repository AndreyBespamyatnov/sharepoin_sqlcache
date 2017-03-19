using System;

namespace Navicon.SP.Components.SqlCache.Models
{
    public class Permissions
    {
        public long Id { get; set; }
        public string Barcode { get; set; }
        public int UserGroupId { get; set; }
        public bool CanWrite { get; set; }
        public string ItemUniqueID { get; set; }
    }
}