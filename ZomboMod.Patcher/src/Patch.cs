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

using System.Linq;
using Mono.Cecil;

namespace ZomboMod.Patcher
{
    public abstract class   Patch
    {
        public ModuleDefinition UnturnedDefinition => ZomboPatcher.UnturnedDef;
        public MethodDefinition CurrentMethod { get; internal set; }
        
        private TypeDefinition _cachedType;
        private bool _hasType = true;
        
        public TypeDefinition Type 
        { 
            get 
            {
                if (_cachedType == null && _hasType)
                {
                    var thisType = ZomboPatcher.PatcherDef.GetType(this.GetType().FullName);
                    var injectAttr = thisType.CustomAttributes.FirstOrDefault(attr =>
                        attr.AttributeType.ToString().Equals("ZomboMod.Patcher.InjectAttribute")
                    );
                    if (injectAttr == null)
                    {
                        _hasType = false;
                        return null;
                    }
                    var inVal = (string) injectAttr.Properties.First(p => p.Name.Equals("In")).Argument.Value;
                    _cachedType = UnturnedDefinition.GetType(inVal);
                }
                return _cachedType;
            } 
        }
        
        protected MethodDefinition GetMethod(string type, string methodName) 
        {
            type = type.Replace("{u}", "SDG.Unturned");
            return UnturnedDefinition.GetType(type).Methods.FirstOrDefault(md => md.Name.Equals(methodName));
        }
        
        protected FieldDefinition GetField(string type, string fieldName) 
        {
            type = type.Replace("{u}", "SDG.Unturned");
            return UnturnedDefinition.GetType(type).Fields.FirstOrDefault(fl => fl.Name.Equals(fieldName));
        }

        /// <summary>
        /// Used by patcher.
        /// Emit instructions.
        /// </summary>
        protected static void Emit(string il) {}

        /// <summary>
        /// Used by patcher.
        /// Skip next instruction.
        /// </summary>
        protected static void SkipNext() {}
    }
}