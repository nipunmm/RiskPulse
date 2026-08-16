namespace RiskPulse.Services.Login
{
    public static class PermissionPageMapper
    {
        private static readonly Dictionary<string, (string Controller, string Action)> PermissionRouteMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { PermissionCatalog.Dashboard, ("Dashboard", "Index") },
                { PermissionCatalog.Submissions, ("Submissions", "Index") },
                { PermissionCatalog.Assessment, ("Assessment", "Index") },
                { PermissionCatalog.Users, ("Users", "Index") },
                { PermissionCatalog.Roles, ("Roles", "Index") },
                { PermissionCatalog.Units, ("Units", "Index") },
                { PermissionCatalog.Saq, ("SaqTemplates", "Index") },
                { PermissionCatalog.Kri, ("KriTemplates", "Index") },
                { PermissionCatalog.RiskRegister, ("RiskRegisterTemplates", "Index") }
            };

        public static (string Controller, string Action) GetRouteForPermission(string? permissionDesc)
        {
            if (!string.IsNullOrWhiteSpace(permissionDesc) &&
                PermissionRouteMap.TryGetValue(permissionDesc, out var route))
            {
                return route;
            }

            return ("Dashboard", "Index");
        }
    }
}
