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

using UnityEngine;

namespace ZomboMod.Entity
{
    public interface IEntity
    {
        uint Health { get; set; }

        Vector3 Position { get; }

        float Rotation { get; set; }

        bool IsUnderWater { get; }

        bool IsOnGround { get; }

        void Teleport( Vector3 position, float rotation );

        void Teleport( Vector3 position );

        void Remove();
    }
}