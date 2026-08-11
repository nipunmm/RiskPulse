namespace RiskPulse.Services.AccessControlService
{
    public static class PermissionPageMapper
    {
        private static readonly Dictionary<string, (string Controller, string Action)> PermissionRouteMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "Dashboard", ("Dashboard", "Index") },
                { "Submissions", ("Submissions", "Index") },
                { "Assessment Control", ("AssessmentControl", "Index") },
                { "Form Builder", ("FormBuilder", "Index") },
                { "Users", ("Users", "Index") },
                { "Roles", ("Roles", "Index") }
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
