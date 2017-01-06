using Mono.Cecil.Cil;
using System;
using Mono.Cecil;

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

        internal static Instruction LoadArg(this ILProcessor il, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            else if (index == 0)
                return il.Create(OpCodes.Ldarg_0);
            else if (index == 1)
                return il.Create(OpCodes.Ldarg_1);
            else if (index == 2)
                return il.Create(OpCodes.Ldarg_2);
            else if (index == 3)
                return il.Create(OpCodes.Ldarg_3);
            else if (index <= byte.MaxValue)
                return il.Create(OpCodes.Ldarg_S, (byte)index);
            else
                return il.Create(OpCodes.Ldarg, index);
        }
    }
}
