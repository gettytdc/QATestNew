using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace XMLprocessor
{
    public static class PseudoTextConvertor
    {
        //http://www.pseudolocalize.com/
        private static readonly Dictionary<char, char> CharMap = new Dictionary<char, char>
        {
            { 'a', 'â'}, {'e', 'é'}, {'i', 'î'}, {'o', 'ô'}, {'u', 'ù'},
            { 'A', 'Â' }, {'E', 'È' }, {'I', 'Î' },{'O', 'Ô' }, {'U', 'Ù' },

            { 'S', '§' }, {'D', 'Ð' }, {'L', '£' },{'C', 'Ç' },{'Y', 'Ý' },

            { 'b', 'ḃ' },{'c', 'ḉ' },{'d', 'ḍ' },{'f', 'ḟ' },{'g', 'ḡ' },
            { 'h', 'λ' },{'k', 'ḵ' },{'l', 'ℓ' },{'m', 'ṁ' },{'n', 'ñ' },
            { 'p', 'ƥ' },{'r', 'ř'},{'s', 'ṡ'},{'v', 'ṽ' },{'w', 'ẁ' },{'x', 'ẋ'},{'y', 'ẏ'},{'z', 'ẑ'}
        };
        private const char PseudoBegin = 'ˁ';
        private const char PseudoEnd = 'ˀ';

        public static string Convert(string inputToken)
        {
            if (string.IsNullOrWhiteSpace(inputToken) ||
                inputToken.Contains("{ORDINAL") ||
                inputToken.Length < 2 ||
                inputToken.IndexOf(PseudoBegin) >= 0) return inputToken;

            var openBraces = 0;
            var regex = new Regex("{Action}");
            var allowPseudoLocalization = !regex.IsMatch(inputToken);
            regex = new Regex("\\|\\*");
            var isFilter = regex.IsMatch(inputToken);

            var pseudoLocalizedToken = new StringBuilder();

            if (!isFilter) pseudoLocalizedToken.Append(PseudoBegin);

            for (int index = 0; index < inputToken.Length; ++index)
            {
                if (inputToken[index] == '{') ++openBraces;
                if (inputToken[index] == '}') --openBraces;
                if (isFilter && inputToken[index] == '(') allowPseudoLocalization = false;
                if (allowPseudoLocalization &&
                    openBraces == 0 &&
                    (index % 2) == 0 &&
                    CharMap.ContainsKey(inputToken[index]))
                    pseudoLocalizedToken.Append(CharMap[inputToken[index]]);
                else
                    pseudoLocalizedToken.Append(inputToken[index]);
            }

            if (!isFilter) pseudoLocalizedToken.Append(PseudoEnd);

            return pseudoLocalizedToken.ToString();
        }
    }
}