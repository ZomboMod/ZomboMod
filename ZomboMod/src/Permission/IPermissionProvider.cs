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
using ZomboMod.Entity;

namespace ZomboMod.Permission
{
    /// <summary>
    /// TODO: Explain how implement it.
    /// </summary>
    public interface IPermissionProvider
    {
        IEnumerable<PermissionGroup> Groups { get; }

        /// <see cref="HasPermission(ulong, string)"/>
        bool HasPermission( Player player, string permission );

        /// <summary>
        /// Check if player has the given permission.
        /// </summary>
        /// <param name="playerId">Id of player.</param>
        /// <param name="permission">Permission to check.</param>
        /// <returns>If player has the given permission.</returns>
        bool HasPermission( ulong playerId, string permission );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        PermissionGroup GetGroup( string name );

        IEnumerable<string> GetPermssions( Player player ); 

        IEnumerable<string> GetPermssions( ulong playerId ); 

        /// <summary>
        /// Load permissions
        /// </summary>
        void Load();

        /// <summary>
        /// Save permissions
        /// </summary>
        void Save();
    }
}