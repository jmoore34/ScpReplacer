using Exiled.API.Features;
using MEC;
using PlayerRoles;
using System;
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

        public static void DespawnAndReplace(this Player player)
        {
            player.Broadcast(10, "<color=yellow>You were AFK despawned.</color>");
            Plugin.Singleton.ReplacePlayer(player);
            Timing.CallDelayed(0.5f, () =>
            {
                player.Role.Set(RoleTypeId.Spectator);
            });
        }

        public static bool IsInElevator(this Player player)
        {
            foreach (Lift lift in Lift.List)
            {
                var elevatorRadius = 2.1f;

                var elevatorPosition = lift.Position;
                var playerPosition = player.Position;

                // normalize y position to ignore it (so we can check 2d radius, not spherical radius)
                playerPosition.y = elevatorPosition.y;

                // but we still want to make sure y isn't too far off
                if ((playerPosition - elevatorPosition).sqrMagnitude < elevatorRadius * elevatorRadius
                    && Math.Abs(player.Position.y - lift.Position.y) < 10)
                    return true;
            }
            return false;
        }


    }
}