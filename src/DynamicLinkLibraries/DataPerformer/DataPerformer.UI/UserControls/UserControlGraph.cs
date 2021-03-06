using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;

using CategoryTheory;

using Diagram.UI;
using Diagram.UI.Interfaces;
using Diagram.UI.Utils;
using Diagram.UI.Labels;

using BaseTypes;
using BaseTypes.Attributes;


using DataPerformer.Interfaces;
using DataPerformer.Portable;
using DataPerformer.Portable.DifferentialEquationProcessors;
using DataPerformer.Interfaces.BufferedData.Interfaces;

using DataPerformer.UI;
using DataPerformer.UI.Objects;


using Event.Interfaces;
using Event.Portable;
using Event.Log.Database.Interfaces;

using Animation.Interfaces;

using ToolBox;

using Chart;
using Chart.Interfaces;
using Chart.Drawing;
using Chart.Drawing.Interfaces;
using Chart.Objects;
using Chart.Panels;

using DataPerformer.UI.Interfaces;

using Event.UI;


using WindowsExtensions;

namespace DataPerformer.UI.UserControls
{
    /// <summary>
    /// User control for graph
    /// </summary>
    public partial class UserControlGraph : UserControl, IBlocking,
        IMouseChartIndicator, IStartStop, IStartStopConsumer, IGraphLabel, IEnabled,
        IAnimationParameters, IPreRemove, IRealtimeUpdate, IRealTimeStartStop,
        IExecuteCommand, ICalculationReason, IPreparation, IPostSet
    {

        #region Specific Fields

        internal Dictionary<IMeasurement, ParametrizedSeries> SeriesDictionary;

        Dictionary<string, object> dcadr = new Dictionary<string, object>();

        List<Tuple<double, Dictionary<IMeasurement, object>>> listAnalysis;

        internal IndicatorWrapper indicatorWrapper = new IndicatorWrapper();

        /// <summary>
        /// The "undefined argument message"
        /// </summary>
        public const string UndefinedArgument = "Argument of function is not defined";

        OfficePickers.ColorPicker.ToolStripColorPicker pic = new OfficePickers.ColorPicker.ToolStripColorPicker();

        private Dictionary<TextBox, IMeasurement> dicMea = new Dictionary<TextBox, IMeasurement>();

        IList<IParameterWriter> pw = new List<IParameterWriter>();

        private Chart.ChartPerformer performer;

        private LogarithmCoordinator coordinator;

        private List<ISeries> ownSeries = new List<ISeries>();

        private IDataConsumer consumer;

        private ITimeMeasureConsumer timeConsumer;

        private Dictionary<IMeasurement, MeasurementsDisasseblyWrapper> measurementsWrapperDictionary = null;

        List<MeasurementsDisasseblyWrapper> measurementsWrapperList = new List<MeasurementsDisasseblyWrapper>();

        private Size inSize;

        private bool isBlocked = false;

        private LogHolder log;

        private string mode = "";

        bool textMode = false;

        Dictionary<IMeasurement, Color[]> dcolorAnalysis;

        Tuple<Dictionary<string, Color[]>, Dictionary<string, bool>,
             Dictionary<string, string>, string[], int[],
              Tuple<double[],
             Dictionary<string, Dictionary<string,
             Tuple<Color[], bool, double[]>>>>[]>
            data = null;

        private static readonly Color[] orderedColors = new Color[]
        {
            Color.Red, Color.Green, Color.Blue, Color.Goldenrod, Color.Fuchsia, Color.Ivory, Color.Khaki
        };

        Func<bool> condition;

        Action realtimeRun;

        Action realtimeStop;

        Panel pan;

        Tuple<double, Dictionary<IMeasurement, object>> currentMeasurementObject = null;

        private PictureBox pb = new PictureBox();

        private Label lt = new Label();

        Form form = null;

        IDesktop desktop;

        IStartStop parent;

        string globalArg;

        string[] globalFunc;

        Dictionary<IMeasurement, Color[]> dmta = null;

        Dictionary<string, IMeasurement> dta = null;

        Dictionary<IMeasurement, object> dobta = null;

        Dictionary<string, object[]> dsta = null;

        Dictionary<string, object> dicto;

        Exception exto;

        IObjectLabel parentLab;

        private bool text;

        static private readonly Image im = ResourceImage.Graph.ToBitmap();

        IDifferentialEquationProcessor processor;

        IMeasurement mc;

        

        private Action internalTextAction;

        /// <summary>
        /// Array of arguments
        /// </summary>
        private Array array = null;

        /// <summary>
        /// Change agument action
        /// </summary>
        private event Action<string> changeArgument = (string s) => { };

        private ToolStripButton[][] startStopPauseButtons;

        private object type;

        private event Action<bool> changeAbsolute = (bool b) => { };

        private event Action realtimeupdate = () => { };

        bool resizeLab;

        private Dictionary<string, string> dicText = new Dictionary<string, string>();

        private TimeType timeType;

        List<Tuple<double, Dictionary<string, object>>> lan = null;

        string currentCommand = "";

        volatile System.Threading.AutoResetEvent analysisPause;

        string calculationReason = "";

        bool beginAnalysis = true;

        ILogItem item = null;

        private int currentCadr = 0;

        private Dictionary<string, IMeasurement> textDictionary;

        XElement textXML;

        string analysisXML;

        IChangeBufferItem changeBufferItem;

        bool bufferItemChanged = false;

        Dictionary<string, object> cadrExt = new Dictionary<string, object>();

        #endregion

        #region Ctor

        internal UserControlGraph()
            : this(false)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resizeLab"></param>
        public UserControlGraph(bool resizeLab)
        {
            InitializeComponent();
            Action act = null;
            this.CreateEventActions(ref realtimeupdate, ref act, ref act);
            startStopPauseButtons = new ToolStripButton[][] { new ToolStripButton[]
            {toolStripButtonStart, toolStripButtonAnimation},
            toolStripButtonStop.ToSingleArray<ToolStripButton>(),
            toolStripButtonPause.ToSingleArray<ToolStripButton>()
            };
            this.resizeLab = resizeLab;
            if (resizeLab)
            {
                inSize = Size;
            }
            PreInit();
            realtimeupdate += indicatorWrapper.UpdateIndicators;
            //tabControlMain.TabPages.Remove(tabPageAnimation);
            tabControlMain.TabPages.Remove(tabPageStartStopRealtime);
        }

        #endregion


        #region IPostSet Members

        void IPostSet.Post()
        {
            try
            {
                postInit();
                refresh();
                performer.Add(this);
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
            }
        }

        #endregion



        #region Create form members

        /// <summary>
        /// Post operation
        /// </summary>
        public void Post()
        {
       /*!!! BAD    try
            {
                postInit();
                refresh();
                performer.Add(this);
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
            }
            */
        }

        #endregion

        #region IExecuteCommand Members

        void IExecuteCommand.Execute(string command)
        {
            if (currentCommand.Equals(""))
            {
                if (command.Equals("start"))
                {
                    currentCommand = command;
                    this.InvokeIfNeeded(StartRealtimeClick); ;
                    return;
                }
            }
            if (currentCommand.Equals("start"))
            {
                if (command.Equals("stop"))
                {
                    this.InvokeIfNeeded(() => { StopRealTime(); });
                }
            }
        }

        #endregion

        #region IBlocking Members

        bool IBlocking.IsBlocked
        {
            get
            {
                return isBlocked;
            }
            set
            {
                isBlocked = value;
            }
        }

        #endregion

        #region IMouseChartIndicator Members

        void IMouseChartIndicator.Indicate(double x, double y)
        {
            labelX.Text = "X = " + x;
            labelY.Text = "Y = " + y;
        }

        bool IMouseChartIndicator.IsEnabled
        {
            get
            {
                return labelX.Visible;
            }
            set
            {
                labelX.Visible = value;
                labelY.Visible = value;
            }
        }

        #endregion

        #region IStartStop Members

        void IStartStop.Action(object type, ActionType actionType)
        {
            if (actionType == ActionType.Start)
            {
                this.type = type;
            }
            this.InvokeIfNeeded<object, ActionType>(StartControl, type, actionType);
        }

        #endregion

        #region IStartStopConsumer Members

        IStartStop IStartStopConsumer.StartStop
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }

        #endregion

        #region IGraphLabel Members

        Tuple<Dictionary<string, Color[]>, Dictionary<string, bool>,
            Dictionary<string, string>, string[], int[],
             Tuple<double[],
            Dictionary<string, Dictionary<string,
            Tuple<Color[], bool, double[]>>>>[]> IGraphLabel.Data
        {
            get
            {
                throw new Exception("Get data from this control is prohibited");
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                if (data != null)
                {
                    throw new Exception("Data fo this control is already exists");
                }
                data = value;
                FillPoints();
                numericUpDownPause.Value = value.Item5[0];
            }
        }

        #endregion

        #region IEnabled Members

