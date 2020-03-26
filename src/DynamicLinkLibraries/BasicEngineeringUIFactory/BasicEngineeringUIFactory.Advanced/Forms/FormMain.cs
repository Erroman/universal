using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml;
using System.Xml.Xsl;
using System.Resources;
using System.Collections;
using System.Reflection;
using System.Diagnostics;

using CategoryTheory;

using BaseTypes;

using Diagram.UI;
using Diagram.UI.Operations;
using Diagram.UI.Factory;
using Diagram.UI.Labels;
using Diagram.UI.Interfaces;
using Diagram.UI.Interfaces.Labels;
using Diagram.UI.Forms;

using TestCategory;

using ResourceService;

using ToolBox;

using DataWarehouse;
using DataWarehouse.Interfaces;
using DataWarehouse.Forms;
using DataWarehouse.Advanced.Forms;

using BasicEngineering.UI.Factory;
using BasicEngineering.UI.Factory.Interfaces;

using DataPerformer.Interfaces;
using DataPerformer.UI.Interfaces;
using DataPerformer.UI;
using DataPerformer.Formula;
using DataPerformer;

using Event.Interfaces;
using Event.Portable;
using Event.Basic;

using Event.Log.Database;

using Event.UI;



using Error.UI.Forms;

//using Scada.Interfaces;

using WindowsExtensions;

using WeifenLuo.WinFormsUI.Docking;



using ZipUtils;

namespace BasicEngineering.UI.Factory.Advanced.Forms
{
    public partial class FormMain : Form, IBlob, IErrorHandler, IProcess, IStartStop,
        IAnimationParameters, IAnimation, IRealTimeStartStop
    {

        #region Fields

        event Action start = () => { };

        event Action stop = () => { };


        public Action updateControls = () => { };

        private PanelDesktop active;

        ToolsDiagram tools;

        EngineeringUIFactory factory;

        DatabaseInterface data;
           
        string fn = "";

        FormDockableTree formTree;

        private DockPanel dockManager;

        private FormDesktop desktop;

        private IApplicationCreator creator;

        Panel docPanel;

        private bool isChecked = true;

        TestCategory.Interfaces.ITest test = null;

        private ToolStripButton[][] startStopPauseButtons;

        private bool paused = false;

        private object[] openSaveControls;

        private bool isRealTime = false;

        private List<IRealtimeUpdate> updatedControls = new List<IRealtimeUpdate>();

        Dictionary<DateTime, List<object>> logs = new Dictionary<DateTime, List<object>>();

        #region Forms

        private FormContainerDesigner formContainer;

        private FormDatabaseTree formData;

        FormTools formTools;

        FormMessages formWarnings;

        #endregion

        #endregion

        #region Ctor

        private FormMain()
        {
            InitializeComponent();
            StaticExtensionDataPerformerUI.Animation = this;
            startStopPauseButtons =
                new ToolStripButton[][] 
                { toolStripButtonStart.ToSingleArray<ToolStripButton>(), 
                    toolStripButtonStop.ToSingleArray<ToolStripButton>(), 
                    toolStripButtonPause.ToSingleArray<ToolStripButton>() 
                };

            openSaveControls = new object[]
            {
                saveToolStripButton,
                openToolStripButton,
                openToolStripMenuItem,
                saveToolStripMenuItem,
                saveAsToolStripMenuItem,
                savetodatabaseToolStripMenuItem
            };

            FormulaEditor.StaticExtensionFormulaEditor.OnCreateProxy += StaticExtensionFormulaEditor_OnCreateProxy;

            StaticExtensionDataPerformerUI.OnUpdateAnalysisUI += () =>
                {
                  //  updateControls();
                };

            StaticExtensionEventPortable.Start += (string calculationReason) => 
            {
                updatedControls.Clear();
                this.EnableRuntime(true, calculationReason, updatedControls);
                updateControls = null;
                foreach (IRealtimeUpdate up in updatedControls)
                {
                    Action update = up.Update;
                    if (update != null)
                    {
                        if (updateControls == null)
                        {
                            updateControls = update;
                        }
                        else
                        {
                            updateControls += update;
                        }
                    }
                }
                if (updateControls == null)
                {
                    updateControls = () => { };
                }
            };
            StaticExtensionEventPortable.Stop += () => { this.EnableRuntime(false); };
            StaticExtensionEventPortable.Stop += () => 
            {
                IEventLog log = StaticExtensionEventInterfaces.CurrentLog;
                if (log != null)
                {
                    if (log is ISaveLog)
                    {
                        string dir = AppDomain.CurrentDomain.BaseDirectory + "Logs";
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        (log as ISaveLog).SaveToFile(
                            dir + Path.DirectorySeparatorChar +
                            (DateTime.Now + "").Replace("/", "_").Replace(":", "_"));
                    }
                    else if (log is IListLog)
                    {
                        logs[DateTime.Now] = (log as IListLog).Log;
                        saveLogsToolStripMenuItem.Enabled = true;
                    }
                }
            };

            StaticExtensionDiagramUIForms.PostFormLoad =
                 (Form form) => { form.LoadControlResources(); };
            StaticExtensionDiagramUIForms.PostLabelLoad =
                (IObjectLabelUI label) =>
                { 
                    if (label is UserControlLabel)
                    {
                        Control control = label as Control;
                        foreach (Control c in control.Controls)
                        {
                            string n = c.Name;
                            if (n == "panelCenter")
                            {
                                c.LoadControlResources();
                                break;
                            }
                        }
                    }


                };
 
        }

        internal FormMain(IApplicationCreator creator)
            : this()
        {
            this.creator = creator;
            //!!! SCADA     
            Scada.Desktop.StaticExtensionScadaDesktop.ScadaFactory = Scada.Desktop.Serializable.StaticExtensionScadaDesktopSerializable.BaseFactory;
            StaticExtensionDiagramUIForms.Extension = creator.Ext;
            openFileDialogScn.Filter = creator.FileFilter;
            saveFileDialogScn.Filter = creator.FileFilter;
            if (creator.DatabaseCoordinator == null)
            {
                databaseToolStripMenuItem.Visible = false;
                savetodatabaseToolStripMenuItem.Visible = false;
                loadfromdatabaseToolStripMenuItem.Visible = false;
            }
            if (creator.Log != null)
            {
                toolStripButtonTest.Visible = true;
                Action close = () => { creator.Log.Flush(); creator.Log.Close(); };
                Application.ThreadExit += (object o, EventArgs e) => { close(); };

            }
            Text = creator.Text;
            creator.LoadResources();
            Dictionary<string, object>[] standardResources = Common.UI.Resources.Utils.ControlUtilites.Resources;
            if (Diagram.UI.Utils.ControlUtilites.Resources == null)
            {
                Diagram.UI.Utils.ControlUtilites.Resources =
                    standardResources;
            }
            StaticExtensionBasicEngineeringUIFactoryAdvanced.Prepare(creator.Resources);
            Icon = creator.Icon;

            Filename = creator.Filename;
            if (Filename == null)
            {
                Filename = "";
            }
            this.LoadControlResources();
            toolStripComboBoxCheckDetails.SelectedIndex = StaticExtensionBasicEngineering.CheckLevel;
            formTree = new FormDockableTree();
            formTree.OnHide += delegate()
            {
                StaticExtensionBasicEngineering.ShowTree = false;
                StaticExtensionBasicEngineering.Save();
            };
            SetSettings();
        }

        #endregion

        #region IErrorHandler Members

        void IErrorHandler.ShowError(Exception exception, object obj)
        {
            if (exception.IsFiction())
            {
                return;
            }
            if (obj != null)
            {
                Type t = obj.GetType();
                if (t.Equals(typeof(Int32)))
                {
                    int errorLevel = (int)obj;
                    if (errorLevel < 0)
                    {
                        return;
                    }
                    if (errorLevel == 0)
                    {
                        Diagram.UI.Utils.ControlUtilites.ShowError(this, exception, StaticExtensionBasicEngineeringUIFactoryAdvanced.Resources);
                        return;
                    }
                }
                try
                {
                    TypeLoadException tl = (TypeLoadException)exception;
                    ShowMessage(tl.TypeName, null);
                }
                catch
                {

                }
                if (exception is ReflectionTypeLoadException)
                {
                    ReflectionTypeLoadException tl = exception as ReflectionTypeLoadException;
                    string s = "";
                    foreach (Exception ss in tl.LoaderExceptions)
                    {
                        s += ss.Message + ";";
                    }
                    ShowMessage(s, null);
                }
                ShowMessage(exception.Message + exception.GetType(), null);
            }
            else
            {
                if (exception is TypeLoadException)
                {
                    TypeLoadException tl = exception as TypeLoadException;
                }
                ShowMessage(exception.Message, null);
            }
        }

        void IErrorHandler.ShowMessage(string message, object obj)
        {
            ShowMessage(message, null);
        }

        #endregion

        #region IProcess Members

        void IProcess.Start()
        {
            try
            {
                IAnimationParameters p = StaticExtensionDataPerformerUI.AnimationParameters;
                object[] o = p.Parameters as object[];
                IStartStop ss = this;
                ss.Action(null, Diagram.UI.Interfaces.ActionType.Start);
                toolStripButtonStop.Enabled = true;
                creator.Start((double)o[0], (double)o[1], (int)o[2], (int)o[3], (int)o[4], active);
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
                //ShowError(ex);
            }
        }

        /// <summary>
        /// Pauses process
        /// </summary>
        void IProcess.Pause()
        {
            factory.PauseWorker();
            IStartStop ss = this;
            ss.Action(null, Diagram.UI.Interfaces.ActionType.Pause);
            Diagram.UI.Interfaces.ActionType.Pause.EnableDisableButtons(startStopPauseButtons);
        }

        /// <summary>
        /// Resumes process
        /// </summary>
        void IProcess.Resume()
        {
            IStartStop ss = this;
            ss.Action(null, Diagram.UI.Interfaces.ActionType.Resume);
            Diagram.UI.Interfaces.ActionType.Resume.EnableDisableButtons(startStopPauseButtons);
            factory.Resume();
        }

        /// <summary>
        /// Terminates process
        /// </summary>
        void IProcess.Terminate()
        {
            IStartStop ss = this;
            ss.Action(null, Diagram.UI.Interfaces.ActionType.Stop);
            paused = false;
            factory.StopWorker();
            Diagram.UI.Interfaces.ActionType.Stop.EnableDisableButtons(startStopPauseButtons);
            toolStripStatusLabelCurentTimeInd.Text = "";
        }

        /// <summary>
        /// Shows status of process
        /// </summary>
        /// <param name="status">Status</param>
        void IProcess.ShowStatus(double status)
        {
        }

        /// <summary>
        /// Time of start
        /// </summary>
        double IProcess.StartTime
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        /// <summary>
        /// Step
        /// </summary>
        double IProcess.Step
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        /// <summary>
        /// Number of steps
        /// </summary>
        int IProcess.Count
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }


