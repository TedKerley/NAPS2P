using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Platform;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.Scan.Wia
{
    public class WiaScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "wia";

        private const int MAX_RETRIES = 5;

        private readonly IWiaTransfer backgroundWiaTransfer;
        private readonly IWiaTransfer foregroundWiaTransfer;
        private readonly IBlankDetector blankDetector;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly ScannedImageHelper scannedImageHelper;

        public WiaScanDriver(BackgroundWiaTransfer backgroundWiaTransfer, ForegroundWiaTransfer foregroundWiaTransfer, IBlankDetector blankDetector, ThumbnailRenderer thumbnailRenderer, ScannedImageHelper scannedImageHelper)
        {
            this.backgroundWiaTransfer = backgroundWiaTransfer;
            this.foregroundWiaTransfer = foregroundWiaTransfer;
            this.blankDetector = blankDetector;
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageHelper = scannedImageHelper;
        }

        public override string DriverName => DRIVER_NAME;

        public override bool IsSupported => PlatformCompat.System.IsWiaDriverSupported;

        protected override ScanDevice PromptForDeviceInternal() => WiaApi.PromptForScanDevice();

        protected override List<ScanDevice> GetDeviceListInternal() => WiaApi.GetScanDeviceList();

        protected override async Task ScanInternal(ScannedImageSource.Concrete source)
        {
            using (var eventLoop = new WiaBackgroundEventLoop(ScanProfile, ScanDevice, ScanParams))
            {
                bool supportsFeeder = eventLoop.GetSync(wia => WiaApi.DeviceSupportsFeeder(wia.Device));
                if (ScanProfile.PaperSource != ScanSource.Glass && !supportsFeeder)
                {
                    throw new NoFeederSupportException();
                }
                bool supportsDuplex = eventLoop.GetSync(wia => WiaApi.DeviceSupportsDuplex(wia.Device));
                if (ScanProfile.PaperSource == ScanSource.Duplex && !supportsDuplex)
                {
                    throw new NoDuplexSupportException();
                }
                int pageNumber = 1;
                int retryCount = 0;
                bool retry = false;
                bool cancel = false;
                do
                {
                    ScannedImage image;
                    try
                    {
                        if (pageNumber > 1 && ScanProfile.WiaDelayBetweenScans)
                        {
                            int delay = (int) (ScanProfile.WiaDelayBetweenScansSeconds.Clamp(0, 30) * 1000);
                            Thread.Sleep(delay);
                        }
                        (image, cancel) = await TransferImage(eventLoop, pageNumber);
                        pageNumber++;
                        retryCount = 0;
                        retry = false;
                    }
                    catch (ScanDriverException e)
                    {
                        if (ScanProfile.WiaRetryOnFailure && e.InnerException is COMException comError 
                            && (uint)comError.ErrorCode == 0x80004005 && retryCount < MAX_RETRIES)
                        {
                            Thread.Sleep(1000);
                            retryCount += 1;
                            retry = true;
                            continue;
                        }
                        throw;
                    }
                    if (image != null)
                    {
                        source.Put(image);
                    }
                } while (retry || (!cancel && ScanProfile.PaperSource != ScanSource.Glass));
            }
        }

        private async Task<(ScannedImage, bool)> TransferImage(WiaBackgroundEventLoop eventLoop, int pageNumber)
        {
            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    var transfer = ScanParams.NoUI ? backgroundWiaTransfer : foregroundWiaTransfer;
                    ChaosMonkey.MaybeError(0, new COMException("Fail", -2147467259));
                    using (var stream = transfer.Transfer(pageNumber, eventLoop, DialogParent, WiaApi.Formats.BMP))
                    {
                        if (stream == null)
                        {
                            return (null, true);
                        }

                        using (Image output = Image.FromStream(stream))
                        {
                            using (var result = scannedImageHelper.PostProcessStep1(output, ScanProfile))
                            {
                                if (blankDetector.ExcludePage(result, ScanProfile))
                                {
                                    return (null, false);
                                }

                                ScanBitDepth bitDepth = ScanProfile.UseNativeUI ? ScanBitDepth.C24Bit : ScanProfile.BitDepth;
                                var image = new ScannedImage(result, bitDepth, ScanProfile.MaxQuality, ScanProfile.Quality);
                                image.SetThumbnail(thumbnailRenderer.RenderThumbnail(result));
                                scannedImageHelper.PostProcessStep2(image, result, ScanProfile, ScanParams, pageNumber);
                                return (image, false);
                            }
                        }
                    }
                }
                catch (NoPagesException)
                {
                    if (ScanProfile.PaperSource != ScanSource.Glass && pageNumber == 1)
                    {
                        // No pages were in the feeder, so show the user an error
                        throw new NoPagesException();
                    }

                    // At least one page was scanned but now the feeder is empty, so exit normally
                    return (null, true);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
