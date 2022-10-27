
namespace Ged2Reg
{
    partial class frmSettingsSets
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
            this.components = new System.ComponentModel.Container();
            this.pbOk = new System.Windows.Forms.Button();
            this.pbCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.pbAdd = new System.Windows.Forms.Button();
            this.cbChooseSet = new System.Windows.Forms.ComboBox();
            this.bsSelectedSet = new System.Windows.Forms.BindingSource(this.components);
            this.teNewName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbSelectCopyFrom = new System.Windows.Forms.ComboBox();
            this.bsBasedOnSet = new System.Windows.Forms.BindingSource(this.components);
            this.label4 = new System.Windows.Forms.Label();
            this.pbDelete = new System.Windows.Forms.Button();
            this.pbRename = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.bsSelectedSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsBasedOnSet)).BeginInit();
            this.SuspendLayout();
            // 
            // pbOk
            // 
            this.pbOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pbOk.Location = new System.Drawing.Point(24, 329);
            this.pbOk.Margin = new System.Windows.Forms.Padding(4);
            this.pbOk.Name = "pbOk";
            this.pbOk.Size = new System.Drawing.Size(126, 57);
            this.pbOk.TabIndex = 0;
            this.pbOk.Text = "Ok";
            this.pbOk.UseVisualStyleBackColor = true;
            this.pbOk.Click += new System.EventHandler(this.pbOk_Click);
            // 
            // pbCancel
            // 
            this.pbCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pbCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.pbCancel.Location = new System.Drawing.Point(312, 329);
            this.pbCancel.Margin = new System.Windows.Forms.Padding(4);
            this.pbCancel.Name = "pbCancel";
            this.pbCancel.Size = new System.Drawing.Size(126, 57);
            this.pbCancel.TabIndex = 1;
            this.pbCancel.Text = "Cancel";
            this.pbCancel.UseVisualStyleBackColor = true;
            this.pbCancel.Click += new System.EventHandler(this.pbCancel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 26);
            this.label2.TabIndex = 4;
            this.label2.Text = "Selected set";
            // 
            // pbAdd
            // 
            this.pbAdd.Location = new System.Drawing.Point(26, 204);
            this.pbAdd.Name = "pbAdd";
            this.pbAdd.Size = new System.Drawing.Size(416, 42);
            this.pbAdd.TabIndex = 5;
            this.pbAdd.Text = "Add new settings set";
            this.pbAdd.UseVisualStyleBackColor = true;
            this.pbAdd.Click += new System.EventHandler(this.pbAdd_Click);
            // 
            // cbChooseSet
            // 
            this.cbChooseSet.DataSource = this.bsSelectedSet;
            this.cbChooseSet.DisplayMember = "Name";
            this.cbChooseSet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbChooseSet.FormattingEnabled = true;
            this.cbChooseSet.Location = new System.Drawing.Point(157, 30);
            this.cbChooseSet.Name = "cbChooseSet";
            this.cbChooseSet.Size = new System.Drawing.Size(283, 34);
            this.cbChooseSet.TabIndex = 6;
            this.cbChooseSet.ValueMember = "Value";
            this.cbChooseSet.SelectedIndexChanged += new System.EventHandler(this.cbChooseSet_SelectedIndexChanged);
            // 
            // bsSelectedSet
            // 
            this.bsSelectedSet.DataSource = typeof(Ged2Reg.frmSettingsSets.ListOfNamedSettingSets);
            this.bsSelectedSet.CurrentChanged += new System.EventHandler(this.listOfNamedSettingSetsBindingSource_CurrentChanged);
            // 
            // teNewName
            // 
            this.teNewName.Location = new System.Drawing.Point(157, 166);
            this.teNewName.Name = "teNewName";
            this.teNewName.Size = new System.Drawing.Size(283, 32);
            this.teNewName.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 169);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 26);
            this.label3.TabIndex = 8;
            this.label3.Text = "New Name";
            // 
            // cbSelectCopyFrom
            // 
            this.cbSelectCopyFrom.DataSource = this.bsBasedOnSet;
            this.cbSelectCopyFrom.DisplayMember = "Name";
            this.cbSelectCopyFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSelectCopyFrom.FormattingEnabled = true;
            this.cbSelectCopyFrom.Location = new System.Drawing.Point(157, 252);
            this.cbSelectCopyFrom.Name = "cbSelectCopyFrom";
            this.cbSelectCopyFrom.Size = new System.Drawing.Size(283, 34);
            this.cbSelectCopyFrom.TabIndex = 10;
            this.cbSelectCopyFrom.ValueMember = "Value";
            // 
            // bsBasedOnSet
            // 
            this.bsBasedOnSet.DataSource = typeof(Ged2Reg.frmSettingsSets.ListOfNamedSettingSets);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 260);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 26);
            this.label4.TabIndex = 9;
            this.label4.Text = "Based on";
            // 
            // pbDelete
            // 
            this.pbDelete.Location = new System.Drawing.Point(24, 70);
            this.pbDelete.Name = "pbDelete";
            this.pbDelete.Size = new System.Drawing.Size(416, 42);
            this.pbDelete.TabIndex = 11;
            this.pbDelete.Text = "Delete selected set";
            this.pbDelete.UseVisualStyleBackColor = true;
            this.pbDelete.Click += new System.EventHandler(this.pbDelete_Click);
            // 
            // pbRename
            // 
            this.pbRename.Location = new System.Drawing.Point(24, 118);
            this.pbRename.Name = "pbRename";
            this.pbRename.Size = new System.Drawing.Size(416, 42);
            this.pbRename.TabIndex = 12;
            this.pbRename.Text = "Rename selected set";
            this.pbRename.UseVisualStyleBackColor = true;
            this.pbRename.Click += new System.EventHandler(this.pbRename_Click);
            // 
            // frmSettingsSets
            // 
            this.AcceptButton = this.pbOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 26F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.pbCancel;
            this.ClientSize = new System.Drawing.Size(484, 412);
            this.ControlBox = false;
            this.Controls.Add(this.pbRename);
            this.Controls.Add(this.pbDelete);
            this.Controls.Add(this.cbSelectCopyFrom);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.teNewName);
            this.Controls.Add(this.cbChooseSet);
            this.Controls.Add(this.pbAdd);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pbCancel);
            this.Controls.Add(this.pbOk);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "frmSettingsSets";
            this.Text = "Ged2Reg - Settings Sets";
            ((System.ComponentModel.ISupportInitialize)(this.bsSelectedSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsBasedOnSet)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button pbOk;
        private System.Windows.Forms.Button pbCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button pbAdd;
        private System.Windows.Forms.ComboBox cbChooseSet;
        private System.Windows.Forms.TextBox teNewName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbSelectCopyFrom;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button pbDelete;
        private System.Windows.Forms.BindingSource bsSelectedSet;
        private System.Windows.Forms.BindingSource bsBasedOnSet;
        private System.Windows.Forms.Button pbRename;
    }
}