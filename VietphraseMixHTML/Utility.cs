using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace VietphraseMixHTML
{
    public class Utility
    {

        public static void MultipleReplace(ref StringBuilder text, Dictionary<string, string> replacements)
        {
            string test = text.ToString();
            test = Regex.Replace(test,
                                 "(" + String.Join("|", replacements.Keys.ToArray()) + ")",
                                 delegate (Match m) { return replacements[m.Value] + " "; }
                );
            text = new StringBuilder(test);
        }

        public static string MultipleReplace(string text, Dictionary<string, string> replacements)
        {
            return Regex.Replace(text,
                                 "(" + String.Join("|", replacements.Keys.ToArray()) + ")",
                                 delegate (Match m) { return replacements[m.Value]; }
                );
        }

        public static string StripHTML(string source)
        {
            try
            {
                string result = source;


                // Remove HTML Development formatting
                // Replace line breaks with space
                // because browsers inserts space
                result = source.Replace('\r', ' ');
                // Replace line breaks with space
                // because browsers inserts space
                result = result.Replace('\n', ' ');
                // Remove step-formatting
                result = result.Replace('\t', ' ');

                // lower case before perform next
                //result = result.ToLower();
                // Remove repeating spaces because browsers ignore them
                result = System.Text.RegularExpressions.Regex.Replace(result,
                                                                      @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*head([^>])*>", "<head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*head( )*>)", "</head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<head>).*(</head>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*script([^>])*>", "<script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*script( )*>)", "</script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
                //         string.Empty,
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                /*result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<script[\s\S]*</script([\s\S]*)>", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);*/

                /*int scriptTagIndex = result.IndexOf("<script");
                while (scriptTagIndex > 0)
                {
                    result = result.Remove(scriptTagIndex, result.IndexOf("/script>") - scriptTagIndex + 8);
                    scriptTagIndex = result.IndexOf("<script");
                }*/
                result = System.Text.RegularExpressions.Regex.Replace(result,
                                                                          @"(\<script).*?(\/script\>)",
                                                                          string.Empty,
                                                                          System.Text.RegularExpressions.RegexOptions.
                                                                              IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*style([^>])*>", "<style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*style( )*>)", "</style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<style>).*(</style>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert tabs in spaces of <td> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*td([^>])*>", "\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI> tags
                // Check if there are line breaks (<br>) or paragraph (<p>)
                result = result.Replace("<br>", "\r<br>");
                result = result.Replace("<br ", "\r<br ");
                result = result.Replace("<p>", "\r<p>");
                result = result.Replace("<p ", "\r<p ");
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*br( )*>", "\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*li( )*>", "\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line paragraphs (double line breaks) in place
                // if <P>, <DIV> and <TR> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*div([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*tr([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*p([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // Remove remaining tags like <a>, links, images,
                // comments etc - anything that's enclosed inside < >
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<[^>]*>", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // replace special characters:
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @" ", " ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&bull;", " * ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lsaquo;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&rsaquo;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&trade;", "(tm)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&frasl;", "/",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lt;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&gt;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&copy;", "(c)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&reg;", "(r)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&(.{2,6});", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // for testing
                //System.Text.RegularExpressions.Regex.Replace(result,
                //       this.txtRegex.Text,string.Empty,
                //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // make line breaking consistent
                result = result.Replace("\n", "\r");

                // Remove extra line breaks and tabs:
                // replace over 2 breaks with 2 and over 4 tabs with 4.
                // Prepare first to remove any whitespaces in between
                // the escaped characters and remove redundant tabs in between line breaks
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\t)", "\t\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\r)", "\t\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\t)", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove redundant tabs
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove multiple tabs following a line break with just one tab
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Initial replacement target string for line breaks
                string breaks = "\r\r\r";
                // Initial replacement target string for tabs
                string tabs = "\t\t\t\t\t";
                // replace extra \r\n . 
                // patch : up to 8 char
                //for (int index = 0; index < result.Length; index++)
                for (int index = 0; index < 5; index++)
                {
                    result = result.Replace(breaks, "\r\r");
                    result = result.Replace(tabs, "\t\t\t\t");
                    breaks = breaks + "\r";
                    tabs = tabs + "\t";
                }
                // patch : replace CR with CR + LF
                result = result.Replace("\r", System.Environment.NewLine);

                result = result.Replace('\uff0c', ',');
                result = result.Replace('\uff01', '!');
                result = result.Replace('\uff08', '(');
                result = result.Replace('\uff09', ')');
                result = result.Replace('\uff1a', ':');
                result = result.Replace('\uff1b', ';');
                result = result.Replace('\uff1f', '?');
                result = result.Replace('\uff5e', '~');
                result = result.Replace('\u2026', '.');
                result = result.Replace('\u201c', '"');
                result = result.Replace('\u201d', '"');
                result = result.Replace('\u203b', '*');
                result = result.Replace('\u3000', ' ');
                result = result.Replace('\u3001', ',');
                result = result.Replace('\u3002', '.');
                result = result.Replace('\u300a', '(');
                result = result.Replace('\u300b', ')');



                // That's it.
                return result;
            }
            catch
            {
                //MessageBox.Show("Error");
                return source;
            }
        }

        internal static string CleanContent(string original, FictionObject fictionObject)
        {
            if (fictionObject.HTMLLink.IndexOf("piaotian") >= 0
                || fictionObject.HTMLLink.IndexOf("69shu") >= 0)
            {
                int start = original.IndexOf("&nbsp;&nbsp;&nbsp;&nbsp;");
                // cannot found text, return as normal strip
                if(start == -1) return StripHTML(original);
                int end = original.IndexOf("</div>", start);
                if(end == -1)
                {
                    end = original.LastIndexOf(@"&nbsp;&nbsp;&nbsp;&nbsp;");
                    end = original.IndexOf("\n",end);
                }
                string subContent = original.Substring(start, end-start);
                return StripHTML(subContent);
            }
            else if(fictionObject.HTMLLink.IndexOf("quledu") >= 0)
            {
                string startSign = "<div id=\"htmlContent\" class=\"contentbox\"";
                int start = original.IndexOf(startSign);                
                if (start != -1)
                {
                    int end = original.IndexOf("</div>", start);
                    if (end != -1)
                    {
                        string subContent = original.Substring(start, end - start);
                        return StripHTML(subContent.Substring(startSign.Length));
                    }
                    else
                    {
                        return StripHTML(original);
                    }
                } 
                else
                {
                    return StripHTML(original);
                }
            }
            else if(fictionObject.HTMLLink.IndexOf("uukanshu") > 0)
            {
                HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(original);
                var node = htmlDocument.DocumentNode.SelectSingleNode("//div[@id=\"contentbox\"]");
                if(node != null)
                {
                    string text = node.InnerHtml;
                    text = text.Replace("<br/>", "\r\n");
                    
                    return StripHTML(text);
                }
                else
                {
                    return StripHTML(original);
                }
            }
            else if (fictionObject.HTMLLink.IndexOf("17k.com") > 0)
            {
                HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(original);
                var node = htmlDocument.DocumentNode.SelectSingleNode("//div[@id=\"chapterContentWapper\"]");
                if (node != null)
                {
                    string text = node.InnerText;
                    text = text.Replace("<br/>", "\r\n");
                    text = text.Replace("< br />", "\r\n");
                    return StripHTML(text);
                }
                else
                {
                    return StripHTML(original);
                }
            }
            else
            {
                return StripHTML(original);
            }
        }

        public static string ReplaceEx(string original,
                    string pattern, string replacement)
        {
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();
            int inc = (original.Length / pattern.Length) *
                      (replacement.Length - pattern.Length);
            char[] chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern,
                                              position0)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                    chars[count++] = original[i];
                for (int i = 0; i < replacement.Length; ++i)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }
            if (position0 == 0) return original;
            for (int i = position0; i < original.Length; ++i)
                chars[count++] = original[i];
            return new string(chars, 0, count);
        }


        /// <summary>
        /// Remove HTML from string with Regex.
        /// </summary>
        public static string StripTagsRegex(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }

        /// <summary>
        /// Compiled regular expression for performance.
        /// </summary>
        static Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

        /// <summary>
        /// Remove HTML from string with compiled Regex.
        /// </summary>
        public static string StripTagsRegexCompiled(string source)
        {
            return _htmlRegex.Replace(source, string.Empty);
        }

        /// <summary>
        /// Remove HTML tags from string using char array.
        /// </summary>
        public static string StripTagsCharArray(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        public delegate void EmptyDelegate();
        public static void TryActionHelper(EmptyDelegate method, int retryCount)
        {
            bool retry = false;
            do
            {
                try
                {
                    method.Invoke();
                    retry = false;
                }
                catch (Exception ex)
                {
                    retry = (--retryCount) > 0 ? true : false;
                    Console.WriteLine(ex.Message);
                }
            } while (retry);
        }

        public static bool IsUtf8Site(string htmlLink)
        {
            foreach (string utf8Site in GlobalCache.UTF8Sites)
            {
                if (htmlLink.IndexOf(utf8Site) >= 0) return true;
            }
            return false;
        }

        public static string CleanZongHeng(string content)
        {
            content = System.Text.RegularExpressions.Regex.Replace(content,
                    @"[\!\*|\-|\@|\#|\?|\=|\~|\%|\#|\|*|\@|\?|\&|\^|\$|\^|\-|\@|\^|\+|\%]{5}.*[\!\*|\-|\@|\#|\?|\=|\~|\%|\#|\|*|\@|\?|\&|\^|\$|\^|\-|\@|\^|\+|\%]{5}",
                    "", RegexOptions.IgnoreCase);

            content = content.Replace(@"quyển sách tung hoành Trung văn lưới thủ phát ,hoan nghênh đọc giả đăng lục www.zongheng.comxem xét càng nhiều  vĩ đại  tác phẩm .", "");
            content = content.Replace(@"ngài đích trương mục hơn ngạch vì :0cá tung hoành tệ ta nếu sung trị giá |miễn phí thu được  tung hoành tệ   100tung hoành tệ  366tung hoành tệ  666tung hoành tệ  888tung hoành tệ  3666tung hoành tệ  6666tung hoành tệ  8888tung hoành tệ  10000tung hoành tệ  66666tung hoành tệ  100000tung hoành tệ", "");
            content = content.Replace("\t\t\t", "");
            content = content.Replace("\n\n\n", "\n\n");
            return content;
        }

        public static string NormalizeName(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var builder = new StringBuilder(value);
            foreach (string normalString in GlobalCache.normalStrings)
            {
                for (int i = 1; i < normalString.Length; i++)
                {
                    builder.Replace(normalString[i], normalString[0]);
                }
            }
            builder.Replace(" ", string.Empty);
            return builder.ToString();
        }
    }
}
