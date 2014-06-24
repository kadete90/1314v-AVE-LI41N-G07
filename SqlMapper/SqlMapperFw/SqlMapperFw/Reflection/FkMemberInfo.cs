using System;
using System.Reflection;

namespace SqlMapperFw.Reflection
{
    //encapsular instâncias -> FK
    public class FkMemberInfo : MemberInfo
    {
        public Type InstanceType { get; private set; }
        public MemberInfo PkInfo { get; private set; }
        private String PkName { get; set; }
        public Object MyInstance { get; private set; }
        public MemberInfo ToBindInfo { get; private set; }

        public FkMemberInfo(MemberInfo memberInfo, Type instanceFrom)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Property:
                    InstanceType = ((PropertyInfo)memberInfo).PropertyType;
                    break;
                case MemberTypes.Field:
                    InstanceType = ((FieldInfo)memberInfo).FieldType;
                    break;
                default:
                    throw new ArgumentException("memberInfo.MemberType");
            }

            if (InstanceType != null) MyInstance = Activator.CreateInstance(InstanceType, null);
            InstanceType = MyInstance.GetType();
            ToBindInfo = memberInfo;
            PkInfo = InstanceType.GetPkMemberInfo();

            if (PkInfo == null)
                throw new Exception("Every entity must have a Primary Key!");

            if (instanceFrom != InstanceType)
            {
                DBNameAttribute fieldNameAttribute = (DBNameAttribute)PkInfo.GetCustomAttribute(typeof(DBNameAttribute));
                PkName = (fieldNameAttribute != null)
                    ? fieldNameAttribute.Name
                    : PkInfo.Name;
            }   
            else
                PkName = memberInfo.Name;

        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return InstanceType.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return InstanceType.GetCustomAttributes(attributeType, inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return PkInfo.IsDefined(attributeType, inherit);
        }

        public override MemberTypes MemberType
        {
            get { return PkInfo.MemberType; }
        }

        public override string Name
        {
            get { return PkName; }
        }

        public override Type DeclaringType
        {
            get { return PkInfo.DeclaringType; }
        }

        public override Type ReflectedType
        {
            get { return PkInfo.ReflectedType; }
        }
    }
}
