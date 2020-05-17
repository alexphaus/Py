using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exp = System.Linq.Expressions.Expression;

namespace Py
{
    partial class Py
    {
        /// <summary>
        /// Def block parser
        /// </summary>
        void PushBlock(List<Expression> body, List<Exp> IL)
        {
            Expression expr;
            var ifparts = new Stack<If>();

            for (int i = 0; i < body.Count; i++)
            {
                expr = body[i]; // cur expression

                switch (expr.Command)
                {
                    case "class":
                        {
                            Class type = ParseClass(expr);
                            Global.Add(type.Name, type);
                        }
                        break;

                    case "def":
                        {
                            Function func = ParseFunc(expr);
                            Global.Add(func.Name, func);
                        }
                        break;

                    case "if":
                    case "elif":
                        ifparts.Push(
                            new If
                            {
                                Test = AsBool(Parse(expr)),
                                Body = ParseBlock(expr.Body)
                            });
                        if (i + 1 == body.Count || body[i + 1].Command?.StartsWith("el") == false)
                            IL.Add(IfThenElse(ifparts));
                        break;

                    case "else":
                        IL.Add(IfThenElse(ifparts, ParseBlock(expr.Body)));
                        break;

                    case "for":
                        {
                            var v = expr[0].Value;
                            expr.RemoveRange(0, 2);
                            if (expr.Count == 2 && expr[0].Value == "range")
                            {

                            }
                        }
                        break;

                    case "while":
                        br.Push(Exp.Label());
                        con.Push(Exp.Label());

                        IL.Add(Exp.Loop(
                            Exp.IfThenElse(
                                test: AsBool(Parse(expr)),
                                ifTrue: ParseBlock(expr.Body),
                                ifFalse: Exp.Goto(br.Peek())
                            ),
                            br.Peek(),
                            con.Peek()));

                        br.Pop();
                        con.Pop();
                        break;

                    case "try":

                        break;

                    case "except":

                        break;

                    case "finally":

                        break;

                    case "import":

                        break;

                    case "from":

                        break;

                    case "return":
                        IL.Add(Exp.Goto(ret.Peek(), Parse(expr)));
                        break;

                    case "break":
                        IL.Add(Exp.Goto(br.Peek()));
                        break;

                    case "continue":
                        IL.Add(Exp.Goto(con.Peek()));
                        break;

                    case "pass":
                        // nothing to do
                        break;

                    case "global":
                        {
                            var w = SplitComma(expr);
                            foreach (var v in w)
                                Local.Add(v[0].Value, GlobalAccess(v[0].Value));
                        }
                        break;

                    case "nonlocal":

                        break;

                    case "del":

                        break;

                    case "raise":

                        break;

                    case "assert":

                        break;

                    case "yield":

                        break;

                    case "print":
                        IL.Add(Exp.Call(typeof(Console).GetMethod("WriteLine", new[] { typeof(object) }), Parse(expr)));
                        break;

                    case "py":
                        PushCL(expr[0].Value, IL);
                        break;

                    case "assign":
                        {
                            (var left, var right, var op) = SplitAssign(expr);

                            var vars = SplitComma(left);
                            var values = SplitComma(right);

                            if (op != Op.None) // augmented assignment
                            {
                                if (vars.Count == 1)
                                {
                                    if (values.Count == 1)
                                    {
                                        Exp var = ParseUnit(vars[0]);
                                        Exp val = Parse(values[0]);
                                        IL.Add(Exp.Assign(var, Dynamic(op, var, val)));
                                    }
                                    else if (values.Count > 1) // tuple 
                                    {
                                        // pass
                                    }
                                }
                                else
                                    throw new Exception("illegal expression for augmented assignment");
                            }
                            else if (vars.Count == values.Count)
                            {
                                for (int j = 0; j < vars.Count; j++)
                                {
                                    IL.Add(Assign(vars[j], Parse(values[j])));
                                }
                            }
                            else if (vars.Count > 1 && values.Count == 1) // unpack
                            {
                                Exp iter = Cache("c_002");

                                var iterable = Parse(values[0]);
                                IL.Add(Exp.Assign(iter, iterable));

                                for (int j = 0; j < vars.Count; j++)
                                {
                                    IL.Add(Assign(vars[j], Call(iter, "__getitem__", Exp.Constant(new Int(j)))));
                                }
                            }
                            else if (vars.Count == 1 && values.Count > 1) // tuple
                            {
                                var tpl = Exp.ListInit(Exp.New(typeof(Tuple)),
                                    values.Select(Parse));
                                IL.Add(Assign(vars[0], tpl));
                            }
                        }
                        break;

                    case null:
                        IL.Add(Parse(expr));
                        break;
                }
            }
        }

        /// <summary>
        /// Default block parser
        /// </summary>
        Exp ParseBlock(List<Expression> body)
        {
            var IL = new List<Exp>();
            PushBlock(body, IL);
            return Exp.Block(IL);
        }

        static Exp IfThenElse(Stack<If> ifparts, Exp elseblock = null)
        {
            If node = ifparts.Pop();
            Exp r;

            if (elseblock is null)
                r = Exp.IfThen(node.Test, node.Body);
            else
                r = Exp.IfThenElse(node.Test, node.Body, elseblock);

            while (ifparts.Count > 0)
            {
                node = ifparts.Pop();
                r = Exp.IfThenElse(node.Test, node.Body, r);
            }

            return r;
        }
    }

    class If
    {
        public Exp Test;
        public Exp Body;
    }
}