        #endregion

        #region IBlob Members

        byte[] IBlob.Bytes
        {
            get
            {
                MemoryStream stream = new MemoryStream();
                SaveAll(stream);
                /*TestCategory.Interfaces.ITestInterface ti = creator.TestInterface;
                if (ti != null)
                {
                    test = ti.Edit(test, active);
                }
                if (test != null)
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(stream, test);
                }*/
                return stream.GetBuffer();

            }
            set
            {
                MemoryStream stream = new MemoryStream(value);
                LoadFromStream(stream, ".cfa");
                /*     TestCategory.Interfaces.ITestInterface ti = creator.TestInterface;
                     if (ti != null)
                     {
                         stream.CreateTestReport(ti);
                     }*/
            }
        }
        string IBlob.Extension
        {
            get { return "cfa"; }
            set { }
        }

        #endregion

        #region IStartStop Members

        void IStartStop.Action(object type, Diagram.UI.Interfaces.ActionType actionType)
        {
            if (StaticExtensionDataPerformerUI.IsRunning
                & actionType == Diagram.UI.Interfaces.ActionType.Stop)
            {
                return;
            }
            actionType.EnableDisableButtons(startStopPauseButtons);
            ActionPrivate(type, actionType);
            if (actionType == Diagram.UI.Interfaces.ActionType.Start)
            {
                if (type != null)
                {
                    Action act = () =>
                        {
                            toolStripButtonPause.Enabled = false;
                            toolStripButtonStart.Enabled = false;
                            toolStripButtonStop.Enabled = false;
                        };
                    this.InvokeIfNeeded(act);
                }
            }
        }

        #endregion

        #region IAnimationParameters Members

