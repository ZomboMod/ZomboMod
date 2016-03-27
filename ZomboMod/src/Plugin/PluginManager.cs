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
using System.IO;
using System.Linq;

namespace ZomboMod.Plugin
{
    public class PluginManager
    {
        public PluginLoader             Loader      { get; }
        public IEnumerable<PluginBase>  Plugins     => _plugins.Values;

        internal PluginManager()
        {
            _plugins = new Dictionary<string, PluginBase>();
            Loader = new PluginLoader();
        }

        public PluginBase GetPlugin( string name )
        {
            return _plugins.ContainsKey( name ) ? _plugins[name] : null;
        }

        public PluginBase GetPlugin( Type type )
        {
            return Plugins.FirstOrDefault( p => p.GetType() == type );
        }
        
        public void UnloadPlugin( PluginBase plugin )
        {
            plugin.Unload();
        }
        
        public void LoadPlugin( PluginBase plugin )
        {
            plugin.Load();
        }

        public T GetPlugin<T>() where T : PluginBase
        {
            return (T) GetPlugin( typeof (T) );
        }

        internal void Init()
        {
            foreach ( var file in Directory.GetFiles( Zombo.PluginsFolder, "*.dll", SearchOption.TopDirectoryOnly ) )
            {
                Console.WriteLine( $"Loading plugin '{Path.GetFileNameWithoutExtension( file )}'" );

                try
                {
                    var plugin = Loader.LoadFrom( file );

                    _plugins.Add( plugin.Info.Name, plugin );

                    LoadPlugin( plugin );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( ex );
                }
            }
        }

        private Dictionary<string, PluginBase> _plugins;
    }
}