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
        public static ModuleDefinition UnityEngineDef { get; set; }

        private static Patch _currentPatchInst;

        private static readonly TokenDefinition[] Defaults = {
            new TokenDefinition( @"([""'])(?:\\\1|.)*?\1", "QUOTED-STRING" ),
            new TokenDefinition( @"^[A-Za-z][*<>_\?\-+/A-Za-z0-9]+", "SYMBOL" ),
            new TokenDefinition( @"\s*\(\s*", "LEFT" ),
            new TokenDefinition( @"\s*\)\s*", "RIGHT" ),
            new TokenDefinition( @"\s*,\s*", "COMMA" ),
            new TokenDefinition(@"[-+]?\d+", "INT")
        };

        private static void ExpectToken(Lexer lexer, string s)
        {
            if ( lexer.Next() && lexer.Token.Equals( s ) ) return;
            throw new Exception( $"Invalid token {lexer.Token} at {lexer.Position}. " +
                                 $"Expected '{s}'." );
        }
        
        private static void ExpectInstruction(Lexer lexer, out string opCode, out string operand)
        {
            ExpectToken(lexer, "SYMBOL");

            var opCode2 = lexer.TokenContents;
            var operand2 = null as string;
            if (lexer.Next() && !lexer.Token.Equals( "RIGHT" ))
            {
                if (lexer.Token.Equals( "COMMA" ))
                {
                    ExpectToken(lexer, "QUOTED-STRING");
                    operand2 = lexer.TokenContents;
                    operand2 = operand2.Substring(1, operand2.Length - 2);
                }
                else
                {
                    throw new Exception($"Invalid token {lexer.Token}('{lexer.TokenContents}') " +
                                        $"at {lexer.Position}. Expected 'COMMA'");
                }
            }
            opCode = opCode2;
            operand = operand2;
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
            /*
                %ct = contains
                %any = any operand
                %sw = startsWith (TODO?)
                %ew = endsWith (TODO?)
                operand = given operand
                op      = instruction operand
            */
            Func<String, String, bool> checkOperand = (operand, op) => {
                if ((string.IsNullOrEmpty(operand) && string.IsNullOrEmpty(op)) ||
                    (string.IsNullOrWhiteSpace(operand) && string.IsNullOrWhiteSpace(op)))
                    return true;
                if (operand.EqualsIgnoreCase("%any"))
                    return true;
                if (op.EqualsIgnoreCase(operand))
                    return true;
                if (operand.StartsWith("%ct"))
                    return op.ContainsIgnoreCase(operand.Substring(3));
                return false;
            };

            switch (typeVal)
            {
                case "INJECT_BODY":
                case "REPLACE_BODY":
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
                        case "PATTERN": 
                        {
                            ExpectToken(lexer, "LEFT");
                            ExpectToken(lexer, "INT"); // INDEX
                            var patterIndex = int.Parse(lexer.TokenContents);
                            
                            ExpectToken(lexer, "COMMA"); // BEFORE or AFTER
                            ExpectToken(lexer, "SYMBOL"); // BEFORE or AFTER
                            var beforeOrAfter = lexer.TokenContents;
                            
                            ExpectToken(lexer, "COMMA");
                            var patterInstructions = new Dictionary<string, string>();// opcode / operand

                            for (;;)
                            {
                                var opCode = null as string;
                                var operand = null as string;
                                
                                try
                                {
                                    ExpectInstruction(lexer, out opCode, out operand);
                                    patterInstructions.Add(opCode, operand);
                                    ExpectToken(lexer, "COMMA");
                                }
                                catch (System.Exception)
                                {
                                    break;
                                }
                            }
                            if (patterInstructions.Count == 0)
                            {
                                throw new Exception( "Expected at least 1 instruction." );
                            }
                            var found = 0;
                            var first = patterInstructions.First();
                            var targetMdInstrs = targetMethod.Body.Instructions;
                            for (int i = 0; i < targetMdInstrs.Count; i++)
                            {
                                var instr = targetMdInstrs[i];

                                if (instr.OpCode.ToString().EqualsIgnoreCase(first.Key) &&
                                    checkOperand(first.Value, instr.Operand.ToString()))
                                {
                                    found++;
                                    for (int j = 1; j < patterInstructions.Count; j++)
                                    {
                                        if (i + 1 >= targetMdInstrs.Count)
                                            break;
                                        instr = targetMdInstrs[++i];
                                        var curPattern = patterInstructions.ElementAt(j);
                                        var curInstrOpCode = instr.OpCode.ToString().Replace(".", "_");
                                        var hasFound = curInstrOpCode.EqualsIgnoreCase(curPattern.Key) &&
                                                       checkOperand(curPattern.Value, instr.Operand?.ToString());
                                        if (hasFound)
                                        {
                                            found++;
                                            if (patterIndex == j)
                                                index = i;
                                        }
                                    }
                                    if (found == patterInstructions.Count && patterIndex == 0)
                                        index = i; // index will be first
                                }
                            }
                            if (index == -1)
                            {
                                throw new Exception($"Count not find pattern {patterInstructions.ToArray().ArrayToString()}. " + 
                                                    $"Found {found} of {patterInstructions.Count}");
                            }
                            index = (beforeOrAfter == "BEFORE") ? index : index + 1;
                            break;
                        }

                        case "BEFORE":
                        case "AFTER":
                        {
                            var at = lexer.TokenContents.ToUpperInvariant();

                            ExpectToken(lexer, "LEFT");

                            var opCode = null as string;
                            var operand = null as string;
                            
                            ExpectInstruction(lexer, out opCode, out operand);
                            
                            var targetMdInstrs = targetMethod.Body.Instructions;
                            for (int i = 0; i < targetMdInstrs.Count; i++)
                            {
                                var instr = targetMdInstrs[i];

                                if (instr.OpCode.ToString().Replace(".", "_").EqualsIgnoreCase(opCode) &&
                                    checkOperand(operand, instr.Operand.ToString() ?? null))
                                {
                                    index = (at == "AFTER" ? i : i + 1);//TODO what?
                                    break;
                                }
                            }
                            if (index == -1)
                            {
                                throw new Exception($"Count not find opCode/operand ({opCode}: '{operand}')");
                            }
                            break;
                        }

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

                    if (typeVal.Equals("REPLACE_BODY"))
                    {
                        targetMethod.Body.Instructions.Clear();
                    }

                    targetMethod.Body.SimplifyMacros();
                    var instructions = mdef.Body.Instructions.Where(c => c.OpCode != OpCodes.Nop).ToList();
                    if (mdef.ReturnType.ToString().Contains("System.Void")) 
                    {
                        instructions = instructions.Take(instructions.Count - 1).ToList();;
                    }
                    for (int i = 0; i < instructions.Count; i++)
                    {
                        var cur = instructions[i];
                        if (cur.OpCode == OpCodes.Ldstr &&
                           instructions[i + 1].Operand.ToString().Contains("Patch::Emit(System.String)"))
                        {
                            var rawInstr = (string) cur.Operand;
                            Parse(rawInstr, targetMethod).ForEach(ii => {
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

                var unturnedAssembly = AssemblyDefinition.ReadAssembly("Assembly-CSharp.dll");
                var unityengineAssembly = AssemblyDefinition.ReadAssembly("UnityEngine.dll");
                var zomboAssembly = AssemblyDefinition.ReadAssembly(@"..\bin\Debug\ZomboMod.dll");
                var patcherAssembly = AssemblyDefinition.ReadAssembly(@"..\bin\Debug\ZomboMod.Patcher.exe");

                UnturnedDef = unturnedAssembly.MainModule;
                ZomboDef = zomboAssembly.MainModule;
                PatcherDef = patcherAssembly.MainModule;
                UnityEngineDef = unityengineAssembly.MainModule;

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
        public static Instruction[] Parse(string raw, MethodDefinition mdef)
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
                    
                    switch (rawOpCode)
                    {
                        case "Call":
                        {
                            var targetModule = null as ModuleDefinition;
                            if (rawOperand == null)
                                throw new Exception($"[Call] Operand == null");

                            if (rawOperand.StartsWith("[unturned]"))
                            {
                                rawOperand = rawOperand.Substring(10).Trim();
                                targetModule = UnturnedDef;
                            }
                            else if (rawOperand.StartsWith("[unityengine]"))
                            {
                                rawOperand = rawOperand.Substring(13).Trim();
                                targetModule = UnityEngineDef;
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
                            instructions.Add(Instruction.Create(OpCodes.Call, UnturnedDef.Import(method.Resolve())));
                            break;
                        }
                        case "Nop": instructions.Add(Instruction.Create(OpCodes.Nop)); break;
                        case "Break": instructions.Add(Instruction.Create(OpCodes.Break)); break;
                        case "Ldarg_0": instructions.Add(Instruction.Create(OpCodes.Ldarg_0)); break;
                        case "Ldarg_1": instructions.Add(Instruction.Create(OpCodes.Ldarg_1)); break;
                        case "Ldarg_2": instructions.Add(Instruction.Create(OpCodes.Ldarg_2)); break;
                        case "Ldarg_3": instructions.Add(Instruction.Create(OpCodes.Ldarg_3)); break;
                        case "Ldloc_0": instructions.Add(Instruction.Create(OpCodes.Ldloc_0)); break;
                        case "Ldloc_1": instructions.Add(Instruction.Create(OpCodes.Ldloc_1)); break;
                        case "Ldloc_2": instructions.Add(Instruction.Create(OpCodes.Ldloc_2)); break;
                        case "Ldloc_3": instructions.Add(Instruction.Create(OpCodes.Ldloc_3)); break;
                        case "Stloc_0": instructions.Add(Instruction.Create(OpCodes.Stloc_0)); break;
                        case "Stloc_1": instructions.Add(Instruction.Create(OpCodes.Stloc_1)); break;
                        case "Stloc_2": instructions.Add(Instruction.Create(OpCodes.Stloc_2)); break;
                        case "Stloc_3": instructions.Add(Instruction.Create(OpCodes.Stloc_3)); break;
                        case "Ldarg_S": instructions.Add(Instruction.Create(OpCodes.Ldarg_S)); break;
                        case "Ldarga_S":
                            var idx = int.Parse(rawOperand);
                            instructions.Add(Instruction.Create(OpCodes.Ldarga_S, mdef.Parameters[idx])); 
                            break;
                        case "Starg_S": instructions.Add(Instruction.Create(OpCodes.Starg_S)); break;
                        case "Ldloc_S": instructions.Add(Instruction.Create(OpCodes.Ldloc_S)); break;
                        case "Ldloca_S": 
                            idx = int.Parse(rawOperand);
                            instructions.Add(Instruction.Create(OpCodes.Ldloca_S, mdef.Body.Variables[idx])); 
                            break;
                        case "Stloc_S": instructions.Add(Instruction.Create(OpCodes.Stloc_S)); break;
                        case "Ldnull": instructions.Add(Instruction.Create(OpCodes.Ldnull)); break;
                        case "Ldc_I4_M1": instructions.Add(Instruction.Create(OpCodes.Ldc_I4_M1)); break;
                        case "Ldc_I4_0": instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0)); break;
                        case "Ldc_I4_1": instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1)); break;
                        case "Ldc_I4_2": instructions.Add(Instruction.Create(OpCodes.Ldc_I4_2)); break;
                        case "Ldc_I4_3": instructions.Add(Instruction.Create(OpCodes.Ldc_I4_3)); break;
                        case "Ldc_I4_4": instructions.Add(Instruction.Create(OpCodes.Ldc_I4_4)); break;
                        case "Ldc_I4_5": instructions.Add(Instruction.Create(OpCodes.Ldc_I4_5)); break;
                        case "Ldc_I4_6": instructions.Add(Instruction.Create(OpCodes.Ldc_I4_6)); break;
                        case "Ldc_I4_7": instructions.Add(Instruction.Create(OpCodes.Ldc_I4_7)); break;
                        case "Ldc_I4_8": instructions.Add(Instruction.Create(OpCodes.Ldc_I4_8)); break;
                        case "Ldc_I4_S": instructions.Add(Instruction.Create(OpCodes.Ldc_I4_S)); break;
                        case "Ldc_I4": instructions.Add(Instruction.Create(OpCodes.Ldc_I4)); break;
                        case "Ldc_I8": instructions.Add(Instruction.Create(OpCodes.Ldc_I8)); break;
                        case "Ldc_R4": instructions.Add(Instruction.Create(OpCodes.Ldc_R4)); break;
                        case "Ldc_R8": instructions.Add(Instruction.Create(OpCodes.Ldc_R8)); break;
                        case "Dup": instructions.Add(Instruction.Create(OpCodes.Dup)); break;
                        case "Pop": instructions.Add(Instruction.Create(OpCodes.Pop)); break;
                        case "Jmp": instructions.Add(Instruction.Create(OpCodes.Jmp)); break;
                        case "Calli": instructions.Add(Instruction.Create(OpCodes.Calli)); break;
                        case "Ret": instructions.Add(Instruction.Create(OpCodes.Ret)); break;
                        case "Br_S": instructions.Add(Instruction.Create(OpCodes.Br_S)); break;
                        case "Brfalse_S": instructions.Add(Instruction.Create(OpCodes.Brfalse_S)); break;
                        case "Brtrue_S": instructions.Add(Instruction.Create(OpCodes.Brtrue_S)); break;
                        case "Beq_S": instructions.Add(Instruction.Create(OpCodes.Beq_S)); break;
                        case "Bge_S": instructions.Add(Instruction.Create(OpCodes.Bge_S)); break;
                        case "Bgt_S": instructions.Add(Instruction.Create(OpCodes.Bgt_S)); break;
                        case "Ble_S": instructions.Add(Instruction.Create(OpCodes.Ble_S)); break;
                        case "Blt_S": instructions.Add(Instruction.Create(OpCodes.Blt_S)); break;
                        case "Bne_Un_S": instructions.Add(Instruction.Create(OpCodes.Bne_Un_S)); break;
                        case "Bge_Un_S": instructions.Add(Instruction.Create(OpCodes.Bge_Un_S)); break;
                        case "Bgt_Un_S": instructions.Add(Instruction.Create(OpCodes.Bgt_Un_S)); break;
                        case "Ble_Un_S": instructions.Add(Instruction.Create(OpCodes.Ble_Un_S)); break;
                        case "Blt_Un_S": instructions.Add(Instruction.Create(OpCodes.Blt_Un_S)); break;
                        case "Br": instructions.Add(Instruction.Create(OpCodes.Br)); break;
                        case "Brfalse": instructions.Add(Instruction.Create(OpCodes.Brfalse)); break;
                        case "Brtrue": instructions.Add(Instruction.Create(OpCodes.Brtrue)); break;
                        case "Beq": instructions.Add(Instruction.Create(OpCodes.Beq)); break;
                        case "Bge": instructions.Add(Instruction.Create(OpCodes.Bge)); break;
                        case "Bgt": instructions.Add(Instruction.Create(OpCodes.Bgt)); break;
                        case "Ble": instructions.Add(Instruction.Create(OpCodes.Ble)); break;
                        case "Blt": instructions.Add(Instruction.Create(OpCodes.Blt)); break;
                        case "Bne_Un": instructions.Add(Instruction.Create(OpCodes.Bne_Un)); break;
                        case "Bge_Un": instructions.Add(Instruction.Create(OpCodes.Bge_Un)); break;
                        case "Bgt_Un": instructions.Add(Instruction.Create(OpCodes.Bgt_Un)); break;
                        case "Ble_Un": instructions.Add(Instruction.Create(OpCodes.Ble_Un)); break;
                        case "Blt_Un": instructions.Add(Instruction.Create(OpCodes.Blt_Un)); break;
                        case "Switch": instructions.Add(Instruction.Create(OpCodes.Switch)); break;
                        case "Ldind_I1": instructions.Add(Instruction.Create(OpCodes.Ldind_I1)); break;
                        case "Ldind_U1": instructions.Add(Instruction.Create(OpCodes.Ldind_U1)); break;
                        case "Ldind_I2": instructions.Add(Instruction.Create(OpCodes.Ldind_I2)); break;
                        case "Ldind_U2": instructions.Add(Instruction.Create(OpCodes.Ldind_U2)); break;
                        case "Ldind_I4": instructions.Add(Instruction.Create(OpCodes.Ldind_I4)); break;
                        case "Ldind_U4": instructions.Add(Instruction.Create(OpCodes.Ldind_U4)); break;
                        case "Ldind_I8": instructions.Add(Instruction.Create(OpCodes.Ldind_I8)); break;
                        case "Ldind_I": instructions.Add(Instruction.Create(OpCodes.Ldind_I)); break;
                        case "Ldind_R4": instructions.Add(Instruction.Create(OpCodes.Ldind_R4)); break;
                        case "Ldind_R8": instructions.Add(Instruction.Create(OpCodes.Ldind_R8)); break;
                        case "Ldind_Ref": instructions.Add(Instruction.Create(OpCodes.Ldind_Ref)); break;
                        case "Stind_Ref": instructions.Add(Instruction.Create(OpCodes.Stind_Ref)); break;
                        case "Stind_I1": instructions.Add(Instruction.Create(OpCodes.Stind_I1)); break;
                        case "Stind_I2": instructions.Add(Instruction.Create(OpCodes.Stind_I2)); break;
                        case "Stind_I4": instructions.Add(Instruction.Create(OpCodes.Stind_I4)); break;
                        case "Stind_I8": instructions.Add(Instruction.Create(OpCodes.Stind_I8)); break;
                        case "Stind_R4": instructions.Add(Instruction.Create(OpCodes.Stind_R4)); break;
                        case "Stind_R8": instructions.Add(Instruction.Create(OpCodes.Stind_R8)); break;
                        case "Add": instructions.Add(Instruction.Create(OpCodes.Add)); break;
                        case "Sub": instructions.Add(Instruction.Create(OpCodes.Sub)); break;
                        case "Mul": instructions.Add(Instruction.Create(OpCodes.Mul)); break;
                        case "Div": instructions.Add(Instruction.Create(OpCodes.Div)); break;
                        case "Div_Un": instructions.Add(Instruction.Create(OpCodes.Div_Un)); break;
                        case "Rem": instructions.Add(Instruction.Create(OpCodes.Rem)); break;
                        case "Rem_Un": instructions.Add(Instruction.Create(OpCodes.Rem_Un)); break;
                        case "And": instructions.Add(Instruction.Create(OpCodes.And)); break;
                        case "Or": instructions.Add(Instruction.Create(OpCodes.Or)); break;
                        case "Xor": instructions.Add(Instruction.Create(OpCodes.Xor)); break;
                        case "Shl": instructions.Add(Instruction.Create(OpCodes.Shl)); break;
                        case "Shr": instructions.Add(Instruction.Create(OpCodes.Shr)); break;
                        case "Shr_Un": instructions.Add(Instruction.Create(OpCodes.Shr_Un)); break;
                        case "Neg": instructions.Add(Instruction.Create(OpCodes.Neg)); break;
                        case "Not": instructions.Add(Instruction.Create(OpCodes.Not)); break;
                        case "Conv_I1": instructions.Add(Instruction.Create(OpCodes.Conv_I1)); break;
                        case "Conv_I2": instructions.Add(Instruction.Create(OpCodes.Conv_I2)); break;
                        case "Conv_I4": instructions.Add(Instruction.Create(OpCodes.Conv_I4)); break;
                        case "Conv_I8": instructions.Add(Instruction.Create(OpCodes.Conv_I8)); break;
                        case "Conv_R4": instructions.Add(Instruction.Create(OpCodes.Conv_R4)); break;
                        case "Conv_R8": instructions.Add(Instruction.Create(OpCodes.Conv_R8)); break;
                        case "Conv_U4": instructions.Add(Instruction.Create(OpCodes.Conv_U4)); break;
                        case "Conv_U8": instructions.Add(Instruction.Create(OpCodes.Conv_U8)); break;
                        case "Callvirt": instructions.Add(Instruction.Create(OpCodes.Callvirt)); break;
                        case "Cpobj": instructions.Add(Instruction.Create(OpCodes.Cpobj)); break;
                        case "Ldobj": instructions.Add(Instruction.Create(OpCodes.Ldobj)); break;
                        case "Ldstr": instructions.Add(Instruction.Create(OpCodes.Ldstr)); break;
                        case "Newobj": instructions.Add(Instruction.Create(OpCodes.Newobj)); break;
                        case "Castclass": instructions.Add(Instruction.Create(OpCodes.Castclass)); break;
                        case "Isinst": instructions.Add(Instruction.Create(OpCodes.Isinst)); break;
                        case "Conv_R_Un": instructions.Add(Instruction.Create(OpCodes.Conv_R_Un)); break;
                        case "Unbox": instructions.Add(Instruction.Create(OpCodes.Unbox)); break;
                        case "Throw": instructions.Add(Instruction.Create(OpCodes.Throw)); break;
                        case "Ldfld": instructions.Add(Instruction.Create(OpCodes.Ldfld)); break;
                        case "Ldflda": instructions.Add(Instruction.Create(OpCodes.Ldflda)); break;
                        case "Stfld": instructions.Add(Instruction.Create(OpCodes.Stfld)); break;
                        case "Ldsfld": instructions.Add(Instruction.Create(OpCodes.Ldsfld)); break;
                        case "Ldsflda": instructions.Add(Instruction.Create(OpCodes.Ldsflda)); break;
                        case "Stsfld": instructions.Add(Instruction.Create(OpCodes.Stsfld)); break;
                        case "Stobj": instructions.Add(Instruction.Create(OpCodes.Stobj)); break;
                        case "Conv_Ovf_I1_Un": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_I1_Un)); break;
                        case "Conv_Ovf_I2_Un": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_I2_Un)); break;
                        case "Conv_Ovf_I4_Un": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_I4_Un)); break;
                        case "Conv_Ovf_I8_Un": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_I8_Un)); break;
                        case "Conv_Ovf_U1_Un": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_U1_Un)); break;
                        case "Conv_Ovf_U2_Un": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_U2_Un)); break;
                        case "Conv_Ovf_U4_Un": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_U4_Un)); break;
                        case "Conv_Ovf_U8_Un": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_U8_Un)); break;
                        case "Conv_Ovf_I_Un": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_I_Un)); break;
                        case "Conv_Ovf_U_Un": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_U_Un)); break;
                        case "Box": instructions.Add(Instruction.Create(OpCodes.Box)); break;
                        case "Newarr": instructions.Add(Instruction.Create(OpCodes.Newarr)); break;
                        case "Ldlen": instructions.Add(Instruction.Create(OpCodes.Ldlen)); break;
                        case "Ldelema": instructions.Add(Instruction.Create(OpCodes.Ldelema)); break;
                        case "Ldelem_I1": instructions.Add(Instruction.Create(OpCodes.Ldelem_I1)); break;
                        case "Ldelem_U1": instructions.Add(Instruction.Create(OpCodes.Ldelem_U1)); break;
                        case "Ldelem_I2": instructions.Add(Instruction.Create(OpCodes.Ldelem_I2)); break;
                        case "Ldelem_U2": instructions.Add(Instruction.Create(OpCodes.Ldelem_U2)); break;
                        case "Ldelem_I4": instructions.Add(Instruction.Create(OpCodes.Ldelem_I4)); break;
                        case "Ldelem_U4": instructions.Add(Instruction.Create(OpCodes.Ldelem_U4)); break;
                        case "Ldelem_I8": instructions.Add(Instruction.Create(OpCodes.Ldelem_I8)); break;
                        case "Ldelem_I": instructions.Add(Instruction.Create(OpCodes.Ldelem_I)); break;
                        case "Ldelem_R4": instructions.Add(Instruction.Create(OpCodes.Ldelem_R4)); break;
                        case "Ldelem_R8": instructions.Add(Instruction.Create(OpCodes.Ldelem_R8)); break;
                        case "Ldelem_Ref": instructions.Add(Instruction.Create(OpCodes.Ldelem_Ref)); break;
                        case "Stelem_I": instructions.Add(Instruction.Create(OpCodes.Stelem_I)); break;
                        case "Stelem_I1": instructions.Add(Instruction.Create(OpCodes.Stelem_I1)); break;
                        case "Stelem_I2": instructions.Add(Instruction.Create(OpCodes.Stelem_I2)); break;
                        case "Stelem_I4": instructions.Add(Instruction.Create(OpCodes.Stelem_I4)); break;
                        case "Stelem_I8": instructions.Add(Instruction.Create(OpCodes.Stelem_I8)); break;
                        case "Stelem_R4": instructions.Add(Instruction.Create(OpCodes.Stelem_R4)); break;
                        case "Stelem_R8": instructions.Add(Instruction.Create(OpCodes.Stelem_R8)); break;
                        case "Stelem_Ref": instructions.Add(Instruction.Create(OpCodes.Stelem_Ref)); break;
                        case "Ldelem_Any": instructions.Add(Instruction.Create(OpCodes.Ldelem_Any)); break;
                        case "Stelem_Any": instructions.Add(Instruction.Create(OpCodes.Stelem_Any)); break;
                        case "Unbox_Any": instructions.Add(Instruction.Create(OpCodes.Unbox_Any)); break;
                        case "Conv_Ovf_I1": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_I1)); break;
                        case "Conv_Ovf_U1": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_U1)); break;
                        case "Conv_Ovf_I2": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_I2)); break;
                        case "Conv_Ovf_U2": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_U2)); break;
                        case "Conv_Ovf_I4": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_I4)); break;
                        case "Conv_Ovf_U4": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_U4)); break;
                        case "Conv_Ovf_I8": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_I8)); break;
                        case "Conv_Ovf_U8": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_U8)); break;
                        case "Refanyval": instructions.Add(Instruction.Create(OpCodes.Refanyval)); break;
                        case "Ckfinite": instructions.Add(Instruction.Create(OpCodes.Ckfinite)); break;
                        case "Mkrefany": instructions.Add(Instruction.Create(OpCodes.Mkrefany)); break;
                        case "Ldtoken": instructions.Add(Instruction.Create(OpCodes.Ldtoken)); break;
                        case "Conv_U2": instructions.Add(Instruction.Create(OpCodes.Conv_U2)); break;
                        case "Conv_U1": instructions.Add(Instruction.Create(OpCodes.Conv_U1)); break;
                        case "Conv_I": instructions.Add(Instruction.Create(OpCodes.Conv_I)); break;
                        case "Conv_Ovf_I": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_I)); break;
                        case "Conv_Ovf_U": instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_U)); break;
                        case "Add_Ovf": instructions.Add(Instruction.Create(OpCodes.Add_Ovf)); break;
                        case "Add_Ovf_Un": instructions.Add(Instruction.Create(OpCodes.Add_Ovf_Un)); break;
                        case "Mul_Ovf": instructions.Add(Instruction.Create(OpCodes.Mul_Ovf)); break;
                        case "Mul_Ovf_Un": instructions.Add(Instruction.Create(OpCodes.Mul_Ovf_Un)); break;
                        case "Sub_Ovf": instructions.Add(Instruction.Create(OpCodes.Sub_Ovf)); break;
                        case "Sub_Ovf_Un": instructions.Add(Instruction.Create(OpCodes.Sub_Ovf_Un)); break;
                        case "Endfinally": instructions.Add(Instruction.Create(OpCodes.Endfinally)); break;
                        case "Leave": instructions.Add(Instruction.Create(OpCodes.Leave)); break;
                        case "Leave_S": instructions.Add(Instruction.Create(OpCodes.Leave_S)); break;
                        case "Stind_I": instructions.Add(Instruction.Create(OpCodes.Stind_I)); break;
                        case "Conv_U": instructions.Add(Instruction.Create(OpCodes.Conv_U)); break;
                        case "Arglist": instructions.Add(Instruction.Create(OpCodes.Arglist)); break;
                        case "Ceq": instructions.Add(Instruction.Create(OpCodes.Ceq)); break;
                        case "Cgt": instructions.Add(Instruction.Create(OpCodes.Cgt)); break;
                        case "Cgt_Un": instructions.Add(Instruction.Create(OpCodes.Cgt_Un)); break;
                        case "Clt": instructions.Add(Instruction.Create(OpCodes.Clt)); break;
                        case "Clt_Un": instructions.Add(Instruction.Create(OpCodes.Clt_Un)); break;
                        case "Ldftn": instructions.Add(Instruction.Create(OpCodes.Ldftn)); break;
                        case "Ldvirtftn": instructions.Add(Instruction.Create(OpCodes.Ldvirtftn)); break;
                        case "Ldarg": instructions.Add(Instruction.Create(OpCodes.Ldarg)); break;
                        case "Ldarga": instructions.Add(Instruction.Create(OpCodes.Ldarga)); break;
                        case "Starg": instructions.Add(Instruction.Create(OpCodes.Starg)); break;
                        case "Ldloc": instructions.Add(Instruction.Create(OpCodes.Ldloc)); break;
                        case "Ldloca": instructions.Add(Instruction.Create(OpCodes.Ldloca)); break;
                        case "Stloc": instructions.Add(Instruction.Create(OpCodes.Stloc)); break;
                        case "Localloc": instructions.Add(Instruction.Create(OpCodes.Localloc)); break;
                        case "Endfilter": instructions.Add(Instruction.Create(OpCodes.Endfilter)); break;
                        case "Unaligned": instructions.Add(Instruction.Create(OpCodes.Unaligned)); break;
                        case "Volatile": instructions.Add(Instruction.Create(OpCodes.Volatile)); break;
                        case "Tail": instructions.Add(Instruction.Create(OpCodes.Tail)); break;
                        case "Initobj": instructions.Add(Instruction.Create(OpCodes.Initobj)); break;
                        case "Constrained": instructions.Add(Instruction.Create(OpCodes.Constrained)); break;
                        case "Cpblk": instructions.Add(Instruction.Create(OpCodes.Cpblk)); break;
                        case "Initblk": instructions.Add(Instruction.Create(OpCodes.Initblk)); break;
                        case "No": instructions.Add(Instruction.Create(OpCodes.No)); break;
                        case "Rethrow": instructions.Add(Instruction.Create(OpCodes.Rethrow)); break;
                        case "Sizeof": instructions.Add(Instruction.Create(OpCodes.Sizeof)); break;
                        case "Refanytype": instructions.Add(Instruction.Create(OpCodes.Refanytype)); break;
                        case "Readonly": instructions.Add(Instruction.Create(OpCodes.Readonly)); break;

                        default:
                            throw new Exception($"Invalid opcode '{rawOpCode}'");
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
