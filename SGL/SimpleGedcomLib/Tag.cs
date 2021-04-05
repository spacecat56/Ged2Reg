using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleGedcomLib
{
    public class Tag
    {
        public static Dictionary<string, TagCode> TagMapping { get; set; }

        public static Regex TagRex = new Regex(@"^((?<level>[0-9])\s+(?<tag>[_A-Z]+)(\s+(?<text>.*))?)|((?<level>0)\s(?<id>@.*?@)\s(?<tag>.+))$");

        private TagCode _code;
        private Tag _parentTag;

        public TagCode Code
        {
            get { return _code; }
            set { _code = value; }
        }

        public Tag ParentTag
        {
            get { return _parentTag; }
            set
            {
                _parentTag = value; 
                if (_parentTag!=null && _parentTag.Level >= Level) 
                    throw new ArgumentException("parent is not at a higher level");                
            }
        }

        public int Level { get; set; }  
        public string TagText { get; set; }
        public TagDescription Description { get; set; }
        public string Content { get; set; }
        public string Id { get; set; } 
        public List<Tag> Children { get; set; } = new List<Tag>();
        public Tag ReferredTag { get; set; }
        public bool IsValid { get; set; }
        public List<Tag> ReferencedFrom { get; set; } = new List<Tag>();


        public bool HasIdentifier => !string.IsNullOrEmpty(Id);
        public bool IsPrivate => Has(TagDescription.Private);
        public bool Is(TagDescription t) => (Description == t);
        public bool Is(TagCode t) => (Code == t);
        public bool Has(TagDescription t) => GetChild(t) != null;
        public bool Has(TagCode t) => GetChild(t) != null;
        public Tag GetChild(TagDescription t) => Children.FirstOrDefault(x => x.Description == t);
        public Tag GetChild(TagCode t) => Children.FirstOrDefault(x => x.Code == t);
        public List<Tag> GetChildren(TagCode t) => Children.FindAll(x => x.Code == t);

        public Tag GetAncestor(TagCode t)
        {

            Tag target = this;
            while (target.Code != t && target.ParentTag != null)
                target = target.ParentTag;

            return target.Code == t ? target : null;
        }

        public Tag GetFirstDescendant(TagCode t)
        {
            foreach (Tag child in Children)
            {
                if (child._code == t)
                    return child;
                Tag rv = child.GetFirstDescendant(t);
                if (rv != null)
                    return rv;
            }

            return null;
        }

        public Tag GetAncestor(int level)
        {
            Tag target = this;
            while (target.Level > level)
                target = target.ParentTag;

            return target.Level == level ? target : null;
        }

        internal Tag() { }

        public Tag(string text, Tag parent)
        {
            Match m = TagRex.Match(text);
            if (!m.Success) return;
            Level = int.Parse(m.Groups["level"].Value);
            TagText = m.Groups["tag"].Value;
            if (!Enum.TryParse(TagText, out _code))
            {
                if (!((TagMapping?.TryGetValue(TagText, out _code))??false))
                    _code = TagCode.UNK;
            }
            Description = _code.Map();
            Content = (m.Groups["text"].Success) ? m.Groups["text"].Value : null;
            Id = (m.Groups["id"].Success) ? m.Groups["id"].Value : null;
            IsValid = true; // assume
            try
            {
                ParentTag = parent;
            }
            catch (Exception)
            {
                IsValid = false; // unless
            }
        }

        public bool Link(Dictionary<string, Tag> refMap)
        {
            if (string.IsNullOrEmpty(Content) || !refMap.ContainsKey(Content)) return false;
            ReferredTag = refMap[Content];
            ReferredTag.ReferencedFrom.Add(this);
            return true;
        }

        public string FullText(bool includeNewlines = false)
        {
            StringBuilder sb = new StringBuilder();
            Tag victim = Children.Find(x => x.Code == TagCode.TEXT || x.Code == TagCode._TEXT) ?? this;
            sb.Append(victim.Content);
            foreach (Tag child in victim.Children)
            {
                if (child.Description == TagDescription.Continued)
                    sb.Append(includeNewlines ? '\n' : ' ').Append(child.Content);
                else if (child.Description == TagDescription.Concatenation)
                    sb.Append(child.Content);
            }
            return sb.ToString();
        }
    }
}
