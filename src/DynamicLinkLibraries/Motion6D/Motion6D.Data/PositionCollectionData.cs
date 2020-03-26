using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

using CategoryTheory;
using Diagram.UI;

using DataPerformer;
using DataPerformer.Interfaces;
using DataPerformer.Portable;


using Motion6D.Interfaces;



namespace Motion6D
{
    /// <summary>
    /// Positions with data
    /// </summary>
    [Serializable()]
    public class PositionCollectionData : PositionCollection, ISerializable,
        ICategoryObject, IDataConsumer, IPostSetArrow
    {

        #region Fields

        /// <summary>
        /// Change input event
        /// </summary>
        private event Action onChangeInput = () => { };

        List<IMeasurements> measurementsData = new List<IMeasurements>();
        List<IMeasurement> measuresData = new List<IMeasurement>();
        List<string> measures = new List<string>();
        string factoryName;
        IPositionFactory factory = PositionFactory.Factory;

        object obj;

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor
        /// </summary>
        public PositionCollectionData()
        {
            factoryName = factory.Name;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected PositionCollectionData(SerializationInfo info, StreamingContext context)
        {
            measures = info.GetValue("Measurements", typeof(List<string>)) as List<string>;
            factoryName = info.GetValue("Factory", typeof(string)) + "";
            factory = PositionFactory.Factory[factoryName];
        }

        #endregion

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Measurements", measures, typeof(List<string>));
            info.AddValue("Factory", factoryName, typeof(string));
        }

        #endregion

        #region IAssociatedObject Members

        object IAssociatedObject.Object
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

        #region IDataConsumer Members

        void IDataConsumer.Add(IMeasurements measurements)
        {
            measurementsData.Add(measurements);
        }

        void IDataConsumer.Remove(IMeasurements measurements)
        {
            measurementsData.Remove(measurements);
        }

        void IDataConsumer.UpdateChildrenData()
        {
            foreach (IMeasurements m in measurementsData)
            {
                m.UpdateMeasurements();
            }
        }

        int IDataConsumer.Count
        {
            get { return measurementsData.Count; }
        }

        IMeasurements IDataConsumer.this[int n]
        {
            get { return measurementsData[n]; }
        }

        void IDataConsumer.Reset()
        {
            this.FullReset();
        }

        event Action IDataConsumer.OnChangeInput
        {
            add { onChangeInput += value; }
            remove { onChangeInput -= value; }
        }

        #endregion

        #region IPostSetArrow Members

        void IPostSetArrow.PostSetArrow()
        {
            SetPositions();
        }

        #endregion

        #region Specific Members

        /// <summary>
        /// Name of factory
        /// </summary>
        public string FactoryName
        {
            get
            {
                return factory.Name;
            }
            set
            {
                factory = PositionFactory.Factory[value];
                factoryName = value;
            }
        }


        /// <summary>
        /// Factory
        /// </summary>
        public IPositionFactory Factory
        {
            get
            {
                return factory;
            }
            set
            {
                factory = value;
                factoryName = factory.Name;
            }
        }

        /// <summary>
        /// Lisf of measures
        /// </summary>
        public List<string> Measures
        {
            get
            {
                return measures;
            }
            set
            {
                measures = value;
                SetPositions();
            }
        }

        /// <summary>
        /// All measures
        /// </summary>
        public List<string> AllMeasures
        {
            get
            {
                Double a = 0;
                return this.GetAllMeasurements(a);
            }
        }

        /// <summary>
        /// Sets positions
        /// </summary>
        protected void SetPositions()
        {
            positions.Clear();
            if (measures.Count < 3)
            {
                return;
            }
            measuresData.Clear();
            IDataConsumer c = this;
            foreach (string ms in measures)
            {
                for (int i = 0; i < c.Count; i++)
                {
                    IMeasurements m = c[i];
                    IAssociatedObject ao = m as IAssociatedObject;
                    string on = this.GetRelativeName(ao) + ".";

                    for (int j = 0; j < m.Count; j++)
                    {
                        IMeasurement mea = m[j];
                        string s = on + mea.Name;
                        if (s.Equals(ms))
                        {
                            measuresData.Add(mea);
                        }
                    }
                }
            }
            List<IIterator> iterators = new List<IIterator>();
            c.GetIterators(iterators);
            foreach (IIterator it in iterators)
            {
                it.Reset();
            }
            IDataConsumer consumer = this;
            IDataRuntime rt = consumer.CreateRuntime(StaticExtensionDataPerformerInterfaces.Calculation);
            while (true)
            {
                rt.UpdateAll();
                object[] ob = new object[measuresData.Count];
                for (int i = 0; i < measuresData.Count; i++)
                {
                    object o = measuresData[i].Parameter();
                    if (o is DBNull | o == null)
                    {
                        goto iterate;
                    }
                    ob[i] = o;
                }
                IPosition position = factory.Create(ob);
                if (position != null)
                {
                    positions.Add(position);
                }
            iterate:
                foreach (IIterator it in iterators)
                {
                    if (!it.Next())
                    {
                        goto fin;
                    }
                }
            }
        fin:
            Parent = parent;
            return;
        }

        #endregion
    }
}