﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CategoryTheory;

using Diagram.UI.Labels;

using Event.Basic.Events;

using Event.UI.UserControls;

namespace Event.UI.Labels
{
    /// <summary>
    /// Label of forced event
    /// </summary>
    [Serializable()]
    public class ForcedEventLabel : UserControlBaseLabel
    {
        #region Fields

        UserControlForcedEvent uc;

        ForcedEvent forced;

        Form form = null;

        #endregion

        #region Ctor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="kind">Kind</param>
        /// <param name="icon">Icon</param>
        public ForcedEventLabel(Type type, string kind, Image icon)
            : base(type, kind, icon)
        {
            BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        }


        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected ForcedEventLabel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        }


        #endregion

        #region Overriden

        /// <summary>
        /// Internal control
        /// </summary>
        protected override UserControl Control
        {
            get
            {
                uc = new UserControlForcedEvent();
                return uc;
            }
        }

        /// <summary>
        /// Object
        /// </summary>
        public override ICategoryObject Object
        {
            get
            {
                return forced;
            }
            set
            {
                forced= value.GetObject<ForcedEvent>();
                uc.Event = forced;
            }
        }

        /// <summary>
        /// Associated form
        /// </summary>
        public override object Form
        {
            get
            {
                return form;
            }
        }

        /// <summary>
        /// Creates Form
        /// </summary>
        public override void CreateForm()
        {
        }

        #endregion
    }
}
