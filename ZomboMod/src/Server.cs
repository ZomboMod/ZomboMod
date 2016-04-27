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
using System.Collections.Generic;
using System.Linq;
using SDG.Unturned;
using Steamworks;
using ZomboMod.Common;
using Player = ZomboMod.Entity.Player;

namespace ZomboMod
{
    public class Server
    {
        public byte MaxPlayers
        {
            get { return Provider.maxPlayers; }
            set { Provider.maxPlayers = value; }
        }

        public uint Ip
        {
            get { return Provider.ip; }
        }

        public ushort Port
        {
            get { return Provider.port; }
            private set { Provider.port = value; }
        }

        public string Name
        {
            get { return Provider.serverName; }
            set { Provider.serverName = value; }
        }

        public string Password
        {
            get { return Provider.serverPassword; }
            set { Provider.serverPassword = value; }
        }

        public string Map
        {
            get { return Provider.map; }
            private set { Provider.map = value; }
        }

        public bool IsPvp
        {
            get { return Provider.isPvP; }
            set { Provider.isPvP = value; }
        }
        
        public bool EnableCheats
        {
            get { return Provider.hasCheats; }
            set { Provider.hasCheats = value; }
        }

        public EGameMode GameMode
        {
            get { return Provider.mode; }
            set { Provider.mode = value; }
        }

        public ECameraMode CameraMode
        {
            get { return Provider.camera; }
            set { Provider.camera = value; }
        }

        public ESteamSecurity SecurityMode
        {
            get { return Dedicator.security; }
            set { Dedicator.security = value; }
        }

        public float Timeout
        {
            get { return Provider.timeout; }
            set { Provider.timeout = value; }
        }

        public float Tps
        {
            get { return Provider.debugTPS; }
        }

        public IEnumerable<Player> OnlinePlayers
        {
            get { return ConnectedPlayers.Values.AsEnumerable(); } 
        }

        public void Broadcast( params string[] messages )
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            Provider.shutdown();
        }

        public void Shotdown( string reason )
        {
            /*
                TODO kick players and shutdown
            */
            throw new NotImplementedException();
        }

        public Player GetPlayer( CSteamID id )
        {
            return ConnectedPlayers.GetOrDefault( id.m_SteamID, null );
        }

        public Player GetPlayer( string name )
        {
            throw new NotImplementedException();
        }

        public Player GetPlayer( SteamPlayer steamPlayer )
        {
            return ConnectedPlayers.Values.FirstOrDefault( p => p.Channel.owner == steamPlayer );
        }

        public Player GetPlayer( SDG.Unturned.Player sdgPlayer )
        {
            return ConnectedPlayers.Values.FirstOrDefault( p => p.SDGPlayer == sdgPlayer );
        }

        private void PlayerDisconnectedCallback( CSteamID id )
        {
        }

        internal Server( ushort port, string map )
        {
            ConnectedPlayers = new Dictionary<ulong, Player>();

            Port = port;
            Map = map;
        }

        /*
            Key -> Player64ID
            Value -> Player
        */
        internal Dictionary<ulong, Player> ConnectedPlayers;
    }
}