        bool IEnabled.Enabled
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                {
                    FillMeasurements();
                }
            }
        }

        #endregion

        #region IAnimationParameters Members

        object IAnimationParameters.Parameters
        {
            get
            {
                double start = Double.Parse(calculatorBoxStart.Text);
                double step = Double.Parse(calculatorBoxStep.Text);

                int steps = Int32.Parse(textBoxStepCount.Text);
                IGraphLabel l = this;
                int i = 0;
                return new object[] { start, step, steps, data.Item5[0], i };
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
                return realtimeupdate;
            }
        }


        event Action IRealtimeUpdate.OnUpdate
        {
            add { }
            remove { }
        }


        #endregion

        #region IRealTimeStartStop Members

        void IRealTimeStartStop.Start()
        {
            if (calculationReason.Equals(StaticExtensionEventInterfaces.PureRealtimeLogAnalysis))
            {
                return;
            }
            this.InvokeIfNeeded(() =>
            {
                toolStripButtonStart.Enabled = false;
                toolStripButtonStop.Enabled = true;
                if (calculationReason.Equals(StaticExtensionEventInterfaces.RealtimeLogAnalysis))
                {
                    toolStripButtonPause.Enabled = true;
                }
            });
        }

        void IRealTimeStartStop.Stop()
        {
            if (calculationReason.Equals(StaticExtensionEventInterfaces.PureRealtimeLogAnalysis))
            {
                return;
            }
            this.InvokeIfNeeded(() =>
            {
                toolStripButtonStart.Enabled = true;
                toolStripButtonStop.Enabled = false;
                toolStripButtonPause.Enabled = false;
            });
        }

        event Action IRealTimeStartStop.OnStart
        {
            add { }
            remove { }
        }

        event Action IRealTimeStartStop.OnStop
        {
            add { }
            remove { }
        }


        #endregion

        #region IPreRemove Members

        void IPreRemove.PreRemove()
        {
            consumer.OnChangeInput -= Fill;
        }

        #endregion

        #region ICalculationReason Members

        string ICalculationReason.CalculationReason
        {
            get
            {
                return calculationReason;
            }

            set
            {
                calculationReason = value;
            }
        }

        #endregion

        #region IPreparation Members

        void IPreparation.Prepare()
        {
            SaveSettings();
        }

        #endregion

        #region Members

        #region Internal

        internal Dictionary<string, object> CadrExternal
        {
            get
            {
                return cadrExt;
            }
        }

        internal Dictionary<string, object> CadrMeasurements
        {
            get
            {
                return dcadr;
            }
        }

        internal void StartIterator(int number)
        {
            IIterator iterator = Iterator;
            if (iterator != null)
            {
                StartIteratorInternal(iterator, number);
                return;
            }
            object l = Log;
            if (l != null)
            {
                //!!!! BAD IDEA 
                // StartLogInternal(l, number);
            }
        }

        internal IObjectLabel ParentLabel
        {
            set
            {
                parentLab = value;
                if (parentLab is Labels.GraphLabel)
                {
                    Labels.GraphLabel gl = parentLab as Labels.GraphLabel;
                    checkBoxSynchronous.Checked = (gl.AnimationType == global::Animation.Interfaces.Enums.AnimationType.Synchronous);
                    userControlTimeUnitAnimation.TimeUnit = gl.TimeUnitAnimation;
                    userControlTimeUnitAnimation.ChangeTimeUnit += userControlTimeUnitAnimation_ChangeTimeUnit;
                    textBoxAnimationScale.Text = gl.TimeScaleAnimation + "";
                }
            }
        }

        internal event Action<bool> ChangeAbsoluteTime
        {
            add { changeAbsolute += value; }
            remove { changeAbsolute -= value; }
        }

        internal event Action<TimeType> ChangeTimeUnit
        {
            add { userControlTimeType.ChangeTimeUnit += value; }
            remove { userControlTimeType.ChangeTimeUnit -= value; }
        }

        internal bool IsAbsuluteTime
        {
            get
            {
                return checkBoxAbsoluteTime.Checked;
            }
            set
            {
                checkBoxAbsoluteTime.Checked = value;
            }
        }

        internal TimeType TimeUnit
        {
            get
            {
                return userControlTimeType.TimeUnit;
            }
            set
            {
                userControlTimeType.TimeUnit = value;
            }
        }

        /// <summary>
        /// Change agument actoion
        /// </summary>
        internal event Action<string> ChangeArgument
        {
            add { changeArgument += value; }
            remove { changeArgument -= value; }
        }

        internal void Set(IDataConsumer dataConsumer, Tuple<Dictionary<string, Color[]>, Dictionary<string, bool>,
            Dictionary<string, string>, string[], int[],
             Tuple<double[],
            Dictionary<string, Dictionary<string,
            Tuple<Color[], bool, double[]>>>>[]> data)
        {
            consumer = dataConsumer as DataConsumer;
            timeConsumer = consumer as ITimeMeasureConsumer;
            Init();
            userControlRealtime.Set(dataConsumer, data);
        }

        internal IDesktop OwnDesktop
        {
            set
            {
                desktop = value;
            }
        }

        internal void PreInit()
        {
            pan = panelCenter;
            pb.Image = im;
            lt.Text = "Enlarge me";
            Control p = pan.Parent;
            pb.Width = im.Width;
            pb.Height = im.Height;
            pb.Top = 30;
            pb.Left = 30;
            p.Controls.Add(pb);
            lt.Top = pb.Bottom + 3;
            lt.Left = pb.Left;
            //p.Controls.Add(lt);
            tabPageGraph.Controls.Add(lt);
            SetVisible();
        }

        internal void SetVisible()
        {
            if (!resizeLab)
            {
                SetVisible(true);
            }
            if (Size.Width < inSize.Width + 10 | Size.Height < inSize.Height + 10)
            {
                SetVisible(false);
                return;
            }
            SetVisible(true);
        }

        internal void Init()
        {
            PanelChart panel = new PanelChart(new int[,] { { 80, 30 }, { 10, 40 } });
            panel.Cursor = Cursors.Cross;
            performer = panel.Performer;
            panelGraph.Controls.Add(panel);
            panel.Dock = DockStyle.Fill;
            performer.Resize();
            coordinator = new LogarithmCoordinator(performer);
            performer.Coordinator = coordinator;
            EditorReceiver.AddEditorDrag(panelGraph);
            PictureReceiver.AddImageDrag(panelGraph);
            DataConsumer consumer = this.consumer as DataConsumer;
            ArrayList graphControls = consumer.GraphControls;
            ControlPanel.LoadControls(panelGraph, graphControls);
            calculatorBoxStart.Text = consumer.Start + "";
            calculatorBoxStep.Text = consumer.Step + "";
            textBoxStepCount.Text = consumer.Steps + "";
            pic.Text = ResourceService.Resources.GetControlResource("Color", DataPerformer.UI.Utils.ControlUtilites.Resources);
            pic.Width = 50;
            StaticExtensionDataPerformerUI.FillSeriesTypeCombo(toolStripButtonType);
            performer.MouseDown += (object sender, MouseEventArgs args) =>
            {
                if (!performer.IsMoved)
                {
                    return;
                }
                if (args.Button == MouseButtons.Right)
                {
                    MoveToX(performer.CurrentX);
                }
            };
        }

        internal string ArgumentString
        {
            get
            {
                return data.Item4[1];
            }
        }

        internal IMeasurement Argument
        {
            get
            {
                object o = comboBoxArg.SelectedItem;
                if (o == null)
                {
                    data.Item4[1] = "Time";
                    changeArgument(data.Item4[1]);
                    return StaticExtensionDataPerformerPortable.Factory.TimeProvider.TimeMeasurement;
                }
                data.Item4[1] = o + "";
                changeArgument(data.Item4[1]);
                return consumer.FindMeasurement(data.Item4[1], true);
            }
        }

        internal Dictionary<string, object[]> Parameters
        {
            get
            {
                IMeasurement arg = Argument;
                Dictionary<string, object[]> d = new Dictionary<string, object[]>();
                return d;
            }
        }

        internal event EventHandler Start
        {
            add
            {
                toolStripButtonStart.Click += value;
            }
            remove
            {
                toolStripButtonStop.Click -= value;
            }
        }

 
        #endregion

        #region Private

        void MoveToX(double x)
        {
            ISeries series = performer.First;
            if (series != null)
            {
                int n = series.GetArgumentPosition(x);
                this.FindChild<Graph.UserControlCadr>().Cadr = n;
                tabControlMain.SelectedTab = tabPageCadr;
            }
        }

        IIterator Iterator
        {
            get
            {
                IDesktop desktop = (consumer as IAssociatedObject).GetRootDesktop();
                IIterator iterator = null;
                desktop.ForEach((BelongsToCollectionPortable b) =>
                {
                    if (b.Source == consumer)
                    {
                        object o = b.Target;
                        if (o is IIterator)
                        {
                            iterator = o as IIterator;
                        }
                    }
                });
                return iterator;
            }
        }

        private object Log
        {
            get
            {
                object l = null;
                IDesktop desktop = (consumer as IAssociatedObject).GetRootDesktop();
                desktop.ForEach((BelongsToCollectionPortable b) =>
                {
                    if (b.Source == consumer)
                    {
                        object o = b.Target;
                        if (o is LogHolder)
                        {
                            log = o as LogHolder;
                            (log as IAssociatedObject).Prepare(true);
                            l = log.Reader;
                        }
                    }
                });
                return l;
            }
        }
