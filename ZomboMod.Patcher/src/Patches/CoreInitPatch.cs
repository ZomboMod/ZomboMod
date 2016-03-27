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
using Mono.Cecil;
using Mono.Cecil.Cil;
using ZomboMod.Patcher.Util;


namespace ZomboMod.Patcher.Patches
{
    public class CoreInitPatch : Patch
    {
        public override void Apply( ModuleDefinition mdef )
        {
            var method = mdef.GetMethod( "SDG.Unturned.Provider", "Awake" );

            var instr = method.Body.Instructions.FirstOrDefault( 
                i => i.OpCode == OpCodes.Call && i.Operand.ToString() == "System.Void SDG.Unturned.Commander::init()" 
            );

            if ( instr != null )
            {
                var init = Instruction.Create( OpCodes.Call, mdef.Import( typeof (Core.ZomboCore).GetMethod( "PreInit" ) ) );
                method.Body.GetILProcessor().InsertAfter( instr, init );
            }
            else
            {
                Severe( "Commander::init() not found" );
            }
        }
    }
}