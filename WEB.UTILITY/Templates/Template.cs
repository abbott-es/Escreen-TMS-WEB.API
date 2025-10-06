using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.UTILITY.Templates
{
    public class Template
    {
        private readonly string _content;

        public Template(string content) => _content = content;

        public string Apply(Dictionary<string, (string Text, string Link)> buttons)
        {
            return buttons.Aggregate(
                new StringBuilder(_content),
                (result, kvp) => result.Replace(new Bracket(kvp.Key), $"[[button:{kvp.Value.Text}||{kvp.Value.Link}]]")).ToString();
        }

        public string Apply(IReadOnlyDictionary<string, string> labels)
        {
            var html = ApplyLabels(labels);
            return html;
        }

        public string ApplyBadges(Dictionary<string, (string Text, string Link, bool isInverted)> badges, IReadOnlyDictionary<string, string> labels)
        {
            var replacedBadges = badges.Aggregate(
                new StringBuilder(_content),
                (result, kvp) => result.Replace(new Bracket(kvp.Key), kvp.Value.isInverted ? $"[[badge-inverted:{kvp.Value.Text}]]" : $"[[badge:{kvp.Value.Text}]]")).ToString();

            var newTeplate = new Template(replacedBadges);
            var htmlLabels = ApplyLabels(labels, newTeplate);

            var updatedTemplate = new Template(htmlLabels);
            var html = ReplaceDataTokens(labels, new string[] { "text", "date" }, updatedTemplate);

            return html;
        }

        private string ApplyLabels(IReadOnlyDictionary<string, string> labels, Template updatedTemplate = null)
        {
            var templateContent = updatedTemplate != null ? updatedTemplate._content : _content;
            var html = Apply(templateContent, labels);
            while (labels.Select(x => x.Key).Any(x => html.Contains("[[" + x + "]]")))
            {
                html = Apply(html, labels);
            }

            return html;
        }

        public string ReplaceDataTokens(IReadOnlyDictionary<string, string> labels, string[] tokenPrefixes, Template updatedTemplate = null)
        {
            var templateContent = updatedTemplate != null ? updatedTemplate._content : _content;
            var html = Apply(templateContent, labels);
            foreach (var prefix in tokenPrefixes)
            {
                while (labels.Select(x => x.Key).Any(x => html.Contains($"[[data-{prefix}:{x}]]")))
                {
                    html = Apply(html, labels, $"data-{prefix}:");
                }
            }

            return html;
        }

        private static string Apply(string html, IReadOnlyDictionary<string, string> args, string prefix = null)
        {
            var result = prefix switch
            {
                "data-date" => args.Aggregate(new StringBuilder(html), (result, kvp) => result.Replace(new Bracket(kvp.Key, prefix), kvp.Value)).ToString(),
                "data-text" => args.Aggregate(new StringBuilder(html), (result, kvp) => result.Replace(new Bracket(kvp.Key, prefix), kvp.Value)).ToString(),
                _ => args.Aggregate(new StringBuilder(html), (result, kvp) => result.Replace(new Bracket(kvp.Key), kvp.Value)).ToString()
            };

            return result;
        }

        private struct Bracket
        {
            private readonly string _key;
            private readonly string _prefix;

            public Bracket(string key, string prefix = null)
            {
                _key = key;
                _prefix = prefix;
            }

            public static implicit operator string(Bracket b) => $"[[{b._prefix}{b._key}]]";
        }
    }
}
