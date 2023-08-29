using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using mixpanel;
using UnityEngine;
using static Monetizr.Campaigns.MonetizrUnitySurvey;

namespace Monetizr.Campaigns
{
    public static class Utils
    {
        public static Vector2 Abs(Vector2 v) => new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));

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

        public static Dictionary<string, string> _ParseJson(string content)
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

        public static Dictionary<string, string> ParseJson(string content)
        {
            var result = new Dictionary<string, string>();

            var root = SimpleJSON.JSON.Parse(content);

            foreach (var key in root)
            {
                var value = key.Value;
                var name = key.Key;

                if (value.IsString || value.IsNumber)
                {
                    string v = key.Value.ToString();

                    if (value.IsString)
                        v = v.Trim('"');

                    result[name] = v;

                    //Debug.LogError($"{name},{v}");
                }

            }

            return result;
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

        public static List<ListType> CreateListFromArray<ArrayType, ListType>(ArrayType[] array, Func<ArrayType, ListType> convertToListType, ListType defaultElement)
        {
            var list = new List<ListType>();

            AddArrayToList(list, array, convertToListType, defaultElement);

            return list;
        }

        public static void AddArrayToList<ArrayType, ListType>(List<ListType> list, ArrayType[] array, Func<ArrayType, ListType> convertToListType, ListType defaultElement)
        {
            if (array == null && defaultElement != null)
            {
                list.Add(defaultElement);
            }
            else
            {
                Array.ForEach(array,
                    (ArrayType elem) =>
                    {
                        var e = convertToListType(elem);

                        if (e != null)
                            list.Add(e);
                    });
            }
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static bool ExtractAllToDirectory(string zipPath, string extractPath)
        {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (!String.IsNullOrEmpty(entry.Name))
                            entry.ExtractToFile(Path.Combine(extractPath, entry.Name), true);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Log.PrintError($"Extract {zipPath} to directory {extractPath} failed with error {e.ToString()}");

                return false;
            }
        }

        public static bool TestJson(string jsonString)
        {
            int quoteCount = 0;
            int cbCount1 = 0;
            int cbCount2 = 0;

            foreach (char c in jsonString)
            {
                switch (c)
                {
                    case '\"': quoteCount++; break;
                    case '{': cbCount1++; break;
                    case '}': cbCount2++; break;
                    default: break;
                }
            }

            if (quoteCount % 2 != 0)
                return false;

            if (cbCount1 != cbCount2)
                return false;

            return true;
        }

        public static string ConvertCreativeToExt(string type, string url)
        {
            if (Path.HasExtension(url))
            {
                //remove starting dot
                return Path.GetExtension(url).Substring(1);
            }

            int i = type.LastIndexOf('/');

            return type.Substring(i + 1);
        }

        public static string ConvertCreativeToFname(string url)
        {
            int i = url.LastIndexOf('=');

            if (i <= 0)
                return Path.GetFileNameWithoutExtension(url);

            return url.Substring(i + 1);
        }

        public static List<string> SplitStringIntoPieces(string str, int pieceLen)
        {
            List<string> pieces = new List<string>();

            for (int i = 0; i < str.Length; i += pieceLen)
            {
                string piece = str.Substring(i, Math.Min(pieceLen, str.Length - i));
                pieces.Add(piece);
            }

            return pieces;
        }

        public static bool isInLandscapeMode()
        {
            return (Screen.width > Screen.height);
        }

        public static float SimpleTween(float k)
        {
            return 0.5f * (1f - Mathf.Cos(Mathf.PI * k));
        }

        public static string EncodeStringIntoAscii(string s)
        {
            if (s == null)
                return "";

            if (s.All(c => c < 128))
                return s;

            var sb = new StringBuilder();
            foreach (var c in s)
            {
                if (c > 127)
                {
                    var encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
