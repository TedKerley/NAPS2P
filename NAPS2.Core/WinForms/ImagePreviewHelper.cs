// --------------------------------------------------------------------------------
//  <copyright file="ImagePreviewHelper.cs" company="NAPS2 Development Team">
//     Copyright 2012-2018 Ben Olden-Cooligan and contributors. All rights reserved.   
//  </copyright>
// --------------------------------------------------------------------------------

namespace NAPS2.WinForms
{
    using System;
    using System.Drawing;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using NAPS2.Scan.Images;

    using Timer = System.Threading.Timer;

    public class ImagePreviewHelper : IDisposable
    {
        private readonly Control parentControl;

        private readonly ScannedImageRenderer scannedImageRenderer;

        private bool previewOutOfDate;

        private Timer previewTimer;

        private bool working;

        private BitmapReference workingImage = new BitmapReference();

        private BitmapReference workingImage2 = new BitmapReference();

        public ImagePreviewHelper(
            ScannedImageRenderer scannedImageRenderer,
            Control parentControl,
            Func<Bitmap> renderPreviewFunc,
            PictureBox pictureBox)
        {
            this.RenderPreviewFunc = renderPreviewFunc;
            this.PictureBox = pictureBox;
            this.scannedImageRenderer = scannedImageRenderer;
            this.parentControl = parentControl;
        }

        public Size CurrentImageSize => this.workingImage.Value.Size;

        public Func<Bitmap> RenderPreviewFunc { get; set; }

        public Bitmap WorkingImage
        {
            get => this.workingImage.Value;
            private set
            {
                this.workingImage.Value = value;
                this.workingImage2.Value = (Bitmap)value.Clone();
            }
        }

        public Bitmap WorkingImage2
        {
            get => this.workingImage2.Value;
            private set => this.workingImage2.Value = value;
        }

        private PictureBox PictureBox { get; }

        // TODO correct dispose pattern.
        public void Dispose()
        {
            this.workingImage.Dispose();
            this.WorkingImage2.Dispose();
            this.previewTimer?.Dispose();
        }

        public Bitmap GetImage() => (Bitmap)this.WorkingImage.Clone();

        public double GetImageHeightRatio()
        {
            if (this.WorkingImage == null)
            {
                return 1;
            }

            double imageAspect = this.WorkingImage.Width / (double)this.WorkingImage.Height;
            double pboxAspect = this.PictureBox.Width / (double)this.PictureBox.Height;
            if (pboxAspect > imageAspect)
            {
                return 1;
            }

            return pboxAspect / imageAspect;
        }

        public double GetImageWidthRatio()
        {
            if (this.WorkingImage == null)
            {
                return 1;
            }

            double imageAspect = this.WorkingImage.Width / (double)this.WorkingImage.Height;
            double pboxAspect = this.PictureBox.Width / (double)this.PictureBox.Height;
            if (imageAspect > pboxAspect)
            {
                return 1;
            }

            return imageAspect / pboxAspect;
        }

        public void SetBlankImage(int widthInPixels, int heightInPixels, Color colour)
        {
            using (Bitmap bitmap = new Bitmap(widthInPixels, heightInPixels))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(colour);
                }

                this.SetImage((Bitmap)bitmap.Clone());
            }
        }

        public void SetImage(Bitmap newImage)
        {
            this.WorkingImage = newImage;

            this.PictureBox.Image?.Dispose();

            // Set the picture box image to a clone, to avoid "the object is in use elsewhere" error.
            this.PictureBox.Image = (Image)this.WorkingImage.Clone();
        }

        public async Task SetImageAsync(ScannedImage image, int maxDimen = 0)
        {
            this.WorkingImage = await this.scannedImageRenderer.Render(image, maxDimen * 2);
            this.WorkingImage2 = (Bitmap)this.WorkingImage.Clone();
        }

        public void UpdatePreviewBox()
        {
            if (this.previewTimer == null)
            {
                this.previewTimer = new Timer(
                    (obj) =>
                        {
                            if (this.previewOutOfDate && !this.working && this.PictureBox != null)
                            {
                                this.working = true;
                                this.previewOutOfDate = false;
                                Bitmap bitmap = this.RenderPreviewFunc();
                                this.parentControl.SafeInvoke(
                                    () =>
                                        {
                                            this.PictureBox.Image?.Dispose();
                                            this.PictureBox.Image = bitmap;
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

        class BitmapReference
        {
            private Bitmap bitmap;

            public Bitmap Value
            {
                get => this.bitmap;
                set
                {
                    if (!ReferenceEquals(this.bitmap, value))
                    {
                        this.bitmap?.Dispose();
                    }

                    this.bitmap = value;
                }
            }

            // TODO correct dispose pattern.
            public void Dispose()
            {
                this.bitmap?.Dispose();
            }
        }
    }
}