/*
        void SaveSettings()
        {
            this.InvokeIfNeeded(SaveSettingsPrivate);
        }
*/
        void SaveSettings()
        {
            object o = comboBoxArg.SelectedItem;
            if (o == null)
            {
                data.Item4[1] = "Time";
                changeArgument(data.Item4[1]);
            }
            data.Item4[1] = o + "";
            Dictionary<string, IMeasurement> t = TextDictionary;
            Dictionary<IMeasurement, Color[]> mc = MeasureColorDictionary;
            object si = comboBoxCond.SelectedItem;
            if (si != null)
            {
                data.Item4[0] = si + "";
            }
            else
            {
                data.Item4[0] = "";
            }
            DataConsumer consumer = this.consumer as DataConsumer;
            if (toolStripComboBoxPoints.SelectedItem != null)
            {
                data.Item4[5] = toolStripComboBoxPoints.SelectedItem + "";
                array = (consumer as ICategoryObject).GetOneDimensionRealArray(data.Item4[5]);
                if (array == null)
                {
                    data.Item4[5] = "";
                }
            }
            else
            {
                data.Item4[5] = "";
            }
            data.Item5[0] = (int)numericUpDownPause.Value;
            consumer.Start = double.Parse(calculatorBoxStart.Text);
            consumer.Step = double.Parse(calculatorBoxStep.Text);
            string sc;
            consumer.Steps = GetValue(comboBoxStepCount, textBoxStepCount, out sc);
            data.Item4[4] = sc;
            this.FindChild<UserControlRealtime>().SaveSettings();
        }

        bool StopRealTime()
        {
            StaticExtensionDataPerformerUI.Interrupt();
            if (StaticExtensionEventPortable.IsRunning)
            {
                StaticExtensionEventPortable.StopRealTime();
                buttonStartStopRealtime.Text = "Start";
                buttonStartStopRealtime.BackColor = Color.Green;
                currentCommand = "";
                return true;
            }
            return false;
        }

        void ChangeBufferItem(IBufferItem item)
        {
            bufferItemChanged = true;
        }

        void fillMea()
        {
            comboBoxArg.Items.Clear();
            panelMea.Controls.Clear();
            int y = 0;
            IDataConsumer consumer = this.consumer;
            if (consumer == null)
            {
                return;
            }
            for (int i = 0; i < consumer.Count; i++)
            {
                IMeasurements measurements = consumer[i];
                PanelMeasureGraph panel = new PanelMeasureGraph(measurements, consumer as DataConsumer, data.Item1);
                panel.Width = panelMea.Width - 10;
                Panel sep = new Panel();
                sep.Width = panel.Width;
                sep.BackColor = Color.Black;
                sep.Height = 3;
                sep.Left = 0;
                sep.Top = y;
                panelMea.Controls.Add(sep);
                y += sep.Height;
                panelMea.Controls.Add(panel);
                panel.Left = 0;
                panel.Top = y;
                y += panel.Height + 1;

            }
        }

        private static Color OrderedColor(int i)
        {
            int k = i % orderedColors.Length;
            return orderedColors[k];
        }

        void Change(ILogItem item)
        {
            beginAnalysis = true;
            currentCadr = 0;
            this.item = item;
        }

        IChangeLogItem Item
        {
            get
            {
                if (log != null)
                {
                    object r = log.Reader;
                    if (r is IChangeLogItem)
                    {
                        return r as IChangeLogItem;
                    }
                }
                return null;
            }
        }

        private void StopAnimationAuto()
        {
            IStartStop ss = this;
            ss.Action(null, ActionType.Stop);
        }

        double TimeScale
        {
            get
            {
                return double.Parse(textBoxAnimationScale.Text);
            }
        }

        TimeSpan Pause
        {
            get
            {
                object[] ap = (this as IAnimationParameters).Parameters as object[];
                return TimeSpan.FromMilliseconds((int)ap[3]);

            }
        }

        private void StopCalc(IAsynchronousCalculation calc)
        {
            if (calc == null)
            {
                return;
            }
            calc.Finish += StopAnimationAuto;
            calc.OnInterrupt += StopAnimationAuto;
        }

        private void StartAnimation(Animation.Interfaces.Enums.AnimationType aType)
        {
            this.type = null;
            string[] rearsons = new string[] { aType.GetReason() };
            double timeScale = TimeScale;
            TimeSpan ts = Pause;
            string reason = aType.GetReason();
            string[] reasons = new string[] { reason };
            if (aType == Animation.Interfaces.Enums.AnimationType.Asynchronous)
            {
                DataConsumer consumer = this.consumer as DataConsumer;
                if (parentLab is Labels.GraphLabel)
                {
                    Labels.GraphLabel gl = parentLab as Labels.GraphLabel;
                    gl.TimeScaleAnimation = timeScale;
                }
                IComponentCollection collection = consumer.CreateCollection(reason);
                IAsynchronousCalculation calc = collection.StartAnimation(reasons, aType, Pause, TimeScale, false, false);
                StopCalc(calc);
                Action<Action> animation = (Action act) =>
                {
                    double start = consumer.Start;
                    double step = consumer.Step;
                    int count = consumer.Steps;
                    consumer.PerformFixed(start, step, count,
                       StaticExtensionDataPerformerPortable.Factory.TimeProvider,
                        DifferentialEquationProcessor.Processor, reason, 1, act, calc);
                };
                StaticExtensionDataPerformerUI.StartSynchronousAnimation(this, animation);
            }
            else
            {
                DataConsumer consumer = this.consumer as DataConsumer;
                IComponentCollection collection = consumer.CreateCollection(reason);
                IAsynchronousCalculation calc = collection.StartAnimation(reasons, aType, Pause, TimeScale, false, false);
                StopCalc(calc);
                Action<Action> animation = (Action act) =>
                {
                    double start = consumer.Start;
                    double step = consumer.Step;
                    int count = consumer.Steps;
                    consumer.PerformFixed(start, step, count, DataPerformer.Portable.StaticExtensionDataPerformerPortable.Factory.TimeProvider,
                    DifferentialEquationProcessor.Processor, reason, 1, act, calc);
                };
                StaticExtensionDataPerformerUI.StartSynchronousAnimation(this, animation);

            }
        }

        private void ResizeLabel(object sender, EventArgs e)
        {
            SetVisible();
        }

        private void GetParent(Control c)
        {
            if (parentLab != null)
            {
                return;
            }
            Control p = c.Parent;
            if (p == null)
            {
                return;
            }
            if (p is IObjectLabel)
            {
                parentLab = p as IObjectLabel;
                return;
            }
            GetParent(p);
        }

        private void SetVisible(bool viz)
        {
            try
            {
                //pan.Visible = viz;
                foreach (Control c in tabPageGraph.Controls)
                {
                    c.Visible = viz;
                }
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
            }
            try
            {
                //lt.Visible = !viz;
            }
            catch (Exception exc)
            {
                exc.ShowError(10);
            }
            try
            {
                pb.Visible = !viz;
            }
            catch (Exception exce)
            {
                exce.ShowError(10);
            }

        }

        void postInit()
        {
            if (data.Item5[1] > 0)
            {
                splitContainerGraph.SplitterDistance = data.Item5[1];
            }
            IDesktop d = parentLab.Desktop;
            List<string> series = StaticExtensionDataPerformerUI.GetSeries(d);
            toolStripButtonSeries.FillCombo(series);
            toolStripMain.Items.Add(pic);
            SetVisible();
            FillMeasurements();
            consumer.OnChangeInput += Fill;
            if (resizeLab)
            {
                base.Resize += ResizeLabel;
            }
        }

        void Fill()
        {
            FillMeasurements();
            userControlRealtime.Set(consumer, data);
        }

        void ShowRuntimeIndicators()
        {
            Labels.GraphLabel gl = this.FindParent<Labels.GraphLabel>();
            Dictionary<string, Size> sizes = gl.sizes;
            indicatorWrapper.sizes = sizes;
            IMeasurementObjectFactory f = StaticExtensionDataPerformerUI.GraphCollection;
            IDataConsumer c = consumer;
           // indicators.RemoveMeasurementObjects();
            //!!! ALL  INDICATORS c.GetMeasurementObjects(indicators, f);
            Dictionary<IMeasurement, string> d = c.GetMeasurementsDictionary();
            UserControlRealtime uc = this.FindChild<UserControlRealtime>();
            List<IMeasurement> l = Current;
            indicatorWrapper.Show(c, l, sizes);
        }

        void FillMeasurements()
        {
            userControlRealtime.FillMeasurements();
            FillPoints();
            comboBoxArg.Items.Clear();
            panelMea.Controls.Clear();
            /*      if (ucmg != null)
                  {
                      if (!ucmg.IsDisposed)
                      {
                          ucmg.Dispose();
                          ucmg = null;
                      }
                  }
                  ucmg = UserControlMeasureGraph.Create(consumer);
                  if (ucmg == null)
                  {
                      return;
                  }
                  ucmg.Data = data;
                  ucmg.Dock = DockStyle.Top;
                  try
                  {
                      panelMea.Controls.Add(ucmg);
                  }
                  catch (Exception ex)
                  {
                      ex.ShowError();
                  }
                  */
            fillMea();
            int n = -1;
            for (int i = 0; i < consumer.Count; i++)
            {

                IMeasurements arrow = consumer[i];
                string name = consumer.GetMeasurementsName(arrow);
                for (int j = 0; j < arrow.Count; j++)
                {
                    IMeasurement m = arrow[j];
                    string s = name + "." + m.Name;
                    comboBoxArg.Items.Add(s);
                    if (s.Equals(data.Item4[1]))
                    {
                        n = j;
                    }

                }
            }
            if (n >= 0)
            {
                comboBoxArg.SelectedIndex = n;
            }
            else
            {
                comboBoxArg.SelectedIndex = -1;
                comboBoxArg.SelectedItem = null;
                comboBoxArg.Text = "Time";
            }

            bool b = false;
            IList<string> l = consumer.GetAllMeasurements(b);
            dicMea.Clear();
            panelText.Controls.Clear();
            comboBoxCond.FillCombo(l);
            comboBoxCond.SelectCombo(data.Item4[0]);
            int y = 0;
            int w = panelText.Width;
            for (int i = 0; i < consumer.Count; i++)
            {
                IMeasurements m = consumer[i];
                string name = consumer.GetMeasurementsName(m);
                Panel pm = new PanelMeasureText(consumer, consumer[i], w, dicMea, name, data.Item3);
                pm.Top = y;
                panelText.Controls.Add(pm);
                y = pm.Bottom;
            }
            double a = 0;
            IList<string> ld = consumer.GetAllMeasurements(a);
            comboBoxStart.FillCombo(ld);
            comboBoxStart.SelectCombo(data.Item4[2]);
            comboBoxStep.FillCombo(ld);
            comboBoxStep.SelectCombo(data.Item4[3]);
            int ni = 0;
            IList<string> li = consumer.GetAllMeasurements(ni);
            comboBoxStepCount.FillCombo(li);
            comboBoxStepCount.SelectCombo(data.Item4[4]);
        }

        private int GetValue(ComboBox cb, TextBox tb, out string str)
        {
            if (cb.SelectedItem == null)
            {
                str = "";
                return int.Parse(tb.Text);
            }
            str = cb.SelectedItem + "";
            IMeasurement m = consumer.FindMeasurement(str, false);
            int i = (int)m.Parameter();
            tb.Text = i + "";
            return i;

        }

        private void StartChart()
        {
            try
            {
                if (array == null)
                {
                    PerformFixed();
                }
                else
                {
                    performArray();
                }
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
                ShowErrorLocal(ex);
            }
        }

        private void StartText()
        {
            try
            {
                if (array != null)
                {
                    performArrayText();
                }
                else
                {
                    performFixedText();
                }
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
                ShowErrorLocal(ex);
            }
        }

        private void CondWrite()
        {
            bool cond = (bool)mc.Parameter();
            if (cond)
            {
                WriteText();
            }
        }

        private void TextAction()
        {
            internalTextAction();
            if (backgroundWorkerText.CancellationPending)
            {
                StaticExtensionDataPerformerPortable.StopRun();
            }
        }

        private void performArrayText()
        {
            try
            {
                internalTextAction = (mc == null) ? new Action(CondWrite) : WriteText;
                consumer.PerformArray(array,
                    (consumer as ICategoryObject).GetRootDesktop(),
                    StaticExtensionDataPerformerPortable.Factory.TimeProvider,
                    processor, 0, TextAction, null);
            }
            catch (Exception e)
            {
                e.ShowError(10);
                exto = e;
            }
        }

        private void performFixedText()
        {
            try
            {
                DataConsumer consumer = this.consumer as DataConsumer;
                consumer.PerformFixed(consumer.Start, consumer.Step, consumer.Steps,
                    DataPerformer.Portable.StaticExtensionDataPerformerPortable.Factory.TimeProvider, processor,
                    StaticExtensionDataPerformerInterfaces.Calculation,
                    0, TextAction);
            }
            catch (Exception e)
            {
                if (!e.IsFiction())
                {
                    e.ShowError(10);
                    exto = e;
                }
            }
        }

        private Dictionary<string, IMeasurement> TextDictionary
        {
            get
            {
                data.Item3.Clear();
                Dictionary<string, IMeasurement> d = new Dictionary<string, IMeasurement>();
                foreach (TextBox tb in dicMea.Keys)
                {
                    string key = tb.Text;
                    if (key.Length > 0)
                    {
                        d[key] = dicMea[tb];
                        PanelMeasureText p = tb.Parent as PanelMeasureText;
                        string n = p.MeasureName + "." + dicMea[tb].Name;
                        data.Item3[n] = key;
                    }
                }
                return d;
            }
        }

        private void WriteText()
        {
            foreach (TextBox tb in dicMea.Keys)
            {
                string key = tb.Text;
                if (key.Length > 0)
                {
                    dicText[key] = dicMea[tb].Parameter() + "";
                }
            }
            foreach (IParameterWriter wr in pw)
            {
                wr.Write(dicText);
            }
        }

        private void WritePar()
        {
            data.Item3.Clear();
            foreach (TextBox tb in dicMea.Keys)
            {
                string key = tb.Text;
                if (key.Length > 0)
                {
                    PanelMeasureText p = tb.Parent as PanelMeasureText;
                    string n = p.MeasureName + "." + dicMea[tb].Name;
                    data.Item3[n] = key;
                }
            }
        }

        private void removeOwn()
        {
            foreach (ISeries s in ownSeries)
            {
                performer.Remove(s);
            }
            ownSeries.Clear();
        }

        void refresh()
        {
            INamedComponent nc = parentLab;
            List<string> series = StaticExtensionDataPerformerUI.GetSeries(nc.Desktop);
            toolStripButtonSeries.FillCombo(series);

        }

        void ShowErrorLocal(Exception ex)
        {
            if (DataConsumer.IsInterrupted(ex))
            {
                //return;
            }
            else
            {
                ex.ShowError(0);
            }
            if (!toolStripButtonStart.Enabled)
            {
                ActParent(ActionType.Stop, null);
            }
        }

        private void ActParent(ActionType actionType, object obj)
        {
            Action<ActionType> act = (ActionType at) =>
            {
                if (parent != null)
                {
                    parent.Action(obj, at);
                }
                if (!StaticExtensionDataPerformerUI.IsRunning)
                {
                    actionType.EnableDisableButtons(startStopPauseButtons);
                }
                if (actionType == ActionType.Start)
                {
                    if (obj != null)
                    {
                        toolStripButtonPause.Enabled = false;
                    }
                }
                else if (actionType == ActionType.Stop)
                {
                    StaticExtensionDataPerformerUI.StopCurrentCalculation();
                }
            };
            this.InvokeIfNeeded<ActionType>(act, actionType);
        }

        private void StartControl(object type, ActionType actionType)
        {
            actionType.EnableDisableButtons(startStopPauseButtons);
            Diagram.UI.StaticExtensionDiagramUIForms.Action(form, type, actionType);
        }

        void StartRealtimeAnalysis(object l, Func<object, bool> stop)
        {
            if (l is IIterator)
            {
                StartRealtimeAnalysis(l as IIterator, (() => { return stop(null); }));
            }
            else
            {
                UserControlRealtime rt = this.FindChild<UserControlRealtime>();
                consumer.RealtimeAnalysis(l, stop, StaticExtensionEventInterfaces.RealtimeLogAnalysis,
                    TimeType.Second, checkBoxAbsoluteTime.Checked);
            }
        }

        void StartRealtimeAnalysis(IIterator l, Func<bool> stop)
        {
            UserControlRealtime rt = this.FindChild<UserControlRealtime>();
            if (l is IChangeBufferItem)
            {
                changeBufferItem = l as IChangeBufferItem;
                changeBufferItem.Change += ChangeBufferItem;
            }
            consumer.PerformIterator(l, l as ITimeMeasureProvider,
                StaticExtensionEventInterfaces.RealtimeLogAnalysis, stop);
        }

        void StartRealtimeClick()
        {
            currentCommand = "start";
            DataConsumer consumer = this.consumer as DataConsumer;
            IDesktop desktop = (consumer as ICategoryObject).GetRootDesktop();
            IComponentCollection collection = consumer.CreateCollection(StaticExtensionEventInterfaces.Realtime);
            IAsynchronousCalculation animation =
                collection.StartAnimation(new string[] {StaticExtensionEventInterfaces.Realtime,
             AnimationType.GetReason()}, AnimationType, Pause, TimeScale, true, checkBoxAbsoluteTime.Checked);
            if (animation != null)
            {
                StaticExtensionEventPortable.OnceStop(animation.Interrupt);
            }
            StaticExtensionEventInterfaces.NewLog = null;
            buttonStartStopRealtime.Text = "Stop";
            buttonStartStopRealtime.BackColor = Color.Red;
            userControlRealtime.Realtime = collection.StartRealtime(userControlTimeType.TimeUnit,
                checkBoxAbsoluteTime.Checked, animation,
                consumer, StaticExtensionEventInterfaces.NewLog, StaticExtensionEventInterfaces.Realtime);
        }

        IEnumerable<PanelMeasureGraph> Panels
        {
            get
            {
                foreach (Control c in panelMea.Controls)
                {
                    if (c is PanelMeasureGraph)
                    {
                        yield return c as PanelMeasureGraph;
                    }
                }

            }
        }

        Dictionary<IMeasurement, Color[]> MeasureColorDictionary
        {
            get
            {
                Dictionary<IMeasurement, Color[]> d = new Dictionary<IMeasurement, Color[]>();
                data.Item1.Clear();
                Dictionary<IMeasurement, string> dd = consumer.GetAllMeasurementsName();
                foreach (PanelMeasureGraph pg in Panels)
                {
                    Dictionary<IMeasurement, Color> dp = pg.MeasurementDictionary;
                    foreach (IMeasurement m in dp.Keys)
                    {
                        Color[] c = new Color[] { dp[m] };
                        d[m] = c;
                        if (dd.ContainsKey(m))
                        {
                            data.Item1[dd[m]] = c;
                        }
                    }
                }
                return d;
            }
        }

        Dictionary<string, IMeasurement> MeasureByNameInternal
        {

            get
            {
                Dictionary<IMeasurement, Color[]> d = MeasureColorDictionary;
                Dictionary<string, IMeasurement> dd = consumer.GetAllMeasurementsByName();
                Dictionary<string, IMeasurement> dictionary = new Dictionary<string, IMeasurement>();
                foreach (string key in dd.Keys)
                {
                    IMeasurement m = dd[key];
                    if (d.ContainsKey(m))
                    {
                        dictionary[key] = m;
                    }
                }
                return dictionary;

            }
        }

        Dictionary<string, IMeasurement> MeasurementsByName
        {
            get
            {

                Dictionary<string, IMeasurement> d = MeasureByNameInternal; //  ucmg.MeasureByName;
                List<string> l = new List<string>(d.Keys);
                measurementsWrapperDictionary = consumer.CreateDisassemblyMeasurements();
                foreach (string key in l)
                {
                    IMeasurement m = d[key];
                    if (measurementsWrapperDictionary.ContainsKey(m))
                    {
                        //d.Remove(key);
                        Color[] c = null;
                        if (dmta.ContainsKey(m))
                        {
                            c = dmta[m];
                        }
                        string s = key.Substring(0, key.IndexOf('.') + 1);
                        MeasurementsDisasseblyWrapper mw = measurementsWrapperDictionary[m];
                        int i = 0;
                        foreach (IMeasurement mm in mw.Measurements)
                        {
                            d[s + mm.Name] = mm;
                            if (c != null)
                            {
                                Color[] col = new Color[c.Length + 1];
                                Array.Copy(c, col, c.Length);
                                col[c.Length] = OrderedColor(i);
                                dmta[mm] = col;
                            }
                            ++i;
                        }
                    }
                }
                return d;
            }
        }

        void StartChartClick()
        {
            try
            {
                DataConsumer consumer = this.consumer as DataConsumer;
                type = 0;
                text = false;
                ActParent(ActionType.Start, global::Animation.Interfaces.Enums.ActionType.Calculation);
                IDataRuntime runtime = consumer.CreateRuntime(StaticExtensionDataPerformerInterfaces.Calculation);
                double st = Double.Parse(calculatorBoxStart.Text);
                runtime.StartAll(st);
                consumer.FullReset();
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        consumer.UpdateChildrenData();
                        break;
                    }
                    catch (Exception ex)
                    {
                        ex.ShowError(10);
                        if (i == 9)
                        {
                            this.ShowErrorLocal(ex);
                            ActParent(ActionType.Stop, null);
                            return;
                        }
                    }
                }
                consumer.Start = double.Parse(calculatorBoxStart.Text);
                consumer.Step = double.Parse(calculatorBoxStep.Text);
                string sc;
                consumer.Steps = GetValue(comboBoxStepCount, textBoxStepCount, out sc);
                data.Item4[4] = sc;
                consumer.GraphControls = ControlPanel.GetControls(panelGraph);
                IMeasurement arg = Argument;
                data.Item1.Clear(); ;
                List<string> val = new List<string>();
                dobta = new Dictionary<IMeasurement, object>();
                Dictionary<IMeasurement, Color[]> d = MeasureColorDictionary;
                if (d != null)
                {
                    dmta = d;
                    ///!!!!!     ucmg.ObjectDictionary = dobta;
                    dta = MeasurementsByName;
                    foreach (string key in dta.Keys)
                    {
                        if (dmta.ContainsKey(dta[key]))
                        {
                            val.Add(key);
                        }
                    }
                    globalArg = data.Item4[1];
                }
                else
                {
                }
                globalFunc = val.ToArray();
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
                ControlExtensions.ShowMessageBoxModal("Refresh please");
                ActParent(ActionType.Stop, null);
                return;
            }
            backgroundWorker.RunWorkerAsync();
        }

        void StartTextClick()
        {

            DataConsumer consumer = this.consumer as DataConsumer;
            try
            {
                Action<string> act = (string filename) =>
                {
                    text = true;
                    pw.Clear();
                    IParameterWriter w = new XmlParameterWriter(filename);
                    pw.Add(w);
                    mc = null;
                    IObjectLabel label = parentLab;
                    object si = comboBoxCond.SelectedItem;
                    if (si != null)
                    {
                        internalTextAction = CondWrite;
                        data.Item4[0] = si + "";
                        mc = consumer.FindMeasurement(data.Item4[0], false);
                    }
                    else
                    {
                        data.Item4[0] = "";
                        internalTextAction = WriteText;
                    }
                    dicText.Clear();
                    WritePar();
                    consumer.Start = Double.Parse(calculatorBoxStart.Text);
                    consumer.Step = Double.Parse(calculatorBoxStep.Text);
                    consumer.Steps = Int32.Parse(textBoxStepCount.Text);
                    double start = consumer.Start;
                    double step = consumer.Step;
                    int count = consumer.Steps;
                    IObjectLabel lab = parentLab;
                    IDesktop d = lab.Desktop.Root;
                    IDataRuntime rt = StaticExtensionDataPerformerPortable.Factory.Create(d, 0);
                    processor = DifferentialEquationProcessor.Processor;
                    if (processor != null)
                    {
                        processor.Set(d);
                    }
                    IMeasurement arg = Argument;
                    if (arg == null)
                    {
                        return;
                    }
                    removeOwn();
                    ActParent(ActionType.Start,
                        Animation.Interfaces.Enums.ActionType.Calculation);
                    rt.Refresh();
                    double st = double.Parse(calculatorBoxStart.Text);
                    rt.StartAll(st);
                    consumer.FullReset();
                    for (int i = 0; i < 10; i++)
                    {
                        try
                        {
                            consumer.UpdateChildrenData();
                            break;
                        }
                        catch (Exception exx)
                        {
                            exx.ShowError(10);
                            if (i == 9)
                            {
                                ShowErrorLocal(exx);
                                ActParent(ActionType.Stop, null);
                                return;
                            }
                        }
                    }
                };
                this.SaveXml(act);
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
                ControlExtensions.ShowMessageBoxModal("Refresh please");
                ActParent(ActionType.Stop, null);
                return;
            }
            backgroundWorkerText.RunWorkerAsync();
        }

        private void PerformFixed()
        {
            try
            {
                dicto =
                    (consumer as DataConsumer).PerformFixed(globalArg, globalFunc, () => backgroundWorker.CancellationPending, measurementsWrapperDictionary);
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
                exto = ex;
            }
        }

        private void performArray()
        {
            try
            {
                dicto =
                    (consumer as DataConsumer).PerformArray(array, globalArg, globalFunc, () => backgroundWorker.CancellationPending);
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
                exto = ex;
            }
        }

        private bool CommonComplete()
        {
            if (exto != null)
            {
                ShowErrorLocal(exto);
                exto = null;
                return true;
            }
            return false;
        }

        private void FillPoints()
        {
            if (IsDisposed)
            {
                return;
            }
            if (toolStripComboBoxPoints.SelectedItem != null)
            {
                return;
            }
            if (consumer != null)
            {
                if (toolStripComboBoxPoints.Items.Count == 0)
                {
                    string[] ss = (consumer as ICategoryObject).GetOneDimensionRealArrays();
                    toolStripComboBoxPoints.FillCombo(ss);
                }
                toolStripComboBoxPoints.SelectCombo(data.Item4[5]);
            }
        }

        string ConvertDouble(string s)
        {
            double a = double.Parse(s);
            return a.StringValue();
        }

        private Animation.Interfaces.Enums.AnimationType AnimationType
        {
            get
            {
                return checkBoxSynchronous.Checked ?
                Animation.Interfaces.Enums.AnimationType.Synchronous :
                Animation.Interfaces.Enums.AnimationType.Asynchronous;
            }
        }

        void SaveExportXml()
        {

            string cond = null;
            object o = comboBoxCond.SelectedItem;
            if (o != null)
            {
                cond = o + "";
            }
            object a = Argument;
            IAssociatedObject ao = consumer as IAssociatedObject;
            string name = ao.GetAbsoluteName();
            data.Item3.Clear();
            WritePar();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<WriteText/>");
            string[] sa = new string[] { "Start", "Step", "Finish" };
            double ss = Double.Parse(calculatorBoxStart.Text);
            string sta = ss.StringValue();
            double ff = ss + (Double.Parse(calculatorBoxStep.Text) * Int32.Parse(textBoxStepCount.Text));
            string fin = ff.StringValue();
            string[] sp = new string[] { sta, textBoxStepCount.Text, fin };
            XmlElement inter = doc.CreateElement("Interval");
            for (int i = 0; i < 3; i++)
            {
                XmlElement e = doc.CreateElement(sa[i]);
                inter.AppendChild(e);
                e.InnerText = sp[i];
            }
            XmlElement r = doc.DocumentElement;
            r.AppendChild(inter);
            XmlElement cn = doc.CreateElement("ChartName");
            cn.InnerText = name;
            r.AppendChild(cn);
            if (cond != null)
            {
                XmlElement cc = doc.CreateElement("Condition");
                cc.InnerText = cond;
                r.AppendChild(cc);
            }
            XmlElement arg = doc.CreateElement("Argument");
            arg.InnerText = data.Item4[1];
            r.AppendChild(arg);
            XmlElement p = doc.CreateElement("Parameters");
            r.AppendChild(p);
            foreach (string n in data.Item3.Keys)
            {
                XmlElement e = doc.CreateElement("Parameter");
                p.AppendChild(e);
                XmlElement nn = doc.CreateElement("Name");
                nn.InnerText = n;
                e.AppendChild(nn);
                XmlElement v = doc.CreateElement("Value");
                v.InnerText = data.Item3[n];
                e.AppendChild(v);
            }
            this.SaveXml(doc);
        }

        #endregion

        #endregion

        #region Event handlers

        private void toolStripButtonAdd_Click(object sender, EventArgs e)
        {
            INamedComponent nc = parentLab;
            IDesktop d = nc.Desktop;
            ISeriesPainter painter = StaticExtensionDataPerformerUI.SelectPainter(toolStripButtonType.SelectedItem + "",
                new Color[] { pic.Color }, performer);
            if (painter == null)
            {
                return;
            }
            object o = d.GetObject(toolStripButtonSeries.SelectedItem + "");
            if (o == null)
            {
                return;
            }
            ISeries series = null;
            if (o is ISeries)
            {
                series = o as ISeries;
                if (series is DrawSeries)
                {
                    DrawSeries ds = series as DrawSeries;
                    painter = ds.Factory.GetPainter(performer);
                }
            }
            if (o is Series)
            {
                Series s = o as Series;
                series = new SeriesGraph(s);
            }
            performer.AddSeries(series, painter);
            performer.RefreshAll();
            Refresh();

        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            StartChart();
            ActParent(ActionType.Start, global::Animation.Interfaces.Enums.ActionType.Calculation);
            this.InvokeIfNeeded(() => { toolStripButtonStop.Enabled = false; });
        }

        private void StartTextAnalysis(object l)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter =
                ResourceService.Resources.GetControlResource("Xml Files|*.xml",
                Utils.ControlUtilites.Resources);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            analysisXML = dialog.FileName;
            beginAnalysis = true;
            textDictionary = TextDictionary;
            StaticExtensionDataPerformerUI.initText(consumer);
            object si = comboBoxCond.SelectedItem;
            if (si != null)
            {
                data.Item4[0] = si + "";
                mc = consumer.FindMeasurement(data.Item4[0], false);
            }
            else
            {
                data.Item4[0] = "";
            }
            measurementsWrapperList.Clear();
            measurementsWrapperDictionary = consumer.CreateDisassemblyMeasurements();
            List<IMeasurement> lm = new List<IMeasurement>();
            foreach (IMeasurement mm in measurementsWrapperDictionary.Keys)
            {
                string k = null;
                foreach (string key in textDictionary.Keys)
                {
                    if (mm == textDictionary[key])
                    {
                        k = key;
                        break;
                    }
                }
                if (k == null)
                {
                    continue;
                }
                textDictionary.Remove(k);
                MeasurementsDisasseblyWrapper wr = measurementsWrapperDictionary[mm];
                measurementsWrapperList.Add(wr);
                foreach (IMeasurement measurement in wr.Measurements)
                {
                    textDictionary[measurement.Name] = measurement;
                }
            }
            condition = (mc == null) ? new Func<bool>(() => { return true; }) :
                () => { return (bool)mc.Parameter(); };
            textXML = XElement.Parse("<Analysis/>");
            Dictionary<IMeasurement, MeasurementsDisasseblyWrapper> disassemblyDictionary =
                consumer.CreateDisassemblyMeasurements();
            Func<object>[] provider = new Func<object>[1];
            IDataConsumer dc = consumer;
            double time = 0;
            ITimeMeasureConsumer tc = consumer as ITimeMeasureConsumer;
            Action<object> act = (object sender) =>
            {
                StaticExtensionDataPerformerUI.performText(sender);
                time = tc.GetTime();
                if (!condition())
                {
                    return;
                }
                XElement e = XElement.Parse("<Cadr/>");
                textXML.Add(e);
                XElement xt = XElement.Parse("<Time/>");
                e.Add(xt);
                xt.Add(time + "");
                XElement xc = XElement.Parse("<Number/>");
                e.Add(xc);
                xc.Add(currentCadr + "");
                ++currentCadr;
                if (item != null)
                {
                    XElement xi = XElement.Parse("<FileName/>");
                    e.Add(xi);
                    xi.Add((item as ILogData).FileName);
                }
                XElement measurements = XElement.Parse("<Measurements/>");
                e.Add(measurements);
                foreach (MeasurementsDisasseblyWrapper wr in measurementsWrapperList)
                {
                    wr.Update();
                }
                foreach (string key in textDictionary.Keys)
                {
                    XElement measurement = XElement.Parse("<Measurement/>");
                    measurements.Add(measurement);
                    measurement.SetNameValue(key, textDictionary[key].Parameter() + "");
                }
            };

            Func<object, bool> stop = (object o) =>
            {
                analysisPause.WaitOne();
                analysisPause.Set();
                if (backgroundWorkerTextRealtimeAnalysis.CancellationPending)
                {
                    return true;
                }
                act(o);
                return false;
            };
            realtimeRun = () => { StartRealtimeAnalysis(l, stop); };
            backgroundWorkerTextRealtimeAnalysis.RunWorkerAsync();
        }

        void StartIterator(IIterator iterator, int number = 0)
        {
            userControlRealtimeAnalysis.Visible = true;
            beginAnalysis = true;
            userControlRealtimeAnalysis.Refresh();
            StartIteratorInternal(iterator, number);
        }

        void StartLogInternal(object log, int number = 0)
        {

            IEnumerable<object> en = consumer.RealtimeAnalysisEnumerable(
                  log,
                  (object o) => { return false; }, StaticExtensionDataPerformerInterfaces.Calculation,
                   TimeType.Second, true);
    
        }

        void StartIteratorInternal(IIterator iterator, int number = 0)
        { 
            /*   Dictionary<IMeasurement, MeasurementsDisasseblyWrapper> disassemblyDictionary = 
                   new Dictionary<IMeasurement, MeasurementsDisasseblyWrapper>();
               Dictionary<IMeasurement, BaseTypes.Interfaces.IDisassemblyObject> disassemblyDict =
                consumer.CreateDisassemblyObjectDictionary();
               foreach (IMeasurement key in disassemblyDict.Keys)
               {
                   disassemblyDictionary[key] = new
                       MeasurementsDisasseblyWrapper(disassemblyDict[key], key);
               }*/
            Dictionary<IMeasurement, MeasurementsDisasseblyWrapper> disassemblyDictionary =
                consumer.CreateDisassemblyMeasurements();
            Func<object>[] provider = new Func<object>[1];
            IDataConsumer dc = consumer;
            dcolorAnalysis = MeasureColorDictionary;
            List<IMeasurement> lmm = new List<IMeasurement>(dcolorAnalysis.Keys);
            List<IMeasurement> all = new List<IMeasurement>();
            Dictionary<IMeasurement, string> dmea = new Dictionary<IMeasurement, string>();
            Dictionary<IMeasurement, string> dmeafull = new Dictionary<IMeasurement, string>();
            Dictionary<IMeasurement, IMeasurements> dmeamea = new Dictionary<IMeasurement, IMeasurements>();
            for (int i = 0; i < consumer.Count; i++)
            {
                IMeasurements mmm = dc[i];
                string n = consumer.GetMeasurementsName(mmm) + ".";
                for (int j = 0; j < mmm.Count; j++)
                {
                    IMeasurement mmmm = mmm[j];
                    dmeamea[mmmm] = mmm;
                    all.Add(mmmm);
                    string namm = n + mmmm.Name;
                    if (dcolorAnalysis.ContainsKey(mmmm))
                    {
                        dmea[mmmm] = namm;
                    }
                    dmeafull[mmmm] = namm;
                }
            }
            foreach (IMeasurement mmm in lmm)
            {
                Color[] c = dcolorAnalysis[mmm];
                if (disassemblyDictionary.ContainsKey(mmm))
                {
                    dcolorAnalysis.Remove(mmm);
                    IMeasurements mtt = dmeamea[mmm];
                    dmeamea.Remove(mmm);
                    string n = consumer.GetMeasurementsName(mtt) + ".";
                    all.Remove(mmm);
                    IMeasurement[] mmp = disassemblyDictionary[mmm].Measurements;
                    int cn = 0;
                    foreach (IMeasurement mmmp in mmp)
                    {
                        string nnn = n + mmmp.Name;
                        dmea[mmmp] = nnn;
                        dmeafull[mmmp] = nnn;
                        List<Color> cc = new List<Color>(c);
                        cc.Add(OrderedColor(cn));
                        dcolorAnalysis[mmmp] = cc.ToArray();
                        ++cn;
                        all.Add(mmmp);
                    }
                    if (dmea.ContainsKey(mmm))
                    {
                        dmea.Remove(mmm);
                    }
                }
            }
            List<IMeasurement> mt = new List<IMeasurement>(dcolorAnalysis.Keys);
            double a = 0;
            float b = 0;
            int cccc = 0;
            foreach (IMeasurement mtt in mt)
            {
                object t = mtt.Type;
                if (!(t.Equals(a) | (t.Equals(b)) | t.Equals(cccc)))
                {
                    if (dcolorAnalysis.ContainsKey(mtt))
                    {
                        dcolorAnalysis.Remove(mtt);
                    }
                }
            }
            listAnalysis = new List<Tuple<double, Dictionary<IMeasurement, object>>>();
            Dictionary<IMeasurement, object> dmo = new Dictionary<IMeasurement, object>();
            Dictionary<string, object> ddcolor = new Dictionary<string, object>();
            Dictionary<string, object> dd = new Dictionary<string, object>();
            List<string> lm = new List<string>(dmea.Values);
            lm.Sort();
            userControlRealtimeAnalysis.Names = lm;

            List<object> sel = new List<object>();
            IEnumerable<IEvent> events = (consumer as IEventHandler).Events;
            foreach (IEvent ev in events)
            {
                sel.Add(ev);
                if (ev is IChildrenObject)
                {
                    sel.AddRange((ev as IChildrenObject).GetChildren<object>());
                }
            }
            double last = 0;
            double t0 = 0;
            double time = 0;
            double startTime = 0;
            bool first = true;
            ITimeMeasureConsumer tc = consumer as ITimeMeasureConsumer;
            int currNum = 0;
            double tstep = 0;
            Dictionary<string, IMeasurement> mbn = MeasurementsByName;
            Action act = () =>
            {
                time = tc.GetTime();
                if (first)
                {
                    startTime = -time;
                    first = false;
                }
                else if (bufferItemChanged)
                {
                    startTime = last - time;
                    bufferItemChanged = false;
                }
                time += startTime;
                last = time;
                ddcolor.Clear();
                dd.Clear();
                Dictionary<IMeasurement, object> current = new Dictionary<IMeasurement, object>();
                try
                {
                    foreach (MeasurementsDisasseblyWrapper wr in disassemblyDictionary.Values)
                    {
                        wr.Update();
                    }
                    ++currNum;
                    if (number != 0)
                    {
                        if (currNum == number)
                        {
                            cadrExt.Clear();
                            dcadr.Clear();
                            foreach (IMeasurement m in all)
                            {
                                object par = m.Parameter();
                                dmo[m] = par;
                                string namd = dmeafull[m];
                                if (dcolorAnalysis.ContainsKey(m))
                                {
                                    ddcolor[namd] = par;
                                }
                                dcadr[namd] = par;
                                if (mbn.ContainsValue(m))
                                {
                                    cadrExt[namd] = par;
                                }
                                dmo[m] = par;
                            }
                        }
                        return;
                    }
                    foreach (IMeasurement m in all)
                    {
                        object par = m.Parameter();
                        dmo[m] = par;
                        string namd = dmeafull[m];
                        if (dcolorAnalysis.ContainsKey(m))
                        {
                            ddcolor[namd] = par;
                        }
                        dd[namd] = par;
                        dmo[m] = par;
                    }
                }
                catch (Exception)
                {
                    return;
                }
                userControlRealtimeAnalysis.Dictionary = dd;
                StaticExtensionDataPerformerUI.UpdateAnalysisUI();
                userControlRealtimeAnalysis.Time = time;
                foreach (IMeasurement m in dcolorAnalysis.Keys)
                {
                    current[m] = dmo[m];
                }
                double tt = (checkBoxDirectoryIteration.Checked) ? tstep : time;
                listAnalysis.Add(
                    new Tuple<double, Dictionary<IMeasurement, object>>(tt, current));
                ++tstep;
                indicatorWrapper.UpdateIndicators();
            };

            Func<bool> stop = () =>
            {
                analysisPause.WaitOne();
                analysisPause.Set();
                if (backgroundWorkerReatimeAnalysis.CancellationPending)
                {
                    return true;
                }
                act();
                return false;
            };
            if (number > 0)
            {
                stop = () =>
                {
                    act();
                    return currNum == number;
                };
            }
            realtimeRun = () =>
            {
                StartRealtimeAnalysis(iterator, stop);
            };
            realtimeStop = () =>
            {
                SeriesDictionary = new Dictionary<IMeasurement, ParametrizedSeries>();
                Func<Func<object>> x = () => { return () => { return currentMeasurementObject.Item1; }; };
                foreach (IMeasurement m in dcolorAnalysis.Keys)
                {
                    Func<Func<object>> y = () =>
                    {
                        return () =>
                        {
                            object o = currentMeasurementObject.Item2[m];
                            double real = 0;
                            if (o is double)
                            {
                                real = (double)o;
                            }
                            else if (o is float)
                            {
                                float fl = (float)o;
                                real = fl;
                            }
                            else if (o is int)
                            {
                                int integer = (int)o;
                                real = integer;
                            }
                            return real;
                        };
                    };
                    SeriesDictionary[m] = new ParametrizedSeries(x, y);
                }
                foreach (Tuple<double, Dictionary<IMeasurement, object>> tp in listAnalysis)
                {
                    currentMeasurementObject = tp;
                    foreach (ParametrizedSeries s in SeriesDictionary.Values)
                    {
                        s.Step();
                    }
                }
                userControlRealtimeAnalysis.Visible = false;
                performer.RemoveAll();
                foreach (IMeasurement m in SeriesDictionary.Keys)
                {
                    performer.AddSeries(SeriesDictionary[m], dcolorAnalysis[m]);
                }
                 performer.RefreshAll();
                StaticExtensionEventPortable.PostStop();
            };
            //  Func<object, bool> f = (object o) => { act(o); return false; };
            // StartRealtimeAnalysis(l, f);
            //   realtimeStop();
            //  return;
            if (number == 0)
            {
                toolStripButtonPause.Enabled = true;
                toolStripButtonStop.Enabled = true;
                toolStripButtonStart.Enabled = false;
                backgroundWorkerReatimeAnalysis.RunWorkerAsync();
                return;
            }
            realtimeRun();
        }

        List<IMeasurement> Current
        {
            get
            {
                if (tabControlMain.SelectedTab == tabPageRealTime)
                {
                    UserControlRealtime uc = this.FindChild<UserControlRealtime>();
                    return uc.Measurements;
                }
                return new List<IMeasurement>(MeasureColorDictionary.Keys);
            }
        }

        private void StartAnalysis(object l)
        {
            userControlRealtimeAnalysis.Visible = true;
            beginAnalysis = true;
            userControlRealtimeAnalysis.Refresh();
            /*   Dictionary<IMeasurement, MeasurementsDisasseblyWrapper> disassemblyDictionary = 
                   new Dictionary<IMeasurement, MeasurementsDisasseblyWrapper>();
               Dictionary<IMeasurement, BaseTypes.Interfaces.IDisassemblyObject> disassemblyDict =
                consumer.CreateDisassemblyObjectDictionary();
               foreach (IMeasurement key in disassemblyDict.Keys)
               {
                   disassemblyDictionary[key] = new
                       MeasurementsDisasseblyWrapper(disassemblyDict[key], key);
               }*/
            Dictionary<IMeasurement, MeasurementsDisasseblyWrapper> disassemblyDictionary =
                consumer.CreateDisassemblyMeasurements();
            Func<object>[] provider = new Func<object>[1];
            IDataConsumer dc = consumer;
            dcolorAnalysis = MeasureColorDictionary;
            List<IMeasurement> lmm = new List<IMeasurement>(dcolorAnalysis.Keys);
            List<IMeasurement> all = new List<IMeasurement>();
            Dictionary<IMeasurement, string> dmea = new Dictionary<IMeasurement, string>();
            Dictionary<IMeasurement, string> dmeafull = new Dictionary<IMeasurement, string>();
            Dictionary<IMeasurement, IMeasurements> dmeamea = new Dictionary<IMeasurement, IMeasurements>();
            for (int i = 0; i < consumer.Count; i++)
            {
                IMeasurements mmm = dc[i];
                string n = consumer.GetMeasurementsName(mmm) + ".";
                for (int j = 0; j < mmm.Count; j++)
                {
                    IMeasurement mmmm = mmm[j];
                    dmeamea[mmmm] = mmm;
                    all.Add(mmmm);
                    string namm = n + mmmm.Name;
                    if (dcolorAnalysis.ContainsKey(mmmm))
                    {
                        dmea[mmmm] = namm;
                    }
                    dmeafull[mmmm] = namm;
                }
            }
            foreach (IMeasurement mmm in lmm)
            {
                Color[] c = dcolorAnalysis[mmm];

                if (disassemblyDictionary.ContainsKey(mmm))
                {
                    dcolorAnalysis.Remove(mmm);
                    IMeasurements mtt = dmeamea[mmm];
                    dmeamea.Remove(mmm);
                    string n = consumer.GetMeasurementsName(mtt) + ".";
                    all.Remove(mmm);
                    IMeasurement[] mmp = disassemblyDictionary[mmm].Measurements;
                    int cn = 0;
                    foreach (IMeasurement mmmp in mmp)
                    {
                        string nnn = n + mmmp.Name;
                        dmea[mmmp] = nnn;
                        dmeafull[mmmp] = nnn;
                        List<Color> cc = new List<Color>(c);
                        cc.Add(OrderedColor(cn));
                        dcolorAnalysis[mmmp] = cc.ToArray();
                        ++cn;
                        all.Add(mmmp);
                    }
                    if (dmea.ContainsKey(mmm))
                    {
                        dmea.Remove(mmm);
                    }
                }
            }
            List<IMeasurement> mt = new List<IMeasurement>(dcolorAnalysis.Keys);
            double a = 0;
            float b = 0;
            int cccc = 0;

            foreach (IMeasurement mtt in mt)
            {
                object t = mtt.Type;
                if (!(t.Equals(a) | (t.Equals(b)) | t.Equals(cccc)))
                {
                    if (dcolorAnalysis.ContainsKey(mtt))
                    {
                        dcolorAnalysis.Remove(mtt);
                    }
                }
            }
            listAnalysis = new List<Tuple<double, Dictionary<IMeasurement, object>>>();
            Dictionary<IMeasurement, object> dmo = new Dictionary<IMeasurement, object>();
            Dictionary<string, object> ddcolor = new Dictionary<string, object>();
            Dictionary<string, object> dd = new Dictionary<string, object>();
            List<string> lm = new List<string>(dmea.Values);
            lm.Sort();
            userControlRealtimeAnalysis.Names = lm;
            List<object> sel = new List<object>();
            IEnumerable<IEvent> events = (consumer as IEventHandler).Events;
            foreach (IEvent ev in events)
            {
                sel.Add(ev);
                if (ev is IChildrenObject)
                {
                    sel.AddRange((ev as IChildrenObject).GetChildren<object>());
                }
            }
            double last = 0;
            double t0 = 0;
            double time = 0;
            ITimeMeasureConsumer tc = consumer as ITimeMeasureConsumer;
            DateTime timeStart = DateTime.Now;
            DateTime ct = timeStart;
            double lastDt = 0;
    
            Action<object> act = (object sender) =>
            {
                time = tc.GetTime();
                if (beginAnalysis)
                {
                    timeStart = DateTime.Now;
                    ct = timeStart;

                    if (time != 0)
                    {
                        double dt = time;
                        t0 = time;
                        last = time;
                        beginAnalysis = false;
                        userControlRealtimeAnalysis.Time = dt;
                    }
                    else
                    {
                        userControlRealtimeAnalysis.Time = 0;
                    }
                }
                else
                {
                    ct = DateTime.Now;
                    TimeSpan ts = ct - timeStart;
                    double ms = ts.TotalSeconds;
                    double dt = (time - t0);
                    double delta = dt - lastDt;
                    delta += dt - ms;
                    if (delta > 0)
                    {
                        System.Threading.Thread.Sleep((int)(1000 * delta));
                        lastDt = dt;
                    }
                    else
                    {

                    }
                 }
                if (sender is Tuple<DateTime, INativeEvent, Dictionary<string, object>>)
                {
                    Tuple<DateTime, INativeEvent, Dictionary<string, object>> tt = sender as Tuple<DateTime, INativeEvent, Dictionary<string, object>>;
                    if (!sel.Contains(tt.Item2))
                    {
                        return;
                    }
                }
                else
                {
                    Tuple<INativeReader, object[], DateTime> tt = sender as Tuple<INativeReader, object[], DateTime>;
                    if (!sel.Contains(tt.Item1))
                    {
                        return;
                    }
                }
                ddcolor.Clear();
                dd.Clear();
                Dictionary<IMeasurement, object> current = new Dictionary<IMeasurement, object>();
                try
                {
                    foreach (MeasurementsDisasseblyWrapper wr in disassemblyDictionary.Values)
                    {
                        wr.Update();
                    }
                    foreach (IMeasurement m in all)
                    {
                        object par = m.Parameter();
                        dmo[m] = par;
                        string namd = dmeafull[m];
                        if (dcolorAnalysis.ContainsKey(m))
                        {
                            ddcolor[namd] = par;
                        }
                        dd[namd] = par;
                        dmo[m] = par;
                    }
                }
                catch (Exception)
                {
                    return;
                }
                userControlRealtimeAnalysis.Dictionary = dd;
                StaticExtensionDataPerformerUI.UpdateAnalysisUI();
                userControlRealtimeAnalysis.Time = time - t0;
                foreach (IMeasurement m in dcolorAnalysis.Keys)
                {
                    current[m] = dmo[m];
                }
                listAnalysis.Add(
                    new Tuple<double, Dictionary<IMeasurement, object>>(time - t0, current));
            };
            Func<object, bool> stop = (object o) =>
            {
                if (o is Tuple<DateTime, INativeEvent, Dictionary<string, object>>)
                {
                    Tuple<DateTime, INativeEvent, Dictionary<string, object>> t = o as Tuple<DateTime, INativeEvent, Dictionary<string, object>>;
                    if (t.Item3.Count == 0)
                    {
                        return false;
                    }
                }
                analysisPause.WaitOne();
                analysisPause.Set();
                if (backgroundWorkerReatimeAnalysis.CancellationPending)
                {
                    return true;
                }
                act(o);
                return false;
            };
            realtimeRun = () => { StartRealtimeAnalysis(l, stop); };
            realtimeStop = () =>
            {
                SeriesDictionary = new Dictionary<IMeasurement, ParametrizedSeries>();
                Func<Func<object>> x = () => { return () => { return currentMeasurementObject.Item1; }; };
                foreach (IMeasurement m in dcolorAnalysis.Keys)
                {
                    Func<Func<object>> y = () =>
                    {
                        return () =>
                        {
                            object o = currentMeasurementObject.Item2[m];
                            double real = 0;
                            if (o is double)
                            {
                                real = (double)o;
                            }
                            else if (o is float)
                            {
                                float fl = (float)o;
                                real = fl;
                            }
                            else if (o is int)
                            {
                                int integer = (int)o;
                                real = integer;
                            }
                            return real;
                        };
                    };
                    SeriesDictionary[m] = new ParametrizedSeries(x, y);
                }
                foreach (Tuple<double, Dictionary<IMeasurement, object>> tp in listAnalysis)
                {
                    currentMeasurementObject = tp;
                    foreach (ParametrizedSeries s in SeriesDictionary.Values)
                    {
                        s.Step();
                    }
                }
                userControlRealtimeAnalysis.Visible = false;
                performer.RemoveAll();
                foreach (IMeasurement m in SeriesDictionary.Keys)
                {
                    performer.AddSeries(SeriesDictionary[m], dcolorAnalysis[m]);
                }
                performer.RefreshAll();
                StaticExtensionEventPortable.PostStop();
            };
            Func<object, bool> f = (object o) => { act(o); return false; };
            toolStripButtonPause.Enabled = true;
            toolStripButtonStop.Enabled = true;
            toolStripButtonStart.Enabled = false;
            backgroundWorkerReatimeAnalysis.RunWorkerAsync();
        }

        private void toolStripButtonStart_Click(object sender, EventArgs e)
        {
            array = null;
            if (toolStripComboBoxPoints.SelectedItem != null)
            {
                data.Item4[5] = toolStripComboBoxPoints.SelectedItem + "";
                array = (consumer as ICategoryObject).GetOneDimensionRealArray(data.Item4[5]);
                if (array == null)
                {
                    data.Item4[5] = "";
                }
            }
            else
            {
                data.Item4[5] = "";
            }
            TabPage page = tabControlMain.SelectedTab;
            IIterator iterator = Iterator;
            object l = Log;
            if ((l != null) | (iterator != null))
            {
                if (page == tabPageGraph | page == tabPageText)
                {
                    if (analysisPause != null)
                    {
                        analysisPause.Set();
                        toolStripButtonPause.Enabled = true;
                        toolStripButtonStop.Enabled = true;
                        toolStripButtonStart.Enabled = false;
                        return;
                    }
                    mode = StaticExtensionEventInterfaces.RealtimeLogAnalysis;
                    lan = new List<Tuple<double, Dictionary<string, object>>>();
                    if (page == tabPageGraph)
                    {
                        if (l != null)
                        {
                            StartAnalysis(l);
                        }
                        if (iterator != null)
                        {
                            StartIterator(iterator);
                        }
                    }
                    else
                    {
                        if (l != null)
                        {
                            StartTextAnalysis(l);
                        }
                        if (iterator != null)
                        {
                            StartTextAnalysis(iterator);
                        }
                    }
                    return;
                }
            }
            if (page == tabPageGraph)
            {
                StartChartClick();
                return;
            }
            else if (page == tabPageText)
            {
                StartTextClick();
            }
            else if (page == tabPageRealTime)
            {
                StartRealtimeClick();
            }
            else if (page == tabPageAnimation)
            {
                StartAnimation(AnimationType);
            }
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            if (StopRealTime())
            {
                return;
            }
            if (mode.Equals(StaticExtensionEventInterfaces.RealtimeLogAnalysis))
            {
                if (analysisPause != null)
                {
                    analysisPause.Set();
                }
                if (textMode)
                {
                    backgroundWorkerTextRealtimeAnalysis.CancelAsync();
                }
                else
                {
                    backgroundWorkerReatimeAnalysis.CancelAsync();
                }
            }
            if (type != null)
            {
                type = 0;
                if (text)
                {
                    backgroundWorkerText.CancelAsync();
                }
                else
                {
                    backgroundWorker.CancelAsync();
                }
            }
            else
            {
                StaticExtensionDataPerformerUI.StopAnimation(this);
            }
            ActParent(ActionType.Stop, null);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (CommonComplete())
            {
                return;
            }
            else
            {
                try
                {
                    performer.Remove(typeof(DynamicSeriesAttribute));
                    /*     if (ucmg != null)
                         {
                             if (dicto != null)
                             {
                                 Dictionary<string, string> d = new Dictionary<string, string>();
                                 foreach (string key in dicto.Keys)
                                 {
                                     foreach (string s in dta.Keys)
                                     {
                                         if (key.Length > s.Length)
                                         {
                                             if (key.Contains(s))
                                             {
                                                 d[key] = s;
                                                 break;
                                             }
                                         }
                                     }
                                 }
                                 foreach (string key in dicto.Keys)
                                 {
                                     object o = dicto[key];
                                     if (!dta.ContainsKey(key))
                                     {
                                         string st = d[key];
                                     }
                                     IMeasurement m = dta[key];
                                     dobta[m] = o;
                                     SeriesTypes.ParametrizedSeries s = o as SeriesTypes.ParametrizedSeries;

                                     ParametrizedSeries series = new ParametrizedSeries(null, null);
                                     series.Add(s);
                                     //b.Tag = series;
                                     bool isStep = false;
                                     if ( data.Item2.ContainsKey(key))
                                     {
                                         isStep = data.Item2[key];
                                     }
                                     Color[] color = dmta[m];

                                     ISeriesPainter painter = isStep ?
                                         new Chart.Drawing.Painters.StepSeriesPainter(color) :
                                         new Chart.Drawing.Painters.SimpleSeriesPainter(color);
                                     performer.AddSeries(series, painter);
                                     ownSeries.Add(series);
                                 }
                             }
                         }
                         else*/
                    {
                        Dictionary<IMeasurement, Color[]> d = MeasureColorDictionary;
                        Dictionary<string, IMeasurement> dd = MeasureByNameInternal;
                        foreach (string key in dicto.Keys)
                        {
                            SeriesTypes.ParametrizedSeries ps = dicto[key] as SeriesTypes.ParametrizedSeries;
                            ParametrizedSeries series = new ParametrizedSeries(null, null);
                            series.Add(ps);

                            performer.AddSeries(series, d[dd[key]][0]);
                            ownSeries.Add(series);
                            /*
                            object[] ob = dsta[key];
                            ob[0] = o;
                            Button b = ob[2] as Button;
                            b.Tag = o;
                            if (!(o is DataPerformer.SeriesTypes.ParametrizedSeries))
                            {
                                b.Tag = new object[] { key, o };
                                continue;
                            }
                            DataPerformer.SeriesTypes.ParametrizedSeries s = ps as DataPerformer.SeriesTypes.ParametrizedSeries;

                            ParametrizedSeries series = new ParametrizedSeries(null, null);
                            series.Add(s);
                            //b.Tag = series;
                            OfficePickers.ColorPicker.ComboBoxColorPicker p = ob[3] as OfficePickers.ColorPicker.ComboBoxColorPicker;
                            performer.AddSeries(series, p.Color);
                            ownSeries.Add(series);*/
                        }
                    }
                    performer.RefreshAll();
                }
                catch (Exception ex)
                {
                    ex.ShowError(10);
                    ShowErrorLocal(ex);
                }
            }
            ActParent(ActionType.Stop, null);
        }

        private void backgroundWorkerText_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (CommonComplete())
            {
                return;
            }
            foreach (IParameterWriter wr in pw)
            {
                wr.Flush();
            }
            ActParent(ActionType.Stop, null);
        }

        private void backgroundWorkerText_DoWork(object sender, DoWorkEventArgs e)
        {
            StartText();
             this.InvokeIfNeeded(() =>
             {
                ActParent(ActionType.Start, global::Animation.Interfaces.Enums.ActionType.Calculation);
                toolStripButtonStop.Enabled = false;
             });
        }

        private void toolStripButtonAnimation_Click(object sender, EventArgs e)
        {
            StartAnimation(AnimationType);
        }

        private void toolStripButtonPause_Click(object sender, EventArgs e)
        {
            if (analysisPause != null)
            {
                analysisPause.Reset();
                toolStripButtonPause.Enabled = false;
            }
            StaticExtensionDataPerformerUI.PauseAnimation(this);
        }

        private void numericUpDownPause_ValueChanged(object sender, EventArgs e)
        {
            data.Item5[0] = (int)numericUpDownPause.Value;
        }

        private void saveXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveExportXml();
        }

        private void checkBoxAbsoluteTime_CheckedChanged(object sender, EventArgs e)
        {
            changeAbsolute(checkBoxAbsoluteTime.Checked);
        }

        private void checkBoxSynchronous_CheckedChanged(object sender, EventArgs e)
        {
            bool b = checkBoxSynchronous.Checked;
            if (parentLab is Labels.GraphLabel)
            {
                Labels.GraphLabel gl = parentLab as Labels.GraphLabel;
                gl.AnimationType = b ? global::Animation.Interfaces.Enums.AnimationType.Synchronous :
                   global::Animation.Interfaces.Enums.AnimationType.Asynchronous;
            }

        }

        private void userControlTimeUnitAnimation_ChangeTimeUnit(TimeType obj)
        {
            timeType = obj;
            if (parentLab is Labels.GraphLabel)
            {
                Labels.GraphLabel gl = parentLab as Labels.GraphLabel;
                gl.TimeUnitAnimation = obj;
            }
        }

        private void buttonStartStopRealtime_Click(object sender, EventArgs e)
        {
            if (StaticExtensionEventPortable.IsRunning)
            {
                StopRealTime();
            }
            else
            {
                StartRealtimeClick();
            }
        }

        private void showRuntimeIndicatorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowRuntimeIndicators();
        }

        private void backgroundWorkerReatimeAnalysis_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                IChangeLogItem it = Item;
                if (it != null)
                {
                    it.Change += Change;
                }
                analysisPause = new System.Threading.AutoResetEvent(true);
                textMode = false;
                realtimeRun();
            }
            catch (Exception exception)
            {
                /* if (exception.IsFiction())
                 {
                     return;
                 }*/
                exception.ShowError();
            }
            if (changeBufferItem != null)
            {
                changeBufferItem.Change -= ChangeBufferItem;
                changeBufferItem = null;
            }
        }

        private void backgroundWorkerReatimeAnalysis_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            analysisPause = null;
            mode = "";
            IChangeLogItem it = Item;
            if (it != null)
            {
                it.Change -= Change;
            }
            realtimeStop();
            toolStripButtonPause.Enabled = false;
            toolStripButtonStart.Enabled = true;
            toolStripButtonStop.Enabled = false;
        }

        private void backgroundWorkerTextRealtimeAnalysis_DoWork(object sender, DoWorkEventArgs e)
        {
            IChangeLogItem it = Item;
            if (it != null)
            {
                it.Change += Change;
            }
            textMode = true;
            analysisPause = new System.Threading.AutoResetEvent(true);
            this.InvokeIfNeeded(() =>
            {
                toolStripButtonPause.Enabled = true;
                toolStripButtonStop.Enabled = true;
                toolStripButtonStart.Enabled = false;
            });
            realtimeRun();
        }

        private void backgroundWorkerTextRealtimeAnalysis_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StaticExtensionDataPerformerUI.finishText();
            using (System.IO.TextWriter w = new System.IO.StreamWriter(analysisXML))
            {
                w.Write(textXML + "");
            }
            analysisPause = null;
            toolStripButtonPause.Enabled = false;
            toolStripButtonStart.Enabled = true;
            toolStripButtonStop.Enabled = false;
        }

        #endregion

        private void panelMea_Resize(object sender, EventArgs e)
        {
            int w = panelMea.Width;
            foreach (Control c in panelMea.Controls)
            {
                c.Width = w - c.Left;
            }
        }
    }
}
