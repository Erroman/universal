﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseTypes.Interfaces;
using CategoryTheory;
using DataPerformer.Formula.Interfaces;
using DataPerformer.Interfaces;
using DataPerformer.Portable;
using Diagram.UI;
using Diagram.UI.Aliases;
using Diagram.UI.Labels;
using Event.Interfaces;
using FormulaEditor;
using FormulaEditor.Interfaces;
using FormulaEditor.Symbols;

namespace DataPerformer.Formula
{
    /// <summary>
    /// Vector formula data transformer
    /// </summary>
    public class VectorFormulaConsumer :
       DataConsumerMeasurements,
       ITimeMeasureConsumer, IVariableDetector,
       IStarted, IRuntimeUpdate, ITreeCollection, ITimeVariable,
        IReplaceMeasurements, IPostSetArrow
    {

        #region Fields

        /// <summary>
        /// String representation of formulas
        /// </summary>
        protected string[] formulaString = new string[0];

        /// <summary>
        /// Formulas
        /// </summary>
        protected MathFormula[] formula  = new MathFormula[0];

        /// <summary>
        /// Dictionary of acceptors
        /// </summary>
        private Dictionary<string, IOperationAcceptor> acc = new Dictionary<string, IOperationAcceptor>();

        /// <summary>
        /// Operation table
        /// </summary>
        protected Dictionary<int, IOperationAcceptor> opTable;

        /// <summary>
        /// Formula creator
        /// </summary>
        protected IFormulaObjectCreator creator;

        /// <summary>
        /// Proxy
        /// </summary>
        protected ITreeCollectionProxy proxy;

        /// <summary>
        /// Time variable
        /// </summary>
        protected VariableMeasurement timeVariable = null;

        /// <summary>
        /// Proxy factory
        /// </summary>
        protected ITreeCollectionProxyFactory proxyFactory = null;

        private IMeasurements th;

   
        #endregion

        #region Ctor

        /// <summary>
        /// Default Constructor
        /// </summary>
        protected VectorFormulaConsumer()
        {
            th = this;
            proxyFactory = StaticExtensionFormulaEditor.Factory;
        }

        #endregion

        #region Overriden Members

        #endregion

        #region IVariableDetector Members

        IOperationAcceptor IVariableDetector.Detect(MathSymbol sym)
        {
            string key = sym.String;
            if (key.Length > 1)
            {
                return null;
            }
            if (!(sym is SimpleSymbol))
            {
                return null;
            }
            SimpleSymbol ss = sym as SimpleSymbol;
            if (!ss.Italic)
            {
                return null;
            }
            if (acc.ContainsKey(key))
            {
                return acc[key];
            }
            return VariableDetector.Detect(sym, acc);
        }

        #endregion

        #region IStarted Members

        void IStarted.Start(double time)
        {
            foreach (FormulaMeasurement fm in measurements)
            {
                fm.Reset();
            }
            if (forward.Count > 0)
            {
                isUpdated = false;
                th.UpdateMeasurements();
                foreach (IMeasurement m in forward.Keys)
                {
                    forward[m].Value = m.Parameter();
                }
            }
        }

        #endregion

        #region IRuntimeUpdate Members

        bool IRuntimeUpdate.ShouldRuntimeUpdate
        {
            get { return shouldRuntimeUpdate; }
            set { shouldRuntimeUpdate = value; }
        }

        #endregion

        #region ITreeCollection Members

        ObjectFormulaTree[] ITreeCollection.Trees
        {
            get
            {
                return FormulaMeasurement.GetTrees(measurements);
            }
        }

        bool ITreeCollection.IsValid
        {
            get
            {
                return proxy != null;
            }
        }

        #endregion

        #region ITimeVariable Members

        VariableMeasurement ITimeVariable.Variable
        {
            get { return timeVariable; }
        }

        #endregion

        #region IReplaceMeasurements Members

        void IReplaceMeasurements.Replace(IMeasurements oldMeasurements, 
            IMeasurement oldMeasure, IMeasurements newMeasurements, IMeasurement newMeasure)
        {
            if (oldMeasure == newMeasure)
            {
                return;
            }
            foreach (string key in acc.Keys)
            {
                object op = acc[key];
                if (!(op is VariableMeasurement))
                {
                    continue;
                }
                VariableMeasurement vm = op as VariableMeasurement;
                if (vm.Measurement == oldMeasure)
                {
                    if (vm == timeVariable)
                    {
                        timeVariable = null;
                    }
                    vm.Measurement = newMeasure;
                    string s = vm.Symbol + " = ";
                    int i = 0;
                    for (; i < arguments.Count; i++)
                    {
                        string ss = arguments[i] as string;
                        ss = ss.Substring(0, 4);
                        if (ss.Equals(s))
                        {
                            break;
                        }
                    }
                    if (i < arguments.Count)
                    {
                        string so = arguments[i] as string;
                        string sr = s + this.GetRelativeName(newMeasurements as IAssociatedObject);
                        sr += "." + newMeasure.Name;
                        replacement[sr] = so;
                        arguments.Insert(i, sr);
                        arguments.RemoveAt(i + 1);
                    }
                }
            }
        }

        #endregion

        #region IPostSetArrow

        /// <summary>
        /// The operation that performs after arrows setting
        /// </summary>
        public void PostSetArrow()
        {

            try
            {
                PostDeserialization();
                isSerialized = true;
                DynamicalParameter parameter = new DynamicalParameter();
                foreach (IMeasurements measurements in measurementsData)
                {
                    string name = this.GetMeasurementsName(measurements);
                    for (int i = 0; i < measurements.Count; i++)
                    {
                        IMeasurement measure = measurements[i];
                        string p = name + "." + measure.Name;
                        List<string> arg = new List<string>(arguments);
                        foreach (string s in arg)
                        {
                            string ss = s.Substring(4);
                            //!!!TEMP====================
                            bool b = ss.Equals(p);
                            if (!b)
                            {
                                if (ss.Replace("/", "_").Equals(p))
                                {
                                    arguments.Remove(s);
                                    arguments.Add(s.Substring(0, 4) + p);
                                    b = true;
                                }
                            }
                            if (b)
                            //!!!TEMP ===
                            {
                                char c = s[0];
                                parameter.Add(c, measure);
                                string key = c + "";
                                if (!acc.ContainsKey(key))
                                {
                                    acc[c + ""] = c.Create( measure, this);
                                }
                            }
                        }
                    }
                }
                timeVariable = null;
                IMeasurement timeMeasure = StaticExtensionDataPerformerPortable.Factory.TimeProvider.TimeMeasurement;
                foreach (string s in arguments)
                {
                    if (s.Substring(4).Equals("Time"))
                    {
                        //timeChar = s[0];
                        // parameter.Add(s[0], timeMeasure);
                        string key = s[0] + "";
                        if (!acc.ContainsKey(key))
                        {
                            timeVariable = s[0].Create(timeMeasure, this);
                            acc[key] = timeVariable;
                        }
                    }
                }
                Parameter = parameter;
                string argStr = AllVariables;
                foreach (string key in parameters.Keys)
                {
                    if (!acc.ContainsKey(key))
                    {
                        acc[key] = new AliasNameVariable(key, this, key);
                    }
                }
                postSetUnary();
            }
            catch (Exception ex)
            {
                ex.ShowError(10);
                if (deriOrder >= 0)
                {
                    if (ex.Message.Equals("VariableMeasure.Derivation"))
                    {
                        --deriOrder;
                        PostSetArrow();
                        return;
                    }
                }
                this.Throw(ex);
            }
            SetFeedback();
            SetForward();
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Table of operations
        /// </summary>
        public Dictionary<int, IOperationAcceptor> OperationTable
        {
            set
            {
                SeriesSymbol.SetOperations(formula, value);
                opTable = value;
                setOperationNames(value);
            }
        }


        /// <summary>
        /// Forward alias
        /// </summary>
        public Dictionary<int, string> ForwardAliases
        {
            get
            {
                return forwardAliases;
            }
            set
            {
                forwardAliases = value;
                SetForward();
            }
        }

        /// <summary>
        /// Numbers of operations
        /// </summary>
        public List<int> OperationNumbers
        {
            get
            {
                List<int> l = new List<int>();
                l.AddRange(SeriesSymbol.GetOperationIndexes(formula));
                return l;
            }
        }





        /// <summary>
        /// Accept parameters
        /// </summary>
        /// <param name="s">String that contains parameters characters</param>
        public void AcceptParameters(string s)
        {
            parameters.Clear();
            string str = "";
            str = ElementaryObjectDetector.GetVariables(formula);
            foreach (char c in s)
            {
                if (str.IndexOf(c) < 0)
                {
                    throw new Exception("Illegal formula parameter");
                }
            }
            foreach (char c in s)
            {
                double a = 0;
                parameters["" + c] = a;
                //arg[c] = a;
            }
        }

        /// <summary>
        /// The "is complete" sign
        /// </summary>
        public bool IsComplete
        {
            get
            {
                int n = 0;
                foreach (object o in acc.Values)
                {
                    if (o is VariableMeasurement)
                    {
                        ++n;
                    }
                }
                return n == arguments.Count;
            }
        }



        /// <summary>
        /// The input dynamical parameter
        /// </summary>
        public override Portable.DynamicalParameter Parameter
        {
            set
            {
                par = value;
                proxy = null;
                if (!isSerialized)
                {
                    acc.Clear();
                    foreach (char c in par.Variables)
                    {
                        string key = c + "";
                        if (!acc.ContainsKey(key))
                        {
                            VariableMeasurement v = c.Create(par[c], this);
                            acc[key] = v;
                        }
                    }
                }
                int n = Dimension;
                measurements = new FormulaMeasurement[n];
                result = new object[n, 2];
                if (!isSerialized)
                {
                    arguments.Clear();
                }
                object ops = operationTable;
                if (!isSerialized)
                {
                    ops = opTable;
                }
                isSerialized = false;
                Dictionary<char, object> table = new Dictionary<char, object>();
                string var = par.Variables;
                Double a = 0;
                if (allVariables == null)
                {
                    allVariables = AllVariables;
                }
                foreach (char c in allVariables)
                {
                    table[c] = a;
                }
                string proh = "";
                foreach (char c in var)
                {
                    IMeasurement m = par[c];
                    object t = m.Type;
                    if (t is IOneVariableFunction | t is Table2D | t is Table3D)
                    {
                        proh += c;
                        table[c] = m.Parameter();
                    }
                    else
                    {
                        table[c] = t;
                    }
                }
                foreach (string s in parameters.Keys)
                {
                    object o = parameters[s];
                    table[s[0]] = AliasTypeDetector.Detector.DetectType(o);
                    if (!acc.ContainsKey(s))
                    {
                        acc[s] = new AliasNameVariable(s, this, s);
                    }
                }
                AssociatedAddition aa = new AssociatedAddition(this, null);
                for (int i = 0; i < n; i++)
                {
                    MathFormula f = formula[i].FullTransform(proh);
                    Dictionary<int, IOperationAcceptor> dop = new Dictionary<int, IOperationAcceptor>();
                    if (ops is Dictionary<int, IOperationAcceptor>)
                    {
                        dop = ops as Dictionary<int, IOperationAcceptor>;
                    }
                    SeriesSymbol.SetOperations(f, dop);
                    try
                    {
                        if (creator == null)
                        {
                            creator = VariableDetector.GetCreator(this);
                        }
                        ObjectFormulaTree t = ObjectFormulaTree.CreateTree(f, creator);
                        measurements[i] = FormulaMeasurement.Create(t, deriOrder, Formula_ + (i + 1), aa);
                    }
                    catch (Exception ex)
                    {
                        ex.ShowError(10);
                        if (ex.Message.Equals("VariableMeasure.Derivation"))
                        {
                            throw ex;
                        }
                        throw new Exception("Formula " + (i + 1) + " : " + ex.Message);
                    }
                }
                try
                {
                    proxy = null;
                    proxy = proxyFactory.CreateProxy(this, StaticExtensionFormulaEditor.CheckValue);
                    update = proxy.Update;
                    FormulaMeasurement.Set(measurements, proxy);
                    foreach (string key in replacement.Keys)
                    {
                        string varp = key[0] + "";
                        if (!acc.ContainsKey(varp))
                        {
                            continue;
                        }
                        object o = acc[varp];
                        if (!(o is VariableMeasurement))
                        {
                            continue;
                        }
                        VariableMeasurement vm = o as VariableMeasurement;
                        string name = key.Substring(4);
                        foreach (IMeasurements mea in measurementsData)
                        {
                            string sn = this.GetRelativeName(mea as IAssociatedObject);
                            for (int i = 0; i < mea.Count; i++)
                            {
                                IMeasurement mm = mea[i];
                                string snn = sn + "." + mm.Name;
                                if (snn.Equals(name))
                                {
                                    vm.Measurement = mm;
                                    goto me;
                                }
                            }
                        }
                    me:
                        continue;
                    }
                    SetFeedback();
                }
                catch (Exception ex)
                {
                    ex.ShowError(-1);
                }
            }
        }

        /// <summary>
        /// All formulas variables
        /// </summary>
        public override string AllVariables
        {
            get
            {
                //try
                //{
                if (allVariables == null)
                {
                    formula = new MathFormula[formulaString.Length];
                    for (int i = 0; i < formula.Length; i++)
                    {
                        formula[i] = MathFormula.FromString(MathSymbolFactory.Sizes, formulaString[i]);
                    }
                    allVariables = ElementaryObjectDetector.GetVariables(formula);
                }
                return allVariables;
            }
        }

        /// <summary>
        /// Variables
        /// </summary>
        public override string Variables
        {
            get
            {
                try
                {
                    string s = "";
                    string str = ElementaryObjectDetector.GetVariables(formula);
                    foreach (char c in str)
                    {
                        if (!parameters.ContainsKey("" + c) & s.IndexOf(c) < 0)
                        {
                            s += c;
                        }
                    }
                    return s;
                }
                catch (Exception ex)
                {
                   ex.ShowError(10);
                }
                return "";
            }
        }


        /// <summary>
        /// Set formula to i - th component
        /// </summary>
        /// <param name="s">The string representation of formula</param>
        /// <param name="i">The component number</param>
        public void SetFormula(string s, int i)
        {
            formulaString[i] = s;
        }

        /// <summary>
        /// Gets i - th component formula
        /// </summary>
        /// <param name="i">The component number</param>
        /// <returns>The string representation of formula</returns>
        public string GetFormula(int i)
        {
            return formulaString[i];
        }

        /// <summary>
        /// Dimension
        /// </summary>
        public int Dimension
        {
            get
            {
                return formulaString.Length;
            }
            set
            {
                formulaString = new string[value];
                result = null;
                formula = null;
            }
        }

        /// <summary>
        /// Accepts formulas
        /// </summary>
        public void AcceptFormulas()
        {
            try
            {
                int n = Dimension;
                measurements = new FormulaMeasurement[n];
                formula = new MathFormula[n];
                result = new object[n, 2];
                parameters.Clear();
                arguments.Clear();
                par = null;
                for (int i = 0; i < n; i++)
                {
                    MathFormula form = MathFormula.FromString(MathSymbolFactory.Sizes, formulaString[i]);
                    formula[i] = form;
                }
                allVariables = ElementaryObjectDetector.GetVariables(formula);
            }
            catch (Exception e)
            {
                e.ShowError(10);
                this.Throw(e);
            }
        }

        #endregion

        #region Protected Members

        #region Protected Virtual Members

        /// <summary>
        /// Time measure
        /// </summary>
        protected override IMeasurement TimeMeasure
        {
            get
            {
                return this.GetTimeMeasure();
            }
            set
            {
               value.Set(this);
            }
        }

        /// <summary>
        /// The count of measurements
        /// </summary>
        protected override int MeasurementsCount
        {
            get { return formulaString.Length; }
        }

        /// <summary>
        /// Post deserializatoion
        /// </summary>
        protected virtual void PostDeserialization()
        { }

        #endregion

        /// <summary>
        /// Initialization
        /// </summary>
        protected void Init()
        {
            update = () =>
            {
                foreach (FormulaMeasurement m in measurements)
                {
                    m.Update();
                }
            };
            creator = VariableDetector.GetCreator(this);
        }

        /// <summary>
        /// Sets forward aliases
        /// </summary>
        protected void SetForward()
        {
            this.SetMeasureAliasLinks(forwardAliases, forward);
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Accepts formulas
        /// </summary>
        private void acceptFormulas()
        {
            try
            {
                AssociatedAddition aa = new AssociatedAddition(this, null);
                int n = Dimension;
                measurements = new FormulaMeasurement[n];
                result = new object[n, 2];
                for (int i = 0; i < n; i++)
                {
                    MathFormula formula = MathFormula.FromString(MathSymbolFactory.Sizes, formulaString[i]);
                    MathFormula f = formula.FullTransform(null);
                    ObjectFormulaTree t = ObjectFormulaTree.CreateTree(f, ElementaryFunctionsCreator.Object);
                    measurements[i] = FormulaMeasurement.Create(t, deriOrder, Formula_ + (i + 1), aa);
                }
            }
            catch (Exception e)
            {
                e.ShowError(10);
                this.Throw(e);
            }

        }

        private void SetFeedback()
        {
            feedAliases.Clear();
            foreach (int i in feedback.Keys)
            {
                feedAliases[i] = this.FindAliasName(feedback[i], false);
            }
        }

        private void setOperationNames(Dictionary<int, IOperationAcceptor> table)
        {
            operationNames.Clear();
            foreach (int i in table.Keys)
            {
                ICategoryObject o = table[i] as ICategoryObject;
                IObjectLabel l = o.Object as IObjectLabel;
                operationNames[i] = this.GetRelativeName(o);
            }
        }

        private void setUnaryTrees(Dictionary<int, ICategoryObject> table)
        {
            string str = Variables;
        }

        private void postSetUnary()
        {
            Dictionary<int, ICategoryObject> t = operationTable;
            setUnaryTrees(t);
        }

        private Dictionary<int, ICategoryObject> operationTable
        {
            get
            {
                Dictionary<int, ICategoryObject> t = new Dictionary<int, ICategoryObject>();
                foreach (ICategoryObject o in operations)
                {
                    IObjectLabel l = o.Object as IObjectLabel;
                    string name = this.GetRelativeName(o);//l.Name;
                    if (!operationNames.ContainsValue(name))
                    {
                        continue;
                    }
                    foreach (int i in operationNames.Keys)
                    {
                        if (name.Equals(operationNames[i]))
                        {
                            t[i] = o;
                            break;
                        }
                    }
                }
                return t;
            }
        }

        #endregion

    }
}