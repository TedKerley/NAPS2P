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

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FPreviewScan));
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.tbRight = new System.Windows.Forms.TrackBar();
            this.tbLeft = new System.Windows.Forms.TrackBar();
            this.tbTop = new System.Windows.Forms.TrackBar();
            this.tbBottom = new System.Windows.Forms.TrackBar();
            this.btnDone = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnPreviewPrevious = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBottom)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Cursor = System.Windows.Forms.Cursors.Cross;
            resources.ApplyResources(this.pictureBox, "pictureBox");
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.TabStop = false;
            this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseDown);
            this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
            // 
            // tbRight
            // 
            resources.ApplyResources(this.tbRight, "tbRight");
            this.tbRight.Name = "tbRight";
            this.tbRight.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbRight.Scroll += new System.EventHandler(this.tbRight_Scroll);
            // 
            // tbLeft
            // 
            resources.ApplyResources(this.tbLeft, "tbLeft");
            this.tbLeft.Name = "tbLeft";
            this.tbLeft.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbLeft.Scroll += new System.EventHandler(this.tbLeft_Scroll);
            // 
            // tbTop
            // 
            resources.ApplyResources(this.tbTop, "tbTop");
            this.tbTop.Name = "tbTop";
            this.tbTop.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbTop.Scroll += new System.EventHandler(this.tbTop_Scroll);
            // 
            // tbBottom
            // 
            resources.ApplyResources(this.tbBottom, "tbBottom");
            this.tbBottom.Name = "tbBottom";
            this.tbBottom.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbBottom.Scroll += new System.EventHandler(this.tbBottom_Scroll);
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
            // FPreviewScan
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnDone;
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnPreviewPrevious);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.btnDone);
            this.Controls.Add(this.tbRight);
            this.Controls.Add(this.tbBottom);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.tbLeft);
            this.Controls.Add(this.tbTop);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FPreviewScan";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FPreviewScan_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBottom)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TrackBar tbRight;
        private System.Windows.Forms.TrackBar tbLeft;
        private System.Windows.Forms.TrackBar tbTop;
        private System.Windows.Forms.TrackBar tbBottom;
        private System.Windows.Forms.Button btnDone;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnPreviewPrevious;
        private System.Windows.Forms.Button btnReset;
    }
}
