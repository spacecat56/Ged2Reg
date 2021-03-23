using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Ged2Reg
{
    public partial class frmRegexTester : Form
    {
        public frmRegexTester()
        {
            InitializeComponent();
        }


        public string TheRegex
        {
            get => teRegex.Text;
            set => teRegex.Text = value;
        }

        public string TheReplacer
        {
            get => teReplace.Text;
            set => teReplace.Text = value;
        }

        public string TheSample => teSampleText.Text;

        public string TheContext
        {
            get => lbContext.Text;
            set => lbContext.Text = value;
        }

        private void pbTest_Click(object sender, EventArgs e)
        {
            try
            {
                teResults.Text = string.Empty;
                StringBuilder sb = new StringBuilder();
                try
                {
                    sb.AppendLine($"Construct regex with pattern [{TheRegex}]");
                    Regex rex = new Regex(TheRegex);
                    sb.AppendLine($"Matching text [{TheSample}]...");
                    Match m = rex.Match(TheSample);
                    sb.AppendLine($"...{m.Success}");
                    if (m.Success)
                    {
                        sb.AppendLine("Groups in match:");
                        sb.AppendLine($"...Entire match, '{m.Groups[0].Value}'");
                        foreach (Group g in m.Groups)
                        {
                            int.TryParse(g.Name, out int numId);
                            if (g.Name == numId.ToString())
                                continue;
                            string val = g.Success && g.Captures.Count > 0
                                ? g.Captures[0].Value
                                : null;
                            sb.AppendLine($"...{g.Name}, '{val}'");
                        }
                        sb.AppendLine("Apply replacement to example:");
                        string result = m.Result(TheReplacer); // NO! does NOT show what the main pgm does: rex.Replace(TheSample, TheReplacer);
                        sb.AppendLine($"'{result}'");
                    }
                }
                catch (Exception exRex)
                {
                    sb.AppendLine($"Test failed with exception: {exRex.Message}");
                }

                teResults.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Exception: {ex.Message}", "Ged2Reg - Exception", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pbOk_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Exception: {ex.Message}");
            }
        }

        private void pbCancel_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Exception: {ex.Message}");
            }
        }
    }
}
