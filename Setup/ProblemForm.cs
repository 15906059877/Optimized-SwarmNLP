
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Evaluator;

namespace SwarmNLP
{
    public partial class ProblemForm : Form
    {
        MainForm Main; 

        public ProblemForm(MainForm main)
        {
            InitializeComponent();

            Main = main;
        }

        private void ProblemForm_Load(object sender, EventArgs e)
        {
            ResetView();
           
        }

        internal void ResetView()
        {
            DimensionBox.Value = Main.Config.Dimensions;

            FunctionList.Items.Clear();
            foreach (string function in Main.Config.FunctionEqs)
                FunctionList.Items.Add(function);

            ConstraintsList.Items.Clear();
            foreach (string constraint in Main.Config.ConstraintEqs)
                ConstraintsList.Items.Add(constraint);

            TimeCheckBox.Checked = Main.Config.TimeUsed;
            TimeBox.Text = Main.Config.TimeInc.ToString();
        }


        private void ProblemForm_FormClosing(object sender, FormClosingEventArgs e)
        {