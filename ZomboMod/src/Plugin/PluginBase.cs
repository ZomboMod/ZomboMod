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

namespace ZomboMod.Plugin
{
    public abstract class PluginBase
    {
        public PluginInfo Info { get; private set; }

        private PluginBase( PluginInfo info )
        {
            Info = info;
        }

        public void OnLoad() {}

        public void OnUnload() {}

        public void OnReload() {}

        internal void Load()
        {
            // load commands
            // load events
            // Call OnLoad
        }

        internal void Unload()
        {
            // reverse of load
        }
    }
}