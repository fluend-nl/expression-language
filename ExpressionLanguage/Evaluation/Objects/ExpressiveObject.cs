using System;
using System.Linq;
using System.Reflection;

namespace Fluend.ExpressionLanguage.Evaluation.Objects
{
    public class ExpressiveObject
    {
        /// <summary>
        /// Try to get the value of the property with the given name.
        /// The property on the object has to be decorated with the
        /// 'Expressive' attribute.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="MissingFieldException"></exception>
        public object? GetPropertyValue(string name)
        {
            return GetProperty(name).GetValue(this);
        }

        /// <summary>
        /// Try to get the type of the property with the given
        /// name. The property has to be decorated with the
        /// 'Expressive' attribute.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Type GetPropertyType(string name)
        {
            return GetProperty(name).PropertyType;
        }
        
        private PropertyInfo GetProperty(string name)
        {
            var property = GetType().GetProperty(name);

            if (null == property || !Attribute.IsDefined(property, typeof(ExpressiveAttribute)))
            {
                throw new MissingFieldException($"No property with the name '{name}' exists on the object.");
            }

            return property;
        }

        /// <summary>
        /// Try to invoke the method on this object with the given
        /// name. The parameters will be used to deduce the correct
        /// overload. The method has to be decorated with the 'Expressive'
        /// attribute.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="MissingMethodException"></exception>
        public object? InvokeMethod(string name, object?[] parameters)
        {
            var parameterTypes = parameters
                .Select(p => p.GetType())
                .ToArray();
            
            return GetMethod(name, parameterTypes)
                .Invoke(this, parameters);
        }

        /// <summary>
        /// Try to get the return type of the  method on
        /// this object with the given name. The parameters
        /// will be used to deduce the correct overload.
        /// The method has to be decorated with the 'Expressive'
        /// attribute.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameterTypes"></param>
        /// <returns></returns>
        /// <exception cref="MissingMethodException"></exception>
        public Type GetMethodReturnType(string name, Type[] parameterTypes)
        {
            return GetMethod(name, parameterTypes)
                .ReturnType;
        }

        private MethodInfo GetMethod(string name, Type[] parameterTypes)
        {
            var method = GetType().GetMethod(name, parameterTypes);

            if (null == method || !Attribute.IsDefined(method, typeof(ExpressiveAttribute)))
            {
                throw new MissingMethodException($"No method with the name '{name}' exists on the object.");
            }

            return method;
        }
    }
}