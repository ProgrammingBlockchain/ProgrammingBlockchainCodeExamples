using System;
using System.Linq;
using System.Text;
using NBitcoin;

namespace _4._3ProofOfBurn
{
    internal class Program
    {
        private static void Main()
        {
            var alice = new Key();

            //Giving some money to alice
            var init = new Transaction
            {
                Outputs =
                {
                    new TxOut(Money.Coins(1.0m), alice)
                }
            };

            var coin = init.Outputs.AsCoins().First();

            //Burning the coin
            var burn = new Transaction();
            burn.Inputs.Add(new TxIn(coin.Outpoint)
            {
                ScriptSig = coin.ScriptPubKey
            }); //Spend the previous coin

            const string message = "Burnt for \"Alice Bakery\"";
            var opReturn = TxNullDataTemplate
                .Instance
                .GenerateScriptPubKey(Encoding.UTF8.GetBytes(message));
            burn.Outputs.Add(new TxOut(Money.Coins(1.0m), opReturn));
            burn.Sign(alice, false);

            Console.WriteLine(burn);
        }
    }
}