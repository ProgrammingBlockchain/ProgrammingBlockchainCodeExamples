using System;
using NBitcoin;
// ReSharper disable All

namespace _2._1KeyEncryption
{
    class Program
    {
        static void Main()
        {
            var privateKey = new Key();
            var bitcoinPrivateKey = privateKey.GetWif(Network.Main);
            Console.WriteLine(bitcoinPrivateKey); // L1tZPQt7HHj5V49YtYAMSbAmwN9zRjajgXQt9gGtXhNZbcwbZk2r
            BitcoinEncryptedSecret encryptedBitcoinPrivateKey = bitcoinPrivateKey.Encrypt("password");
            Console.WriteLine(encryptedBitcoinPrivateKey); // 6PYKYQQgx947Be41aHGypBhK6TA5Xhi9TdPBkatV3fHbbKrdDoBoXFCyLK
            var decryptedBitcoinPrivateKey = encryptedBitcoinPrivateKey.GetSecret("password");
            Console.WriteLine(decryptedBitcoinPrivateKey); // L1tZPQt7HHj5V49YtYAMSbAmwN9zRjajgXQt9gGtXhNZbcwbZk2r

            Console.ReadLine();
        }
    }
}
