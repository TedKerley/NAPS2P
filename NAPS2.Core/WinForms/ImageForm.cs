using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    partial class ImageForm : FormBase
    {
        private readonly ChangeTracker changeTracker;

        private readonly ScannedImageRenderer scannedImageRenderer;

        private ImageForm()
        {
            // For the designer only
            InitializeComponent();
            
        }

        Lazy<ImagePreviewHelper> imagePreviewHelperLazy; 

        protected ImageForm(ChangeTracker changeTracker, ScannedImageRenderer scannedImageRenderer)
        {
            this.changeTracker = changeTracker;
            this.scannedImageRenderer = scannedImageRenderer;

            // Use a lazy variable because the PictureBox property is virtual.
            this.imagePreviewHelperLazy = new Lazy<ImagePreviewHelper>(() => new ImagePreviewHelper(this.scannedImageRenderer, this, this.RenderPreview, this.PictureBox));

            InitializeComponent();
            
        }

        public ScannedImage Image { get; set; }

        public List<ScannedImage> SelectedImages { get; set; }

        protected virtual IEnumerable<Transform> Transforms => throw new NotImplementedException();

        protected virtual PictureBox PictureBox => throw new NotImplementedException();

        private bool TransformMultiple => SelectedImages != null && checkboxApplyToSelected.Checked;

        private IEnumerable<ScannedImage> ImagesToTransform => TransformMultiple ? SelectedImages : Enumerable.Repeat(Image, 1);

        protected ImagePreviewHelper ImagePreviewHelper => this.imagePreviewHelperLazy.Value;

        protected virtual Bitmap RenderPreview()
        {
            var result = this.ImagePreviewHelper.GetImage();
            foreach (var transform in Transforms)
            {
                if (!transform.IsNull)
                {
                    result = transform.Perform(result);
                }
            }
            return result;
        }

        protected virtual void InitTransform()
        {
        }

        protected virtual void ResetTransform()
        {
        }

        protected virtual void TransformSaved()
        {
        }

       

        private async void ImageForm_Load(object sender, EventArgs e)
        {
            checkboxApplyToSelected.BringToFront();
            btnRevert.BringToFront();
            btnCancel.BringToFront();
            btnOK.BringToFront();
            if (SelectedImages != null && SelectedImages.Count > 1)
            {
                checkboxApplyToSelected.Text = string.Format(checkboxApplyToSelected.Text, SelectedImages.Count);
            }
            else
            {
                ConditionalControls.Hide(checkboxApplyToSelected, 6);
            }

            Size = new Size(600, 600);

            int maxDimen = Screen.AllScreens.Max(s => Math.Max(s.WorkingArea.Height, s.WorkingArea.Width));

            await this.ImagePreviewHelper.SetImageAsync(this.Image, maxDimen);

            InitTransform();
            UpdatePreviewBox();
        }
        
        protected void UpdatePreviewBox()
        {
            this.ImagePreviewHelper.UpdatePreviewBox();

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (Transforms.Any(x => !x.IsNull))
            {
                foreach (var img in ImagesToTransform)
                {
                    lock (img)
                    {
                        foreach (var t in Transforms)
                        {
                            img.AddTransform(t);
                        }
                    }
                }
                changeTracker.Made();
            }
            TransformSaved();
            Close();
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            ResetTransform();
            UpdatePreviewBox();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {
                this.ImagePreviewHelper.Dispose();
                this.PictureBox.Image?.Dispose();
                this.components?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