        object IAnimationParameters.Parameters
        {
            get
            {
                double[] d = ControlDouble;
                int[] i = ControlInt;
                return new object[] { d[0], d[1], i[0], i[1], i[2] };
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region IAnimation Members

        void IAnimation.Start(Action<Action> animation)
        {
            StaticExtensionDataPerformerUI.AnimationAction = animation;
            IProcess p = this;
            if (paused)
            {
                p.Resume();
                return;
            }
            StartDisable();
            p.Start();
        }

        void IAnimation.Stop()
        {
            StopEnable();
            IProcess p = this;
            p.Terminate();
            paused = false;
        }

        void IAnimation.Pause()
        {
            paused = true;
            IProcess p = this;
            p.Pause();
        }


        #endregion

        #region IRealTimeStartStop Members


        void IRealTimeStartStop.Start()
        {
            toolStripStatusLabel.Text = "Realtime";
        }

        void IRealTimeStartStop.Stop()
        {
            toolStripStatusLabel.Text = "";
        }


        event Action IRealTimeStartStop.OnStart
        {
            add { start += value; }
            remove { start -= value; }
        }

        event Action IRealTimeStartStop.OnStop
        {
            add { stop += value; }
            remove { stop -= value; }
        }

        #endregion

        #region LoadSave


        void SaveLogs()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory + "Logs";
            BinaryFormatter f = new BinaryFormatter();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            foreach (DateTime dt in logs.Keys)
            {
                string fn = dir + Path.DirectorySeparatorChar + (dt + "").Replace("/", "_").Replace(":", "_") + ".serializable";
                using (Stream stream = File.OpenWrite(fn))
                {
                    List<object> l = logs[dt];
                    l.TransformLogSave();
                 //   foreach (object o in l)
                 //   {
                        f.Serialize(stream, l);
                    //   }
                    StaticExtensionEventInterfaces.LogPostSave(fn);
                }
            }
            logs.Clear();
            saveLogsToolStripMenuItem.Enabled = false;
        }


        public bool SaveAll(Stream stream)
        {
            try
            {
                Check();
                active.SaveAll(stream);
                if (creator.TestInterface != null)
                {
                    test = creator.TestInterface.Edit(this.test, active);
                }
                if (test != null)
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(stream, this.test);
                }
                isChecked = false;
                return true;
            }
            catch (Exception ex)
            {
                isChecked = false;
                ex.ShowError(1);
            }
            return false;
        }

        public bool LoadFromStream(Stream stream, string ext)
        {
            bool b = active.LoadFromStream(stream, SerializationInterface.StaticExtensionSerializationInterface.Binder, ext, ".cfa");
            if (!b)
            {
                return false;
            }
            if (creator.TestInterface != null)
            {
                stream.Position = 0;
                stream.CreateTestReport(creator.TestInterface);
            }
            return true;
        }

        private void Check()
        {
            if (!isChecked)
            {
                isChecked = true;
                active.Check();
            }
        }

