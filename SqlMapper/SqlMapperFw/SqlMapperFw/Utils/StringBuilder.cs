using System;
using System.Collections.Generic;

namespace SqlMapperFw.Utils
{
    public static class StringBuilder
    {

        public static string StringBuilderDicionary(this Dictionary<string, PairInfoBind> dictionary)
        {
            //"EmployeeName = @name"
            String s = "";
            foreach (KeyValuePair<string, PairInfoBind> mi in dictionary)
                s += mi.Key + "=@" + mi.Value.MemberInfo.Name + ", ";
            if (s != "")
                s = s.Substring(0, s.Length - 2);
            return s;
        }

        public static string StringBuilderKeyCollection(this Dictionary<string, PairInfoBind>.KeyCollection keyCollection)
        {
            String s = "";
            foreach (String fieldName in keyCollection)
                s += "@" + fieldName + ", ";
            if (s != "")
                s = s.Substring(0, s.Length - 2);
            return s;
        }
    }
}
