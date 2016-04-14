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
using System.Collections.Generic;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using ZomboMod.Permission;
using SDGPlayer = SDG.Unturned.Player;
using SteamUser = ZomboMod.Steam.SteamUser;
namespace ZomboMod.Entity
{
    public class Player : IEntity, ILivingEntity, IPermissible
    {
        public SteamUser SteamUser { get; }

        public string CharacterName
        {
            get { return SteamPlayer.playerID.characterName; }
        }

        public byte CharacterId
        {
            get { return SteamPlayer.playerID.characterID; }
        }

        public SteamChannel Channel
        {
            get { return SteamPlayer.player.channel;  }
        }

        public bool IsPro
        {
            get { return SteamPlayer.isPro; }
        }

        public uint Health
        {
            get { return SDGPlayer.life.health; }
            set { throw new NotImplementedException(); }
        }

        public uint Hunger
        {
            get { return SDGPlayer.life.food; }
            set { throw new NotImplementedException(); }
        }

        public uint Thirst
        {
            get { return SDGPlayer.life.water; }
            set { throw new NotImplementedException(); }
        }

        public uint Stamina
        {
            get { return SDGPlayer.life.stamina; }
            set { throw new NotImplementedException(); }
        }

        public uint Experience
        {
            get { return SDGPlayer.skills.experience; }
            set 
            {
                typeof(PlayerSkills).GetField( "_experience" ).SetValue( SDGPlayer.skills, value );
                Channel.send( "tellExperience", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, value );
            }
        }

        public Item Mask
        {
            get { return _mask; }
            set
            {
                if ( value == null )
                    SDGPlayer.clothing.askWearMask( 0, 0, new byte[0], true );
                else
                    SDGPlayer.clothing.askWearMask( value.id, value.quality,
                                                    value.state, true );
            }
        }

        public Item Vest
        {
            get { return _vest; }
            set
            {
                if ( value == null )
                    SDGPlayer.clothing.askWearVest( 0, 0, new byte[0], true );
                else
                    SDGPlayer.clothing.askWearVest( value.id, value.quality,
                                                    value.state, true );
            }
        }

        public Item Hat
        {
            get { return _hat; }
            set
            {
                if ( value == null )
                    SDGPlayer.clothing.askWearHat( 0, 0, new byte[0], true );
                else
                    SDGPlayer.clothing.askWearHat( value.id, value.quality,
                                                   value.state, true );
            }
        }

        public Item Glasses
        {
            get { return _glasses; }
            set
            {
                if ( value == null )
                    SDGPlayer.clothing.askWearGlasses( 0, 0, new byte[0], true );
                else
                    SDGPlayer.clothing.askWearGlasses( value.id, value.quality,
                                                       value.state, true );
            }
        }

        public Item Shirt
        {
            get { return _shirt; }
            set
            {
                if ( value == null )
                    SDGPlayer.clothing.askWearShirt( 0, 0, new byte[0], true );
                else
                    SDGPlayer.clothing.askWearShirt( value.id, value.quality,
                                                     value.state, true );
            }
        }

        public Item Pants
        {
            get { return _pants; }
            set
            {
                if ( value == null )
                    SDGPlayer.clothing.askWearPants( 0, 0, new byte[0], true );
                else
                    SDGPlayer.clothing.askWearPants( value.id, value.quality,
                                                     value.state, true );
            }
        }

        public Item Backpack
        {
            get { return _backpack; }
            set
            {
                if ( value == null )
                    SDGPlayer.clothing.askWearBackpack( 0, 0, new byte[0], true );
                else
                    SDGPlayer.clothing.askWearBackpack( value.id, value.quality, 
                                                        value.state, true );
            }
        }

        public Item ItemInHand
        {
            get;
            set;
        }

        private bool IsAdmin
        {
            get { return SteamPlayer.isAdmin; }
            set { SteamPlayer.isAdmin = value; }
        }

        public PlayerInventory Inventory
        {
            get { return SDGPlayer.inventory; }
        }

        public InteractableVehicle CurrentVehicle
        {
            get { return SDGPlayer.movement.getVehicle(); }
        }

        public float Ping
        {
            get { return Channel.owner.ping * 1000; } 
        }

