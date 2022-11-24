using Exiled.API.Features.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SCPReplacer
{
    public static class Util
    {
        /// <summary>
        ///  Extension method that allows extracting the SCP number from a Role
        /// </summary>
        /// <param name="role">a player's Role</param>
        /// <returns>e.g. "079"</returns>
        public static string ScpNumber(this Exiled.API.Features.Roles.Role role)
        {
            return Regex.Replace(role.ToString(), @"[^0-9]", "");
        }
    }

}
