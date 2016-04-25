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
        ///   BEFORE(OpCode, 'Operand')
        ///   AFTER(OpCode, 'Operand')
        ///   START
        ///   END
        /// </summary>
        public string At { get; set; }
        
        /// <summary>
        /// Available:
        ///   INJECT_BODY - Inject method body.
        ///   EXECUTE     - Just execute the method.
        /// </summary>
        public string Type { get; set; }
    }
}