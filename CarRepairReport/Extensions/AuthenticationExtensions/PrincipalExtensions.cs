﻿namespace CarRepairReport.Extensions.AuthenticationExtensions
{
    using System.Linq;
    using System.Security.Principal;
    using Microsoft.AspNet.Identity;

    public static class PrincipalExtensions
    {
        public static bool IsInAllRoles(this IPrincipal principal, params string[] roles)
        {
            return roles.All(r => principal.IsInRole(r));
        }

        public static bool IsInAnyRoles(this IPrincipal principal, params string[] roles)
        {
            return roles.Any(r => principal.IsInRole(r));
        }
    }
}