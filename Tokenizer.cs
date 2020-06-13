using System;
using System.Collections.Generic;
using System.Text;

namespace Py
{
    enum Op
    {
        None,
        // Exponentiation
        Power,
        // Multiplicative
        Multiply,
        Divide,
        FloorDivide,
        Modulo,
        // Additive [Unary]
        Invert,
        Add,
        Subtract,
        // Shift
        LeftShift,
        RightShift,
        // Bitwise AND
        And,
        // Bitwise XOR
        Xor,
        // Bitwise OR
        Or,
        // Relational
        Equal,
        NotEqual,
        Less,
        LessOrEqual,
        Greater,
        GreaterOrEqual,
        Is,
        IsNot,
        In,
        NotIn,
        // Logical NOT
        Not,
        // Logical AND
        AndAlso,
        // Logical OR
        OrElse
    }

    enum Capture
    {
        None,
        Identifier,
        Number,
        String,
        DocString,
        Comment,
        Indent
    }

    enum TokenType
    {
        Keyword,
        Identifier,
        Member,
        Number,
        String,
        Operator,
        NewLine,
        Parenthesis,
        Brackets,
        Braces,
        Colon,
        Comma,
        Assign
    }

    class Token
    {
        public TokenType Type;
        public string Value;
        public Op op;
        public int i;
        public List<Token> Subset; // for parenthesis, brackets, braces
    }

