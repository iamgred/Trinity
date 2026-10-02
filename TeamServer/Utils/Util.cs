//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace TeamServer.Utils
{
    public static class Util
    {
        public static string GetEnumValue<T>(T enumValue) where T : Enum
        {
            var type = enumValue.GetType();
            var memInfo = type.GetMember(enumValue.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(EnumMemberAttribute), false);
            return ((EnumMemberAttribute)attributes[0]).Value!;
        }

        public static T GetEnumString<T>(string value) where T : Enum
        {
            var type = typeof(T);
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute)) as EnumMemberAttribute;
                if (attribute != null && attribute.Value!.Equals(value))
                {
                    return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException($"Unkown value: {value}");
        }

        public static string GenerateAES256Key()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                return Convert.ToBase64String(aes.Key);
            }
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//