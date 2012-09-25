﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Reflection;

namespace Glass.Mapper
{
    public static  class Utilities
    {

        /// <summary>
        /// Returns a delegate method that will load a class based on its constuctor
        /// </summary>
        /// <returns></returns>
        public static IDictionary<ConstructorInfo, Delegate> CreateConstructorDelegates(Type type)
        {
            var constructors = type.GetConstructors();

            var dic = new Dictionary<ConstructorInfo, Delegate>();

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                var dynMethod = new DynamicMethod("DM$OBJ_FACTORY_" + type.Name, type, parameters.Select(x => x.ParameterType).ToArray(), type);

                ILGenerator ilGen = dynMethod.GetILGenerator();
                for (int i = 0; i < parameters.Count(); i++)
                {
                    ilGen.Emit(OpCodes.Ldarg, i);
                }

                ilGen.Emit(OpCodes.Newobj, constructor);

                ilGen.Emit(OpCodes.Ret);

                Type genericType = null;
                switch (parameters.Count())
                {
                    case 0:
                        genericType = typeof(Func<>);
                        break;
                    case 1:
                        genericType = typeof(Func<,>);
                        break;
                    case 2:
                        genericType = typeof(Func<,,>);
                        break;
                    case 3:
                        genericType = typeof(Func<,,,>);
                        break;
                    case 4:
                        genericType = typeof(Func<,,,,>);
                        break;
                    default:
                        throw new MapperException("Only supports constructors with  a maximum of 4 parameters");
                }

                var delegateType = genericType.MakeGenericType(parameters.Select(x => x.ParameterType).Concat(new[] { type }).ToArray());


                dic[constructor] = dynMethod.CreateDelegate(delegateType);
            }

            return dic;
        }
    }
}
