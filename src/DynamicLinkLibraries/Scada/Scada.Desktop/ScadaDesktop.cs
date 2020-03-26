﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using CategoryTheory;

using BaseTypes.Attributes;

using Diagram.UI;
using Diagram.UI.Interfaces;
using Diagram.UI.Labels;

using DataPerformer.Interfaces;
using DataPerformer.Portable;


using Event.Interfaces;
using Event.Portable;
using Event.Portable.Events;

using Scada.Interfaces;

namespace Scada.Desktop
{
    /// <summary>
    /// Scada from desktop
    /// </summary>
    public abstract class ScadaDesktop : ScadaInterface, IScadaFactory
    {
        #region Fields

     /*   /// <summary>
        /// Singleton
        /// </summary>
        public static readonly ScadaDesktop Singleton = 
            new ScadaDesktop(null, null, TimeType.Second, true, null, null);*/

        /// <summary>
        /// The "is enabled" sign
        /// </summary>
        protected bool isEnabled = false;

        /// <summary>
        /// Desktop
        /// </summary>
        protected IDesktop desktop;

        /// <summary>
        /// Data consumer
        /// </summary>
        protected IDataConsumer dataConsumer;

        /// <summary>
        /// Name of consumer
        /// </summary>
        protected string consumerName;

        /// <summary>
        /// Time unit
        /// </summary>
        protected TimeType timeUnit;

        /// <summary>
        /// Collection
        /// </summary>
        protected IComponentCollection collection;

        /// <summary>
        /// Error handler
        /// </summary>
        protected Scada.Interfaces.IErrorHandler errorHandler;

        /// <summary>
        /// Create Xml documet
        /// </summary>
        protected Action<IDesktop, XElement> onCreateXmlFactory = (IDesktop desktop, XElement document) => { };


        /// <summary>
        /// The "is absolute time" sign
        /// </summary>
        protected bool isAbsoluteTime;

        /// <summary>
        /// Realtime step
        /// </summary>
        protected IAsynchronousCalculation realtimeStep;
   
        /// <summary>
        /// Refresh action
        /// </summary>
        protected Action onRefresh = () => { };

        /// <summary>
        /// Events with data
        /// </summary>
        protected Event.Interfaces.IEvent[] eventsData;

        #endregion

        #region Ctor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="desktop">The desktop</param>
        /// <param name="dataConsumer">Data consumer</param>
        /// <param name="timeUnit">Time unit</param>
        /// <param name="isAbsoluteTime">The "is absolute time" sign</param>
        /// <param name="realtimeStep">Realtime Step</param>
        /// <param name="events">Events</param>
        protected ScadaDesktop(IDesktop desktop, string dataConsumer, TimeType timeUnit, bool isAbsoluteTime,
            IAsynchronousCalculation realtimeStep, Event.Interfaces.IEvent[] events)
        {
            if (desktop == null)
            {
                return;
            }
            this.realtimeStep = realtimeStep;
            this.desktop = desktop;
            this.timeUnit = timeUnit;
            this.isAbsoluteTime = isAbsoluteTime;
            consumerName = dataConsumer;
            eventsData = events;
            CreateScada();
        }

        #endregion

        #region Overriden Members

        /// <summary>
        /// Gets object
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="">Name</param>
        /// <returns>The object</returns>
        public override T GetObject<T>(string name)
        {
            return desktop.GetObject<T>(name);
        }

        /// <summary>
        /// Gets inputs
        /// </summary>
        /// <param names="names">Input names</param>
        /// <returns>Inputs</returns>
        public override Action<object[]> GetInput(string[] names)
        {
            string s = names[0].Substring(0, names[0].LastIndexOf('.'));
            for (int i = 1; i < names.Length; i++)
            {
                if (!s.Equals(names[i].Substring(0, names[1].LastIndexOf('.'))))
                {
                    throw new Exception("Different sources");
                }
            }
            ForcedEventData f = desktop.GetObject<ForcedEventData>(s);
            s += ".";
            List<Tuple<string, object>> l = f.Types;
            List<int> li = new List<int>();
            List<string> ln = new List<string>();
            for (int i = 0; i < l.Count; i++)
            {
                string ts = l[i].Item1;
               ln.Add(s + ts);
            }
            List<int> lin = new List<int>();
            foreach (string name in names)
            {
                lin.Add(ln.IndexOf(name));
            }
            int[] indx = lin.ToArray();
            return (object[] o) =>
                {
                    object[] data = f.Data;
                    for (int i = 0; i < o.Length; i++)
                    {
                        data[indx[i]] = o[i];
                    }
                    f.Force();
                };
        }

        /// <summary>
        /// Gets outputs
        /// </summary>
        /// <param name="names">Names</param>
        /// <returns>Outputs</returns>
        public override Func<object[]> GetOutput(string[] names)
        {
            IMeasurement[] m = desktop.FindMeasurements(names);
            object[] o = new object[m.Length];
            return () =>
                {
                    for (int i = 0; i < o.Length; i++)
                    {
                        o[i] = m[i].Parameter();
                    }
                    return o;
                };
        }

