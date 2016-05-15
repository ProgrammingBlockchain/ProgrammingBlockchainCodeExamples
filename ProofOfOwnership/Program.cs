using System;
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
    }
}
