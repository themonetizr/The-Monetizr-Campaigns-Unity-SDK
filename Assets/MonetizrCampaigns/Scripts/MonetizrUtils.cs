using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Monetizr.Campaigns.MonetizrUnitySurvey;

namespace Monetizr.Campaigns
{
    public static class Utils
    {
        public static int[] ConvertToIntArray(string s, char delimeter = '.')
        {
            return Array.ConvertAll(s.Split(delimeter), (v) => { int k = 0; return int.TryParse(v, out k) ? k : 0; });
        }

        public static int CompareVersions(string First, string Second)
        {
            var f = ConvertToIntArray(First);
            var s = ConvertToIntArray(Second);

            for (int i = 0; i < 3; i++)
            {
                int f_i = 0;

                if (f.Length > i)
                    f_i = f[i];

                int s_i = 0;

                if (s.Length > i)
                    s_i = s[i];

                if (f_i > s_i)
                    return 1;

                if (f_i < s_i)
                    return -1;
            }

            return 0;
        }

        public static Dictionary<string, string> ParseJson(string content)
        {
            content = content.Trim(new[] { '{', '}' }).Replace('\'', '\"');

            var trimmedChars = new[] { ' ', '\"' };

            //regex to split only unquoted separators
            Regex regxComma = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            Regex regxColon = new Regex(":(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            string[] commaSplit = regxComma.Split(content);

            return regxComma.Split(content)
                            .Select(v => regxColon.Split(v))
                            .ToDictionary(v => v.First().Trim(trimmedChars), v => v.Last().Trim(trimmedChars));
        }

        //Unity FromJson doesn't support Dictionaries
        public static Dictionary<string, string> ParseContentString(string content, Dictionary<string, object> dict = null)
        {
            Dictionary<string, string> res = null;

            if (dict != null)
            {
                res = new Dictionary<string, string>();

                foreach (KeyValuePair<string, object> kvp in dict)
                {
                    //Log.Print($"-----{kvp.Key} {(string)kvp.Value}");

                    res.Add(kvp.Key, (string)kvp.Value);
                }
            }
            else
            {
                res = ParseJson(content);
            }

            Dictionary<string, string> res2 = new Dictionary<string, string>();

            foreach (var p in res)
            {
                string value = p.Value;
                string key = p.Key;

                for (int i = 0; i < 5; i++)
                {
                    int startId = value.IndexOf('%');

                    if (startId == -1)
                        break;

                    int endId = value.IndexOf('%', startId + 1);

                    if (endId == -1)
                        break;

                    string result = value.Substring(startId + 1, endId - startId - 1);

                    //Log.Print($"-----{startId} {endId} {result}");

                    if (res.ContainsKey(result))
                    {
                        value = value.Replace($"%{result}%", res[result]);
                        //Log.Print($"-----replace {result} {res[result]}");
                    }

                }

                res2.Add(key, value);
            }

            return res2;
        }

        public static void ShuffleList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var temp = list[i];
                int randomIndex = UnityEngine.Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        public static List<ListType> ArrayToList<ArrayType, ListType>(ArrayType[] array, Func<ArrayType, ListType> convertToListType, ListType defaultElement)
        {
            var list = new List<ListType>();

            if (array == null && defaultElement != null)
            {
                list.Add(defaultElement);
            }
            else
            {
                Array.ForEach(array, (ArrayType elem) => { list.Add(convertToListType(elem)); });
            }

            return list;
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static void ExtractAllToDirectory(string zipPath, string extractPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!String.IsNullOrEmpty(entry.Name))
                        entry.ExtractToFile(Path.Combine(extractPath, entry.Name));
                }
            }
        }
    }
}
