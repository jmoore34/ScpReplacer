using System.Text.RegularExpressions;

namespace SCPReplacer
{
    public static class Util
    {
        /// <summary>
        /// Extension method that allows extracting the SCP number from a Role
        /// </summary>
        /// <param name="role">a player's Role</param>
        /// <returns>e.g. "079"</returns>
        public static string ScpNumber(this Exiled.API.Features.Roles.Role role)
        {
            return Regex.Replace(role.Type.ToString(), @"[^0-9]", "");
        }
        
        /// <summary>
        /// Extension method that allows extracting the SCP number from a Role name as a string
        /// </summary>
        /// <param name="role">a player's Role name</param>
        /// <returns>e.g. "079"</returns>
        public static string ScpNumber(this string role)
        {
            return Regex.Replace(role, @"[^0-9]", "");
        }
    }
}