using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Debug;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Monetizr.SDK.Utils
{
    public static class MonetizrUtils
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

            Regex regxComma = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            Regex regxColon = new Regex(":(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            string[] commaSplit = regxComma.Split(content);

            return regxComma.Split(content)
                            .Select(v => regxColon.Split(v))
                            .ToDictionary(v => v.First().Trim(trimmedChars), v => v.Last().Trim(trimmedChars));
        }
        
        public static StringBuilder UnescapeString(StringBuilder content)
        {
            return content.Replace("\\\"", "\"").Replace("\\\\", "\\");
        }

        public static string UnescapeJson(string json)
        {
            int bracketIndex = json.IndexOf("{", StringComparison.Ordinal);
            int quoteIndex = json.IndexOf('\"', bracketIndex);

            if (quoteIndex - bracketIndex == 1)
                return json;

            int escapeLevel = 0;
            for (int i = bracketIndex; i < quoteIndex; i++)
                if (json[i] == '\\')
                    escapeLevel++;

            if (escapeLevel == 0)
                return json;

            var sb = new StringBuilder(json);

            while (escapeLevel > 0)
            {
                sb = UnescapeString(sb);
                escapeLevel--;
            }

            return sb.ToString();
        }

        public static Dictionary<string, string> ParseJson(string content)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(content))
                return result;

            content = UnescapeJson(content);

            var root = SimpleJSON.JSON.Parse(content);

            foreach (var key in root)
            {
                var value = key.Value;
                var name = key.Key;
                
                if (value.IsString || value.IsNumber || value.IsBoolean)
                {
                    string v = key.Value.ToString();

                    if (value.IsString)
                        v = v.Trim('"');

                    result[name] = v;
                }

            }

            return result;
        }

        public static Dictionary<string, string> ParseContentString(string content)
        {
            var res = ParseJson(content);

            var res2 = new Dictionary<string, string>();

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

                    if (result.Equals(value))
                        break;

                    if (res.TryGetValue(result, out var re))
                    {
                        value = value.Replace($"%{result}%", re);
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

        public static List<TListType> CreateListFromArray<TArrayType, TListType>(TArrayType[] array, Func<TArrayType, TListType> convertToListType, TListType defaultElement)
        {
            var list = new List<TListType>();

            AddArrayToList(list, array, convertToListType, defaultElement);

            return list;
        }

        public static void AddArrayToList<TArrayType, TListType>(List<TListType> list, TArrayType[] array, Func<TArrayType, TListType> convertToListType, TListType defaultElement)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (convertToListType == null)
            {
                throw new ArgumentNullException(nameof(convertToListType));
            }

            if (array == null)
            {
                if (defaultElement != null)
                    list.Add(defaultElement);
            }
            else
            {
                int initialCount = list.Count;
                list.Capacity = initialCount + array.Length;

                foreach (var t in array)
                {
                    var e = convertToListType(t);

                    if (e != null)
                        list.Add(e);
                }
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
                MonetizrLogger.PrintError($"Exception in ExtractAllToDirectory. Extracting {zipPath} to directory {extractPath} failed with error\n{e}");

                return false;
            }
        }

        public static bool ValidateJson(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return false;

            int quoteCount = 0;
            int cbCount = 0;
            int sbCount = 0;

            foreach (char c in jsonString)
            {
                switch (c)
                {
                    case '\"': quoteCount++; break;
                    case '{': cbCount++; break;
                    case '}': cbCount--; break;
                    case '[': sbCount++; break;
                    case ']': sbCount--; break;
                }

                if (cbCount < 0)
                {
                    MonetizrLogger.Print("Curly bracket problem");
                    return false;
                }

                if (sbCount < 0)
                {
                    MonetizrLogger.Print("Square bracket problem");
                    return false;
                }
            }

            if (quoteCount % 2 != 0)
            {
                MonetizrLogger.Print($"Quote problem {quoteCount}");
            }

            if (cbCount != 0)
            {
                MonetizrLogger.Print("Curly bracket problem");
            }

            if (sbCount != 0)
            {
                MonetizrLogger.Print("Square bracket problem");
            }

            return quoteCount % 2 == 0 && cbCount == 0 && sbCount == 0;
        }

        public static string ConvertCreativeToExt(string type, string url)
        {
            if (Path.HasExtension(url))
            {
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

        public static Dictionary<string, string> ParseConditionsString(string conditionsString)
        {
            var output = new Dictionary<string, string>();

            var pairs = conditionsString.Split(';');
            
            foreach (var pair in pairs)
            {
                string[] parts = pair.Split('=');
                
                if (parts.Length == 2)
                {
                    output.Add(parts[0], parts[1]);
                }
            }

            return output;
        }
        
        private static readonly string[] ScoreNames = { "", "k", "M", "B", "T", "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj", "ak", "al", "am", "an", "ao", "ap", "aq", "ar", "as", "at", "au", "av", "aw", "ax", "ay", "az", "ba", "bb", "bc", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bk", "bl", "bm", "bn", "bo", "bp", "bq", "br", "bs", "bt", "bu", "bv", "bw", "bx", "by", "bz" };

        public static string ScoresToString(double scores)
        {
            int index = 0;

            while (Math.Abs(scores) >= 1000 && index < ScoreNames.Length - 1)
            {
                scores /= 1000;
                index++;
            }

            string scoreString = (scores % 1 == 0) ? 
                scores.ToString("N0", CultureInfo.InvariantCulture) : 
                scores.ToString("F1", CultureInfo.InvariantCulture);

            return scoreString + ScoreNames[index];
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
                    sb.Append("\\u");
                    sb.Append(((int)c).ToString("x4"));
                }
                else
                {
                    sb.Append(c);
                }
                
            }
            return sb.ToString();
        }

        public static string UnescapeString(string content)
        {
            return content.Replace("\\\"", "\"").Replace("\\\\", "\\");
        }

        public static string PrintDictionaryValuesInOneLine(Dictionary<string, string> dict)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var pair in dict)
            {
                sb.Append($"Key: {pair.Key}, Value: {pair.Value}; ");
            }

            if (sb.Length > 0) sb.Length -= 2;

            return sb.ToString();
        }

        public static string GetVideoPlayerURL (ServerCampaign serverCampaign)
        {
            string fallbackVideoPlayerURL = "https://image.themonetizr.com/videoplayer/html.zip";
            string globalSettingsVideoPlayerURL = serverCampaign.serverSettings.GetParam("videoplayer", "");
            if (string.IsNullOrEmpty(globalSettingsVideoPlayerURL))
            {
                MonetizrLogger.Print("VideoPlayer URL is from GlobalSettings.");
                return fallbackVideoPlayerURL;
            }
            else
            {
                MonetizrLogger.Print("VideoPlayer URL is from FallbackURL.");
                return globalSettingsVideoPlayerURL;
            }
        }

        public static string ExtractValueFromJSON(string jsonString, string parameter)
        {
            string key = $"\"{parameter}\"";

            int keyIndex = jsonString.IndexOf(key, StringComparison.Ordinal);
            if (keyIndex == -1)
            {
                return null;
            }

            int colonIndex = jsonString.IndexOf(':', keyIndex);
            if (colonIndex == -1)
            {
                return null;
            }

            int valueStartIndex = colonIndex + 1;
            while (valueStartIndex < jsonString.Length && char.IsWhiteSpace(jsonString[valueStartIndex]))
            {
                valueStartIndex++;
            }

            char startingChar = jsonString[valueStartIndex];
            bool isQuoted = startingChar == '"';
            int valueEndIndex;

            if (isQuoted)
            {
                valueEndIndex = valueStartIndex + 1;
                while (valueEndIndex < jsonString.Length)
                {
                    valueEndIndex = jsonString.IndexOf('"', valueEndIndex);
                    if (valueEndIndex == -1 || jsonString[valueEndIndex - 1] != '\\')
                        break;
                    valueEndIndex++;
                }

                if (valueEndIndex == -1)
                {
                    return null;
                }
            }
            else
            {
                char[] delimiters = { ',', '}', ']' };
                valueEndIndex = jsonString.IndexOfAny(delimiters, valueStartIndex);
                if (valueEndIndex == -1)
                {
                    valueEndIndex = jsonString.Length;
                }
            }

            string value = jsonString.Substring(valueStartIndex, valueEndIndex - valueStartIndex).Trim();

            if (isQuoted)
            {
                value = value.Trim('"');
            }

            value = value.Replace("\\/", "/").Replace("\\\"", "\"").Replace("\\\\", "\\");

            return value;
        }

        public static string ExtractNestedValue(string jsonString, string parentKey, string nestedKey)
        {
            string parentValue = ExtractValueFromJSON(jsonString, parentKey);

            if (parentValue == null)
                return null;

            return ExtractValueFromJSON(parentValue, nestedKey);
        }

    }
}
