using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace blockchain
{
    class Program
    {
        private static Random random = new Random();

        private static int userCount = 0;
        public static int blockCount = 0;

        static void Main(string[] args)
        {
            List<User> Users = new List<User>();
            List<Transaction> TxPool = new List<Transaction>();
            Block block = new Block();
            List<Block> Blockchain = new List<Block>();

            Users = generateUsers(1000);
            TxPool = generateTxPool(10000, Users);

            block = applyTxsToBlock(TxPool, 100);
            Blockchain.Add(block);

            while (TxPool.Count > 0)
            {
                Console.WriteLine("{0} transactions left in the pool. Mining process started.", TxPool.Count);
                mineBlock(Blockchain, TxPool, Users);
            }
        }
        
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static List<User> generateUsers(int count)
        {
            List<User> newUsers = new List<User>();
            for (int i = 0; i < count; i++)
            {
                User GeneratedUsers = new User();
                GeneratedUsers.name = RandomString(10);
                GeneratedUsers.hashKey = new string(Hash.hashFunc(GeneratedUsers.name));
                GeneratedUsers.balance = random.Next(100, 1000000);
                userCount++;

                newUsers.Add(GeneratedUsers);
            }
            return newUsers;
        }
        public static List<Transaction> generateTxPool(int count, List<User> Users)
        {
            List<Transaction> newTxPool = new List<Transaction>();
            string tempId = "";
            for (int i = 0, j = 0; i < count; i++, j++)
            {
                Transaction tx = new Transaction();
                if (j >= userCount - 2) { j = 0; }
                tx.Sender = Users[j].hashKey;
                tx.Receiver = Users[j + 1].hashKey;
                tx.Amount = random.Next(100, 1000);
                tempId = tx.Sender + tx.Receiver + tx.Amount.ToString();
                tx.ID = new string(Hash.hashFunc(tempId));

                newTxPool.Add(tx);
            }

            return newTxPool;
        }
        public static Block applyTxsToBlock(List<Transaction> txPool, int count)
        {
            string tempTxId = string.Empty;
            int index;

            Block newBlock = new Block();
            blockCount++;
            newBlock.prevHash = string.Empty;
            newBlock.timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            newBlock.version = blockCount;
            newBlock.nonce = 0;
            newBlock.diffTarget = "0000";
            newBlock.TxPool = new List<Transaction>();
            for (int i = 0; i < count; i++)
            {
                index = random.Next(txPool.Count);
                newBlock.TxPool.Add(txPool[index]);
                tempTxId += txPool[index].ID;

                txPool.RemoveAt(index);
            }
            newBlock.merkelRootHash = new string(Hash.hashFunc(tempTxId));

            return newBlock;
        }

        public static void mineBlock(List<Block> Blockchain, List<Transaction> txPool, List<User> Users)
        {
            Block prevBlock = new Block();
            string newHash = "";
            string tempHash = "";
            bool isMined = false;
            var timer = new Stopwatch();

            prevBlock = Blockchain.Last();
            Console.WriteLine("Difficulty target: {0}.\nMining new block . . .\n", prevBlock.diffTarget);
            
            timer.Start();

            while (!isMined)
            {
                tempHash = prevBlock.prevHash + prevBlock.timestamp + prevBlock.version + prevBlock.merkelRootHash + prevBlock.nonce + prevBlock.diffTarget;
                newHash = new string(Hash.hashFunc(tempHash, true));
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@"D:\Mokslai\3 semestras\blockchain\blockchain\UsedHashesInMining.txt", true))
                {
                    file.WriteLine(newHash);
                }
                if (newHash.Substring(0, prevBlock.diffTarget.Length) == prevBlock.diffTarget)
                {
                    isMined = true;
                    Console.WriteLine("New block has been mined. Hash: " + newHash);
                }
                else
                {
                    prevBlock.nonce++;
                }
            }

            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;
            Console.WriteLine("Time taken for mining: {0}s", timeTaken.ToString(@"s\.fff"));

            addNewBlock(Blockchain, prevBlock.nonce, txPool, Users);
        }

        public static Block addNewBlock(List<Block> Blockchain, int nonce, List<Transaction> txPool, List<User> Users)
        {
            string tempPrevHash = string.Empty;

            Block prevBlock = new Block();
            Block newBlock = new Block();

            prevBlock = Blockchain.Last();

            blockCount++;

            tempPrevHash = prevBlock.prevHash + prevBlock.timestamp + prevBlock.version + prevBlock.merkelRootHash + prevBlock.nonce + prevBlock.diffTarget;
            newBlock.prevHash = new string(Hash.hashFunc(tempPrevHash));

            newBlock.timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            newBlock.version = blockCount;
            newBlock.nonce = nonce;
            newBlock.diffTarget = prevBlock.diffTarget;

            newBlock = processNewTrx(txPool, 100, newBlock, Users);

            return newBlock;
        }

        public static Block processNewTrx(List<Transaction> txPool, int count, Block block, List<User> Users)
        {
            string tempTxId = string.Empty;
            int index;
            bool redFlag = false;

            if (txPool.Count <= count) { count = txPool.Count; }
            for (int i = 0; i < count; i++)
            {
                redFlag = false;
                index = random.Next(txPool.Count);

                foreach (var user in Users)
                {
                    if (user.hashKey == txPool[index].Sender)
                    {
                        user.balance -= txPool[index].Amount;
                        if (user.balance < 0)
                        {
                            redFlag = true;
                            break;
                        }
                    }
                    else if (user.hashKey == txPool[index].Receiver)
                    {
                        user.balance += txPool[index].Amount;
                    }
                }
                if (!redFlag)
                {
                    block.TxPool = new List<Transaction>();
                    block.TxPool.Add(txPool[index]);
                    tempTxId += txPool[index].ID;
                }
                txPool.RemoveAt(index);
            }
            block.merkelRootHash = new string(Hash.hashFunc(tempTxId));

            return block;
        }
    }
}
