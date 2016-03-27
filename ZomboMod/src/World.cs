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

using System.Collections.Generic;
using System.Linq;
using ZomboMod.Entity;


/*
    World {
        SpawnVehicle( )     -> Vehicle
        SpawnItem( )        -> DroppedItem
        SpawnZombie( )      -> Zombie
        SpawnStructure( )
        SpawnAnimal( )      -> Animal
    }

*/

namespace ZomboMod
{
    public class World
    {
        public IEnumerable<IEntity> Entities    => _entities.AsEnumerable();
        public IEnumerable<Vehicle> Vehicles    => _entities.Where( e => e is Vehicle ).Cast<Vehicle>();
        public IEnumerable<Zombie>  Zombies     => _entities.Where( e => e is Zombie ).Cast<Zombie>();

        public World()
        {
            _entities = new List<IEntity>();
        }

        private List<IEntity> _entities;
    }
}