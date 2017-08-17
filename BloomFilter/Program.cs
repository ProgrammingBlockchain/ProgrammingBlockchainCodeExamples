// ============================================================================
// FileName: Program.cs
//
// Description:
// A minimal BitCoin client that searches for all transactions related to a single
// address using a bloom filter. The steps are:
//
// 1. Load blockchain headers from disk (if not available will be requested from 
//    the full node but that takes longer),
// 2. Connect to a full BitCoin node (this sample is hard coded to use the loopback 
//    address so the full node will need to be on the same machine),
// 3. Keep the blockchain headers synchronised with the full node and periodically
//    save them to disk,
// 4. Once the blockchain headers are synchronised use set a bloom filter and get 
//    all blocks within a certain date range to check for relevant transactions.
//
// NOTE: This sample does not do any block verification and is NOT suitable for 
// any kind of use on the main BitCoin network.
//
// Dependencies:
// The program relies on NBitcoin (https://github.com/MetacoSA/NBitcoin) for the 
// underlying BitCoin primitives.
//
// Hints:
// The original purpose for this sample was to gain an understanding of the BitCoin
// protocol. An invaluable tool for anyone attempting the same thing is WireShark
// (https://www.wireshark.org/) which has a built in BitCoin protocol decoder. To
// use WireShark with the loopback adapter on Windows install Npcap (https://nmap.org/npcap/).
//
// The command line used for the local bitcoin full node:
// "C:\Program Files\Bitcoin\daemon\bitcoind" -printtoconsole -datadir=f:\temp\bitcoind -server -testnet -debug=1 -bind=[::1]:18333
//
// Author(s):
// Aaron Clauson (https://github.com/sipsorcery)
//
// History:
// 14 Aug 2017  Aaron Clauson          Created.
//
// License: 
// Public Domain
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Protocol;
using log4net;

namespace BitCoinTest
{
    class Program
    {
        static ILog logger = log4net.LogManager.GetLogger("default");
        static Network _network = Network.TestNet;
        static string _chainFile = "chaintest.data";

        // Adjust the values below based on the BitCoin address that needs to be searched
        // and the dates that the address had some transactions (check with https://testnet.blockexplorer.com/). 
        static string _addressPrivateKey = "cR7X4Nd5WqA5mNwgX67th4Jo3K9vTTm28w8njLL9JT8hHPdbstL8";
        static DateTimeOffset _startSearchTime = DateTimeOffset.Parse("14 Jul 2017");
        static DateTimeOffset _endSearchTime = DateTimeOffset.Parse("29 Jul 2017");

        // Bloom filter parameters.
        static int _nElements = 64;
        static double _falsePositiveRate = 0.0001;
        static uint _nTweakIn = 50;

        static void Main(string[] args)
        {
            Console.WriteLine("Press q at any time to quit.");

            log4net.Config.XmlConfigurator.Configure();

            var tokenSource = new CancellationTokenSource();
            CancellationToken ct = tokenSource.Token;

            Key key = Key.Parse(_addressPrivateKey, _network);
            BitcoinPubKeyAddress addr = key.PubKey.GetAddress(_network);

            var chain = new ConcurrentChain(_network);
            Node node = null;

            LoadChain(chain, ct).ContinueWith(t =>
            {
                Task.Run(() => { PersistChain(chain, ct); });
                ConnectNodeAndSyncHeaders(chain, ct).ContinueWith(async (nodeTask) =>
                {
                    node = nodeTask.Result;
                    var txs = await GetTransactions(chain, node, addr, _startSearchTime, _endSearchTime, ct);

                    logger.DebugFormat("Number of matching transactions {0}.", txs.Count);
                });
            }, ct);

            while (true)
            {
                var keyPress = Console.ReadKey();
                if (keyPress.KeyChar == 'q')
                {
                    break;
                }
            }

            logger.DebugFormat("Exiting...");

            if (node != null)
            {
                node.DisconnectAsync();
            }

            tokenSource.Cancel();
        }

        /// <summary>
        /// Attempts to load the persisted blockchain headers from persistent storage. This is 
        /// a lot quicker than loading from a peer node.
        /// </summary>
        private async static Task LoadChain(ConcurrentChain chain, CancellationToken ct)
        {
            logger.DebugFormat("Commencing blockchain headers load from disk...");

            ct.ThrowIfCancellationRequested();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (File.Exists(_chainFile))
            {
                await Task.Run(() => { chain.Load(File.ReadAllBytes(_chainFile)); });
            }

            sw.Stop();

            logger.DebugFormat("Block headers load from disk, chain height {0} in {1}s.", chain.Height, sw.Elapsed.Seconds);
        }

        /// <summary>
        /// Peridically persists any updated blockchain headers to disk.
        /// </summary>
        private static void PersistChain(ConcurrentChain chain, CancellationToken ct)
        {
            logger.DebugFormat("Starting persist blockchain task.");

            ct.ThrowIfCancellationRequested();

            int chainHeight = (chain != null) ? chain.Height : 0;

            while (ct.IsCancellationRequested == false)
            {
                if (chain.Height != chainHeight)
                {
                    using (var fs = File.Open(_chainFile, FileMode.Create))
                    {
                        chain.WriteTo(fs);
                    }

                    logger.DebugFormat("Chain height increased to {0} ({1})", chain.Height, DateTime.Now.ToString("HH:mm:ss"));
                    chainHeight = chain.Height;
                }

                Task.Delay(5000, ct).Wait();
            }
        }

