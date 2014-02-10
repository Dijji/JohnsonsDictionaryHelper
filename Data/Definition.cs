// Copyright (c) 2014, Dijji, and released under BSD, as defined by the text in the root of this distribution.
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Johnson
{
    [XmlInclude(typeof(Quote))]
    public class Definition : Node
    {
        private string text;
        private ObservableCollection<Quote> quotes = new ObservableCollection<Quote>();

        public Definition() { }
        public Definition(MainWindow window, Node parent) : base(window, parent) { }

        public override void SetState(MainWindow window, Node parent)
        {
            base.SetState(window, parent);
            foreach (Quote q in Quotes)
                q.SetState(window, this);
        }

        public string Text { get { return text; } set { CleanseSaveAndNotify(ref text, value, "Text"); } }
        public ObservableCollection<Quote> Quotes { get { return quotes; } }

        [XmlIgnore]
        public double TreeViewItemWidth { get { return window.TreeViewWidth - Entry.DefinitionTreeDelta; } }

        public Quote AddQuote()
        {
            Quote q = new Quote(window, this);
            Quotes.Add(q);
            return q;
        }

        public void RemoveQuote(Quote q)
        {
            Quotes.Remove(q);
        }

        public void OnFontSizeChange()
        {
            OnPropertyUpdated("FontSize");
            foreach (Quote q in Quotes)
                q.OnFontSizeChange();
        }

        public void OnTreeViewWidthChange()
        {
            OnPropertyUpdated("TreeViewItemWidth");
            foreach (Quote q in Quotes)
                q.OnTreeViewWidthChange();
        }
    }
}
