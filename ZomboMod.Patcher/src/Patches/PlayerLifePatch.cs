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
using Mono.Cecil.Rocks;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using ZomboMod.Core;

using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace ZomboMod.Patcher.Patches
{
    [Inject(In = "SDG.Unturned.PlayerLife")]
    public sealed class PlayerLifePatch : Patch
    {
        [Inject(In = "askDamage", At = "START")]
        public void OnPlayerDamaged(byte amount   , Vector3 newRagdoll , EDeathCause newCause , 
                                    ELimb newLimb , CSteamID newKiller)
        {
            ZomboCore.OnPlayerDamaged( null, ref amount, ref newRagdoll, ref newCause, ref newLimb, ref newKiller );
        }

        // TODO: workaround, just for now
        [Inject(Type = "EXECUTE", In = "askDamage")]
        public void OnPlayerDamaged2()
        {
            CurrentMethod.Body.SimplifyMacros();
            CurrentMethod.Body.Instructions.Insert(0, Create(Ldarg_0));
            CurrentMethod.Body.OptimizeMacros();
            CurrentMethod.Body.Instructions[1] = Create(Call, GetMethod("{u}.PlayerCaller", "get_player")); // Replace ldnull
        }
    }
}