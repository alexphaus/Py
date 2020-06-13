using System;
using System.Linq;
using System.Collections.Generic;
using Exp = System.Linq.Expressions.Expression;

namespace Py
{
    public enum ArgType
    {
        Default,
        Assignment,
        Args, // *args
        Kwargs // **kwargs
    }

    public class ArgInfo
    {
        public ArgType Type;
        public String Key;
    }

    public class Args
    {
        public Object self;
        public Object[] Input; // tmp
        public ArgInfo[] Info;
        public int ord;

        public Object this[int index]
        {
            get
            {
                return Input[index];
            }
        }

        public void Sort(Params ps)
        {
            if (ord == ps.ord)
                return;
            /* if both args and params count are only-positional equal,
               return the input arguments with no filtering */

            /* reg all input data */

            var args = new Tuple();
            var kwargs = new Dict();

            ArgInfo argInfo;

            for (int i = 0; i < Info.Length; i++)
            {
                argInfo = Info[i];

                switch (argInfo.Type)
                {
                    case ArgType.Default:
                        args.tuple.Add(Input[i]);
                        break;

                    case ArgType.Assignment:
                        kwargs.dict.Add(argInfo.Key, Input[i]);
                        break;

                    case ArgType.Args: // unpack list
                        var iterable = ((List)Input[i]).list;
                        foreach (Object elmnt in iterable)
                            args.tuple.Add(elmnt);
                        break;

                    case ArgType.Kwargs: // unpack dict
                        var dict = ((Dict)Input[i]).dict;
                        foreach (var pair in dict)
                            kwargs.dict.Add(pair.Key, pair.Value);
                        break;
                }
            }

            // sort data

            Object[] r = new Object[ps.Info.Length];

            ParamInfo pi;

            for (int i = 0; i < r.Length; i++)
            {
                pi = ps.Info[i];

                switch (pi.Type)
                {
                    case ArgType.Args:
                        r[i] = args;
                        break;

                    case ArgType.Kwargs:
                        r[i] = kwargs;
                        break;

                    default:
                        if (args.tuple.Count > 0)
                        {
                            r[i] = args.tuple[0];
                            args.tuple.RemoveAt(0);
                        }
                        else if (kwargs.dict.TryGetValue(pi.Name, out Object value))
                        {
                            r[i] = value;
                            kwargs.dict.Remove(pi.Name);
                        }
                        else if (pi.Type == ArgType.Assignment)
                        {
                            r[i] = pi.DefaultValue;
                        }
                        break;
                }
            }

            Input = r;
        }

        public Args Shift()
        {
            return new Args
            {
                Input = Input.Skip(1).ToArray(),
                Info = Info.Skip(1).ToArray(),
                ord = ord > 0 ? ord - 1 : ord
            };
        }

        /* 0-arg */

        static Args _arg0 = new Args
        {
            Input = new Object[0],
            Info = new ArgInfo[0],
            ord = 0
        };

        public static Args Empty { get => _arg0; }

        /* 1-arg */

        static Args _arg1 = new Args
        {
            Input = new Object[1],
            Info = new[] { new ArgInfo() },
            ord = 1
        };

        public static Args Create(Object arg0)
        {
            _arg1.Input[0] = arg0;
            return _arg1;
        }

        /* 2-arg */

        static Args _arg2 = new Args
        {
            Input = new Object[2],
            Info = new[] { new ArgInfo(), new ArgInfo() },
            ord = 2
        };

        public static Args Create(Object arg0, Object arg1)
        {
            _arg2.Input[0] = arg0;
            _arg2.Input[1] = arg1;
            return _arg2;
        }
    }

    public class ParamInfo
    {
        public ArgType Type;
        public String Name;
        public Object DefaultValue;
    }

    public class Params
    {
        public ParamInfo[] Info;
        public int ord;
    }

    partial class Py
    {
        const int UNPARAM = -1;
        const int UNARG = -2;

        Params ParseParameters(List<Token> tokens, bool omitFirstParameter = false)
        {
            var w = Split(tokens, TokenType.Comma);

            if (omitFirstParameter)
                w.RemoveAt(0);

            var ps = new Params();
            ps.Info = new ParamInfo[w.Count];
            ps.ord = w.Count;

            List<Token> v;

            for (int i = 0; i < w.Count; i++)
            {
                v = w[i];
                var pi = new ParamInfo();

                if (Contains(v, TokenType.Assign)) // optional parameter
                {
                    pi.Type = ArgType.Assignment;
                    pi.Name = new String(v[0].Value);
                    v.RemoveRange(0, 2);
                    //pi.DefaultValue = ((System.Linq.Expressions.ConstantExpression)Parse(v)).Value;
                    ps.ord = UNPARAM;
                }
                else if (v[0].Type == TokenType.Operator && v[0].op == Op.Multiply) // *args
                {
                    if (v[1].Type == TokenType.Operator && v[1].op == Op.Multiply) // **kwargs
                    {
                        pi.Type = ArgType.Kwargs;
                        pi.Name = new String(v[2].Value);
                    }
                    else
                    {
                        pi.Type = ArgType.Args;
                        pi.Name = new String(v[1].Value);
                    }
                    ps.ord = UNPARAM;
                }
                else
                {
                    pi.Type = ArgType.Default;
                    pi.Name = new String(v[0].Value);
                }

                ps.Info[i] = pi;
            }

            return ps;
        }

        Exp ParseArguments(Exp self, List<Token> tokens)
        {
            var w = Split(tokens, TokenType.Comma);
            List<Token> v;

            if (w.Count == 0)
                return Exp.Constant(Args.Empty);

            var arg = new Args {
                Input = new Object[w.Count],
                Info = new ArgInfo[w.Count],
                ord = w.Count
            };

            Exp arg_ = Exp.Constant(arg);
            Exp input = Exp.Field(arg_, "Input");

            var setter = new List<Exp>();

            for (int i = 0; i < w.Count; i++)
            {
                v = w[i];
                var argInfo = new ArgInfo();

                if (Contains(v, TokenType.Assign))
                {
                    argInfo.Type = ArgType.Assignment;
                    argInfo.Key = new String(v[0].Value);
                    v.RemoveRange(0, 2);
                    arg.ord = UNARG;
                }
                else if (v[0].Type == TokenType.Operator && v[0].op == Op.Multiply) // *args
                {
                    if (v[1].Type == TokenType.Operator && v[1].op == Op.Multiply) // **kwargs
                    {
                        argInfo.Type = ArgType.Kwargs;
                        v.RemoveRange(0, 2);
                    }
                    else
                    {
                        argInfo.Type = ArgType.Args;
                        v.RemoveAt(0);
                    }
                    arg.ord = UNARG;
                }
                else
                {
                    argInfo.Type = ArgType.Default;
                }

                setter.Add(Exp.Assign(Exp.ArrayAccess(input, Exp.Constant(i)), Parse(v)));
                arg.Info[i] = argInfo;
            }

            setter.Add(arg_);

            return Exp.Block(setter);
        }
    }
}