        /// <summary>
        /// The "is enabled" sign
        /// </summary>
        public override bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                if (isEnabled == value)
                {
                    return;
                }
                isEnabled = value;
                if (value)
                {
                    collection.StartRealtime(timeUnit, isAbsoluteTime, realtimeStep, null, null, null);
                    onStart();
                }
                else
                {
                    StaticExtensionEventPortable.StopRealTime();
                    onStop();
                }
            }
        }

        /// <summary>
        /// Error Handler
        /// </summary>
        public override Scada.Interfaces.IErrorHandler ErrorHandler
        {
            get
            {
                return errorHandler;
            }
            set
            {
                errorHandler = value;
                StaticExtensionDiagramUI.ErrorHandler = new ErrorHandlerWrapper(value);
            }
        }

        #endregion

        #region Virtual Members

        /// <summary>
        /// Crerates SCADA
        /// </summary>
        protected virtual void CreateScada()
        {
            IEnumerable<ICategoryObject> categoryObjects = desktop.CategoryObjects;
            foreach (ICategoryObject ob in categoryObjects)
            {
                IObjectLabel l = ob.Object as IObjectLabel;
                string name = l.RootName;
                List<string> list = null;
                if (objects.ContainsKey(name))
                {
                    list = objects[name];
                }
                else
                {
                    list = new List<string>();
                    objects[name] = list;
                }
                list.Add(ob.GetType().AssemblyQualifiedName);
            }
            events.Clear();
            dEvents.Clear();
            if (consumerName == null)
            {
                return;
            }
            dataConsumer = desktop.GetObject<IDataConsumer>(consumerName);
            collection = dataConsumer.CreateCollection("Realtime");
            collection.ForEach((Event.Interfaces.IEvent ev) =>
                {
                    string s = (ev as IAssociatedObject).GetRootName();
                    events.Add(s);
                    dEvents[s] = new EventWrapper(ev);
                }
            );
            collection.ForEach((IEventHandler eventHandler) =>
            {
                IEnumerable<Event.Interfaces.IEvent> evs = eventHandler.Events;
                foreach (Event.Interfaces.IEvent ev in evs)
                {
                    string s = (ev as IAssociatedObject).GetRootName();
                    if (events.Contains(s))
                    {
                        continue;
                    }
                    events.Add(s);
                    dEvents[s] = new EventWrapper(ev);
                }
            });
            inputs.Clear();
            dInput.Clear();
            collection.ForEach((ForcedEventData f) =>
            {
                string s = (f as IAssociatedObject).GetRootName();
                List<Tuple<string, object>> l = f.Types;
                for (int i = 0; i < l.Count; i++)
                {
                    Tuple<string, object> t = l[i];
                    int[] k = new int[] { i };
                    string n = s + "." + t.Item1;
                    inputs.Add(n, t.Item2);
                    dInput[n] = (object obj) =>
                    {
                        object[] arr = f.Data;
                        arr[k[0]] = obj;
                        f.Force();
                    };
                }
            });
            outputs.Clear();
            dOutput.Clear();
            collection.ForEach((IMeasurements measurements) =>
            {
                string s = (measurements as IAssociatedObject).GetRootName();
                for (int i = 0; i < measurements.Count; i++)
                {
                    IMeasurement measurement = measurements[i];
                    string n = s + "." + measurement.Name;
                    outputs.Add(n, measurement.Type);
                    dOutput[n] = measurement.Parameter;
                }
            });
            dConstant.Clear();
            collection.ForEach((IAlias alias) =>
            {
                string s = (alias as IAssociatedObject).GetRootName();
                foreach (string name in alias.AliasNames)
                {
                    string n = s + '.' + name;
                    Tuple<string, object> t = new Tuple<string, object>(n, alias.GetType(name));
                    dConstant[n] = (object o) =>
                    {
                        alias[name] = o;
                    };
                }
            });
            
         }

        /// <summary>
        /// Creates scada from desktop
        /// </summary>
        /// <param name="desktop">The desktop</param>
        /// <param name="timeUnit">Time unit</param>
        /// <param name="isAbsoluteTime">The "is absolute time" sing</param>
        /// <param name="realtimeStep">Realtime Step</param>
        /// <returns>The scada</returns>
        public abstract IScadaInterface Create(IDesktop desktop, string dataConsumer, TimeType timeUnit,
            bool isAbsoluteTime, IAsynchronousCalculation realtimeStep);
  
        /// <summary>
        /// Xml document
        /// </summary>
        public override XElement XmlDocument
        {
            get
            {
                 XElement document = StaticExtensionScadaInterfaces.CreateXML(this);
                 onCreateXml(document);
                 return document;
            }
        }

        /// <summary>
        /// On refresh event
        /// </summary>
        public override event Action OnRefresh
        {
            add { onRefresh += value; }
            remove { onRefresh -= value; }
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Desktop
        /// </summary>
        public IDesktop Desktop
        {
            get
            { 
                return desktop; 
            }
        }

        #endregion

        #region IScadaFactory Members

        event Action<IDesktop, XElement> IScadaFactory.OnCreateXml
        {
            add { onCreateXmlFactory += value; }
            remove { onCreateXmlFactory -= value; }
        }

        #endregion
    
    }
}
