// Copyright (c) 2014, Dijji, and released under BSD, as defined by the text in the root of this distribution.
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Johnson
{
    public enum EntryStyle
    {
        Single,
        NumCommon,
        NumEach,
    }
 
    [XmlInclude(typeof(Definition))]
    [XmlInclude(typeof(Quote))]
    public class Entry : Node
    {
        private int nextDivId = 1;

        // Property values
        private bool haveFileDir;
        private bool haveSelection;
        private bool canInsert;
        private double fontSize = 12.0;

        // Entry content
        private string id;
        private EntryStyle style;
        private string preamble;
        private string headword;
        private string headword2;
        private string headword3;
        private bool definitionBrace;
        private string postamble;
        private string grammar;
        private string from;
        private string singleDefinition;
        private string endNote;

        // Values need to be added here for any further grammatical usages encountered
        static private List<string> grammarValues = new List<string> { 
            "adj.", "adv.", "n.s.",  
            "part.adj.", "particip.adj.", "participial adj.",   // same thing, but Johnson varies the text
            "u.a.", "u.s.", "v.a.", "v.n."
        };

        // Because of TreeView's deeply recursive structure, TextBoxes in definitions and quotes cannot be auto-sized 
        // by WPF to fit the TreeView as we do with the ListBox.
        // So instead we track the TreeView width, and bind the TextBox widths to a value that is TreeView width minus these
        // heuristically determined values.
        static public double DefinitionTreeDelta = 80.0;
        static public double QuoteTreeDelta = 120.0;

        private ObservableCollection<Definition> definitions = new ObservableCollection<Definition>();
        private ObservableCollection<Quote> generalQuotes = new ObservableCollection<Quote>();

        public EntryStyle Style { get { return style; } set { style = value; OnPropertyUpdated("Style"); } }
        public string Id { get { return id; } set { id = value; OnPropertyUpdated("Id"); OnPropertyUpdated("Title"); } }
        public string Preamble { get { return preamble; } set {  CleanseSaveAndNotify(ref preamble, value, "Preamble"); } }
        public string Headword { get { return headword != null ? headword : ""; } set { CleanseSaveAndNotify(ref headword, value, "Headword"); Id = ""; } }
        public string Headword2 { get { return headword2 != null ? headword2 : ""; } set { CleanseSaveAndNotify(ref headword2, value, "Headword2"); } }
        public string Headword3 { get { return headword3 != null ? headword3 : ""; } set { CleanseSaveAndNotify(ref headword3, value, "Headword3"); } }
        public bool DefinitionBrace { get { return definitionBrace; } set { definitionBrace = value; OnPropertyUpdated("DefinitionBrace"); } }
        public string Postamble { get { return postamble; } set { CleanseSaveAndNotify(ref postamble, value, "Postamble"); } }
        public string Grammar { get { return grammar; } set { grammar = value != null ? value : ""; Id = ""; OnPropertyUpdated("Grammar"); } }
        public string From { get { return from; } set { CleanseSaveAndNotify(ref from, value, "From"); } }
        public string SingleDefinition { get { return singleDefinition; } set { CleanseSaveAndNotify(ref singleDefinition, value, "SingleDefinition"); } }
        public string EndNote { get { return endNote != null ? endNote : ""; } set { CleanseSaveAndNotify(ref endNote, value, "EndNote"); } }

        public ObservableCollection<Definition> Definitions { get { return definitions; } }
        public ObservableCollection<Quote> GeneralQuotes { get { return generalQuotes; } }

        [XmlIgnore]
        public List<string> GrammarValues { get { return grammarValues; } }
        [XmlIgnore]
        public bool HaveFileDir { get { return haveFileDir; } set { haveFileDir = value; OnPropertyUpdated("HaveFileDir"); } }
        [XmlIgnore]
        public bool HaveSelection { get { return haveSelection; } set { haveSelection = value; OnPropertyUpdated("HaveSelection"); } }
        [XmlIgnore]
        public bool CanInsert { get { return canInsert; } set { canInsert = value; OnPropertyUpdated("CanInsert"); } }
        [XmlIgnore]
        public override double FontSize { get { return fontSize; } set { fontSize = value; OnFontSizeChange(); } }

        [XmlIgnore]
        public string Title
        {
            get
            {
                return Johnson.Properties.Resources.Title + ((Id.Length > 0) ? " - " + Id : "");
            }
        }

        public Entry()
        {
            Clear();
        }

        public Entry(MainWindow window) : base(window, null)
        {
            divId = NextDivId();
            Clear();
        }

        // As the root of the tree, override to provide the next value at tree scope
        protected override int NextDivId() { return nextDivId++; }

        public void Clear()
        {
            Id = "";
            Preamble = "";
            Headword = "";
            Headword2 = "";
            Headword3 = "";
            DefinitionBrace = false;
            Postamble = "";
            Grammar = "";
            From = "";
            ClearDefinitions();
        }

        public void ClearDefinitions()
        {
            SingleDefinition = "";
            EndNote = "";
            GeneralQuotes.Clear();
            Definitions.Clear();
        }

        public void AddDefinition()
        {
            Definitions.Add(new Definition(window, this));
        }

        public void RemoveDefinition(Definition d)
        {
            Definitions.Remove(d);
        }

        public void RemoveDefinitionAt(int index)
        {
            Definitions.RemoveAt(index);
        }

        public void AddGeneralQuote()
        {
            GeneralQuotes.Add(new Quote(window, this));
        }

        public void RemoveGeneralQuoteAt(int index)
        {
            GeneralQuotes.RemoveAt(index);
        }

        // Used to copy data from a deserialized entry into the single entry used for editing
        public void TakeDataFrom(Entry e)
        {
            Style = e.Style;
            Preamble = e.Preamble;
            Headword = e.Headword;
            Headword2 = e.Headword2;
            Headword3 = e.Headword3;
            DefinitionBrace = e.DefinitionBrace;
            Postamble = e.Postamble;
            Grammar = e.Grammar;
            From = e.From;
            SingleDefinition = e.SingleDefinition;
            EndNote = e.EndNote;

            GeneralQuotes.Clear();
            foreach (Quote q in e.GeneralQuotes)
            {
                q.SetState(window, this);
                GeneralQuotes.Add(q);
            }

            Definitions.Clear();
            foreach (Definition d in e.Definitions)
            {
                d.SetState(window, this);
                Definitions.Add(d);
            }
        }

        public void OnFontSizeChange()
        {
            // When the font size changes, let all the nodes know so that they can
            // notify their TextBoxes of the change
            OnPropertyUpdated("FontSize");
            foreach (Definition d in Definitions)
                d.OnFontSizeChange();            
            foreach (Quote q in GeneralQuotes)
                q.OnFontSizeChange();
        }

        public void OnTreeViewWidthChange()
        {
            // When the TreeView changes width, let all the nodes know so that they can
            // update their width properties, which the TextBoxes bind to
            foreach (Definition d in Definitions)
                d.OnTreeViewWidthChange();
            foreach (Quote q in GeneralQuotes)
                q.OnTreeViewWidthChange();
        }
    }
}
