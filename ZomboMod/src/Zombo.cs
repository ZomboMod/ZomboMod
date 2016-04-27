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
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SDG.Unturned;
using Steamworks;
using ZomboMod.Configuration;
using ZomboMod.Permission;
using ZomboMod.Permission.Internal;
using ZomboMod.Plugin;

namespace ZomboMod
{
    public static class Zombo
    {
        /// <summary>
        /// Singleton instance of Unturned Server.
        /// </summary>
        public static Server Server { get; private set; }

        /// <summary>
        /// Singleton instance of Unturned world.
        /// </summary>
        public static World  World { get; private set; }

        /// <summary>
        /// Singleton instance of Zombo plugin manager.
        /// </summary>
        public static PluginManager PluginManager { get; private set; }

        /// <summary>
        /// Server instance name.
        /// </summary>
        public static string InstanceName { get; private set; }

        /// <summary>
        /// Zombo folder.
        /// </summary>
        public static string Folder { get; private set; }
        
        /// <summary>
        /// Plugins folder.
        /// </summary>
        public static string PluginsFolder { get; private set; }

        /// <summary>
        /// Settings
        /// </summary>
        public static ZomboSettings Settings { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        public static IPermissionProvider PermissionProvider { get; private set; }

        /// <summary>
        /// Called by ZomboCore.
        /// </summary>
        internal static void Init()
        {
            if ( Server != null )
            {
                throw new InvalidOperationException( "Zombo already initalized!" );
            }

            InstanceName    = Dedicator.serverID;
            Folder          = $"Servers/{InstanceName}/Zombo/";
            PluginsFolder   = Folder + "/Plugins/";

            if ( !Directory.Exists( Folder ) )
            {
                Directory.CreateDirectory( Folder );
            }

            if ( !Directory.Exists( PluginsFolder ) )
            {
                Directory.CreateDirectory( PluginsFolder );
            }

            Settings = new ZomboSettings();

            var settingsFile = Folder + "Settings.json";

            if ( File.Exists( settingsFile ) )
            {
                Settings.Load( settingsFile );
            }
            else
            {
                Settings.LoadDefault();
                Settings.Save( settingsFile );
            }

            Server  = new Server( Settings.Server.Port, Settings.Server.Map );
            World   = new World();

            Server.IsPvp = Settings.Server.EnablePvp;
            Server.GameMode = Settings.Server.GameMode;
            Server.CameraMode = Settings.Server.CameraMode;
            Server.SecurityMode = Settings.Server.Security;
            Server.MaxPlayers = Settings.Server.MaxPlayers;
            Server.Name = Settings.Server.Name;
            Server.Password = Settings.Server.Password;
            Server.Timeout = Settings.Server.Timeout;

            // TODO LOGGER
            Console.WriteLine();
            Console.WriteLine( "Pvp: {0}", Server.IsPvp ? "Enabled" : "Disabled" );
            Console.WriteLine( "GameMode: {0}", Server.GameMode );
            Console.WriteLine( "CameraMode: {0}", Server.CameraMode );
            Console.WriteLine( "SecurityMode: {0}", Server.SecurityMode );
            Console.WriteLine( "MaxPlayers: {0}", Server.MaxPlayers );
            Console.WriteLine( "Name: {0}", Server.Name );
            Console.WriteLine( "Password: {0}", string.IsNullOrEmpty( Server.Password ) ? "None" : Server.Password );
            Console.WriteLine( "Timeout: {0}", Server.Timeout );
            Console.WriteLine();

            PermissionProvider = new PermissionProvider();
            PermissionProvider.Load();
            PermissionProvider.Save();

            PluginManager = new PluginManager();
            PluginManager.Init();

            Commander.register( new TestCommand() );
        }
    }

    /*
        TODO:
         just for test
    */
    public class TestCommand : Command
    {
        public TestCommand()
        {
            _command = "test";
        }

        protected override void execute( CSteamID executorID, string parameter )
        {
            var player = Zombo.Server.OnlinePlayers.FirstOrDefault();
            
            Console.WriteLine(Zombo.Server.OnlinePlayers);
        }
    }

    public class ZomboSettings : JsonConfig
    {
        public ServerSettings Server { get; set; }

        public override void Load( string filePath )
        {
            try
            {
                var json = JObject.Parse( File.ReadAllText( filePath ) );
                
                // Only for validation.
                EGameMode gamemode;
                ESteamSecurity security;
                ECameraMode camera;
                int maxPlayers, port;
                
                if ( !TryParseEnum( json["Server"]["GameMode"]?.ToString(), out gamemode ) || gamemode == EGameMode.ANY )
                {
                    throw new ArgumentException( $"Invalid GameMode '{json["GameMode"]}'. Expected 'EASY, NORMAL, HARD or PRO'." );
                }
                
                if ( !TryParseEnum( json["Server"]["Security"]?.ToString(), out security ) )
                {
                    throw new ArgumentException( $"Invalid Security '{json["Security"]}'. Expected 'SECURE, INSECURE or LAN'." );
                }
                
                if ( !TryParseEnum( json["Server"]["CameraMode"]?.ToString(), out camera ) || camera == ECameraMode.ANY )
                {
                    throw new ArgumentException( $"Invalid CameraMode '{json["CameraMode"]}'. Expected 'FIRST, THIRD or BOTH'." );
                }
                
                
                if ( !int.TryParse( json["Server"]["MaxPlayers"]?.ToString(), out maxPlayers ) || (maxPlayers < byte.MinValue || maxPlayers > byte.MaxValue) )
                {
                    throw new ArgumentException( $"Invalid MaxPlayers '{json["MaxPlayers"]}'. Expected a number between 0 and 255.");
                }
                
                if ( !int.TryParse( json["Server"]["Port"]?.ToString(), out port ) || (port < ushort.MinValue || maxPlayers > ushort.MaxValue) )
                {
                    throw new ArgumentException( $"Invalid Port '{json["Port"]}'. Expected a number between 0 and 65535.");
                }
                
                base.Load( filePath );
            }
            catch (Exception ex)
            {
                Console.WriteLine( $"Invalid configuration '{FileName}'.");
                Console.WriteLine( ex );
                LoadDefault();
            }
        }

        public void LoadDefault()
        {
            Server = new ServerSettings {
                GameMode = EGameMode.NORMAL,
                CameraMode = ECameraMode.BOTH,
                Security = ESteamSecurity.SECURE,
                Name = "Zombo Server",
                Password = string.Empty,
                Map = "PEI",
                EnablePvp = true,
                Port = 27015,
                MaxPlayers = 32,
                Timeout = .75f
            };
        }

        public struct ServerSettings
        {
            public bool EnablePvp;
            public ushort Port;
            public byte MaxPlayers;
            public float Timeout;
            public string Name;
            public string Password;
            public string Map;

            [JsonConverter( typeof( StringEnumConverter ) )]
            public EGameMode GameMode;

            [JsonConverter( typeof( StringEnumConverter ) )]
            public ECameraMode CameraMode;

            [JsonConverter( typeof( StringEnumConverter ) )]
            public ESteamSecurity Security;
        }


        // TODO: move
        private static bool TryParseEnum<T>( string raw, out T ret )
        {
            try
            {
                ret =  (T) Enum.Parse( typeof (T), raw, true );
                return true;
            }
            catch (Exception)
            {
                ret = default(T);
                return false;
            }
        }
    }
}