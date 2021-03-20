using System;
using System.Text.RegularExpressions;

namespace G2RModel.Model
{
    public class TextFixer
    {
        public Regex Finder { get; private set; }
        public string Fixer { get; set; }
        public string FinderText { get; set; }
        public bool IsValid { get; set; }

        public TextFixer Init()
        {
            if (string.IsNullOrEmpty(FinderText) || string.IsNullOrEmpty(Fixer))
            {
                IsValid = false;
                return null;
            }
            try
            {
                Finder = new Regex(FinderText);
                var dummy = Finder.Match("success is irrelevant");
                //var g = dummy.Groups["hit"];
                IsValid = true;
                return this;
            }
            catch (Exception ex)
            {
                IsValid = false;
                return null;
            }
        }

        public string Exec(string victim)
        {
            if (!IsValid && Init() == null)
                return victim;

            if (string.IsNullOrEmpty(victim))
                return victim;

            try
            {
                Match m = Finder.Match(victim);
                return !m.Success ? victim : m.Result(Fixer);
            }
            catch
            {
                return victim;
            }
        }

    }
}
