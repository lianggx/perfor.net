using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;

namespace Perfor.Lib.Reflection
{
    /// <summary>
    ///  动态对象管理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicManager<T>
    {
        #region Identity
        private Type returnType = null;
        private readonly MethodInfo getValue = typeof(IDataRecord).GetMethod("get_Item", new Type[] { typeof(int) });
        private readonly MethodInfo isDbNull = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
        private delegate T CreateHandler(IDataRecord record);
        private CreateHandler OnCreateLoad;

        public DynamicManager()
        {
            returnType = typeof(T);
        }
        #endregion

        /// <summary>
        ///  调用委托，创建真实对象
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public T CreateObject(IDataRecord record)
        {
            return OnCreateLoad(record);
        }

        /// <summary>
        ///  初始化 IDataRecord 对象到动态对象的映射
        /// </summary>
        /// <param name="record"></param>
        public void InitDynamicObject(IDataRecord record)
        {
            DynamicMethod mainMethod = new DynamicMethod("CreateDynamicObject", returnType, new Type[] { typeof(IDataRecord) }, returnType, true);
            ILGenerator generator = mainMethod.GetILGenerator();
            LocalBuilder result = generator.DeclareLocal(returnType);
            generator.Emit(OpCodes.Newobj, returnType.GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);

            for (int i = 0; i < record.FieldCount; i++)
            {
                string fieldName = record.GetName(i);
                PropertyInfo pi = returnType.GetProperty(fieldName);
                if (pi == null) continue;
                MethodInfo setMethod = pi.GetSetMethod();
                // 不存在该属性或者属性无法通过set进行赋值
                if (setMethod == null) continue;

                // 进行数据有效性检测
                Label endMark = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Callvirt, isDbNull);
                generator.Emit(OpCodes.Brtrue, endMark);

                // 开始赋值操作
                generator.Emit(OpCodes.Ldloc, result);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Callvirt, getValue);
                generator.Emit(OpCodes.Unbox_Any, record.GetFieldType(i));
                generator.Emit(OpCodes.Callvirt, setMethod);

                generator.MarkLabel(endMark);
            }
            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);

            OnCreateLoad = (CreateHandler)mainMethod.CreateDelegate(typeof(CreateHandler));
        }
    }
}
