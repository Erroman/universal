﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;



using Diagram.UI.Aliases;

using SerializationInterface;

using DataPerformer.Portable;
using DataPerformer.Interfaces;

using Motion6D.Interfaces;

namespace Motion6D
{
    /// <summary>
    /// Facet phisical field
    /// </summary>
    [Serializable()]
    public class FacetPhysicalField3D : PhysicalFieldBase, IFacetConsumer
    {

        #region Fields

        private IFacet facets;

        //private double tempPos;

        private Dictionary<int, string> aliases = new Dictionary<int, string>();

        private Dictionary<int, AliasName> parameters = new Dictionary<int, AliasName>();

        string areaString = "";

        string normalString = "";

        private Func<object, object, object>[] adds = null;

        private AliasName area;

        private AliasName normal;

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor
        /// </summary>
        public FacetPhysicalField3D()
        {
        }


        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected FacetPhysicalField3D(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            areaString = info.Deserialize<string>("Area");
            normalString = info.Deserialize<string>("Normal");
            aliases = info.Deserialize<Dictionary<int, string>>("Additional");
        }


        /// <summary>
        /// Calculates field
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Array of components of field</returns>
        public override object[] this[double[] position]
        {
            get
            {
                return Calculate(position);
            }
        }

        #endregion

        #region Overriden Members


        /// <summary>
        /// Implementation of serialization
        /// </summary>
        /// <param name="info">Serialization Info</param>
        /// <param name="context">Streaming Context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.Serialize<string>("Area", areaString);
            info.Serialize<string>("Normal", normalString);
            info.Serialize<Dictionary<int, string>>("Additional", aliases);
        }

        /// <summary>
        /// The operation that performs after arrows setting
        /// </summary>
        public override void PostSetArrow()
        {
            base.PostSetArrow();
        }

        #endregion

        #region IFacetConsumer Members

        IFacet IFacetConsumer.Facet
        {
            get
            {
                return facets;
            }
            set
            {
                if ((facets != null) & (value != null))
                {
                    throw new Exception("Facets already exists");
                }
                facets = value;
            }
        }

        #endregion


        #region Specific Members


        private void SetAdditionalAliases()
        {
            area = null;
            normal = null;
            parameters.Clear();
            if (areaString.Length > 0)
            {
                area = this.FindAliasName(areaString, false);
                if (area == null)
                {
                    areaString = "";
                }
            }
            if (normalString.Length > 0)
            {
                normal = this.FindAliasName(normalString, false);
                if (normal == null)
                {
                    normalString = "";
                }
            }
            List<int> del = new List<int>();
            foreach (int i in aliases.Keys)
            {
                AliasName an = this.FindAliasName(aliases[i], false);
                if (an == null)
                {
                    del.Add(i);
                    continue;
                }
                parameters[i] = an;
            }
            foreach (int i in del)
            {
                aliases.Remove(i);
            }

        }


        private object[] Calculate(double[] position)
        {
            int n = facets.Count;
            for (int ic = 0; ic < n; ic++)
            {
                double[] p = facets[ic];
                for (int i = 0; i < position.Length; i++)
                {
                    AliasName an = al[i];
                    an.SetValue(position[i] - p[i]);
                }
                if (area != null)
                {
                    area.SetValue(facets.GetArea(ic));
                }
                foreach (int k in parameters.Keys)
                {
                    parameters[k].SetValue(facets[ic, k]);
                }
                if (normal != null)
                {
                    normal.SetValue(facets.GetNormal(ic));
                }
                foreach (IMeasurements m in measurements)
                {
                        m.UpdateMeasurements();
                }
                for (int i = 0; i < measures.Length; i++)
                {
                    object o = measures[i].Parameter();
                    result[i] = adds[i](result[i], o);
                }
                this.FullReset();
            /*    foreach (IMeasurements m in measurements)
                {
                    isUpdated[m] = false;
                }*/
            }
            return result;
        }
        
        #endregion


    }
}
