﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections;


using CategoryTheory;

using Diagram.UI.Labels;
using Diagram.UI;
using Diagram.UI.UserControls;


using DataPerformer.Interfaces;
using DataPerformer;
using Regression;


namespace DataPerformer.UI.UserControls
{
    /// <summary>
    /// User control of regression selection
    /// </summary>
    public partial class RegressionSelectionUserControl : UserControl
    {
        IStructuredSelectionCollection s;
        Hashtable controls = new Hashtable();
        AliasRegression reg;
        string name;

        private RegressionSelectionUserControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Costructor
        /// </summary>
        /// <param name="reg">Regression component</param>
        /// <param name="s">Selection</param>
        public RegressionSelectionUserControl(AliasRegression reg, 
            IStructuredSelectionCollection s) : this()
        {
            this.s = s;
            this.reg = reg;
            IAssociatedObject ao = s as IAssociatedObject;
            IObjectLabel label = ao.Object as IObjectLabel;
            name = label.RootName;//NamedComponent.GetText(label);
            Control c = new UserControlObject(null, label);
            c.Left = 0;
            c.Top = 0;
            Controls.Add(c);
            int y = c.Height;
            for (int i = 0; i < s.Count; i++)
            {
                IStructuredSelection sel = s[i];
                Label l = new Label();
                l.Text = sel.Name;
                l.Top = y + 10;
                l.Left = 10;
                Controls.Add(l);
                y = l.Top + l.Height;
                NumericUpDown n = new NumericUpDown();
                n.Minimum = -1;
                n.Value = -1;
                n.Left = 10;
                n.Top = y + 10;
                y = n.Top + n.Height + 20;
                Controls.Add(n);
                controls[n] = reg.GetRelativeName(s as IAssociatedObject) + "." + sel.Name;
            }
            Height = y;
        }


        /// <summary>
        /// Table of selections
        /// </summary>
        public Hashtable Table
        {
            set
            {
                foreach (int i in value.Keys)
                {
                    string nam = value[i] as string;
                    foreach (NumericUpDown nup in controls.Keys)
                    {
                        string str = controls[nup] as string;
                        if (str.Equals(nam))
                        {
                            nup.Value = i;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fills 
        /// </summary>
        /// <param name="table">Table of selections</param>
        public void Fill(Hashtable table)
        {
            foreach (NumericUpDown nup in controls.Keys)
            {
                int i = (int)nup.Value;
                if (i < 0)
                {
                    continue;
                }
                if (table.ContainsKey(i))
                {
                    throw new Exception("More than one secection with equal number");
                }
                table[i] = controls[nup];
            }
        }

    }
}
