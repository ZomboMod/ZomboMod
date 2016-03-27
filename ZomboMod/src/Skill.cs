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

namespace ZomboMod
{
    public class Skill
    {
        public byte Level { get; set; }

        public byte MaxLevel { get; set; }

        public uint Cost { get; set; }

        internal Skill( byte level, byte maxLevel, uint cost )
        {
            Level = level;
            MaxLevel = maxLevel;
            Cost = cost;
        }

        public class Type
        {
            public static readonly Type OVERKILL = new Type( 0, 0 );
            public static readonly Type SHARPSHOOTER = new Type( 0, 1 );
            public static readonly Type DEXTERITY = new Type( 0, 2 );
            public static readonly Type CARDIO = new Type( 0, 3 );
            public static readonly Type EXERCISE = new Type( 0, 4 );
            public static readonly Type DIVING = new Type( 0, 5 );
            public static readonly Type PARKOUR = new Type( 0, 6 );
            public static readonly Type SNEAKYBEAKY = new Type( 1, 0 );
            public static readonly Type VITALITY = new Type( 1, 1 );
            public static readonly Type IMMUNITY = new Type( 1, 2 );
            public static readonly Type TOUGHNESS = new Type( 1, 3 );
            public static readonly Type STRENGTH = new Type( 1, 4 );
            public static readonly Type WARMBLOODED = new Type( 1, 5 );
            public static readonly Type SURVIVAL = new Type( 1, 6 );
            public static readonly Type HEALING = new Type( 2, 0 );
            public static readonly Type CRAFTING = new Type( 2, 1 );
            public static readonly Type OUTDOORS = new Type( 2, 2 );
            public static readonly Type COOKING = new Type( 2, 3 );
            public static readonly Type FISHING = new Type( 2, 4 );
            public static readonly Type AGRICULTURE = new Type( 2, 5 );
            public static readonly Type MECHANIC = new Type( 2, 6 );
            public static readonly Type ENGINEER = new Type( 2, 7 );

            public static readonly Type[] All =
            {
                OVERKILL,
                SHARPSHOOTER,
                DEXTERITY,
                CARDIO,
                EXERCISE,
                DIVING,
                PARKOUR,
                SNEAKYBEAKY,
                VITALITY,
                IMMUNITY,
                TOUGHNESS,
                STRENGTH,
                WARMBLOODED,
                SURVIVAL,
                HEALING,
                CRAFTING,
                OUTDOORS,
                COOKING,
                FISHING,
                AGRICULTURE,
                MECHANIC,
                ENGINEER
            };

            internal byte SpecialityIndex;
            internal byte SkillIndex;

            private Type( byte specialityIndex, byte skillIndex )
            {
                SpecialityIndex = specialityIndex;
                SkillIndex = skillIndex;
            }
        }
    }
}