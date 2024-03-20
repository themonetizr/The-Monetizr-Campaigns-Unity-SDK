using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monetizr.Campaigns
{
    abstract class TagsReplacer
    {
        private Dictionary<string, Func<string>> _urlModifiers = new Dictionary<string, Func<string>>();
        protected void SetModifiers(Dictionary<string, Func<string>> m)
        {
            _urlModifiers = m;
        }

        private List<string> FindMacrosInSquareBrackets(string input)
        {
            List<string> macros = new List<string>();

            int startIndex = -1;

            for (int i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case '[':
                        startIndex = i;
                        break;
                    case ']':
                        if (i > startIndex)
                            macros.Add(input.Substring(startIndex + 1, i - startIndex - 1));

                        break;
                }
            }

            return macros;
        }

        private string ReplaceMacros(string input, List<string> macrosList)
        {
            if (macrosList.Count == 0)
                return input;

            StringBuilder sb = new StringBuilder(input);

            foreach (var m in macrosList)
            {
                string replaceMacro = $"[{m}]";

                if (_urlModifiers.TryGetValue(m, out var f))
                {
                    sb.Replace(replaceMacro, f());
                }
                else
                {
                    sb.Replace(replaceMacro, UnknownModifier(m));
                }
            }

            return sb.ToString();
        }

        internal string ReplaceAngularMacros(string str)
        {            
            if (!str.Contains("<<") || !str.Contains(">>") )
                return str;
            
            str = str.Replace("<<","[").Replace(">>", "]");
            
            var macrosList = FindMacrosInSquareBrackets(str);
            str = ReplaceMacros(str, macrosList);
            return Uri.EscapeDataString(str);
        }
        
        
        internal string Replace(string url)
        {
            if (!url.Contains('.') || !url.Contains('[') )
                return url;

            var sb = new StringBuilder();

            int queryStringIndex = url.IndexOf('?');

            if (queryStringIndex < 0)
                return url;

            string queryString = url.Substring(queryStringIndex + 1);

            string[] keyValuePairs = queryString.Split('&');

            if (keyValuePairs.Length == 0)
                return url;

            StringBuilder result = new StringBuilder(url.Substring(0, queryStringIndex + 1));
            int pairNum = 0;

            foreach (string pair in keyValuePairs)
            {
                if (pairNum > 0)
                    result.Append("&");

                if (string.IsNullOrEmpty(pair))
                    return url;

                string[] keyValue = pair.Split('=');

                if (string.IsNullOrEmpty(keyValue[0]) || string.IsNullOrEmpty(keyValue[1]) || keyValue.Length != 2)
                    return url;

                string parameter = keyValue[0];
                string value = Uri.UnescapeDataString(keyValue[1]);

                var macrosList = FindMacrosInSquareBrackets(value);
                value = ReplaceMacros(value, macrosList);

                result.Append(parameter);
                result.Append("=");
                result.Append(Uri.EscapeDataString(value));

                pairNum++;
            }
            
            return result.ToString();
        }

        protected abstract string UnknownModifier(string tag);
    }
}
