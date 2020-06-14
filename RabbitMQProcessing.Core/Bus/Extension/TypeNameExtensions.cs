using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitMQProcessing.Core.Bus.Extension
{
    public static class TypeNameExtensions
    {
        public static string GetTypeName(this Type type)
        {
            string name;

            if (!type.IsGenericType) return type.Name;

            var allNames = type.GetGenericArguments().Select(t => t.Name).ToList();
            var allTypes = string.Join(",", allNames);
            name = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{allTypes}>";

            return name;
        }

        public static string GetTypeName(this object @object)
        {
            return @object.GetType().GetTypeName();
        }
    }
}
