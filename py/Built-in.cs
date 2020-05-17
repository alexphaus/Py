using System;
using System.Collections.Generic;
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
            
        }

        public override Object Callvirt(string name, Args arg)
        {
            switch (name)
            {
                case "append":
                    list.Add(arg[0]);
                    break;

                case "__len__":
                    return new Int(list.Count);

                default:
                    break; // throw
            }
            return None;
        }

        public void Add(Object value)
        {
            list.Add(value);
        }

        public override Object __getitem__(Object key)
        {
            return list[((Int)key).i];
        }

        public override bool __bool__()
        {
            return list.Count > 0;
        }

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

                default:
                    break; // throw
            }
            return None;
        }

        public void Add(Object value)
        {
            tuple.Add(value);
        }

        public override Object __getitem__(Object key)
        {
            return tuple[((Int)key).i];
        }

        public override bool __bool__()
        {
            return tuple.Count > 0;
        }

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

        public override Object Callvirt(string name, Args arg)
        {
            return None;
        }

        public void Add(Object key, Object value)
        {
            dict.Add(key, value);
        }

        public override bool __bool__()
        {
            return dict.Count > 0;
        }

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

        public override Object Callvirt(string name, Args arg)
        {
            return None;
        }

        public override bool __bool__()
        {
            return set.Count > 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("set([");

            foreach (Object obj in set)
            {
                sb.Append(obj.ToString());
                sb.Append(", ");
            }

            if (set.Count > 0)
                sb.Remove(sb.Length - 2, 2);

            sb.Append("])");
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
            return None;
        }

        public override Object __getitem__(Object key)
        {
            return new String(str[((Int)key).i].ToString());
        }

        public override Object __add__(Object other)
        {
            return new String(str + ((String)other).str);
        }

        public override bool __bool__()
        {
            return str.Length > 0;
        }

        public override object __object__()
        {
            return str;
        }

        public override bool Equals(object obj)
        {
            if (obj is String other)
            {
                return other.str == str;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return str.GetHashCode();
        }

        public override string ToString()
        {
            return str;
        }
    }

    public class Int : Object
    {
        public int i;

        public Int(int value)
        {
            i = value;
        }

        public override Object Callvirt(string name, Args arg)
        {
            return None;
        }

        public override Object __add__(Object other)
        {
            return new Int(i + ((Int)other).i);
        }

        public override Object __sub__(Object other)
        {
            return new Int(i - ((Int)other).i);
        }

        public override Object __mul__(Object other)
        {
            return new Int(i * ((Int)other).i);
        }

        public override Object __div__(Object other)
        {
            return new Int(i / ((Int)other).i);
        }

        public override Object __lt__(Object other)
        {
            return i < ((Int)other).i ? True : False;
        }

        public override bool __bool__()
        {
            return i != 0;
        }

        public override object __object__()
        {
            return i;
        }

        public override bool Equals(object obj)
        {
            if (obj is Int other)
            {
                return other.i == i;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return i;
        }

        public override string ToString()
        {
            return i.ToString();
        }
    }

    public class Float : Object
    {
        public double f;

        public Float(double value)
        {
            f = value;
        }

        public override Object Callvirt(string name, Args arg)
        {
            return None;
        }

        public override bool __bool__()
        {
            return f != 0;
        }

        public override object __object__()
        {
            return f;
        }

        public override string ToString()
        {
            return f.ToString();
        }
    }

    public class Bool : Object
    {
        public bool b;

        public Bool(bool value)
        {
            b = value;
        }

        public override Object Callvirt(string name, Args arg)
        {
            return None;
        }

        public override bool __bool__()
        {
            return b;
        }

        public override object __object__()
        {
            return b;
        }

        public override string ToString()
        {
            return b.ToString();
        }
    }

    public class NoneType : Object
    {
        public override Object Callvirt(string name, Args arg)
        {
            return None;
        }

        public override bool __bool__()
        {
            return false;
        }

        public override object __object__()
        {
            return null;
        }

        public override string ToString()
        {
            return "None";
        }
    }

    public class CObj //: IObject
    {
        public readonly static CObj Null = new CObj(null);

        public object obj;

        public CObj(object value)
        {
            obj = value;
        }
    }

    public class Range //: IObject
    {

    }

    public class Generator //: IObject
    {

    }
}