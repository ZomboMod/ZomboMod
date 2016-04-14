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
using Mono.Cecil.Rocks;
using ZomboMod.Patcher.Util;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

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
            
            CreateSetters( mdef );
        }
        
        public void CreateSetters( ModuleDefinition mdef )
        {
            new [] {
                new [] { "SDG.Unturned.PlayerSkills" , "experience" , "_experience"   },
                new [] { "SDG.Unturned.PlayerLife"   , "isBleeding" , "_isBleeding"   },
                new [] { "SDG.Unturned.PlayerLife"   , "isBroken"   , "_isBroken"     },
                new [] { "SDG.Unturned.PlayerLife"   , "health"     , "_health"       },
                new [] { "SDG.Unturned.PlayerLife"   , "food"       , "_food"         }
            }.ForEach( arr => CreateSetter( mdef, arr[0], arr[1], arr[2]) ); 
        }
        
        /*
            Create a simple setter...
            
            set {
                this.backingField = value;
            }
        */
        public void CreateSetter( ModuleDefinition mdef, string typeName, string propName, 
                                  string backingFieldName )
        {
            var type = mdef.GetType( typeName );
            var prop = type.Properties.FirstOrDefault( f => f.Name.Equals( propName ) );
            
            if ( prop == null )
            {
                Severe( $"(CreateSetter) prop == null" );
            }
            
            var setMethod = new MethodDefinition( $"set_{propName}", MethodAttributes.Public, mdef.Import( typeof(void) ) );
            var index = 0;
            var backingField = mdef.GetField( type, backingFieldName );
            
            if ( backingField == null )
            {
                Severe( $"(CreateSetter) backingField == null" );
            }
            
            setMethod.Parameters.Add( new ParameterDefinition( mdef.Import( typeof(uint) ) ) );
            
            setMethod.Body.Instructions.Insert( index++, Create( Nop ) );
            setMethod.Body.Instructions.Insert( index++, Create( Ldarg_0 ) );
            setMethod.Body.Instructions.Insert( index++, Create( Ldarg_1 ) );
            setMethod.Body.Instructions.Insert( index++, Create( Stfld, backingField ) );
            setMethod.Body.Instructions.Insert( index++, Create( Ret ) );
            
            type.Methods.Add( setMethod );
            prop.SetMethod = setMethod;
        }
    }
}
