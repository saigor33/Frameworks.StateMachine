using System.Collections.Generic;

namespace Frameworks.StateMachine.StateGraphVisualizer
{
    public static class CommonSubstringFinder
    {
        public static string GetCommonSubstring(HashSet<string> strings, string commonSubstring = null)
        {
            foreach (string str in strings)
            {
                commonSubstring = commonSubstring == null
                    ? str
                    : GetCommonSubstring(commonSubstring, str);
            }

            return commonSubstring;
        }

        static string GetCommonSubstring(string str1, string str2)
        {
            int? lastMatchSubstringIndex = null;
            for (int i = 0; i < str1.Length; i++)
            {
                if (i >= str2.Length
                    || str1[i] != str2[i])
                {
                    break;
                }

                lastMatchSubstringIndex = i;
            }

            int commonSubstringLenght = lastMatchSubstringIndex.HasValue
                ? lastMatchSubstringIndex.Value + 1
                : 0;

            return str1.Substring(0, commonSubstringLenght);
        }
    }
}