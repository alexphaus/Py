using System;
using System.Collections.Generic;
using Exp = System.Linq.Expressions.Expression;
using System.Text;
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

        Exp cl_exp(string s)
        {
            if (dcl.TryGetValue(s, out object cl))
            {
                return (Exp)cl;
            }
            else if (Local != null && Local.TryGetValue(s, out Exp local))
            {
                return local;
            }
            else
                return GlobalAccess(s);
        }

        void PushCL(string s, List<Exp> IL)
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
                char id = arg[0];
                arg = arg.Substring(1);

                switch (id)
                {
                    case ':': // label
                        types[i] = typeof(System.Linq.Expressions.LabelTarget);
                        parameters[i] = dcl[arg];
                        break;

                    case '%': // string
                        types[i] = typeof(string);
                        parameters[i] = arg;
                        break;

                    case '*': // method-info
                        string[] spt = arg.Split(',');
                        string name = spt[1];

                        Type t = cl_type(spt[0]);
                        Type[] types1 = new Type[spt.Length - 2];

                        for (int j = 2; j < spt.Length; j++)
                            types1[j - 2] = cl_type(spt[j]);

                        if (name == ".ctor")
                        {
                            types[i] = typeof(ConstructorInfo);
                            parameters[i] = t.GetConstructor(types1);
                        }
                        else
                        {
                            types[i] = typeof(MethodInfo);
                            parameters[i] = t.GetMethod(name, types1);
                        }
                        break;

                    case '@': // type
                        if (arg[0] == '[')
                        {
                            arg = arg.Substring(1, arg.Length - 2);
                            spt = arg.Split(',');
                            types1 = new Type[spt.Length];
                            for (int j = 0; j < spt.Length; j++)
                                types1[j] = cl_type(spt[j]);
                            types[i] = typeof(Type[]);
                            parameters[i] = types1;
                        }
                        else
                        {
                            types[i] = typeof(Type);
                            parameters[i] = cl_type(arg);
                        }
                        break;

                    case '$': // exp
                        if (arg[0] == '[')
                        {
                            arg = arg.Substring(1, arg.Length - 2);
                            spt = arg.Split(',');
                            Exp[] arr = new Exp[spt.Length];
                            for (int j = 0; j < spt.Length; j++)
                                arr[j] = cl_exp(spt[j]);
                            types[i] = typeof(Exp[]);
                            parameters[i] = arr;
                        }
                        else
                        {
                            types[i] = typeof(Exp);
                            parameters[i] = cl_exp(arg);
                        }
                        break;
                }
            }

            object line = typeof(Exp).GetMethod(
                cmd, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase,
                null, types, null).Invoke(null, parameters);

            if (key == "+")
                IL.Add((Exp)line);
            else
                dcl[key] = line;
        }
    }
}