using System;
using System.Collections.Generic;
using System.Text;

namespace blockchain
{
    class Transaction
    {
        public string ID { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public int Amount { get; set; }
    }
}