        /// <summary>
        /// Attempts to connect to a full BitCoin node and request any missing block headers.
        /// </summary>
        private static async Task<Node> ConnectNodeAndSyncHeaders(ConcurrentChain chain, CancellationToken ct)
        {
            logger.DebugFormat("Connecting to full node...");

            ct.ThrowIfCancellationRequested();

            ManualResetEventSlim headersSyncedSignal = new ManualResetEventSlim();
            var parameters = new NodeConnectionParameters();
            parameters.IsRelay = false;

            var scanLocation = new BlockLocator();
            scanLocation.Blocks.Add(chain.Tip != null ? chain.Tip.HashBlock : _network.GetGenesis().GetHash());

            var node = Node.ConnectToLocal(_network, parameters);

            logger.DebugFormat("Connected to node " + node.RemoteSocketEndpoint + ".");

            node.MessageReceived += (node1, message) =>
            {
                ct.ThrowIfCancellationRequested();

                switch (message.Message.Payload)
                {
                    case HeadersPayload hdr:

                        if (hdr.Headers != null && hdr.Headers.Count > 0)
                        {
                            logger.DebugFormat("Received {0} blocks start {1} to {2} height {3}.", hdr.Headers.Count, hdr.Headers.First().BlockTime, hdr.Headers.Last().BlockTime, chain.Height);

                            scanLocation.Blocks.Clear();
                            scanLocation.Blocks.Add(hdr.Headers.Last().GetHash());

                            if (hdr != null)
                            {
                                var tip = chain.Tip;
                                foreach (var header in hdr.Headers)
                                {
                                    var prev = tip.FindAncestorOrSelf(header.HashPrevBlock);
                                    if (prev == null)
                                    {
                                        break;
                                    }
                                    tip = new ChainedBlock(header, header.GetHash(), prev);
                                    chain.SetTip(tip);

                                    ct.ThrowIfCancellationRequested();
                                }
                            }

                            var getHeadersPayload = new GetHeadersPayload(scanLocation);
                            node.SendMessageAsync(getHeadersPayload);
                        }
                        else
                        {
                            // Headers synchronised.
                            logger.DebugFormat("Block headers synchronised.");
                            headersSyncedSignal.Set();
                        }

                        break;

                    case InvPayload inv:
                        logger.DebugFormat("Inventory items {0}, first type {1}.", inv.Count(), inv.First().Type);

                        if (inv.Any(x => x.Type == InventoryType.MSG_BLOCK))
                        {
                            // New block available.
                            var getHeadersPayload = new GetHeadersPayload(scanLocation);
                            node.SendMessage(getHeadersPayload);
                        }

                        break;

                    case MerkleBlockPayload merkleBlk:
                        break;

                    case TxPayload tx:
                        break;

                    default:
                        logger.DebugFormat(message.Message.Command);
                        break;
                }
            };

            node.Disconnected += n =>
            {
                logger.DebugFormat("Node disconnected, chain height " + chain.Height + ".");
            };

            node.VersionHandshake(ct);
            node.PingPong(ct);

            logger.DebugFormat("Requesting block headers greater than height {0}.", chain.Height);
            node.SendMessage(new GetHeadersPayload(scanLocation));

            logger.DebugFormat("Bitcoin node connected.");

            await Task.Run(() => {
                headersSyncedSignal.Wait(ct);
            });

            return node;
        }

        /// <summary>
        /// Once the blockchain headers have been synchronised this method will attempt to find all transactions relevant to a single address.
        /// To find the transactions there are two options: first option the full blocks can be completely downloaded and searched which is what a full node
        /// would do; second option is to set a bloom filter and then request the desired blocks from a connected full node.
        /// </summary>
        private static async Task<List<uint256>> GetTransactions(ConcurrentChain chain, Node node, BitcoinPubKeyAddress addr, DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
        {
            logger.DebugFormat("Transaction search task commencing...");

            ct.ThrowIfCancellationRequested();

            ManualResetEventSlim searchCompleteSignal = new ManualResetEventSlim();
            BloomFilter filter = new BloomFilter(_nElements, _falsePositiveRate, _nTweakIn, BloomFlags.UPDATE_NONE);
            logger.DebugFormat("Setting bloom for address " + addr.Hash + ".");
            filter.Insert(addr.Hash.ToBytes());

            List<uint256> txs = new List<uint256>();

            var searchBlocks = chain.ToEnumerable(true).Where(x => x.Header.BlockTime > start && x.Header.BlockTime < end).ToList();
            int searchBlocksIndex = 0;

            node.MessageReceived += (node1, message) =>
            {
                switch (message.Message.Payload)
                {
                    case MerkleBlockPayload merkleBlk:
                        foreach (var tx in merkleBlk.Object.PartialMerkleTree.GetMatchedTransactions())
                        {
                            logger.DebugFormat("Matched merkle block TX ID {0}.", tx);
                            txs.Add(tx);
                        }

                        if(searchBlocksIndex < searchBlocks.Count())
                        {
                            var dp = new GetDataPayload(new InventoryVector(InventoryType.MSG_FILTERED_BLOCK, searchBlocks[searchBlocksIndex++].HashBlock));
                            node.SendMessage(dp);
                        }
                        else
                        {
                            searchCompleteSignal.Set();
                        }

                        break;

                    case TxPayload tx:
                        logger.DebugFormat("TX ID {0}.", tx.Object.GetHash());
                        break;
                }
            };

            node.SendMessage(new FilterLoadPayload(filter));

            var dataPayload = new GetDataPayload(new InventoryVector(InventoryType.MSG_FILTERED_BLOCK, searchBlocks[searchBlocksIndex++].HashBlock));
            node.SendMessage(dataPayload);

            await Task.Run(() =>
            {
                searchCompleteSignal.Wait(ct);
                logger.DebugFormat("Block search task completed.");
            });

            return txs;
        }
    }
}
