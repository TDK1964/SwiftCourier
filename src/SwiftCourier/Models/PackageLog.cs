using System;
using System.Collections.Generic;

namespace SwiftCourier.Models
{
    public partial class PackageLog
    {
        public int Id { get; set; }
        public int PackageId { get; set; }
        public string LogMessage { get; set; }
        public DateTime LoggedAt { get; set; }

        public virtual Package Package { get; set; }
    }
}
