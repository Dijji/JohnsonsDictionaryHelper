// Copyright (c) 2014, Dijji, and released under BSD, as defined by the text in the root of this distribution.
using System.Xml.Serialization;

namespace Johnson
{
    public class Quote : Node
    {
        private string text;
        private string source;

        public Quote() { }
        public Quote(MainWindow window, Node parent) : base(window, parent) { }

        public Definition Parent { get { return parent as Definition; } }
        public string Text { get { return text; } set { CleanseSaveAndNotify(ref text, value, "Text"); } }
        public string Source { get { return source; } set { CleanseSaveAndNotify(ref source, value, "Source"); } }

        [XmlIgnore]
        public double TreeViewItemWidth { get { return window.TreeViewWidth - Entry.QuoteTreeDelta; } }

        public void Delete()
        {
            Parent.RemoveQuote(this);
        }

        public void OnFontSizeChange()
        {
            OnPropertyUpdated("FontSize");
        }

        public void OnTreeViewWidthChange()
        {
            OnPropertyUpdated("TreeViewItemWidth");
        }
    }
}
