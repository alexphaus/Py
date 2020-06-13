using System;
using System.Collections.Generic;
using System.Text;

namespace Py
{
    class Expression : List<Token>
    {
        public string Command;
        public List<Expression> Body;
    }

    partial class Py
    {
        List<Expression> Struct(List<Token> tokens)
        {
            var indentd = new Dictionary<string, List<Expression>>();
            var __main__ = new List<Expression>();
            string ind = string.Empty;
            indentd.Add(ind, __main__);
            var expr = new Expression();
            Token tok;

            for (int i = 0; i < tokens.Count; i++)
            {
                tok = tokens[i];

                switch (tok.Type)
                {
                    case TokenType.NewLine:
                        indentd[ind].Add(expr);
                        expr = new Expression();
                        ind = tok.Value;
                        break;

                    case TokenType.Colon:
                        expr.Body = new List<Expression>(); // init
                        indentd[tokens[i + 1].Value] = expr.Body;
                        break;

                    case TokenType.Keyword:
                        if (expr.Count == 0)
                            expr.Command = tok.Value;
                        else
                            goto default;
                        break;

                    case TokenType.String:
                        if (expr.Count == 0)
                        {
                            expr.Command = "emit";
                            goto default;
                        }
                        break;

                    case TokenType.Assign:
                        expr.Command = "assign";
                        goto default;

                    default:
                        expr.Add(tok);
                        break;
                }
            }

            if (expr.Count > 0)
                indentd[ind].Add(expr);

            return __main__;
        }

        (List<Token> left, List<Token> right, Op op) SplitAssign(List<Token> expr)
        {
            var left = new List<Token>();
            var right = new List<Token>();
            var op = Op.None;
            var cur = left;

            foreach (Token tok in expr)
                if (tok.Type == TokenType.Assign)
                {
                    op = tok.op;
                    cur = right;
                }
                else
                    cur.Add(tok);

            return (left, right, op);
        }

        List<List<Token>> Split(List<Token> expr, TokenType tokenType)
        {
            var values = new List<List<Token>>();
            List<Token> cur;

            if (expr.Count == 0)
                return values;
            else
            {
                cur = new List<Token>();
                values.Add(cur);
            }

            foreach (Token tok in expr)
            {
                if (tok.Type == tokenType)
                {
                    cur = new List<Token>();
                    values.Add(cur);
                }
                else
                    cur.Add(tok);
            }

            return values;
        }
    }
}