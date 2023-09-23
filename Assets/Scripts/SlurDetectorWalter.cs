using System.Collections.Generic;
using Universal.Phonetic.Metaphone;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.IO;


public class SlurData
{
    public string MatchedPhrase { get; set; }
    public string MatchedPhonetic { get; set; }

    public SlurData(string matchedPhrase, string matchedPhonetic)
    {
        MatchedPhrase = matchedPhrase;
        MatchedPhonetic = matchedPhonetic;
    }
}

public class PhoneticData
{
    public string Word { get; set; }
    public string Phonetic { get; set; }

    public PhoneticData(string word, string phonetic)
    {
        Word = word;
        Phonetic = phonetic;
    }
}

public class SlurDetectorWalter : MonoBehaviour
{

    //load Black and White List
    public static string[] WhiteList { get; } = LoadWhiteList();
    private static string[] LoadWhiteList()
    {
        string line = File.ReadAllText("slurfilterLists/whitelist.txt");
        return line.Split(',');
    }
    public static string[] BlackList { get; } = LoadBlackList();
    private static string[] LoadBlackList()
    {
        string line = File.ReadAllText("slurfilterLists/blacklist.txt");
        return line.Split(',');
    }

    //prefilter with regex,since its cheapest, spaces dont trick it
    public static string RegrexFNSlurFilterPrecheck(string input)
    {
        string FNslurRegexPattern = @"\b(f\s*[@aeou]+\s*[ckgh]*\s*|f\s*[@aeou]+\s*[ckgh]*\s*[s]|f\s*[@aeou]+\s*[ckgh]*\s*[@aeiou]*\s*t*|n\s*[ai1l]+\s*[gk]+\s*[@aeiou]+\s*[rs]*)\b";
        return Regex.Replace(input, FNslurRegexPattern, "NOPE", RegexOptions.IgnoreCase);
    }

    //prefilter with blacklist
    public static string blacklistFilter(string input)
    {
        foreach (var word in BlackList)
        {
            input = input.Replace(word, "NOPE", StringComparison.OrdinalIgnoreCase);
        }
        return input;
    }

    private static object slurLogFileLock = new object();
    private static string slurLogFilePath = "slurfilterLists/slurRepalceLog.txt";
    static void LogSlurReplacementToFile(string message)
    {
        lock (slurLogFileLock)
        {
            // Append a line to the file
            File.AppendAllText(logFilePath, message + Environment.NewLine);
        }
    }

    // call slurCheck with topic or scene, replaces with NOPE
    public string RemoveSlurs(string input)
    {
        input = RegrexFNSlurFilterPrecheck(input);
        input = blacklistFilter(input);

        List<SlurData> slurData = GetSlurData(input);

        foreach (SlurData slur in slurData)
        {
            if (!WhiteList.Contains(slur.MatchedPhrase) && slur.MatchedPhrase.Length > 3)
            {
                LogSlurReplacementToFile($"Removed {slur.MatchedPhrase} | {slur.MatchedPhonetic}");
                input = input.Replace(slur.MatchedPhrase, "nope", StringComparison.OrdinalIgnoreCase);
            }
        }
        return output;
    }

    public static List<SlurData> GetSlurData(string englishWords)
    {
        List<SlurData> slurData = new List<SlurData>();

        string[] words = englishWords.Split(' ', '\n');

        (List<PhoneticData> phoneticData, string inputAsPhonetics, string inputAsPhoneticsWithoutSpaces) = Meta3PhonicateVowels(words);

        foreach (PhoneticData data in phoneticData)
        {
            //Debug.Log($"Phonetic for {data.Word} is {data.Phonetic}");
            if (KnownSlursPhonetic.Any(s => s == data.Phonetic))
            {
                if (data.Word == string.Empty) continue;
                slurData.Add(new SlurData(data.Word, data.Phonetic));
            }
        }

        // This is to get any multi-length ones like the knee ones and in-betweens
        foreach (string slurPhonetic in KnownSlursPhonetic)
        {
            int index = inputAsPhonetics.IndexOf(slurPhonetic);
            if (index != -1)
            {
                int numSpaces = GetNumSpacesBefore(inputAsPhonetics, index);
                string multi = string.Empty;
                string[] phones = slurPhonetic.Split(" ");

                int i = 0;
                foreach (string phone in phones)
                {
                    PhoneticData data = phoneticData[numSpaces + i];
                    i++;
                    if (data == null) continue;
                    multi += data.Word + " ";
                }
                slurData.Add(new SlurData(multi.Trim(), slurPhonetic));
            }
        }

        // This is to check for slurs with spaces between them
        foreach (string phonetic in KnownSlursPhonetic)
        {
            string slurPhonetic = Regex.Replace(phonetic, @"\s+", string.Empty);

            int index = inputAsPhoneticsWithoutSpaces.IndexOf(slurPhonetic);
            if (index != -1)
            {
                // At this point we know it's in there, but we gotta brute force it to find it, this is the last resort
                slurData.AddRange(FindSpacedSlurFromPhonetics(slurPhonetic, phoneticData));
            }
        }

        // Most slurs we care about have a shorter and longer version, we need to replace longer versions first.
        slurData.Sort((a, b) => b.MatchedPhrase.Length - a.MatchedPhrase.Length);

        return slurData;
    }

