﻿namespace DataPerformer.UI.UserControls
{
    partial class UserControlArrayTransformer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelCenter = new System.Windows.Forms.Panel();
            this.userControlComboboxList = new Diagram.UI.UserControls.UserControlComboboxList();
            this.panelRight = new System.Windows.Forms.Panel();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.checkBoxObjectType = new System.Windows.Forms.CheckBox();
            this.Accept = new System.Windows.Forms.Button();
            this.panelCenter.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelCenter
            // 
            this.panelCenter.Controls.Add(this.userControlComboboxList);
            this.panelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCenter.Location = new System.Drawing.Point(10, 10);
            this.panelCenter.Name = "panelCenter";
            this.panelCenter.Size = new System.Drawing.Size(240, 62);
            this.panelCenter.TabIndex = 15;
            // 
            // userControlComboboxList
            // 
            this.userControlComboboxList.Count = 1;
            this.userControlComboboxList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userControlComboboxList.LabelFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.userControlComboboxList.Location = new System.Drawing.Point(0, 0);
            this.userControlComboboxList.Name = "userControlComboboxList";
            this.userControlComboboxList.Size = new System.Drawing.Size(240, 62);
            this.userControlComboboxList.TabIndex = 0;
            this.userControlComboboxList.Texts = new string[] {
        "Measurement"};
            // 
            // panelRight
            // 
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelRight.Location = new System.Drawing.Point(250, 10);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(10, 62);
            this.panelRight.TabIndex = 13;
            // 
            // panelLeft
            // 
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 10);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(10, 62);
            this.panelLeft.TabIndex = 12;
            // 
            // panelTop
            // 
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(260, 10);
            this.panelTop.TabIndex = 11;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.Accept);
            this.panelBottom.Controls.Add(this.checkBoxObjectType);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 72);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(260, 45);
            this.panelBottom.TabIndex = 14;
            // 
            // checkBoxObjectType
            // 
            this.checkBoxObjectType.AutoSize = true;
            this.checkBoxObjectType.Location = new System.Drawing.Point(7, 16);
            this.checkBoxObjectType.Name = "checkBoxObjectType";
            this.checkBoxObjectType.Size = new System.Drawing.Size(80, 17);
            this.checkBoxObjectType.TabIndex = 0;
            this.checkBoxObjectType.Text = "Object type";
            this.checkBoxObjectType.UseVisualStyleBackColor = true;
            // 
            // Accept
            // 
            this.Accept.Location = new System.Drawing.Point(113, 10);
            this.Accept.Name = "Accept";
            this.Accept.Size = new System.Drawing.Size(75, 23);
            this.Accept.TabIndex = 1;
            this.Accept.Text = "Accept";
            this.Accept.UseVisualStyleBackColor = true;
            this.Accept.Click += new System.EventHandler(this.Accept_Click);
            // 
            // UserControlArrayTransformer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelCenter);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelBottom);
            this.Name = "UserControlArrayTransformer";
            this.Size = new System.Drawing.Size(260, 117);
            this.panelCenter.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelCenter;
        private Diagram.UI.UserControls.UserControlComboboxList userControlComboboxList;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.CheckBox checkBoxObjectType;
        private System.Windows.Forms.Button Accept;
    }
}
