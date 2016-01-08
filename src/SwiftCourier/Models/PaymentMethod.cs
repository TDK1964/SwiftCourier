using System;
using System.Collections.Generic;

namespace SwiftCourier.Models
{
    public partial class PaymentMethod
    {
        public PaymentMethod()
        {
            PaymentMethodFields = new HashSet<PaymentMethodField>();
            Payments = new HashSet<Payment>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<PaymentMethodField> PaymentMethodFields { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}