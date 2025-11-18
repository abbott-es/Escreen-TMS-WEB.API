using System;
using System.Text.RegularExpressions;

namespace WEB.GATEWAY.Middleware
{
    /// <summary>
    /// Builds a regex from a route template (e.g., "/client-api/api/client/{id}",
    /// "/location-api/api/location/{id:guid}", "/files/{*path}") and tests a path.
    /// </summary>
    public static class RouteTemplate
    {
        private const string GuidPattern = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
        private static readonly Regex ParamRegex = new(
            // {name}, {name:constraint}, {*catchAll}
            @"\{(\*?)([A-Za-z_][A-Za-z0-9_]*)?(?::([A-Za-z0-9_]+))?\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string BuildPattern(string template, bool allowTrailingSlash = false)
        {
            if (string.IsNullOrWhiteSpace(template))
                throw new ArgumentException("Route template is required.", nameof(template));

            // Normalize to start with slash and trim trailing slash like your NormalizePath
            if (template[0] != '/')
                template = "/" + template;

            template = template.Trim().TrimEnd('/').ToLowerInvariant();

            int index = 0;
            string pattern = "^";

            foreach (Match m in ParamRegex.Matches(template))
            {
                // literal before the param
                if (m.Index > index)
                {
                    string literal = template.Substring(index, m.Index - index);
                    pattern += Regex.Escape(literal);
                }

                bool isCatchAll = m.Groups[1].Value == "*";
                string name = m.Groups[2].Success ? m.Groups[2].Value : (isCatchAll ? "catchAll" : throw new FormatException("Unnamed parameter."));
                string constraint = m.Groups[3].Success ? m.Groups[3].Value.ToLowerInvariant() : "";

                pattern += BuildGroup(isCatchAll, constraint);
                index = m.Index + m.Length;
            }

            // trailing literal
            if (index < template.Length)
            {
                string tail = template.Substring(index);
                pattern += Regex.Escape(tail);
            }

            pattern += allowTrailingSlash ? "/?$" : "$";
            return pattern;
        }

        private static string BuildGroup(bool isCatchAll, string constraint)
        {
            if (isCatchAll)
            {
                // capture-all: allow anything (at least one char). Use .* if empty allowed.
                return "(.+)";
            }

            // Map common constraints
            string segment = constraint switch
            {
                "" => @"[^/]+",
                "int" => @"\d+",
                "long" => @"\d+",
                "guid" => GuidPattern,
                "alpha" => @"[A-Za-z]+",
                "alphanum" or "alphanumeric" => @"[A-Za-z0-9]+",
                "slug" => @"[a-z0-9]+(?:-[a-z0-9]+)*",
                _ => @"[^/]+", // fallback
            };

            return $"({segment})";
        }
    }
}
