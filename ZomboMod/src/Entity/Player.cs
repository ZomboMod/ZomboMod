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
using ZomboMod.Common;
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
        
        public EPlayerTemperature Temperature
        {
            get { return SDGPlayer.life.temperature; }
            set { throw new NotImplementedException(); }
        }

        public uint Virus
        {
            get { return SDGPlayer.life.virus; }
            set
            {
                if ( value > 0xFF )
                    throw new ArgumentOutOfRangeException("virus must be between 0 and 255");
                var bValue = (byte) value;
                Reflection.GetField<PlayerLife>( "_virus" ).SetValue( SDGPlayer.life, bValue );
                Channel.send( "tellVirus", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, bValue ); 
            }
        }

        public uint Health
        {
            get { return SDGPlayer.life.health; }
            set
            {
                if ( value > 0xFF )
                    throw new ArgumentOutOfRangeException("health must be between 0 and 255");
                var bValue = (byte) value;
                Reflection.GetField<PlayerLife>( "_health" ).SetValue( SDGPlayer.life, bValue );
                Channel.send( "tellHealth", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, bValue ); 
            }
        }

        public uint Food
        {
            get { return SDGPlayer.life.food; }
            set
            {
                if ( value > 0xFF )
                    throw new ArgumentOutOfRangeException("food must be between 0 and 255");
                var bValue = (byte) value;
                Reflection.GetField<PlayerLife>( "_food" ).SetValue( SDGPlayer.life, bValue );
                Channel.send( "tellFood", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, bValue ); 
            }
        }

        public uint Water
        {
            get { return SDGPlayer.life.water; }
            set
            {
                if ( value > 0xFF )
                    throw new ArgumentOutOfRangeException("water must be between 0 and 255");
                var bValue = (byte) value;
                Reflection.GetField<PlayerLife>( "_water" ).SetValue( SDGPlayer.life, bValue );
                Channel.send( "tellWater", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, bValue ); 
            }
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
                Reflection.GetField<PlayerSkills>( "_experience" ).SetValue( SDGPlayer.skills, value ); ;
                Channel.send( "tellExperience", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, value );
            }
        }

        public Item Mask
        {
            get { return _mask; }
            set
            {
                var quality = value?.quality ?? 0;
                var id = value?.id ?? 0;
                var state = value?.state ?? new byte[0];

                Channel.send( "tellWearMask", ESteamCall.ALL,
                    ESteamPacket.UPDATE_RELIABLE_BUFFER, id, quality, state, true );
            }
        }

        public Item Vest
        {
            get { return _vest; }
            set
            {
                var quality = value?.quality ?? 0;
                var id = value?.id ?? 0;
                var state = value?.state ?? new byte[0];

                Channel.send( "tellWearVest", ESteamCall.ALL,
                    ESteamPacket.UPDATE_RELIABLE_BUFFER, id, quality, state, true );
            }
        }

        public Item Hat
        {
            get { return _hat; }
            set
            {
                var quality = value?.quality ?? 0;
                var id = value?.id ?? 0;
                var state = value?.state ?? new byte[0];

                Channel.send( "tellWearHat", ESteamCall.ALL,
                    ESteamPacket.UPDATE_RELIABLE_BUFFER, id, quality, state, true );
            }
        }

        public Item Glasses
        {
            get { return _glasses; }
            set
            {
                var quality = value?.quality ?? 0;
                var id = value?.id ?? 0;
                var state = value?.state ?? new byte[0];

                Channel.send( "tellWearGlasses", ESteamCall.ALL,
                    ESteamPacket.UPDATE_RELIABLE_BUFFER, id, quality, state, true );
            }
        }

        public Item Shirt
        {
            get { return _shirt; }
            set
            {
                var quality = value?.quality ?? 0;
                var id = value?.id ?? 0;
                var state = value?.state ?? new byte[0];

                Channel.send( "tellWearShirt", ESteamCall.ALL,
                    ESteamPacket.UPDATE_RELIABLE_BUFFER, id, quality, state, true );
            }
        }

        public Item Pants
        {
            get { return _pants; }
            set
            {
                var quality = value?.quality ?? 0;
                var id = value?.id ?? 0;
                var state = value?.state ?? new byte[0];

                Channel.send( "tellWearPants", ESteamCall.ALL,
                    ESteamPacket.UPDATE_RELIABLE_BUFFER, id, quality, state, true );
            }
        }

        public Item Backpack
        {
            get { return _backpack; }
            set
            {
                var quality = value?.quality ?? 0;
                var id = value?.id ?? 0;
                var state = value?.state ?? new byte[0];

                Channel.send( "tellWearBackpack", ESteamCall.ALL,
                    ESteamPacket.UPDATE_RELIABLE_BUFFER, id, quality, state, true );
            }
        }

        public Item ItemInHand
        {
            get;
            set;
        }

        public bool IsAdmin
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
                Reflection.GetField<PlayerLife>( "_isBleeding" ).SetValue( SDGPlayer.life, value );
                Channel.send( "tellBleeding", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, value ); 
            }
        }

        public bool IsLegBroken
        {
            get { return SDGPlayer.life.isBroken; }
            set 
            {
                Reflection.GetField<PlayerLife>( "_isBroken" ).SetValue( SDGPlayer.life, value );
                Channel.send( "tellBroken", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, value ); 
            }
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
            get { return SDGPlayer.transform.eulerAngles.y; }
            set { throw new NotImplementedException(); }
        }

        public Vector3 Position
        {
            get { return SDGPlayer.transform.position; }
        }

        public IEnumerable<string> Permissions
        {
            get { return Zombo.PermissionProvider.GetPermssions( this ); }
        }

        public void Teleport( Vector3 position, float rotation )
        {
            SDGPlayer.transform.position = position;
            Channel.send( "askTeleport", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, 
                          Position, MeasurementTool.angleToByte( rotation ) );
        }

        public void Teleport( Vector3 position )
        {
            Teleport( position, Rotation );
        }

        public void Kick( string reason = "Undefined" )
        {
            Provider.kick( SteamUser.SteamID, reason );
        }

        public bool GiveItem( Item item )
        {
            return GiveItem( item, false );
        }

        public bool GiveItem( Item item, bool dropIfInventoryFull )
        {
            return false;
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
            messages.ForEach( msg => {
                ChatManager.say( SteamUser.SteamID, msg, Color.green );
            });
        }

        /// <summary>
        /// Make player say something on chat.
        /// </summary>
        /// <param name="messages">Messages to say</param>
        public void Chat( params string[] messages )
        {
            Chat( EChatMode.GLOBAL, messages );
        }

        /// <summary>
        /// Make player say something on chat.
        /// </summary>
        /// <param name="messages">Messages to say</param>
        /// <param name="chatMode">ChatMode</param>
        /// <see cref="EChatMode"/>
        public void Chat( EChatMode chatMode, params string[] messages )
        {
            throw new NotImplementedException();
        }

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