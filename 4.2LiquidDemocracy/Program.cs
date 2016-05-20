//ReSharper disable All

using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using NBitcoin.OpenAsset;

namespace _4._2LiquidDemocracy
{
    internal class Program
    {
        private static void Main()
        {
            var powerCoin = new Key();
            var alice = new Key();
            var bob = new Key();
            var satoshi = new Key();
            var init = new Transaction()
            {
                Outputs =
                {
                    new TxOut(Money.Coins(1.0m), powerCoin),
                    new TxOut(Money.Coins(1.0m), alice),
                    new TxOut(Money.Coins(1.0m), bob),
                    new TxOut(Money.Coins(1.0m), satoshi),
                }
            };

            var repo = new NoSqlColoredTransactionRepository();
            repo.Transactions.Put(init);

            var issuance = GetCoins(init, powerCoin)
                .Select(c => new IssuanceCoin(c))
                .ToArray();
            var builder = new TransactionBuilder();
            var toAlice =
                builder
                .AddCoins(issuance)
                .AddKeys(powerCoin)
                .IssueAsset(alice, new AssetMoney(powerCoin, 2))
                .SetChange(powerCoin)
                .Then()
                .AddCoins(GetCoins(init, alice))
                .AddKeys(alice)
                .Send(alice, Money.Coins(0.2m))
                .SetChange(alice)
                .BuildTransaction(true);
            repo.Transactions.Put(toAlice);

            var votingCoin = new Key();
            var init2 = new Transaction()
            {
                Outputs =
                    {
                        new TxOut(Money.Coins(1.0m), votingCoin),
                    }
            };
            repo.Transactions.Put(init2);

            issuance = GetCoins(init2, votingCoin).Select(c => new IssuanceCoin(c)).ToArray();
            builder = new TransactionBuilder();
            var toVoters =
                builder
                .AddCoins(issuance)
                .AddKeys(votingCoin)
                .IssueAsset(alice, new AssetMoney(votingCoin, 1))
                .IssueAsset(satoshi, new AssetMoney(votingCoin, 1))
                .SetChange(votingCoin)
                .BuildTransaction(true);
            repo.Transactions.Put(toVoters);

            var aliceVotingCoin = ColoredCoin.Find(toVoters, repo)
                        .Where(c => c.ScriptPubKey == alice.ScriptPubKey)
                        .ToArray();
            builder = new TransactionBuilder();
            var toBob =
                builder
                .AddCoins(aliceVotingCoin)
                .AddKeys(alice)
                .SendAsset(bob, new AssetMoney(votingCoin, 1))
                .BuildTransaction(true);
            repo.Transactions.Put(toBob);

            var bobVotingCoin = ColoredCoin.Find(toVoters, repo)
                        .Where(c => c.ScriptPubKey == bob.ScriptPubKey)
                        .ToArray();

            builder = new TransactionBuilder();
            var vote =
                builder
                .AddCoins(bobVotingCoin)
                .AddKeys(bob)
                .SendAsset(BitcoinAddress.Create("1HZwkjkeaoZfTSaJxDw6aKkxp45agDiEzN"),
                            new AssetMoney(votingCoin, 1))
                .BuildTransaction(true);

            issuance = GetCoins(init2, votingCoin).Select(c => new IssuanceCoin(c)).ToArray();
            issuance[0].DefinitionUrl = new Uri("http://boss.com/vote01.json");
            builder = new TransactionBuilder();
            toVoters =
                builder
                .AddCoins(issuance)
                .AddKeys(votingCoin)
                .IssueAsset(alice, new AssetMoney(votingCoin, 1))
                .IssueAsset(satoshi, new AssetMoney(votingCoin, 1))
                .SetChange(votingCoin)
                .BuildTransaction(true);
            repo.Transactions.Put(toVoters);

            Console.ReadLine();
        }

        private static IEnumerable<Coin> GetCoins(Transaction tx, Key owner)
        {
            return tx.Outputs.AsCoins().Where(c => c.ScriptPubKey == owner.ScriptPubKey);
        }

    }
}