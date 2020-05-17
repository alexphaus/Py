﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Exp = System.Linq.Expressions.Expression;

namespace Py
{
    partial class Py
    {
        Exp ParseUnit(List<Token> expr)
        {
            Token tok;

            if (expr.Count == 0)
                return NoneExp;

            else if (expr.Count == 1)
            {
                tok = expr[0];

                switch (tok.Type)
                {
                    case TokenType.Identifier:
                        // keywords
                        switch (tok.Value)
                        {
                            case "None":
                                return NoneExp;

                            case "True":
                                return TrueExp;

                            case "False":
                                return FalseExp;
                        }
                        // user identifiers
                        if (Local != null && Local.TryGetValue(tok.Value, out Exp value))
                            return value;
                        else
                            return GlobalAccess(tok.Value);

                    case TokenType.Number:
                        if (int.TryParse(tok.Value, out int i))
                            return Exp.Constant(new Int(i), Any);

                        else if (double.TryParse(tok.Value, out double d))
                            return Exp.Constant(new Float(d), Any);

                        break;

                    case TokenType.String:
                        return Exp.Constant(new String(tok.Value), Any);

                    case TokenType.Parenthesis:
                        {
                            var w = SplitComma(tok.Subset);

                            if (w.Count == 1) // parenthesis
                                return Parse(w[0]);

                            // tuple

                            var tpl = Exp.New(typeof(Tuple));
                            if (w.Count == 0)
                                return tpl;
                            else
                                return Exp.ListInit(tpl, w.Select(Parse));
                        }
                    case TokenType.Brackets:
                        {
                            var w = SplitComma(tok.Subset);

                            var lst = Exp.New(typeof(List));
                            if (w.Count == 0)
                                return lst;
                            else
                                return Exp.ListInit(lst, w.Select(Parse));
                        }
                    case TokenType.Braces:
                        {
                            var w = SplitComma(tok.Subset);

                            var dct = Exp.New(typeof(Dict));
                            if (w.Count == 0)
                                return dct;
                            else
                            {
                                return Exp.ListInit(dct, w.Select(Parse));
                            }
                        }
                }
            }
            else // length > 1
            {
                tok = expr[expr.Count - 1]; // last

                if (tok.Type == TokenType.Parenthesis)
                {
                    List<Token> arg = tok.Subset;

                    expr.RemoveAt(expr.Count - 1); // remove parenthesis
                    tok = expr[expr.Count - 1]; // last

                    if (tok.Type == TokenType.Member) // call method
                    {
                        string name = tok.Value;
                        expr.RemoveAt(expr.Count - 1);
                        Exp obj = ParseUnit(expr);
                        Exp arg_ = ParseArguments(obj, arg);
                        return Exp.Call(obj, Callvirt, Exp.Constant(name), arg_);
                    }
                    else // invoke
                    {
                        if (expr.Count == 1 && tok.Type == TokenType.Identifier)
                        {
                            switch (tok.Value)
                            {
                                case "len":
                                    return Exp.Call(Parse(arg), Callvirt, Exp.Constant("__len__"), NullExp);

                                case "super":
                                    return Local["self"];
                            }
                        }
                        Exp obj = ParseUnit(expr);
                        Exp arg_ = ParseArguments(obj, arg);
                        return Exp.Call(obj, typeof(Object).GetMethod("__call__"), arg_);
                    }
                }
                else if (tok.Type == TokenType.Brackets) // get item
                {
                    expr.RemoveAt(expr.Count - 1); // remove brackets
                    Exp obj = ParseUnit(expr);
                    Exp key = Parse(tok.Subset);
                    return Exp.Call(obj, Callvirt, key);
                }
                else if (tok.Type == TokenType.Member) // get attr
                {
                    string name = tok.Value;
                    expr.RemoveAt(expr.Count - 1); // remove member
                    Exp obj = ParseUnit(expr);
                    return Exp.Call(obj, typeof(Object).GetMethod("__getattr__"), Exp.Constant(name));
                }
            }

            throw new Exception("invalid syntax");
        }

        /* if-else */
        Exp Parse(List<Token> expr)
        {
            List<Token> a = null, b = null;
            var c = new List<Token>();
            bool cap = true;
            
            foreach (Token tok in expr)
            {
                if (cap)
                {
                    if (tok.Type == TokenType.Keyword)
                    {
                        if (tok.Value == "if")
                        {
                            a = c;
                            c = new List<Token>();
                            continue;
                        }
                        else if (tok.Value == "else")
                        {
                            b = c;
                            c = new List<Token>();
                            cap = false;
                            continue;
                        }
                    }
                }
                c.Add(tok);
            }

            if (cap)
                return ParseMath(c);
            else
                return Exp.Condition(AsBool(ParseMath(b)), ParseMath(a), Parse(c));
        }

        Exp ParseMath(List<Token> expr)
        {
            // parse operation context
            var operands = new List<Exp>();
            var operators = new List<(Op op, int prior)>();
            var u_expr = new List<Token>();
            Token tok;

            for (int i = 0; i < expr.Count; i++)
            {
                tok = expr[i];
                if (tok.Type == TokenType.Operator && u_expr.Count > 0) // binary
                {
                    operands.Add(ParseUnit(u_expr));
                    operators.Add((tok.op, tok.i));
                    u_expr = new List<Token>();
                }
                else
                    u_expr.Add(tok);
            }

            // append last token
            operands.Add(ParseUnit(u_expr));

            while (operators.Count > 0)
            {
                int i = IndexOfMin(operators);
                operands[i] = Dynamic(operators[i].op, operands[i], operands[i + 1]);
                operands.RemoveAt(i + 1);
                operators.RemoveAt(i);
            }

            return operands[0];
        }

        Exp Dynamic(Op op, Exp left, Exp right)
        {
            var meth = OpMeth[(int)op];

            if (meth is null)
            {
                switch (op)
                {
                    case Op.OrElse:
                        Exp c = Cache("c_001");
                        return Exp.Condition(AsBool(Exp.Assign(c, left)), c, right);

                    case Op.AndAlso:
                        c = Cache("c_001");
                        return Exp.Condition(AsBool(Exp.Assign(c, left)), right, c);

                    default:
                        return null; /* shouldn't get here */
                }
            }
            else
            {
                return Exp.Call(left, typeof(Object).GetMethod(meth), right);
            }
        }

        /// <summary>
        /// Get the index of the lowest priority value
        /// </summary>
        int IndexOfMin(List<(Op op, int prior)> lst)
        {
            int min = lst[0].prior;
            int minIndex = 0;
            for (int i = 1; i < lst.Count; i++)
            {
                if (lst[i].prior < min)
                {
                    min = lst[i].prior;
                    minIndex = i;
                }
            }
            return minIndex;
        }
    }
}