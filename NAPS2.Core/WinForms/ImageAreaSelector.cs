// <copyright file="ImageAreaSelector.cs" company="High Quality Solutions">
//     Copyright (c)  High Quality Solutions Limited. All rights reserved.
// </copyright>

namespace NAPS2.WinForms
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Windows.Forms;

    using NAPS2.Scan;
    using NAPS2.Scan.Wia;

    using Timer = System.Threading.Timer;

    public partial class ImageAreaSelector : UserControl
    {
        private Point dragStartCoords;

        private LayoutManager layoutManager;

        private bool previewOutOfDate;

        private Timer previewTimer;

        private bool working;

        private Bitmap workingImage;

        private Bitmap workingImage2;

        public ImageAreaSelector()
        {

            this.InitializeComponent();

            this.layoutManager = new LayoutManager(this)
                .Bind(this.pictureBox).LeftToForm().RightToForm().TopToForm().BottomToForm()
                .Bind(this.tbLeft, this.tbRight).WidthTo(() => (int)(this.GetImageWidthRatio() * this.pictureBox.Width)).LeftTo(() => (int)((1 - this.GetImageWidthRatio()) * this.pictureBox.Width / 2))
                .Bind(this.tbTop, this.tbBottom).HeightTo(() => (int)(this.GetImageHeightRatio() * this.pictureBox.Height)).TopTo(() => (int)((1 - this.GetImageHeightRatio()) * this.pictureBox.Height / 2))
                .Bind(this.tbBottom).RightToForm()
                .Bind(this.tbRight).BottomToForm()
                .Bind(this.tbLeft).TopToForm()
                .Bind(this.tbTop).LeftToForm()
                .Activate();
        }

        public Offset Offsets { get; } = new Offset();

        public void Reset(ScanProfile currentScanProfile)
        {
            // Create default images so the the preview size can be set with a default image.
            PageDimensions pageDimensions = currentScanProfile.PageSize.PageDimensions();

            int previewResolution = currentScanProfile.PrevewResolution.ToIntDpi();

            int widthInPixels = (int)(pageDimensions.WidthInInches() * previewResolution);
            int heightInPixels = (int)(pageDimensions.HeightInInches() * previewResolution);
            this.workingImage?.Dispose();
            this.workingImage = new Bitmap(widthInPixels, heightInPixels);

            using (Graphics g = Graphics.FromImage(this.workingImage))
            {
                g.Clear(Color.Linen);
            }

            this.workingImage2?.Dispose();
            this.workingImage2 = (Bitmap)this.workingImage.Clone();

            this.UpdateCropBounds();
            this.UpdatePreviewBox();
            this.UpdateLayout();
        }

        public void SetImage(Bitmap newImageBitmap)
        {
            this.Offsets.Clear();

            this.workingImage?.Dispose();
            this.workingImage = (Bitmap)newImageBitmap.Clone();

            this.workingImage2?.Dispose();
            this.workingImage2 = (Bitmap)newImageBitmap.Clone();

            this.pictureBox.Image?.Dispose();

            // Set the picture box image to a clone, to avoid "the object is in use elsewhere" error.
            this.pictureBox.Image = (Image)this.workingImage.Clone();

            this.UpdateLayout();
            this.UpdateCropBounds();
        }

        public void UpdateLayout()
        {
            this.layoutManager.UpdateLayout();
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
                this.workingImage?.Dispose();
                this.workingImage2?.Dispose();
                this.pictureBox.Image?.Dispose();
                this.previewTimer?.Dispose();
            }

            base.Dispose(disposing);
        }

      
        private double GetImageHeightRatio()
        {
            if (this.workingImage == null)
            {
                return 1;
            }

            double imageAspect = this.workingImage.Width / (double)this.workingImage.Height;
            double pboxAspect = this.pictureBox.Width / (double)this.pictureBox.Height;
            if (pboxAspect > imageAspect)
            {
                return 1;
            }

            return pboxAspect / imageAspect;
        }

        private double GetImageWidthRatio()
        {
            if (this.workingImage == null)
            {
                return 1;
            }

            double imageAspect = this.workingImage.Width / (double)this.workingImage.Height;
            double pboxAspect = this.pictureBox.Width / (double)this.pictureBox.Height;
            if (imageAspect > pboxAspect)
            {
                return 1;
            }

            return imageAspect / pboxAspect;
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            this.dragStartCoords = this.TranslatePboxCoords(e.Location);
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point dragEndCoords = this.TranslatePboxCoords(e.Location);
                if (dragEndCoords.X > this.dragStartCoords.X)
                {
                    this.tbLeft.Value = this.dragStartCoords.X;
                    this.tbRight.Value = dragEndCoords.X;
                }
                else
                {
                    this.tbLeft.Value = dragEndCoords.X;
                    this.tbRight.Value = this.dragStartCoords.X;
                }

                if (dragEndCoords.Y > this.dragStartCoords.Y)
                {
                    this.tbTop.Value = this.workingImage.Height - this.dragStartCoords.Y;
                    this.tbBottom.Value = this.workingImage.Height - dragEndCoords.Y;
                }
                else
                {
                    this.tbTop.Value = this.workingImage.Height - dragEndCoords.Y;
                    this.tbBottom.Value = this.workingImage.Height - this.dragStartCoords.Y;
                }

                this.UpdateTransform();
            }
        }

        private void SetCropBounds(int width, int height)
        {
            this.tbLeft.Maximum = this.tbRight.Maximum = width;
            this.tbTop.Maximum = this.tbBottom.Maximum = height;

            this.tbLeft.Value = 0;
            this.tbBottom.Value = 0;
            this.tbRight.Value = width;
            this.tbTop.Value = height;
        }

        private void tbBottom_Scroll(object sender, EventArgs e)
        {
            this.UpdateTransform();
        }

        private void tbLeft_Scroll(object sender, EventArgs e)
        {
            this.UpdateTransform();
        }

        private void tbRight_Scroll(object sender, EventArgs e)
        {
            this.UpdateTransform();
        }

        private void tbTop_Scroll(object sender, EventArgs e)
        {
            this.UpdateTransform();
        }

        private Point TranslatePboxCoords(Point point)
        {
            double px = point.X - 1;
            double py = point.Y - 1;
            double imageAspect = this.workingImage.Width / (double)this.workingImage.Height;
            double pboxWidth = this.pictureBox.Width - 2;
            double pboxHeight = this.pictureBox.Height - 2;
            double pboxAspect = pboxWidth / pboxHeight;
            if (pboxAspect > imageAspect)
            {
                // Empty space on left/right
                double emptyWidth = (1 - imageAspect / pboxAspect) / 2 * pboxWidth;
                px = pboxAspect / imageAspect * (px - emptyWidth);
            }
            else
            {
                // Empty space on top/bottom
                double emptyHeight = (1 - pboxAspect / imageAspect) / 2 * pboxHeight;
                py = imageAspect / pboxAspect * (py - emptyHeight);
            }

            double x = px / pboxWidth * this.workingImage.Width;
            double y = py / pboxHeight * this.workingImage.Height;
            x = Math.Max(Math.Min(x, this.workingImage.Width), 0);
            y = Math.Max(Math.Min(y, this.workingImage.Height), 0);
            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }

        private void UpdateCropBounds()
        {
            this.SetCropBounds(this.workingImage.Width, this.workingImage.Height);
        }

        private void UpdatePreviewBox()
        {
            if (this.previewTimer == null)
            {
                this.previewTimer = new Timer(
                    (obj) =>
                        {
                            if (this.previewOutOfDate && !this.working)
                            {
                                this.working = true;
                                this.previewOutOfDate = false;
                                Bitmap bitmap = new Bitmap(this.workingImage2.Width, this.workingImage2.Height);
                                using (Graphics g = Graphics.FromImage(bitmap))
                                {
                                    g.Clear(Color.Transparent);
                                    ImageAttributes attrs = new ImageAttributes();
                                    attrs.SetColorMatrix(new ColorMatrix { Matrix33 = 0.5f });
                                    g.DrawImage(
                                        this.workingImage2,
                                        new Rectangle(0, 0, this.workingImage2.Width, this.workingImage2.Height),
                                        0,
                                        0,
                                        this.workingImage2.Width,
                                        this.workingImage2.Height,
                                        GraphicsUnit.Pixel,
                                        attrs);
                                    Rectangle cropBorderRect = new Rectangle(
                                        this.Offsets.Left,
                                        this.Offsets.Top,
                                        this.workingImage2.Width - this.Offsets.Left - this.Offsets.Right - 1,
                                        this.workingImage2.Height - this.Offsets.Top - this.Offsets.Bottom - 1);
                                    g.SetClip(cropBorderRect);
                                    g.DrawImage(
                                        this.workingImage2,
                                        new Rectangle(0, 0, this.workingImage2.Width, this.workingImage2.Height));
                                    g.ResetClip();
                                    g.DrawRectangle(new Pen(Color.Black, 1.0f), cropBorderRect);
                                }

                                this.SafeInvoke(
                                    () =>
                                        {
                                            this.pictureBox.Image?.Dispose();
                                            this.pictureBox.Image = bitmap;
                                        });
                                this.working = false;
                            }
                        },
                    null,
                    0,
                    100);
            }

            this.previewOutOfDate = true;
        }

        private void UpdateTransform()
        {
            this.Offsets.Left = Math.Min(this.tbLeft.Value, this.tbRight.Value);

            this.Offsets.Right = this.workingImage.Width - Math.Max(this.tbLeft.Value, this.tbRight.Value);
            this.Offsets.Bottom = Math.Min(this.tbTop.Value, this.tbBottom.Value);
            this.Offsets.Top = this.workingImage.Height - Math.Max(this.tbTop.Value, this.tbBottom.Value);

            this.UpdatePreviewBox();
        }
    }
}