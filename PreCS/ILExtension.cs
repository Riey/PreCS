using Mono.Cecil.Cil;
using System;

namespace PreCS
{
    static class ILExtension
    {
        internal static Instruction Save(this ILProcessor il, object value)
        {
            if (value == null)
                return il.Create(OpCodes.Nop);
            switch (value)
            {
                case ulong ulnum:
                    return il.Save(unchecked((long)ulnum));
                case long lnum:
                    return il.Create(OpCodes.Ldc_I8, lnum);
                case uint unum:
                    return il.Save(unchecked((int)unum));
                case int num:
                    if (num == -1)
                        return il.Create(OpCodes.Ldc_I4_M1);
                    else if (num >= 0 && num <= 8)
                        return il.Create((OpCode)typeof(OpCodes).GetField("Ldc_I4_" + num.ToString()).GetValue(null));
                    else if (num <= 127 && num >= -128)
                        return il.Create(OpCodes.Ldc_I4_S, (SByte)num);
                    else
                        return il.Create(OpCodes.Ldc_I4, num);
                case string str:
                    return il.Create(OpCodes.Ldstr, str);
                default:
                    return il.Create(OpCodes.Nop);
            }
        }

        internal static object Convert(this Instruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException(nameof(instruction));
            switch (instruction.OpCode.Code)
            {
                case Code.Nop:
                    return Convert(instruction.Next);
                case Code.Ldc_I4:
                case Code.Ldc_I8:
                case Code.Ldc_R4:
                case Code.Ldc_R8:
                    return instruction.Operand;
                case Code.Ldc_I4_S:
                    return (Int32)(SByte)instruction.Operand;
                case Code.Ldc_I4_M1:
                    return (Int32)(-1);
                default:
                    int code = (int)instruction.OpCode.Code;
                    if (code >= 22 && code <= 30)
                        return code - 22;
                    else
                        throw new ArgumentException("OpCode must be Ldc_XX", nameof(instruction));
            }
        }
    }
}
