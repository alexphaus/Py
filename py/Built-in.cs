using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using static Py.Py;

namespace Py
{
    /* built-in types */

    public class List : Object
    {
        public List<Object> list;

        public List()
        {
            list = new List<Object>();
        }

        public List(Object[] arr)
        {
            list = new List<Object>(arr);
        }

        public override Object Callvirt(string name, Args arg)
        {
            switch (name)
            {
                case "append":
                    list.Add(arg[0]);
                    return None;

                case "__len__":
                    return new Int(list.Count);

                case "toarray":
                    {
                        Type elemType = (Type)arg[0].d;
                        var arrayvalue = list.Select(x => x.d).ToArray();
                        var destinationArray = Array.CreateInstance(elemType, arrayvalue.Length);
                        Array.Copy(arrayvalue, destinationArray, arrayvalue.Length);
                        return new Dynamic(destinationArray);
                    }
            }
            throw new Exception($"object 'list' has no method '{name}'");
        }

        public override Object __getitem__(Object key)
        {
            return list[key.i];
        }

        public override bool b => list.Count > 0;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            foreach (Object obj in list)
            {
                sb.Append(obj.ToString());
                sb.Append(", ");
            }

            if (list.Count > 0)
                sb.Remove(sb.Length - 2, 2);

            sb.Append("]");
            return sb.ToString();
        }
    }

    public class Tuple : Object
    {
        public List<Object> tuple;

        public Tuple()
        {
            tuple = new List<Object>();
        }

        public Tuple(Object[] arr)
        {
            tuple = new List<Object>(arr);
        }

        public override Object Callvirt(string name, Args arg)
        {
            switch (name)
            {
                case "index":

                    break;

                case "count":

                    break;

                case "__len__":
                    return new Int(tuple.Count);
            }
            throw new Exception($"object 'tuple' has no method '{name}'");
        }

        public override Object __getitem__(Object key)
        {
            return tuple[key.i];
        }

        public override bool b => tuple.Count > 0;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");

            foreach (Object obj in tuple)
            {
                sb.Append(obj.ToString());
                sb.Append(", ");
            }

            if (tuple.Count > 0)
                sb.Remove(sb.Length - 2, 2);

            sb.Append(")");
            return sb.ToString();
        }
    }

    public class Dict : Object
    {
        public Dictionary<Object, Object> dict;

        public Dict()
        {
            dict = new Dictionary<Object, Object>();
        }

        public Dict(Dictionary<Object, Object> dct)
        {
            dict = dct;
        }

        public override Object Callvirt(string name, Args arg)
        {
            return None;
        }

        public override Object __getattr__(String name)
        {
            return dict[name];
        }

        public override Object __setattr__(String name, Object value)
        {
            dict[name] = value;
            return value;
        }

        public override Object __getitem__(Object key)
        {
            return dict[key];
        }

        public override Object __setitem__(Object key, Object value)
        {
            dict[key] = value;
            return value;
        }

        public override bool b => dict.Count > 0;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");

            foreach (var pair in dict)
            {
                sb.Append(pair.Key.ToString());
                sb.Append(": ");
                sb.Append(pair.Value.ToString());
                sb.Append(", ");
            }

            if (dict.Count > 0)
                sb.Remove(sb.Length - 2, 2);

            sb.Append("}");
            return sb.ToString();
        }
    }

    public class Set : Object
    {
        public HashSet<Object> set;

        public Set()
        {
            set = new HashSet<Object>();
        }

        public Set(Object[] arr)
        {
            set = new HashSet<Object>(arr);
        }

        public override Object Callvirt(string name, Args arg)
        {
            return None;
        }

        public override bool b => set.Count > 0;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");

            foreach (Object obj in set)
            {
                sb.Append(obj.ToString());
                sb.Append(", ");
            }

            if (set.Count > 0)
                sb.Remove(sb.Length - 2, 2);

            sb.Append("}");
            return sb.ToString();
        }
    }

    public class String : Object
    {
        public string str;

        public String(string s)
        {
            str = s;
        }

        public override Object Callvirt(string name, Args arg)
        {
            switch (name)
            {
                case "upper":
                    return new String(str.ToUpper());
            }

            throw new Exception($"object 'str' has no method '{name}'");
        }

        public override Object __getitem__(Object key)
        {
            return new String(str[key.i].ToString());
        }

        public override Object __add__(Object other) =>
            new String(str + ((String)other).str);

        public override bool Equals(object obj)
        {
            if (obj is String other)
            {
                return other.str == str;
            }
            return false;
        }

        public override int GetHashCode() => str.GetHashCode();

        public override int i => int.Parse(str);

        public override double f => double.Parse(str);

        public override bool b => str.Length > 0;

        public override dynamic d => str;

        public override string ToString() => str;
    }

    public class Int : Object
    {
        int v;

        public Int(int value)
        {
            v = value;
        }

        public override Object Callvirt(string name, Args arg)
        {
            return None;
        }

        // Binary Operators

        public override Object __add__(Object other) =>
            new Int(v + other.i);

        public override Object __sub__(Object other) =>
            new Int(v - other.i);

        public override Object __mul__(Object other) =>
            new Int(v * other.i);

        public override Object __div__(Object other) =>
            new Float((double)v / other.f);

        public override Object __floordiv__(Object other) =>
            new Int(v / other.i);

        public override Object __mod__(Object other) =>
            new Int(v % other.i);

        public override Object __pow__(Object other) =>
            new Float(Math.Pow(v, other.i));

        public override Object __lshift__(Object other) =>
            new Int(v << other.i);

        public override Object __rshift__(Object other) =>
            new Int(v >> other.i);

        public override Object __and__(Object other) =>
            new Int(v & other.i);

        public override Object __xor__(Object other) =>
            new Int(v ^ other.i);

        public override Object __or__(Object other) =>
            new Int(v | other.i);

        // Unary Operators

        public override Object __neg__() =>
            new Int(-v);

        public override Object __pos__() =>
            new Int(+v);

        public override Object __invert__() =>
            new Int(~v);

        // Comparison Operators

        public override Object __lt__(Object other) =>
            v < other.i ? Py.True : Py.False;

        public override Object __le__(Object other) =>
            v <= other.i ? Py.True : Py.False;

        public override Object __eq__(Object other) =>
            v == other.i ? Py.True : Py.False;

        public override Object __ne__(Object other) =>
            v != other.i ? Py.True : Py.False;

        public override Object __ge__(Object other) =>
            v >= other.i ? Py.True : Py.False;

        public override Object __gt__(Object other) =>
            v > other.i ? Py.True : Py.False;

        public override bool Equals(object obj)
        {
            if (obj is Int other)
            {
                return other.i == v;
            }
            return false;
        }

        public override int GetHashCode() => v;

        public override int i => v;

        public override double f => v;

        public override bool b => v != 0;

        public override dynamic d => v;

        public override string ToString() => v.ToString();
    }

    public class Float : Object
    {
        double v;

        public Float(double value)
        {
            v = value;
        }

        public override Object Callvirt(string name, Args arg)
        {
            return None;
        }

        // Binary Operators

        public override Object __add__(Object other) =>
            new Float(v + other.f);

        public override Object __sub__(Object other) =>
            new Float(v - other.f);

        public override Object __mul__(Object other) =>
            new Float(v * other.f);

        public override Object __div__(Object other) =>
            new Float(v / other.f);

        public override Object __floordiv__(Object other) =>
            new Float(Math.Floor(v / other.f));

        public override Object __mod__(Object other) =>
            new Float(v % other.f);

        public override Object __pow__(Object other) =>
            new Float(Math.Pow(v, other.f));

        // Unary Operators

        public override Object __neg__() =>
            new Float(-v);

        public override Object __pos__() =>
            new Float(+v);

        // Comparison Operators

        public override Object __lt__(Object other) =>
            v < other.f ? Py.True : Py.False;

        public override Object __le__(Object other) =>
            v <= other.f ? Py.True : Py.False;

        public override Object __eq__(Object other) =>
            v == other.f ? Py.True : Py.False;

        public override Object __ne__(Object other) =>
            v != other.f ? Py.True : Py.False;

        public override Object __ge__(Object other) =>
            v >= other.f ? Py.True : Py.False;

        public override Object __gt__(Object other) =>
            v > other.f ? Py.True : Py.False;

        public override int i => (int)v;

        public override double f => v;

        public override bool b => v != 0;

        public override dynamic d => v;

        public override string ToString() => v.ToString();
    }

    public class Bool : Object
    {
        bool v;

        public Bool(bool value)
        {
            v = value;
        }

        public override Object Callvirt(string name, Args arg)
        {
            return None;
        }

        public override bool b => v;

        public override dynamic d => v;

        public override string ToString() => v.ToString();
    }

    public class NoneType : Object
    {
        public override Object Callvirt(string name, Args arg)
        {
            return None;
        }

        public override bool b => false;

        public override dynamic d => null;

        public override string ToString() => "None";
    }

    public class Dynamic : Object
    {
        dynamic v;

        public Dynamic(object value)
        {
            v = value;
        }

        public override Object Callvirt(string name, Args arg)
        {
            Type type = v is Type t ? t : ((object)v).GetType();

            object[] parameters = new object[arg.Input.Length];
            Type[] types = new Type[arg.Input.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                object a = arg.Input[i].d;
                parameters[i] = a;
                types[i] = a is Type ? typeof(Type) : a.GetType();
            }

            var m = type.GetMethod(name, types);

            if (m is null)
                throw new Exception($"Dynamic '{type}' has no method '{name}' with the supplied argument types '" + string.Join<Type>("', '", types) + "'");

            return new Dynamic(m.Invoke(v, parameters));
        }

        public override Object __call__(Args arg)
        {
            object[] parameters = new object[arg.Input.Length];

            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = arg.Input[i].d;

            if (v is Type type)
                return new Dynamic(Activator.CreateInstance(type, parameters));
            else
            {
                var m = ((object)v).GetType().GetMethod("Invoke");
                return new Dynamic(m.Invoke(v, parameters));
            }
        }

        public override Object __getattr__(String name)
        {
            Type type = v is Type t ? t : ((object)v).GetType();

            // property
            var pi = type.GetProperty(name.str);
            if (pi != null)
                return new Dynamic(pi.GetValue(v));

            // field
            var fi = type.GetField(name.str);
            if (fi != null)
                return new Dynamic(fi.GetValue(v));

            throw new Exception($"Dynamic '{type}' has no attribute '{name}'");
        }

        public override Object __setattr__(String name, Object value)
        {
            Type type = v is Type t ? t : ((object)v).GetType();

            // property
            var pi = type.GetProperty(name.str);
            if (pi != null)
            {
                pi.SetValue(v, value.d);
                return value;
            }

            // field
            var fi = type.GetField(name.str);
            if (fi != null)
            {
                fi.SetValue(v, value.i);
                return value;
            }

            // event
            var ei = type.GetEvent(name.str);
            if (ei != null)
            {
                var fx = (Action<object, object>)((s, e) =>
                    value.__call__(Args.Create(new Dynamic(s), new Dynamic(e))));
                var handler = Delegate.CreateDelegate(ei.EventHandlerType, fx.Target, fx.Method);
                ei.AddEventHandler(v, handler);
                return value;
            }

            throw new Exception($"Dynamic '{type}' has no attribute '{name}'");
        }

        public override Object __getitem__(Object key)
        {
            if (v is Type type)
            {
                dynamic index = key.d;
                if (index is null)
                {
                    return new Dynamic(type.MakeArrayType());
                }
                else if (index is Type t)
                {
                    return new Dynamic(type.MakeGenericType(t));
                }
            }
            return new Dynamic(v[key.d]);
        }

        public override Object __setitem__(Object key, Object value)
        {
            v[key.d] = value.d;
            return null;
        }

        // Binary Operators

        public override Object __add__(Object other) =>
            new Dynamic(v + other.d);

        public override Object __sub__(Object other) =>
            new Dynamic(v - other.d);

        public override Object __mul__(Object other) =>
            new Dynamic(v * other.d);

        public override Object __div__(Object other) =>
            new Dynamic(v / other.d);

        public override Object __floordiv__(Object other) =>
            new Dynamic(Math.Floor(v / other.d));

        public override Object __mod__(Object other) =>
            new Dynamic(v % other.d);

        public override Object __pow__(Object other) =>
            new Dynamic(Math.Pow(v, other.d));

        public override Object __lshift__(Object other) =>
            new Dynamic(v << other.d);

        public override Object __rshift__(Object other) =>
            new Dynamic(v >> other.d);

        public override Object __and__(Object other) =>
            new Dynamic(v & other.d);

        public override Object __xor__(Object other) =>
            new Dynamic(v ^ other.d);

        public override Object __or__(Object other) =>
            new Dynamic(v | other.d);

        // Unary Operators

        public override Object __neg__() =>
            new Dynamic(-v);

        public override Object __pos__() =>
            new Dynamic(+v);

        public override Object __invert__() =>
            new Dynamic(~v);

        // Comparison Operators

        public override Object __lt__(Object other) =>
            new Dynamic(v < other.d);

        public override Object __le__(Object other) =>
            new Dynamic(v <= other.d);

        public override Object __eq__(Object other) =>
            new Dynamic(v == other.d);

        public override Object __ne__(Object other) =>
            new Dynamic(v != other.d);

        public override Object __ge__(Object other) =>
            new Dynamic(v >= other.d);

        public override Object __gt__(Object other) =>
            new Dynamic(v > other.d);

        // Logical Operators

        public override Object __not__() =>
            new Dynamic(!v);

        public override Object __andalso__(Object other) =>
            new Dynamic(v && other.d);

        public override Object __orelse__(Object other) =>
            new Dynamic(v || other.d);

        // Membership Operators

        public override Object __in__(Object other)
        {
            object seq = other.d;

            if (seq is System.Collections.IList lst)
                return new Dynamic(lst.Contains(v));

            else if (seq is string str)
                return new Dynamic(str.Contains((string)v));

            else if (seq is System.Collections.IDictionary dct)
                return new Dynamic(dct.Contains(v));

            throw new Exception($"Unsupported sequence '{seq.GetType()}'");
        }

        // Identity Operators

        public override Object __is__(Object other)
        {
            return new Dynamic(((object)v).GetType() == (Type)other.d);
        }

        public override int i => (int)v;

        public override double f => (double)v;

        public override dynamic d => v;

        public override bool b => (bool)v;

        public override string ToString() => v?.ToString() ?? "NULL";
    }

    public class Range //: IObject
    {

    }

    public class Generator //: IObject
    {

    }
}