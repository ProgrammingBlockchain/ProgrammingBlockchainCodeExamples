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
            //===========================================================================================
            //Chapter8. Using the TransactionBuilder

            RandomUtils.Random = new UnsecureRandom();

            
            //Private key generator.
            Key privateKeyGenerator = new Key();
            BitcoinSecret bitcoinSecretFromPrivateKeyGenerator = privateKeyGenerator.GetBitcoinSecret(Network.Main);
            Key privateKeyFromBitcoinSecret = bitcoinSecretFromPrivateKeyGenerator.PrivateKey;
            Console.WriteLine($"privateKeyFromBitcoinSecret.ToString(Network.Main): {privateKeyFromBitcoinSecret.ToString(Network.Main)}");
            //L5DZpEdbDDhhk3EqtktmGXKv3L9GxttYTecxDhM5huLd82qd9uvo is for Alice
            //KxMrK5EJeUZ1z3Jyo2zPkurRVtYFefab4WQitV5CyjKApHsWfWg9 is for Bob
            //KyStsAHgSehHvewS5YfGwhQGfEWYd8qY2XZg6q2M6TqaM8Q8rayg is for Satoshi
            //L2f9Ntm8UUeTLZFv25oZ8WoRW8kAofUjdUdtCq9axCp1hZrsLZja is for ScanKey

            BitcoinSecret bitcoinSecretForAlice = new BitcoinSecret("L5DZpEdbDDhhk3EqtktmGXKv3L9GxttYTecxDhM5huLd82qd9uvo", Network.Main);
            BitcoinSecret bitcoinSecretForBob = new BitcoinSecret("KxMrK5EJeUZ1z3Jyo2zPkurRVtYFefab4WQitV5CyjKApHsWfWg9", Network.Main);
            BitcoinSecret bitcoinSecretForSatoshi = new BitcoinSecret("KyStsAHgSehHvewS5YfGwhQGfEWYd8qY2XZg6q2M6TqaM8Q8rayg", Network.Main);
            BitcoinSecret bitcoinSecretForScanKey = new BitcoinSecret("L2f9Ntm8UUeTLZFv25oZ8WoRW8kAofUjdUdtCq9axCp1hZrsLZja", Network.Main);


            Key bobPrivateKey = bitcoinSecretForBob.PrivateKey;
            Key alicePrivateKey = bitcoinSecretForAlice.PrivateKey;
            Key satoshiPrivateKey = bitcoinSecretForSatoshi.PrivateKey;
            Key privateKeyForScanKey = bitcoinSecretForScanKey.PrivateKey;


            Script scriptPubKeyOfBobAlice =
                PayToMultiSigTemplate.Instance.GenerateScriptPubKey(2, bobPrivateKey.PubKey, alicePrivateKey.PubKey);

            //This transaction will send money to Bob and Alice.
            //The thing you should notice is that this transaction is added by various types of scriptPubKey, such as P2PK(bobPrivateKey.PubKey), P2PKH(alicePrivateKey.PubKey.Hash), and multi-sig ScriptPubKey(scriptPubKeyOfBobAlice).
            var txGettingCoinForBobAlice = new Transaction();
            txGettingCoinForBobAlice.Outputs.Add(new TxOut(Money.Coins(1m), bobPrivateKey.PubKey)); 
            txGettingCoinForBobAlice.Outputs.Add(new TxOut(Money.Coins(1m), alicePrivateKey.PubKey.Hash));
            txGettingCoinForBobAlice.Outputs.Add(new TxOut(Money.Coins(1m), scriptPubKeyOfBobAlice));

            //Now let’s say they want to use the coins of this transaction to pay Satoshi.
            //First they have to get their coins.
            Coin[] coins = txGettingCoinForBobAlice.Outputs.AsCoins().ToArray();

            Coin bobCoin = coins[0];
            Coin aliceCoin = coins[1];
            Coin bobAliceCoin = coins[2];

            //Now let’s say Bob wants to send 0.2 BTC, Alice 0.3 BTC, and they agree to use bobAlice to send 0.5 BTC.
            //Build the transaction by using the features of the TransactionBuilder class.
            var builderForSendingCoinToSatoshi = new TransactionBuilder();
            Transaction txForSpendingCoinToSatoshi = builderForSendingCoinToSatoshi
                    .AddCoins(bobCoin)
                    .AddKeys(bobPrivateKey)
                    .Send(satoshiPrivateKey, Money.Coins(0.2m))
                    .SetChange(bobPrivateKey)
                    .Then()
                    .AddCoins(aliceCoin)
                    .AddKeys(alicePrivateKey)
                    .Send(satoshiPrivateKey, Money.Coins(0.3m))
                    .SetChange(alicePrivateKey)
                    .Then()
                    .AddCoins(bobAliceCoin)
                    .AddKeys(bobPrivateKey, alicePrivateKey)
                    .Send(satoshiPrivateKey, Money.Coins(0.5m))
                    .SetChange(scriptPubKeyOfBobAlice)
                    .SendFees(Money.Coins(0.0001m))
                    .BuildTransaction(sign: true);
            Console.WriteLine(txForSpendingCoinToSatoshi);

            //Then you can verify it is fully signed and ready to send to the network.
            //Verify you did not screw up.
            Console.WriteLine(builderForSendingCoinToSatoshi.Verify(txForSpendingCoinToSatoshi)); 



            //============================================================================================
            //Do with a ScriptCoin.

            var txGettingScriptCoinForBobAlice = new Transaction();
            txGettingScriptCoinForBobAlice.Outputs.Add(new TxOut(Money.Coins(1.0m), scriptPubKeyOfBobAlice.Hash));

            coins = txGettingScriptCoinForBobAlice.Outputs.AsCoins().ToArray();
            ScriptCoin bobAliceScriptCoin = coins[0].ToScriptCoin(scriptPubKeyOfBobAlice);

            //Then the signature:
            var builderForSendingScriptCoinToSatoshi = new TransactionBuilder();
            var txForSendingScriptCoinToSatoshi = builderForSendingScriptCoinToSatoshi
                    .AddCoins(bobAliceScriptCoin)
                    .AddKeys(bobPrivateKey, alicePrivateKey)
                    .Send(satoshiPrivateKey, Money.Coins(0.9m))
                    .SetChange(scriptPubKeyOfBobAlice.Hash)
                    .SendFees(Money.Coins(0.0001m))
                    .BuildTransaction(true);
            Console.WriteLine(builderForSendingScriptCoinToSatoshi.Verify(txForSendingScriptCoinToSatoshi));
            

            //============================================================================================
            //Do with a StealthCoin.

            //Let’s create a Bitcoin stealth address for Bob and Alice as in previous chapter:
            BitcoinStealthAddress bitcoinStealthAddressForBobAlice =
                new BitcoinStealthAddress
                    (
                        scanKey: privateKeyForScanKey.PubKey,
                        pubKeys: new[] { alicePrivateKey.PubKey, bobPrivateKey.PubKey },
                        signatureCount: 2,
                        bitfield: null,
                        network: Network.Main
                    );


            //Let’s say someone sent the coin to this transaction via the txGettingCoinForBobAliceToBitcoinStealthAddress which is a BitcoinStealthAddress:
            var txGettingCoinForBobAliceToBitcoinStealthAddress = new Transaction();
            bitcoinStealthAddressForBobAlice
                .SendTo(txGettingCoinForBobAliceToBitcoinStealthAddress, Money.Coins(1.0m));

            //The scanner will detect the StealthCoin:
            StealthCoin stealthCoin
                = StealthCoin.Find(txGettingCoinForBobAliceToBitcoinStealthAddress, bitcoinStealthAddressForBobAlice, privateKeyForScanKey);

            //And forward it to Bob and Alice, who will sign:
            //Let Bob and Alice sign and spend the coin.
            TransactionBuilder builderForBobAliceToBitcoinStealthAddress = new TransactionBuilder();
            txGettingCoinForBobAliceToBitcoinStealthAddress = builderForBobAliceToBitcoinStealthAddress
                    .AddCoins(stealthCoin)
                    .AddKeys(bobPrivateKey, alicePrivateKey, privateKeyForScanKey)
                    .Send(satoshiPrivateKey, Money.Coins(0.9m))
                    .SetChange(scriptPubKeyOfBobAlice.Hash)
                    .SendFees(Money.Coins(0.0001m))
                    .BuildTransaction(true);
            Console.WriteLine(builderForBobAliceToBitcoinStealthAddress.Verify(txGettingCoinForBobAliceToBitcoinStealthAddress));
            
            Console.ReadLine();
        }
    }
}
