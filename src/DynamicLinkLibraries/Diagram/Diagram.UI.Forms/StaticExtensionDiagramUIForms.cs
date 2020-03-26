﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using CategoryTheory;

using Diagram.UI.Interfaces;
using Diagram.UI.Labels;
using Diagram.UI.Interfaces.Labels;
using Diagram.UI.UserControls;

using ResourceService;

namespace Diagram.UI
{

    /// <summary>
    /// Static extension
    /// </summary>
    [InitAssembly]
    public static class StaticExtensionDiagramUIForms
    {

        #region Fields

        /// <summary>
        /// All forms
        /// </summary>
        private static List<Form> forms = new List<Form>();

        private static List<IDragDrop> dragDrop;

        private static IStartFile startFile = new ApplicationStartFile();

        private static string ext;

        private static Action<Form> postFormLoad = (Form f) => { };

        private static Action<IObjectLabelUI> postLabelLoad = (IObjectLabelUI l) => { };

        #endregion

        #region Public Members


        /// <summary>
        /// Sets control
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="control">Control</param>
        static public void SetControl(this IObjectLabelUI label, Control control)
        {
            UserControlBaseLabel bl = (label as Control).FindChild<UserControlBaseLabel>();
            foreach (Control cc in bl.Controls)
            {
                string ccn = cc.Name;
                if (ccn.Equals("panelCenter"))
                {
                    Control contr = null;
                    foreach (Control ccc in cc.Controls)
                    {
                        contr = ccc;
                        break;
                    }
                    cc.Controls.Remove(contr);
                    contr.Dock = DockStyle.Fill;
                    control.Controls.Add(contr);
                    return;
                }
            }
        }

        /// <summary>
        /// Sets control of the object
        /// </summary>
        /// <param name="categoryObject">The object</param>
        /// <param name="control">The control</param>
        /// <returns>True in case of succces and false otherwise</returns>
        static public bool SetControl(this ICategoryObject categoryObject, Control control)
        {
            IUIFactory factory = StaticExtensionDiagramUIFactory.UIFactory;
            IObjectLabelUI label = factory.CreateLabel(categoryObject);
            if (label == null)
            {
                return false;
            }
            label.SetControl(control);
            return true;
        }

        /// <summary>
        /// Preparation of a control
        /// </summary>
        /// <param name="control">The control</param>
        static public void Prepare(this Control control)
        {
            if (control is IPreparation)
            {
                (control as IPreparation).Prepare();
            }
            foreach (Control c in control.Controls)
            {
                c.Prepare();
            }
        }

        /// <summary>
        /// Opens log directory
        /// </summary>
        static public void OpenLogDirectory()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory + @"Logs\";
            if (!Directory.Exists(dir))
            {
                return;
            }

            try
            {
                ProcessStartInfo info = new ProcessStartInfo("explorer", dir);
                info.RedirectStandardOutput = true;
                info.RedirectStandardInput = true;
                info.RedirectStandardError = true;
                info.UseShellExecute = false;
                info.CreateNoWindow = false;
                Process p = new Process();
                p.StartInfo = info;
                p.Start();
                p.WaitForExit();
            }

            catch (Exception ex)
            {
                ex.ShowError();
            }
        }

        /// <summary>
        /// Saves comments
        /// </summary>
        /// <param name="control">The control</param>
        public static ICollection GetComments(this Control control)
        {
            UserControlComments uc = control.FindChild<UserControlComments>();
            if (uc != null)
            {
                return uc.Comments;
            }
            return null;
        }

        /// <summary>
        /// Saves comments
        /// </summary>
        /// <param name="control">The control</param>
        /// <param name="comments">The control</param>
        public static void SetComments(this Control control, ICollection comments)
        {
            UserControlComments uc = control.FindChild<UserControlComments>();
            if (uc != null)
            {
                uc.Comments = comments;
            }
       }

        /// <summary>
        /// For each control acton
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="action">Action</param>
        public static void ForEeach(this Control control, Action<Control> action)
        {
            action(control);
            foreach (Control c in control.Controls)
            {
                c.ForEeach(action);
            }
        }

