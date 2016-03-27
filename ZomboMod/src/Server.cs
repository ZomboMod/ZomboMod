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

        public IEnumerable<Player> OnlinePlayers
        {
            get { return ConnectedPlayers.AsEnumerable(); } 
        }

        internal Server( ushort port, string map )
        {
            ConnectedPlayers = new List<Player>();

            Port = port;
            Map = map;

            Provider.onServerConnected += PlayerConnectedCallback;
            Provider.onServerDisconnected += PlayerDisconnectedCallback;
        }

        public void Broadcast( params string[] messages )
        {
            throw new NotImplementedException();
        }

        public Player GetPlayer( CSteamID id )
        {
            return ConnectedPlayers.FirstOrDefault(p => p.SteamUser.SteamID == id);
        }

        public Player GetPlayer( string name )
        {
            return ConnectedPlayers.FirstOrDefault( p => p.Name.ContainsIgnoreCase( name ) ); 
        }

        public Player GetPlayer( SteamPlayer steamPlayer )
        {
            return ConnectedPlayers.FirstOrDefault( p => p.Channel.owner == steamPlayer );
        }

        public Player GetPlayer( SDG.Unturned.Player sdgPlayer )
        {
            return ConnectedPlayers.FirstOrDefault( p => p.SDGPlayer == sdgPlayer );
        }

        private void PlayerDisconnectedCallback( CSteamID id )
        {
            ConnectedPlayers.RemoveAll( p => p.SteamUser.SteamID == id );
        }

        private void PlayerConnectedCallback( CSteamID id )
        {
            ConnectedPlayers.Add( new Player( PlayerTool.getPlayer(id) ) );
        }

        internal List<Player> ConnectedPlayers;
    }
}