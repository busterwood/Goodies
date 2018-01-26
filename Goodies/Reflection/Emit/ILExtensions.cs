using System;
using System.Reflection;
using System.Reflection.Emit;

namespace BusterWood.Reflection.Emit
{
    public static class ILExtensions
    {
        public static LocalBuilder DeclareLocal<T>(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            return il.DeclareLocal(typeof(T));
        }

        /// <summary>
        /// Adds one value from another.  
        /// Given val1 and val2 have been pushed to the stack, this instruction returns val1 + val1
        /// </summary>
        public static ILGenerator Add(this ILGenerator il, bool checkOverflow = false, bool unsignedValue = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (checkOverflow && unsignedValue)
                il.Emit(OpCodes.Add_Ovf_Un);
            if (checkOverflow)
                il.Emit(OpCodes.Add_Ovf);
            else
                il.Emit(OpCodes.Add);
            return il;
        }

        /// <summary>
        /// Subtracts one value from another.  
        /// Given val1 and val2 have been pushed to the stack, this instruction returns val2 - val1
        /// </summary>
        public static ILGenerator Subtract(this ILGenerator il, bool checkOverflow = false, bool unsignedValue = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (checkOverflow && unsignedValue)
                il.Emit(OpCodes.Sub_Ovf_Un);
            if (checkOverflow)
                il.Emit(OpCodes.Sub_Ovf);
            else
                il.Emit(OpCodes.Sub);
            return il;
        }


        public static ILGenerator Multiply(this ILGenerator il, bool checkOverflow = false, bool unsignedValue = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (checkOverflow && unsignedValue)
                il.Emit(OpCodes.Mul_Ovf_Un);
            else if (checkOverflow)
                il.Emit(OpCodes.Mul_Ovf);
            else
                il.Emit(OpCodes.Mul);
            return il;
        }

        /// <summary>Divides two integers or floating point numbers</summary>
        public static ILGenerator Divide(this ILGenerator il, bool unsignedValue = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (unsignedValue)
                il.Emit(OpCodes.Div_Un);
            else
                il.Emit(OpCodes.Div);
            return il;
        }

        /// <summary>Negates the value on the top of the stack</summary>
        public static ILGenerator Negate(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Neg);
            return il;
        }