        /// <summary>
        /// Post set controls
        /// </summary>
        /// <param name="control">The control</param>
        public static void PostSet(this Control control)
        {
            if (control is IPostSet)
            {
                (control as IPostSet).Post();
            }
            foreach (Control c in control.Controls)
            {
                c.PostSet();
            }
        }

        /// <summary>
        /// Finds parent
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="control">The control</param>
        /// <returns>The parent</returns>
        public static T FindParent<T>(this Control control) where T : Control
        {
            if (control is T)
            {
                return control as T;
            }
            Control c = control.Parent;
            if (c != null)
            {
                return c.FindParent<T>();
            }
            return null;
        }

        /// <summary>
        /// Finds child
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="control">The control</param>
        /// <returns>The child</returns>
        public static T FindChild<T>(this Control control) where T : Control
        {
            if (control is T)
            {
                return control as T;
            }
            foreach (Control c in control.Controls)
            {
                T t = c.FindChild<T>();
                if (t != null)
                {
                    return t;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds child object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="control">The control</param>
        /// <param name="exludeOwn">Exclude own component</param>
        /// <returns>The child object</returns>
        public static T FindChildObject<T>(this Control control, bool exludeOwn = true) where T : class
        {
            if (!exludeOwn)
            {
                if (control is T)
                {
                    return control as T;
                }
            }
            foreach (Control c in control.Controls)
            {
                T t = c.FindChildObject<T>(false);
                if (t != null)
                {
                    return t;
                }
            }
            return null;
        }





        /// <summary>
        /// Post form load
        /// </summary>
        public static Action<Form> PostFormLoad
        {
            get
            {
                return postFormLoad;
            }
            set
            {
                postFormLoad = value;
            }
        }

  

        /// <summary>
        /// Gets parent label of the control
        /// </summary>
        /// <param name="control">The control</param>
        /// <returns>The lablel</returns>
        public static IObjectLabel GetParentLablel(this Control control)
        {
            Control c = control.Parent;
            if (c == null)
            {
                return null;
            }
            if (c is IObjectLabel)
            {
                return c as IObjectLabel;
            }
            return c.GetParentLablel();
        }

        /// <summary>
        /// Post label load
        /// </summary>
        public static Action<IObjectLabelUI> PostLabelLoad
        {
            get
            {
                return postLabelLoad;
            }
            set
            {
                postLabelLoad = value;
            }
        }

  
  

        /// <summary>
        /// Extension
        /// </summary>
        public static string Extension
        {
            get
            {
                return ext;
            }
            set
            {
                ext = value;
            }
        }

        /// <summary>
        /// Starts files
        /// </summary>
        /// <param name="files">Files</param>
        public static bool Start(this string[] files)
        {
            List<string> fn = new List<string>();
            foreach (string f in files)
            {
                if (System.IO.Path.GetExtension(f).ToLower().Equals(ext))
                {
                    fn.Add(f);
                }
                if (fn.Count > 1)
                {
                    foreach (string file in fn)
                    {
                        startFile.Start(file);
                    }
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Starts buffers
        /// </summary>
        /// <param name="buffers">buffers</param>
        public static void Start(this IEnumerable<byte[]> buffers)
        {
            foreach (byte[] buffer in buffers)
            {
                startFile.Start(buffer);
            }
        }

        /// <summary>
        /// Gets coordinates of control
        /// </summary>
        /// <param name="control">The control</param>
        /// <param name="x">X - coordinate</param>
        /// <param name="y">Y - coordinate</param>
        public static void GetCoordinates(this Control control, out int x, out int y)
        {
            x = control.Left;
            y = control.Top;
            Control p = control.Parent;
            if (p == null)
            {
                return;
            }
            int xp = 0;
            int yp = 0;
            GetCoordinates(p, out xp, out yp);
            x += xp;
            y += yp;
        }

        /// <summary>
        /// Performs action with form
        /// </summary>
        /// <param name="form">Form for action</param>
        /// <param name="type">Action type</param>
        /// <param name="actionType">Type of action</param>
        public static void Action(this Form form, object type, ActionType actionType)
        {
            if (form != null)
            {
                if (!form.IsDisposed)
                {
                    if (form is IStartStop)
                    {
                        IStartStop ss = form as IStartStop;
                        ss.Action(type, actionType);
                    }
                }
            }
        }

        /// <summary>
        /// Pre removes control
        /// </summary>
        /// <param name="control">The control</param>
        public static void PreRemove(this Control control)
        {
            if (control == null)
            {
                return;
            }
            if (control.IsDisposed)
            {
                return;
            }
            foreach (Control c in control.Controls)
            {
                c.PreRemove();
            }
            if (control is IShowForm)
            {
                IShowForm sf = control as IShowForm;
                object o = sf.Form;
                if (o != null)
                {
                    if (o is Control)
                    {
                        Control c = o as Control;
                        c.PreRemove();
                    }
                }
            }
            if (control is IPreRemove)
            {
                IPreRemove p = control as IPreRemove;
                p.PreRemove();
            }
        }

        /// <summary>
        /// Shows form
        /// </summary>
        /// <param name="sf">Object which shows form</param>
        public static void Show(this IShowForm sf)
        {
            sf.ShowForm();
            Form form = sf.Form as Form;
            if (form == null)
            {
                return;
            }
            if (forms.Contains(form))
            {
                if (form.IsDisposed)
                {
                    forms.Remove(form);
                }
                return;
            }
            else
            {
                forms.Add(form);
                if (sf is IEnabled)
                {
                    IEnabled en = sf as IEnabled;
                    form.FormClosed += (object senrer, FormClosedEventArgs ev) =>
                   {
                       try
                       {
                           if (forms.Contains(form))
                           {
                               forms.Remove(form);
                           }
                           if (!en.Enabled)
                           {
                               en.Enabled = true;
                           }
                       }
                       catch (Exception ex)
                       {
                           ex.ShowError(10);
                       }
                   };
                    if (en.Enabled)
                    {
                        en.Enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Post set arrow operation
        /// </summary>
        /// <param name="control">Control</param>
        public static void PostSetArrow(this Control control)
        {
            if (control is CategoryTheory.IPostSetArrow)
            {
                CategoryTheory.IPostSetArrow ps = control as CategoryTheory.IPostSetArrow;
                ps.PostSetArrow();
            }
            foreach (Control c in control.Controls)
            {
                c.PostSetArrow();
            }
        }

        /// <summary>
        /// Checks whether button preesd for arrow motion
        /// </summary>
        /// <param name="args">Mouse arguments</param>
        /// <returns>True for arrow motion and false otherwise</returns>
        static internal bool IsArrowClick(this MouseEventArgs args)
        {
            return (args.Button == MouseButtons.Left) & ((Control.ModifierKeys & Keys.Shift) != Keys.Shift);
        }

        /// <summary>
        /// Gets root label of control
        /// </summary>
        /// <param name="control">The control</param>
        /// <returns>The root label</returns>
        public static IObjectLabel GetRootLabel(this Control control)
        {
            IObjectLabel l = null;
            GetRootLabel(control, ref l);
            return l;
        }

        /// <summary>
        /// Gets form of control
        /// </summary>
        /// <param name="control">The control</param>
        /// <returns>The form</returns>
        public static Form GetForm(this Control control)
        {
            if (control is Form)
            {
                return control as Form;
            }
            return control.Parent.GetForm();
        }

        /// <summary>
        /// Gets palette buttons from control
        /// </summary>
        /// <param name="control">The control</param>
        /// <returns>Palette buttons</returns>
        public static Dictionary<Type, Dictionary<string, IPaletteButton>> GetPaletteButtons(this Control control)
        {
            Dictionary<Type, Dictionary<string, IPaletteButton>> dictionary =
                new Dictionary<Type, Dictionary<string, IPaletteButton>>();
            GetButtons(control, dictionary);
            return dictionary;
        }

        /// <summary>
        /// Saves Xml
        /// </summary>
        /// <param name="form">Parent form</param>
        /// <param name="doc">Xml document</param>
        public static void SaveXml(this IWin32Window form, XmlDocument doc)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = ResourceService.Resources.GetControlResource("Xml Files|*.xml", Diagram.UI.Utils.ControlUtilites.Resources);
            if (dialog.ShowDialog(form) != DialogResult.OK)
            {
                return;
            }
            doc.Save(dialog.FileName);
        }

        /// <summary>
        /// Saves xml
        /// </summary>
        /// <param name="form">Parent form</param>
        /// <param name="action">Action with filename</param>
        public static void SaveXml(this IWin32Window form, Action<string> action)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = Resources.GetControlResource("Xml Files|*.xml", Utils.ControlUtilites.Resources);
            if (dialog.ShowDialog(form) != DialogResult.OK)
            {
                return;
            }
            action(dialog.FileName);
      }



        /// <summary>
        /// Enables / disables buttons
        /// </summary>
        /// <param name="buttons">Buttons</param>
        /// <param name="enabled">True is enable</param>
        public static void SetEnabled(this IEnumerable<ToolStripButton> buttons, bool enabled)
        {
            foreach (ToolStripButton b in buttons)
            {
                b.Enabled = enabled;
            }

        }

        /// <summary>
        /// Shows localized text
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="resources">Localization resources</param>
        public static void ShowLocalized(this string text, Dictionary<string, object>[] resources)
        {
            text.GetControlResource(resources).Show();
        }

        /// <summary>
        /// Enables/disables buttons
        /// </summary>
        /// <param name="actionType">Action type</param>
        /// <param name="buttons">Buttons</param>
        public static void EnableDisableButtons(this Diagram.UI.Interfaces.ActionType actionType, ToolStripButton[][] buttons)
        {
            if (actionType == Diagram.UI.Interfaces.ActionType.Start |
                actionType == ActionType.Resume)
            {

                buttons[0].SetEnabled(false);
                buttons[1].SetEnabled(true);
                if (buttons.Length > 2)
                {
                    buttons[2].SetEnabled(true);
                }
                return;
            }

            if (actionType == Diagram.UI.Interfaces.ActionType.Stop)
            {
                buttons[0].SetEnabled(true);
                buttons[1].SetEnabled(false);
                if (buttons.Length > 2)
                {
                    buttons[2].SetEnabled(false);
                }
                return;
            }
            if (actionType == ActionType.Pause)
            {
                buttons[0].SetEnabled(true);
                buttons[1].SetEnabled(true);
                if (buttons.Length > 2)
                {
                    buttons[2].SetEnabled(false);
                }
                return;
            }

        }

        /// <summary>
        /// Updates form UI
        /// </summary>
        /// <param name="updatableForm">Form</param>
        public static void UpdateFormUI(this IUpdatableForm updatableForm)
        {
            updatableForm.UpdateFormUI();
        }

        /// <summary>
        /// First
        /// </summary>
        static bool first = true;

        /// <summary>
        /// Inits itself
        /// </summary>
        static public void Init()
        {
            if (!first)
            {
                return;
            }
            first = false;
            StaticExtensionCategoryTheory.FindObject = FindFormObject.Singleton;
            dragDrop = new List<IDragDrop>(
                AssemblyService.StaticExtensionAssemblyService.GetInterfacesFromBaseDirectory<IDragDrop>());
            new Binder();
        }

        /// <summary>
        /// Sets alias to property grid
        /// </summary>
        /// <param name="grid">The grid</param>
        /// <param name="a">The alias</param>
        static public void SetAlias(this PropertyGrid grid, IAlias a)
        {
            grid.SelectedObject = new Diagram.UI.PropertyEditors.AliasTable(a, false);
        }


        /// <summary>
        /// Sets double alias to property grid
        /// </summary>
        /// <param name="grid">The grid</param>
        /// <param name="a">The alias</param>
        static public void SetDoubleAlias(this PropertyGrid grid, IAlias a)
        {
            grid.SelectedObject = new Diagram.UI.PropertyEditors.AliasTable(a, true);
        }

        /// <summary>
        /// Sets array editor
        /// </summary>
        /// <param name="grid">Grid</param>
        /// <param name="types">Types</param>
        /// <param name="array">Array</param>
        static public void SetArrayEditor(this PropertyGrid grid, List<Tuple<string, object>> types, object[] array)
        {
            grid.SelectedObject = new PropertyEditors.ListArrayEditor(types, array);
        }

        /// <summary>
        /// Transforns an image to icon
        /// </summary>
        /// <param name="image">The image</param>
        /// <returns>The icon</returns>
        static public Icon ToIcon(this Image image)
        {
            using (Bitmap bmp = new Bitmap(image.Width, image.Height))
            {
                bmp.MakeTransparent(Color.White);
                Graphics.FromImage(bmp).DrawImage(image, 0, 0);
                return Icon.FromHandle(bmp.GetHicon());
            }

        }

        /// <summary>
        /// Gets base icon
        /// </summary>
        /// <param name="control">Control</param>
        /// <returns>Base icon</returns>
        static public System.Drawing.Icon GetBaseIcon(this Control control)
        {
            if (control == null)
            {
                return null;
            }
            if (control is UserControlLabel)
            {
                return (control as UserControlLabel).Icon;
            }
            return control.Parent.GetBaseIcon();
        }

        /// <summary>
        /// Creates Wrapper
        /// </summary>
        /// <param name="label">Wrapped label</param>
        /// <param name="icon">Icon</param>
        /// <param name="changeSize">The "change size" sign</param>
        /// <returns>Wrapper</returns>
        static public IObjectLabelUI CreateLabelUI(this IObjectLabel label, object icon, bool changeSize)
        {
            return UserControlLabel.CreateLabel(label, icon, changeSize);
        }

        /// <summary>
        /// Creates label
        /// </summary>
        /// <param name="type">Type of child label</param>
        /// <param name="changeSize">The "change size" sign</param>
        /// <returns>Object label</returns>
        public static IObjectLabelUI CreateLabelUI(this Type type, bool changeSize)
        {
            return UserControlBaseLabel.Create(type, changeSize);
        }


        #endregion

        #region Internal Members

        internal static void SetDragDrop(this PanelDesktop desktop)
        {
            foreach (IDragDrop dd in dragDrop)
            {
                IDragDrop d = dd.New;
                d.Set(null, desktop);
            }
        }

        #endregion

        #region Private Members


        private static void GetButtons(Control control, Dictionary<Type, Dictionary<string, IPaletteButton>> dictionary)
        {
            if (control is IPaletteButton)
            {
                IPaletteButton b = control as IPaletteButton;
                Dictionary<string, IPaletteButton> d = null;
                Type t = b.ReflectionType;
                if (dictionary.ContainsKey(t))
                {
                    d = dictionary[t];
                }
                else
                {
                    d = new Dictionary<string, IPaletteButton>();
                    dictionary[t] = d;
                }
                d[b.Kind] = b;
            }
            foreach (Control c in control.Controls)
            {
                GetButtons(c, dictionary);
            }
            if (control is ToolBar)
            {
                ToolBar tb = control as ToolBar;
                foreach (object o in tb.Buttons)
                {
                    if (o is IPaletteButton)
                    {
                        IPaletteButton b = o as IPaletteButton;
                        Dictionary<string, IPaletteButton> d = null;
                        Type t = b.ReflectionType;
                        if (dictionary.ContainsKey(t))
                        {
                            d = dictionary[t];
                        }
                        else
                        {
                            d = new Dictionary<string, IPaletteButton>();
                            dictionary[t] = d;
                        }
                        d[b.Kind] = b;
                    }
                }
            }
        }

        private static void GetRootLabel(Control c, ref IObjectLabel l)
        {
            if (c is IObjectLabel)
            {
                l = c as IObjectLabel;
            }
            Control p = c.Parent;
            if (p == null)
            {
                return;
            }
            GetRootLabel(p, ref l);
        }


        #endregion

        #region Ctor




        static StaticExtensionDiagramUIForms()
        {
            Init();
        }

        #endregion

        #region Serialization Binder Class

        class Binder : System.Runtime.Serialization.SerializationBinder
        {
            static bool first = true;

            readonly string ass = typeof(Binder).Assembly.FullName;

            internal Binder()
            {
                if (first)
                {
                    first = false;
                    this.Add();
                }
            }
            public override Type BindToType(string assemblyName, string typeName)
            {
                if (assemblyName.Contains("DiagramUIForm"))
                {
                    return Type.GetType(String.Format("{0}, {1}", typeName.Replace("DiagramUI.", "Diagram.UI."), ass));
                }
                return null;
            }
        }
       
        #endregion

    }
}