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
using Steamworks;
using UnityEngine;
using ZomboMod.Core;

namespace ZomboMod.Patcher.Patches
{
    [Inject(In = "SDG.Unturned.PlayerLife")]
    public sealed class PlayerLifePatch : Patch
    {
        
        [Inject(In = "askDamage", At = "START")]
        public void OnPlayerDamaged(byte amount, Vector3 newRagdoll, EDeathCause newCause,
                                    ELimb newLimb, CSteamID newKiller)
        {
            // Load 'base.player' into stack
            Emit(@"
                Ldarg_0;
                Call, [unturned] SDG.Unturned.PlayerCaller::get_player();
            ");
            SkipNext(); // Skip Ldnull vv
            ZomboCore.OnPlayerDamaged(null, ref amount, ref newRagdoll, ref newCause, ref newLimb, ref newKiller);
        }
    }
}