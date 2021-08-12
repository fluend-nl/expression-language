using System.Collections.Generic;
using System.Linq;

namespace Fluend.ExpressionLanguage.Evaluation.Functions
{
    public class ExpressiveFunctionSet
    {
        private readonly Dictionary<string, List<ExpressiveFunction>> _functions = new();

        public ExpressiveFunctionSet()
        {
        }

        public ExpressiveFunctionSet(params ExpressiveFunction[] functions)
        {
            foreach (var function in functions)
            {
                Add(function);
            }
        }
        
        /// <summary>
        /// Add a function to the function set.
        /// </summary>
        /// <param name="function"></param>
        public void Add(ExpressiveFunction function)
        {
            if (!Has(function.Name))
            {
                _functions[function.Name] = new List<ExpressiveFunction>();
            }

            _functions[function.Name].Add(function);
        }

        /// <summary>
        /// Retrieve a list of all functions with the given name.
        /// If no function with that name is registered, an empty
        /// list is returned. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<ExpressiveFunction> Get(string name)
        {
            if (!Has(name))
            {
                return new();
            }

            return _functions[name];
        }
        
        /// <summary>
        /// Get the function with the given name and signature.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public ExpressiveFunction? Get(string name, Signature signature)
        {
            if (!Has(name))
            {
                return null;
            }
            
            return _functions[name]
                .Find(f => f.Signature == signature);
        }

        /// <summary>
        /// Check whether there are any functions registered
        /// under the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Has(string name)
        {
            return _functions.ContainsKey(name);
        }

        /// <summary>
        /// Check whether a function with the given name
        /// and signature is defined.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public bool Has(string name, Signature signature)
        {
            return null != Get(name, signature);
        }

        /// <summary>
        /// Remove the given function.
        /// </summary>
        /// <param name="function"></param>
        public void Remove(ExpressiveFunction function)
        {
            if (!Has(function.Name))
            {
                return;
            }
            
            _functions[function.Name].Remove(function);

            if (0 == _functions[function.Name].Count)
            {
                _functions.Remove(function.Name);
            }
        }
        
        /// <summary>
        /// Remove all functions with the given name.
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            _functions.Remove(name);
        }

        /// <summary>
        /// Remove the function with the given name
        /// and signature.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="signature"></param>
        public void Remove(string name, Signature signature)
        {
            var function = Get(name, signature);
            
            if (null != function)
            {
                Remove(function);   
            }
        }

        /// <summary>
        /// Get the names of all defined functions.
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetFunctionNames()
        {
            return _functions.Keys;
        }

        /// <summary>
        /// Merge the given function set into
        /// this one.
        /// </summary>
        /// <param name="other"></param>
        public void Merge(ExpressiveFunctionSet other)
        {
            foreach (var (name, overloads) in other._functions)
            {
                if (Has(name))
                {
                    _functions[name].AddRange(overloads
                        .Where(overload => !Has(name, overload.Signature)));
                }
                else
                {
                    _functions[name] = overloads;
                }
            }
        }
    }
}