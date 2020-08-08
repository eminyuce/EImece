using System;
using System.Text.RegularExpressions;
using System.Web;

using TidyManaged;

namespace EImece.Domain.Helpers
{
    public class TidyManagedHtmlHelper
    {
        public static string ClearHTML(string html)
        {
            string ret = html;
            try
            {
                using (Document doc = Document.FromString(html))
                {
                    doc.ShowWarnings = false;
                    doc.Quiet = true;
                    doc.OutputXhtml = true;

                    doc.AddTidyMetaElement = false;
                    doc.DocType = DocTypeMode.Strict;
                    doc.DropEmptyParagraphs = true;
                    doc.DropFontTags = true;
                    doc.DropProprietaryAttributes = true;
                    doc.RemoveComments = true;
                    doc.FixAttributeUris = true;
                    doc.FixUrlBackslashes = true;
                    doc.IndentBlockElements = AutoBool.Yes;
                    doc.InputCharacterEncoding = EncodingType.Utf8;
                    doc.CharacterEncoding = EncodingType.Utf8;
                    doc.CleanWord2000 = true;
                    doc.DefaultAltText = "image";
                    doc.OutputCharacterEncoding = EncodingType.Utf8;
                    doc.JoinClasses = true;
                    doc.JoinStyles = true;
                    doc.LineBreakBeforeBR = true;
                    doc.MakeBare = true;
                    doc.MakeClean = true;
                    doc.MergeDivs = AutoBool.No;
                    doc.OutputXhtml = true;
                    doc.UseLogicalEmphasis = true;
                    doc.ForceOutput = true;

                    doc.OutputBodyOnly = AutoBool.Yes;

                    doc.CleanAndRepair();

                    ret = doc.Save();
                }
            }
#pragma warning disable CS0168 // The variable 'e' is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // The variable 'e' is declared but never used
            {
            }

            return ret;
        }

        public static string StripTags(string source)
        {
            source = ClearHTML(source.Replace(Environment.NewLine, " ").Replace("\n", " ")).Replace("<\\p>", "<\\p>" + Environment.NewLine);

            return StripTagsRegexCompiled(HttpUtility.HtmlDecode(source).Trim());
        }

        private static Regex _htmlRegexComents = new Regex("<!--.*?-->", RegexOptions.Compiled);
        private static Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

        /// <summary>
        /// Remove HTML from string with compiled Regex.
        /// </summary>
        private static string StripTagsRegexCompiled(string source)
        {
            string s1 = _htmlRegexComents.Replace(source, String.Empty);

            return _htmlRegex.Replace(s1, String.Empty);
        }
    }
}