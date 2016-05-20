using System;
using System.Linq;
using NBitcoin;
// ReSharper disable All

namespace ProofOfOwnership
{
    class Program
    {
        static void Main()
        {
            SignAsCraigWright();
            VerifySatoshi();
            VerifyDorier();

            /* BONUS: Get the first Bitcoin Address */
            Console.WriteLine(GetFirstBitcoinAddressEver()); // 1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa
            
            Console.ReadLine();
        }

        static void SignAsCraigWright()
        {
            var bitcoinPrivateKey = new BitcoinSecret("KzgjNRhcJ3HRjxVdFhv14BrYUKrYBzdoxQyR2iJBHG9SNGGgbmtC");

            var message = "I am Craig Wright";
            string signature = bitcoinPrivateKey.PrivateKey.SignMessage(message);
            Console.WriteLine(signature); // IN5v9+3HGW1q71OqQ1boSZTm0/DCiMpI8E4JB1nD67TCbIVMRk/e3KrTT9GvOuu3NGN0w8R2lWOV2cxnBp+Of8c=
        }
        static void VerifySatoshi()
        {
            var message = "I am Craig Wright";
            var signature = "IN5v9+3HGW1q71OqQ1boSZTm0/DCiMpI8E4JB1nD67TCbIVMRk/e3KrTT9GvOuu3NGN0w8R2lWOV2cxnBp+Of8c=";

            var address = new BitcoinPubKeyAddress("1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa");
            bool isCraigWrightSatoshi = address.VerifyMessage(message, signature);

            Console.WriteLine("Is Craig Wright Satoshi? " + isCraigWrightSatoshi);
        }
        static void VerifyDorier()
        {
            var address = new BitcoinPubKeyAddress("1KF8kUVHK42XzgcmJF4Lxz4wcL5WDL97PB");
            var message = "Nicolas Dorier Book Funding Address";
            var signature = "H1jiXPzun3rXi0N9v9R5fAWrfEae9WPmlL5DJBj1eTStSvpKdRR8Io6/uT9tGH/3OnzG6ym5yytuWoA9ahkC3dQ=";
            Console.WriteLine(address.VerifyMessage(message, signature));
        }

        static string GetFirstBitcoinAddressEver()
        {
            /* 
             * You probably know The Blockchain is a chain of blocks,
             * All the way back to the first block ever, called genesis.
             * Here is how you can get it: 
             */
            var genesisBlock = Network.Main.GetGenesis();

            /* 
             * You probably also know a block is made up of transactions.
             * Here is how you can get the first transaction ever:
             */
            var firstTransactionEver = genesisBlock.Transactions.First();

            /* 
             * You might not know that a transaction can have multiple outputs (and inputs).
             * Here is how you can get the first output ever:
             */
            var firstOutputEver = firstTransactionEver.Outputs.First();

            /* 
             * You usually see a destination of an output as a bitcoin address             * 
             * But the Bitcoin network doesn't know addresses, it knows ScriptPubKeys
             * A ScriptPubKey looks something like this:
             * OP_DUP OP_HASH160 62e907b15cbf27d5425399ebf6f0fb50ebb88f18 OP_EQUALVERIFY OP_CHECKSIG 
             * Let's get the ScriptPubKey of the first output ever:  
             */
            var firstScriptPubKeyEver = firstOutputEver.ScriptPubKey;

            /*
             * Actually your wallet software is what decodes addresses into ScriptPubKeys.
             * Or decodes ScriptPubKeys to addresses (when it is possible).
             * Here is how it does it:
             */

            /* 
             * In early times a ScriptPubKey contained one public key.
             * Here is the first ScriptPubKey ever:
             * 04678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5f OP_CHECKSIG
             * From a public key you can derive a bitcoin address.
             * So in order to get the first address ever, we can get the first public key ever:
             */

            var firstPubKeyEver = firstScriptPubKeyEver.GetDestinationPublicKeys().First();

            /*
             * You can get a bitcoin address from a public key and with the network identifier:
             */
            var firstBitcoinAddressEver = firstPubKeyEver.GetAddress(Network.Main);

            return firstBitcoinAddressEver.ToString(); 
        }
    }
}
