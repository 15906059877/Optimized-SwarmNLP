using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Evaluator;

namespace SwarmNLP
{
    public partial class InputFunction : Form
    {
        MainForm Main;
        int Dims;
        bool SetFunction;
        internal MethodResults ResultFunction;

        public InputFunction(MainForm 