namespace NAPS2.WinForms
{
    partial class ImageAreaSelector
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbRight = new System.Windows.Forms.TrackBar();
            this.tbBottom = new System.Windows.Forms.TrackBar();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.tbLeft = new System.Windows.Forms.TrackBar();
            this.tbTop = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.tbRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTop)).BeginInit();
            this.SuspendLayout();
            // 
            // tbRight
            // 
            this.tbRight.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbRight.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.tbRight.Location = new System.Drawing.Point(27, 476);
            this.tbRight.Name = "tbRight";
            this.tbRight.Size = new System.Drawing.Size(417, 45);
            this.tbRight.TabIndex = 10;
            this.tbRight.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbRight.Scroll += new System.EventHandler(this.tbBottom_Scroll);
            // 
            // tbBottom
            // 
            this.tbBottom.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbBottom.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.tbBottom.Location = new System.Drawing.Point(439, 42);
            this.tbBottom.Name = "tbBottom";
            this.tbBottom.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbBottom.Size = new System.Drawing.Size(45, 428);
            this.tbBottom.TabIndex = 9;
            this.tbBottom.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbBottom.Scroll += new System.EventHandler(this.tbRight_Scroll);
            // 
            // pictureBox
            // 
            this.pictureBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox.Location = new System.Drawing.Point(38, 54);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(395, 406);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox.TabIndex = 6;
            this.pictureBox.TabStop = false;
            this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseDown);
            this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
            // 
            // tbLeft
            // 
            this.tbLeft.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbLeft.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.tbLeft.Location = new System.Drawing.Point(27, 18);
            this.tbLeft.Name = "tbLeft";
            this.tbLeft.Size = new System.Drawing.Size(417, 45);
            this.tbLeft.TabIndex = 7;
            this.tbLeft.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbLeft.Scroll += new System.EventHandler(this.tbTop_Scroll);
            // 
            // tbTop
            // 
            this.tbTop.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbTop.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.tbTop.Location = new System.Drawing.Point(14, 42);
            this.tbTop.Name = "tbTop";
            this.tbTop.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbTop.Size = new System.Drawing.Size(45, 428);
            this.tbTop.TabIndex = 8;
            this.tbTop.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbTop.Scroll += new System.EventHandler(this.tbLeft_Scroll);
            // 
            // ImageAreaSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbRight);
            this.Controls.Add(this.tbBottom);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.tbTop);
            this.Controls.Add(this.tbLeft);
            this.Name = "ImageAreaSelector";
            this.Size = new System.Drawing.Size(470, 515);
            ((System.ComponentModel.ISupportInitialize)(this.tbRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTop)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar tbRight;
        private System.Windows.Forms.TrackBar tbBottom;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TrackBar tbLeft;
        private System.Windows.Forms.TrackBar tbTop;
    }
}
