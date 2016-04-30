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
using System.Text;

namespace ZomboMod.Common
{
    public static class MiscUtil
    {
        public static bool TryParseEnum<T>( string raw, out T ret )
        {
            try
            {
                ret =  (T) Enum.Parse( typeof(T), raw, true );
                return true;
            }
            catch (Exception)
            {
                ret = default(T);
                return false;
            }
        }
        
        public static string ArrayToString<T>( T[] array, string separator = ", ",
                                               string start = "[", string end = "]"  )
        {
            var sb = new StringBuilder( start );
            var arrLength = array.Length;
            
            for (int i = 0; i < arrLength; i++)
            {
                sb.Append( array[i] );
                
                if ( (i + 1) != arrLength )
                {
                    sb.Append( separator );
                }
            }
            
            sb.Append( end );
            return sb.ToString();
        }
    }
}