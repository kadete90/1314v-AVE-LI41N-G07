using System;

namespace SqlMapperFw.Reflection
{
    public class ReflectionMethods
    {
        public static bool ImplementsInterface(Type t, Type tIntf)
        {
            if (t == null)
                throw new ArgumentNullException("t");
            if (tIntf == null)
                throw new ArgumentNullException("tIntf");
            return tIntf.IsInterface && t.IsClass && tIntf.IsAssignableFrom(t);
        }
    }
}
