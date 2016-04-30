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

namespace ZomboMod.Patcher
{
    [AttributeUsage( AttributeTargets.All, Inherited = false, AllowMultiple = false )]
    public sealed class InjectAttribute : Attribute
    {
        /// <summary>
        /// If this attribute is being used in an type, 'In' will be 
        /// the type, otherwise it will be the target method that will be injected.
        /// </summary>
        public string In { get; set; }

        /// <summary>
        /// Available:
        ///   Will search for given Operand & Opcode, if found it will injected before.
        ///   BEFORE(OpCode, 'Operand')
        ///
        ///   Will search for given Operand & Opcode, if found it will injected after.
        ///   AFTER(OpCode, 'Operand')
        ///
        ///   Will search for an pattern of OpCodes & Operands, and will inject
        ///   before the given OpCode/Operand index.
        ///   For Example:
        ///       PATTERN(1, Ldarg_0, '', Ldarg_1, '', Call, 'some::method')
        ///   He will inject BEFORE or AFTER "Ldarg_1, ''"
        ///   PATTERN(index, BEFORE|AFTER, [OpCode, 'Operand' ...])
        ///   
        ///   Will inject at start of method body.
        ///   START
        ///
        ///   Will inject at end of method body.
        ///   END
        /// </summary>
        public string At { get; set; }
        
        /// <summary>
        /// Available:
        ///   REPLACE_BODY - Replace all instructions.
        ///   INJECT_BODY  - Inject method body.
        ///   EXECUTE      - Just execute the method.
        /// </summary>
        public string Type { get; set; }
    }
}