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
using UnityEngine;

namespace ZomboMod.Entity
{
    public class Vehicle : IEntity
    {
        public uint Health { get; set; }

        public Vector3 Position { get; }

        public float Rotation { get; set; }

        public bool IsUnderWater { get; }

        public bool IsOnGround { get; }

        public IEnumerable<Player> Passagers { get; }

        public int Seats { get; }

        public void Teleport( Vector3 position, float rotation )
        {
            throw new NotImplementedException();
        }

        public void Teleport( Vector3 position )
        {
            throw new NotImplementedException();
        }

        public void Remove()
        {
            throw new NotImplementedException();
        }

        public void Explode()
        {
            throw new NotImplementedException();
        }

        internal Vehicle( GameObject handle )
        {
            _handle = handle;
        }

        private GameObject _handle;
    }
}