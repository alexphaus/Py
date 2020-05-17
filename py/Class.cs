using System;
using System.Collections.Generic;
using System.Text;
using Exp = System.Linq.Expressions.Expression;

namespace Py
{
    public class Object
    {
        public Class __class__ { get; }
        public Dictionary<string, Object> loc { get; }

        public Object()
        {

        }

        public Object(Class type)
        {
            __class__ = type;
            loc = new Dictionary<string, Object>();
        }

        public virtual Object Callvirt(string name, Args arg)
        {
            if (__class__.Methods.TryGetValue(name, out Function func))
            {
                arg.self = this;
                return func.__call__(arg);
            }
            
            throw new Exception($"'{__class__.Name}' object has no attribute '{name}'");
        }

        public virtual Object __call__(Args arg) =>
            Callvirt("__call__", arg);

        public virtual Object __getattr__(string name) => loc[name];

        public virtual Object __setattr__(string name, Object value) =>
            loc[name] = value;

        public virtual Object __getitem__(Object key) =>
            Callvirt("__getitem__", Args.Create(key));

        public virtual Object __setitem__(Object key, Object value) =>
            Callvirt("__setitem__", Args.Create(key, value));

        /* OPERATION METHODS */

        public virtual Object __add__(Object other) =>
            Callvirt("__add__", Args.Create(other));

        public virtual Object __sub__(Object other) =>
            Callvirt("__sub__", Args.Create(other));

        public virtual Object __mul__(Object other) =>
            Callvirt("__mul__", Args.Create(other));

        public virtual Object __div__(Object other) =>
            Callvirt("__div__", Args.Create(other));

        public virtual Object __lt__(Object other) =>
            Callvirt("__lt__", Args.Create(other));

        public virtual bool __bool__() => true;

        public virtual object __object__() => this;

        public override string ToString()
        {
            return $"<object '{__class__.Name}'>";
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
            methods.TryGetValue("__init__", out __init__);
            Fields = fields;
            this.initf = initf;
            Base = parent;
        }

        public override Object Callvirt(string name, Args arg)
        {
            if (Methods.TryGetValue(name, out Function func))
            {
                var self = arg[0];
                arg = arg.Shift(); // remove first argument
                return func.__call__(arg);
            }
            
            throw new Exception($"'{__class__.Name}' class has no attribute '{name}'");
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
                else if (HasAssign(expr))
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