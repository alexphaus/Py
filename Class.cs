using System;
using System.Collections.Generic;
using Exp = System.Linq.Expressions.Expression;

namespace Py
{
    public class Object
    {
        public Class __class__ { get; }
        public Dictionary<Object, Object> __dict__ { get; }

        public Object()
        {

        }

        public Object(Class type)
        {
            __class__ = type;
            __dict__ = new Dictionary<Object, Object>();
        }

        public virtual Object Callvirt(string name, Args arg)
        {
            if (__class__.Methods.TryGetValue(name, out Function func))
            {
                arg.self = this;
                return func.__call__(arg);
            }
            
            throw new Exception($"object '{__class__.Name}' has no method '{name}'");
        }

        public virtual Object __call__(Args arg) =>
            Callvirt("__call__", arg);

        public virtual Object __getattr__(String name) => __dict__[name];

        public virtual Object __setattr__(String name, Object value) =>
            __dict__[name] = value;

        public virtual Object __getitem__(Object key) =>
            Callvirt("__getitem__", Args.Create(key));

        public virtual Object __setitem__(Object key, Object value) =>
            Callvirt("__setitem__", Args.Create(key, value));

        // Binary Operators

        public virtual Object __add__(Object other) =>
            Callvirt("__add__", Args.Create(other));

        public virtual Object __sub__(Object other) =>
            Callvirt("__sub__", Args.Create(other));

        public virtual Object __mul__(Object other) =>
            Callvirt("__mul__", Args.Create(other));

        public virtual Object __div__(Object other) =>
            Callvirt("__div__", Args.Create(other));

        public virtual Object __floordiv__(Object other) =>
            Callvirt("__floordiv__", Args.Create(other));

        public virtual Object __mod__(Object other) =>
            Callvirt("__mod__", Args.Create(other));

        public virtual Object __pow__(Object other) =>
            Callvirt("__pow__", Args.Create(other));

        public virtual Object __lshift__(Object other) =>
            Callvirt("__lshift__", Args.Create(other));

        public virtual Object __rshift__(Object other) =>
            Callvirt("__rshift__", Args.Create(other));

        public virtual Object __and__(Object other) =>
            Callvirt("__and__", Args.Create(other));

        public virtual Object __xor__(Object other) =>
            Callvirt("__xor__", Args.Create(other));

        public virtual Object __or__(Object other) =>
            Callvirt("__or__", Args.Create(other));

        // Unary Operators

        public virtual Object __neg__() =>
            Callvirt("__neg__", Args.Empty);

        public virtual Object __pos__() =>
            Callvirt("__pos__", Args.Empty);

        public virtual Object __invert__() =>
            Callvirt("__invert__", Args.Empty);

        // Comparison Operators

        public virtual Object __lt__(Object other) =>
            Callvirt("__lt__", Args.Create(other));

        public virtual Object __le__(Object other) =>
            Callvirt("__le__", Args.Create(other));

        public virtual Object __eq__(Object other) =>
            Callvirt("__eq__", Args.Create(other));

        public virtual Object __ne__(Object other) =>
            Callvirt("__ne__", Args.Create(other));

        public virtual Object __ge__(Object other) =>
            Callvirt("__ge__", Args.Create(other));

        public virtual Object __gt__(Object other) =>
            Callvirt("__gt__", Args.Create(other));

        // Logical Operators

        public virtual Object __not__() =>
            b ? Py.False : Py.True;

        public virtual Object __andalso__(Object other) =>
            b ? other : this;

        public virtual Object __orelse__(Object other) =>
            b ? this : other;

        // Membership Operators

        public virtual Object __in__(Object other) =>
            Callvirt("__contains__", Args.Create(other));

        public virtual Object __notin__(Object other) =>
            __in__(other).__not__();

        // Identity Operators

        public virtual Object __is__(Object other) =>
            ReferenceEquals(this, other) ? Py.True : Py.False;

        public virtual Object __isnot__(Object other) =>
            __is__(other).__not__();

        // Conversion Operators

        public virtual int i => Callvirt("__int__", Args.Empty).i;

        public virtual double f => Callvirt("__float__", Args.Empty).f;

        public virtual bool b => Callvirt("__bool__", Args.Empty).b;
        
        public virtual dynamic d => Callvirt("__dynamic__", Args.Empty).d;

        public override string ToString()
        {
            return __class__ != null ? $"<object '{__class__.Name}'>" : base.ToString();
        }
    }

    public class Class : Object
    {
        public Class Base { get; }
        public string Name { get; }
        public Dictionary<string, Function> Methods { get; }
        public Dictionary<string, Exp> Fields { get; }
        Action<Object> initf;

        /* cached magic methods */
        public Function __init__;
        
        public Class(string name,
                    Dictionary<string, Function> methods,
                    Dictionary<string, Exp> fields,
                    Action<Object> initf,
                    Class parent)
        {
            Name = name;
            Methods = methods;
            Fields = fields;
            this.initf = initf;
            Base = parent;

            methods.TryGetValue("__init__", out __init__);
        }

        public override Object Callvirt(string name, Args arg)
        {
            if (Methods.TryGetValue(name, out Function func))
            {
                var self = arg[0];
                arg = arg.Shift(); // remove first argument
                arg.self = self;
                return func.__call__(arg);
            }
            
            throw new Exception($"'class {__class__.Name}' has no method '{name}'");
        }

        public override Object __call__(Args arg)
        {
            var instance = new Object(this);
            initf?.Invoke(instance); // initialize fields
            arg.self = instance;
            __init__?.__call__(arg); // call __init__
            return instance;
        }

        public override string ToString()
        {
            return $"<class '{Name}'>";
        }
    }

    partial class Py
    {
        Class ParseClass(Expression cls)
        {
            string name = cls[0].Value;
            var methods = new Dictionary<string, Function>();
            var fields = new Dictionary<string, Exp>();
            Action<Object> initf = null;
            Class parent = null;

            // inheritance
            if (cls.Count == 2)
            {
                parent = (Class)Py.Global[cls[1].Subset[0].Value];
                // copy methods
                foreach (var m in parent.Methods)
                    methods.Add(m.Key, m.Value);
                // copy fields
                foreach (var f in parent.Fields)
                    fields.Add(f.Key, f.Value);
            }

            // parse fields and methods
            // overload in case member is inherited from base class
            for (int i = 0; i < cls.Body.Count; i++)
            {
                Expression expr = cls.Body[i];

                if (expr.Command == "def")
                {
                    Function func = ParseFunc(expr, has_self: true);
                    methods[func.Name] = func;
                }
                else if (Contains(expr, TokenType.Assign))
                {
                    (var left, var right, var op) = SplitAssign(expr);
                    fields[left[0].Value] = Parse(right);
                }
            }

            // build field initializer
            if (fields.Count > 0)
            {
                var self = Exp.Parameter(typeof(Object), "self");
                Exp loc = Exp.Property(self, "loc");
                var buildr = new List<Exp>();

                foreach (var f in fields)
                    buildr.Add(Exp.Call(loc,
                        typeof(Dictionary<string, Object>).GetMethod("Add"),
                        Exp.Constant(f.Key), f.Value));

                Exp body = Exp.Block(buildr);
                initf = Exp.Lambda<Action<Object>>(body, self).Compile();
            }

            Class type = new Class(name, methods, fields, initf, parent);
            return type;
        }
    }
}