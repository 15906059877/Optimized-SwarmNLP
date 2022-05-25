using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using SwarmNLP.Properties;
using Microsoft.CSharp;
using Evaluator;

namespace SwarmNLP
{
    //delegate float OptFunctionHandler(PointF pos);
    //delegate bool MeetsConstraintsHandler(PointF pos);
    
    delegate void Voidhandler();

    public partial class MainForm : Form
    {
        Thread SwarmThread;
        Thread RedrawThread;
        internal AutoResetEvent ProcessEvent = new AutoResetEvent(true);
        internal AutoResetEvent RefreshEvent = new AutoResetEvent(true);
        bool Shutdown;

        Bitmap LatestBG;

        float ImageWidth;
        float ImageHeight;

        Random RndGen = new Random(unchecked((int)DateTime.Now.Ticks));

        internal Setup Config = new Setup();

        // problem
        internal bool[] Max;
        internal MethodResults[] Objectives;
        internal MethodResults Constraints;

        internal float Time = 0;
       
        internal ProblemForm Problem;
        internal WindowForm Window;
        internal SwarmForm Swarm;

        internal int xDim = 0; // for x1
        internal int yDim = 1; // for x2
        internal int Obj = 0;

        // Sim status
        internal bool Play;
        bool Step;
        bool Reset;
        internal bool SetupSwarm = true;
        bool FreezeRefresh;

        internal float[] BestGlobalValue = new float[1];
        internal float[] BestGlobalCoord = new float[2];

        internal List<float[]> NonDominatingCoords = new List<float[]>();
        internal List<float[]> NonDominatingValues = new List<float[]>();

        internal List<Particle> Bugs = new List<Particle>();

        internal int BugSize = 2;
        internal int SolutionsKept = 100;

        int ColorBug = System.Drawing.ColorTranslator.ToOle(Color.LimeGreen);
        int ColorOpt = System.Drawing.ColorTranslator.ToOle(Color.Orange);



        public MainForm()
        {
            InitializeComponent();

            Problem = new ProblemForm(this);
            Window = new WindowForm(this);
            Swarm = new SwarmForm(this);

            Config.FunctionEqs.Add("Maximize: sin(x1 - x2)");

            // testing
            //Config.FunctionEq = ("Math.Pow(x1,2) + Math.Pow(x2,2)");
            //Config.FunctionEq = ("cos(abs(x1)+abs(x2))");
            //Config.FunctionEq = ("cos(abs(x1)+abs(x2))*(abs(x1)+abs(x2))");
            //Config.FunctionEq = ("sin( pow( pow(x1,2)+pow(x2,2), 0.5 ) - t)"); Good
            //Config.FunctionEq = ("sin(t*pi*(pow(x1,2)+pow(x2,2)))/2");
            //Config.FunctionEq = ("sin(exp(x1)*t)*cos(x1)*x2*t");
            //Config.FunctionEq = ("exp(pow(x1,2) + pow(x2,2))");

            Config.ConstraintEqs.Add("x1 > -9");
            Config.ConstraintEqs.Add("x2 > -9");
            Config.ConstraintEqs.Add("x1 < 9");
            Config.ConstraintEqs.Add("x2 < 9");

            CompileProblem();

            RefreshAxisCombos();
        }

        internal void CompileProblem()
        {
            int objCount = Config.FunctionEqs.Count;

            Max = new bool[objCount];
            Objectives = new MethodResults[objCount];

            for (int i = 0; i < objCount; i++)
            {
                Max[i] = Config.FunctionEqs[i].StartsWith("Maximize:");
                Objectives[i] = CompileFunction(Config.FunctionEqs[i].Substring(10), Config.Dimensions);
            }

            Constraints = CompileConstraints(Config.ConstraintEqs, Config.Dimensions);
        }

        internal void RefreshAxisCombos()
        {
            xAxisCombo.Items.Clear();
            yAxisCombo.Items.Clear();

            for (int i = 1; i <= Config.Dimensions; i++)
            {
                xAxisCombo.Items.Add("x" + i.ToString());
                yAxisCombo.Items.Add("x" + i.ToString());
            }

            xAxisCombo.SelectedIndex = 0;
            yAxisCombo.SelectedIndex = Config.Dimensions > 0 ? 1 : 0;


            ObjCombo.Items.Clear();
            
            for(int i = 1; i <= Config.FunctionEqs.Count; i++)
                ObjCombo.Items.Add(i.ToString());

            bool visible = Config.FunctionEqs.Count > 1;

            ObjCombo.Visible = visible;
            ObjLabel.Visible = visible;

            ObjCombo.SelectedIndex = 0;
        }

        internal MethodResults CompileFunction(string code, int dims)
        {
            code = FillCode(code, dims);
            
            code = "return (float) ( " + code + ");";

            StringBuilder source = new StringBuilder();
            source.Append("public float OptFunction(float[] x, float t)");
            source.Append(Environment.NewLine);
            source.Append("{");
            source.Append(Environment.NewLine);
            source.Append(code);
            source.Append(Environment.NewLine);
            source.Append("}");

            try
            {
                return Eval.CreateVirtualMethod(
                    new CSharpCodeProvider().CreateCompiler(),
                    source.ToString(),
                    "OptFunction",
                    new CSharpLanguage(),
                    false,
                    "System.dll",
                    "System.Drawing.dll");
            }
            catch (CompilationException ce)
            {
                MessageBox.Show(this, "Compilation Errors: " + Environment.NewLine + ce.ToString());
            }

            return null;
        }

        internal string FillCode(string code, int dims)
        {
            code = code.Replace("sin", "Math.Sin");
            code = code.Replace("cos", "Math.Cos");
            code = code.Replace("tan", "Math.Tan");
            code = code.Replace("pow", "Math.Pow");
            code = code.Replace("abs", "Math.Abs");
            code = code.Replace("ln",  "Math.Log");
            code = code.Replace("exp", "Math.Exp");
            code = code.Replace("pi", "Math.PI");
            code = code.Replace("sqrt", "Math.Sqrt");

            code = code.Replace("=", "==");
            code = code.Replace("<==", "<=");
            code = code.Replace(">==", ">=");

            for (int i = dims; i >= 1; i--) // fill x11 before x1
                code = code.Replace("x" + i.ToString(), "x[" + ((int)(i-1)).ToString() + "]");

            return code;
        }

        internal MethodResults CompileConstraint(string constraint, int dims)
        {
            List<string> list = new List<string>();
            list.Add(constraint);

            return CompileConstraints(list, dims);
        }

        internal MethodResults CompileConstraints(List<string> constraints, int dims)
        {
            string code = "return ";

            foreach(string eq in constraints)
                code += "(" +  FillCode(eq, dims) + ") &&";

            // if no constraints then constraints always met (true)
            if (constraints.Count == 0)
                code += "true";
            else
                code = code.TrimEnd('&', ' ');

            code += ";";

            StringBuilder source = new StringBuilder();
            source.Append("public bool MeetsConstraint(float[] x, float t)");
            source.Append(Environment.NewLine);
            source.Append("{");
            source.Append(Environment.NewLine);
            source.Append(code);
            source.Append(Environment.NewLine);
            source.Append("}");

            try
            {
                return Eval.CreateVirtualMethod(
                    new CSharpCodeProvider().CreateCompiler(),
                 