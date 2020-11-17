using System;
using System.Collections.Generic;
using System.Text;

namespace blockchain
{
    class Block
    {
        /* header */
        public string prevHash { get; set; }
        public long timestamp { get; set; }
        public int version { get; set; }
        public string merkelRootHash { get; set; }
        public int nonce { get; set; }
        public string diffTarget { get; set; }

        /* body */
        public List<Transaction> TxPool;
    }
}
