using System.Text.RegularExpressions;

namespace Lexogic
{
    public class JSONParserHelper
    {
        public static string ParseJSONTags(string input)
        {
            const string patternMa = @"\{ma\}.*?\{\/ma\}";
            const string patternMat = @"\{mat\}.*?\{\/mat\}";
            const string patternEtLink = @"\{et_link.*?\}";
            const string patternDxEty = @"\{dx_ety\}|\{\/dx_ety\}";
            const string patternDxt = @"\{dxt\|([^\|]+).*?\}";

            const string patternB = @"\{b\}(.*?)\{\/b\}";           // bold
            const string patternIt = @"\{it\}(.*?)\{\/it\}";        // italic
            const string patternSc = @"\{sc\}(.*?)\{\/sc\}";        // small caps
            const string patternInf = @"\{inf\}(.*?)\{\/inf\}";     // subscript
            const string patternSup = @"\{sup\}(.*?)\{\/sup\}";     // superscript
            const string patternPBr = @"\{p_br\}";                  // para break
            const string patternLq = @"\{ldquo\}";                  // left 2x quote
            const string patternRq = @"\{rdquo\}";                  // right 2x quote
            const string patternBc = @"\{bc\}";                     // bold colon

            string output = Regex.Replace(input, patternMa, "", RegexOptions.Singleline);
            output = Regex.Replace(output, patternMat, "", RegexOptions.Singleline);
            output = Regex.Replace(output, patternEtLink, "", RegexOptions.Singleline);
            output = Regex.Replace(output, patternDxEty, "", RegexOptions.Singleline);
            output = Regex.Replace(output, patternDxt, " $1", RegexOptions.Singleline);

            output = Regex.Replace(output, patternB, "**$1**", RegexOptions.Singleline);
            output = Regex.Replace(output, patternIt, "*$1*", RegexOptions.Singleline);
            output = Regex.Replace(output, patternSc, "$1", RegexOptions.Singleline);
            output = Regex.Replace(output, patternInf, "*$1*", RegexOptions.Singleline);
            output = Regex.Replace(output, patternSup, "*$1*", RegexOptions.Singleline);
            output = Regex.Replace(output, patternPBr, "\n", RegexOptions.Singleline);
            output = Regex.Replace(output, patternLq, "\"", RegexOptions.Singleline);
            output = Regex.Replace(output, patternRq, "\"", RegexOptions.Singleline);
            output = Regex.Replace(output, patternBc, "**: **", RegexOptions.Singleline);

            output = Regex.Replace(output, @"\s+", " ").Trim();

            return output;
        }
    }
}