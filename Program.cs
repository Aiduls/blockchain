using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace blockchain
{
    class Program
    {
        private static Random random = new Random();

        private static int userCount = 0;
        public static int blockCount = 0;
        public static bool isCancelled = false;
        //static CancellationTokenSource _tokenSource;

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

            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = 5;

            while (TxPool.Count > 0)
            {
                Console.WriteLine("There are transactions left in the pool. Mining process started.");
                if (TxPool.Count >= 500)
                {
                    isCancelled = false;
                    Parallel.Invoke(
                    () => mineBlock(Blockchain, TxPool.GetRange(0, 100), Users, TxPool),
                    () => mineBlock(Blockchain, TxPool.GetRange(100, 200), Users, TxPool),
                    () => mineBlock(Blockchain, TxPool.GetRange(200, 300), Users, TxPool),
                    () => mineBlock(Blockchain, TxPool.GetRange(300, 400), Users, TxPool),
                    () => mineBlock(Blockchain, TxPool.GetRange(400, 500), Users, TxPool));
                }
                else
                {
                    mineBlock(Blockchain, TxPool, Users, TxPool);
                }
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
            newBlock.diffTarget = "000000";
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

        public static void mineBlock(List<Block> Blockchain, List<Transaction> txPool, List<User> Users, List<Transaction> fullTxPool)
        {
            Block prevBlock = new Block();
            string newHash = "";
            string tempHash = "";
            bool isMined = false;
            var timer = new Stopwatch();

            prevBlock = Blockchain.Last();

            Console.WriteLine("Current thread: {0}\nMining new block . . .\n", Thread.CurrentThread.ManagedThreadId);

            timer.Start();

            while (!isMined)
            {
                try
                {
                    isThreadCancelled();
                    tempHash = prevBlock.prevHash + prevBlock.timestamp + prevBlock.version + prevBlock.merkelRootHash + prevBlock.nonce + prevBlock.diffTarget;
                    newHash = new string(Hash.hashFunc(tempHash, true));
                    if (newHash.Substring(0, prevBlock.diffTarget.Length) == prevBlock.diffTarget)
                    {
                        isThreadCancelled();
                        isMined = true;
                        isCancelled = true;
                        Console.WriteLine("New block has been mined. Lucky thread: {0}. Hash: {1}", Thread.CurrentThread.ManagedThreadId, newHash);
                    }
                    else
                    {
                        prevBlock.nonce++;
                        isThreadCancelled();
                    }
                }
                catch (TaskCanceledException e)
                {
                    return;
                }
            }

            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;
            Console.WriteLine("Time taken for mining: {0}s", timeTaken.ToString(@"s\.fff"));

            addNewBlock(Blockchain, prevBlock.nonce, txPool, Users, fullTxPool);

        }

        public static Block addNewBlock(List<Block> Blockchain, int nonce, List<Transaction> txPool, List<User> Users, List<Transaction> fullTxPool)
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

            newBlock = processNewTrx(txPool, newBlock, Users, fullTxPool);

            return newBlock;
        }

        public static Block processNewTrx(List<Transaction> txPool, Block block, List<User> Users, List<Transaction> fullTxPool)
        {
            string tempTxId = string.Empty;
            bool redFlag = false;
            List<string> merkleTree = new List<string>();
            List<string> trxToBeRemoved = new List<string>();

            for (int i = 0; i < txPool.Count; i++)
            {
                redFlag = false;

                // Transaction hash verification
                if (txPool[i].ID != new string(Hash.hashFunc(txPool[i].Sender + txPool[i].Receiver + txPool[i].Amount.ToString())))
                {
                    redFlag = true;
                    Console.WriteLine("ERROR: Transaction {0} hashes don't match", txPool[i].ID);
                }

                // Balance verification
                foreach (var user in Users)
                {
                    if (user.hashKey == txPool[i].Sender)
                    {
                        user.balance -= txPool[i].Amount;
                        if (user.balance < 0)
                        {
                            //Console.WriteLine("warning: User {0} has insufficient funds for transaction {1}", user.hashKey, txPool[i].ID);
                            redFlag = true;
                            break;
                        }
                    }
                    else if (user.hashKey == txPool[i].Receiver)
                    {
                        user.balance += txPool[i].Amount;
                    }
                }
                if (!redFlag)
                {
                    block.TxPool = new List<Transaction>();
                    block.TxPool.Add(txPool[i]);
                    merkleTree.Add(txPool[i].ID);
                }

                fullTxPool.RemoveAt(i);
            }

            block.merkelRootHash = BuildMerkleRoot(merkleTree);

            return block;
        }

        static string BuildMerkleRoot(List<String> merkleTree)
        {
            List<string> merkleBranches = new List<String>();
            string tempHash = "";

            if (!merkleTree.Any())
            {
                return "";
            }
            if (merkleTree.Count() == 1)
            {
                return merkleTree.First();
            }
            if (merkleTree.Count() % 2 > 0)
            {
                merkleTree.Add(merkleTree.Last());
            }
            for (int i = 0; i < merkleTree.Count; i += 2)
            {
                var leafPair = string.Concat(merkleTree[i], merkleTree[i + 1]);
                tempHash = new string(Hash.hashFunc(leafPair)); ;
                merkleBranches.Add(tempHash);
            }
            return BuildMerkleRoot(merkleBranches);
        }

        public static void isThreadCancelled()
        {
            if (isCancelled)
            {
                throw new TaskCanceledException();
            }
        }
    }
}
