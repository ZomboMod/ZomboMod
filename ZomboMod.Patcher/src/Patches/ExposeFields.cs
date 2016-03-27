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
using ZomboMod.Patcher.Util;

namespace ZomboMod.Patcher.Patches
{
    public class ExposeFields : Patch
    {
        public override void Apply( ModuleDefinition mdef )
        {
            Action<string, string> ExposeField = ( type, field ) => {
                var vehicleManagerField = mdef.GetField( $"SDG.Unturned.{type}", field );
                vehicleManagerField.IsPrivate = false;
                vehicleManagerField.IsPublic = true;
            };

            ExposeField( "VehicleManager", "manager" );
            ExposeField( "ItemManager", "manager" );
            ExposeField( "AnimalManager", "manager" );
            ExposeField( "BarricadeManager", "manager" );
            ExposeField( "BeaconManager", "manager" );
            ExposeField( "ChatManager", "manager" );
            ExposeField( "ClaimManager", "manager" );
            ExposeField( "EffectManager", "manager" );
            ExposeField( "LevelManager", "manager" );
            ExposeField( "LightingManager", "manager" );
            ExposeField( "ObjectManager", "manager" );
        }
    }
}