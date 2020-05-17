using System;
using System.Collections.Generic;
using System.Text;
using Exp = System.Linq.Expressions.Expression;

namespace Py
{
    public class Function : Object
    {
        public string Name;
        public Params Parameters;
        public Func<Args, Object> fx;

        public override Object __call__(Args arg)
        {
            arg.Sort(Parameters);
            return fx(arg);
        }
    }

    partial class Py
    {
        Function ParseFunc(Expression def, bool has_self = false)
        {
            string name = def[0].Value;
            var ps = ParseParameters(def[1].Subset, omitFirstParameter: has_self);

            /* write IL */
            LocalBuilder loc = Local;
            Local = new LocalBuilder();

            var IL = new List<Exp>();
            ret.Push(Exp.Label(typeof(Object)));
            
            // load parameters

            var arg = Exp.Parameter(typeof(Args), "arg");

            if (has_self)
            {
                var self = Exp.Variable(typeof(Object), "self");
                Local.Add(self);
                IL.Add(Exp.Assign(self, Exp.Field(arg, "self")));
            }

            ParamInfo pi;
            for (int i = 0; i < ps.Info.Length; i++)
            {
                pi = ps.Info[i];
                var p = Exp.Variable(typeof(Object), pi.Name.str);
                Local.Add(p);
                IL.Add(Exp.Assign(p, Exp.Property(arg, "Item", Exp.Constant(i))));
            }

            PushBlock(def.Body, IL);

            // add the return label target
            IL.Add(Exp.Label(ret.Pop(), NoneExp));

            var body = Exp.Block(Local.Variables, IL);

            Local = loc;
            var func = Exp.Lambda<Func<Args, Object>>(body, arg).Compile();

            return new Function
            {
                Name = name,
                Parameters = ps,
                fx = func
            };
        }
    }
}