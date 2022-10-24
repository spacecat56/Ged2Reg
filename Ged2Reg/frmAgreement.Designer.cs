
namespace Ged2Reg
{
    partial class frmAgreement
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
            this.pbAgree = new System.Windows.Forms.Button();
            this.pbCancel = new System.Windows.Forms.Button();
            this.teAgreement = new System.Windows.Forms.TextBox();
            this.lbUser = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // pbAgree
            // 
            this.pbAgree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pbAgree.Location = new System.Drawing.Point(22, 610);
            this.pbAgree.Name = "pbAgree";
            this.pbAgree.Size = new System.Drawing.Size(234, 55);
            this.pbAgree.TabIndex = 0;
            this.pbAgree.Text = "I AGREE";
            this.pbAgree.UseVisualStyleBackColor = true;
            this.pbAgree.Click += new System.EventHandler(this.pbAgree_Click);
            // 
            // pbCancel
            // 
            this.pbCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pbCancel.Location = new System.Drawing.Point(426, 608);
            this.pbCancel.Name = "pbCancel";
            this.pbCancel.Size = new System.Drawing.Size(249, 57);
            this.pbCancel.TabIndex = 1;
            this.pbCancel.Text = "Cancel";
            this.pbCancel.UseVisualStyleBackColor = true;
            this.pbCancel.Click += new System.EventHandler(this.pbCancel_Click);
            // 
            // teAgreement
            // 
            this.teAgreement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.teAgreement.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.teAgreement.Location = new System.Drawing.Point(18, 68);
            this.teAgreement.Multiline = true;
            this.teAgreement.Name = "teAgreement";
            this.teAgreement.ReadOnly = true;
            this.teAgreement.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.teAgreement.Size = new System.Drawing.Size(657, 512);
            this.teAgreement.TabIndex = 2;
            // 
            // lbUser
            // 
            this.lbUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbUser.AutoSize = true;
            this.lbUser.Location = new System.Drawing.Point(286, 622);
            this.lbUser.Name = "lbUser";
            this.lbUser.Size = new System.Drawing.Size(71, 30);
            this.lbUser.TabIndex = 3;
            this.lbUser.Text = "label1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(22, 9);
            this.label1.MaximumSize = new System.Drawing.Size(640, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(552, 56);
            this.label1.TabIndex = 4;
            this.label1.Text = "You may use this Software only if you agree to the license and acknowledge the no" +
    "tice.";
            // 
            // frmAgreement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 690);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbUser);
            this.Controls.Add(this.teAgreement);
            this.Controls.Add(this.pbCancel);
            this.Controls.Add(this.pbAgree);
            this.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmAgreement";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Ged2Reg - User Agreement";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button pbAgree;
        private System.Windows.Forms.Button pbCancel;
        private System.Windows.Forms.TextBox teAgreement;
        private System.Windows.Forms.Label lbUser;
        private System.Windows.Forms.Label label1;
    }
}