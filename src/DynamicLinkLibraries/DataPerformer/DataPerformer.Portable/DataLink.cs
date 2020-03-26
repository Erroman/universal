﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CategoryTheory;

using Diagram.UI;
using Diagram.UI.Interfaces;
using Diagram.UI.Labels;

using DataPerformer.Interfaces;
using DataPerformer.Portable.Interfaces;


namespace DataPerformer.Portable
{

    /// <summary>
    /// The link between data provider and data consumer
    /// </summary>
    public class DataLink : ICategoryArrow,
        IRemovableObject, IDataLinkFactory
    {

        #region Fields

        /// <summary>
        /// Error message
        /// </summary>
        public static readonly string SetProviderBefore = "You should create measurements source before consumer";

        /// <summary>
        /// DataLink checker
        /// </summary>
        protected static Action<DataLink> checker;

        /// <summary>
        /// The source of this arrow
        /// </summary>
        protected IDataConsumer source;

        /// <summary>
        /// The target of this arrow
        /// </summary>
        protected IMeasurements target;


        /// <summary>
        /// Linked object
        /// </summary>
        protected object obj;

        /// <summary>
        /// Data link factory
        /// </summary>
        protected static IDataLinkFactory dataLinkFactory;// = new DataLink();

        protected MeasurementsWrapper mv = null;

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor
        /// </summary>
        public DataLink()
        {
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        static DataLink()
        {
            dataLinkFactory = new DataLink();
        }

        #endregion

        #region ICategoryArrow Members

        /// <summary>
        /// The source of this arrow
        /// </summary>
        public ICategoryObject Source
        {
            set
            {
                if (source != null)
                {
                    throw new Exception();
                }
                IDataLinkFactory f = this;
                source = f.GetConsumer(value);
            }
            get
            {
                return source as ICategoryObject;
            }
        }

        /// <summary>
        /// The target of this arrow
        /// </summary>
        public ICategoryObject Target
        {
            get
            {
                return target as ICategoryObject;
            }
            set
            {
                if (target != null)
                {
                    throw new Exception();
                }
                IDataLinkFactory f = this;
                IMeasurements t = f.GetMeasurements(value);
                if (t is MeasurementsWrapper)
                {
                    mv = t as MeasurementsWrapper;
                }
                bool check = true;
                IAssociatedObject s = source as IAssociatedObject;
                if (s.Object != null & value.Object != null)
                {
                    if (check)
                    {
                        INamedComponent ns = s.Object as INamedComponent;
                        INamedComponent nt = value.Object as INamedComponent;
                        if (nt != null & ns != null)
                        {
                            if (nt.GetDifference(ns) >= 0)
                            {
                                throw new Exception(SetProviderBefore);
                            }
                        }
                    }
                    source.Add(t);
                    target = value as IMeasurements;
                }
                if (!check)
                {
                    return;
                }
                try
                {
                    if (checker != null)
                    {
                        checker(this);
                    }
                }
                catch (Exception e)
                {
                    e.ShowError(10);
                    source.Remove(target);
                    throw e;
                }
            }
        }


        #endregion

        #region IAssociatedObject Members

        /// <summary>
        /// Associated object
        /// </summary>
        public object Object
        {
            get
            {
                return obj;
            }
            set
            {
                obj = value;
            }
        }

        #endregion

        #region IRemovableObject Members

        /// <summary>
        /// The post remove operation
        /// </summary>
        public void RemoveObject()
        {
            if (source == null | target == null)
            {
                return;
            }
            if (mv != null)
            {
                source.Remove(mv);
                return;
            }
            source.Remove(target);
        }

        #endregion

        #region IDataLinkFactory Members

        IDataConsumer IDataLinkFactory.GetConsumer(ICategoryObject source)
        {
         /*   if (source is IChildrenObject)
            {
                IDataConsumer dcc = (source as IChildrenObject).GetChildren<IDataConsumer>();
                if (dcc != null)
                {
                    return dcc;
                }
            }*/
            IAssociatedObject ao = source;
            object o = ao.Object;
            if (o is INamedComponent)
            {
                IDataConsumer dcl = null;
                INamedComponent comp = o as INamedComponent;
                IDesktop desktop = comp.Root.Desktop;
                desktop.ForEach<DataLink>((DataLink dl) =>
                {
                    if (dcl != null)
                    {
                        return;
                    }
                    object dt = dl.Source;
                    if (dt is IAssociatedObject)
                    {
                        IAssociatedObject aot = dt as IAssociatedObject;
                        if (aot.Object == o)
                        {
                            dcl = dl.source as IDataConsumer;
                        }
                    }
                });
               
                if (dcl != null)
                {
                    return dcl;
                }
            }

            IDataConsumer dc = DataConsumerWrapper.Create(source);
            if (dc == null)
            {
                CategoryException.ThrowIllegalTargetException();
            }
            return dc;
        }

        IMeasurements IDataLinkFactory.GetMeasurements(ICategoryObject target)
        {
            IAssociatedObject ao = target;
            object o = ao.Object;
            if (o is INamedComponent)
            {
                IMeasurements ml = null;
                INamedComponent comp = o as INamedComponent;
                IDesktop d = null;
                INamedComponent r = comp.Root;
                if (r != null)
                {
                    d = r.Desktop;
                }
                else
                {
                    d = comp.Desktop;
                }
                if (d != null)
                {
                    d.ForEach((DataLink dl) =>
                    {
                        if (ml != null)
                        {
                            return;
                        }
                        object dt = dl.Target;
                        if (dt is IAssociatedObject)
                        {
                            IAssociatedObject aot = dt as IAssociatedObject;
                            if (aot.Object == o & (!(aot is IChildrenObject)))
                            {
                                ml = dl.Target as IMeasurements;
                            }
                        }
                    });
                    if (ml != null)
                    {
                        return ml;
                    }
                }
            }
            IMeasurements m = MeasurementsWrapper.Create(target);
            if (m == null)
            {
                CategoryException.ThrowIllegalTargetException();
            }
            return m;
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Checker of data link
        /// </summary>
        public static Action<DataLink> Checker
        {
            set
            {
                checker = value;
            }
        }

        /// <summary>
        /// Data link factory
        /// </summary>
        public static IDataLinkFactory DataLinkFactory
        {
            get
            {
                return dataLinkFactory;
            }
            set
            {
                dataLinkFactory = value;
            }
        }


        /// <summary>
        /// Measurements provider
        /// </summary>
        public IMeasurements Measurements
        {
            get
            {
                return target;
            }
        }

        #endregion

    }

}

