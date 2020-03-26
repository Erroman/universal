﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using BaseTypes.Interfaces;
using BaseTypes.Attributes;

using Diagram.UI.Interfaces;
using Diagram.UI.Utils;

using CelestialMechanics.Wrapper.UI.Wrappers;
using CelestialMechanics.Wrapper.Classes;

namespace CelestialMechanics.Wrapper.UI.UserControls
{
    public partial class UserControlOrbit : UserControl
    {
        #region Fields

        VectorWrapper vector;

        OrbitalWrapper orbital;

        CelestialMechanics.Wrapper.Classes.Orbit orbit;

        List<ComboBox> boxes;


        Dictionary<string, TransfomationType> dt =
            new Dictionary<string, TransfomationType>()
            {
                {"Time to Motion",  TransfomationType.TimeToMotion},
               {"Parameters to Obtit", TransfomationType.ParametersToObtit},
               {"Orbit to Parameters", TransfomationType.OrbitToParameters},
            };

        bool fill = true;

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor
        /// </summary>
        public UserControlOrbit()
        {
            InitializeComponent();
            foreach (string s in dt.Keys)
            {
                comboBoxTransformationType.Items.Add(s);
            }
            boxes = userControlComboboxListExpot.Boxes;
            foreach (ComboBox b in boxes)
            {
                b.SelectedIndexChanged += b_SelectedIndexChanged; 
            }
        }

        #endregion

        #region Intertnal Members

        internal CelestialMechanics.Wrapper.Classes.Orbit Orbit
        {
            set
            {
                userControlPhysicalUnit.Attribute =
                    (value as IPhysicalUnitTypeAttribute).PhysicalUnitTypeAttribute;
                vector = new VectorWrapper(value);
                orbital = new OrbitalWrapper(value);
                propertyGridOcsulating.SelectedObject = orbital;
                propertyGridVector.SelectedObject = vector;
                orbit = value;
                for (int i = 0; i < comboBoxTransformationType.Items.Count; i++)
                {
                    if (dt[comboBoxTransformationType.Items[i] + ""].Equals(value.Tuple.Item1))
                    {
                        comboBoxTransformationType.SelectedIndex = i;
                        break;
                    }
                }
                comboBoxTransformationType.SelectedIndexChanged +=
                    comboBoxTransformationType_SelectedIndexChanged;
                Fill();
                Select();
                (orbit as IAddRemove).AddAction += UserControlOrbit_AddRemoveAction;
                (orbit as IAddRemove).RemoveAction += UserControlOrbit_AddRemoveAction;

            }
        }

  
        #endregion

        #region Event Handlers

        private void comboBoxTransformationType_SelectedIndexChanged(object sender, EventArgs e)
        {
            TransfomationType tt =
                dt[comboBoxTransformationType.SelectedItem + ""];
            Tuple<TransfomationType, PhysicalUnitTypeAttribute, bool,
                   double, double[], byte[], DateTime, Tuple<Dictionary<int, string>, double[]>> t = orbit.Tuple;
            orbit.Tuple = new Tuple<TransfomationType, PhysicalUnitTypeAttribute, bool,
                double, double[], byte[], DateTime, Tuple<Dictionary<int, string>, double[]>>(
               tt, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Rest);
            bool b = tt == TransfomationType.TimeToMotion;
            foreach (Control c in tabPageExport.Controls)
            {
                c.Enabled = b;
            }
    
        }

        private void tabControl_TabIndexChanged(object sender, EventArgs e)
        {
            propertyGridOcsulating.SelectedObject = orbital;
            propertyGridVector.SelectedObject = vector;
        }



        void b_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (fill)
            {
                return;
            }
            Dictionary<int, string> exp = orbit.Tuple.Rest.Item1;
            exp.Clear();
            for (int i = 0; i < boxes.Count; i++)
            {
                ComboBox b = boxes[i];
                object o = b.SelectedItem;
                if (o != null)
                {
                    exp[i] = o + "";
                }
            }
        }

        void UserControlOrbit_AddRemoveAction(object obj)
        {
            Fill();
            Select();
        }

        #endregion

        #region Private Members

        void Fill()
        {
            fill = true;
            boxes.FillCombo(orbit.Exports);
            fill = false;
        }

        void Select()
        {
            fill = true;
            Dictionary<int, string> exp = orbit.Tuple.Rest.Item1;
            foreach (int i in exp.Keys)
            {
                boxes[i].SelectCombo(exp[i]);
            }
            fill = false;
        }

        #endregion

    }
}
