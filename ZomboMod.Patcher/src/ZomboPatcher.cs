﻿/*
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using ZomboMod.Common;

namespace ZomboMod.Patcher
{
    public sealed class ZomboPatcher
    {
        public static ModuleDefinition UnturnedDef { get; set; }
        public static ModuleDefinition ZomboDef { get; set; }
        public static ModuleDefinition PatcherDef { get; set; }

        private static Patch _currentPatchInst;

        private static readonly TokenDefinition[] Defaults = {
            new TokenDefinition( @"([""'])(?:\\\1|.)*?\1", "QUOTED-STRING" ),
            new TokenDefinition( @"[*<>\?\-+/A-Za-z->!]+", "SYMBOL" ),
            new TokenDefinition( @"\s*\(\s*", "LEFT" ),
            new TokenDefinition( @"\s*\)\s*", "RIGHT" ),
            new TokenDefinition( @"\s*,\s*", "COMMA" )
        };

        private static void ExpectToken(Lexer lexer, string s)
        {
            if ( lexer.Next() && lexer.Token.Equals( s ) ) return;
            throw new Exception( $"Invalid token {lexer.Token} at {lexer.Position}. " +
                                 $"Expected '{s}'." );
        }

        private static void InjectMethod(MethodDefinition mdef, CustomAttribute injectAttr)
        {
            var inProp = injectAttr.Properties.FirstOrDefault(p => p.Name.Equals("In"));
            var atProp = injectAttr.Properties.FirstOrDefault(p => p.Name.Equals("At"));
            var typeProp = injectAttr.Properties.FirstOrDefault(p => p.Name.Equals("Type"));
            var typeVal = (string) typeProp.Argument.Value ?? "INJECT_BODY"; // default

            var atVal = atProp.Argument.Value as string;
            var inVal = inProp.Argument.Value as string;

            var injectAttrOfPatch = mdef.DeclaringType.CustomAttributes.FirstOrDefault(attr =>
                attr.AttributeType.ToString().Equals("ZomboMod.Patcher.InjectAttribute")
            );
            var targetType = null as TypeDefinition;
            var targetMethod = null as MethodDefinition;

            if (injectAttrOfPatch != null)
            {
                targetType = UnturnedDef.GetType((string) injectAttrOfPatch.Properties
                                               .First(p => p.Name.Equals("In")).Argument.Value);
            }
            if (inVal != null && targetType != null)
            {
                targetMethod = targetType.Methods.FirstOrDefault(m => m.Name.Equals(inVal));
                _currentPatchInst.CurrentMethod = targetMethod;
            }

            switch (typeVal)
            {
                case "INJECT_BODY":
                {
                    if (injectAttrOfPatch == null)
                    {
                        throw new Exception($"{mdef.DeclaringType} must have 'Inject' attribute.");
                    }

                    if (inVal == null)
                    {
                        throw new Exception($"inVal == null at {mdef}.");
                    }
                    if (atVal == null)
                    {
                        throw new Exception($"atVal == null at {mdef}.");
                    }

                    var lexer = new Lexer(new StringReader(atVal), Defaults);
                    var index = -1;

                    Console.WriteLine($"Injecting '{mdef.DeclaringType}::{mdef.Name}' in '{targetType}::{inVal}' at '{atVal}'");

                    if (!lexer.Next() ) return;

                    if (!lexer.Token.Equals("SYMBOL"))
                    {
                        throw new Exception( $"Invalid token {lexer.Token}('{lexer.TokenContents}') " +
                                            $"at {lexer.Position}. Expected 'SYMBOL'" );
                    }

                    switch (lexer.TokenContents.ToUpperInvariant())
                    {
                        case "BEFORE":
                        case "AFTER":
                            var at = lexer.TokenContents.ToUpperInvariant();

                            ExpectToken(lexer, "LEFT");
                            ExpectToken(lexer, "SYMBOL");

                            var opCode = lexer.TokenContents;
                            var operand = null as string;

                            if (lexer.Next() && !lexer.Token.Equals( "RIGHT" ))
                            {
                                if (lexer.Token.Equals( "COMMA" ))
                                {
                                    ExpectToken(lexer, "QUOTED-STRING");
                                    operand = lexer.TokenContents;
                                    operand = operand.Substring(1, operand.Length - 2);//Remove quotes
                                }
                                else
                                {
                                    throw new Exception($"Invalid token {lexer.Token}('{lexer.TokenContents}') " +
                                                        $"at {lexer.Position}. Expected 'RIGHT'");
                                }
                            }
                            /*
                                %ct = contains
                                %sw = startsWith (TODO?)
                                %ew = endsWith (TODO?)
                            */
                            Func<String, bool> checkOperand = op => {
                                if (op.EqualsIgnoreCase(operand))
                                    return true;
                                if (operand.StartsWith("%ct"))
                                    return op.ContainsIgnoreCase(operand.Substring(3));
                                return false;
                            };
                            var targetMdInstrs = targetMethod.Body.Instructions;
                            for (int i = 0; i < targetMdInstrs.Count; i++)
                            {
                                var instr = targetMdInstrs[i];

                                if (instr.OpCode.ToString().EqualsIgnoreCase(opCode) &&
                                    checkOperand(instr.Operand.ToString()))
                                {
                                    index = (at == "AFTER" ? i : i + 1);
                                    break;
                                }
                            }
                            if (index == -1)
                            {
                                throw new Exception($"Count not find opCode/operand ({opCode}: '{operand}')");
                            }
                            break;

                        case "START":
                            index = 0;
                            break;

                        case "END":
                            index = targetMethod.Body.Instructions.Count - 1;
                            break;

                        default:
                            throw new Exception($"Invalid token content '{lexer.TokenContents}' " +
                                                $"at {lexer.Position}.");
                    }

                    Collection<VariableDefinition> newVars = new Collection<VariableDefinition>();
                    mdef.Body.Variables.ForEach(newVars.Add);
                    targetMethod.Body.Variables.ForEach(newVars.Add);
                    targetMethod.Body.Variables.Clear();
                    newVars.ForEach(targetMethod.Body.Variables.Add);

                    targetMethod.Body.SimplifyMacros();
                    var instructions = mdef.Body.Instructions.Where(c => c.OpCode != OpCodes.Nop).ToList();
                    for (int i = 0; i < instructions.Count; i++)
                    {
                        var cur = instructions[i];
                        if (cur.OpCode == OpCodes.Ret)
                        {
                            continue;
                        }
                        if (cur.OpCode == OpCodes.Ldstr &&
                           instructions[i + 1].Operand.ToString().Contains("Patch::Emit(System.String)"))
                        {
                            var rawInstr = (string) cur.Operand;
                            Parse(rawInstr).ForEach(ii => {
                                targetMethod.Body.Instructions.Insert(index++, ii);
                            });
                            i += 1; // skip call Emit()
                            continue;
                        }
                        if (cur.OpCode == OpCodes.Call &&
                            cur.Operand.ToString().Contains("Patch::SkipNext()"))
                        {
                            i+= 1;
                            continue;
                        }
                        var methodRef = cur.Operand as MethodReference;
                        if (methodRef != null)
                        {
                            cur.Operand = UnturnedDef.Import(methodRef.Resolve());
                        }
                        targetMethod.Body.Instructions.Insert(index++, cur);
                    }
                    targetMethod.Body.OptimizeMacros();
                    break;
                }

                case "EXECUTE":
                {
                    // Execute method via reflection
                    Console.WriteLine($"Executing '{mdef.DeclaringType}::{mdef.Name}'");
                    var currentAssembly = typeof(ZomboPatcher).Assembly;
                    var declaringType = currentAssembly.GetType(mdef.DeclaringType.ToString());
                    var method = declaringType.GetMethod(mdef.Name);
                    method.Invoke(_currentPatchInst, null);
                    break;
                }
            }
        }

        private static void Main(string[] args)
        {
            try
            {

                Console.Title = "ZomboPatcher";
                Directory.SetCurrentDirectory( @"..\..\UnturnedDlls\" );

                var unturnedAssembly = AssemblyDefinition.ReadAssembly( "Assembly-CSharp.dll" );
                var zomboAssembly = AssemblyDefinition.ReadAssembly( @"..\bin\Debug\ZomboMod.dll" );
                var patcherAssembly = AssemblyDefinition.ReadAssembly( @"..\bin\Debug\ZomboMod.Patcher.exe" );

                UnturnedDef = unturnedAssembly.MainModule;
                ZomboDef = zomboAssembly.MainModule;
                PatcherDef = patcherAssembly.MainModule;

                patcherAssembly.MainModule.AssemblyReferences.Add( zomboAssembly.Name );
                unturnedAssembly.MainModule.AssemblyReferences.Add( zomboAssembly.Name );

                var patchType = PatcherDef.GetType("ZomboMod.Patcher.Patch");

                patcherAssembly.MainModule
                    .GetAllTypes()
                    .Where(t => !t.IsAbstract)
                    .Where(t => t.BaseType == patchType) // TODO: recursive check ?
                    .SelectMany(t => {
                        var type = typeof(ZomboPatcher).Assembly.GetType(t.FullName);
                        _currentPatchInst = (Patch) Activator.CreateInstance(type);
                        return t.Methods;
                    })
                    .Where(m => m.CustomAttributes.FirstOrDefault() != null)//For some reason the .ctor pass in first Where (WTF?)
                    .Where(m => m.IsPublic)
                    .ForEach(m => {
                        var injectAttr = m.CustomAttributes.FirstOrDefault();
                        InjectMethod(m, injectAttr);
                    });

                unturnedAssembly.Write("Patched.dll");
                unturnedAssembly.Write(@"C:\Users\Leonardo\Documents\Unturned\Zombo\All\Unturned\Unturned_Data\Managed\Assembly-CSharp.dll");

                //TODO: Programmatically copy all UnturnedDlls to Debug folder to avoid errors.
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex );
            }

            Console.ReadKey();
        }

        //TODO: Implement on demand
        public static Instruction[] Parse(string raw)
        {
            var instructions = new List<Instruction>();
            var opCodesType = typeof(OpCodes);
            var fieldFlags = BindingFlags.Public | BindingFlags.Static;
            var rawInstructions = new Dictionary<string, string>();
            var opCodeBuilder = new StringBuilder();
            var operandBuilder = new StringBuilder();

            raw.Split(';')
               .Select(r => r.Trim())
               .Where(r => !string.IsNullOrEmpty(r))
               .ForEach(r => {
                    var rawOpCode = null as string;
                    var rawOperand = null as string;
                    if (r.Contains(','))
                    {
                        rawOpCode = r.Substring(0, r.IndexOf(','));
                        rawOperand = r.Substring(r.IndexOf(',') + 1);
                    }
                    else
                    {
                        rawOpCode = r;
                    }
                    rawOpCode = rawOpCode?.Trim();
                    rawOperand = rawOperand?.Trim();
                    OpCode? opCode = (OpCode) opCodesType.GetField(rawOpCode, fieldFlags)?.GetValue(null);

                    if (!opCode.HasValue)
                        throw new Exception($"Invalid opcode '{rawOpCode}'");
                    var opCodeVal = opCode.Value;

                    if (opCodeVal == OpCodes.Call)
                    {
                        var targetModule = null as ModuleDefinition;

                        if (rawOperand == null)
                            throw new Exception($"[Call] Operand == null");

                        if (rawOperand.StartsWith("[unturned]"))
                        {
                            rawOperand = rawOperand.Substring(10).Trim();
                            targetModule = UnturnedDef;
                        }
                        else if (rawOperand.StartsWith("[zombo]"))
                        {
                            rawOperand = rawOperand.Substring(8).Trim();
                            targetModule = ZomboDef;
                        }

                        var rawMethodParts = rawOperand.Split(new string[] { "::" },
                                                              StringSplitOptions.None);
                        var rawType = rawMethodParts[0];
                        var rawMethod = rawMethodParts[1];

                        // Ignore parameters for now
                        if (rawMethod.EndsWith("()"))
                            rawMethod = rawMethod.Substring(0, rawMethod.Length - 2);

                        var type = targetModule.GetType(rawType);

                        if (type == null)
                            throw new Exception($"Type not found '{rawType}'");
                        var method = type.Methods.FirstOrDefault(md => md.Name.Equals(rawMethod));

                        if (type == null)
                            throw new Exception($"Method not found '{rawMethod}'");

                        instructions.Add(Instruction.Create(OpCodes.Call, method));
                    }
                    else
                    {
                        instructions.Add(Instruction.Create(opCodeVal));
                    }
                });
            return instructions.ToArray();
        }
    }

    //TODO Reformat code
    #region __LEX__

    public interface IMatcher
    {
        int Match(string text);
    }

    internal sealed class RegexMatcher : IMatcher
    {
        private readonly Regex regex;

        public RegexMatcher(string regex)
        {
            this.regex = new Regex($"^{regex}");
        }

        public int Match(string text)
        {
            var m = regex.Match(text);
            return m.Success ? m.Length : 0;
        }

        public override string ToString()
        {
            return regex.ToString();
        }
    }

    public sealed class TokenDefinition
    {
        public readonly IMatcher Matcher;
        public readonly object Token;

        public TokenDefinition(string regex, object token)
        {
            Matcher = new RegexMatcher(regex);
            Token = token;
        }
    }

    public sealed class Lexer : IDisposable
    {
        private readonly TextReader reader;
        private readonly TokenDefinition[] tokenDefinitions;
        private string lineRemaining;

        public string TokenContents { get; private set; }
        public object Token { get; private set; }
        public int LineNumber { get; private set; }
        public int Position { get; private set; }

        public Lexer(TextReader reader, TokenDefinition[] tokenDefinitions)
        {
            this.reader = reader;
            this.tokenDefinitions = tokenDefinitions;
            nextLine();
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        private void nextLine()
        {
            do
            {
                lineRemaining = reader.ReadLine();
                ++LineNumber;
                Position = 0;
            }
            while (lineRemaining != null && lineRemaining.Length == 0);
        }

        public bool Next()
        {
            if (lineRemaining == null)
            {
                return false;
            }

            foreach (var def in tokenDefinitions)
            {
                var matched = def.Matcher.Match(lineRemaining);
                if (matched <= 0)
                {
                    continue;
                }
                Position += matched;
                Token = def.Token;
                TokenContents = lineRemaining.Substring(0, matched);
                lineRemaining = lineRemaining.Substring(matched);
                if (lineRemaining.Length == 0)
                {
                    nextLine();
                }
                return true;
            }

            throw new Exception($"Unable to match against any tokens at line " +
                                $"{LineNumber} position {Position} \"{lineRemaining}\"");
        }
    }
    #endregion
}
