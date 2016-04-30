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
using System.Linq;
using Mono.Cecil.Cil;
using SDG.Unturned;
using Steamworks;
using ZomboMod.Core;
using ZomboMod.Patcher.Util;

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

        [Inject(In = "askChat", At = "PATTERN(2, BEFORE, call, '%ctSDG.Unturned.Provider::get_isServer()', brfalse, '%any', ldarg_1, '')")]
        public void InjectPlayerChatted()
        {
           Emit(@"
            Ldarg_1;
            Ldloca_S, 2;
            Ldarga_S, 1;
            Ldarga_S, 2;
            Call, [zombo] ZomboMod.Core.ZomboCore::OnPlayerChatted();
           ");
        }

        [Inject(In = "askChat", At = "START")]
        public void LoadDefaultColor()
        {
            // Load Color.White in "color" variable.
            Emit(@"
                Call, [unityengine] UnityEngine.Color::get_white();
                Stloc_2;
            ");
        }

        [Inject(Type = "EXECUTE", In = "askChat")]
        public void RemoveAskChatColorStuffs()
        {
            var foundColorStuffs = false;
            var instrs = CurrentMethod.Body.Instructions;

            for (int i = 0; i < instrs.Count - 3; i++)
            {
                /*
                    Search for:
                        IL_013d: call UnityEngine.Color UnityEngine.Color::get_white()
                        IL_0142: stloc.2
                        IL_0143: ldloc.0
                        IL_0144: ldfld System.Boolean SDG.Unturned.SteamPlayer::isAdmin
                        
                        Color aDMIN = Color.white;
                        if (steamPlayer.isAdmin)
                        {
                            aDMIN = Palette.ADMIN;
                        }
                        else if (steamPlayer.isPro)
                        {
                            aDMIN = Palette.PRO;
                        }
                */
                if (instrs[i].OpCode == OpCodes.Call && 
                    instrs[i].Operand.ToString().Contains("Color::get_white()") &&
                    instrs[i + 1].OpCode == OpCodes.Stloc_2 &&
                    instrs[i + 2].OpCode == OpCodes.Ldloc_0 &&
                    instrs[i + 3].OpCode == OpCodes.Ldfld   &&
                    instrs[i + 3].Operand.ToString().Contains("SteamPlayer::isAdmin") )
                {
                    foundColorStuffs = true;
                    for (int j = i; j <= i + 12; j++)
                    {
                        instrs.RemoveAt(i); // And remove
                    }
                }
            }
            if (!foundColorStuffs)
            {
                Console.WriteLine("foundColorStuffs == false");
            }
        }
    }
}