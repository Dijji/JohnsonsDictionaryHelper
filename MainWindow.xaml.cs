// Copyright (c) 2014, Dijji, and released under BSD, as defined by the text in the root of this distribution.
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace Johnson
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        // The dictionary entry being edited
        private Entry entry;

        // Property values
        private bool opening = false;
        private double treeViewWidth;
        private string fileDir;

        // Used to build HTML preview
        private string cssStyle;
        private string javaScript;

        // Other
        private string resourceDir;             // Directory containing HTML resources
        private bool updateHTML = false;        // True if deferred HTML refresh needed
        private bool existsCheckNeeded = false; // True if exisitence of saved definition needs to be checked
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            entry = new Entry(this);
            entry.FontSize = Properties.Settings.Default.FontSize;
            this.DataContext = entry;
            FileDir = Properties.Settings.Default.FileDirectory;
            entry.HaveFileDir = FileDir.Length > 0;
            resourceDir = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Resources";
            cssStyle = System.IO.File.ReadAllText(resourceDir + @"\style.css");
            javaScript = System.IO.File.ReadAllText(resourceDir + @"\script.js");
        }
        #endregion

        #region Properties
        // True while a saved entry is being loaded
        public bool Opening { get { return opening; } }

        // The last Node touched by the user
        public Node LastTouched { get; set; }

        // Last known width of definitions TreeView
        public double TreeViewWidth { get { return treeViewWidth; } }

        // The directory where saved entries are stored
        private string FileDir
        {
            get
            {
                if (fileDir == null || fileDir.Length == 0)
                    return "";
                else
                    return fileDir;
            }
            set
            {
                fileDir = value;
                if (fileDir.Length > 0 && !fileDir.EndsWith("\\"))
                    fileDir += "\\";
            }
        }
        #endregion

        #region UX Button Handlers

        private void ChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = Johnson.Properties.Resources.SelectFolder;
                dialog.RootFolder = Environment.SpecialFolder.Desktop;
                dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileDir = dialog.SelectedPath;
                    Properties.Settings.Default.FileDirectory = FileDir;
                    Properties.Settings.Default.Save();
                    entry.HaveFileDir = true;
                }
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.InitialDirectory = FileDir;
                dialog.Filter = Johnson.Properties.Resources.XmlFilter;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        XmlSerializer x = new XmlSerializer(typeof(Entry));
                        TextReader reader = new StreamReader(dialog.FileName);
                        Entry loaded = (Entry)x.Deserialize(reader);
                        reader.Close();
                        opening = true;
                        entry.TakeDataFrom(loaded);
                        entry.Id = System.IO.Path.GetFileNameWithoutExtension(dialog.SafeFileName);
                        opening = false;
                        OnValueUpdated();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (HaveValidEntryId())
            {
                string fileName = FileNameFromEntryId(entry.Id) + ".xml";

                try
                {
                    XmlSerializer x = new XmlSerializer(typeof(Entry));
                    TextWriter writer = new StreamWriter(FileDir + fileName);
                    x.Serialize(writer, entry);
                    writer.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (HaveValidEntryId())
            {
                TextWriter writer = new StreamWriter(FileDir + FileNameFromEntryId(entry.Id) + ".txt");
                writer.Write(GenerateHTML());
                writer.Close();
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(GenerateHTML());
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            entry.Clear();
            OnValueUpdated();
        }

        private void AddDefinition_Click(object sender, RoutedEventArgs e)
        {
            entry.AddDefinition();
        }

        private void AddGeneralQuote_Click(object sender, RoutedEventArgs e)
        {
            entry.AddGeneralQuote();
        }

        private void RemoveGeneralQuoteSingle_Click(object sender, RoutedEventArgs e)
        {
            if (listQuoteSingle.SelectedIndex != -1)
            {
                entry.RemoveGeneralQuoteAt(listQuoteSingle.SelectedIndex);
                OnValueUpdated();
            }
       }

        private void RemoveGeneralQuoteNumCommon_Click(object sender, RoutedEventArgs e)
        {
            if (listQuoteNumCommon.SelectedIndex != -1)
            {
                entry.RemoveGeneralQuoteAt(listQuoteNumCommon.SelectedIndex);
                OnValueUpdated();
            }
        }

        private void RemoveDefinitionNumCommon_Click(object sender, RoutedEventArgs e)
        {
            if (listDefinitionNumCommon.SelectedIndex != -1)
            {
                entry.RemoveDefinitionAt(listDefinitionNumCommon.SelectedIndex);
                OnValueUpdated();
            }
        }

        private void RemoveDefinitionNumEach_Click(object sender, RoutedEventArgs e)
        {
            Definition d = treeDefinitionNumEach.SelectedItem as Definition;
            if (d != null)
            {
                entry.RemoveDefinition(d);
                OnValueUpdated();
            }
        }

        private void RemoveAllDefinitionsNum_Click(object sender, RoutedEventArgs e)
        {
            entry.ClearDefinitions();
            OnValueUpdated();
        }

        private void AddQuoteNumEach_Click(object sender, RoutedEventArgs e)
        {
            Definition d = treeDefinitionNumEach.SelectedItem as Definition;
            if (d == null)
            {
                // If the focus is on quote, add a quote to the parent definition
                Quote q = treeDefinitionNumEach.SelectedItem as Quote;
                if (q != null)
                {
                    d = q.Parent;
                }
            }
            if (d != null)
            {
                d.AddQuote();
            }
        }

        private void RemoveQuoteNumEach_Click(object sender, RoutedEventArgs e)
        {
            Quote q = treeDefinitionNumEach.SelectedItem as Quote;
            if (q != null)
            {
                q.Delete();
                OnValueUpdated();
            }
        }
        #endregion

        #region Button Handler helpers
        private string FileNameFromEntryId(string id)
        {
            // More to come here as they emerge
            return id.Replace("&AElig;", "Ae").Replace("&OElig;", "Oe");
        }

        private bool HaveValidEntryId()
        {
            if (entry.Id.Length == 0)
            {
                entry.Id = entry.Headword.Trim().Replace("'", "") + " " + entry.Grammar.Replace(".", "");
                existsCheckNeeded = true;
            }

            if (existsCheckNeeded)
            {
                if (File.Exists(FileDir + entry.Id + ".xml") ||
                    File.Exists(FileDir + entry.Id + ".txt"))
                {
                    MessageBox.Show(Johnson.Properties.Resources.FileExists);
                    return false;
                }
                else
                    existsCheckNeeded = false;
            }

            return true;
        }
        #endregion
     
        #region UX Other Event Handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OnValueUpdated();
        }

        // Invoke whenever a data value changes
        public void OnValueUpdated()
        {
            DisplayHTML();
        }

        private void webBrowser1_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // If a display update is pending, do it now
            if (updateHTML)
                DisplayHTML();
        }

        private void treeDefinitionNumEach_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            treeViewWidth = e.NewSize.Width;
            entry.OnTreeViewWidthChange();
        }

        private void ListViewTextGotFocus(object sender, RoutedEventArgs e)
        {
            // Select list item when its text gets focus
            ListBoxItem item = UIHelpers.TryFindParent<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (item != null)
                item.IsSelected = true;

            TextBox_SelectionChanged(sender, e);  // belt and braces to ensure toolbar is correct
        }

        private void TreeViewTextGotFocus(object sender, RoutedEventArgs e)
        {
            // Select tree view item when its text gets focus
            TreeViewItem item = UIHelpers.TryFindParent<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (item != null)
                item.IsSelected = true;

            TextBox_SelectionChanged(sender, e);  // belt and braces to ensure toolbar is correct
        }

        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (sender != null)
            {
                entry.HaveSelection = tb.SelectionLength > 0;
                entry.CanInsert = (tb.SelectionLength == 0);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            entry.HaveSelection = false;
            entry.CanInsert = false;
        }
        #endregion

        #region Context menu and toolbar handlers 

        // Enable/disable Context Menu items on opening
        void ContextMenuOpened(Object sender, RoutedEventArgs args)
        {
            ContextMenu cm = (ContextMenu)sender;
            TextBox tb = (TextBox)cm.PlacementTarget;

            foreach (var item in cm.Items)
            {
                MenuItem menuItem = item as MenuItem;

                if (menuItem != null)
                {
                    string tag = menuItem.Tag as string;

                    // No tag means always enabled
                    if (tag == null || tag == "")
                        menuItem.IsEnabled = true;

                    // Only allow e.g. copy/cut/italicise if something is selected to copy/cut/italicise. 
                    else if (tag == "Selection")
                        menuItem.IsEnabled = tb.SelectedText.Length > 0;

                    // Only allow e.g. paste if there is text on the clipboard to paste. 
                    else if (tag == "Clipboard")
                        menuItem.IsEnabled = Clipboard.ContainsText();

                    // Only allow character insertion if nothing is selected
                    else if (tag == "Insert")
                        menuItem.IsEnabled = tb.SelectedText.Length == 0;
                }
            }
        }

        void ClickPaste(Object sender, RoutedEventArgs args) { TargetTextBox(sender, args).Paste(); }
        void ClickCopy(Object sender, RoutedEventArgs args) { TargetTextBox(sender, args).Copy(); }
        void ClickCut(Object sender, RoutedEventArgs args) { TargetTextBox(sender, args).Cut(); }

        const string startItalic = "<i>";
        const string endItalic = "</i>";

        void ClickItalicise(Object sender, RoutedEventArgs args)
        {
            TextBox tb = TargetTextBox(sender, args);

            if (tb != null && tb.SelectionLength > 0)
            {
                string text = tb.Text;
                int pre = tb.SelectionStart;
                int post = pre + tb.SelectionLength;
                while (pre < text.Length && text[pre] == ' ' && post > pre)
                    pre++;
                while (text[post - 1] == ' ' && post > pre)
                    post--;
                if (post > pre)
                {
                    tb.Text = text.Insert(post, endItalic).Insert(pre, startItalic);
                    tb.SelectionStart = post + startItalic.Length + endItalic.Length;
                    tb.SelectionLength = 0;
                }
                args.Handled = true;
            }
        }

        void ClickRemoveBreaks(Object sender, RoutedEventArgs args)
        {
            TextBox tb = TargetTextBox(sender, args);

            if (tb != null)
            {
                tb.Text = tb.Text.Replace("-\n\r", "").Replace("-\n\n", "").Replace("-\n", "")
                                 .Replace(" :", ":").Replace(" ;", ";")
                                 .Replace("\n\r", " ").Replace("\n", " ")
                                 .Replace("  ", " ").Trim();
                args.Handled = true;
            }
        }

        void ClickAEUpper(Object sender, RoutedEventArgs args)
        {
            InsertText(TargetTextBox(sender, args), "&AElig;");
        }

        void ClickAELower(Object sender, RoutedEventArgs args)
        {
            InsertText(TargetTextBox(sender, args), "&aelig;");
        }

        void ClickOEUpper(Object sender, RoutedEventArgs args)
        {
            InsertText(TargetTextBox(sender, args), "&OElig;");
        }

        void ClickOELower(Object sender, RoutedEventArgs args)
        {
            InsertText(TargetTextBox(sender, args), "&oelig;");
        }

        void ClickNumber(Object sender, RoutedEventArgs args)
        {
            InsertText(TargetTextBox(sender, args), "&#8470;");
        }

        void ClickSection(Object sender, RoutedEventArgs args)
        {
            InsertText(TargetTextBox(sender, args), "&sect;");
        }

        void ClickDash(Object sender, RoutedEventArgs args)
        {
            InsertText(TargetTextBox(sender, args), "&mdash;");
        }

        void ClickSpace(Object sender, RoutedEventArgs args)
        {
            InsertText(TargetTextBox(sender, args), "&emsp;");
        }

        void ClickGrowFont(Object sender, RoutedEventArgs args)
        {
            entry.FontSize += 1.0;
            Properties.Settings.Default.FontSize = entry.FontSize;
            Properties.Settings.Default.Save();
            OnValueUpdated();
        }

        void ClickShrinkFont(Object sender, RoutedEventArgs args)
        {
            entry.FontSize -= 1.0;
            Properties.Settings.Default.FontSize = entry.FontSize;
            Properties.Settings.Default.Save();
            OnValueUpdated();
        }

        #endregion

        #region Command handling helpers
        TextBox TargetTextBox(Object sender, RoutedEventArgs args)
        {
            if (sender is MenuItem)
            {
                var cm = ((MenuItem)sender).Parent as ContextMenu;
                return cm.PlacementTarget as TextBox;
            }
            else if (sender is MainWindow)
                return args.OriginalSource as TextBox;
            else
                return null;
        }

        private void InsertText(TextBox tb, string text)
        {
            if (tb != null)
            {
                int at = tb.SelectionStart;
                tb.Text = tb.Text.Insert(at, text);
                tb.SelectionStart = at + text.Length;
            }
        }
        #endregion

        #region Event Handlers if Awesomium control used
        //protected override void OnClosed(EventArgs e)
        //{
        //    base.OnClosed(e);

        //    // Dispose our control.
        //    webControl.Dispose();

        //    // Shutdown the core.
        //    WebCore.Shutdown();
        //}

        //private void webControl_ProcessCreated(object sender, WebViewEventArgs e)
        //{
        //    try
        //    {
        //        webControl.LoadHTML(htmlPrefix);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //}

        //private void webControl_DocumentReady(object sender, UrlEventArgs e)
        //{
        //    if (updateHTML)
        //        DisplayHTML();

        //    // scroll to div for selected defn.quote etc.
        //    if (LastTouched != null)
        //    {
        //        webControl.ExecuteJavascript("scrollToDiv(" + LastTouched.Idref + ");"");
        //    }
        //}
        #endregion

        #region HTML Generation and Display

        // Display a preview of the dictionary entry in  the Web control
        private void DisplayHTML()
        {
           if (!Opening)
            {
                if (webBrowser1.IsLoaded)
                {
                    string scriptNav = "";

                    //scroll to div for selected defn.quote etc.
                    if (LastTouched != null)
                    {
                        scriptNav = "<script language=\"javascript\" type=\"text/javascript\">" +
                                   javaScript +
                                   "scrollToDiv(" + LastTouched.DivId + ");" +
                                   "</script>";
                    }

                    webBrowser1.NavigateToString(GenerateStyle() + GenerateHTML(true) + scriptNav);
                    updateHTML = false;
                }
                // If Awesomium web control is used
                //if (webControl.IsDocumentReady)
                //{
                //    webControl.LoadHTML(prefix + GetHTML());
                //    updateHTML = false;
                //}
                else
                    updateHTML = true;
            }
        }

        const int LongDefinitionThreshold = 100;   // used e.g. to decide if definition should be inside or outside bracketed table

        private string GenerateStyle()
        {
            return "<style type=\"text/css\">" + cssStyle + "body{zoom: " + Convert.ToInt32(entry.FontSize * 80/12.0).ToString() + "%;}</style>";
        }

        private string GenerateHTML(bool forPreview = false)
        {
            StringBuilder sb = new StringBuilder();
            if (forPreview)
                sb.Append("<div id='" + entry.DivId + "'>");
            if (entry.Preamble.Length > 0)
            {
                sb.Append(entry.Preamble.Trim());
                sb.Append(" ");
            }
            if (!entry.DefinitionBrace)
            {
                if (entry.Headword.Length > 0)
                {
                    sb.Append(HeadwordHTML(entry.Headword));
                }
                if (entry.Headword2.Length > 0)
                {
                    sb.Append(", or ");
                    sb.Append(HeadwordHTML(entry.Headword2, true));
                }
                if (entry.Headword3.Length > 0)
                {
                    sb.Append(", or ");
                    sb.Append(HeadwordHTML(entry.Headword3, true));
                }
                if (entry.Postamble.Length > 0)
                {
                    sb.Append(", ");
                    sb.Append(entry.Postamble.Trim());
                    sb.Append(". ");
                }
                else
                    sb.Append(". ");
                if (entry.Grammar.Length > 0)
                {
                    sb.Append("<i>");
                    sb.Append(entry.Grammar);
                    sb.Append("</i> ");
                }
                if (entry.From.Length > 0)
                {
                    sb.Append("[");
                    sb.Append(FullStopAndBreaks(entry.From));
                    sb.Append("] ");
                }
                if (entry.Style == EntryStyle.Single)
                {
                    if (entry.SingleDefinition.Length > 0)
                    {
                        sb.Append(FullStopAndBreaks(entry.SingleDefinition, true));
                    }
                    if (forPreview)
                        sb.Append("</p>");
                    sb.Append("\r\n");
                }
                // Numbered defns
                else if (entry.SingleDefinition.Length > 0)
                {
                    // SingleDefinition holds any prefixed notes
                    sb.Append(FullStopAndBreaks(entry.SingleDefinition, true));
                }
            }
            else
            {
                sb.Append("<table border=\"0\"><tr><td>");
                sb.Append(HeadwordHTML(entry.Headword));
                sb.Append(".<br>");
                sb.Append(HeadwordHTML(entry.Headword2));
                if (entry.Headword3.Length > 0)
                {
                    sb.Append(".<br>");
                    sb.Append(HeadwordHTML(entry.Headword3));
                }
                Uri uri;
                string height;
                string width;
                if (entry.Headword3.Length > 0)
                {
                    sb.Append(".</td><td>");
                    uri = !forPreview ?
                        new Uri("http://johnsonsdictionaryonline.com/wp-content/themes/johnson/images/right-4line-bracket.gif") :
                        new Uri(resourceDir + @"\right-4line-bracket.gif");
                    height = !forPreview ? "\"100%\"" : "\"100px\"";
                    width = "\"22px\"";
                }
                else
                {
                    sb.Append(".</td><td width=\"19px\">");
                    uri = !forPreview ?
                        new Uri("http://johnsonsdictionaryonline.com/wp-content/themes/johnson/images/right-3line-bracket.gif") :
                        new Uri(resourceDir + @"\right-3line-bracket.gif");
                    height = !forPreview ? "\"100%\"" : "\"60px\"";
                    width = "\"16px\"";
                }
                sb.Append("<img src=\"" + uri.ToString() + "\" height=");
                sb.Append(height);
                sb.Append(" width=");
                sb.Append(width);
                sb.Append("></td><td>");
                if (entry.Grammar.Length > 0)
                {
                    sb.Append("<i>");
                    sb.Append(entry.Grammar);
                    sb.Append("</i> ");
                }
                if (entry.From.Length > 0)
                {
                    sb.Append("[");
                    sb.Append(FullStopAndBreaks(entry.From));
                    sb.Append("] ");
                }
                // single and numbered defns
                if (entry.SingleDefinition.Length > 0 && entry.SingleDefinition.Length < LongDefinitionThreshold)
                {
                    sb.Append(FullStopAndBreaks(entry.SingleDefinition, true));
                }
                sb.Append("</td></tr></table>");
                sb.Append("\r\n");
                if (entry.SingleDefinition.Length >= LongDefinitionThreshold)
                {
                    sb.Append(FullStopAndBreaks(entry.SingleDefinition, true));
                }
            }

            if (entry.Style != EntryStyle.Single && entry.Definitions.Count > 0)
            {
                sb.Append("\r\n<ol>");
                foreach (Definition d in entry.Definitions)
                {
                    sb.Append("\r\n<li>");
                    if (forPreview)
                        sb.Append("<div id='" + d.DivId + "'>");
                    sb.Append(FullStopAndBreaks(d.Text, true));
                    if (forPreview)
                        sb.Append("</p>");
                    sb.Append("\r\n");

                    if (entry.Style == EntryStyle.NumEach && d.Quotes.Count > 0)
                    {
                        foreach (Quote q in d.Quotes)
                        {
                            if (q.Text != null && q.Text.Length > 0)
                            {
                                if (forPreview)
                                    sb.Append("<p><div id='" + q.DivId + "'>");
                                sb.Append("\r\n<span class=\"quotedText\">");
                                sb.Append(FullStopAndBreaks(q.Text, true));
                                sb.Append("</span>");
                            }
                            if (q.Source != null && q.Source.Length > 0)
                            {
                                sb.Append(" <i>");
                                sb.Append(FullStopAndBreaks(q.Source));
                                sb.Append("</i>\r\n");
                            }
                        }
                        if (forPreview)
                            sb.Append("</p></div>");
                    }
                    if (forPreview)
                        sb.Append("</div>");
                }
                sb.Append("</ol>\r\n");
            }

            if (entry.Style == EntryStyle.Single || entry.Style == EntryStyle.NumCommon)
            {
                if (entry.GeneralQuotes.Count > 0)
                {
                    foreach (Quote q in entry.GeneralQuotes)
                    {
                        if (q.Text != null && q.Text.Length > 0)
                        {
                            if (forPreview)
                                sb.Append("<p><div id='" + q.DivId + "'>");
                            sb.Append("\r\n<span class=\"quotedText\">");
                            sb.Append(FullStopAndBreaks(q.Text, true));
                            sb.Append("</span>");
                        }
                        if (q.Source != null && q.Source.Length > 0)
                        {
                            sb.Append(" <i>");
                            sb.Append(FullStopAndBreaks(q.Source));
                            sb.Append("</i>\r\n");
                        }
                        if (forPreview)
                            sb.Append(" </div>");
                    }
                }
                sb.Append("\r\n");
                if (forPreview)
                    sb.Append("</p>");
            }

            if (entry.EndNote.Length > 0)
            {
                sb.Append(FullStopAndBreaks(entry.EndNote));
                sb.Append("\r\n");
            }

            if (forPreview)
                sb.Append("</div>");

            return sb.ToString();
        }

        private string HeadwordHTML(string word, bool headword2 = false)
        {
            StringBuilder sb = new StringBuilder();
            string className = headword2 ? "headword2" : "headword";
            sb.Append("<span class=\"");
            sb.Append(className);
            sb.Append("\">");
            string headword = word.Trim();
            if (headword.Length > 1)
                headword = headword.Substring(0, 1).ToUpper() + headword.Substring(1);
            else
                headword = headword.ToUpper();
            sb.Append(headword);
            sb.Append("</span>");
            return sb.ToString();
        }

        private string FullStopAndBreaks(string text, bool breaks = false)
        {
            if (text == null)
                return "";

            string result = text.Trim();
            if (!result.EndsWith(".") && !result.EndsWith("?") && !result.EndsWith("!") && !result.EndsWith("-"))
                result += ".";
            if (breaks)
                // This is funky because XML round trips replace \r\n with \n
                result = result.Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("<br>", "<br>\r\n");

            return result;
        }
        #endregion
    }

}
