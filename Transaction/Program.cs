// ReSharper disable All
using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using QBitNinja.Client;

namespace Transaction
{
    class Program
    {
        static void Main()
        {
            // Create a client
            QBitNinjaClient client = new QBitNinjaClient(Network.Main);
            // Parse transaction id to NBitcoin.uint256 so the client can eat it
            var transactionId = uint256.Parse("f13dc48fb035bbf0a6e989a26b3ecb57b84f85e0836e777d6edf60d87a4a2d94");
            // Query the transaction
            QBitNinja.Client.Models.GetTransactionResponse transactionResponse = client.GetTransaction(transactionId).Result;


            NBitcoin.Transaction transaction = transactionResponse.Transaction;

            Console.WriteLine(transactionResponse.TransactionId); // f13dc48fb035bbf0a6e989a26b3ecb57b84f85e0836e777d6edf60d87a4a2d94
            Console.WriteLine(transaction.GetHash()); // f13dc48fb035bbf0a6e989a26b3ecb57b84f85e0836e777d6edf60d87a4a2d94

            // RECEIVED COINS
            List<ICoin> receivedCoins = transactionResponse.ReceivedCoins;
            foreach (Coin coin in receivedCoins)
            {
                Money amount = coin.Amount;

                Console.WriteLine(amount.ToDecimal(MoneyUnit.BTC));
                var paymentScript = coin.ScriptPubKey;
                Console.WriteLine(paymentScript);  // It's the ScriptPubKey
                var address = paymentScript.GetDestinationAddress(Network.Main);
                Console.WriteLine(address);
                Console.WriteLine();
            }

            // RECEIVED COINS
            var outputs = transaction.Outputs;
            foreach (TxOut output in outputs)
            {
                Coin coin = new Coin(transaction, output);
                Money amount = coin.Amount;

                Console.WriteLine(amount.ToDecimal(MoneyUnit.BTC));
                var paymentScript = coin.GetScriptCode();
                Console.WriteLine(paymentScript);  // It's the ScriptPubKey
                var address = paymentScript.GetDestinationAddress(Network.Main);
                Console.WriteLine(address);
                Console.WriteLine();
            }

            // RECEIVED COINS
            foreach (TxOut output in outputs)
            {
                Money amount = output.Value;

                Console.WriteLine(amount.ToDecimal(MoneyUnit.BTC));
                var paymentScript = output.ScriptPubKey;
                Console.WriteLine(paymentScript);  // It's the ScriptPubKey
                var address = paymentScript.GetDestinationAddress(Network.Main);
                Console.WriteLine(address);
                Console.WriteLine();
            }

            // SPENT COINS
            List<ICoin> spentCoins = transactionResponse.SpentCoins;
            foreach (Coin coin in spentCoins)
            {
                Money amount = coin.Amount;

                Console.WriteLine(amount.ToDecimal(MoneyUnit.BTC));
                var paymentScript = coin.ScriptPubKey;
                Console.WriteLine(paymentScript);  // It's the ScriptPubKey
                var address = paymentScript.GetDestinationAddress(Network.Main);
                Console.WriteLine(address);
                Console.WriteLine();
            }

            // SPENT COINS
            foreach (Coin coin in spentCoins)
            {
                TxOut previousOutput = coin.TxOut;
                Money amount = previousOutput.Value;

                Console.WriteLine(amount.ToDecimal(MoneyUnit.BTC));
                var paymentScript = previousOutput.ScriptPubKey;
                Console.WriteLine(paymentScript);  // It's the ScriptPubKey
                var address = paymentScript.GetDestinationAddress(Network.Main);
                Console.WriteLine(address);
                Console.WriteLine();
            }


            var fee = transaction.GetFee(spentCoins.ToArray());
            Console.WriteLine(fee);

            var inputs = transaction.Inputs;
            foreach (TxIn input in inputs)
            {
                OutPoint previousOutpoint = input.PrevOut;
                Console.WriteLine(previousOutpoint.Hash); // hash of prev tx
                Console.WriteLine(previousOutpoint.N); // idx of out from prev tx, that has been spent in the current tx
                Console.WriteLine();
            }

            // Let's create a txout with 21 bitcoin from the first ScriptPubKey in our current transaction
            Money twentyOneBtc = new Money(21, MoneyUnit.BTC);
            var scriptPubKey = transaction.Outputs.First().ScriptPubKey;
            TxOut txOut = new TxOut(twentyOneBtc, scriptPubKey);

            OutPoint firstOutPoint = spentCoins.First().Outpoint;
            Console.WriteLine(firstOutPoint.Hash); // 4788c5ef8ffd0463422bcafdfab240f5bf0be690482ceccde79c51cfce209edd
            Console.WriteLine(firstOutPoint.N); // 0

            Console.WriteLine(transaction.Inputs.Count); // 9

            OutPoint firstPreviousOutPoint = transaction.Inputs.First().PrevOut;
            var firstPreviousTransactionResponse = client.GetTransaction(firstPreviousOutPoint.Hash).Result;
            Console.WriteLine(firstPreviousTransactionResponse.IsCoinbase); // False
            NBitcoin.Transaction firstPreviousTransaction = firstPreviousTransactionResponse.Transaction;

            //while (firstPreviousTransactionResponse.IsCoinbase == false)
            //{
            //    Console.WriteLine(firstPreviousTransaction.GetHash());

            //    firstPreviousOutPoint = firstPreviousTransaction.Inputs.First().PrevOut;
            //    firstPreviousTransactionResponse = client.GetTransaction(firstPreviousOutPoint.Hash).Result;
            //    firstPreviousTransaction = firstPreviousTransactionResponse.Transaction;
            //}

            Money spentAmount = Money.Zero;
            foreach (var spentCoin in spentCoins)
            {
                spentAmount = (Money)spentCoin.Amount.Add(spentAmount);
            }
            Console.WriteLine(spentAmount.ToDecimal(MoneyUnit.BTC)); // 13.19703492

            Money receivedAmount = Money.Zero;
            foreach (var receivedCoin in receivedCoins)
            {
                receivedAmount = (Money)receivedCoin.Amount.Add(receivedAmount);
            }
            Console.WriteLine(receivedAmount.ToDecimal(MoneyUnit.BTC)); // 13.19683492

            Console.WriteLine((spentAmount - receivedAmount).ToDecimal(MoneyUnit.BTC));

            Console.WriteLine(spentAmount.ToDecimal(MoneyUnit.BTC)-receivedAmount.ToDecimal(MoneyUnit.BTC));

            //var inputs = transaction.Inputs;
            //foreach (TxIn input in inputs)
            //{
            //    uint256 previousTransactionId = input.PrevOut.Hash;
            //    GetTransactionResponse previousTransactionResponse = client.GetTransaction(previousTransactionId).Result;

            //    NBitcoin.Transaction previousTransaction = previousTransactionResponse.Transaction;

            //    var previousTransactionOutputs = previousTransaction.Outputs;
            //    foreach (TxOut previousTransactionOutput in previousTransactionOutputs)
            //    {
            //        Money amount = previousTransactionOutput.Value;

            //        Console.WriteLine(amount.ToDecimal(MoneyUnit.BTC));
            //        var paymentScript = previousTransactionOutput.ScriptPubKey;
            //        Console.WriteLine(paymentScript);  // It's the ScriptPubKey
            //        var address = paymentScript.GetDestinationAddress(Network.Main);
            //        Console.WriteLine(address);
            //        Console.WriteLine();
            //    }
            //}

            Console.ReadLine();
        }
    }
}
