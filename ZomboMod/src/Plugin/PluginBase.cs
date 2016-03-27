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

using System.Linq;

namespace ZomboMod.Plugin
{
    public abstract class PluginBase
    {
        public PluginInfo Info { get; private set; }

        internal PluginBase( PluginInfo info )
        {
            Info = info;
        }

        protected PluginBase()
        {
            Info = (PluginInfo) GetType().GetCustomAttributes( typeof( PluginInfo ), false )
                                         .First( f => f is PluginInfo );
        }

        public virtual void OnLoad() {}

        public virtual void OnUnload() {}

        public virtual void OnReload() {}

        internal void Load()
        {
            // TODO: load commands & events

            OnLoad();
        }

        internal void Unload()
        {
            // TODO: reverse of load
            OnUnload();
        }
    }
}