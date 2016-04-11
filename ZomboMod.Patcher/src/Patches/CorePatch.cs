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
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using ZomboMod.Core;
using ZomboMod.Patcher.Util;

using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace ZomboMod.Patcher.Patches
{
    public class CorePatch : Patch
    {
        public override void Apply( ModuleDefinition mdef )
        {
            const string providerType = "SDG.Unturned.Provider";

            var addPlayerMethod = mdef.GetMethod( providerType, "addPlayer" );
            var addPlayerInstr = addPlayerMethod.Body.Instructions;

            var instructions = new [] {
                Create( Ldarg_0 )                                 , // playerID
                Create( Ldarga_S, addPlayerMethod.Parameters[1] ) , // point
                Create( Ldarga_S, addPlayerMethod.Parameters[2] ) , // angle
                Create( Ldarga_S, addPlayerMethod.Parameters[3] ) , // isPro
                Create( Ldarga_S, addPlayerMethod.Parameters[4] ) , // isAdmin
                Create( Ldarga_S, addPlayerMethod.Parameters[5] ) , // channel
                Create( Ldarga_S, addPlayerMethod.Parameters[6] ) , // face
                Create( Ldarga_S, addPlayerMethod.Parameters[7] ) , // hair
                Create( Ldarga_S, addPlayerMethod.Parameters[8] ) , // beard
                Create( Ldarga_S, addPlayerMethod.Parameters[9] ) , // skin
                Create( Ldarga_S, addPlayerMethod.Parameters[10] ), // color
                Create( Ldarga_S, addPlayerMethod.Parameters[11] ), // hand
                Create( Ldarga_S, addPlayerMethod.Parameters[12] ), // shirtItem
                Create( Ldarga_S, addPlayerMethod.Parameters[13] ), // pantsItem
                Create( Ldarga_S, addPlayerMethod.Parameters[14] ), // hatItem
                Create( Ldarga_S, addPlayerMethod.Parameters[15] ), // backpackItem
                Create( Ldarga_S, addPlayerMethod.Parameters[16] ), // vestItem
                Create( Ldarga_S, addPlayerMethod.Parameters[17] ), // maskItem
                Create( Ldarga_S, addPlayerMethod.Parameters[18] ), // glassesItem
                Create( Ldarga_S, addPlayerMethod.Parameters[19] ), // skinItems
                Create( Ldarga_S, addPlayerMethod.Parameters[20] ), // skillset
                Create( Ldarga_S, addPlayerMethod.Parameters[21] ), // isAnonymous,
                Create( Call, ImportCoreMethod( mdef, "OnPlayerPreAdded" ) )
            };


            /*
                Inject ZomboCore::OnPlayerPreAdded()
            */
            addPlayerMethod.Body.SimplifyMacros();
            instructions.Reverse().ForEach( i => {
                addPlayerMethod.Body.Instructions.Insert( 0, i );
            } );
            addPlayerMethod.Body.OptimizeMacros();

            /*
                Inject ZomboCore::OnPlayerAdded()
            */
            addPlayerMethod.Body.Variables.Add( new VariableDefinition( "steamPlayer", mdef.GetType("SDG.Unturned.SteamPlayer") ) );
            var index = -1;

            /*
                Search for
                        IL_028e: ldarg.s isAnonymous
                        IL_0290: newobj instance void SDG.Unturned.SteamPlayer::.ctor(class SDG.Unturned.SteamPlayerID ...
            */
            for ( var i = 0; i < addPlayerInstr.Count  /* Skip last instruction */; i++ )
            {
                if ( addPlayerInstr[i].OpCode == Ldarg_S &&
                     addPlayerInstr[i].Operand.ToString() == "isAnonymous" &&
                     addPlayerInstr[i + 1].OpCode == Newobj &&
                     addPlayerInstr[i + 1].Operand.ToString().Contains( "SteamPlayer::.ctor" ) )
                {
                    index = i + 1;
                }
            }

            if ( index != -1 )
            {
                var localSteamPlayerVar = addPlayerMethod.Body.Variables.LastOrDefault();

                addPlayerMethod.Body.SimplifyMacros();
                addPlayerMethod.Body.Instructions.Insert( ++index, Create( Stloc_S, localSteamPlayerVar ) );
                addPlayerMethod.Body.Instructions.Insert( ++index, Create( Ldloc_S, localSteamPlayerVar ) );
                addPlayerMethod.Body.Instructions.Insert( ++index, Create( Call, ImportCoreMethod( mdef, "OnPlayerAdded" ) ) );
                addPlayerMethod.Body.Instructions.Insert( ++index, Create( Ldloc_S, localSteamPlayerVar ) );
                addPlayerMethod.Body.OptimizeMacros();
            }
            else
            {
                Severe( "index == -1" );
            }

            addPlayerMethod.Body.Instructions.ForEach( Console.WriteLine );

            /*
                Inject ZomboCore::PreInit() at Provider::Awake()
            */
            var method = mdef.GetMethod( "SDG.Unturned.Provider", "Awake" );

            var instr = method.Body.Instructions.FirstOrDefault( 
                i => i.OpCode == Call && i.Operand.ToString() == "System.Void SDG.Unturned.SteamAdminlist::load()" 
            );

            if ( instr != null )
            {
                var init = Create( Call, ImportCoreMethod( mdef, "PreInit" ) );
                method.Body.GetILProcessor().InsertAfter( instr, init );
            }
            else
            {
                Severe( "Commander::init() not found" );
            }
        }

        private static MethodReference ImportCoreMethod( ModuleDefinition mdef, string mdName )
        {
            return mdef.Import( typeof (ZomboCore).GetMethod( mdName ) );
        }
    }
}