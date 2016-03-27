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


using Steamworks;

using SDGPlayer = SDG.Unturned.Player;

namespace ZomboMod.Steam
{
    public class SteamUser
    {
        public CSteamID SteamID { get; }

        public CSteamID GroupID { get; }

        public string Name { get; }

        public SteamUser( SDGPlayer player )
        {
            SteamID = player.channel.owner.playerID.steamID;
            GroupID = player.channel.owner.playerID.group;
        }
    }
}