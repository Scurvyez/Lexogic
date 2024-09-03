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
            
            string output = Regex.Replace(input, patternMa, "", RegexOptions.Singleline);
            output = Regex.Replace(output, patternMat, "", RegexOptions.Singleline);
            output = Regex.Replace(output, patternEtLink, "", RegexOptions.Singleline);
            output = Regex.Replace(output, patternDxEty, "", RegexOptions.Singleline);
            output = Regex.Replace(output, patternDxt, " $1", RegexOptions.Singleline);
            output = output.Replace("{it}", "*").Replace("{/it}", "*");
            output = Regex.Replace(output, @"\s+", " ").Trim();

            return output;
        }
    }
}