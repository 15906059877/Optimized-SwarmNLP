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
                    source.ToString(),
                    "MeetsConstraint",
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


        private void MainForm_Load(object sender, EventArgs e)
        {
            RedrawThread = new Thread(GenerateBackground);
            RedrawThread.Start();

            SwarmThread = new Thread(RunSwarm);
            SwarmThread.Start();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Shutdown = true;
            
            ProcessEvent.Set();
            RefreshEvent.Set();

            SwarmThread.Join();
            RedrawThread.Join();
        }

        private void MainDisplay_Paint(object sender, PaintEventArgs e)
        {
            ProcessEvent.Set();
        }

        /// <summary>
        /// An intense function because it runs the objective function for every pixel
        /// That's why it's not done often, and in a different thread
        /// </summary>
        void GenerateBackground()
        {
            float[,] heightMap;
            bool[,] failMap;

            float result = 0;
            float highest = float.MinValue;
            float lowest = float.MaxValue;
            float range = 1;
            float[] graphPos = new float[Config.Dimensions];
            float drawTime = Time;

            float[] coords = new float[Config.Dimensions];
            float[] maxs = new float[Config.Dimensions];
            float[] mins = new float[Config.Dimensions];

            while (!Shutdown)
            {
                RefreshEvent.WaitOne();

                if (FreezeRefresh)
                    continue;

                int width = MainDisplay.Width / Config.Scaling;
                int height = MainDisplay.Height / Config.Scaling;

                if (width <= 0 || height <= 0)
                    continue;

                Bitmap bg = new Bitmap(width, height);

                ImageWidth = width;
                ImageHeight = height;

                heightMap = new float[width, height];
                failMap = new bool[width, height];

                highest = float.MinValue;
                lowest = float.MaxValue;

                // keep these vars constant so drawing is consistant while other thread keeps playing

                drawTime = Time;

                if (Config.Dimensions != coords.Length)
                {
                    coords = new float[Config.Dimensions];
                    mins = new float[Config.Dimensions];
                    maxs = new float[Config.Dimensions];
                }

                BestGlobalCoord.CopyTo(coords, 0);
                Config.winMax.CopyTo(maxs, 0);
                Config.winMin.CopyTo(mins, 0);

                // get height of points
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        // get height
                        graphPos = BitmapToGraph(x, y, coords, mins, maxs);
                        result = (float)Objectives[Obj].Invoke(graphPos, drawTime);

                        // normalize so gradient data isnt drowned out by extreme ranges
                        if(result < BestGlobalValue[Obj] - 1000)
                            result = BestGlobalValue[Obj] - 1000;

                        if (result > BestGlobalValue[Obj] + 1000)
                            result = BestGlobalValue[Obj] + 1000;

                        // set highest / lowest
                        if (result < lowest)
                                lowest = result;

                        if (result > highest)
                            highest = result;

                        heightMap[x, y] = result;

                        // find if pixel boundary crosses the constraint
                        failMap[x, y] = !((bool)Constraints.Invoke(graphPos, drawTime));
                    }

                range = highest - lowest;
                if (range == 0)
                    range = 1;


                // much faster to update bitmap like this
                BitmapData bgData = bg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bg.PixelFormat);

                unsafe
                {
                    int* pData = (int*)bgData.Scan0.ToPointer();

                    int pos = 0;
                    int redness = 0;
                    int greyness = 0;
                    int intensity = 0;
                    int hue = 0;
                    Color rainbow;

                    int boundaryColor = ColorTranslator.ToOle(Color.Blue);

                    // draw points
                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                        {
                            pos = y * bgData.Stride / 4 + x; // stride is always in bytes, y is adjusted for 32 bit

                            //hue = (int) (150 - (150 * ((HeightMap[x, y] - lowest) / range)));
                            //rainbow = HLStoRGB(hue, 120, 240);

                            intensity = failMap[x, y] ? 127 : 255;

                            greyness = (byte)(intensity * ((heightMap[x, y] - lowest) / range));

                            redness = greyness;

                            if (failMap[x, y])
                            {
                                greyness = 96 + greyness; // lower bound for failed point is 96, max 224
                                redness = greyness + 32; // red can go from 128 to 256
                            }


                            //pData[pos] = ColorTranslator.ToOle(Color.FromArgb(255, color, color, color));
                            pData[pos] = (255 << 24) | (redness << 16) | (greyness << 8) | greyness; // ARGB
                        }
                }

                bg.UnlockBits(bgData);

                LatestBG = bg;
                ProcessEvent.Set(); // make sure its drawn
            }
        }

        void RunSwarm()
        {
            Bitmap Background = new Bitmap(10, 10);

            
            IntPtr mainDC;
            IntPtr memDC;
            IntPtr tempDC;
            IntPtr OffscreenBmp;
            IntPtr oldBmp;
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr prevBmp;

            IntPtr bugPen = Win32.CreatePen(Win32.PenStyles.PS_SOLID, 1, ColorBug);
            IntPtr bugBrush = Win32.CreateSolidBrush(ColorBug);;

            IntPtr optPen = Win32.CreatePen(Win32.PenStyles.PS_SOLID, 1, ColorOpt);
            IntPtr optBrush = Win32.CreateSolidBrush(ColorOpt);

            IntPtr oldPen;
            IntPtr oldBrush;

            int attempts = 0;
            int cycleTime = 0;
            Point displayPos = new Point();
            float[] bugValue = new float[1];


            while (!Shutdown)
            {
                ProcessEvent.WaitOne();

                // draw background to offscreen
                if (Background.Width != MainDisplay.Width / Config.Scaling || Background.Height != MainDisplay.Height / Config.Scaling)
                    RefreshEvent.Set();

                if (Reset)
                {
                    Reset = false;
                    Time = 0;

                    BestGlobalValue = new float[Objectives.Length];
                    bugValue = new float[Objectives.Length];

                    for(int i = 0; i < Objectives.Length; i++)
                        BestGlobalValue[i] = Max[i] ? float.MinValue : float.MaxValue;

                    BestGlobalCoord = new float[Config.Dimensions];

                    NonDominatingCoords.Clear();
                    NonDominatingValues.Clear();
                }

                if (SetupSwarm)
                {
                    SetupSwarm = false;

                    Bugs.Clear();

                    if(TryGetFeasible( BestGlobalCoord))
                        for(int i = 0; i < BestGlobalValue.Length; i++)
                            BestGlobalValue[i] = (float) Objectives[i].Invoke(BestGlobalCoord, Time);

                    RefreshEvent.Set();  // ensures that when new setup loaded, it is viewed in right slice
                }

                if (LatestBG != null)
                {
                    Background = LatestBG;
                    LatestBG = null;

                    if (hBitmap != IntPtr.Zero)
                    {
                        Win32.DeleteObject(hBitmap);
                        hBitmap = IntPtr.Zero;
                    }

                    hBitmap = Background.GetHbitmap();
                    
                }

                // start drawing
                Graphics mainGraphics = MainDisplay.CreateGraphics();
                mainDC = mainGraphics.GetHdc();

                // create memeory DC and select an offscreen bmp into it
                memDC = Win32.CreateCompatibleDC(mainDC);
                OffscreenBmp = Win32.CreateCompatibleBitmap(mainDC, MainDisplay.Width, MainDisplay.Height);
                oldBmp = Win32.SelectObject(memDC, OffscreenBmp);
                

                tempDC = Win32.CreateCompatibleDC(mainDC);       
                prevBmp = Win32.SelectObject(tempDC, hBitmap);
                Win32.StretchBlt(memDC, 0, 0, MainDisplay.Width, MainDisplay.Height, tempDC, 0, 0, Background.Width, Background.Height, (int)Win32.SRCCOPY);
                Win32.SelectObject(tempDC, prevBmp); 
                Win32.DeleteDC(tempDC);


                // draw the swarm
                oldPen = Win32.SelectObject(memDC, bugPen);
                oldBrush = Win32.SelectObject(memDC, bugBrush);


                // add/remove bugs
                while (Bugs.Count > Config.Entities)
                    Bugs.RemoveAt(0);

                if (Config.Entities > Bugs.Count)
                {
                    int add = Bugs.Count; // AddBug can fail so we only interate a set amount
                    for (int i = 0; i < Config.Entities - add; i++)
                        AddBug();
                }


                // use cycle time so that we can 'see' particles moving towards new destination, otherwise it looks like random jumps
                foreach (Particle bug in Bugs)
                {
                    if (Play || Step)
                    {
                        if (cycleTime == 0)
                        {
                            // get velocity   THIS IS THE MEAT
                            if (Config.Replusion)
                            {
                                Particle randBug = Bugs[RndGen.Next(0, Bugs.Count - 1)];

                                for (int i = 0; i < Config.Dimensions; i++)
                                    bug.TestV[i] = Config.Inertia * bug.Velocity[i] +
                                                    Config.PersonalPref * (float)RndGen.NextDouble() * (bug.Best[i] - bug.Postion[i]) +
                                                    -1 * Config.GlobalPref * (float)RndGen.NextDouble() * /* Config.Inertia */ (randBug.Best[i] - bug.Postion[i]) +
                                                    2 * (float)RndGen.NextDouble() * /* Config.Inertia */ (float)RndGen.NextDouble();

                            }
                            else
                            {
                                // choose a random best global to go towards
                                var bestGlobal = BestGlobalCoord;
                                if(NonDominatingCoords.Count > 0)
                                    bestGlobal = NonDominatingCoords[RndGen.Next(NonDominatingCoords.Count)];

                                for(int i = 0; i < Config.Dimensions; i++)
                                    bug.TestV[i] = Config.Inertia * bug.Velocity[i] +
                                                    Config.PersonalPref * (float)RndGen.NextDouble() * (bug.Best[i] - bug.Postion[i]) +
                                                    Config.GlobalPref * (float)RndGen.NextDouble() *  (bestGlobal[i] - bug.Postion[i]);

                            }

                            bug.MoveNext = false;
                            
                            attempts = 20;
                            while (attempts > 0)
                                // if new position feasible switch set velocity to the test
                                if (IsFeasible(bug.Postion, bug.TestV))
                                {
                                    float[] velocity = bug.TestV;
                                    bug.TestV = bug.Velocity;
                                    bug.Velocity = velocity;

                                    bug.MoveNext = true;

                                    break;
                                }
                                // while outside bounds, halve velocity and recalc
                                else
                                {
                                    for (int i = 0; i < Config.Dimensions; i++)
                                        bug.TestV[i] = bug.TestV[i] / 2;

                                    attempts--;
                                }
                        }

                        // set next position
                        if(bug.MoveNext)
                            for (int i = 0; i < Config.Dimensions; i++)
                                bug.Postion[i] = bug.Postion[i] + bug.Velocity[i] / Config.FlyTime;


                        // let the particles fly a bit, take their values every so often
                        if (cycleTime == 0)
                        {
                            // check for personal / global best 
                            for(int i = 0; i < Objectives.Length; i++)
                                bugValue[i] = (float)Objectives[i].Invoke(bug.Postion, Time);

                            // if time used in objectives, then values at coords need to be re-evaluated
                            if (Config.TimeUsed)
                            {
                                for (int i = 0; i < Objectives.Length; i++)
                                    BestGlobalValue[i] = (float)Objectives[i].Invoke(BestGlobalCoord, Time);

                                for (int i = 0; i < NonDominatingCoords.Count; i++)
                                    for (int x = 0; x < Objectives.Length; x++)
                                        NonDominatingValues[i][x] = (float)Objectives[x].Invoke(NonDominatingCoords[i], Time);
                            }

                            // if the position is better or non-dominates it's best
                            if( NonDominatesPoint(bugValue, bug.BestValue))
                            {
                                bugValue.CopyTo(bug.BestValue, 0);

                                bug.Postion.CopyTo(bug.Best, 0);
                            }

                            // if the bug position is not dominated by prev solutions
                            if ( NonDominatesSet(bug.Postion, bugValue))
                            {
                                bug.Postion.CopyTo( BestGlobalCoord, 0);

                                // redraw if our view plane changed because a gobal optimal changed
                                if (Config.Dimensions > 2)
                                    RefreshEvent.Set();

                                bugValue.CopyTo(BestGlobalValue, 0);

                                // record points in non-dominating set
                                var bestCoord = new float[BestGlobalCoord.Length];
                                var bestValue = new float[bugValue.Length];

                                BestGlobalCoord.CopyTo(bestCoord, 0);
                                bugValue.CopyTo(bestValue, 0);

                                NonDominatingCoords.Add(bestCoord);
                                NonDominatingValues.Add(bestValue);
                                if (NonDominatingCoords.Count > SolutionsKept)
                                {
                                    NonDominatingCoords.RemoveAt(0);
                                    NonDominatingValues.RemoveAt(0);
                                }

                                BeginInvoke(new Voidhandler(StatusBarUpdate));
                            }
                        }
                    }

                    // draw
                    displayPos = GraphtoWindow(bug.Postion);
                    Win32.Ellipse(memDC, displayPos.X - BugSize, displayPos.Y - BugSize, displayPos.X + BugSize, displayPos.Y + BugSize);
                }

                Win32.SelectObject(memDC, oldPen);
                Win32.SelectObject(memDC, oldBrush);


                // draw global optimum
                oldPen = Win32.SelectObject(memDC, optPen);
                oldBrush = Win32.SelectObject(memDC, optBrush);

                displayPos = GraphtoWindow(BestGlobalCoord);
                Win32.Ellipse(memDC, displayPos.X - BugSize, displayPos.Y - BugSize, displayPos.X + BugSize, displayPos.Y + BugSize);

                foreach (var coord in NonDominatingCoords)
                {
                    displayPos = GraphtoWindow(coord); 
                    Win32.Ellipse(memDC, displayPos.X - 1, displayPos.Y - 1, displayPos.X + 1, displayPos.Y + 1);
                }

                Win32.SelectObject(memDC, oldPen);
                Win32.SelectObject(memDC, oldBrush);

                // copy to main screen
                Win32.BitBlt(mainDC, 0, 0, MainDisplay.Width, MainDisplay.Height, memDC, 0, 0, Win32.TernaryRasterOperations.SRCCOPY);


                // release objects
                Win32.SelectObject(memDC, oldBmp);
                Win32.DeleteObject(OffscreenBmp);

                Win32.DeleteDC(memDC);
                mainGraphics.ReleaseHdc(mainDC);


                // finish
                if (Play || Step)
                {
                    cycleTime++;

                    if (cycleTime >= Config.FlyTime)
                        cycleTime = 0;

                    if (Config.TimeInc != 0)
                    {
                        Time += Config.TimeInc;
                        RefreshEvent.Set();

                        for(int i = 0; i < BestGlobalValue.Length; i++)
                        BestGlobalValue[i] = (float)Objectives[i].Invoke(BestGlobalCoord, Time); // value of best position has changed under the new time slice
                    }
                }

                Step = false;

                //Thread.Sleep(20);

                if (Play)
                    ProcessEvent.Set();
            }

            if (hBitmap != IntPtr.Zero)
            {
                Win32.DeleteObject(hBitmap);
                hBitmap = IntPtr.Zero;
            }
        }

        bool NonDominatesPoint(float[] test, float[] best)
        {
            // if coordinate is better than 'best' in at least 1 objective function then it is non-dominating
            for (int i = 0; i < best.Length; i++)
                if ((Max[i] && test[i] > best[i]) || (!Max[i] && test[i] < best[i]))
                    return true;

            return false;
        }

        bool NonDominatesSet(float[] testCoord, float[] testValue)
        {
            bool dominating = false;
            bool dominated = false;
            bool equal = true;

            for(int x = 0; x < NonDominatingValues.Count; x++)
            {
                var best = NonDominatingValues[x];

                dominating = false;
                dominated = false;
                equal = true;

                for (int i = 0; i < best.Length; i++)
                    if ((Max[i] && testValue[i] >= best[i]) || (!Max[i] && testValue[i] <= best[i]))
                    {
                        dominating = true;

                        if (testValue[i] != best[i])
                            equal = false;
                    }
                    else
                        dominated = true;

                // if totally dominated, ignore
                if (dominated && !dominating)
                    return false;

                // if values are equal then return if coords are equal as well
                if (equal)
                {
                    var coord = NonDominatingCoords[x];
                    for(int i = 0; i < coord.Length; i++)
                        if (testCoord[i] != coord[i