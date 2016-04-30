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

using System;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace ZomboMod.Core
{
    public static class ZomboCore
    {
        public const string HEADER = @"
    ______               _             __  __           _ 
   |___  /              | |           |  \/  |         | |
      / / ___  _ __ ___ | |__   ___   | \  / | ___   __| |
     / / / _ \| '_ ` _ \| '_ \ / _ \  | |\/| |/ _ \ / _` |
    / /_| (_) | | | | | | |_) | (_) | | |  | | (_) | (_| |
   /_____\___/|_| |_| |_|_.__/ \___/  |_|  |_|\___/ \__,_|
                                                          
               v1.0.0 for Unturned 3.14.15.0              
                                                          
";

        public static void PreInit()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine( HEADER );
            Console.ForegroundColor = ConsoleColor.White;

            Zombo.Init();

            Provider.onServerDisconnected += OnPlayerDisconnected;
        }

        public static void OnPlayerPreAdded( SteamPlayerID playerId, ref Vector3 point   , ref byte angle                     ,
                                             ref bool isPro        , ref bool isAdmin    , ref int channel  , ref byte face   ,
                                             ref byte hair         , ref byte beard      , ref Color skin   , ref Color color ,
                                             ref bool hand         , ref int shirtItem   , ref int pantsItem, ref int hatItem ,
                                             ref int backpackItem  , ref int vestItem    , ref int maskiTEM                   , 
                                             ref int glassesItem   , ref int[] skinItems , ref EPlayerSkillset skillset       )
        {
            Console.WriteLine( playerId );
        }

        public static void OnPlayerAdded( SteamPlayer player )
        {
            Zombo.Server.ConnectedPlayers.Add( player.playerID.steamID.m_SteamID, new Entity.Player( player ) );
        }

        public static void OnPlayerDisconnected( CSteamID id )
        {
            Zombo.Server.ConnectedPlayers.Remove( id.m_SteamID );
        }

        public static void OnPlayerDamaged( Player player, ref byte amount, ref Vector3 newRagdoll, 
                                            ref EDeathCause newCause, ref ELimb newLimb, ref CSteamID newKiller )
        {
            Console.WriteLine( player + " " + amount + " " + newCause );
        }

        public static void OnPlayerChatted( CSteamID playerId, ref Color color, ref byte mode, ref string text )
        {
            Console.WriteLine("Chatted " + playerId + " " + mode + " " + text );
        }

        public static bool ProcessChat( SteamPlayer player, string text )
        {
            if (!text.StartsWith("/"))
            {
                return true;
            }
            if (player.isAdmin)
            {
                Commander.execute(player.playerID.steamID, text.Substring(1));
            }
            return false;
        }
    }
}