        private void SaveAsScadaXml()
        {
            try
            {
                IList<IObjectLabel> objects;
                IList<IArrowLabel> labels;
                IObjectLabel l = null;
                active.GetSelected(out objects, out labels);
                bool b = (objects.Count == 1) & (labels.Count == 0);
                if (b)
                {
                    l = objects[0];
                    ICategoryObject ob = l.Object;
                    b = ob is IDataConsumer;
                }
                if (!b)
                {
                    MessageBox.Show(this, "Select coordinator");
                    return;
                }
                if (saveFileDialogScadaXml.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                Scada.Interfaces.IScadaInterface scada = Scada.Desktop.StaticExtensionScadaDesktop.ScadaFromDesktop(active,
                    l.Name, BaseTypes.Attributes.TimeType.Second, false, null);
                scada.XmlDocument.Save(saveFileDialogScadaXml.FileName);
            }
            catch (Exception ex)
            {
                ex.ShowError();
            }
        }

        #endregion

        #region Init

        protected void Prepare()
        {
            factory = creator.Factory as EngineeringUIFactory;
            factory.TimeIndication += (double time) =>
                {
                    this.InvokeIfNeeded(() => { toolStripStatusLabelCurentTimeInd.Text = time + ""; });
                };


            factory.CancelProcess += () => { this.InvokeIfNeeded(StopAll); };
            creator.ApplicationInitializer.InitializeApplication();
            if (creator.Log != null)
            {
                factory.ErrorHandler = new Diagram.UI.ErrorHandlers.TextWriterErrorHandler(creator.Log);
            }
            else
            {
                factory.ErrorHandler = this;
            }
            Resources.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Environment.CurrentDirectory = Resources.CurrentDirectory + "";
            CommonOperations.CreateArrowControl(panelToolLeft, tools);
            ButtonWrapper.Add(creator.Buttons, tabControlControls, tools, new Size(25, 25), StaticExtensionBasicEngineeringUIFactoryAdvanced.Resources, true);
            ContainerPerformerUI.InitContainers(AppDomain.CurrentDomain.BaseDirectory, tools, tabControlControls,
                StaticExtensionBasicEngineeringUIFactoryAdvanced.Resources);

        }

        void StopAll()
        {
            StopEnable();
            IStartStop ss = this;
            ss.Action(null, Diagram.UI.Interfaces.ActionType.Stop);
        }

        private void initAll()
        {
            DataPerformer.Portable.StaticExtensionDataPerformerPortable.Desktop = active;
            StaticExtensionDiagramUI.PostLoadDesktop += PostLoad;
            initTools();
            initContainers();
        }


        void PostLoad(IDesktop desktop)
        {
           
            PanelDesktop pd = desktop as PanelDesktop;
            IEnumerable<object> coll = active.AllComponents;
            foreach (object o in coll)
            {
                if (o is IStartStopConsumer)
                {
                    IStartStopConsumer ssc = o as IStartStopConsumer;
                    ssc.StartStop = this;
                }
            }
        }



        private void initTools()
        {
            EditorReceiver.AddEditorDrag(active);
            PictureReceiver.AddImageDrag(active);
        }

        private void initContainers()
        {
            Assembly ass = typeof(FormMain).Assembly;
            string path = Path.GetDirectoryName(ass.Location);
        }

        private void createDockMan()
        {
            if (dockManager == null)
            {
                dockManager = new DockPanel();
                docPanel = this.panel1;
                //                dockManager.ActiveDocumentChanged += dockManager_ActiveDocumentChanged;
                this.dockManager.ActiveAutoHideContent = null;
                this.dockManager.Dock = System.Windows.Forms.DockStyle.Fill;
                this.dockManager.Location = new System.Drawing.Point(0, 28);
                this.dockManager.Name = "dockManager";
                this.dockManager.Size = new System.Drawing.Size(853, 600);
                this.dockManager.TabIndex = 1;
                docPanel.Controls.Add(dockManager);
                dockManager.DocumentStyle = DocumentStyle.DockingWindow;
                if (StaticExtensionBasicEngineering.LeftPortion > 0)
                {
                    dockManager.DockLeftPortion = StaticExtensionBasicEngineering.LeftPortion;
                    dockManager.DockRightPortion = StaticExtensionBasicEngineering.RightPortion;
                }

            }
        }

        #endregion

        #region Work with files


        private string Filename
        {
            get
            {
                return fn;
            }
            set
            {
                fn = value;
                string t = creator.Text.Localize();
                if (fn == null)
                {
                    Text = t;
                }
                else
                {
                    if (fn.Length > 0)
                    {
                        Text = t + " [" + Filename + "]";
                    }
                }
            }
        }

        private void SaveAs()
        {
            try
            {
                Action a = () =>
                    {
                        Check();
                        saveFileDialogScn.InitialDirectory = ResourceService.Resources.CurrentDirectory + "scn";
                        if (saveFileDialogScn.ShowDialog() != DialogResult.OK)
                        {
                            return;
                        }
                        Save(saveFileDialogScn.FileName);
                        Filename = saveFileDialogScn.FileName;
                    };
                OpenSave(a);
            }
            catch (Exception ex)
            {
                ex.ShowError(1);
            }
        }

        private void Save(string filename)
        {
            try
            {
                Check();
                using (Stream stream = File.OpenWrite(filename))
                {
                    SaveAll(stream);
                }
           }
            catch (Exception ex)
            {
                ex.ShowError(1);
            }
        }


        private void Save()
        {

            if (Filename == null)
            {
                SaveAs();
                return;
            }
            if (Filename.Length == 0)
            {
                SaveAs();
                return;
            }
            Action a = () => { Save(Filename); };
            OpenSave(a);
        }



        void LoadContainer(Stream stream)
        {
        }

        void OpenSave(Action action)
        {
            openSaveControls.EnabledLock(action);
        }

        void Open()
        {
            Action a = () =>
                {
                    if (openFileDialogScn.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                    log(openFileDialogScn.FileName);
                    string ext = Path.GetExtension(openFileDialogScn.FileName);
                    using (Stream stream = File.OpenRead(openFileDialogScn.FileName))
                    {
                        LoadFromStream(stream, ext);
                    }
                    if (ext.ToLower().Equals(creator.Ext))
                    {
                        Filename = openFileDialogScn.FileName + "";
                    }
                    else
                    {
                        Filename = "";
                    }
                };
            OpenSave(a);
        }


        void log(string filename)
        {
            return;
        }


        #endregion

        #region Relicts

        #region Tests

        void testPerf()
        {
            /*   int n = 100;
               PureDesktopPeer d = new PureDesktopPeer();
               Stream stream = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory +
                   "0.cfa");
               bool b = d.Load(stream, null);
               stream.Close();
               BitmapConsumer.IBitmapConsumer cons =
                   PureDesktop.GetAssociatedObject<BitmapConsumer.IBitmapConsumer>(d, "Picture map");
               StreamWriter wr = new StreamWriter("0.txt", false);
               DateTime t0 = DateTime.Now;
               wr.WriteLine(t0.ToLongTimeString());
               for (int i = 0; i < n; i++)
               {
                   cons.Process();
               }
               DateTime t1 = DateTime.Now;
               wr.WriteLine(t1.ToLongTimeString());
               TimeSpan dt = t1 - t0;
               double s = (60 * (double)dt.Minutes + (double)dt.Seconds + (double)dt.Milliseconds / 1000) / n;
               wr.WriteLine(s);
               wr.Close();*/
        }



        #endregion

        #region Sandcastle

        void setEnvProps(XmlDocument doc)
        {
            XmlNodeList l = doc.GetElementsByTagName("property");
            foreach (XmlElement e in l)
            {
                string name = e.Attributes["name"].Value;
                string value = e.Attributes["value"].Value;
                Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
            }
        }

        string systemReplace(string str)
        {
            string s = str;
            while (true)
            {
                if (!s.Contains("$"))
                {
                    break;
                }
                int n = s.IndexOf('$');
                IDictionary d = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);
                foreach (object o in d.Keys)
                {
                    string ss = "${" + o + "}";
                    if (s.Contains(ss))
                    {
                        s = s.Replace(ss, d[o] + "");
                    }
                }
            }
            return s; ;
        }

        void copy(XmlDocument doc)
        {
            XmlNodeList l = doc.GetElementsByTagName("copy");
            foreach (XmlElement e in l)
            {
                copy(e);
            }
        }

        void copy(XmlElement e)
        {
            string inFile = systemReplace(e.Attributes["file"].Value);
            string outFile = systemReplace(e.Attributes["tofile"].Value);
            StreamReader r = new StreamReader(inFile);
            StreamWriter w = new StreamWriter(outFile);
            XmlNodeList l = e.GetElementsByTagName("replacestring");
            Dictionary<string, string> d = new Dictionary<string, string>();
            foreach (XmlElement el in l)
            {
                d[el.Attributes["from"].Value] = el.Attributes["to"].Value;
            }
            while (true)
            {
                string s = r.ReadLine();
                if (s == null)
                {
                    break;
                }
                foreach (string p in d.Keys)
                {
                    s = s.Replace(p, systemReplace(d[p]));
                }
                w.Write(s);
            }
            r.Close();
            w.Close();
        }


        private void exec(XmlDocument doc)
        {
            XmlNodeList l = doc.GetElementsByTagName("exec");
            exec(l[0] as XmlElement);
        }

        private void xmlToXsl(XmlDocument doc, string workDir)
        {
            string sxsl = "/xsl:";
            string sout = "/out:";

            XmlNodeList l = doc.GetElementsByTagName("arg");
            string[] s = new string[3];
            for (int i = 0; i < 3; i++)
            {
                int k = 0;

                string pref = null;
                XmlElement e = l[i] as XmlElement;
                string str = e.GetAttribute("value");
                if (str.Contains(sxsl))
                {
                    k = 1;
                    pref = sxsl;
                }
                if (str.Contains(sout))
                {
                    pref = sout;
                    k = 2;
                }
                if (pref != null)
                {
                    str = str.Substring(pref.Length);
                }
                str = systemReplace(str);
                if (str[0] == '\"' & str[str.Length - 1] == '\"')
                {
                    str = str.Substring(1, str.Length - 2);
                }
                if (File.Exists(workDir + str))
                {
                    str = workDir + str;
                }
                s[k] = str;
            }

            /*XslTransform xsl = new XslTransform();
            xsl.Load(s[1]);
            xsl.Transform(s[0], s[2]);*/

        }

        private void exec(XmlElement e)
        {
            string program = systemReplace(e.GetAttribute("program"));
            string workingDir = systemReplace(e.GetAttribute("workingdir"));
            string args = "";
            if (e.Attributes["commandline"] != null)
            {
                args = " " + e.GetAttribute("commandline");
            }
            XmlNodeList l = e.GetElementsByTagName("arg");
            foreach (XmlElement el in l)
            {
                string arg = systemReplace(el.GetAttribute("value"));
                args += " " + arg;
            }
            ProcessStartInfo info = new ProcessStartInfo(program, args);
            info.WorkingDirectory = workingDir;
            info.RedirectStandardOutput = true;
            info.RedirectStandardInput = true;
            info.RedirectStandardError = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            Process p = new Process();
            p.StartInfo = info;
            p.Start();
            p.WaitForExit();
            StreamWriter wr = new StreamWriter("1.txt");
            while (true)
            {
                string s = p.StandardOutput.ReadLine();
                if (s == null)
                {
                    break;
                }
                wr.WriteLine(s);
            }
            wr.Close();
        }

        #endregion


        #endregion

        #region Event Handlers

        #region Open & Close Window

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

            if (creator.Holder != null)
            {
                // e
            }
            ShowDesktop();
            if (StaticExtensionBasicEngineering.ShowTools)
            {
                ShowTools();
            }
            if (StaticExtensionBasicEngineering.ShowTree)
            {
                ShowTree();
            }
            if (StaticExtensionBasicEngineering.ShowWarnings)
            {
                ShowWarnings();
            }
            if (StaticExtensionBasicEngineering.ShowDatabase)
            {
                ShowData();
            }
            initAll();
            try
            {
                Prepare();
                ByteHolder holder = creator.Holder;
                if (holder != null)
                {
                    IDesktop edit = holder.Desktop;
                    active.Load(edit);
                }
                else
                {
                    byte[] b = StaticExtensionBasicEngineering.SavedState.StringToBytes();
                    MemoryStream ms = null;
                    if (b.Length > 1)
                    {
                        ms = new MemoryStream(b);
                    }
                    Stream stream = ms;
                    if (Filename != null)
                    {
                        if (Filename.Length != 0)
                        {
                            log(Filename);
                            stream = File.OpenRead(Filename);
                        }
                    }
                    if (stream != null)
                    {
                        LoadFromStream(stream, creator.Ext);
                        stream.Close();
                    }
                   /// !!! LOG StaticExtensionEventInterfaces.LogFactory = new MemoryLog();
                }
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (logs.Count > 0)
            {
                string cap = GetControlResource("Warning");
                string text = GetControlResource("Logs are not saved. Save them?");
                DialogResult dr = MessageBox.Show(this, text, cap, MessageBoxButtons.YesNoCancel, 
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (dr == DialogResult.Yes)
                {
                    SaveLogs();
                }
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            if (!SaveSettings())
            {
                e.Cancel = true;
            }
            if (!e.Cancel)
            {
              /*  string dir = AppDomain.CurrentDomain.BaseDirectory + "Logs" + Path.DirectorySeparatorChar;
                if (Directory.Exists(dir))
                {
                    dir.ZipDirectoryFiles("*.serializable");
                }
                */
            }
        }

        #endregion

        #region Click Control Events

        private void saveLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveLogs();
        }


        private void openLogDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
          
        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            Dictionary<object, IComponentCollection> d = new Dictionary<object, IComponentCollection>();
            active.Decompose(d);
        }

        private void clearallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            active.RemoveAll();
            active.Redraw();
            active.Refresh();
        }