        /// <summary>Divides the top two values on the stack and computes the remainder</summary>
        public static ILGenerator Remainder(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Rem);
            return il;
        }

        /// <summary>
        /// Shifts an 1) integer value (int32, int64, native int) to the left by 2) the number of bits (int32)
        /// </summary>
        public static ILGenerator ShiftLeft(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Shl);
            return il;
        }

        /// <summary>
        /// Shifts an 1) integer value (int32, int64, native int) to the right by 2) the number of bits (int32)
        /// </summary>
        public static ILGenerator ShiftRight(this ILGenerator il, bool unsigned = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (unsigned)
                il.Emit(OpCodes.Shr_Un);
            else
                il.Emit(OpCodes.Shr);
            return il;
        }

        /// <summary>Bitwise AND of the two integer values that are on the top of the stack</summary>
        public static ILGenerator And(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.And);
            return il;
        }

        /// <summary>Bitwise OR of the two integer values that are on the top of the stack</summary>
        public static ILGenerator Or(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Or);
            return il;
        }

        /// <summary>Bitwise XOR of the two integer values that are on the top of the stack</summary>
        public static ILGenerator Xor(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Xor);
            return il;
        }

        /// <summary>Bitwise complement of the two integer values that are on the top of the stack</summary>
        public static ILGenerator Not(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Not);
            return il;
        }
        
        /// <summary>Discards the value on the top of the stack</summary>
        public static ILGenerator Pop(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Pop);
            return il;
        }

        /// <summary>Stores a value type in a managed <see cref="Object"/></summary>
        public static ILGenerator Box(this ILGenerator il, Type type)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            il.Emit(OpCodes.Box, type);
            return il;
        }

        /// <summary>Gets the value of a boxed value type</summary>
        public static ILGenerator Unbox(this ILGenerator il, Type type)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            il.Emit(OpCodes.Unbox, type);
            return il;
        }

        /// <summary>
        /// Like <see cref="Unbox(ILGenerator, Type)"/> but equivalent to <see cref="Cast(ILGenerator, Type)"/> for reference types.
        /// Useful for generic types when you don't know if the generic argument is a value or reference type.
        /// </summary>
        public static ILGenerator UnboxAny(this ILGenerator il, Type type)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            il.Emit(OpCodes.Unbox_Any, type);
            return il;
        }

        /// <summary>Loads "this" onto the stack, i.e. argument zero</summary>
        public static ILGenerator This(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Ldarg_0);
            return il;
        }

        /// <summary>Loads an argument address onto the stack</summary>
        public static ILGenerator ArgAddress(this ILGenerator il, int index)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "index cannot be negative");

            if (index <= 255)
                il.Emit(OpCodes.Ldarga_S, index); // short form, more compact IL
            else
                il.Emit(OpCodes.Ldarga, index);
            return il;
        }

        /// <summary>Loads an argument onto the stack</summary>
        public static ILGenerator Arg(this ILGenerator il, int index)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "index cannot be negative");
            switch (index)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (index <= 255)
                        il.Emit(OpCodes.Ldarg_S, index);  // short form, more compact IL
                    else
                        il.Emit(OpCodes.Ldarg, index);
                    break;
            }
            return il;
        }

        /// <summary>Goto a label</summary>
        public static ILGenerator Goto(this ILGenerator il, Label label)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Br, label);
            return il;
        }
        
        /// <summary>Goto label if the last two arguments on the stack are equal</summary>
        public static ILGenerator IfEqualGoto(this ILGenerator il, Label label)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Beq, label);
            return il;
        }

        /// <summary>Goto label if the last argument on the stack is false</summary>
        public static ILGenerator IfFalseGoto(this ILGenerator il, Label notEqual)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Brfalse, notEqual);
            return il;
        }

        /// <summary>Goto label if the last two arguments on the stack are equal</summary>
        public static ILGenerator IfNotEqualGoto(this ILGenerator il, Label notEqual)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Bne_Un, notEqual);
            return il;
        }

        /// <summary>Goto label if the last argument on the stack is true</summary>
        public static ILGenerator IfTrueGoto(this ILGenerator il, Label label)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Brtrue, label);
            return il;
        }

        /// <summary>Goto label if the value1 is greater than or equal to value2</summary>
        public static ILGenerator IfGreaterOrEqualGoto(this ILGenerator il, Label label, bool unsigned = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (unsigned)
                il.Emit(OpCodes.Bge_Un, label);
            else
                il.Emit(OpCodes.Bge, label);
            return il;
        }

        /// <summary>Goto label if the value1 is greater than value2</summary>
        public static ILGenerator IfGreaterGoto(this ILGenerator il, Label label, bool unsigned = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (unsigned)
                il.Emit(OpCodes.Bgt_Un, label);
            else
                il.Emit(OpCodes.Bgt, label);
            return il;
        }
        
        /// <summary>Goto label if the value1 is less than value2</summary>
        public static ILGenerator IfLessGoto(this ILGenerator il, Label label, bool unsigned = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (unsigned)
                il.Emit(OpCodes.Blt_Un, label);
            else
                il.Emit(OpCodes.Blt, label);
            return il;
        }

        /// <summary>Goto label if the value1 is less than or equal to value2</summary>
        public static ILGenerator IfLessOrEqualGoto(this ILGenerator il, Label label, bool unsigned = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (unsigned)
                il.Emit(OpCodes.Ble_Un, label);
            else
                il.Emit(OpCodes.Ble, label);
            return il;
        }
        
        /// <summary>Leave a protected region of code</summary>
        public static ILGenerator Leave(this ILGenerator il, Label label)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            il.Emit(OpCodes.Leave, label);
            return il;
        }

        public static ILGenerator Call<T>(this ILGenerator il, string method, bool tailCall = false)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            return il.Call(typeof(T).GetTypeInfo().GetDeclaredMethod(method), tailCall);
        }

        public static ILGenerator Call(this ILGenerator il, MethodInfo method, bool tailCall = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (tailCall)
                il.Emit(OpCodes.Tailcall, method);

            il.Emit(OpCodes.Call, method);
            return il;
        }

        public static ILGenerator Call(this ILGenerator il, ConstructorInfo ctor)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (ctor == null)
                throw new ArgumentNullException(nameof(ctor));

            il.Emit(OpCodes.Call, ctor);
            return il;
        }

        public static ILGenerator GetPropertyValue<T>(this ILGenerator il, string propName)
        {
            if (propName == null)
                throw new ArgumentNullException(nameof(propName));

            return il.CallGetProperty(typeof(T).GetTypeInfo().GetDeclaredProperty(propName));
        }

        public static ILGenerator CallGetProperty(this ILGenerator il, PropertyInfo prop)
        {
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            il.CallVirt(prop.GetMethod);
            return il;
        }

        public static ILGenerator CallVirt<T>(this ILGenerator il, string method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            return il.CallVirt(typeof(T).GetTypeInfo().GetDeclaredMethod(method));
        }

        /// <summary>
        /// Call an instance method of a class, or interface.
        /// </summary>
        /// <param name="constrainedType">Used for allowing virtual method calls on generic types which *might* be a value type</param>
        public static ILGenerator CallVirt(this ILGenerator il, MethodInfo method, bool tailCall = false, Type constrainedType = null)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (constrainedType != null)
                il.Emit(OpCodes.Constrained, constrainedType);

            if (tailCall)
                il.Emit(OpCodes.Tailcall, method);

            il.Emit(OpCodes.Callvirt, method);
            return il;
        }

        /// <summary>Cast a reference type.</summary>
        public static ILGenerator Cast(this ILGenerator il, Type toType)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (toType == null)
                throw new ArgumentNullException(nameof(toType));

            il.Emit(OpCodes.Castclass, toType);
            return il;
        }

        /// <summary>
        /// Creates a new instance of a reference type via the supplied <paramref name="constructor"/>.
        /// Does *not* work for value types.
        /// </summary>
        public static ILGenerator New(this ILGenerator il, ConstructorInfo constructor)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));

            il.Emit(OpCodes.Newobj, constructor);
            return il;
        }

        public static ILGenerator LoadAddress(this ILGenerator il, LocalBuilder local)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (local == null)
                throw new ArgumentNullException(nameof(local));

            if (local.LocalIndex < 256)
                il.Emit(OpCodes.Ldloca_S, (byte)local.LocalIndex); // short form, more compact IL
            else
                il.Emit(OpCodes.Ldloca, local.LocalIndex);
            return il;
        }

        public static ILGenerator LoadAddress(this ILGenerator il, FieldBuilder f)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (f == null)
                throw new ArgumentNullException(nameof(f));

            il.Emit(OpCodes.Ldflda, f);
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, string str)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Ldstr, str);
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, long i)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Ldc_I8, i);
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, float i)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Ldc_R4, i);
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, double i)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Ldc_R8, i);
            return il;
        }

        /// <summary>Loads an integer onto the stack.  Uses optimized code for i between 0 and 8</summary>
        public static ILGenerator Load(this ILGenerator il, int i)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            // use short form of instruction for -1 >= i <= 8
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
                    il.Emit(OpCodes.Ldc_I4, i); // long form of instruction
                    break;
            }
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            il.Emit(OpCodes.Ldtoken, type);
            il.Call(typeof(Type).GetTypeInfo().GetDeclaredMethod(nameof(Type.GetTypeFromHandle)));
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, LocalBuilder local)
        {
            if (local == null)
                throw new ArgumentNullException(nameof(local));
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Ldloc, local);
            return il;
        }

        /// <summary>Loads a static or non-static field onto the stack</summary>
        public static ILGenerator Load(this ILGenerator il, FieldBuilder field, bool @volatile = false, bool unaligned = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (unaligned && field.IsStatic)
                throw new ArgumentException("Static fields cannot be un-aligned");

            if (@volatile)
                il.Emit(OpCodes.Volatile);
            if (unaligned)
                il.Emit(OpCodes.Unaligned);

            if (field.IsStatic)
                il.Emit(OpCodes.Ldsfld, field);
            else
                il.Emit(OpCodes.Ldfld, field);
            return il;
        }

        /// <summary>Gets a value stored in an array.  Push 1) the array and 2) the index to the stack</summary>
        public static ILGenerator LoadElement<T>(this ILGenerator il) => LoadElement(il, typeof(T));

        /// <summary>Gets a value stored in an array.  Push 1) the array and 2) the index to the stack</summary>
        public static ILGenerator LoadElement(this ILGenerator il, Type type)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // use the short form for native structs
            if (type == typeof(sbyte))
                il.Emit(OpCodes.Ldelem_I1);
            else if (type == typeof(short))
                il.Emit(OpCodes.Ldelem_I2);
            else if (type == typeof(int))
                il.Emit(OpCodes.Ldelem_I4);
            else if (type == typeof(long))
                il.Emit(OpCodes.Ldelem_I8);
            else if (type == typeof(byte))
                il.Emit(OpCodes.Ldelem_U1);
            else if (type == typeof(ushort))
                il.Emit(OpCodes.Ldelem_U2);
            else if (type == typeof(uint))
                il.Emit(OpCodes.Ldelem_U4);
            else if (type == typeof(float))
                il.Emit(OpCodes.Ldelem_R4);
            else if (type == typeof(double))
                il.Emit(OpCodes.Ldelem_R8);
            else if (type.IsValueType)
                il.Emit(OpCodes.Ldelem, type); // use long form for value types
            else 
                il.Emit(OpCodes.Ldelem_Ref, type); // use long form for reference types
            return il;
        }

        /// <summary>
        /// Loads the address of a array element (as a managed pointer).  Requires 1) the array and 2) the index to be pushed to the stack.
        /// </summary>
        /// <param name="type">The type of element</param>
        /// <param name="readonly">Means the returned managed pointer cannot be changed, and run-time type checks are bypassed</param>
        public static ILGenerator LoadElementAddress(this ILGenerator il, Type type, bool @readonly = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (@readonly)
                il.Emit(OpCodes.Readonly);

            il.Emit(OpCodes.Ldelema, type);
            return il;
        }

        /// <summary>Loads null onto the stack</summary>
        public static ILGenerator Null(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Ldnull);
            return il;
        }

        /// <summary>Tries to cast the value on the stack to a type, result is the cast value or null</summary>
        public static ILGenerator AsType(this ILGenerator il, Type type)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            il.Emit(OpCodes.Isinst, type);
            return il;
        }

        public static ILGenerator Store(this ILGenerator il, LocalBuilder local)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (local == null)
                throw new ArgumentNullException(nameof(local));
            il.Emit(OpCodes.Stloc, local);
            return il;
        }

        /// <summary>Store the value on the top of the stack in argument slot <paramref name="argIdx"/></summary>
        public static ILGenerator StoreArg(this ILGenerator il, short argIdx)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (argIdx <= 256)
                il.Emit(OpCodes.Starg_S, argIdx); // short form, smaller IL
            else
                il.Emit(OpCodes.Starg, argIdx);
            return il;
        }

        /// <summary>Sets an instance or static field with the value on top of the stack</summary>
        public static ILGenerator Store(this ILGenerator il, FieldBuilder field, bool @volatile = false, bool unaligned = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (unaligned && field.IsStatic)
                throw new ArgumentException("Static fields cannot be un-aligned");

            if (@volatile)
                il.Emit(OpCodes.Volatile);
            if (unaligned)
                il.Emit(OpCodes.Unaligned);

            if (field.IsStatic)
                il.Emit(OpCodes.Stsfld, field);
            else
                il.Emit(OpCodes.Stfld, field);
            return il;
        }

        /// <summary>
        /// Sets the value of an array at a specific index.
        /// Push 1) array reference, 2) array index, 3) the value to store
        /// </summary>
        public static ILGenerator StoreElement<T>(this ILGenerator il) => StoreElement(il, typeof(T));

        /// <summary>
        /// Sets the value of an array at a specific index.
        /// Push 1) array reference, 2) array index, 3) the value to store
        /// </summary>
        public static ILGenerator StoreElement(this ILGenerator il, Type type)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // use the short form for native structs
            if (type == typeof(sbyte))
                il.Emit(OpCodes.Stelem_I1);
            else if (type == typeof(short))
                il.Emit(OpCodes.Stelem_I2);
            else if (type == typeof(int))
                il.Emit(OpCodes.Stelem_I4);
            else if (type == typeof(long))
                il.Emit(OpCodes.Stelem_I8);
            else if (type == typeof(float))
                il.Emit(OpCodes.Stelem_R4);
            else if (type == typeof(double))
                il.Emit(OpCodes.Stelem_R8);
            else if (type.IsValueType)
                il.Emit(OpCodes.Stelem, type); // use long form for value types
            else
                il.Emit(OpCodes.Stelem_Ref, type); // use long form for reference types
            return il;
        }

        public static ILGenerator Return(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Ret);
            return il;
        }

        public static ILGenerator Return(this ILGenerator il, bool val)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(val ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);
            return il;
        }

        public static ILGenerator Switch(this ILGenerator il, Label[] lables)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (lables == null)
                throw new ArgumentNullException(nameof(lables));

            il.Emit(OpCodes.Switch, lables);
            return il;
        }

        /// <summary>Converts the numeric value on the top of the stack to a different numeric type</summary>
        public static ILGenerator Convert(this ILGenerator il, Type toType, bool detectOverflow = false, bool unsignedValue = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (toType == typeof(sbyte))
            {
                if (detectOverflow && unsignedValue)
                    il.Emit(OpCodes.Conv_Ovf_I1_Un);
                else if (detectOverflow)
                    il.Emit(OpCodes.Conv_Ovf_I1);
                else
                    il.Emit(OpCodes.Conv_I1);
            }
            else if (toType == typeof(short))
            {
                if (detectOverflow && unsignedValue)
                    il.Emit(OpCodes.Conv_Ovf_I2_Un);
                else if (detectOverflow)
                    il.Emit(OpCodes.Conv_Ovf_I2);
                else
                    il.Emit(OpCodes.Conv_I2);
            }
            else if (toType == typeof(int))
            {
                if (detectOverflow && unsignedValue)
                    il.Emit(OpCodes.Conv_Ovf_I4_Un);
                else if (detectOverflow)
                    il.Emit(OpCodes.Conv_Ovf_I4);
                else
                    il.Emit(OpCodes.Conv_I4);
            }
            else if (toType == typeof(long))
            {
                if (detectOverflow && unsignedValue)
                    il.Emit(OpCodes.Conv_Ovf_I8_Un);
                else if (detectOverflow)
                    il.Emit(OpCodes.Conv_Ovf_I8);
                else
                    il.Emit(OpCodes.Conv_I8);
            }
            else if (toType == typeof(byte))
            {
                if (detectOverflow && unsignedValue)
                    il.Emit(OpCodes.Conv_Ovf_U1_Un);
                else if (detectOverflow)
                    il.Emit(OpCodes.Conv_Ovf_U1);
                else
                    il.Emit(OpCodes.Conv_U1);
            }
            else if (toType == typeof(ushort))
            {
                if (detectOverflow && unsignedValue)
                    il.Emit(OpCodes.Conv_Ovf_U2_Un);
                else if (detectOverflow)
                    il.Emit(OpCodes.Conv_Ovf_U2);
                else
                    il.Emit(OpCodes.Conv_U2);
            }
            else if (toType == typeof(uint))
            {
                if (detectOverflow && unsignedValue)
                    il.Emit(OpCodes.Conv_Ovf_U4_Un);
                else if (detectOverflow)
                    il.Emit(OpCodes.Conv_Ovf_U4);
                else
                    il.Emit(OpCodes.Conv_U4);
            }
            else if (toType == typeof(ulong))
            {
                if (detectOverflow && unsignedValue)
                    il.Emit(OpCodes.Conv_Ovf_U8_Un);
                else if (detectOverflow)
                    il.Emit(OpCodes.Conv_Ovf_U8);
                else
                    il.Emit(OpCodes.Conv_U8);
            }
            else if (toType == typeof(float))
            {
                il.Emit(OpCodes.Conv_R4);
            }
            else if (toType == typeof(double))
            {
                il.Emit(OpCodes.Conv_R8);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(toType), "Only primitive number types are supported");
            }
            return il;
        }

        /// <summary>Duplicates the value on the top of the stack</summary>
        public static ILGenerator Duplicate(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Dup);
            return il;
        }

        /// <summary>
        /// Sets a block of memory to a value.  Requires an address, byte value to set, and number of bytes to set (int32) to be pushed to the stack
        /// </summary>
        public static ILGenerator InitBlock(this ILGenerator il, bool @volatile = false, bool unaligned = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (@volatile)
                il.Emit(OpCodes.Volatile);
            if (unaligned)
                il.Emit(OpCodes.Unaligned);

            il.Emit(OpCodes.Initblk);
            return il;
        }

        /// <summary>
        /// Copies a block of memory.  Requires an destination address, source address, and number of bytes to copy (int32) to be pushed to the stack
        /// </summary>
        public static ILGenerator CopyBlock(this ILGenerator il, bool @volatile = false, bool unaligned = false)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            if (@volatile)
                il.Emit(OpCodes.Volatile);
            if (unaligned)
                il.Emit(OpCodes.Unaligned);

            il.Emit(OpCodes.Cpblk);
            return il;
        }

        /// <summary>Pushed a (unmanaged) pointer to the argument list of the current method</summary>
        public static ILGenerator ArgList(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));
            il.Emit(OpCodes.Arglist);
            return il;
        }

        public static ILGenerator Throw(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            il.Emit(OpCodes.Throw);
            return il;
        }      

        public static ILGenerator Rethrow(this ILGenerator il)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            il.Emit(OpCodes.Rethrow);
            return il;
        }      

        public static ILGenerator SizeOf(this ILGenerator il, Type type)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            il.Emit(OpCodes.Sizeof, type);
            return il;
        }

        /// <summary>
        /// Transfers control to <paramref name="method"/>, including all the arguments of the current method, which must match the destination method argument list.
        /// </summary>
        public static ILGenerator Jump(this ILGenerator il, MethodInfo method)
        {
            if (il == null)
                throw new ArgumentNullException(nameof(il));

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            il.Emit(OpCodes.Jmp, method);
            return il;
        }
    }
}