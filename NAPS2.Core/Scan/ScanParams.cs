// --------------------------------------------------------------------------------
//  <copyright file="ScanParams.cs" company="NAPS2 Development Team">
//     Copyright 2012-2018 Ben Olden-Cooligan and contributors. All rights reserved.   
//  </copyright>
// --------------------------------------------------------------------------------

namespace NAPS2.Scan
{
    using System.Runtime.Serialization;
    using System.Threading;

    using NAPS2.Ocr;

    /// <summary>
    ///     Scan configuration that is separate from the user profile.
    ///     This lets scans behave a bit differently in the Batch Scan window, NAPS2.Console, etc.
    /// </summary>
    public class ScanParams
    {
        public bool DetectPatchCodes { get; set; }

        public bool Modal { get; set; } = true;

        public bool NoAutoSave { get; set; }

        public bool NoThumbnails { get; set; }

        public bool SkipPostProcessing { get; set; }

        public bool? DoOcr { get; set; }
        public bool NoUI { get; set; }

        [IgnoreDataMember]
        public CancellationToken OcrCancelToken { get; set; }

        [IgnoreDataMember]
        public OcrParams OcrParams { get; set; }

        public Offset Offsets { get; set; }
    }
}