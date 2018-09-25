// --------------------------------------------------------------------------------
//  <copyright file="ScanParams.cs" company="NAPS2 Development Team">
//     Copyright 2012-2018 Ben Olden-Cooligan and contributors. All rights reserved.   
//  </copyright>
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using NAPS2.Ocr;

namespace NAPS2.Scan
{
    /// <summary>
    ///     Scan configuration that is separate from the user profile.
    ///     This lets scans behave a bit differently in the Batch Scan window, NAPS2.Console, etc.
    /// </summary>
    public class ScanParams
    {
        public bool DetectPatchCodes { get; set; }

        public bool Modal { get; set; } = true;

        public bool NoUI { get; set; }

        public bool NoAutoSave { get; set; }

        public bool NoUI { get; set; }

        [IgnoreDataMember]
        public OcrParams OcrParams { get; set; }

        [IgnoreDataMember]
        public CancellationToken OcrCancelToken { get; set; }

        public Offset Offsets { get; set; }
    }
}