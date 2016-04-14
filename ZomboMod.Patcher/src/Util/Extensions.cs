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
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace ZomboMod.Patcher.Util
{
    public static class Extensions
    {
        public static void ForEach<T>( this T[] array, Action<T> act )
        {
            foreach ( var obj in array )
            {
                act( obj );
            }
        }

        public static void ForEach<T>( this IEnumerable<T> enume, Action<T> act )
        {
            foreach ( var obj in enume )
            {
                act( obj );
            }
        }

        public static MethodDefinition GetMethod( this ModuleDefinition mdef, string typeName, string methodName, int paramsCount = -1 )
        {
            var type = mdef.GetType( typeName );

            if ( type == null )
            {
                throw new InvalidOperationException( $"Type {typeName} not found." );
            }

            var method = type.GetMethods().FirstOrDefault( _ => {
                return _.Name == methodName && (paramsCount == -1 || _.Parameters.Count == paramsCount);
            } );

            if ( method == null )
            {
                throw new InvalidOperationException( $"Method {typeName}::{methodName} not found." );
            }

            return method;
        }

        public static FieldDefinition GetField( this ModuleDefinition mdef, TypeDefinition type, string fieldName )
        {
            return GetField( mdef, type.FullName, fieldName );
        }

        public static FieldDefinition GetField( this ModuleDefinition mdef, string typeName, string fieldName )
        {
            var type = mdef.GetType( typeName );

            if ( type == null )
            {
                throw new InvalidOperationException( $"Type {typeName} not found." );
            }

            var method = type.Fields.First( _ => _.Name == fieldName );

            if ( method == null )
            {
                throw new InvalidOperationException( $"Field {typeName}.{fieldName} not found." );
            }

            return method;
        }

        public static PropertyDefinition GetProp( this ModuleDefinition mdef, string typeName, string propName )
        {
            var type = mdef.GetType( typeName );

            if ( type == null )
            {
                throw new InvalidOperationException( $"Type {typeName} not found." );
            }

            var method = type.Properties.First( _ => _.Name == propName );

            if ( method == null )
            {
                throw new InvalidOperationException( $"Property {typeName}.{propName} not found." );
            }

            return method;
        }
    }
}