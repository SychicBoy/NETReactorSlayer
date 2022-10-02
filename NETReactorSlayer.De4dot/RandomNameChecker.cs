using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NETReactorSlayer.De4dot
{
    public static class RandomNameChecker
    {
        public static bool IsNonRandom(string name)
        {
            if (name.Length < 5)
                return true;
            if (NoUpper.IsMatch(name))
                return true;
            if (AllUpper.IsMatch(name))
                return true;

            for (var i = 0; i < name.Length - 1; i++)
            {
                if (IsDigit(name[i]))
                    return false;
                if (i > 0 && IsUpper(name[i]) && IsUpper(name[i - 1]))
                    return false;
            }

            var words = GetCamelWords(name);
            var vowels = 0;
            foreach (var word in words)
                if (word.Length > 1 && HasVowel(word))
                    vowels++;
            switch (words.Count)
            {
                case 1:
                    return vowels == words.Count;
                case 2:
                case 3:
                    return vowels >= 1;
                case 4:
                case 5:
                    return vowels >= 2;
                case 6:
                    return vowels >= 3;
                case 7:
                    return vowels >= 4;
                default:
                    return vowels >= words.Count - 4;
            }
        }

        private static bool HasVowel(string s)
        {
            foreach (var c in s)
                switch (c)
                {
                    case 'A':
                    case 'a':
                    case 'E':
                    case 'e':
                    case 'I':
                    case 'i':
                    case 'O':
                    case 'o':
                    case 'U':
                    case 'u':
                    case 'Y':
                    case 'y':
                        return true;
                }

            return false;
        }

        private static List<string> GetCamelWords(string name)
        {
            var words = new List<string>();
            var sb = new StringBuilder();

            foreach (var c in name)
            {
                if (IsUpper(c))
                {
                    if (sb.Length > 0)
                        words.Add(sb.ToString());
                    sb.Length = 0;
                }

                sb.Append(c);
            }

            if (sb.Length > 0)
                words.Add(sb.ToString());

            return words;
        }

        public static bool IsRandom(string name)
        {
            var len = name.Length;
            if (len < 5)
                return false;

            var typeWords = GetTypeWords(name);

            if (CountNumbers(typeWords, 2))
                return true;

            CountTypeWords(typeWords, out var upper);
            if (upper >= 3)
                return true;
            var hasTwoUpperWords = upper == 2;

            foreach (var word in typeWords)
                if (word.Length > 1 && IsDigit(word[0]))
                    return true;

            for (var i = 2; i < typeWords.Count; i++)
                if (IsDigit(typeWords[i - 1][0]) && IsLower(typeWords[i - 2][0]) && IsLower(typeWords[i][0]))
                    return true;

            if (hasTwoUpperWords && HasDigit(name))
                return true;

            if (IsLower(name[len - 3]) && IsUpper(name[len - 2]) && IsDigit(name[len - 1]))
                return true;

            return false;
        }

        private static bool HasDigit(string s)
        {
            foreach (var c in s)
                if (IsDigit(c))
                    return true;
            return false;
        }

        private static List<string> GetTypeWords(string s)
        {
            var words = new List<string>();
            var sb = new StringBuilder();

            for (var i = 0; i < s.Length;)
                if (IsDigit(s[i]))
                {
                    sb.Length = 0;
                    while (i < s.Length && IsDigit(s[i]))
                        sb.Append(s[i++]);
                    words.Add(sb.ToString());
                }
                else if (IsUpper(s[i]))
                {
                    sb.Length = 0;
                    while (i < s.Length && IsUpper(s[i]))
                        sb.Append(s[i++]);
                    words.Add(sb.ToString());
                }
                else if (IsLower(s[i]))
                {
                    sb.Length = 0;
                    while (i < s.Length && IsLower(s[i]))
                        sb.Append(s[i++]);
                    words.Add(sb.ToString());
                }
                else
                {
                    sb.Length = 0;
                    while (i < s.Length)
                    {
                        if (IsDigit(s[i]) || IsUpper(s[i]) || IsLower(s[i]))
                            break;
                        sb.Append(s[i++]);
                    }

                    words.Add(sb.ToString());
                }

            return words;
        }

        private static bool CountNumbers(List<string> words, int numbers)
        {
            var num = 0;
            foreach (var word in words)
            {
                if (string.IsNullOrEmpty(word))
                    continue;
                if (IsDigit(word[0]) && ++num >= numbers)
                    return true;
            }

            return false;
        }

        private static void CountTypeWords(List<string> words, out int upper)
        {
            upper = 0;

            foreach (var c in from word in words where word.Length > 1 select word[0])
                if (IsDigit(c))
                {
                }
                else if (IsLower(c))
                {
                }
                else if (IsUpper(c)) upper++;
        }

        private static bool IsLower(char c) => 'a' <= c && c <= 'z';

        private static bool IsUpper(char c) => 'A' <= c && c <= 'Z';

        private static bool IsDigit(char c) => '0' <= c && c <= '9';

        private static readonly Regex AllUpper = new Regex(@"^[A-Z]+$");

        private static readonly Regex NoUpper = new Regex(@"^[^A-Z]+$");
    }
}