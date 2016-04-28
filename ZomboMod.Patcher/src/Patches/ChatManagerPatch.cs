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

using SDG.Unturned;
using ZomboMod.Core;

namespace ZomboMod.Patcher.Patches
{
    [Inject(In = "SDG.Unturned.ChatManager")]
    public sealed class ChatManagerPatch : Patch
    {
        [Inject(Type = "REPLACE_BODY", In = "process", At = "START")]
        public static bool ProcessChat(SteamPlayer player, string text)
        {
            return ZomboCore.ProcessChat(player, text);
        }
    }
}