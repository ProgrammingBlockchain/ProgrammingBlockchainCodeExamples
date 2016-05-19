using System;
using System.Linq;
using NBitcoin;
using NBitcoin.OpenAsset;
// ReSharper disable All

namespace _4._1UnitTests
{
    internal class Program
    {
        private static void Main()
        {
            var gold = new Key();
            var silver = new Key();
            var goldId = gold.PubKey.ScriptPubKey.Hash.ToAssetId();
            var silverId = silver.PubKey.ScriptPubKey.Hash.ToAssetId();

            var bob = new Key();
            var alice = new Key();
            var satoshi = new Key();

            var init = new Transaction
            {
                Outputs =
                {
                    new TxOut("1.0", gold),
                    new TxOut("1.0", silver),
                    new TxOut("1.0", satoshi)
                }
            };

            var repo = new NoSqlColoredTransactionRepository();

            repo.Transactions.Put(init);

            ColoredTransaction color = ColoredTransaction.FetchColors(init, repo);
            Console.WriteLine(color);

            var issuanceCoins =
                init
                    .Outputs
                    .AsCoins()
                    .Take(2)
                    .Select((c, i) => new IssuanceCoin(c))
                    .OfType<ICoin>()
                    .ToArray();

            var builder = new TransactionBuilder();
            var sendGoldToSatoshi =
                builder
                .AddKeys(gold)
                .AddCoins(issuanceCoins[0])
                .IssueAsset(satoshi, new AssetMoney(goldId, 10))
                .SetChange(gold)
                .BuildTransaction(true);
                        repo.Transactions.Put(sendGoldToSatoshi);
                        color = ColoredTransaction.FetchColors(sendGoldToSatoshi, repo);
                        Console.WriteLine(color);

            var goldCoin = ColoredCoin.Find(sendGoldToSatoshi, color).FirstOrDefault();

            builder = new TransactionBuilder();
            var sendToBobAndAlice =
                    builder
                    .AddKeys(satoshi)
                    .AddCoins(goldCoin)
                    .SendAsset(alice, new AssetMoney(goldId, 4))
                    .SetChange(satoshi)
                    .BuildTransaction(true);


            var satoshiBtc = init.Outputs.AsCoins().Last();
            builder = new TransactionBuilder();
            var sendToAlice =
                    builder
                    .AddKeys(satoshi)
                    .AddCoins(goldCoin, satoshiBtc)
                    .SendAsset(alice, new AssetMoney(goldId, 4))
                    .SetChange(satoshi)
                    .BuildTransaction(true);
            repo.Transactions.Put(sendToAlice);
            color = ColoredTransaction.FetchColors(sendToAlice, repo);

            Console.WriteLine(sendToAlice);
            Console.WriteLine(color);

            Console.ReadLine();
        }
    }
}