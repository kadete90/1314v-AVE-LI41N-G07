using System;
using System.Collections.Generic;
using System.Reflection;
using SqlMapperFw.Reflection;

namespace SqlMapperFw.DataMappers
{
    public class BindFields<T>
    {
        private readonly Dictionary<String, MemberInfo> FieldNamesDictionary;
        //private readonly T TInstance;

        public BindFields(Dictionary<String, MemberInfo> fieldNamesDictionary)
        {
            FieldNamesDictionary = fieldNamesDictionary;
            //TInstance = Activator.CreateInstance<T>();
        }

        //
        //public T matchFields(T instance)
        //{
            
        //}

        //retorna uma nova instância de T com valores obtidos da base de dados
        public T bind(object[] dbRowValues)
        {
            T TInstance = (T)Activator.CreateInstance(typeof(T), dbRowValues);
            //TODO
            //int i = 0;
            //foreach (var ED_Field in FieldNamesDictionary)
            //{
            //    TInstance.GetType().GetMember(ED_Field.Value.Name)[0].SetValue(ED_Field.Value, dbRowValues[i++]);
            //}
            return TInstance;
        }
    }
}
