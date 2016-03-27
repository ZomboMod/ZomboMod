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
using System.Reflection;
using SDG.Unturned;

namespace ZomboMod.Core
{
    public static class ZomboCore
    {
        public const string HEADER = @"
    ______               _             __  __           _ 
   |___  /              | |           |  \/  |         | |
      / / ___  _ __ ___ | |__   ___   | \  / | ___   __| |
     / / / _ \| '_ ` _ \| '_ \ / _ \  | |\/| |/ _ \ / _` |
    / /_| (_) | | | | | | |_) | (_) | | |  | | (_) | (_| |
   /_____\___/|_| |_| |_|_.__/ \___/  |_|  |_|\___/ \__,_|
                                                          
";
        public static void PreInit()
        {
            Commander.init();

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine( HEADER );
            Console.ForegroundColor = ConsoleColor.White;

            var zomboType = typeof (Zombo);
            zomboType.GetMethod( "Init", (BindingFlags) 0x28 ).Invoke( null, new object[0] );
        }
    }
}
