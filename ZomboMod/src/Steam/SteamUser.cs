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


using SDG.Unturned;
using Steamworks;

namespace ZomboMod.Steam
{
    public class SteamUser
    {
        public CSteamID SteamID { get; }

        public CSteamID GroupID { get; }

        public string Name { get; }

        public SteamUser( SteamPlayer player )
        {
            SteamID = player.playerID.steamID;
            GroupID = player.playerID.group;
            Name = player.playerID.playerName;
        }
    }
}