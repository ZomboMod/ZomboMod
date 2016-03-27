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
using System.Globalization;

namespace ZomboMod.Common
{
    public static class Extensions
    {
        public static void ForEach<T>( this T[] array, Action<T> act )
        {
            foreach ( var obj in array ) act( obj );
        }

        public static void ForEach<T>( this IEnumerable<T> enumerable, Action<T> act )
        {
            foreach ( var obj in enumerable ) act( obj );
        }

        public static void ForEach<T>( this HashSet<T> set, Action<T> act )
        {
            foreach ( var obj in set ) act( obj );
        }

        public static V GetOrDefault<K, V>( this Dictionary<K, V> dict, K key, V def )
        {
            V value;
            return dict.TryGetValue( key, out value ) ? value : def;
        }

        public static bool ContainsIgnoreCase( this string str, string part )
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf( str, part, CompareOptions.IgnoreCase ) >= 0;
        }

        public static bool EqualsIgnoreCase( this string str, string str2 )
        {
            return string.Compare( str, str2, StringComparison.InvariantCultureIgnoreCase ) == 0;
        }
    }
}