    partial class Py
    {
        List<Token> Tokenize(string src)
        {
            Capture cap = Capture.None;
            var str = new StringBuilder();
            bool isMember = false;
            var stack = new Stack<List<Token>>();
            stack.Push(new List<Token>()); // main

            void push(TokenType type, string value) =>
                stack.Peek().Add(new Token { Type = type, Value = value });

            void pushSubset(TokenType type)
            {
                List<Token> set = stack.Pop();
                stack.Peek().Add(new Token { Type = type, Subset = set });
            }

            void pushOp(Op op) =>
                stack.Peek().Add(new Token { Type = TokenType.Operator, op = op, i = pre[(int)op] });

            char c, f, l = '\0';
            src = src + "\r\n";

            for (int i = 0, to = src.Length - 1; i < to; i++)
            {
                c = src[i]; // current char
                f = src[i + 1]; // following char

                switch (cap)
                {
                    case Capture.Identifier:
                        if (char.IsLetterOrDigit(c) || c == '_')
                        {
                            str.Append(c);
                        }
                        else
                        {
                            string id = str.ToString();
                            switch (id)
                            {
                                case "is":
                                    pushOp(Op.Is);
                                    break;

                                case "in":
                                    {
                                        // "not" "in"
                                        var lst = stack.Peek();
                                        if (lst.Count > 0)
                                        {
                                            Token last = lst[lst.Count - 1];
                                            if (last.Type == TokenType.Operator && last.op == Op.Not)
                                            {
                                                last.op = Op.NotIn;
                                                last.i = pre[(int)Op.NotIn];
                                            }
                                            else
                                                pushOp(Op.In);
                                        }
                                        else
                                            throw new Exception("invalid syntax");
                                    }
                                    break;

                                case "not":
                                    {
                                        // "is" "not"
                                        var lst = stack.Peek();
                                        if (lst.Count > 0)
                                        {
                                            Token last = lst[lst.Count - 1];
                                            if (last.Type == TokenType.Operator && last.op == Op.Is)
                                            {
                                                last.op = Op.IsNot;
                                                last.i = pre[(int)Op.IsNot];
                                                break;
                                            }
                                        }
                                        pushOp(Op.Not);
                                    }
                                    break;

                                case "and":
                                    pushOp(Op.AndAlso);
                                    break;

                                case "or":
                                    pushOp(Op.OrElse);
                                    break;

                                case "class":
                                case "def":
                                case "if":
                                case "elif":
                                case "else":
                                case "for":
                                case "while":
                                case "try":
                                case "except":
                                case "finally":
                                case "import":
                                case "from":
                                case "as":
                                case "return":
                                case "break":
                                case "continue":
                                case "pass":
                                case "global":
                                case "nonlocal":
                                case "del":
                                case "raise":
                                case "assert":
                                case "yield":
                                case "print":
                                case "lambda":
                                case "py":
                                    push(TokenType.Keyword, id);
                                    break;

                                default:
                                    if (isMember)
                                    {
                                        push(TokenType.Member, id);
                                        isMember = false;
                                    }
                                    else
                                        push(TokenType.Identifier, id);
                                    break;
                            }
                            str.Clear();
                            cap = Capture.None;
                            goto case Capture.None;
                        }
                        break;

                    case Capture.Number:
                        if (char.IsDigit(c) || c == '.')
                        {
                            str.Append(c);
                        }
                        else
                        {
                            string number = str.ToString();
                            push(TokenType.Number, number);
                            str.Clear();
                            cap = Capture.None;
                            goto case Capture.None;
                        }
                        break;

                    case Capture.Indent:
                        if (c == ' ' || c == '\t')
                        {
                            str.Append(c);
                        }
                        else if (c == '\n')
                        {
                            str.Clear();
                        }
                        else
                        {
                            if (str.Length > 0)
                            {
                                push(TokenType.NewLine, str.ToString());
                                str.Clear();
                            }
                            else
                                push(TokenType.NewLine, string.Empty);

                            cap = Capture.None;
                            goto case Capture.None;
                        }
                        break;

                    case Capture.None:
                        switch (c)
                        {
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                            case '_':
                                str.Append(c);
                                cap = Capture.Identifier;
                                break;

                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                            case '0':
                                str.Append(c);
                                cap = Capture.Number;
                                break;

                            case ' ':
                            case '\t':
                            case '\r':
                            case ';':
                                // pass
                                break;

                            case '\n':
                                // new line omitted inside parenthesis, brackets or braces
                                if (stack.Count == 1)
                                    cap = Capture.Indent;
                                break;

                            case '.':
                                isMember = true;
                                break;

                            case ':':
                                push(TokenType.Colon, null);
                                break;

                            case ',':
                                push(TokenType.Comma, null);
                                break;

                            case '(':
                            case '[':
                            case '{':
                                stack.Push(new List<Token>());
                                break;

                            case ')':
                                pushSubset(TokenType.Parenthesis);
                                break;

                            case ']':
                                pushSubset(TokenType.Brackets);
                                break;

                            case '}':
                                pushSubset(TokenType.Braces);
                                break;

                            case '"':
                            case '\'':
                                if (f == c && src[i + 2] == c)
                                {
                                    cap = Capture.DocString;
                                    i += 2;
                                }
                                else
                                    cap = Capture.String;
                                l = c;
                                break;

                            case '#':
                                cap = Capture.Comment;
                                break;

                            case '~':
                                pushOp(Op.Invert);
                                break;

                            case '+':
                                if (f == '=')
                                {
                                    stack.Peek().Add(new Token { Type = TokenType.Assign, op = Op.Add });
                                    i++;
                                }
                                else
                                    pushOp(Op.Add);
                                break;

                            case '-':
                                if (f == '=')
                                {
                                    stack.Peek().Add(new Token { Type = TokenType.Assign, op = Op.Subtract });
                                    i++;
                                }
                                else
                                    pushOp(Op.Subtract);
                                break;

                            case '*':
                                if (f == '=')
                                {
                                    stack.Peek().Add(new Token { Type = TokenType.Assign, op = Op.Multiply });
                                    i++;
                                }
                                else if (f == '*')
                                {
                                    if (src[i + 2] == '=')
                                    {
                                        stack.Peek().Add(new Token { Type = TokenType.Assign, op = Op.Power });
                                        i += 2;
                                    }
                                    else
                                    {
                                        pushOp(Op.Power);
                                        i++;
                                    }
                                }
                                else
                                    pushOp(Op.Multiply);
                                break;

                            case '/':
                                if (f == '=')
                                {
                                    stack.Peek().Add(new Token { Type = TokenType.Assign, op = Op.Divide });
                                    i++;
                                }
                                else if (f == '/')
                                {
                                    if (src[i + 2] == '=')
                                    {
                                        stack.Peek().Add(new Token { Type = TokenType.Assign, op = Op.FloorDivide });
                                        i += 2;
                                    }
                                    else
                                    {
                                        pushOp(Op.FloorDivide);
                                        i++;
                                    }
                                }
                                else
                                    pushOp(Op.Divide);
                                break;

                            case '%':
                                if (f == '=')
                                {
                                    stack.Peek().Add(new Token { Type = TokenType.Assign, op = Op.Modulo });
                                    i++;
                                }
                                else
                                    pushOp(Op.Modulo);
                                break;

                            case '<':
                                if (f == '=')
                                {
                                    pushOp(Op.LessOrEqual);
                                    i++;
                                }
                                else if (f == '<')
                                {
                                    pushOp(Op.LeftShift);
                                    i++;
                                }
                                else
                                    pushOp(Op.Less);
                                break;

                            case '>':
                                if (f == '=')
                                {
                                    pushOp(Op.GreaterOrEqual);
                                    i++;
                                }
                                else if (f == '>')
                                {
                                    pushOp(Op.RightShift);
                                    i++;
                                }
                                else
                                    pushOp(Op.Greater);
                                break;

                            case '&':
                                pushOp(Op.And);
                                break;

                            case '^':
                                pushOp(Op.Xor);
                                break;

                            case '|':
                                pushOp(Op.Or);
                                break;

                            case '!':
                                if (f == '=')
                                {
                                    pushOp(Op.NotEqual);
                                    i++;
                                }
                                break;

                            case '=':
                                if (f == '=')
                                {
                                    pushOp(Op.Equal);
                                    i++;
                                }
                                else
                                    push(TokenType.Assign, null);
                                break;
                        }
                        break;

                    case Capture.String:
                        if (c == l)
                        {
                            push(TokenType.String, str.ToString());
                            str.Clear();
                            cap = Capture.None;
                        }
                        else if (c == '\r' || c == '\n')
                            throw new Exception("EOL while scanning string literal");
                        else
                            str.Append(c);
                        break;

                    case Capture.DocString:
                        if (c == l && f == l && src[i + 2] == l)
                        {
                            push(TokenType.String, str.ToString());
                            str.Clear();
                            cap = Capture.None;
                            i += 2;
                        }
                        else
                            str.Append(c);
                        break;

                    case Capture.Comment:
                        if (c == '\n')
                        {
                            cap = Capture.None;
                        }
                        break;
                }
            }
            return stack.Pop();
        }
    }
}