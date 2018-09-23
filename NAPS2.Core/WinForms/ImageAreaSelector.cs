// <copyright file="ImageAreaSelector.cs" company="High Quality Solutions">
//     Copyright (c)  High Quality Solutions Limited. All rights reserved.
// </copyright>

namespace NAPS2.WinForms
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using NAPS2.Scan;
    using NAPS2.Scan.Images;
    using NAPS2.Scan.Images.Transforms;
    using NAPS2.Scan.Wia;

   
  
    public partial class ImageAreaSelector : UserControl
    {
        public EventHandler SelectedAreaChanged;
        public ImagePreviewHelper ImagePreviewHelper
        {
            get => this.imagePreviewHelper;
            set
            {
                this.imagePreviewHelper = value;
                this.ConfigureLayoutManager();
            }
        }

        private Point dragStartCoords;

        private LayoutManager layoutManager;

        private ImagePreviewHelper imagePreviewHelper;

        public ImageAreaSelector()
        {
            this.InitializeComponent();
            this.layoutManager = new LayoutManager(this);
        }

        public ImageAreaSelector(ImagePreviewHelper imagePreviewHelper) : this()
        {
            this.ImagePreviewHelper = imagePreviewHelper;
            this.ConfigureLayoutManager();
        }

        private void ConfigureLayoutManager()
        {
            this.layoutManager.Bind(this.pictureBox).LeftToForm().RightToForm().TopToForm()
                .BottomToForm().Bind(this.tbLeft, this.tbRight)
                .WidthTo(() => (int)(this.GetImageWidthRatio() * this.pictureBox.Width))
                .LeftTo(() => (int)((1 - this.GetImageWidthRatio()) * this.pictureBox.Width / 2))
                .Bind(this.tbTop, this.tbBottom).HeightTo(() => (int)(this.GetImageHeightRatio() * this.pictureBox.Height))
                .TopTo(() => (int)((1 - this.GetImageHeightRatio()) * this.pictureBox.Height / 2)).Bind(this.tbBottom)
                .RightToForm().Bind(this.tbRight).BottomToForm().Bind(this.tbLeft).TopToForm().Bind(this.tbTop).LeftToForm()
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
            
            this.ImagePreviewHelper.SetBlankImage(widthInPixels, heightInPixels, Color.Linen);

            

            this.UpdateCropBounds();
            
            this.UpdateLayout();
        }


        public async Task SetImageAsync(ScannedImage image)
        {
            await this.ImagePreviewHelper.SetImageAsync(image);

            this.UpdateLayout();
            this.UpdateCropBounds();
        }


        
        public void SetImage(Bitmap newImageBitmap)
        {
            this.Offsets.Clear();

            
            this.ImagePreviewHelper.SetImage((Bitmap)newImageBitmap.Clone());

            this.UpdateLayout();
            this.UpdateCropBounds();
        }

        public void UpdateLayout()
        {
            this.layoutManager.UpdateLayout();
        }

        private Bitmap WorkingImage => this.ImagePreviewHelper?.WorkingImage;
        private Bitmap WorkingImage2 => this.ImagePreviewHelper?.WorkingImage2;


        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
                
                this.pictureBox.Image?.Dispose();
                this.ImagePreviewHelper.Dispose();
            }

            base.Dispose(disposing);
        }

        public PictureBox PictureBox => this.pictureBox;


        private double GetImageRatio(bool isWidthRatio)
        {
            if (this.WorkingImage == null)
            {
                return 1;
            }

            double imageAspect = this.WorkingImage.Width / (double)this.WorkingImage.Height;
            double pictureBoxAspect = this.PictureBox.Width / (double)this.PictureBox.Height;

            double leftValue = isWidthRatio ? imageAspect : pictureBoxAspect;
            double rightValue = isWidthRatio ? pictureBoxAspect : imageAspect;

            if (leftValue > rightValue)
            {
                return 1;
            }

            return leftValue / rightValue;
        }

        private double GetImageHeightRatio()
        {
            return this.GetImageRatio(isWidthRatio: false);
        }

        private double GetImageWidthRatio()
        {
            return GetImageRatio(isWidthRatio: true);
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
                    this.tbTop.Value = this.WorkingImage.Height - this.dragStartCoords.Y;
                    this.tbBottom.Value = this.WorkingImage.Height - dragEndCoords.Y;
                }
                else
                {
                    this.tbTop.Value = this.WorkingImage.Height - dragEndCoords.Y;
                    this.tbBottom.Value = this.WorkingImage.Height - this.dragStartCoords.Y;
                }

                this.UpdateOffsets();
            }
        }

        public void ClearSelection()
        {
            this.SetCropBounds(this.WorkingImage.Width, this.WorkingImage.Height);
            this.UpdateOffsets();
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
            this.UpdateOffsets();
        }

        private void tbLeft_Scroll(object sender, EventArgs e)
        {
            this.UpdateOffsets();
        }

        private void tbRight_Scroll(object sender, EventArgs e)
        {
            this.UpdateOffsets();
        }

        private void tbTop_Scroll(object sender, EventArgs e)
        {
            this.UpdateOffsets();
        }

        private Point TranslatePboxCoords(Point point)
        {
            double px = point.X - 1;
            double py = point.Y - 1;
            double imageAspect = this.WorkingImage.Width / (double)this.WorkingImage.Height;
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

            double x = px / pboxWidth * this.WorkingImage.Width;
            double y = py / pboxHeight * this.WorkingImage.Height;
            x = Math.Max(Math.Min(x, this.WorkingImage.Width), 0);
            y = Math.Max(Math.Min(y, this.WorkingImage.Height), 0);
            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }

        public void UpdateCropBounds()
        {
            if (this.WorkingImage != null)
            {
                this.SetCropBounds(this.WorkingImage.Width, this.WorkingImage.Height);
            }
        }

       
        private void UpdatePreviewBox()
        {
            this.ImagePreviewHelper.UpdatePreviewBox();
        }

        public Bitmap RenderPreview()
        {
            Bitmap bitmap = new Bitmap(this.WorkingImage2.Width, this.WorkingImage2.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                ImageAttributes attrs = new ImageAttributes();
                attrs.SetColorMatrix(new ColorMatrix { Matrix33 = 0.5f });
                g.DrawImage(
                    this.WorkingImage2,
                    new Rectangle(0, 0, this.WorkingImage2.Width, this.WorkingImage2.Height),
                    0,
                    0,
                    this.WorkingImage2.Width,
                    this.WorkingImage2.Height,
                    GraphicsUnit.Pixel,
                    attrs);
                Rectangle cropBorderRect = new Rectangle(
                    this.Offsets.Left,
                    this.Offsets.Top,
                    this.WorkingImage2.Width - this.Offsets.Left - this.Offsets.Right - 1,
                    this.WorkingImage2.Height - this.Offsets.Top - this.Offsets.Bottom - 1);
                g.SetClip(cropBorderRect);
                g.DrawImage(this.WorkingImage2, new Rectangle(0, 0, this.WorkingImage2.Width, this.WorkingImage2.Height));
                g.ResetClip();
                g.DrawRectangle(new Pen(Color.Black, 1.0f), cropBorderRect);
            }

            return bitmap;
        }

        private void UpdateOffsets()
        {
            this.Offsets.Left = Math.Min(this.tbLeft.Value, this.tbRight.Value);

            this.Offsets.Right = this.WorkingImage.Width - Math.Max(this.tbLeft.Value, this.tbRight.Value);
            this.Offsets.Bottom = Math.Min(this.tbTop.Value, this.tbBottom.Value);
            this.Offsets.Top = this.WorkingImage.Height - Math.Max(this.tbTop.Value, this.tbBottom.Value);

            this.UpdatePreviewBox();
            this.SelectedAreaChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ExtendToSelection()
        {
            CropTransform transform = this.CreateCropTransform();
            this.SetImage(transform.Perform(this.WorkingImage));
        }

        public CropTransform CreateCropTransform()
        {
              CropTransform transform = new CropTransform
              {
                  Left = Math.Min(this.tbLeft.Value, this.tbRight.Value),
                  Right = this.WorkingImage.Width - Math.Max(this.tbLeft.Value, this.tbRight.Value),
                  Bottom = Math.Min(this.tbTop.Value, this.tbBottom.Value),
                  Top = this.WorkingImage.Height - Math.Max(this.tbTop.Value, this.tbBottom.Value),
                  OriginalHeight = this.WorkingImage.Height,
                  OriginalWidth = this.WorkingImage.Width
              };
                return transform;
        }
    }
}