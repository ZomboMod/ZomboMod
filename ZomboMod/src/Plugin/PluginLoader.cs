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
using System.Reflection;

namespace ZomboMod.Plugin
{
    public class PluginLoader
    {
        /// <summary>
        /// Load plugin from a file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Instace of loaded plugin</returns>
        /// <see cref="LoadFrom(Assembly)"/>
        public PluginBase LoadFrom( string path )
        {
            var rawAsm = File.ReadAllBytes( path );
            
            return LoadFrom( Assembly.Load( rawAsm ) );
        }

        /// <summary>
        /// Load plugin from assembly (Does not call the <see cref="PluginBase.OnLoad"/>)
        /// </summary>
        /// <param name="asm">Target assembly</param>
        /// <returns>Instance of loaded plugin.</returns>
        public PluginBase LoadFrom( Assembly asm )
        {
            var pluginType = ( from t in asm.GetTypes() 
                               where typeof(PluginBase).IsAssignableFrom( t )
                               where !t.IsAbstract
                               select t ).First();

            if ( !pluginType.GetConstructors().Any() )
            {
                throw new InvalidOperationException( $"Cannot load '{pluginType}' because it has no public constructors." );
            }

            var pluginInfoAttr = pluginType.GetCustomAttributes( typeof (PluginInfo), false ).FirstOrDefault();

            if ( pluginInfoAttr == null )
            {
                throw new InvalidOperationException( $"Cannot load '{pluginType}' because it doesn't have the 'PluginInfo' attribute." );
            }

            return Activator.CreateInstance( pluginType, pluginInfoAttr ) as PluginBase;
        }
    }
}