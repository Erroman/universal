﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;



using Diagram.UI;
using Diagram.UI.Interfaces;

using CommonService;

using DataPerformer;

using DataWarehouse;
using DataWarehouse.Interfaces;

using BasicEngineering.UI.Factory.Interfaces;
using BasicEngineering.UI.Factory.Forms;



namespace BasicEngineering.UI.Factory
{
    /// <summary>
    /// Default creator
    /// </summary>
    public class DefaultApplicationCreator : IApplicationCreator
    {

        #region Fields


        private LightDictionary<string, ButtonWrapper[]> buttons;

        Icon icon;
        EngineeringUIFactory factory;
        string filename;
        Action<double, double, int, int, int, IDesktop> start;
        string text;
        string ext;
        IApplicationInitializer initializer;

        ByteHolder holder;

        Dictionary<string, object>[] resources;

        IDatabaseCoordinator coordinator;

        TextWriter log = null;

        TestCategory.Interfaces.ITestInterface testInterface;

        string fileFilter;

 
        #endregion

        #region Ctor

        public DefaultApplicationCreator(IDatabaseCoordinator coordinator, LightDictionary<string, ButtonWrapper[]> buttons,
            Icon icon, EngineeringUIFactory factory, ByteHolder holder, string filename, Action<double, double, int, int, int, IDesktop> start, 
            Dictionary<string, object>[] resources, string text,
            string ext, string fileFilter, IApplicationInitializer initializer, TextWriter log,
            TestCategory.Interfaces.ITestInterface testInterface)
        {
            
            this.coordinator = coordinator;
            this.buttons = buttons;
            this.icon = icon;
            this.factory = factory;
            this.holder = holder;
            this.filename = filename;
            this.start = start;
            this.resources = resources;
            this.text = text;
            this.ext = ext;
            this.fileFilter = fileFilter;
            this.initializer = initializer;
            this.log = log;
            this.testInterface = testInterface;
        }

        #endregion

        #region IApplicationCreator Members

        LightDictionary<string, ButtonWrapper[]> IApplicationCreator.Buttons
        {
            get { return buttons; }
        }

        Icon IApplicationCreator.Icon
        {
            get { return icon; }
        }

        IUIFactory IApplicationCreator.Factory
        {
            get { return factory; }
        }

        string IApplicationCreator.Filename
        {
            get { return filename; }
        }

        Action<double, double, int, int, int, IDesktop> IApplicationCreator.Start
        {
            get { return start; }
        }

        string IApplicationCreator.Text
        {
            get { return text; }
        }

        string IApplicationCreator.Ext
        {
            get { return ext; }
        }

        string IApplicationCreator.FileFilter
        {
            get { return fileFilter; }
        }

        IApplicationInitializer IApplicationCreator.ApplicationInitializer
        {
            get { return initializer; }
        }

        ByteHolder IApplicationCreator.Holder
        {
            get
            {
                return holder;
            }
            set
            {
                holder = value;
            }
        }

        Dictionary<string, object>[] IApplicationCreator.Resources
        {
            get
            {
                return resources;
            }
        }


        void IApplicationCreator.LoadResources()
        {
            loadResources();
        }

        IDatabaseCoordinator IApplicationCreator.DatabaseCoordinator
        {
            get
            {
                return coordinator;
            }
        }

        TextWriter IApplicationCreator.Log
        {
            get { return log; }
        }


        TestCategory.Interfaces.ITestInterface IApplicationCreator.TestInterface
        {
            get { return testInterface; }
        }

 

        #endregion

        #region Members