    private static List<SlurData> FindSpacedSlurFromPhonetics(string slurPhoneticWithoutSpaces, List<PhoneticData> phoneticData)
    {
        List<SlurData> slurData = new List<SlurData>();
        string word = string.Empty;
        char[] chars = slurPhoneticWithoutSpaces.ToCharArray();
        List<string> remainingChars = chars.Select(c => c.ToString()).ToList();
        foreach (PhoneticData data in phoneticData)
        {
            string toCheck = remainingChars[0];
            for (int i = 1; i < data.Phonetic.Length; i++)
            {
                if (i >= remainingChars.Count) break;
                toCheck += remainingChars[i];
            }

            if (data.Phonetic == toCheck)
            {
                word += data.Word + " ";
                remainingChars.RemoveRange(0, data.Phonetic.Length);
                if (remainingChars.Count == 0)
                {
                    slurData.Add(new SlurData(word.Trim(), slurPhoneticWithoutSpaces));
                    remainingChars = chars.Select(c => c.ToString()).ToList();
                    word = string.Empty;
                }
            }
            else
            {
                remainingChars = chars.Select(c => c.ToString()).ToList();
                word = string.Empty;
            }
        }

        return slurData;
    }

    private static int GetNumSpacesBefore(string str, int index)
    {
        int c = 0;
        for (int i = 0; i < index; i++)
        {
            if (i >= str.Length) break;
            if (str[i] == ' ') c++;
        }
        return c;
    }

    private static List<string> KnownSlursPhonetic = new List<string>
        {
            "FAJAT",
            "FARJAT",

            "NKR",
            "NKA",
            "NAKA",
            "NKKA",
            "NKKR",
            "NAKKAR",
            "NAKKA",
            "NAKAR",
            "NATAR",

            "NJR",
            "NJA",
            "NAJA",
            "NJJA",
            "NJJR",
            "NAJJA",

            "NA KRA",
            "NA KAR",
            "NA JAR",

            "NKA",
            "NKR",
            "NAKAR",
            "FKAT"
        };

    public static (List<PhoneticData>, string, string) Meta3PhonicateVowels(string[] words)
    {
        var mV3 = new Metaphone3();

        List<PhoneticData> phoneticData = new List<PhoneticData>();
        string phonetics = string.Empty;
        string phoneticsWithoutSpaces = string.Empty;

        List<string> encodedWords = new List<string>();
        int processedCharacters = 0;
        foreach (var word in words)
        {
            encodedWords.Add(word);
            mV3.SetWord(word);
            mV3.SetEncodeVowels(true);
            mV3.Encode();
            string phone = mV3.GetMetaph();
            phoneticData.Add(new PhoneticData(word, phone));
            phonetics += phone + " ";
            phoneticsWithoutSpaces += phone;
            processedCharacters += word.Length;
            //// We need to pad each as if it were its own character
            //while (phoneticData.Count < processedCharacters) phoneticData.Add(new PhoneticData());
            //while (phoneticsWithoutSpaces.Length < processedCharacters) phoneticsWithoutSpaces += "|";
        }

        return (phoneticData, phonetics.Trim(), phoneticsWithoutSpaces.Trim());
    }
}