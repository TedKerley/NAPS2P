using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FCrop
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FCrop));
            this.imageAreaSelector = new NAPS2.WinForms.ImageAreaSelector();
            this.SuspendLayout();
            // 
            // imageAreaSelector
            // 
            resources.ApplyResources(this.imageAreaSelector, "imageAreaSelector");
            this.imageAreaSelector.ImagePreviewHelper = null;
            this.imageAreaSelector.Name = "imageAreaSelector";
            // 
            // FCrop
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.imageAreaSelector);
            this.Name = "FCrop";
            this.Load += new System.EventHandler(this.FCrop_Load);
            this.Controls.SetChildIndex(this.imageAreaSelector, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private ImageAreaSelector imageAreaSelector;
    }
}
