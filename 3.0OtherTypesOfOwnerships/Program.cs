// ReSharper disable All
using System;
using System.Linq;
using System.Text;
using NBitcoin;
using NBitcoin.Crypto;

namespace _3._0OtherTypesOfOwnerships
{
    class Program
    {
        static void Main()
        {
            var publicKeyHash = new Key().PubKey.Hash;
            var bitcoinAddress = publicKeyHash.GetAddress(Network.Main);
            Console.WriteLine(publicKeyHash); // 41e0d7ab8af1ba5452b824116a31357dc931cf28
            Console.WriteLine(bitcoinAddress); // 171LGoEKyVzgQstGwnTHVh3TFTgo5PsqiY

            var scriptPubKey = bitcoinAddress.ScriptPubKey;
            Console.WriteLine(scriptPubKey); // OP_DUP OP_HASH160 41e0d7ab8af1ba5452b824116a31357dc931cf28 OP_EQUALVERIFY OP_CHECKSIG
            var sameBitcoinAddress = scriptPubKey.GetDestinationAddress(Network.Main);

            Block genesisBlock = Network.Main.GetGenesis();
            Transaction firstTransactionEver = genesisBlock.Transactions.First();
            Console.WriteLine(firstTransactionEver);

            var firstOutputEver = firstTransactionEver.Outputs.First();
            var firstScriptPubKeyEver = firstOutputEver.ScriptPubKey;
            Console.WriteLine(firstScriptPubKeyEver); // 04678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5f OP_CHECKSIG


            var firstBitcoinAddressEver = firstScriptPubKeyEver.GetDestinationAddress(Network.Main);
            Console.WriteLine(firstBitcoinAddressEver == null); // True

            var firstPubKeyEver = firstScriptPubKeyEver.GetDestinationPublicKeys().First();
            Console.WriteLine(firstPubKeyEver); // 04678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5f

            var key = new Key();
            Console.WriteLine("Pay to public key : " + key.PubKey.ScriptPubKey);
            Console.WriteLine();
            Console.WriteLine("Pay to public key hash : " + key.PubKey.Hash.ScriptPubKey);

            
            /* MUSTISIG */

            Key bob = new Key();
            Key alice = new Key();
            Key satoshi = new Key();

            scriptPubKey = PayToMultiSigTemplate.Instance.GenerateScriptPubKey(2, new PubKey[]
            {
                bob.PubKey,
                alice.PubKey,
                satoshi.PubKey
            });

            Console.WriteLine(scriptPubKey);

            var received = new Transaction();
            received.Outputs.Add(new TxOut(Money.Coins(1.0m), scriptPubKey));

            Coin coin = received.Outputs.AsCoins().First();

            BitcoinAddress nico = new Key().PubKey.GetAddress(Network.Main);
            TransactionBuilder builder = new TransactionBuilder();
            Transaction unsigned = 
                builder
                    .AddCoins(coin)
                    .Send(nico, Money.Coins(1.0m))
                    .BuildTransaction(sign: false);
            
            Transaction aliceSigned =
                builder
                    .AddCoins(coin)
                    .AddKeys(alice)
                    .SignTransaction(unsigned);

            Transaction bobSigned =
                builder
                    .AddCoins(coin)
                    .AddKeys(bob)
                    .SignTransaction(unsigned);

            Transaction fullySigned =
                builder
                    .AddCoins(coin)
                    .CombineSignatures(aliceSigned, bobSigned);

            Console.WriteLine(fullySigned);

            /* Pay to Script Hash */

            Console.WriteLine(scriptPubKey);
            Console.WriteLine(scriptPubKey.Hash.ScriptPubKey);

            Script redeemScript = 
                PayToMultiSigTemplate
                .Instance
                .GenerateScriptPubKey(2, new[] { bob.PubKey, alice.PubKey, satoshi.PubKey });
            received = new Transaction();
            //Pay to the script hash
            received.Outputs.Add(new TxOut(Money.Coins(1.0m), redeemScript.Hash));

            ScriptCoin scriptCoin = received.Outputs.AsCoins().First().ToScriptCoin(redeemScript);

            // P2SH(P2WPKH)
            
            Console.WriteLine(key.PubKey.WitHash.ScriptPubKey.Hash.ScriptPubKey);

            Console.WriteLine(key.PubKey.ScriptPubKey.WitHash.ScriptPubKey.Hash.ScriptPubKey);

            // Arbitrary

            BitcoinAddress address = BitcoinAddress.Create("1KF8kUVHK42XzgcmJF4Lxz4wcL5WDL97PB");
            var birth = Encoding.UTF8.GetBytes("18/07/1988");
            var birthHash = Hashes.Hash256(birth);
            redeemScript = new Script(
                "OP_IF "
                    + "OP_HASH256 " + Op.GetPushOp(birthHash.ToBytes()) + " OP_EQUAL " +
                "OP_ELSE "
                    + address.ScriptPubKey + " " +
                "OP_ENDIF");

            var tx = new Transaction();
            tx.Outputs.Add(new TxOut(Money.Parse("0.0001"), redeemScript.Hash));
            scriptCoin = tx.Outputs.AsCoins().First().ToScriptCoin(redeemScript);

            //Create spending transaction
            Transaction spending = new Transaction();
            spending.AddInput(new TxIn(new OutPoint(tx, 0)));

            ////Option 1 : Spender knows my birthdate
            Op pushBirthdate = Op.GetPushOp(birth);
            Op selectIf = OpcodeType.OP_1; //go to if
            Op redeemBytes = Op.GetPushOp(redeemScript.ToBytes());
            Script scriptSig = new Script(pushBirthdate, selectIf, redeemBytes);
            spending.Inputs[0].ScriptSig = scriptSig;

            //Verify the script pass
            var result = spending
                            .Inputs
                            .AsIndexedInputs()
                            .First()
                            .VerifyScript(tx.Outputs[0].ScriptPubKey);
            Console.WriteLine(result); // True
                                       ///////////

            ////Option 2 : Spender knows my private key
            BitcoinSecret secret = new BitcoinSecret("...");
            var sig = spending.SignInput(secret, scriptCoin);
            var p2pkhProof = PayToPubkeyHashTemplate
                .Instance
                .GenerateScriptSig(sig, secret.PrivateKey.PubKey);
            selectIf = OpcodeType.OP_0; //go to else
            scriptSig = p2pkhProof + selectIf + redeemBytes;
            spending.Inputs[0].ScriptSig = scriptSig;


            //Verify the script pass
            result = spending
                            .Inputs
                            .AsIndexedInputs()
                            .First()
                            .VerifyScript(tx.Outputs[0].ScriptPubKey);
            Console.WriteLine(result);
            ///////////

            Console.ReadLine();
        }
    }
}
