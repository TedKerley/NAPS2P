// --------------------------------------------------------------------------------
//  <copyright file="FPreviewScan.cs" company="NAPS2 Development Team">
//     Copyright 2012-2018 Ben Olden-Cooligan and contributors. All rights reserved.   
//  </copyright>
// --------------------------------------------------------------------------------

namespace NAPS2.WinForms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Threading.Tasks;

    using NAPS2.Config;
    using NAPS2.Scan;
    using NAPS2.Scan.Images;
    using NAPS2.Scan.Images.Transforms;
    using NAPS2.Util;

    partial class FPreviewScan : FormBase
    {
        private const int SCAN_OVERSIZE_TOLERANCE = 5;

        private readonly AppConfigManager appConfigManager;

        private readonly IProfileManager profileManager;

        private readonly ScannedImageRenderer scannedImageRenderer;

        private readonly IScanPerformer scanPerformer;

        private readonly ThumbnailRenderer thumbnailRenderer;

        private ScanProfile currentScanProfile;

        private LayoutManager layoutManager;

        private Offset previousOffsets = null;

        public FPreviewScan(
            ThumbnailRenderer thumbnailRenderer,
            ScannedImageRenderer scannedImageRenderer,
            IScanPerformer scanPerformer,
            IProfileManager profileManager,
            AppConfigManager appConfigManager)
        {
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageRenderer = scannedImageRenderer;
            this.scanPerformer = scanPerformer;
            this.profileManager = profileManager;
            this.appConfigManager = appConfigManager;

            this.InitializeComponent();
            this.imageAreaSelector.ImagePreviewHelper = new ImagePreviewHelper(
                this.scannedImageRenderer,
                this,
                this.imageAreaSelector.RenderPreview,
                this.imageAreaSelector.PictureBox);
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

            this.layoutManager = new LayoutManager(this).Bind(this.imageAreaSelector).WidthToForm().HeightToForm()
                .Bind(
                    this.btnDone,
                    this.btnReset,
                    this.btnPreview,
                    this.btnScan,
                    this.btnPreviewPrevious,
                    this.chkUseScanner).TopToForm().Bind(
                    this.btnPreview,
                    this.btnScan,
                    this.btnReset,
                    this.btnDone,
                    this.btnPreviewPrevious,
                    this.chkUseScanner).RightToForm().Activate();

            this.Reset();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void btnPreview_Click(object sender, EventArgs e)
        {
            await this.PreviewScanAsync(usePrevious: false);
        }

        private async void btnPreviewPrevious_Click(object sender, EventArgs e)
        {
            await this.PreviewScanAsync(usePrevious: true);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            this.Reset();
        }

        private async void btnScan_Click(object sender, EventArgs e)
        {
            await this.ScanAsync(this.ImageCallback, preview: false, usePrevious: false);
        }

        private async Task CropImageToRequestedSizeAsync(
            ScanProfile scanProfile,
            int dpi,
            Offset offsetsToUse,
            ScannedImage scan)
        {
            int requestedImageWidth = (int)(scanProfile.PageSize.PageDimensions().WidthInInches() * dpi
                                            - offsetsToUse.Left - offsetsToUse.Right);

            int requestedImageHeight = (int)(scanProfile.PageSize.PageDimensions().HeightInInches() * dpi
                                             - offsetsToUse.Top - offsetsToUse.Bottom);

            using (Bitmap bitmap = (Bitmap)Image.FromStream(await this.scannedImageRenderer.RenderToStream(scan)))
            {
                int widthOversize = bitmap.Width - requestedImageWidth;
                int heightOversize = bitmap.Height - requestedImageHeight;
                if (widthOversize > SCAN_OVERSIZE_TOLERANCE || heightOversize > SCAN_OVERSIZE_TOLERANCE)
                {
                    scan.AddTransform(new CropTransform() { Bottom = heightOversize, Right = widthOversize });
                    scan.SetThumbnail(await this.thumbnailRenderer.RenderThumbnail(scan));
                }
            }
        }

        private Offset GenerateNewOffsets(bool usePrevious)
        {
            Offset offsetsToUse = usePrevious
                                      ? this.previousOffsets
                                      :
                                      this.previousOffsets == null || this.previousOffsets.IsEmpty
                                          ?
                                          this.imageAreaSelector.Offsets.Clone()
                                          : this.previousOffsets.Append(this.imageAreaSelector.Offsets);
            return offsetsToUse;
        }

        private async Task PreviewScanAsync(bool usePrevious)
        {
            ScannedImage newImage = null;
            Bitmap bitmap = null;

            try
            {
                if (usePrevious || this.chkUseScanner.Checked)
                {
                    this.previousOffsets = await this.ScanAsync(
                                               image => newImage = image,
                                               preview: true,
                                               usePrevious: usePrevious);

                    // Disconnect bitmap from underlying file, allowing recovery file dispose to delete the associated file.
                    bitmap = (Bitmap)Image.FromStream(await this.scannedImageRenderer.RenderToStream(newImage));

                    this.imageAreaSelector.SetImage(bitmap);
                }
                else
                {
                    this.previousOffsets = this.GenerateNewOffsets(usePrevious: false);
                    this.imageAreaSelector.ExtendToSelection();
                }

                this.btnScan.Enabled = true;
                this.btnPreviewPrevious.Enabled = true;
                if (!this.chkUseScanner.Enabled)
                {
                    this.chkUseScanner.Enabled = true;

                    // Default to preview image area without rescanning.
                    // This is only set after the first preview scan.
                    this.chkUseScanner.Checked = false;
                }

                this.UpdateLayout();
            }
            finally
            {
                bitmap?.Dispose();
                newImage?.Dispose();
            }
        }

        private void Reset()
        {
            this.previousOffsets = null;

            // The scan button is disabled until a preview has been performed.
            this.btnScan.Enabled = false;
            this.btnPreviewPrevious.Enabled = false;
            this.chkUseScanner.Enabled = false;

            this.imageAreaSelector.Reset(this.currentScanProfile);
            this.UpdateLayout();
        }

        private async Task<Offset> ScanAsync(Action<ScannedImage> onImageAction, bool preview, bool usePrevious)
        {
            Offset offsetsToUse = this.GenerateNewOffsets(usePrevious);

            ScanProfile scanProfile = this.currentScanProfile.Clone();

            double scale = preview
                               ? 1
                               : (double)scanProfile.Resolution.ToIntDpi() / scanProfile.PrevewResolution.ToIntDpi();
            offsetsToUse = offsetsToUse.Scale(scale);

            int dpi = preview ? scanProfile.PrevewResolution.ToIntDpi() : scanProfile.Resolution.ToIntDpi();

            // Need a local copy to crop width to the correct size
            ScannedImage scan = null;

            // Set the scan resolution - rather than change all of the underlying code to use the preview
            // resolution if doing a preview scan, just set the resolution in the cloned profile.
            scanProfile.Resolution = preview ? scanProfile.PrevewResolution : scanProfile.Resolution;

            // Perform the scan - TODO correct the scaling - smaller images are appearing larger.
            // TODO - correct IDisposable implementations in changed code (not here).
            // TODO - fix error on normal TWAIN scan (not preview).
            // TODO - fix thumbnail image scaling after scan - for both twain and wia.
            // TODO - fix main scan - doesn't work in twain (maybe because offsets are not valid).
            await this.scanPerformer.PerformScan(
                scanProfile,
                new ScanParams() { Offsets = offsetsToUse },
                this,
                null,
                s => scan = s);

            // Note - there seems to be a minimum size to the scan, so crop to the requested sizes. 
            await this.CropImageToRequestedSizeAsync(scanProfile, dpi, offsetsToUse, scan);

            onImageAction(scan);

            return offsetsToUse;
        }

        private void UpdateLayout()
        {
            this.imageAreaSelector.UpdateLayout();
            this.layoutManager.UpdateLayout();
        }
    }
}