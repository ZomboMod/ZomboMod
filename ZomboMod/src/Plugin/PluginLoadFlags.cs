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

namespace ZomboMod.Plugin
{
    [Flags]
    public enum PluginLoadFlags
    {
        /// <summary>
        /// Nothing
        /// </summary>
        NONE                   = 0,

        /// <summary>
        /// Dynamically register all commands in Plugin.
        /// </summary>
        AUTO_REGISTER_COMMANDS = 1 << 0,

        /// <summary>
        /// Dynamically register all events (handlers) in Plugin.
        /// </summary>
        AUTO_REGISTER_EVENTS   = 1 << 1
    }
}
