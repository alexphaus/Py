using System;
using System.Collections.Generic;
using Exp = System.Linq.Expressions.Expression;
using System.Reflection;

namespace Py
{
    partial class Py
    {
        static Dictionary<string, object> dcl = new Dictionary<string, object>();

        Type cl_type(string s)
        {
            return Type.GetType(s);
        }

        object cl_exp(string s)
        {
            if (dcl.TryGetValue(s, out object cl))
                return cl;

            else if (Local != null && Local.TryGetValue(s, out Exp local))
                return local;
            else
                return GlobalAccess(s);
        }

        void Emit(string s, List<Exp> IL)
        {
            string[] args = s.Split();
            string key = args[0];
            string cmd = args[1];
            int pcnt = args.Length - 2;
            Type[] types = new Type[pcnt];
            object[] parameters = new object[pcnt];

            for (int i = 0; i < pcnt; i++)
            {
                string arg = args[i + 2];
                
                if (arg.StartsWith('%')) // string
                {
                    parameters[i] = arg.Substring(1);
                }
                else if (arg.StartsWith('$'))
                {

                }
                else if (arg.StartsWith('/')) // Conversion
                {
                    parameters[i] = Exp.New(typeof(Dynamic).GetConstructor(new[] { typeof(object) }), (Exp)cl_exp(arg));
                }
                else if (arg.StartsWith('?')) // Constant
                {
                    arg = arg.Substring(1);

                    if (int.TryParse(arg, out int n))
                        parameters[i] = Exp.Constant(n);

                    else if (double.TryParse(arg, out double f))
                        parameters[i] = Exp.Constant(f);

                    switch (arg)
                    {
                        case "true":
                            parameters[i] = Exp.Constant(true);
                            break;

                        case "false":
                            parameters[i] = Exp.Constant(false);
                            break;

                        case "null":
                            parameters[i] = Exp.Constant(null);
                            break;

                        default:
                            parameters[i] = Exp.Constant(arg);
                            break;
                    }
                }
                else if (arg.StartsWith('@')) // Type
                {
                    parameters[i] = cl_type(arg.Substring(1));
                }
                else if (arg.StartsWith('*')) // MethodInfo
                {
                    string[] toks = arg.Substring(1).Split(',');
                    Type type = cl_type(toks[0]);
                    string name = toks[1];
                    Type[] parameterTypes = new Type[toks.Length - 2];
                    
                    for (int j = 0; j < parameterTypes.Length; j++)
                        parameterTypes[j] = cl_type(toks[j + 2]);

                    if (name == ".ctor")
                        parameters[i] = type.GetConstructor(parameterTypes);
                    else
                        parameters[i] = type.GetMethod(name, parameterTypes);
                }
                else if (arg.StartsWith('(')) // Convert
                {
                    
                }
                else if (arg.StartsWith('[')) // Exp[]
                {
                    var toks = arg.Substring(1, arg.Length - 2).Split(',');
                    var arr = new Exp[toks.Length];
                    for (int j = 0; j < arr.Length; j++)
                        arr[j] = (Exp)cl_exp(toks[j]);
                    parameters[i] = arr;
                }
                else if (arg.StartsWith("@[")) // Type[]
                {
                    var toks = arg.Substring(2, arg.Length - 3).Split(',');
                    var arr = new Type[toks.Length];
                    for (int j = 0; j < arr.Length; j++)
                        arr[j] = cl_type(toks[j]);
                    parameters[i] = arr;
                }
                else // Exp & data
                {
                    parameters[i] = cl_exp(arg);
                }
            }

            object r = null;

            switch (cmd)
            {
                case "ret":
                    r = Exp.Return(ret.Peek(), (Exp)parameters[0]);
                    break;

                default:
                    for (int i = 0; i < parameters.Length; i++)
                        types[i] = parameters[i].GetType();

                    r = typeof(Exp).GetMethod(cmd,
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase,
                        null, types, null).Invoke(null, parameters);
                    break;
            }
            
            if (key == "+")
                IL.Add((Exp)r);
            else
                dcl[key] = r;
        }
    }
}