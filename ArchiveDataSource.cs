using System;

namespace Navicon.SP.Components.SqlCache
{
    public struct ArchiveDataSource
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string EditUrl { get; set; }
        public string BarCode { get; set; }

        public string LP { get; set; }

        public string SE { get; set; }

        public string Contractor { get; set; }

        public string DocumentType { get; set; }

        public string DocumentKind { get; set; }

        public DateTime? DocumentDate { get; set; }
        
        public string DocumentNumber { get; set; }

        public string Currency { get; set; }
        public string BPStatus { get; set; }
        public string SumPlusVAT { get; set; }
        public string Description { get; set; }
        public string RegNumber { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string Status { get; set; }
        public string ListID { get; set; }
        public string ItemID { get; set; }

        public string FullId { get; set; }
    }
}