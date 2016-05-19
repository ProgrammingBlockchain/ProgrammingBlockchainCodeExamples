// ReSharper disable All
using System;
using System.Threading;
using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.OpenAsset;
using NBitcoin.Protocol;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace _4._1OtherAssets
{
    class Program
    {
        static void Main()
        {
           var coin = new Coin(
                fromTxHash: new uint256("eb49a599c749c82d824caf9dd69c4e359261d49bbb0b9d6dc18c59bc9214e43b"),
                fromOutputIndex: 0,
                amount: Money.Satoshis(2000000),
                scriptPubKey: new Script(Encoders.Hex.DecodeData("76a914c81e8e7b7ffca043b088a992795b15887c96159288ac")));

            var issuance = new IssuanceCoin(coin);


            var nico = BitcoinAddress.Create("15sYbVpRh6dyWycZMwPdxJWD4xbfxReeHe");
            //var bookKey = new BitcoinSecret("???????");
            var bookKey = new Key().GetBitcoinSecret(Network.Main); // Just a fake key in order to not get an exception

            var builder = new TransactionBuilder();

            Transaction tx = builder
                .AddKeys(bookKey)
                .AddCoins(issuance)
                .IssueAsset(nico, new AssetMoney(issuance.AssetId, quantity: 10))
                .SendFees(Money.Coins(0.0001m))
                .SetChange(bookKey.GetAddress())
                .BuildTransaction(true);

            Console.WriteLine(tx);

            Console.WriteLine(builder.Verify(tx));

            nico = BitcoinAddress.Create("15sYbVpRh6dyWycZMwPdxJWD4xbfxReeHe");
            Console.WriteLine(nico.ToColoredAddress());


            /* QBITNINJA */

            //var client = new QBitNinjaClient(Network.Main);
            //BroadcastResponse broadcastResponse = client.Broadcast(tx).Result;

            //if (!broadcastResponse.Success)
            //{
            //    Console.WriteLine("ErrorCode: " + broadcastResponse.Error.ErrorCode);
            //    Console.WriteLine("Error message: " + broadcastResponse.Error.Reason);
            //}
            //else
            //{
            //    Console.WriteLine("Success!");
            //}

            /* OR BITCOIN CORE */

            //using (var node = Node.ConnectToLocal(Network.Main)) //Connect to the node
            //{
            //    node.VersionHandshake(); //Say hello
            //                             //Advertize your transaction (send just the hash)
            //    node.SendMessage(new InvPayload(InventoryType.MSG_TX, tx.GetHash()));
            //    //Send it
            //    node.SendMessage(new TxPayload(tx));
            //    Thread.Sleep(500); //Wait a bit
            //}


            coin = new Coin(
                fromTxHash: new uint256("fa6db7a2e478f3a8a0d1a77456ca5c9fa593e49fd0cf65c7e349e5a4cbe58842"),
                fromOutputIndex: 0,
                amount: Money.Satoshis(2000000),
                scriptPubKey: new Script(Encoders.Hex.DecodeData("76a914356facdac5f5bcae995d13e667bb5864fd1e7d5988ac")));
            BitcoinAssetId assetId = new BitcoinAssetId("AVAVfLSb1KZf9tJzrUVpktjxKUXGxUTD4e");
            ColoredCoin colored = coin.ToColoredCoin(assetId, 10);

            var book = BitcoinAddress.Create("1KF8kUVHK42XzgcmJF4Lxz4wcL5WDL97PB");
            var nicoSecret = new BitcoinSecret("??????????");
            nico = nicoSecret.GetAddress(); //15sYbVpRh6dyWycZMwPdxJWD4xbfxReeHe

            var forFees = new Coin(
                fromTxHash: new uint256("7f296e96ec3525511b836ace0377a9fbb723a47bdfb07c6bc3a6f2a0c23eba26"),
                fromOutputIndex: 0,
                amount: Money.Satoshis(4425000),
                scriptPubKey: new Script(Encoders.Hex.DecodeData("76a914356facdac5f5bcae995d13e667bb5864fd1e7d5988ac")));

            builder = new TransactionBuilder();
            tx = builder
                .AddKeys(nicoSecret)
                .AddCoins(colored, forFees)
                .SendAsset(book, new AssetMoney(assetId, 10))
                .SetChange(nico)
                .SendFees(Money.Coins(0.0001m))
                .BuildTransaction(true);
            Console.WriteLine(tx);



            Console.ReadLine();
        }
    }
}
