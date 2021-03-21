
namespace Ged2Reg
{
    partial class frmRegexTester
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbContext = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.teSampleText = new System.Windows.Forms.TextBox();
            this.teRegex = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.teReplace = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pbTest = new System.Windows.Forms.Button();
            this.teResults = new System.Windows.Forms.TextBox();
            this.pbOk = new System.Windows.Forms.Button();
            this.pbCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbContext
            // 
            this.lbContext.AutoSize = true;
            this.lbContext.Location = new System.Drawing.Point(33, 44);
            this.lbContext.Name = "lbContext";
            this.lbContext.Size = new System.Drawing.Size(58, 20);
            this.lbContext.TabIndex = 0;
            this.lbContext.Text = "context";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Sample text";
            // 
            // teSampleText
            // 
            this.teSampleText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.teSampleText.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.teSampleText.Location = new System.Drawing.Point(33, 117);
            this.teSampleText.Name = "teSampleText";
            this.teSampleText.Size = new System.Drawing.Size(727, 24);
            this.teSampleText.TabIndex = 2;
            // 
            // teRegex
            // 
            this.teRegex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.teRegex.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.teRegex.Location = new System.Drawing.Point(33, 193);
            this.teRegex.Name = "teRegex";
            this.teRegex.Size = new System.Drawing.Size(727, 24);
            this.teRegex.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(33, 160);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(186, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Recognizer pattern (regex)";
            // 
            // teReplace
            // 
            this.teReplace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.teReplace.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.teReplace.Location = new System.Drawing.Point(33, 275);
            this.teReplace.Name = "teReplace";
            this.teReplace.Size = new System.Drawing.Size(727, 24);
            this.teReplace.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 242);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(148, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Replacement pattern";
            // 
            // pbTest
            // 
            this.pbTest.Location = new System.Drawing.Point(33, 327);
            this.pbTest.Name = "pbTest";
            this.pbTest.Size = new System.Drawing.Size(214, 41);
            this.pbTest.TabIndex = 7;
            this.pbTest.Text = "Test / results:";
            this.pbTest.UseVisualStyleBackColor = true;
            this.pbTest.Click += new System.EventHandler(this.pbTest_Click);
            // 
            // teResults
            // 
            this.teResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.teResults.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.teResults.Location = new System.Drawing.Point(33, 374);
            this.teResults.Multiline = true;
            this.teResults.Name = "teResults";
            this.teResults.ReadOnly = true;
            this.teResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.teResults.Size = new System.Drawing.Size(727, 230);
            this.teResults.TabIndex = 8;
            // 
            // pbOk
            // 
            this.pbOk.Location = new System.Drawing.Point(33, 624);
            this.pbOk.Name = "pbOk";
            this.pbOk.Size = new System.Drawing.Size(112, 52);
            this.pbOk.TabIndex = 9;
            this.pbOk.Text = "Ok";
            this.pbOk.UseVisualStyleBackColor = true;
            this.pbOk.Click += new System.EventHandler(this.pbOk_Click);
            // 
            // pbCancel
            // 
            this.pbCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbCancel.Location = new System.Drawing.Point(648, 624);
            this.pbCancel.Name = "pbCancel";
            this.pbCancel.Size = new System.Drawing.Size(112, 52);
            this.pbCancel.TabIndex = 10;
            this.pbCancel.Text = "Cancel";
            this.pbCancel.UseVisualStyleBackColor = true;
            this.pbCancel.Click += new System.EventHandler(this.pbCancel_Click);
            // 
            // frmRegexTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 696);
            this.Controls.Add(this.pbCancel);
            this.Controls.Add(this.pbOk);
            this.Controls.Add(this.teResults);
            this.Controls.Add(this.pbTest);
            this.Controls.Add(this.teReplace);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.teRegex);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.teSampleText);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbContext);
            this.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Name = "frmRegexTester";
            this.Text = "Ged2Reg - Regex Tester";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbContext;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox teSampleText;
        private System.Windows.Forms.TextBox teRegex;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox teReplace;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button pbTest;
        private System.Windows.Forms.TextBox teResults;
        private System.Windows.Forms.Button pbOk;
        private System.Windows.Forms.Button pbCancel;
    }
}