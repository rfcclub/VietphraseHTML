using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VietphraseMixHTML
{
    public class GlobalCache
    {
        public static Dictionary<string, string> ChinesePhienAmWords
        {
            get; set;
        }

        public static Dictionary<string, string> Names
        {
            get;
            set;
        }
        public static Dictionary<string, string> ThanhNgu
        {
            get;
            set;
        }

        public static Dictionary<string, string> VietPhrase
        {
            get;
            set;
        }
        public static Dictionary<string, string> Downloaders
        {
            get;
            set;
        }
        public static Dictionary<string, string> DownloaderSignatures
        {
            get;
            set;
        }

        public static IList<string> UTF8Sites { get; set; }
        public static string VietPhrasePattern { get; set; }
        public static string ChinesePhienAmPattern { get; set; }
        public static string NamesPattern { get; set; }

        public static string TemplatePath {get;set; }

        public static void Init(string basePath)
        {
            ChinesePhienAmWords = new Dictionary<string, string>();
            Names = new Dictionary<string, string>();
            ThanhNgu = new Dictionary<string, string>();
            VietPhrase = new Dictionary<string, string>();
            UTF8Sites = new List<string>();
            Downloaders = new Dictionary<string, string>();
            DownloaderSignatures = new Dictionary<string, string>();

            string dir = basePath.Trim() + "\\data\\";
            TemplatePath = basePath + "\\template\\";
            ReadFileToList(ChinesePhienAmWords, dir + "ChinesePhienAmWords.txt");
            ReadFileToList(Names, dir + "Names.txt");
            ReadFileToList(ThanhNgu, dir + "ThanhNgu.txt");
            ReadFileToList(VietPhrase, dir + "VietPhrase.txt");
            ReadFileToList(UTF8Sites,dir + "utf8website.txt");
            ReadConfigToList(Downloaders, dir + "DownloaderConfiguration.txt");
            ReadConfigToList(DownloaderSignatures, dir + "DownloaderSignature.txt");
            BuildPattern();
            
        }

        private static void BuildPattern()
        {
            ChinesePhienAmPattern = "(" + String.Join("|", ChinesePhienAmWords.Keys.ToArray()) + ")";
            VietPhrasePattern = "(" + String.Join("|", VietPhrase.Keys.ToArray()) + ")";
            NamesPattern = "(" + String.Join("|", Names.Keys.ToArray()) + ")";
        }

        private static void ReadFileToList(IList<string> list, string s)
        {
            StreamReader stream = new StreamReader(File.OpenRead(s), Encoding.UTF8);

            while (!stream.EndOfStream)
            {
                bool found = false;
                string line = stream.ReadLine();
                foreach (string s1 in list)
                {
                    if(s1.Equals(line))
                    {
                        found = true;
                        break;
                    }
                }
                if(!found) list.Add(line);
            }
            stream.Close();
        }

        private static void ReadFileToList(Dictionary<string, string> dictionary, string s)
        {
            StreamReader stream = new StreamReader(File.OpenRead(s),Encoding.UTF8);
            
            while(!stream.EndOfStream)
            {
                string line = stream.ReadLine();
                string[] lines = line.Split('=');
                if(lines.Length != 2 )
                {
                    continue;
                }
                if (!dictionary.ContainsKey(lines[0]))
                {
                    // LAY VIETPHRASE 1 NGHIA
                    string[] vp2Nghia = lines[1].Split('/');
                    if (vp2Nghia.Length < 2)
                    {
                        // Kiem tra coi co dau :
                        string[] tuDongNghia = lines[1].ToString().Split(':');
                        if (tuDongNghia.Length < 2)
                        {
                            dictionary.Add(lines[0], lines[1]);
                        }
                        else
                        {
                            dictionary.Add(lines[0], tuDongNghia[0]);
                        }
                    }
                    else
                    {
                        // Kiem tra coi co dau :
                        string[] tuDongNghia = vp2Nghia[0].Split(':');
                        if(tuDongNghia.Length < 2)
                        {
                            dictionary.Add(lines[0], vp2Nghia[0]);
                        }
                        else
                        {
                            dictionary.Add(lines[0], tuDongNghia[0]);
                        }
                    }
                }
            }
            stream.Close();
        }

        private static void ReadConfigToList(Dictionary<string, string> dictionary, string s)
        {
            if(dictionary == null) dictionary = new Dictionary<string, string>();
            dictionary.Clear();
            StreamReader stream = new StreamReader(File.OpenRead(s), Encoding.UTF8);
            while (!stream.EndOfStream)
            {
                string line = stream.ReadLine();
                string[] lines = line.Split('=');
                if (lines.Length != 2)
                {
                    continue;
                }
                if (!dictionary.ContainsKey(lines[0]))
                {
                    dictionary.Add(lines[0].Trim(), lines[1].Trim());
                }
            }
            stream.Close();
        }
    }
}
