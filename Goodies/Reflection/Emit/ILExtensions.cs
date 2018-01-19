using System;
using System.Reflection;
using System.Reflection.Emit;

namespace BusterWood.Reflection.Emit
{
    public static class ILExtensions
    {
        public static LocalBuilder DeclareLocal<T>(this ILGenerator il)
        {
            return il.DeclareLocal(typeof(T));
        }

        public static ILGenerator Add(this ILGenerator il)
        {
            il.Emit(OpCodes.Add);
            return il;
        }

        public static ILGenerator Xor(this ILGenerator il)
        {
            il.Emit(OpCodes.Xor);
            return il;
        }

        public static ILGenerator Box(this ILGenerator il, Type type)
        {
            il.Emit(OpCodes.Box, type);
            return il;
        }

        public static ILGenerator This(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            return il;
        }

        public static ILGenerator Arg0(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            return il;
        }

        public static ILGenerator Arg1(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_1);
            return il;
        }

        public static ILGenerator Arg2(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_2);
            return il;
        }

        public static ILGenerator Arg1Address(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldarga_S, 1);
            return il;
        }

        public static ILGenerator Arg2Address(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldarga_S, 2);
            return il;
        }

        public static ILGenerator Arg(this ILGenerator il, int index)
        {
            il.Emit(OpCodes.Ldarg, index);
            return il;
        }
        /// <summary>Goto label is last two arguments on the stack are equal</summary>
        public static ILGenerator IfEqualGoto(this ILGenerator il, Label label)
        {
            il.Emit(OpCodes.Beq, label);
            return il;
        }

        /// <summary>Goto label is last argument on the stack is false</summary>
        public static ILGenerator IfFalseGoto(this ILGenerator il, Label notEqual)
        {
            il.Emit(OpCodes.Brfalse, notEqual);
            return il;
        }

        /// <summary>Goto label is last two arguments on the stack are equal</summary>
        public static ILGenerator IfNotEqualGoto(this ILGenerator il, Label notEqual)
        {
            il.Emit(OpCodes.Bne_Un, notEqual);
            return il;
        }

        /// <summary>Goto label is last argument on the stack is true</summary>
        public static ILGenerator IfTrueGoto(this ILGenerator il, Label label)
        {
            il.Emit(OpCodes.Brtrue, label);
            return il;
        }

        public static ILGenerator Call<T>(this ILGenerator il, string method) => il.Call(typeof(T).GetTypeInfo().GetDeclaredMethod(method));

        public static ILGenerator Call(this ILGenerator il, MethodInfo method)
        {
            il.Emit(OpCodes.Call, method);
            return il;
        }

        public static ILGenerator Call(this ILGenerator il, ConstructorInfo ctor)
        {
            il.Emit(OpCodes.Call, ctor);
            return il;
        }

        public static ILGenerator GetPropertyValue<T>(this ILGenerator il, string propName) => il.CallGetProperty(typeof(T).GetTypeInfo().GetDeclaredProperty(propName));

        public static ILGenerator CallGetProperty(this ILGenerator il, PropertyInfo prop)
        {
            il.CallVirt(prop.GetMethod);
            return il;
        }

        public static ILGenerator CallVirt<T>(this ILGenerator il, string method) => il.CallVirt(typeof(T).GetTypeInfo().GetDeclaredMethod(method));

        public static ILGenerator CallVirt(this ILGenerator il, MethodInfo method)
        {
            il.Emit(OpCodes.Callvirt, method);
            return il;
        }

        public static ILGenerator Cast(this ILGenerator il, Type toType)
        {
            il.Emit(OpCodes.Castclass, toType);
            return il;
        }

        public static ILGenerator New(this ILGenerator il, ConstructorInfo ctor)
        {
            il.Emit(OpCodes.Newobj, ctor);
            return il;
        }

        public static ILGenerator Constant(this ILGenerator il, int i)
        {
            il.Emit(OpCodes.Ldc_I4, i);
            return il;
        }

        public static ILGenerator Multiply(this ILGenerator il)
        {
            il.Emit(OpCodes.Mul);
            return il;
        }

        public static ILGenerator LoadAddress(this ILGenerator il, LocalBuilder local)
        {
            il.Emit(OpCodes.Ldloca, local);
            return il;
        }

        public static ILGenerator LoadAddress(this ILGenerator il, FieldBuilder f)
        {
            il.Emit(OpCodes.Ldflda, f);
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, string str)
        {
            il.Emit(OpCodes.Ldstr, str);
            return il;
        }

        /// <summary>Loads an integer onto the stack.  Uses optimized code for i between 0 and 8</summary>
        public static ILGenerator Load(this ILGenerator il, int i)
        {
            switch (i)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    il.Emit(OpCodes.Ldc_I4, i);
                    break;
            }
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, Type type)
        {
            il.Emit(OpCodes.Ldtoken, type);
            il.Call(typeof(Type).GetTypeInfo().GetDeclaredMethod(nameof(Type.GetTypeFromHandle)));
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, LocalBuilder local)
        {
            il.Emit(OpCodes.Ldloc, local);
            return il;
        }

        public static ILGenerator Null(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldnull);
            return il;
        }

        public static ILGenerator Store(this ILGenerator il, LocalBuilder local)
        {
            il.Emit(OpCodes.Stloc, local);
            return il;
        }

        public static ILGenerator AsType(this ILGenerator il, Type type)
        {
            il.Emit(OpCodes.Isinst, type);
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, FieldBuilder field)
        {
            il.Emit(OpCodes.Ldfld, field);
            return il;
        }

        public static ILGenerator Store(this ILGenerator il, FieldBuilder field)
        {
            il.Emit(OpCodes.Stfld, field);
            return il;
        }

        public static ILGenerator Return(this ILGenerator il)
        {
            il.Emit(OpCodes.Ret);
            return il;
        }

        public static ILGenerator Return(this ILGenerator il, bool val)
        {
            il.Emit(val ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);
            return il;
        }

        public static ILGenerator Switch(this ILGenerator il, Label[] lables)
        {
            il.Emit(OpCodes.Switch, lables);
            return il;
        }

    }
}