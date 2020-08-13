using System.Reflection;

namespace WC
{
    public static class MethodBaseExtensions
    {
        public static string GetName(this MethodBase methodBase)
        {
            if (methodBase == null)
            {
                return null;
            }
            else if (methodBase.DeclaringType.Name.Contains("<") && methodBase.DeclaringType.Name.Contains(">"))
            {
                int lowerIdx = methodBase.DeclaringType.Name.IndexOf("<");
                int higherIdx = methodBase.DeclaringType.Name.IndexOf(">");

                return methodBase.DeclaringType.Name.Substring(lowerIdx + 1, higherIdx - lowerIdx - 1);
            }
            else
            {
                return methodBase.Name;
            }
        }
    }
}
