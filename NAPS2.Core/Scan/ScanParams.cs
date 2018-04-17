using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Scan
{
    using NAPS2.Scan.Wia;

    public class ScanParams
    {
        public bool DetectPatchCodes { get; set; }

        public bool NoUI { get; set; }

        public bool NoAutoSave { get; set; }

        public Offset Offsets { get; set; }
    }
}
