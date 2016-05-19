using System;
using System.Linq;
using NBitcoin;
using NBitcoin.Stealth;

// ReSharper disable All

namespace _3._1TransactionBuilder
{
    class Program
    {
        static void Main()
        {
            /* Create a fake transaction */
            var bob = new Key();
            var alice = new Key();
            var satoshi = new Key();

            Script bobAlice = 
                PayToMultiSigTemplate.Instance.GenerateScriptPubKey(
                    2, 
                    bob.PubKey, alice.PubKey);

            var init = new Transaction();
            init.Outputs.Add(new TxOut(Money.Coins(1m), bob.PubKey)); // P2PK
            init.Outputs.Add(new TxOut(Money.Coins(1m), alice.PubKey.Hash)); // P2PKH
            init.Outputs.Add(new TxOut(Money.Coins(1m), bobAlice));

            /* Get the coins of the initial transaction */
            Coin[] coins = init.Outputs.AsCoins().ToArray();

            Coin bobCoin = coins[0];
            Coin aliceCoin = coins[1];
            Coin bobAliceCoin = coins[2];

            /* Build the transaction */
            var builder = new TransactionBuilder();
            Transaction tx = builder
                    .AddCoins(bobCoin)
                    .AddKeys(bob)
                    .Send(satoshi, Money.Coins(0.2m))
                    .SetChange(bob)
                    .Then()
                    .AddCoins(aliceCoin)
                    .AddKeys(alice)
                    .Send(satoshi, Money.Coins(0.3m))
                    .SetChange(alice)
                    .Then()
                    .AddCoins(bobAliceCoin)
                    .AddKeys(bob, alice)
                    .Send(satoshi, Money.Coins(0.5m))
                    .SetChange(bobAlice)
                    .SendFees(Money.Coins(0.0001m))
                    .BuildTransaction(sign: true);


            /* Verify you did not screw up */
            Console.WriteLine(builder.Verify(tx)); // True





            /* ScriptCoin */
            init = new Transaction();
            init.Outputs.Add(new TxOut(Money.Coins(1.0m), bobAlice.Hash));

            coins = init.Outputs.AsCoins().ToArray();
            ScriptCoin bobAliceScriptCoin = coins[0].ToScriptCoin(bobAlice);

            builder = new TransactionBuilder();
            tx = builder
                    .AddCoins(bobAliceScriptCoin)
                    .AddKeys(bob, alice)
                    .Send(satoshi, Money.Coins(0.9m))
                    .SetChange(bobAlice.Hash)
                    .SendFees(Money.Coins(0.0001m))
                    .BuildTransaction(true);
            Console.WriteLine(builder.Verify(tx)); // True




            /* STEALTH COIN */

            Key scanKey = new Key();
            BitcoinStealthAddress darkAliceBob =
                new BitcoinStealthAddress
                    (
                        scanKey: scanKey.PubKey,
                        pubKeys: new[] { alice.PubKey, bob.PubKey },
                        signatureCount: 2,
                        bitfield: null,
                        network: Network.Main
                    );

            //Someone sent to darkAliceBob
            init = new Transaction();
            darkAliceBob
                .SendTo(init, Money.Coins(1.0m));

            //Get the stealth coin with the scanKey
            StealthCoin stealthCoin
                = StealthCoin.Find(init, darkAliceBob, scanKey);

            //Spend it
            tx = builder
                    .AddCoins(stealthCoin)
                    .AddKeys(bob, alice, scanKey)
                    .Send(satoshi, Money.Coins(0.9m))
                    .SetChange(bobAlice.Hash)
                    .SendFees(Money.Coins(0.0001m))
                    .BuildTransaction(true);
            Console.WriteLine(builder.Verify(tx)); // True
            
            Console.ReadLine();
        }
    }
}
