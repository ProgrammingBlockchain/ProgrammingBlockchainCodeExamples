using System;
using NBitcoin;

namespace _2._2Bip38
{
    class Program
    {
        static void Main()
        {
            var passphraseCode = new BitcoinPassphraseCode("my secret", Network.Main, null);

            EncryptedKeyResult encryptedKeyResult = passphraseCode.GenerateEncryptedSecret();

            var generatedAddress = encryptedKeyResult.GeneratedAddress; 
            var encryptedKey = encryptedKeyResult.EncryptedKey; 
            var confirmationCode = encryptedKeyResult.ConfirmationCode; 

            Console.WriteLine(generatedAddress); // 14KZsAVLwafhttaykXxCZt95HqadPXuz73
            Console.WriteLine(encryptedKey); // 6PnWtBokjVKMjuSQit1h1Ph6rLMSFz2n4u3bjPJH1JMcp1WHqVSfr5ebNS
            Console.WriteLine(confirmationCode); // cfrm38VUcrdt2zf1dCgf4e8gPNJJxnhJSdxYg6STRAEs7QuAuLJmT5W7uNqj88hzh9bBnU9GFkN

            Console.WriteLine(confirmationCode.Check("my secret", generatedAddress)); // True
            var bitcoinPrivateKey = encryptedKey.GetSecret("my secret");
            Console.WriteLine(bitcoinPrivateKey.GetAddress() == generatedAddress); // True
            Console.WriteLine(bitcoinPrivateKey); // KzzHhrkr39a7upeqHzYNNeJuaf1SVDBpxdFDuMvFKbFhcBytDF1R

            Console.ReadLine();
        }
    }
}
