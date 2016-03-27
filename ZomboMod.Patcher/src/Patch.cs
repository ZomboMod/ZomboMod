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
using Mono.Cecil;

namespace ZomboMod.Patcher
{
    public abstract class Patch
    {
        public void Error( string msg )
        {
            var tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine( $"[{GetType()}] {msg}" );
            Console.ForegroundColor = tmp;
        }

        public void Warning( string msg )
        {
            var tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine( $"[{GetType()}] {msg}" );
            Console.ForegroundColor = tmp;
        }

        public void Severe( string msg )
        {
            throw new InvalidOperationException( msg );
        }

        public abstract void Apply( ModuleDefinition mdef );
    }
}