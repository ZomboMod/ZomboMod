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
using Mono.Cecil.Rocks;
using SDG.Unturned;
using UnityEngine;
using ZomboMod.Core;

using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace ZomboMod.Patcher.Patches
{
    [Inject( In = "SDG.Unturned.Provider" )]
    public sealed class ProviderPatch : Patch
    {
        [Inject(In = "Awake", At = "BEFORE(Call, 'System.Void SDG.Unturned.SteamAdminlist::load()')")]
        public void InitCore()
        {
            ZomboCore.PreInit();
        }
        
        [Inject(In = "addPlayer", At = "START")]
        public static void OnPlayerPreAdded(SteamPlayerID playerId , Vector3 point   , byte angle                        ,
                                            bool isPro             , bool isAdmin    , int channel     , byte face       ,
                                            byte hair              , byte beard      , Color skin      , Color color     ,
                                            bool hand              , int shirtItem   , int pantsItem   , int hatItem     ,
                                            int backpackItem       , int vestItem    , int maskItem                      , 
                                            int glassesItem        , int[] skinItems , EPlayerSkillset skillset          )     
        {
            ZomboCore.OnPlayerPreAdded(playerId, ref point, ref angle, ref isPro, ref isAdmin, ref channel, ref face, ref hair,
                                       ref beard, ref skin, ref color, ref hand, ref shirtItem, ref pantsItem, ref hatItem, 
                                       ref backpackItem, ref vestItem, ref maskItem, ref glassesItem, ref skinItems, ref skillset );
        }
        
        [Inject(Type = "EXECUTE", In = "addPlayer")]
        public void OnPlayerAdded() 
        {
            var callbackMd = UnturnedDefinition.Import(typeof(ZomboCore).GetMethod("OnPlayerAdded"));
            var varDef = new VariableDefinition( "steamPlayer", UnturnedDefinition.GetType("SDG.Unturned.SteamPlayer"));
            var instrs = base.CurrentMethod.Body.Instructions;
            var index = -1;
            CurrentMethod.Body.Variables.Add( varDef );
            
            for ( var i = 0; i < instrs.Count; i++ )
            {
                if ( instrs[i].OpCode == Ldarg_S &&
                     instrs[i].Operand.ToString() == "skillset" &&
                     instrs[i + 1].OpCode == Newobj &&
                     instrs[i + 1].Operand.ToString().Contains( "SteamPlayer::.ctor" ) )
                {
                    index = i + 1;
                }
            }

            if ( index != -1 )
            {
                var localSteamPlayerVar = CurrentMethod.Body.Variables.LastOrDefault();

                CurrentMethod.Body.SimplifyMacros();
                CurrentMethod.Body.Instructions.Insert(++index, Create(Stloc_S, localSteamPlayerVar));
                CurrentMethod.Body.Instructions.Insert(++index, Create(Ldloc_S, localSteamPlayerVar));
                CurrentMethod.Body.Instructions.Insert(++index, Create(Call, callbackMd));
                CurrentMethod.Body.Instructions.Insert(++index, Create(Ldloc_S, localSteamPlayerVar));
                CurrentMethod.Body.OptimizeMacros();
            }
            else
            {
                Console.WriteLine( "index == -1" );
            }
        }
    }
}