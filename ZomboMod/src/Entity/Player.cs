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
using UnityEngine;
using ZomboMod.Permission;
using ZomboMod.Steam;

using SDGPlayer = SDG.Unturned.Player;

namespace ZomboMod.Entity
{
    public class Player : IEntity, ILivingEntity, IPermissible
    {
        public SteamUser SteamUser { get; }

        public SteamChannel Channel { get; }

        public string Name { get; }

        public bool IsPro { get; }

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
            set { throw new NotImplementedException(); }
        }

        public Item Hat
        {
            get;
            set;
        }

        public Item Glasses
        {
            get;
            set;
        }

        public Item Shirt
        {
            get;
            set;
        }

        public Item Pants
        {
            get;
            set;
        }

        public Item Backpack
        {
            get;
            set;
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
            set { throw new NotImplementedException(); }
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
            throw new NotImplementedException();
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

        public void Suicide()
        {
            throw new NotImplementedException();
        }

        public void Kill()
        {
            throw new NotImplementedException();
        }

        public bool HasPermission( string permission )
        {
            return Zombo.PermissionProvider.HasPermission( this, permission );
        }

        void IEntity.Remove() {}

        internal Player( SDGPlayer sdgPlayer )
        {
            SDGPlayer = sdgPlayer;

            SteamUser = new SteamUser( sdgPlayer );

            Channel = sdgPlayer.channel;
            SteamPlayer = Channel.owner;
            IsPro = SteamPlayer.isPro;

            Name = SteamPlayer.playerID.characterName;
        }

        internal SDGPlayer SDGPlayer;
        internal SteamPlayer SteamPlayer;
    }
}