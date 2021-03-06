﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using CategoryTheory;

using Diagram.UI;

using DataPerformer.Portable;
using DataPerformer.Interfaces;
using DataPerformer.Interfaces.Attributes;

using Event.Interfaces;

namespace SoundService
{
    /// <summary>
    /// Collection of sounds
    /// </summary>
    [Serializable()]
    [CalculationReasons(new string[] { Event.Interfaces.StaticExtensionEventInterfaces.Realtime, "Testing" })]
    public class SoundCollection : CategoryObject, ISerializable, IDataConsumer,
        IPostSetArrow, ITimeMeasureConsumer, IRealtimeUpdate, IMeasurements
    {
        #region Fields
        
        /// <summary>
        /// Change input event
        /// </summary>
        private event Action onChangeInput = () => { };
        
        /// <summary>
        /// Directory of sounds
        /// </summary>
        static private string soundDirectory;

        List<IMeasurements> measurements = new List<IMeasurements>();

        Dictionary<string, string> sounds = new Dictionary<string, string>();

        Dictionary<IMeasurement, string> measures = new Dictionary<IMeasurement, string>();

        IMeasurement timeMeasure;

        private event Action<string> playSound = (string filename) =>
    {
        string fn = soundDirectory + filename;
        if (!System.IO.File.Exists(fn))
        {
            ("Sound file '" + fn + " do not exist").Show();
            return;
        }
        try
        {
            System.Media.SoundPlayer pl = new System.Media.SoundPlayer();
            pl.SoundLocation = fn;
            pl.Play();
        }
        catch (Exception ex)
        {
            ex.ShowError(1);
        }
    };


        bool isUpdated = false;

        #endregion

        #region Ctor


        /// <summary>
        /// Default constructor
        /// </summary>
        public SoundCollection()
        {
        }

        /// <summary>
        /// Deserialization construcror
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected SoundCollection(SerializationInfo info, StreamingContext context)
        {
            sounds = info.GetValue("Sounds", typeof(Dictionary<string, string>)) as Dictionary<string, string>;
        }

        #endregion

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Sounds", sounds, typeof(Dictionary<string, string>));
        }

        #endregion

        #region IDataConsumer Members

        void IDataConsumer.Add(IMeasurements measurements)
        {
            this.measurements.Add(measurements);
            onChangeInput();
        }

        void IDataConsumer.Remove(IMeasurements measurements)
        {
            this.measurements.Remove(measurements);
            onChangeInput();
        }

        void IDataConsumer.UpdateChildrenData()
        {
            measurements.UpdateChildrenData();
        }

        int IDataConsumer.Count
        {
            get { return measurements.Count; }
        }


        IMeasurements IDataConsumer.this[int n]
        {
            get { return measurements[n]; }
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

        #region IPostSetArrow


        void IPostSetArrow.PostSetArrow()
        {
            measures.Clear();
            try
            {
                foreach (string key in sounds.Keys)
                {
                    measures[this.FindMeasurement(key, false)] = sounds[key];
                }
            }
            catch (Exception ex)
            {
                ex.ShowError(1);
            }
        }

        #endregion

        #region ITimeMeasureConsumer Members

        IMeasurement ITimeMeasureConsumer.Time
        {
            get
            {
                return timeMeasure;
            }
            set
            {
                timeMeasure = value;
            }
        }

        #endregion

        #region IMeasurements Members

        int IMeasurements.Count
        {
            get { return 0; }
        }

        IMeasurement IMeasurements.this[int number]
        {
            get { return null; }
        }

        void IMeasurements.UpdateMeasurements()
        {
        }

        bool IMeasurements.IsUpdated
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        #endregion

        #region IRealtimeUpdate Members

        Action IRealtimeUpdate.Update
        {
            get
            {
                return RealtimeUpdate;
            }
        }

    
        event Action IRealtimeUpdate.OnUpdate
        {
            add {  }
            remove {  }
        }

   

        #endregion

        #region Own Members

        #region Public

        /// <summary>
        /// Dictionary of sounds
        /// </summary>
        public Dictionary<string, string> Sounds
        {
            get
            {
                return sounds;
            }
        }


        /// <summary>
        /// Sources
        /// </summary>
        public List<string> Sources
        {
            get
            {
                return this.GetAllMeasurements(this.GetDesktop(), false);
            }
        }

        /// <summary>
        /// Directory of sounds
        /// </summary>
        internal static string SoundDirectory
        {
            get
            {
                return soundDirectory;
            }
            set
            {
                soundDirectory = value;
            }
        }

        /// <summary>
        /// Play sound event
        /// </summary>
        public event Action<string> PlaySound
        {
            add { playSound += value; }
            remove { playSound -= value; }
        }

        #endregion

        #region Private

        void RealtimeUpdate()
        {
            foreach (IMeasurement m in measures.Keys)
            {
                if ((bool)m.Parameter())
                {
                    playSound(measures[m]);
                }
            }
        }


        #endregion

        #endregion
   }
}