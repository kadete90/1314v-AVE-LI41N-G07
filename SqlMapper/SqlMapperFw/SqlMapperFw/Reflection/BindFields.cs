using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlMapperFw.Reflection
{
    public class BindFields<T>
    {
        private readonly Dictionary<String, MemberInfo> FieldNamesDictionary;

        public BindFields(Dictionary<String, MemberInfo> fieldNamesDictionary)
        {
            FieldNamesDictionary = fieldNamesDictionary;
        }

        //retorna uma nova instância de T com valores obtidos da base de dados
        public T bind(object[] dbRowValues)
        {
            T TInstance = (T)Activator.CreateInstance(typeof(T));
            int i = 0;
            foreach (MemberInfo mi in FieldNamesDictionary.Values)
            {
                MemberInfo mymi = TInstance.GetType().GetMember(mi.Name)[0];
                if (mymi != null)
                {
                    object valueToInsert = dbRowValues[i++];
                    if (!ReferenceEquals(valueToInsert.ToString(), "" ))
                        mymi.SetValue(TInstance, valueToInsert);              
                }
            }
            return TInstance;
        }
    }
}
