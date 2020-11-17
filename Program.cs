using System;
using System.Collections.Generic;
using System.Linq;

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

            Users = generateUsers(1000);
            TxPool = generateTxPool(10000, Users);

            block = applyTxsToBlock(TxPool, 100);
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
            for (int i = 0, j = 0; i < count; i++, j++)
            {
                Transaction tx = new Transaction();
                if (j >= userCount - 2) { j = 0; }
                tx.Sender = Users[j].hashKey;
                tx.Receiver = Users[j + 1].hashKey;
                tx.Amount = random.Next(100, 1000);

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
            newBlock.diffTarget = "00000";
            newBlock.TxPool = new List<Transaction>();
            for (int i = 0; i < count; i++)
            {
                index = random.Next(txPool.Count);
                newBlock.TxPool.Add(txPool[index]);
                tempTxId += txPool[index].ID;
            }
            newBlock.merkelRootHash = new string(Hash.hashFunc(tempTxId));

            return newBlock;
        }
    }
}
