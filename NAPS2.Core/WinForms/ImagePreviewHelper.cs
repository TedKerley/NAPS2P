// <copyright file="ImagePreviewHelper.cs" company="High Quality Solutions">
//     Copyright (c)  High Quality Solutions Limited. All rights reserved.
// </copyright>

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
        class BitmapReference
        {
            private Bitmap bitmap;

            public Bitmap Value
            {
                get => this.bitmap;
                set 
            {
                this.bitmap?.Dispose();
                this.bitmap = value;
            }
        }

            // TODO correct dispose pattern.
            public void Dispose()
            {
                this.bitmap?.Dispose();
            }
        }

        public void CreateBlankImage(int widthInPixels, int heightInPixels, Color colour)
        {
            this.WorkingImage = new Bitmap(widthInPixels, heightInPixels);

            using (Graphics g = Graphics.FromImage(this.WorkingImage))
            {
                g.Clear(colour);
            }

            this.WorkingImage2 = (Bitmap)this.WorkingImage.Clone();
        }

        public Bitmap WorkingImage
        {
            get => this.workingImage.Value;
            set 

        {
            this.workingImage.Value = value;
            this.workingImage2.Value = value;
        }
    }

        public Bitmap WorkingImage2
        {
            get => this.workingImage2.Value;
            private set => this.workingImage2.Value = value;
        }

        private readonly Control parentControl;

        private readonly ScannedImageRenderer scannedImageRenderer;

        private bool previewOutOfDate;

        private Timer previewTimer;

        private bool working;

        private BitmapReference workingImage = new BitmapReference();

        private BitmapReference workingImage2 = new BitmapReference();

        public ImagePreviewHelper(ScannedImageRenderer scannedImageRenderer, Control parentControl, Func<Bitmap> renderPreviewFunc)
        {
            this.RenderPreviewFunc = renderPreviewFunc;
            this.scannedImageRenderer = scannedImageRenderer;
            this.parentControl = parentControl;
        }

        public Func<Bitmap> RenderPreviewFunc { get; set; }

        private PictureBox PictureBox { get; }

        // TODO correct dispose pattern.
        public void Dispose()
        {
            this.workingImage.Dispose();
            this.WorkingImage2.Dispose();
            this.previewTimer?.Dispose();
        }

        public Bitmap GetImage() => (Bitmap)this.WorkingImage.Clone();

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

        
    }
}