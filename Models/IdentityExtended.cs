using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace TournamentsProject.Models
{
    public static class IdentityExtended
    {
        public static string FirstName(this IPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                ClaimsIdentity claimsIdentity = user.Identity as ClaimsIdentity;
                foreach (var claim in claimsIdentity.Claims)
                {
                    if (claim.Type == "FirstName")
                        return claim.Value;
                }
                return "";
            }
            else
                return "";
        }
    }
}