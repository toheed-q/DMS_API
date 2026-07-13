namespace DMS_Backend.Common
{
    /// <summary>The three roles in the system.</summary>
    public static class Roles
    {
        /// <summary>Full access to everything.</summary>
        public const string Admin = "Admin";

        /// <summary>Office/desktop staff. Access governed by their permission flags.</summary>
        public const string User = "User";

        /// <summary>Field salesman (mobile). Scoped to their OWN data only.</summary>
        public const string Salesman = "Salesman";
    }

    /// <summary>
    /// Granular permissions — these mirror the CanAccess* flags on the User record,
    /// and are issued as claims in the JWT.
    /// </summary>
    public static class Permissions
    {
        public const string ClaimType = "perm";

        public const string POS = "POS";
        public const string Products = "Products";
        public const string StockViewer = "StockViewer";
        public const string Shops = "Shops";
        public const string Routes = "Routes";
        public const string Categories = "Categories";
        public const string BillsHistory = "BillsHistory";
        public const string Employees = "Employees";
        public const string Companies = "Companies";
    }
}