        public static FormMain CreateForm(IDatabaseCoordinator coordinator, ByteHolder holder,
            OrdinaryDifferentialEquations.IDifferentialEquationSolver ordSolver,
    DataPerformer.Portable.DifferentialEquationProcessors.DifferentialEquationProcessor diffProcessor,
     DataPerformer.Portable.Interfaces.IDataRuntimeFactory strategy, IApplicationInitializer[] initializers,
            IUIFactory[] factories,
     bool throwsRepeatException, LightDictionary<string, ButtonWrapper[]> buttons, Dictionary<string, object>[]
            resources,
            Icon icon, string filename, Action<double, double, int, int, int, IDesktop> start, string text,
            string ext, string fileFilter, TextWriter log, TestCategory.Interfaces.ITestInterface testInterface)
        {
            EngineeringUIFactory factory = new EngineeringUIFactory(factories, true, ext);
            EngineeringInitializer initializer = new EngineeringInitializer(coordinator, ordSolver, diffProcessor, 
                strategy, initializers, throwsRepeatException, resources, log);
            DefaultApplicationCreator creator = new DefaultApplicationCreator(coordinator, buttons, icon, factory, holder, filename, start, 
              resources,  text,
            ext, fileFilter, initializer, log, testInterface);
            return CreateForm(creator);
        }

        public static FormMain CreateForm(IDatabaseCoordinator coordinator, ByteHolder holder,
             OrdinaryDifferentialEquations.IDifferentialEquationSolver ordSolver,
     DataPerformer.Portable.DifferentialEquationProcessors.DifferentialEquationProcessor diffProcessor,
             DataPerformer.Portable.Interfaces.IDataRuntimeFactory strategy,
      IApplicationInitializer[] initializers,
         IUIFactory[] factories,
      bool throwsRepeatException, LightDictionary<string, ButtonWrapper[]> buttons,
             Icon icon, string filename, Dictionary<string, object>[] resources, string text,
             string ext, string fileFilter, TextWriter logWriter, TestCategory.Interfaces.ITestInterface testInterface)
        {
            EngineeringUIFactory factory = new EngineeringUIFactory(factories, true, ext);
            EngineeringInitializer initializer = new EngineeringInitializer(coordinator, ordSolver, diffProcessor,
                strategy, initializers, throwsRepeatException, resources, logWriter);
            DefaultApplicationCreator creator = new DefaultApplicationCreator(coordinator, buttons, icon, factory, holder, 
                filename, factory.Start, resources, text,
            ext, fileFilter, initializer, logWriter, testInterface);
            return CreateForm(creator);
        }

        public static FormMain CreateForm(IDatabaseCoordinator coordinator, ByteHolder holder,
      OrdinaryDifferentialEquations.IDifferentialEquationSolver ordSolver,
DataPerformer.Portable.DifferentialEquationProcessors.DifferentialEquationProcessor diffProcessor,
IApplicationInitializer[] initializers,
  IUIFactory[] factories,
bool throwsRepeatException, LightDictionary<string, ButtonWrapper[]> buttons,
      Icon icon, string filename, Dictionary<string, object>[] resources, string text,
      string ext, string fileFilter, TextWriter logWriter, TestCategory.Interfaces.ITestInterface testInterface)
        {
            EngineeringUIFactory factory = new EngineeringUIFactory(factories, true, ext);
            EngineeringInitializer initializer = new EngineeringInitializer(coordinator, ordSolver, diffProcessor,
             DataPerformer.Runtime.DataRuntimeFactory.Object, initializers, throwsRepeatException, resources, logWriter);
            DefaultApplicationCreator creator = new DefaultApplicationCreator(coordinator, buttons, icon, factory, holder, filename, 
                factory.Start, resources, text,
            ext, fileFilter, initializer, logWriter, testInterface);
            return CreateForm(creator);
        }



        
 
        public static FormMain CreateForm(IApplicationCreator creator)
        {
            FormMain f = new FormMain(creator);
            return f;
        }


        static private void loadResources()
        {

            string sApp = ResourceService.Resources.CurrentDirectory;
            System.Globalization.CultureInfo c = System.Globalization.CultureInfo.CurrentCulture;
        }
 
        #endregion

    }
}
