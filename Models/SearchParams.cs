namespace Navicon.SP.Components.SqlCache.Models
{
    using System.Collections.Generic;
    using System.Collections.Specialized;

    using Microsoft.SharePoint;

    public struct SearchParams
    {
        private readonly IEnumerable<string> _barcodes;

        public SearchParams(IEnumerable<string> barcodes, bool? isArchiveElement = null)
            : this()
        {
            this._barcodes = barcodes;
            if (isArchiveElement.HasValue)
            {
                this.IsArchiveElement = isArchiveElement;
            }

            this.UserGroupIds = new List<int>();
        }

        public List<int> UserGroupIds { get; set; }

        public IEnumerable<string> Barcodes
        {
            get { return this._barcodes; }
        }

        public bool? IsArchiveElement { get; set; }
        public List<SpFieldsStructHidden> FieldsStruct { get; set; }

        public struct SpFieldsStructHidden
        {
            public string DisplayName { get; set; }
            public string InternalName { get; set; }
            public StringCollection AvailibleValues { get; set; }
            public string Value { get; set; }
            public string Type { get; set; }
            public SPFieldType FieldType { get; set; }
            public string ListUrl { get; set; } 
        }
    }
}