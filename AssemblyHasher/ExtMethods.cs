using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyHasher
{
    public static class ExtMethods
    {
        public static T ResolveArgument<T>(this List<string> arguments, string key, T defaultValue) where T : IConvertible
        {
            if (arguments.Any(arg => arg.Contains(key)))
            {
                var result = defaultValue;
                var value = arguments.Where(arg => arg.Contains(key)).FirstOrDefault();
                if (value.Contains(':'))
                    result = (T)Convert.ChangeType(value.Replace(key+":", "").Trim(), typeof(T));

                arguments.RemoveAll(arg => arg.Contains(key));
                return result;
            }

            return defaultValue;
        }

        public static T ResolveArgument<T>(this List<string> arguments, string key, T notFoundValue, T foundValue) where T : IConvertible
        {
            var result = arguments.Any(arg => arg.Contains(key)) ? foundValue : notFoundValue;
            arguments.RemoveAll(arg => arg.Contains(key));
            return result;
        }
    }
}
