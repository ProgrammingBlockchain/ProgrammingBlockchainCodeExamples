using System;
using NBitcoin;
// ReSharper disable All

namespace PaymentScript
{
    class Program
    {
        static void Main()
        {
            var publicKeyHash = new KeyId("14836dbe7f38c5ac3d49e8d790af808a4ee9edcf");

            var testNetAddress = publicKeyHash.GetAddress(Network.TestNet);
            var mainNetAddress = publicKeyHash.GetAddress(Network.Main);

            Console.WriteLine(mainNetAddress.ScriptPubKey); // OP_DUP OP_HASH160 14836dbe7f38c5ac3d49e8d790af808a4ee9edcf OP_EQUALVERIFY OP_CHECKSIG
            Console.WriteLine(testNetAddress.ScriptPubKey); // OP_DUP OP_HASH160 14836dbe7f38c5ac3d49e8d790af808a4ee9edcf OP_EQUALVERIFY OP_CHECKSIG

            
            var paymentScript = publicKeyHash.ScriptPubKey;
            var sameMainNetAddress = paymentScript.GetDestinationAddress(Network.Main);
            Console.WriteLine(mainNetAddress == sameMainNetAddress); // True


            var samePublicKeyHash = (KeyId)paymentScript.GetDestination();
            Console.WriteLine(publicKeyHash == samePublicKeyHash); // True
            var sameMainNetAddress2 = new BitcoinPubKeyAddress(samePublicKeyHash, Network.Main);
            Console.WriteLine(mainNetAddress == sameMainNetAddress2); // True


            Console.ReadLine();
        }
    }
}