        public bool IsInVehicle
        {
            get { return CurrentVehicle != null; }
        }

        public bool IsDead
        {
            get { return SDGPlayer.life.isDead;  }
        }

        public bool IsBleeding
        {
            get { return SDGPlayer.life.isBleeding; }
            set 
            {
                typeof( PlayerLife ).GetField( "_bleeding" ).SetValue( SDGPlayer.life, value );
                Channel.send( "tellBleeding", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, value ); 
            }
        }

        public bool IsLegBroken
        {
            get { return SDGPlayer.life.isBroken; }
            set { throw new NotImplementedException(); }
        }

        public bool IsFreezing
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool IsUnderWater
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsOnGround
        {
            get { throw new NotImplementedException(); }
        }

        public float Rotation
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Vector3 Position
        {
            get { return SDGPlayer.transform.position; }
            set { SDGPlayer.transform.position = value; }
        }

        public IEnumerable<string> Permissions
        {
            get { return Zombo.PermissionProvider.GetPermssions( this ); }
        }

        public void Teleport( Vector3 position, float rotation )
        {
            throw new NotImplementedException();
        }

        public void Teleport( Vector3 position )
        {
            throw new NotImplementedException();
        }

        public void Kick( string reason = "Undefined" )
        {
            Provider.kick( SteamUser.SteamID, reason );
        }

        public Vector3? GetEyePosition( float distance, int masks )
        {
            throw new NotImplementedException();
        }

        public Vector3? GetEyePosition( float distance )
        {
            throw new NotImplementedException();
        }

        public Skill GetSkill( Skill.Type Type )
        {
            throw new NotImplementedException();
        }

        public void SendMessage( params string[] messages )
        {
            throw new NotImplementedException();
        }

        public void Chat( params string[] messages )
        {
            throw new NotImplementedException();
        }

        //TODO TEST
        public void Suicide()
        {
            SDGPlayer.life.askSuicide( SteamUser.SteamID );
        }

        public void Kill()
        {
            EPlayerKill outKill; // Unused
            Kill( EDeathCause.KILL, ELimb.SKULL, CSteamID.Nil, out outKill );
        }

        public void Kill( EDeathCause cause, ELimb limb, CSteamID killer )
        {
            EPlayerKill outKill; // Unused
            Kill( cause, limb, killer, out outKill );
        }

        public void Kill( EDeathCause cause, ELimb limb, CSteamID killer, out EPlayerKill outKill )
        {
            SDGPlayer.life.askDamage( byte.MaxValue, Position.normalized, cause, 
                                      limb, killer, out outKill );
        }

        public bool HasPermission( string permission )
        {
            return Zombo.PermissionProvider.HasPermission( this, permission );
        }

        void IEntity.Remove()
        {
            throw new NotSupportedException( "Cannot use IEntity::remove on Player, use Player::Kick instead." );
        }

        internal Player( SteamPlayer handle )
        {
            SteamPlayer = handle;
            SDGPlayer = handle.player;

            SteamUser = new SteamUser( SteamPlayer );

            SDGPlayer.clothing.onHatUpdated      += ( id, quality, state ) => _hat = new Item( id, 1, quality, state );
            SDGPlayer.clothing.onGlassesUpdated  += ( id, quality, state ) => _glasses = new Item( id, 1, quality, state );
            SDGPlayer.clothing.onBackpackUpdated += ( id, quality, state ) => _backpack = new Item( id, 1, quality, state );
            SDGPlayer.clothing.onPantsUpdated    += ( id, quality, state ) => _pants = new Item( id, 1, quality, state );
            SDGPlayer.clothing.onMaskUpdated     += ( id, quality, state ) => _mask = new Item( id, 1, quality, state );
            SDGPlayer.clothing.onVestUpdated     += ( id, quality, state ) => _vest = new Item( id, 1, quality, state );
            SDGPlayer.clothing.onShirtUpdated    += ( id, quality, state ) => _shirt = new Item( id, 1, quality, state );
        }

        internal SDGPlayer SDGPlayer;
        internal SteamPlayer SteamPlayer;

        private Item _hat, _vest, _glasses, _mask, _shirt, _pants, _backpack, _itemInHand;
    }
}