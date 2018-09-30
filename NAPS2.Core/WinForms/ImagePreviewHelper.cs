// --------------------------------------------------------------------------------
//  <copyright file="ImagePreviewHelper.cs" company="NAPS2 Development Team">
//     Copyright 2012-2018 Ben Olden-Cooligan and contributors. All rights reserved.   
//  </copyright>
// --------------------------------------------------------------------------------

namespace NAPS2.WinForms
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using NAPS2.Scan.Images;

    using Timer = System.Threading.Timer;

    public class ImagePreviewHelper : IDisposable
    {
        private readonly Control parentControl;

        private readonly ScannedImageRenderer scannedImageRenderer;

        /// <summary>
        ///     Flag to record whether this <see cref="ImagePreviewHelper" /> instance has been disposed.
        /// </summary>
        private bool disposed = false;

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

        /// <summary>
        ///     Finalizes an instance of the <see cref="ImagePreviewHelper" /> class.
        ///     Disposes of un-managed resources in the event that that this <see cref="ImagePreviewHelper" /> instance has
        ///     not already been disposed when it is garbage collected.
        /// </summary>
        ~ImagePreviewHelper()
        {
            Debug.Assert(false, "Dispose method not called for ImagePreviewHelper.");
            this.Dispose(disposing: false);
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

        /// <summary>
        ///     Implementation of the IDisposable interface.
        ///     See http://msdn.microsoft.com/en-us/library/b1yfkh5e(v=vs.110).aspx
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
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

        /// <summary>
        ///     Helper function to perform the actual cleanup.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> if the <see cref="ImagePreviewHelper" /> instance is being explicitly disposed (i.e. Dispose() is
        ///     being called from user code from a using statement or finally block), false if the instance is disposed by garbage
        ///     collection via the finalizer.
        /// </param>
        /// <remarks>
        ///     If called via the finalizer, aggregated managed disposable objects do not need to be disposed, since they will be
        ///     disposed by
        ///     their own finalizers or finalizers of their base classes.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Free other state (managed member disposable objects).
                    this.workingImage.Dispose();
                    this.WorkingImage2.Dispose();
                    this.previewTimer?.Dispose();
                }

                // Free own state (un-managed objects).
                // Set large fields to null.

                // Record that this instance has been disposed.
                this.disposed = true;
            }
        }

        class BitmapReference : IDisposable
        {
            private Bitmap bitmap;

            /// <summary>
            ///     Flag to record whether this <see cref="BitmapReference" /> instance has been disposed.
            /// </summary>
            private bool disposed = false;

            /// <summary>
            ///     Finalizes an instance of the <see cref="BitmapReference" /> class.
            ///     Disposes of un-managed resources in the event that that this <see cref="BitmapReference" /> instance has
            ///     not already been disposed when it is garbage collected.
            /// </summary>
            ~BitmapReference()
            {
                Debug.Assert(false, "Dispose method not called for BitmapReference.");
                this.Dispose(disposing: false);
            }

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

            /// <summary>
            ///     Implementation of the IDisposable interface.
            ///     See http://msdn.microsoft.com/en-us/library/b1yfkh5e(v=vs.110).aspx
            /// </summary>
            public void Dispose()
            {
                this.Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            ///     Helper function to perform the actual cleanup.
            /// </summary>
            /// <param name="disposing">
            ///     <c>true</c> if the <see cref="BitmapReference" /> instance is being explicitly disposed (i.e. Dispose() is
            ///     being called from user code from a using statement or finally block), false if the instance is disposed by garbage
            ///     collection
            ///     via the finalizer.
            /// </param>
            /// <remarks>
            ///     If called via the finalizer, aggregated managed disposable objects do not need to be disposed, since they will be
            ///     disposed by
            ///     their own finalizers or finalizers of their base classes.
            /// </remarks>
            protected virtual void Dispose(bool disposing)
            {
                if (!this.disposed)
                {
                    if (disposing)
                    {
                        // Free other state (managed member disposable objects).
                        this.bitmap?.Dispose();
                    }

                    // Free own state (un-managed objects).
                    // Set large fields to null.

                    // Record that this instance has been disposed.
                    this.disposed = true;
                }
            }
        }
    }
}