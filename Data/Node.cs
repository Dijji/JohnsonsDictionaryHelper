// Copyright (c) 2014, Dijji, and released under BSD, as defined by the text in the root of this distribution.
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Johnson
{
    public class Node : INotifyPropertyChanged
    {
        protected MainWindow window;
        protected Node parent;
        protected int divId;

        // Required for XML deserialization
        public Node() { }

        public Node(MainWindow window, Node parent)
        {
            this.window = window;
            this.parent = parent;
            divId = NextDivId();
        }

        ~Node()
        {
            if (window != null && window.LastTouched == this)
                window.LastTouched = null;
        }

        // Used to set up window and hierarchy after deserialization from XML
        public virtual void SetState(MainWindow window, Node parent)
        {
            this.window = window;
            this.parent = parent;
            divId = NextDivId();
        }

        [XmlIgnore]
        public string DivId { get { return divId.ToString(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        // Next division id is provided, ultimately, by the root of a node hierarchy
        protected virtual int NextDivId() { return parent != null ? parent.NextDivId() : 0; }

        // Font size is provided, ultimately, by the root of a node hierarchy
        [XmlIgnore]
        public virtual double FontSize { get { return parent != null ? parent.FontSize : 12.0; } set { ;} }


        protected void CleanseSaveAndNotify(ref string target, string value, string property)
        {
            if (value == null)
                value = "";

            // fix some typical OCR scanning glitches before saving value
            target = value.Replace("’", "'").Replace("‘", "'").Replace("ﬁ", "fi").Replace("ﬂ", "fl");

            if (target != value && window != null && !window.Opening)
                // If we changed the value and so need to notify the change, and the update might be from a UI property setter,
                // defer notification to stop it being eaten by the setter's caller, who, being the setter, ignores it as expected
                window.Dispatcher.BeginInvoke(new Action<string>(this.OnPropertyUpdated), property);
            else
                OnPropertyUpdated(property);

            if (window != null)
                window.LastTouched = this;
        }

        protected void OnPropertyUpdated(string property)
        {
            OnPropertyChanged(property);

            if (window != null)
                window.OnValueUpdated();
        }

        protected void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
