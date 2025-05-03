using System.Text.RegularExpressions;
using System.Text;

namespace T3MenuAPI;

public static partial class Lib
{
    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)] private static partial Regex TagRegex();
    public static string TruncateHtml(this string html, int maxLength)
    {
        if (maxLength <= 0 || string.IsNullOrEmpty(html))
            return html;

        string textOnly = TagRegex().Replace(html, string.Empty);
        if (textOnly.Length <= maxLength)
            return html;

        Stack<string> tagStack = new();
        StringBuilder result = new();
        int visibleLength = 0, i = 0;

        while (i < html.Length && visibleLength < maxLength)
        {
            if (html[i] == '<')
            {
                Match match = TagRegex().Match(html, i);
                if (match.Success && match.Index == i)
                {
                    string tag = match.Value;
                    result.Append(tag);
                    i += tag.Length;

                    if (!tag.StartsWith("</", StringComparison.Ordinal))
                    {
                        string tagName = tag.Split([' ', '>', '/'], StringSplitOptions.RemoveEmptyEntries)[0].TrimStart('<');
                        if (!tag.EndsWith("/>", StringComparison.Ordinal) && !tagName.StartsWith('!'))
                            tagStack.Push(tagName);
                    }
                    else if (tagStack.Count > 0)
                    {
                        tagStack.Pop();
                    }

                    continue;
                }
            }

            result.Append(html[i]);
            visibleLength++;
            i++;
        }

        while (tagStack.Count > 0)
            result.Append($"</{tagStack.Pop()}>");

        return result.ToString();
    }

}