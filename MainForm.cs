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
        internal AutoResetEvent Pro