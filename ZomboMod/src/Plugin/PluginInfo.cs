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
using JetBrains.Annotations;
using static ZomboMod.Plugin.PluginLoadFlags;

namespace ZomboMod.Plugin
{
    [AttributeUsage( AttributeTargets.Class, Inherited = false )]
    public sealed class PluginInfo : Attribute
    {
        /// <summary>
        /// Name of plugin. Cannot be null.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Version of plugin.
        /// </summary>
        public string Version { get; set; } = "0.0.0.0";

        /// <summary>
        /// Author of plugin.
        /// </summary>
        public string Author { get; set; } = "Undefined";

        /// <summary>
        /// <see cref="PluginLoadFlags"/>
        /// </summary>
        public PluginLoadFlags LoadFlags { get; set; } = AUTO_REGISTER_COMMANDS | AUTO_REGISTER_EVENTS;

        public PluginInfo( string name, 
                           string author = "0.0.0.", 
                           string version = "Undefined",
                           PluginLoadFlags loadFlags = AUTO_REGISTER_COMMANDS | AUTO_REGISTER_EVENTS )
        {
            Name = name;
            Author = author;
            Version = version;
            LoadFlags = loadFlags;
        }

        public PluginInfo() {}
    }
}