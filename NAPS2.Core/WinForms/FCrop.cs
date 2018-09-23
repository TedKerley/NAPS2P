using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    partial class FCrop : ImageForm
    {
        private static CropTransform _lastTransform;

        private static Size _lastSize;

       private LayoutManager lm;

        public FCrop(ChangeTracker changeTracker, ScannedImageRenderer scannedImageRenderer)
            : base(changeTracker, scannedImageRenderer)
        {
            InitializeComponent();

            this.imageAreaSelector.ImagePreviewHelper = this.ImagePreviewHelper;
            lm = new LayoutManager(this).Bind(this.imageAreaSelector).WidthToForm().HeightToForm().Activate();
            this.imageAreaSelector.SelectedAreaChanged += (sender, args) => this.UpdateTransform();

        }

        public CropTransform CropTransform { get; private set; }

        protected override IEnumerable<Transform> Transforms => new[] { CropTransform };

        protected override PictureBox PictureBox => this.imageAreaSelector.PictureBox;

        protected override Bitmap RenderPreview()
        {
            return this.imageAreaSelector.RenderPreview();
        }

        

        protected override void InitTransform()
        {
            if (_lastTransform != null && _lastSize == this.ImagePreviewHelper.WorkingImage.Size)
            {
                CropTransform = _lastTransform;
            }
            else
            {
                this.UpdateTransform();
            }

            this.imageAreaSelector.UpdateCropBounds();
            this.imageAreaSelector.UpdateLayout();
            this.lm.UpdateLayout();

        }

        private void UpdateTransform()
        {
            this.CropTransform = this.imageAreaSelector.CreateCropTransform();
        }

        protected override void ResetTransform()
        {
            this.imageAreaSelector.ClearSelection();
            CropTransform = this.imageAreaSelector.CreateCropTransform();


        }

        protected override void TransformSaved()
        {
            _lastTransform = CropTransform;
            _lastSize = this.ImagePreviewHelper.WorkingImage.Size;
        }

        private async void FCrop_Load(object sender, EventArgs e)
        {
            await this.imageAreaSelector.SetImageAsync(this.Image);
        }

    }
    
}
