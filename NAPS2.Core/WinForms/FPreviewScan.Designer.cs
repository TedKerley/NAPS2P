using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FPreviewScan
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FPreviewScan));
            this.btnDone = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnPreviewPrevious = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.imageAreaSelector = new NAPS2.WinForms.ImageAreaSelector();
            this.chkUseScanner = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnDone
            // 
            this.btnDone.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnDone, "btnDone");
            this.btnDone.Name = "btnDone";
            this.btnDone.UseVisualStyleBackColor = true;
            this.btnDone.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnScan
            // 
            this.btnScan.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.btnScan, "btnScan");
            this.btnScan.Image = global::NAPS2.Icons.control_play_blue;
            this.btnScan.Name = "btnScan";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.btnPreview, "btnPreview");
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // btnPreviewPrevious
            // 
            this.btnPreviewPrevious.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.btnPreviewPrevious, "btnPreviewPrevious");
            this.btnPreviewPrevious.Name = "btnPreviewPrevious";
            this.btnPreviewPrevious.UseVisualStyleBackColor = true;
            this.btnPreviewPrevious.Click += new System.EventHandler(this.btnPreviewPrevious_Click);
            // 
            // btnReset
            // 
            resources.ApplyResources(this.btnReset, "btnReset");
            this.btnReset.Name = "btnReset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // imageAreaSelector
            // 
            resources.ApplyResources(this.imageAreaSelector, "imageAreaSelector");
            this.imageAreaSelector.Name = "imageAreaSelector";
            // 
            // useScannerCheckbox
            // 
            resources.ApplyResources(this.chkUseScanner, "chkUseScanner");
            this.chkUseScanner.Checked = true;
            this.chkUseScanner.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseScanner.Name = "chkUseScanner";
            this.chkUseScanner.UseVisualStyleBackColor = true;
            // 
            // FPreviewScan
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnDone;
            this.Controls.Add(this.chkUseScanner);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnPreviewPrevious);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.btnDone);
            this.Controls.Add(this.imageAreaSelector);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FPreviewScan";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnDone;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnPreviewPrevious;
        private System.Windows.Forms.Button btnReset;
        private ImageAreaSelector imageAreaSelector;
        private System.Windows.Forms.CheckBox chkUseScanner;
    }
}
