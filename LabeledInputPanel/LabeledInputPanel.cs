using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LabeledInputPanel
{
    //[Designer(typeof(ExpanderControlDesigner))]
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public partial class LabeledInputPanel: UserControl
    {
        public LabeledInputPanel()
        {
            InitializeComponent();
        }

        #region Overrides of Control

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control.Name == "label1") return;
            if (e.Control.Name == "panel1") return;
            e.Control.Parent = panel1;
            e.Control.Dock = DockStyle.Right;
        }

        public string LabelText
        {
            get => label1.Text;
            set => label1.Text = value;
        }

        #endregion
    }
}
