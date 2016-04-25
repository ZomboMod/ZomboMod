using System;
using System.Reflection;
using static System.Reflection.BindingFlags;

namespace ZomboMod.Common
{
    public static class Reflection
    {
        public static FieldInfo GetField<T>( string fieldName )
        {
            var fld = typeof (T).GetField( fieldName, Public | NonPublic | Static | Instance );//TODO

            if ( fld == null )
            {
                throw new Exception( $"Field {typeof(T).Name}.{fieldName} not found!");
            }

            return fld;
        }
    }
}