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

using Mono.Cecil;
using ZomboMod.Patcher.Util;

namespace ZomboMod.Patcher.Patches
{
    public class ExposeFields : Patch
    {
        public override void Apply( ModuleDefinition mdef )
        {
            var fields = new[] {
                new[] { "VehicleManager", "manager" },
                new[] { "ItemManager", "manager" },
                new[] { "AnimalManager", "manager" },
                new[] { "BarricadeManager", "manager" },
                new[] { "BeaconManager", "manager" },
                new[] { "ChatManager", "manager" },
                new[] { "ClaimManager", "manager" },
                new[] { "EffectManager", "manager" },
                new[] { "LevelManager", "manager" },
                new[] { "LightingManager", "manager" },
                new[] { "ObjectManager", "manager" },
            };

            fields.ForEach( f => {
                var vehicleManagerField = mdef.GetField( $"SDG.Unturned.{f[0]}", f[1] );
                vehicleManagerField.IsPrivate = false;
                vehicleManagerField.IsPublic = true;
            } );
        }
    }
}