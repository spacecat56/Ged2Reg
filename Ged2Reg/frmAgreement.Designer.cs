
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
            this.SuspendLayout();
            // 
            // pbAgree
            // 
            this.pbAgree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pbAgree.Location = new System.Drawing.Point(49, 578);
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
            this.pbCancel.Location = new System.Drawing.Point(418, 576);
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
            this.teAgreement.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.teAgreement.Location = new System.Drawing.Point(45, 36);
            this.teAgreement.Multiline = true;
            this.teAgreement.Name = "teAgreement";
            this.teAgreement.ReadOnly = true;
            this.teAgreement.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.teAgreement.Size = new System.Drawing.Size(616, 511);
            this.teAgreement.TabIndex = 2;
            // 
            // lbUser
            // 
            this.lbUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbUser.AutoSize = true;
            this.lbUser.Location = new System.Drawing.Point(313, 590);
            this.lbUser.Name = "lbUser";
            this.lbUser.Size = new System.Drawing.Size(71, 30);
            this.lbUser.TabIndex = 3;
            this.lbUser.Text = "label1";
            // 
            // frmAgreement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(716, 690);
            this.ControlBox = false;
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
    }
}