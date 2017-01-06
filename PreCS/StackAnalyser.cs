using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PreCS
{
    internal static class StackAnalyser
    {
        internal static Module ExternModule;
        private static Instruction _lastInstruction;
        internal static object PopObject(Instruction instruction, List<Instruction> removeList = null)
        {
            _lastInstruction = instruction;
            object result = null;
            switch (instruction.OpCode.Code)
            {
                case Code.Add:
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                    result = (dynamic)PopObject(instruction.Previous, removeList) + (dynamic)PopObject(_lastInstruction.Previous, removeList);
                    break;
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                    result = (dynamic)PopObject(instruction.Previous, removeList) - (dynamic)PopObject(_lastInstruction.Previous, removeList);
                    break;
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                    result = (dynamic)PopObject(instruction.Previous, removeList) * (dynamic)PopObject(_lastInstruction.Previous, removeList);
                    break;

                case Code.Div:
                case Code.Div_Un:
                    result = (dynamic)PopObject(instruction.Previous, removeList) / (dynamic)PopObject(instruction.Previous, removeList);
                    break;

                case Code.Conv_I8:
                    result = Convert.ToInt64(PopObject(instruction.Previous, removeList));
                    break;
                case Code.Conv_I4:
                    result = Convert.ToInt32(PopObject(instruction.Previous, removeList));
                    break;
                case Code.Conv_I2:
                    result = Convert.ToInt16(PopObject(instruction.Previous, removeList));
                    break;
                case Code.Conv_I1:
                    result = Convert.ToSByte(PopObject(instruction.Previous, removeList));
                    break;
                case Code.Conv_U8:
                    result = Convert.ToUInt64(PopObject(instruction.Previous, removeList));
                    break;
                case Code.Conv_U4:
                    result = Convert.ToUInt32(PopObject(instruction.Previous, removeList));
                    break;
                case Code.Conv_U2:
                    result = Convert.ToUInt16(PopObject(instruction.Previous, removeList));
                    break;
                case Code.Conv_U1:
                    result = Convert.ToByte(PopObject(instruction.Previous, removeList));
                    break;

                case Code.Ldc_I4:
                case Code.Ldc_I8:
                case Code.Ldc_R4:
                case Code.Ldc_R8:
                case Code.Ldstr:
                    result = instruction.Operand;
                    break;

                case Code.Ldsfld:
                    {
                        var field = (instruction.Operand as FieldReference);
                        var type = Type.GetType(field.DeclaringType.FullName) ?? ExternModule.GetType(field.DeclaringType.FullName);
                        result = type.GetField(field.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                        break;
                    }

                case Code.Stsfld:
                    {
                        var field = (instruction.Operand as FieldReference);
                        var type = Type.GetType(field.DeclaringType.FullName) ?? ExternModule.GetType(field.DeclaringType.FullName);
                        type.GetField(field.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(PopObject(instruction.Previous, removeList));
                        break;
                    }

                case Code.Ldc_I4_S:
                    result = (int)(sbyte)instruction.Operand;
                    break;

                case Code.Ldc_I4_0:
                case Code.Ldc_I4_1:
                case Code.Ldc_I4_2:
                case Code.Ldc_I4_3:
                case Code.Ldc_I4_4:
                case Code.Ldc_I4_5:
                case Code.Ldc_I4_6:
                case Code.Ldc_I4_7:
                case Code.Ldc_I4_8:
                case Code.Ldc_I4_M1:
                    result = (int)instruction.OpCode.Code - 22;
                    break;

                default:
                    result = PopObject(instruction.Previous, removeList);
                    break;
            }
            removeList?.Add(instruction);
            return result;
        }

        internal static object[] PopObjects(Instruction instruction, int count, List<Instruction> removeList = null)
        {
            var args = new object[count];
            for (int i = 0; i < count; i++)
            {
                args[i] = PopObject(instruction, removeList);
            }
            return args;
        }
    }
}