        private void clearselectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            active.RemoveSelected();
            active.Redraw();
            active.Refresh();

        }

        private void editorOfAliasesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditAliases();
        }

        private void deletecommentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            active.DeleteComments();
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                Check();
                active.RefreshObjects();
              //  update();
                GC.Collect();
            }
            catch (Exception ex)
            {
                ex.ShowError(1);
            }
        }

        private void toolStripButtonFont_Click(object sender, EventArgs e)
        {
            try
            {
                TextBox box = ControlPanel.GetActiveTextBox(active);
                if (box == null)
                {
                    return;
                }
                FontDialog dlg = new FontDialog();
                dlg.ShowDialog(this);
                Font font = dlg.Font;
                box.Font = font;
            }
            catch (Exception ex)
            {
                ex.ShowError(1);
            }
        }

        private void toolStripButtonClear_Click(object sender, EventArgs e)
        {
            active.TempDelete();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            active.SelectAll(true);
        }

        private void unselectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            active.SelectAll(false);
        }

        private void toolStripButtonCut_Click(object sender, EventArgs e)
        {
            cutToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButtonCopy_Click(object sender, EventArgs e)
        {
            copyToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButtonPaste_Click(object sender, EventArgs e)
        {
            pasteToolStripMenuItem_Click(sender, e);
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StaticExtensionBasicEngineering.Server = "";
            data = null;
            prepareData();
        }

        private void toolStripButtonStart_Click(object sender, EventArgs e)
        {
            (this as IAnimation).Start(null);
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            if (isRealTime)
            {
                return;
            }
          (this as IAnimation).Stop();
        }

        private void toolStripButtonPause_Click(object sender, EventArgs e)
        {
            (this as IAnimation).Pause();
        }

        private void readWriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!prepareData())
            {
                return;
            }
            FormDatabase form = new FormDatabase(
                new StandardStarter(ResourceService.Resources.CurrentDirectory + "Temp\\"),
                data, true);
            form.ShowDialog(this);

        }
 
        private void loadfromdatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadFromDatabase();
        }

        private void toolStripButtonLoadFromDatabase_Click(object sender, EventArgs e)
        {
            loadFromDatabase();
        }

        private void changeorderofselectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<IObjectLabel> objs;
            IList<IArrowLabel> arrs;
            active.GetSelected(out objs, out arrs);
            if ((arrs.Count != 0) | (objs.Count != 1))
            {
                return;
            }
            Form form = new FormOrder(active);
            form.ShowDialog(this);
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
          //  update();
        }

        private void savetodatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveToDatabase();
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
           Open();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream stream = new MemoryStream();
            active.SaveSelected(stream);
            Clipboard.SetData("AstroFrame", stream);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            object o = Clipboard.GetData("AstroFrame");
            if (o == null)
            {
                return;
            }
            Stream stream = o as Stream;
            LoadFromStream(stream, creator.Ext);
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clearselectedToolStripMenuItem_Click(sender, e);
        }

        private void saveasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void saveControl(object sender, EventArgs e)
        {
            Save();
        }

        private void toolStripButtonStrict_Click(object sender, EventArgs e)
        {
            if (toolStripButtonStrict.Checked)
            {
                Diagram.UI.StaticExtensionDiagramUI.SetStrictErrorHandler();
            }
            else
            {
                Diagram.UI.StaticExtensionDiagramUI.ErrorHandler = null;
            }
        }

        private void browseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            StaticExtensionBasicEngineering.SoundDirectory = folderBrowserDialog.SelectedPath;
            toolStripMenuItemSoundDirectory.Text = StaticExtensionBasicEngineering.SoundDirectory;
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void derivationCalculatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new DataPerformer.UI.Forms.FormDerivationCalculator("xyzt");
            f.Show();
        }

        private void containerDesignerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showContainerDesigner();
        }

        private void FormulaImport()
        {
            FormulaEditor.UI.Forms.FormulaEditorForm ff = new FormulaEditor.UI.Forms.FormulaEditorForm("", null, new string[] { "a" });
            ff.Test = true;
            ff.Show();
        }

        private void toolStripButtonTest_Click(object sender, EventArgs e)
        {
            TestCategory.UI.TestPerformerUI perf = new TestCategory.UI.TestPerformerUI();
            perf.TestData(data, null, active, creator.Ext, creator.Ext, creator.Log);
            PureDesktopPeer d = new PureDesktopPeer();
            active.Copy(d);
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, d);
            MemoryStream str = new MemoryStream(ms.GetBuffer());
            d = bf.Deserialize(str) as PureDesktopPeer;
            clearallToolStripMenuItem_Click(null, null);
            active.Load(d);

        }

        private void toolStripButtonSync_Click(object sender, EventArgs e)
        {
          //  update();
        }

        private void dataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowData();
        }

        private void objectTreelBarToolStripMenuItemObjects_Click(object sender, EventArgs e)
        {
            ShowTree();
        }

        private void toolboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowTools();
        }

        private void toolStripComboBoxCheckDetails_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetCheckLevel();
        }

        private void saveSCADAXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAsScadaXml();
        }


        private void generateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            active.SaveCSCodeToFile(@"D:\AUsers\1MySoft\CSharp\src\Generated\GeneratedProject\StaticExtensionGeneratedProject.cs", "GeneratedProject", "StaticExtensionGeneratedProject");
        }

        private void checkDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckDesktop();
        }

        private static void StaticExtensionFormulaEditor_OnCreateProxy(FormulaEditor.Interfaces.ITreeCollection collection, object code)
        {
            if (collection is IAssociatedObject)
            {
                IAssociatedObject ao = collection as IAssociatedObject;
                string n = ao.GetName(StaticExtensionDiagramUI.CurrentDeskop);
                n = n.Replace('/', '_').Replace('.', '_');
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                if (dir[dir.Length - 1] != Path.DirectorySeparatorChar)
                {
                    dir += Path.DirectorySeparatorChar;
                }
                string d = dir + "Code" + Path.DirectorySeparatorChar;
                if (!Directory.Exists(d))
                {
                    Directory.CreateDirectory(d);
                }
                using (TextWriter writer = new StreamWriter(d + n + ".cs"))
                {
                    writer.Write(code + "");
                }
            }
        }


        #endregion

        #endregion

        #region Work with database

        private bool prepareData()
        {
            if (data != null)
            {
                return true;
            }
            string ser = "";
            try
            {
                ser = server;
                if (ser.Length == 0)
                {
                    connect();
                    if (server.Length == 0)
                    {
                        return false;
                    }
                    ser = server;
                }
                if (formData != null)
                {
                    formData.Hide();
                    formData = null;
                }
                IDatabaseInterface inter = //ReportingServer.ReportingInterface.GetService("");
                       DataWarehouse.StaticExtensionDataWarehouse.Coordinator[ser];

                IUser user = new User("", "", null);
                data = new DatabaseInterface(user, inter);
                ShowData();
                return true;
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
                if (StaticExtensionBasicEngineering.ConnectionStrings.Contains(ser))
                {
                    StaticExtensionBasicEngineering.ConnectionStrings.Remove(ser);
                }
                StaticExtensionBasicEngineering.Server = "";
                StaticExtensionBasicEngineering.Save();
                // loadfromdatabaseToolStripMenuItem.Visible = false;
                // savetodatabaseToolStripMenuItem.Visible = false;
                /*toolStripButtonLoadFromDatabase.Visible = false;
                databaseToolStripMenuItem.Visible = false;*/
            }
            return false;
        }


        private void connect()
        {
            /*
            FormConnectToDatabase form = new FormConnectToDatabase();
            form.ShowDialog(this);*/
            System.Collections.Specialized.StringCollection str = StaticExtensionBasicEngineering.ConnectionStrings;

            List<string> l = new List<string>();
            foreach (string s in str)
            {
                l.Add(s);
            }
            l.Sort();
            string serv = server + "";
            FormConnectionString form = new FormConnectionString(l, server);
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                server = "";
                return;
            }
            serv = form.ConnectionString;
            if (!l.Contains(serv))
            {
                StaticExtensionBasicEngineering.ConnectionStrings.Add(serv);
                StaticExtensionBasicEngineering.Save();
            }
            server = serv;
        }

        private string server
        {
            get
            {

                // StaticExtension.Server = "3d-monstr";
                // StaticExtension.Save();
                string s = StaticExtensionBasicEngineering.Server;
                if (s.Length == 0)
                {
                    // dataToolStripMenuItem.Visible = false;
                }
                return StaticExtensionBasicEngineering.Server;
            }
            set
            {
                StaticExtensionBasicEngineering.Server = value;
                /*if (value.Length == 0)
                {
                    dataToolStripMenuItem.Visible = false;
                }
                else
                {
                    dataToolStripMenuItem.Visible = false;
                }*/
                StaticExtensionBasicEngineering.Save();
            }
        }



        void loadFromDatabase()
        {
            if (!prepareData())
            {
                return;
            }
            try
            {
                FormDatabase form = new FormDatabase(this, data, true);
                form.ShowDialog(this);
            }
            catch (Exception e)
            {
                e.ShowError(10);
                WindowsExtensions.ControlExtensions.ShowMessageBoxModal(e.Message);
            }
        }



        void SaveToDatabase()
        {
            if (!prepareData())
            {
                return;
            }
            FormDatabase form = new FormDatabase(this, data, false);

            form.ShowDialog(this);
        }

        #endregion

        #region Shows Child Windows

        private void showContainerDesigner()
        {
            FormContainerDesigner.Show(active, ref formContainer);
        }

        private void messagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowWarnings();
        }

        private void AddSreamCreator(string name, PanelDesktop desktop)
        {
            Action<Stream> act = null;
            if (creator.TestInterface != null)
            {
                act = (Stream stream) => { stream.CreateTestReport(creator.TestInterface); };
            }
            desktop.AddStreamCreator(name, act);

        }

        private void ShowDesktop()
        {
            createDockMan();

            if (desktop == null)
            {
                tools = new ToolsDiagram(creator.Factory);
                desktop = new FormDesktop(tools, creator.Ext);
                AddSreamCreator("DataWarehouse.DatabaseStreamCreator", desktop.Desktop);
                tools.Tree = formTree.Tree;
            }
            desktop.Show(dockManager);
            active = desktop.Desktop;
            active.AllowDrop = true;
        }

        private void ShowTree()
        {
            createDockMan();
            formTree.Show(dockManager);
            StaticExtensionBasicEngineering.ShowTree = true;
            StaticExtensionBasicEngineering.Save();
        }

        private void ShowWarnings()
        {
            if (formWarnings == null)
            {
                formWarnings = new FormMessages();
                formWarnings.LoadControlResources();
                DockContent c = formWarnings;
                formWarnings.OnHide += delegate()
                {
                    StaticExtensionBasicEngineering.ShowWarnings = false;
                    StaticExtensionBasicEngineering.Save();
                };
            }
            StaticExtensionBasicEngineering.ShowWarnings = true;
            StaticExtensionBasicEngineering.Save();
            formWarnings.Show(dockManager);
        }

        private void ShowControl()
        {
            toolStripTextBoxStart.Text = StaticExtensionBasicEngineering.StartTime + "";
            toolStripTextBoxStep.Text = StaticExtensionBasicEngineering.Step + "";
            toolStripTextBoxPause.Text = StaticExtensionBasicEngineering.Pause + "";
            toolStripTextBoxStepCount.Text = StaticExtensionBasicEngineering.StepCount + "";
            toolStripTextBoxTimeIndicator.Text = StaticExtensionBasicEngineering.TimeIndicator + "";
            toolStripMenuItemSoundDirectory.Text = StaticExtensionBasicEngineering.SoundDirectory;
        }



        void ShowTools()
        {
            if (formTools == null)
            {
                formTools = new FormTools();
                DockContent c = formTools;
                formTools.OnHide += delegate()
                {
                    StaticExtensionBasicEngineering.ShowTools = false;
                    StaticExtensionBasicEngineering.Save();
                };
            }
            StaticExtensionBasicEngineering.ShowTools = true;
            StaticExtensionBasicEngineering.Save();
            formTools.Show(dockManager);
        }

        private void OpenNode(TreeNode node)
        {
            object o = node.Tag;
            if (o == null)
            {
                return;
            }
            if (!(o is ILeaf))
            {
                return;
            }
            Common.UI.IStreamCreator sc = new DatabaseStreamCreator(o as ILeaf);
            Stream stream = sc.Stream;
            active.LoadFromStream(stream, 
                SerializationInterface.StaticExtensionSerializationInterface.Binder, 
                active.Extension, active.Extension);
        }



        void ShowData()
        {
            if (creator.DatabaseCoordinator == null)
            {
                return;
            }
            createDockMan();
            if (formData == null)
            {
                if (data == null)
                {
                    try
                    {
                        string ser = server;
                        if (ser.Length == 0)
                        {
                            return;
                        }
                        IDatabaseInterface inter = //ReportingServer.ReportingInterface.GetService("");
                      DataWarehouse.StaticExtensionDataWarehouse.Coordinator[ser];

                        IUser user = new User("", "", null);
                        data = new DatabaseInterface(user, inter);
                        EngineeringUIFactory.Data = data;
                    }
                    catch (Exception ex)
                    {
                        ex.ShowError(10);
                        server = "";
                        return;
                    }
                }
                try
                {
                    formData = new FormDatabaseTree(data, creator.Ext, creator.Icon.ToBitmap());
                    formData.OnHide += delegate()
                    {
                        StaticExtensionBasicEngineering.ShowDatabase = false;
                        StaticExtensionBasicEngineering.Save();
                    };
                    formData.OpenNode += OpenNode;
                }
                catch (Exception exc)
                {
                    exc.ShowError(10);
                    WindowsExtensions.ControlExtensions.ShowMessageBoxModal(exc.Message);
                    StaticExtensionBasicEngineering.ShowDatabase = false;
                    StaticExtensionBasicEngineering.Save();
                    return;
                }
            }
            StaticExtensionBasicEngineering.ShowDatabase = true;
            StaticExtensionBasicEngineering.Save();
            formData.Show(dockManager);
        }

        #endregion

        #region Members

        #region Public Members

        public IApplicationCreator Creator
        {
            get
            {
                return creator;
            }
        }


        #endregion

        #region Nonpublic Members


        private void CheckDesktop()
        {
            IAssociatedObject ao = active.InvalidCompilation();
            if (ao != null)
            {
                ShowObject(ao);
                return;
            }
        }


        private static void ShowObject(IAssociatedObject ao)
        {
            object o = ao.Object;
            if (o is IObjectLabelUI)
            {
                IObjectLabelUI olui = o as IObjectLabelUI;
                olui.Selected = true;
            }
            if (o is IArrowLabelUI)
            {
                IArrowLabelUI alui = o as IArrowLabelUI;
                alui.Selected = true;
            }
            if (o is IShowForm)
            {
                IShowForm sf = o as IShowForm;
                sf.ShowForm();
            }
        }


        private void ShowMessage(string message, string level)
        {
            Error.UI.StaticExtensionErrorUI.PlayError();
            if (formWarnings == null)
            {
                return;
            }
            if (formWarnings.IsHidden)
            {
                return;
            }
            formWarnings.Add(new string[] { level, message.Localize() });
        }

        private void ActionPrivate(object type, Diagram.UI.Interfaces.ActionType actionType)
        {
            if (actionType == Diagram.UI.Interfaces.ActionType.Start)
            {
                StartDisable();
                if (type != null)
                {
                    if (type.Equals(Animation.Interfaces.Enums.ActionType.Calculation))
                    {
                        toolStripStatusLabel.Text = "Calculation";
                    }
                }
            }
            else
            {
                StopEnable();
            }
            StaticExtensionDiagramUIFactory.Action(active, type, actionType);
            actionType.EnableDisableButtons(startStopPauseButtons);
        }

        void StartDisable()
        {
            ToolStripItemCollection coll = toolStripMain.Items;
            foreach (ToolStripItem it in coll)
            {
                it.Enabled = false;
            }
            active.AllowDrop = false;
            menuStripMain.Enabled = false;
            tabControlControls.Enabled = false;
            toolStripStatusLabel.Text = "Animation";
        }

        void StopEnable()
        {
            ToolStripItemCollection coll = toolStripMain.Items;
            foreach (ToolStripItem it in coll)
            {
                it.Enabled = true;
            }
            active.AllowDrop = true;
            menuStripMain.Enabled = true;
            tabControlControls.Enabled = true;
            toolStripStatusLabel.Text = "";
        }

        static protected string GetControlResource(string str)
        {
            return Resources.GetControlResource(str, Diagram.UI.Utils.ControlUtilites.Resources);
        }


    
        void SetSettings()
        {
            StaticExtensionEventLogDatabase.ConnectionString =
                StaticExtensionBasicEngineering.LogConnectionString;
            FormClosed += (object sender, FormClosedEventArgs e) =>
            {
               if  (StaticExtensionEventLogDatabase.Data is IDisposable)
                {
                    (StaticExtensionEventLogDatabase.Data as IDisposable).Dispose();
                }
            };
            WindowState = StaticExtensionBasicEngineering.FullScreen ? FormWindowState.Maximized : FormWindowState.Normal;
            Left = StaticExtensionBasicEngineering.Left = Left;
            Top = StaticExtensionBasicEngineering.Top = Top;
            if (StaticExtensionBasicEngineering.Width > 0)
            {
                Width = StaticExtensionBasicEngineering.Width;
                Height = StaticExtensionBasicEngineering.Height;
            }
            toolStripComboBoxCheckDetails.SelectedIndex = StaticExtensionBasicEngineering.CheckLevel;
        /// !!! SETS CHECK LEVEL    SetCheckLevel();
            toolStripComboBoxCheckDetails.SelectedIndexChanged += toolStripComboBoxCheckDetails_SelectedIndexChanged;
            ShowControl();
        }

        private double StartTime
        {
            get
            {
                return GetDouble(toolStripTextBoxStart, StaticExtensionBasicEngineering.StartTime);
            }
        }

        private double GetDouble(ToolStripTextBox tb, double def)
        {
            try
            {
                return Double.Parse(tb.Text);
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
            }
            return def;
        }

        private int GetInt(ToolStripTextBox tb, int def)
        {
            try
            {
                return Int32.Parse(tb.Text);
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
            }
            return def;
        }

        private double[] GetDouble(ToolStripTextBox[] tb, double[] def)
        {
            double[] d = new double[tb.Length];
            for (int i = 0; i < d.Length; i++)
            {
                d[i] = GetDouble(tb[i], d[i]);
            }
            return d;
        }

        private int[] GetInt(ToolStripTextBox[] tb, int[] def)
        {
            int[] d = new int[tb.Length];
            for (int i = 0; i < d.Length; i++)
            {
                d[i] = GetInt(tb[i], d[i]);
            }
            return d;
        }

        double[] ControlDouble
        {
            get
            {
                double[] d = new double[]{StaticExtensionBasicEngineering.StartTime,
                StaticExtensionBasicEngineering.StartStep};
                double[] dx = GetDouble(new ToolStripTextBox[]
            {
                toolStripTextBoxStart,
                toolStripTextBoxStep
            }, d);
                return dx;
            }
        }

        int[] ControlInt
        {
            get
            {
                ToolStripTextBox[] tb = new ToolStripTextBox[]
      {
            toolStripTextBoxStepCount,
            toolStripTextBoxPause,
            toolStripTextBoxTimeIndicator
      };

                int[] i = new int[]
        {
           StaticExtensionBasicEngineering.StepCount,
          StaticExtensionBasicEngineering.Pause,
          StaticExtensionBasicEngineering.TimeIndicator
 

        };
                return GetInt(tb, i);
            }
        }

        bool SaveSettings()
        {
            GC.Collect();

            if (creator.Holder != null)
            {
                DialogResult dr = WindowsExtensions.ControlExtensions.ShowMessageBoxModal(
                    ResourceService.Resources.GetControlResource("Save data?", Diagram.UI.Utils.ControlUtilites.Resources),
                    ResourceService.Resources.GetControlResource(creator.Text, Diagram.UI.Utils.ControlUtilites.Resources),
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (dr == DialogResult.Cancel)
                {
                    return false;
                }
                if (dr == DialogResult.No)
                {
                    return true;
                }

            }
            StaticExtensionBasicEngineering.FullScreen = (WindowState == FormWindowState.Maximized);
            StaticExtensionBasicEngineering.Left = Left;
            StaticExtensionBasicEngineering.Top = Top;
            StaticExtensionBasicEngineering.Width = Width;
            StaticExtensionBasicEngineering.Height = Height;
            if (dockManager != null)
            {
                //dockManager.Set.SetLayouot();
                StaticExtensionBasicEngineering.LeftPortion = dockManager.DockLeftPortion;
                StaticExtensionBasicEngineering.RightPortion = dockManager.DockRightPortion;
            }
            MemoryStream ms = new MemoryStream();
            if (!SaveAll(ms))
            {
                return false;
            }
            double[] d = ControlDouble;
            StaticExtensionBasicEngineering.StartTime = d[0];
            StaticExtensionBasicEngineering.Step = d[1];
            int[] ii = ControlInt;
            StaticExtensionBasicEngineering.StepCount = ii[0];
            StaticExtensionBasicEngineering.Pause = ii[1];
            StaticExtensionBasicEngineering.TimeIndicator = ii[2];
            StaticExtensionBasicEngineering.LogConnectionString = StaticExtensionEventLogDatabase.ConnectionString;
            StaticExtensionBasicEngineering.Save();
            if (creator.Holder == null)
            {
                byte[] b = ms.GetBuffer();
                StaticExtensionBasicEngineering.SavedState = b.BytesToString();
                StaticExtensionBasicEngineering.Save();
            }
            else
            {
                PureDesktopPeer edit = new PureDesktopPeer();
                active.Copy(edit);
                ByteHolder holder = new ByteHolder();
                holder.Bytes = edit.Content;
                creator.Holder = holder;
            }
            return true;
        }

        private void SetCheckLevel()
        {
            int i = toolStripComboBoxCheckDetails.SelectedIndex;
            if (i == 0)
            {
                FormulaMeasurement.CheckValue =
                    FormulaMeasurement.Check;
            }
            else
            {
                FormulaMeasurement.CheckValue =
                    FormulaMeasurement.EmptyCheck;
            }
            StaticExtensionBasicEngineering.CheckLevel = i;
            StaticExtensionBasicEngineering.Save();
        }

        private void EditAliases()
        {
            FormAliasEdit form = new FormAliasEdit();
            form.Desktop = active.Desktop;
            form.Show();
        }

        #endregion

        #endregion

    }
}


