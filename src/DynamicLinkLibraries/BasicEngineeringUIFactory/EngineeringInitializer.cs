﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Diagram.UI;
using Diagram.UI.Interfaces;
using Diagram.UI.Factory;

using DataWarehouse;
using DataWarehouse.Interfaces;
using EngineeringInitializer;

using DataPerformer;
using DataPerformer.Interfaces;



using BasicEngineering.UI.Factory.Forms;
using BasicEngineering.UI.Factory.Interfaces;
using DataPerformer.Portable.Interfaces;

namespace BasicEngineering.UI.Factory
{
    /// <summary>
    /// Initializer of engineering allpication
    /// </summary>
    public class EngineeringInitializer : IApplicationInitializer
    {
        #region Fields

        IDatabaseCoordinator coordinator;

        private static bool isInitialized;

        OrdinaryDifferentialEquations.IDifferentialEquationSolver ordSolver;
        DataPerformer.Portable.DifferentialEquationProcessors.DifferentialEquationProcessor diffProcessor;
            IDataRuntimeFactory strategy;
            IApplicationInitializer[] initializers;
            bool throwsRepeatException;
            Dictionary<string, object>[] resources;
            private TextWriter logWriter = null;

        #endregion

        #region Ctor

        public EngineeringInitializer(IDatabaseCoordinator coordinator, OrdinaryDifferentialEquations.IDifferentialEquationSolver ordSolver,
           DataPerformer.Portable.DifferentialEquationProcessors.DifferentialEquationProcessor diffProcessor,
            DataPerformer.Portable.Interfaces.IDataRuntimeFactory strategy, IApplicationInitializer[] initializers,
            bool throwsRepeatException, Dictionary<string, object>[] resources, TextWriter logWriter)
        {
            this.coordinator = coordinator;
            this.ordSolver = ordSolver;
            this.diffProcessor = diffProcessor;
            this.strategy = strategy;
            this.initializers = initializers;
            this.throwsRepeatException = throwsRepeatException;
            if (coordinator == null)
            {
                DataWarehouse.StaticExtensionDataWarehouse.SetAppBaseCoordinator();
            }
            else
            {
                DataWarehouse.StaticExtensionDataWarehouse.Coordinator = coordinator;
            }
            this.resources = resources;
            this.logWriter = logWriter;
        }

        #endregion

        #region IApplicationInitializer Members

        void IApplicationInitializer.InitializeApplication()
        {
            if (isInitialized)
            {
                if (throwsRepeatException)
                {
                    throw new Exception("Double initialization");
                }
                return;
            }
            isInitialized = true;
            StaticExtensionDataWarehouse.Coordinator = coordinator;
            Initialize(ordSolver, strategy, coordinator, initializers, throwsRepeatException);
            if (logWriter == null)
            {
                //DataPerformer.UI.Utils.ControlUtilites.ErrorHandler = DataPerformer.UI.Utils.DataPerformerErrorHandler.Object;
            }
            else
            {
               // DataPerformer.UI.Utils.ControlUtilites.ErrorHandler = new Diagram.UI.ErrorHandlers.TextWriterErrorHandler(logWriter);
            }

            // ================== Formula Editor UI preparation ================

            Chart.Classes.DataTextChooser.Localize = delegate(Control control)
            {
                ResourceService.Resources.LoadControlResources(control, resources);
            };

            FormulaEditor.UI.FormulaEditorPanel fp = new FormulaEditor.UI.FormulaEditorPanel();
            fp.Prepare();
            string seb = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string sel = "abcdefghijklmnopqrstuvwxyz_";
            string[] sym = new string[] { seb, sel };
            System.Globalization.CultureInfo c = System.Globalization.CultureInfo.CurrentCulture;
            if (c.TwoLetterISOLanguageName.ToLower().Equals("ru"))
            {
                string srb = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЬЫЪЭЮЯ";
                string srl = "абвгдеёжзийклмнопрстуфхцчшщьыъэюя";
                sym = new string[] { seb, sel, srb, srl };
            }
            
            //char ct = '\u0442';

            DataPerformer.UI.PanelFormula.Symbols = sym;

            // ================== End formula Editor UI preparation ================


            DataPerformer.UI.HeaderControl.Object = new DataPerformer.UI.HeaderControl();
            PanelDesktop.Cleaner = null;

            DataPerformer.UI.SimplePointCollectionChooserFactory.Set();
            Chart.Drawing.Factory.CirclePointFactory.Set();
        }

        #endregion

        private static void Initialize(OrdinaryDifferentialEquations.IDifferentialEquationSolver ordSolver,
            IDataRuntimeFactory strategy, IDatabaseCoordinator coordinator, IApplicationInitializer[] initializers,
            bool throwsRepeatException)
        {
            List<IApplicationInitializer> init = null;
            if (initializers == null)
            {
                init = new List<IApplicationInitializer>();
            }
            else
            {
                init = new List<IApplicationInitializer>(initializers);
            }
            IApplicationInitializer initializer = new BasicEngineeringInitializer(ordSolver, 
                DataPerformer.Portable.DifferentialEquationProcessors.RungeProcessor.Processor, strategy, init.ToArray(), 
                throwsRepeatException);
            initializer.InitializeApplication();
        }

 
    }
}
