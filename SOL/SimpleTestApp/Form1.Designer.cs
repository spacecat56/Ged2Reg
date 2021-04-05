
namespace SimpleTestApp
{
    partial class Form1
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
            this.pbLoad = new System.Windows.Forms.Button();
            this.pbSave = new System.Windows.Forms.Button();
            this.pbEdit = new System.Windows.Forms.Button();
            this.teWorkingDir = new System.Windows.Forms.TextBox();
            this.pbNew = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pbLoad
            // 
            this.pbLoad.Location = new System.Drawing.Point(74, 114);
            this.pbLoad.Name = "pbLoad";
            this.pbLoad.Size = new System.Drawing.Size(105, 58);
            this.pbLoad.TabIndex = 0;
            this.pbLoad.Text = "Load";
            this.pbLoad.UseVisualStyleBackColor = true;
            this.pbLoad.Click += new System.EventHandler(this.pbLoad_Click);
            // 
            // pbSave
            // 
            this.pbSave.Location = new System.Drawing.Point(74, 455);
            this.pbSave.Name = "pbSave";
            this.pbSave.Size = new System.Drawing.Size(105, 58);
            this.pbSave.TabIndex = 1;
            this.pbSave.Text = "Save";
            this.pbSave.UseVisualStyleBackColor = true;
            this.pbSave.Click += new System.EventHandler(this.pbSave_Click);
            // 
            // pbEdit
            // 
            this.pbEdit.Location = new System.Drawing.Point(74, 357);
            this.pbEdit.Name = "pbEdit";
            this.pbEdit.Size = new System.Drawing.Size(105, 58);
            this.pbEdit.TabIndex = 2;
            this.pbEdit.Text = "Edit";
            this.pbEdit.UseVisualStyleBackColor = true;
            this.pbEdit.Click += new System.EventHandler(this.pbEdit_Click);
            // 
            // teWorkingDir
            // 
            this.teWorkingDir.Location = new System.Drawing.Point(74, 35);
            this.teWorkingDir.Name = "teWorkingDir";
            this.teWorkingDir.Size = new System.Drawing.Size(332, 32);
            this.teWorkingDir.TabIndex = 3;
            this.teWorkingDir.Text = "D:\\projects\\public\\_data\\doc";
            // 
            // pbNew
            // 
            this.pbNew.Location = new System.Drawing.Point(74, 254);
            this.pbNew.Name = "pbNew";
            this.pbNew.Size = new System.Drawing.Size(105, 58);
            this.pbNew.TabIndex = 4;
            this.pbNew.Text = "New";
            this.pbNew.UseVisualStyleBackColor = true;
            this.pbNew.Click += new System.EventHandler(this.pbNew_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 26F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(845, 672);
            this.Controls.Add(this.pbNew);
            this.Controls.Add(this.teWorkingDir);
            this.Controls.Add(this.pbEdit);
            this.Controls.Add(this.pbSave);
            this.Controls.Add(this.pbLoad);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button pbLoad;
        private System.Windows.Forms.Button pbSave;
        private System.Windows.Forms.Button pbEdit;
        private System.Windows.Forms.TextBox teWorkingDir;
        private System.Windows.Forms.Button pbNew;
    }
}

