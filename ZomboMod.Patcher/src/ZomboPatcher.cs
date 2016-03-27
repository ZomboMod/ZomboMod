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
using System.IO;
using System.Linq;
using Mono.Cecil;
using ZomboMod.Patcher.Util;

namespace ZomboMod.Patcher
{
    public class ZomboPatcher
    {
        /*
            Directory that patched file will be placed.
        */
        private const string PATCHED_DIR = @"C:\Users\Leonardo\Documents\Unturned\Zombo\All\Unturned\Unturned_Data\Managed\";

        public static void Main( string[] args )
        {
            try
            {
                Console.Title = "ZomboPatcher";

                /*
                    Change from 'bin\Debug' to 'UnturnedDlls\'
                */
                Directory.SetCurrentDirectory( @"..\..\UnturnedDlls\" );

                var mainModule = AssemblyDefinition.ReadAssembly( "Assembly-CSharp.dll" ).MainModule;
                var zomboCore = AssemblyDefinition.ReadAssembly( @"..\bin\Debug\ZomboMod.dll" );

                foreach ( var dfs in zomboCore.MainModule.AssemblyReferences )
                {
                    mainModule.AssemblyReferences.Add( dfs );
                }

                var allTypes = typeof (ZomboPatcher).Assembly.GetTypes();
                
                allTypes.Where( _ => typeof(Patch).IsAssignableFrom( _ ) )
                        .Where( _ => !_.IsAbstract )
                        .ForEach( _ => {
                            Console.WriteLine( $"Applying patch: '{_.Name}'" );
                            var inst = (Patch) Activator.CreateInstance( _ );
                            inst.Apply( mainModule );
                            Console.WriteLine( $"Applied patch: '{_.Name}'" );
                        });

                Console.WriteLine( "Finished!" );

                mainModule.Write( $"{PATCHED_DIR}Assembly-CSharp.dll" );
            }
            catch (Exception xe)
            {
                Console.WriteLine( xe );
            }

            Console.ReadKey();
        }
    }
}