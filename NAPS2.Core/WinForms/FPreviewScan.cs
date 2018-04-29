// <copyright file="FPreviewScan.cs" company="High Quality Solutions">
//     Copyright (c)  High Quality Solutions Limited. All rights reserved.
// </copyright>

using NAPS2.Scan.Images.Transforms;

namespace NAPS2.WinForms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Windows.Forms;

    using NAPS2.Config;
    using NAPS2.Scan;
    using NAPS2.Scan.Images;
    using NAPS2.Scan.Wia;
    using NAPS2.Util;

    using Timer = System.Threading.Timer;

    partial class FPreviewScan : FormBase
    {
        private const int SCAN_OVERSIZE_TOLERANCE = 5;

        private readonly AppConfigManager appConfigManager;

        private readonly ChangeTracker changeTracker;

        private readonly Offset offsets = new Offset();

        private readonly IProfileManager profileManager;

        private readonly ScannedImageRenderer scannedImageRenderer;

        private readonly IScanPerformer scanPerformer;

        private readonly ThumbnailRenderer thumbnailRenderer;

        private ScanProfile currentScanProfile;

        private Point dragStartCoords;

        private bool previewOutOfDate;

        private Timer previewTimer;

        private Offset previousOffsets = null;

        private bool working;

        private Bitmap workingImage;

        private Bitmap workingImage2;
        private LayoutManager layoutManager;

        public FPreviewScan(
            ChangeTracker changeTracker,
            ThumbnailRenderer thumbnailRenderer,
            ScannedImageRenderer scannedImageRenderer,
            IScanPerformer scanPerformer,
            IProfileManager profileManager,
            AppConfigManager appConfigManager)
        {
            this.changeTracker = changeTracker;
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageRenderer = scannedImageRenderer;
            this.scanPerformer = scanPerformer;
            this.profileManager = profileManager;
            this.appConfigManager = appConfigManager;
            this.InitializeComponent();
        }

        public Action<ScannedImage> ImageCallback { get; set; }

        public List<ScannedImage> SelectedImages { get; set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            // Set to hidden until the scan profile has been found, to prevent a fleeting glimpse of the form if
            // the users cancels a new profile dialog.
            this.Visible = false;

            // Get the current scan profile.  If no profiles have been defined, this will launch the new profile dialog.
            // If the user cancels that dialog, this form cannot continue, so just close the form.
            this.currentScanProfile = this.profileManager.DefaultOrNewProfile(this.FormFactory, this.appConfigManager);
            if (this.currentScanProfile == null)
            {
                this.Close();
            }

            // Form can be shown now there is a valid profile.
            this.Visible = true;

            this.layoutManager = new LayoutManager(this).Bind(this.pictureBox).WidthToForm().HeightToForm()
                .Bind(this.tbLeft, this.tbRight).WidthTo(() => (int)(this.GetImageWidthRatio() * this.pictureBox.Width)).LeftTo(() => (int)((1 - this.GetImageWidthRatio()) * this.pictureBox.Width / 2))
                .Bind(this.tbTop, this.tbBottom).HeightTo(() => (int)(this.GetImageHeightRatio() * this.pictureBox.Height)).TopTo(() => (int)((1 - this.GetImageHeightRatio()) * this.pictureBox.Height / 2))
                .Bind(this.tbBottom).RightToForm().Bind(this.tbRight).BottomToForm()
                .Bind(this.btnDone, this.btnReset, this.btnPreview, this.btnScan, this.btnPreviewPrevious).TopToForm()
                .Bind(this.btnPreview, this.btnScan, this.btnReset, this.btnDone, this.btnPreviewPrevious).RightToForm().Activate();


            this.Reset();
        }

        private void Reset()
        {
            this.offsets.Clear();
            this.previousOffsets = null;

            // The scan button is disabled until a preview has been performed.
            this.btnScan.Enabled = false;
            this.btnPreviewPrevious.Enabled = false;
            
            // Create default images so the the preview size can be set with a default image.
           
            var pageDimensions = this.currentScanProfile.PageSize.PageDimensions();

            
            var previewResolution = this.currentScanProfile.PrevewResolution.ToIntDpi();

            int widthInPixels = (int) (pageDimensions.WidthInInches() * previewResolution);
            int heightInPixels = (int) (pageDimensions.HeightInInches() * previewResolution);
            this.workingImage?.Dispose();
            this.workingImage = new Bitmap(widthInPixels, heightInPixels);

            using (var g = Graphics.FromImage(this.workingImage))
            {
                g.Clear(Color.Linen);
            }

            this.workingImage2?.Dispose();
            this.workingImage2 = (Bitmap) workingImage.Clone();

            this.UpdateCropBounds();
            this.UpdatePreviewBox();
            this.UpdateLayout();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            this.PreviewScan(usePrevious: false);
        }

        private void PreviewScan(bool usePrevious)
        {
            ScannedImage newImage = null;
            Bitmap bitmap = null;

            try
            {
                this.previousOffsets = this.Scan(image => newImage = image, preview: true, usePrevious: usePrevious);
                this.offsets.Clear();

                // Disconnect bitmap from underlying file, allowing recoveryfile dispose to delete the associated file.
                bitmap = (Bitmap)Image.FromStream(this.scannedImageRenderer.RenderToStream(newImage));

                this.workingImage?.Dispose();
                this.workingImage = (Bitmap)bitmap.Clone();

                this.workingImage2?.Dispose();
                this.workingImage2 = (Bitmap)bitmap.Clone();
            }
            finally
            {
                bitmap?.Dispose();
                newImage?.Dispose();
            }

            this.pictureBox.Image?.Dispose();

            // Set the picture box image to a clone, to avoid "the object is in use elsewhere" error.
            this.pictureBox.Image = (Image)this.workingImage.Clone();

            this.UpdateLayout();
            this.UpdateCropBounds();

            this.btnScan.Enabled = true;
            this.btnPreviewPrevious.Enabled = true;
        }

        private void btnPreviewPrevious_Click(object sender, EventArgs e)
        {
            this.PreviewScan(usePrevious: true);
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            this.Scan(this.ImageCallback, preview: false, usePrevious: false);
        }

        private void FPreviewScan_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.workingImage?.Dispose();
            this.workingImage2?.Dispose();
            this.pictureBox.Image?.Dispose();
            this.previewTimer?.Dispose();
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
                var dragEndCoords = this.TranslatePboxCoords(e.Location);
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

        private Offset ScaleCropTransform(ScannedImage img, Bitmap referenceBitmap)
        {
            using (var bitmap = this.scannedImageRenderer.Render(img))
            {
                double xScale = bitmap.Width / (double)referenceBitmap.Width,
                       yScale = bitmap.Height / (double)referenceBitmap.Height;
                return new Offset
                           {
                               Left = (int)Math.Round(this.offsets.Left * xScale),
                               Right = (int)Math.Round(this.offsets.Right * xScale),
                               Top = (int)Math.Round(this.offsets.Top * yScale),
                               Bottom = (int)Math.Round(this.offsets.Bottom * yScale)
                           };
            }
        }

        private Offset Scan(Action<ScannedImage> onImageAction, bool preview, bool usePrevious)
        {
            Offset offsetsToUse = usePrevious ? 
                this.previousOffsets : 
                this.previousOffsets == null || this.previousOffsets.IsEmpty
                                      ? this.offsets.Clone()
                                      : this.previousOffsets.Append(this.offsets);

            var scanProfile = this.currentScanProfile.Clone();

            double scale = preview ? 1 : (double)scanProfile.Resolution.ToIntDpi() / scanProfile.PrevewResolution.ToIntDpi();
            offsetsToUse = offsetsToUse.Scale(scale);

            int dpi = preview
                ? scanProfile.PrevewResolution.ToIntDpi()
                : scanProfile.Resolution.ToIntDpi();
 

            // Need a local copy to crop width to the correct size
            ScannedImage scan = null;

            scanProfile.Resolution = preview ? scanProfile.PrevewResolution : scanProfile.Resolution;
            this.scanPerformer.PerformScan(
                scanProfile,
                new ScanParams() { Offsets = offsetsToUse },
                this,
                null,
                s => scan = s);

            // Note - there seems to be a minimum size to the scan, so crop to the requested sizes. 
            int requestedImageWidth = (int)(scanProfile.PageSize.PageDimensions().WidthInInches() * dpi - offsetsToUse.Left - offsetsToUse.Right);

            int requestedImageHeight = (int)(scanProfile.PageSize.PageDimensions().HeightInInches() * dpi - offsetsToUse.Top - offsetsToUse.Bottom);

            using (Bitmap bitmap = (Bitmap) Image.FromStream(this.scannedImageRenderer.RenderToStream(scan)))
            {
                int widthOversize = bitmap.Width - requestedImageWidth;
                int heightOversize = bitmap.Height - requestedImageHeight;
                if (widthOversize > SCAN_OVERSIZE_TOLERANCE || heightOversize > SCAN_OVERSIZE_TOLERANCE)
                {

                    scan.AddTransform(new CropTransform()
                    {
                        Bottom = heightOversize,
                        Right = widthOversize
                    });
                    scan.SetThumbnail(thumbnailRenderer.RenderThumbnail(scan));
                }

            }

            onImageAction(scan);

            return offsetsToUse;
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
                var emptyWidth = (1 - imageAspect / pboxAspect) / 2 * pboxWidth;
                px = pboxAspect / imageAspect * (px - emptyWidth);
            }
            else
            {
                // Empty space on top/bottom
                var emptyHeight = (1 - pboxAspect / imageAspect) / 2 * pboxHeight;
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

        private void SetCropBounds(int width, int height)
        {
            this.tbLeft.Maximum = this.tbRight.Maximum = width;
            this.tbTop.Maximum = this.tbBottom.Maximum = height;

            this.tbLeft.Value = 0; 
            this.tbBottom.Value = 0;
            this.tbRight.Value = width;
            this.tbTop.Value = height;

        }

        private void UpdateLayout()
        {
            

            layoutManager.UpdateLayout();
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
                                var bitmap = new Bitmap(this.workingImage2.Width, this.workingImage2.Height);
                                using (var g = Graphics.FromImage(bitmap))
                                {
                                    g.Clear(Color.Transparent);
                                    var attrs = new ImageAttributes();
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
                                    var cropBorderRect = new Rectangle(
                                        this.offsets.Left,
                                        this.offsets.Top,
                                        this.workingImage2.Width - this.offsets.Left - this.offsets.Right-1,
                                        this.workingImage2.Height - this.offsets.Top - this.offsets.Bottom-1);
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
            this.offsets.Left = Math.Min(this.tbLeft.Value, this.tbRight.Value);

            this.offsets.Right = this.workingImage.Width - Math.Max(this.tbLeft.Value, this.tbRight.Value);
            this.offsets.Bottom = Math.Min(this.tbTop.Value, this.tbBottom.Value);
            this.offsets.Top = this.workingImage.Height - Math.Max(this.tbTop.Value, this.tbBottom.Value);

            this.UpdatePreviewBox();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            this.Reset();
        }
    }
}