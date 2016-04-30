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

        public static string ArrayToString<T>( this T[] arr )
        {
            return MiscUtil.ArrayToString( arr );
        }


        public static V GetOrDefault<K, V>( this Dictionary<K, V> dict, K key, V def )
        {
            V value;
            return dict.TryGetValue( key, out value ) ? value : def;
        }

        public static void PutOrUpdate<K, V>( this Dictionary<K, V> dict, K key, V val )
        {
            if ( dict.ContainsKey( key ) )
                dict[key] = val;
            else
                dict.Add( key, val );
        }


        public static bool ContainsIgnoreCase( this string str, string part )
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf( str, part, CompareOptions.IgnoreCase ) >= 0;
        }

        public static bool EqualsIgnoreCase( this string str, string str2 )
        {
            return string.Compare( str, str2, StringComparison.InvariantCultureIgnoreCase ) == 0;
        }
        
        public static bool IsNullOrEmpty( this string str )
        {
            return string.IsNullOrEmpty( str );
        }

        public static string Capitalize( this string str )
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase( str.ToLowerInvariant() );
        }
        
        public static string Format( this string str, params object[] args ) 
        {
            return string.Format( str, args );   
        }
    }
}