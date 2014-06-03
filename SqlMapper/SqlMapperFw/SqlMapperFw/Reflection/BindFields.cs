using System;
using System.Collections.Generic;

namespace SqlMapperFw.Reflection
{
    public class BindFields<T>
    {

        public BindFields()
        {
        
        }


        public T bind(T instance, object[] dbField, List<string> type)
        {

            foreach(var x in dbField)
            {
                Console.WriteLine(x);
            }
            return instance;
        }
    }
}
