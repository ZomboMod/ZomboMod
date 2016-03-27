/*
*    ______               _             __  __           _ 
*   |___  /              | |           |  \/  |         | |
*      / / ___  _ __ ___ | |__   ___   | \  / | ___   __| |
*     / / / _ \| '_ ` _ \| '_ \ / _ \  | |\/| |/ _ \ / _` |
*    / /_| (_) | | | | | | |_) | (_) | | |  | | (_) | (_| |
*   /_____\___/|_| |_| |_|_.__/ \___/  |_|  |_|\___/ \__,_|
*
*          This file is part of ZomboMod Project.
*             https://www.github.com/ZomboMod
*
*             Copyright (C) 2016 Leonardosnt
*          ZomboMod is licensed under CC BY-NC-SA.
*   
*/

using System.Collections.Generic;
using System.Linq;
using ZomboMod.Common;
using ZomboMod.Entity;

namespace ZomboMod.Permission.Internal
{
    internal class PermissionProvider : IPermissionProvider
    {
        public IEnumerable<PermissionGroup> Groups => Storage.Groups.Values.AsEnumerable();

        public bool HasPermission( Player player, string permission )
        {
            return HasPermission( player.SteamUser.SteamID.m_SteamID, permission );
        }

        public bool HasPermission( ulong playerId, string permission )
        {
            return GetPermssions( playerId ).Any( p => p.EqualsIgnoreCase( permission ) );
        }

        public PermissionGroup GetGroup( string name )
        {
            return Storage.Groups.GetOrDefault( name, null );
        }

        public IEnumerable<string> GetPermssions( Player player )
        {
            return GetPermssions( player.SteamUser.SteamID.m_SteamID );
        }

        public IEnumerable<string> GetPermssions( ulong playerId )
        {
            return Storage.Groups
                    .Where( g => g.Value.Players.Contains( playerId ) || g.Key.EqualsIgnoreCase( "default" )  )
                    .SelectMany( g => g.Value.Permissions )
                    .Distinct();
        }

        public void Load()
        {
            Storage.Load();
        }

        public void Save()
        {
            Storage.Save();
        }

        internal PermissionProvider()
        {
            Storage = new PermissionStorage();
        }

        internal PermissionStorage Storage;
    }
}
