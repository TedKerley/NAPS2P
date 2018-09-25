﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Scan.Twain;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.Worker
{
    /// <summary>
    /// The WCF service implementation for NAPS2.Worker.exe.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        IncludeExceptionDetailInFaults = true,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WorkerService : IWorkerService
    {
        private readonly TwainWrapper twainWrapper;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly MapiWrapper mapiWrapper;

        public WorkerService(TwainWrapper twainWrapper, ThumbnailRenderer thumbnailRenderer, MapiWrapper mapiWrapper)
        {
            this.twainWrapper = twainWrapper;
            this.thumbnailRenderer = thumbnailRenderer;
            this.mapiWrapper = mapiWrapper;
        }

        public void Init(string recoveryFolderPath)
        {
            Callback = OperationContext.Current.GetCallbackChannel<IWorkerCallback>();
            RecoveryImage.RecoveryFolder = new DirectoryInfo(recoveryFolderPath);
        }

        public List<ScanDevice> TwainGetDeviceList(TwainImpl twainImpl)
        {
            return twainWrapper.GetDeviceList(twainImpl);
        }

        public void TwainScan(ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr hwnd)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var imagePathDict = new Dictionary<ScannedImage, string>();
                    twainWrapper.Scan(hwnd == IntPtr.Zero ? null : new Win32Window(hwnd), scanDevice, scanProfile, scanParams,
                        new WorkerImageSource(Callback, imagePathDict), (img, _, path) => imagePathDict.Add(img, path));
                }
                catch (Exception e)
                {
                    var stream = new MemoryStream();
                    new NetDataContractSerializer().Serialize(stream, e);
                    Callback.Error(stream.ToArray());
                }
                finally
                {
                    Callback.Finish();
                }
            }, TaskCreationOptions.LongRunning);
        }

        public MapiSendMailReturnCode SendMapiEmail(EmailMessage message)
        {
            return mapiWrapper.SendEmail(message);
        }

        public byte[] RenderThumbnail(ScannedImage.Snapshot snapshot, int size)
        {
            var stream = new MemoryStream();
            using (var bitmap = Task.Factory.StartNew(() => thumbnailRenderer.RenderThumbnail(snapshot, size)).Unwrap().Result)
            {
                bitmap.Save(stream, ImageFormat.Png);
            }
            return stream.ToArray();
        }

        public void Dispose()
        {
        }

        public IWorkerCallback Callback { get; set; }

        private class WorkerImageSource : ScannedImageSource.Concrete
        {
            private readonly IWorkerCallback callback;
            private readonly Dictionary<ScannedImage, string> imagePathDict;

            public WorkerImageSource(IWorkerCallback callback, Dictionary<ScannedImage, string> imagePathDict)
            {
                this.callback = callback;
                this.imagePathDict = imagePathDict;
            }

            public override void Put(ScannedImage image)
            {
                MemoryStream stream = null;
                var thumb = image.GetThumbnail();
                if (thumb != null)
                {
                    stream = new MemoryStream();
                    thumb.Save(stream, ImageFormat.Png);
                }
                callback.TwainImageReceived(image.RecoveryIndexImage, stream?.ToArray(), imagePathDict.Get(image));
            }
        }
    }
}
