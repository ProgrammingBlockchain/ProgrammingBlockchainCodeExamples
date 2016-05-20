using System;
using System.Linq;
using System.Runtime.InteropServices;
using NBitcoin;
using NBitcoin.DataEncoders;

// ReSharper disable All

namespace BitcoinAddress
{
    class Program
    {
        static void Main()
        {
            Key privateKey = new Key(); // generate a random private key

            PubKey publicKey = privateKey.PubKey;
            Console.WriteLine(publicKey); // 0251036303164f6c458e9f7abecb4e55e5ce9ec2b2f1d06d633c9653a07976560c

            Console.WriteLine(publicKey.GetAddress(Network.Main)); // 1PUYsjwfNmX64wS368ZR5FMouTtUmvtmTY
            Console.WriteLine(publicKey.GetAddress(Network.TestNet)); // n3zWAo2eBnxLr3ueohXnuAa8mTVBhxmPhq

            var publicKeyHash = publicKey.Hash;
            Console.WriteLine(publicKeyHash); // f6889b21b5540353a29ed18c45ea0031280c42cf
            var mainNetAddress = publicKeyHash.GetAddress(Network.Main);
            var testNetAddress = publicKeyHash.GetAddress(Network.TestNet);

            Console.WriteLine(mainNetAddress); // 1PUYsjwfNmX64wS368ZR5FMouTtUmvtmTY
            Console.WriteLine(testNetAddress); // n3zWAo2eBnxLr3ueohXnuAa8mTVBhxmPhq
            
            Console.ReadLine();
        }
    }
}
