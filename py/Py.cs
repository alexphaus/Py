using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Exp = System.Linq.Expressions.Expression;
using ParameterExpression = System.Linq.Expressions.ParameterExpression;

namespace Py
{
    public class LocalBuilder : Dictionary<string, Exp>
    {
        public List<ParameterExpression> Variables = new List<ParameterExpression>();

        public void Add(ParameterExpression var)
        {
            Variables.Add(var);
            Add(var.Name, var);
        }
    }

    public partial class Py
    {
        public static Dictionary<string, Object> Global = new Dictionary<string, Object>();
        LocalBuilder Local;

        /* control flow */
        Stack<LabelTarget> ret = new Stack<LabelTarget>();
        Stack<LabelTarget> con = new Stack<LabelTarget>();
        Stack<LabelTarget> br = new Stack<LabelTarget>();

        /* useful constants */
        public readonly static Object None = new NoneType();
        public readonly static Object True = new Bool(true);
        public readonly static Object False = new Bool(false);

        /// <summary>
        /// Alias for typeof(Object)
        /// </summary>
        public readonly static Type Any = typeof(Object);
        public readonly static MethodInfo Callvirt = Any.GetMethod("Callvirt");
        public readonly static MethodInfo __bool__ = Any.GetMethod("__bool__");

        public readonly static Exp NullExp = Exp.Constant(null, Any);
        public readonly static Exp NoneExp = Exp.Constant(None, Any);
        public readonly static Exp TrueExp = Exp.Constant(True, Any);
        public readonly static Exp FalseExp = Exp.Constant(False, Any);

        static readonly Exp _G = Exp.Constant(Global);

        static void Main(string[] args)
        {
            var w = new Stopwatch();
            w.Start();

            try
            {
                string file_path = args[0];
                string src = System.IO.File.ReadAllText(file_path);

                var interpreter = new Py();
                interpreter.Execute(src);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR " + ex.ToString());
            }

            w.Stop();
            Console.WriteLine("~" + w.Elapsed.TotalSeconds);
        }

        public void Execute(string src)
        {
            List<Token> tokens = Tokenize(src);
            List<Expression> body = Struct(tokens);
            var lmb = Exp.Lambda<Action>(ParseBlock(body));
            var del = lmb.Compile();
            del.Invoke();
        }

        Exp ArrayIndex(Exp collection, Exp index)
        {
            return Exp.ArrayAccess(collection, index);
        }

        Exp GlobalAccess(string name)
        {
            return Exp.Property(_G, "Item", Exp.Constant(name));
        }

        Exp AsBool(Exp expr)
        {
            return Exp.Call(expr, __bool__);
        }

        /* 1-arg callvirt */
        static Args _arg1 = new Args
        {
            Input = new Object[1],
            Info = new[] { new ArgInfo() },
            ord = 1
        };
        static Exp arg1 = Exp.Constant(_arg1);

        Exp Call(Exp obj, string name, Exp arg0)
        {
            return Exp.Call(obj, Callvirt, Exp.Constant(name),
                Exp.Block
                (
                    Exp.Assign(Exp.Field(arg1, "self"), obj),
                    Exp.Assign(Exp.ArrayAccess(Exp.Field(arg1, "Input"), Exp.Constant(0)), arg0),
                    arg1
                ));
        }

        Exp GetItem(Exp collection, Exp key)
        {
            return Call(collection, "__getitem__", key);
        }

        Exp SetItem(Exp collection, Exp key, Exp value)
        {
            return null;
        }

        bool HasAssign(List<Token> tokens)
        {
            foreach (Token tok in tokens)
                if (tok.Type == TokenType.Assign)
                    return true;

            return false;
        }

        Exp Assign(List<Token> expr, Exp value)
        {
            if (expr.Count == 1)
            {
                Token tok = expr[0];
                if (tok.Type == TokenType.Identifier)
                {
                    if (Local != null)
                    {
                        if (Local.TryGetValue(tok.Value, out Exp id))
                            return Exp.Assign(id, value);
                        else
                        {
                            var var = Exp.Variable(typeof(Object), tok.Value);
                            Local.Add(var);
                            return Exp.Assign(var, value);
                        }
                        /* if 'name' is not present in the 'Local' dictionary defines 
                         * new local variable instead of accessing 'Global' (by default) 
                         */
                    }
                    else
                        return Exp.Assign(GlobalAccess(tok.Value), value);
                }
            }
            else
            {
                Token tok = expr[^1]; // last
                if (tok.Type == TokenType.Member)
                {
                    expr.RemoveAt(expr.Count - 1);
                    return Exp.Call(Parse(expr), typeof(Object).GetMethod("__setattr__"), Exp.Constant(tok.Value), value);
                }
            }
            throw new Exception("the left hand of the expression is not assignable");
        }

        Exp Cache(string name)
        {
            if (Local is null)
            {
                return GlobalAccess(name);
            }
            else if (Local.TryGetValue(name, out Exp c))
            {
                return c;
            }
            else
            {
                var id = Exp.Variable(typeof(Object), name);
                Local.Add(id);
                return id;
            }
        }

        static int[] pre = {
            0, // none
            1, // **
            2, 2, 2, 2, // *, /, //, %
            3, 3, 3, // ~, +, -
            4, 4, // <<, >>
            5, 6, 7, // &, ^, |
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, // ==, !=, <, <=, >, >=, is, is not, in, not in
            9, 10, 11 // not, and, or
        };

        static string[] OpMeth = {
            null,
            "__pow__", //Exp.Constant("__pow__"),
            "__mul__", //Exp.Constant("__mul__"),
            "__div__", //Exp.Constant("__div__"),
            "__floordiv__", //Exp.Constant("__floordiv__"),
            "__mod__", //Exp.Constant("__mod__"),
            "__invert__", //Exp.Constant("__invert__"),
            "__add__",
            "__sub__", //Exp.Constant("__sub__"),
            "__lshift__", //Exp.Constant("__lshift__"),
            "__rshift__", //Exp.Constant("__rshift__"),
            "__and__", //Exp.Constant("__and__"),
            "__xor__", //Exp.Constant("__xor__"),
            "__or__", //Exp.Constant("__or__"),
            "__eq__", //Exp.Constant("__eq__"),
            "__ne__", //Exp.Constant("__ne__"),
            "__lt__",
            "__le__", //Exp.Constant("__le__"),
            "__gt__", //Exp.Constant("__gt__"),
            "__ge__", //Exp.Constant("__ge__"),
            null,
            null,
            null,
            null,
            null,
            null,
            null
        };
    }
}