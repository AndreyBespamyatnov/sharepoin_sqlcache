namespace Navicon.SP.Components.SqlCache
{
    using Navicon.SP.SqlCache.Common.Box;

    public static class Constants
    {
        public const string SqlScriptTablePrefix = "D";
        public const string PermissionsDataTableName = "Permissions";
        public const string DataBaseName = "Navicon.SqlCache";
        public const string BarCodeFieldName = Fields.BarCode;
        public const string ItemUniqueIDFieldName = Fields.ItemUniqueID;
        
        public const string ReadPermisionFieldName = Fields.RP;
        public const string WritePermisionFieldName = Fields.WP;
        public const string ColumnSpPrefix = "sp_";
    }
}