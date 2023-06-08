using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using MEC;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Given an IEnumerable (like a list), get a random element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">a list or other enumerable object</param>
        /// <returns>a random item from that enumerable</returns>
        public static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            int index = UnityEngine.Random.Range(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }

        public static void Replace(this ScpToReplace role)
        {
            var chosenPlayer = role.Volunteers.RandomElement();
            role.Volunteers = null;

            // Late join spawn reason used to help distinguish from moderator forececlass
            if (role.CustomRole is not null)
            {
                Timing.CallDelayed(1f, () => role.CustomRole.AddRole(chosenPlayer));
            }
            else
            {
                chosenPlayer.Role.Set(role.Role, Exiled.API.Enums.SpawnReason.LateJoin);
            }
            Plugin.Singleton.ScpsAwaitingReplacement.Remove(role);

            // Broadcast to everyone that the SCP has been replaced
            // and give a slightly different message to the player that replaced the SCP
            foreach (var p in Player.List)
            {
                if (p == chosenPlayer)
                {
                    // For the player that replaced the SCP:
                    p.Broadcast(5, Plugin.Singleton.Translation.BroadcastHeader +
                        Plugin.Singleton.Translation.ChangedSuccessfullySelfBroadcast.Replace("%NUMBER%", role.Name)
                        );

                    // clear custom roles and effects the user already has
                    foreach (CustomRole customRole in p.GetCustomRoles())
                        customRole.RemoveRole(p);
                    p.DisableAllEffects();
                    continue;
                }
                // for everyone else:
                p.Broadcast(5, Plugin.Singleton.Translation.BroadcastHeader +
                    Plugin.Singleton.Translation.ChangedSuccessfullyEveryoneBroadcast.Replace("%NUMBER%", role.Name)
                    );
            }
            Log.Info($"{chosenPlayer.Nickname} has replaced SCP-{role.Name}");
        }
    }
}