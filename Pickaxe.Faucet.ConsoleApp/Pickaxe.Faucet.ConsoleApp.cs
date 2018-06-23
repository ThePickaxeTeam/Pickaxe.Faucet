using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Pickaxe.Blockchain.Common;
using Pickaxe.Blockchain.Contracts;
using Pickaxe.Blockchain.Clients;
using Pickaxe.Blockchain.Contracts.Serialization;
using Pickaxe.Blockchain.Common.Extensions;

namespace Pickaxe.Faucet.ConsoleApp
{
    class Program
    {

        static void Main(string[] args)
        {            
            ////1. User input transaction creation:

            //Console.WriteLine("\n   ___PICKAXE BLOCKCHAIN FAUCET___\n");
            //Console.Write("Enter your PickAxe blockchain address: ");
            //var recipientAddress = Console.ReadLine();
            //int value = 1;
            //while (true)
            //{
            //    Console.Write("\n\nEnter desired amount of coins [1-100]: ");
            //    value = int.Parse(Console.ReadLine());
            //    if (value > 0 && value <= 100)
            //        break;
            //    else
            //        Console.WriteLine("\nError. Try again!!!");
            //}
            //
            //var transLog = new Dictionary<string, int>();
            //if (transLog.Keys.Contains(recipientAddress))
            //{
            //    if (transLog[recipientAddress] + value <= 100)
            //    {
            //        CreateAndSignFaucetTransaction(recipientAddress, value);
            //        transLog[recipientAddress] += value;
            //    }
            //    else
            //    {
            //        Console.WriteLine($"Your maximum amount to request is: {100 - transLog[recipientAddress]}");
            //        Console.WriteLine("Transaction cancelled!");
            //    }
            //}
            //else
            //{
            //    CreateAndSignFaucetTransaction(recipientAddress, value);
            //    transLog.Add(recipientAddress, value);
            //}


            ////2. Create several hardcoded transactions for test purposes:

            Dictionary<string, int> transList = new Dictionary<string, int>();
            transList.Add("3bF7592CCa4a95b3D1D2d03Abe2963D815b6985E", 12);
            transList.Add("f60F081D8eeE1d8D1b16Bfec1Ed70B2C6a3e7bE6", 23);
            transList.Add("561EAF23AF19874F60fcaBe19C043731C21e22b0", 34);
            transList.Add("060f99Ccf38f58135E2FfffB02496eF47e92519F", 45);
            transList.Add("180C404173c7719Cb2704E27Db92CeB44B0beCFa", 56);
            transList.Add("487CC44A14eab5F37A2D6569aBdCb9d3054011db", 67);
            transList.Add("070247EAd2a009bB982eAeD8B5E383B4564A69E7", 78);
            transList.Add("841cBa841EC532180238D2b8266C4f693D8B8800", 89);
            foreach (var address in transList)
            {
                CreateAndSignFaucetTransaction(address.Key, address.Value);
                Console.ReadLine();
            }

        }
               
        public static string BytesToHex(byte[] bytes)
        {
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }

        //static readonly X9ECParameters curve = SecNamedCurves.GetByName("secp256k1");
        private static readonly BigInteger faucetPrivateKey = new BigInteger("7e4670ae70c98d24f3662c172dc510a085578b9ccc717e6c2f4e547edd960a34", 16);
        //private static readonly string faucetPublicKey = GeneratePubKeyFromPrivateKey("7e4670ae70c98d24f3662c172dc510a085578b9ccc717e6c2f4e547edd960a34");
        //private static readonly string faucetAddress = GenerateAddressFromPubKey(faucetPublicKey); //faucet address: c3293572dbe6ebc60de4a20ed0e21446cae66b17

        public static string faucetPublicKey = "c74a8458cd7a7e48f4b7ae6f4ae9f56c5c88c0f03e7c59cb4132b9d9d1600bba1";
        public static string faucetAddress = "c3293572dbe6ebc60de4a20ed0e21446cae66b17";

        //public static ECPoint GetPublicKeyFromPrivateKey(BigInteger privKey)
        //{
        //    ECPoint pubKey = curve.G.Multiply(privKey).Normalize();
        //    return pubKey;
        //}
        //
        //public static string EncodeECPointHexCompressed(ECPoint point)
        //{
        //    BigInteger x = point.XCoord.ToBigInteger();
        //    return x.ToString(16) + Convert.ToInt32(!x.TestBit(0));
        //}
        //
        //public static string CalcRipeMD160(string text)
        //{
        //    byte[] bytes = Encoding.UTF8.GetBytes(text);
        //    RipeMD160Digest digest = new RipeMD160Digest();
        //    digest.BlockUpdate(bytes, 0, bytes.Length);
        //    byte[] result = new byte[digest.GetDigestSize()];
        //    digest.DoFinal(result, 0);
        //    return BytesToHex(result);
        //}
        //
        //public static string GeneratePubKeyFromPrivateKey(string privKeyHex)
        //{
        //    BigInteger privateKey = new BigInteger(privKeyHex, 16);
        //    ECPoint pubKey = GetPublicKeyFromPrivateKey(privateKey);
        //    string pubKeyCompressed = EncodeECPointHexCompressed(pubKey);
        //    return pubKeyCompressed;
        //}
        //
        //public static string GenerateAddressFromPubKey(string pubKey)
        //{
        //    string address = CalcRipeMD160(pubKey);
        //    return address;
        //}
        //
        //private static byte[] CalcSHA256(string text)
        //{
        //    byte[] bytes = Encoding.UTF8.GetBytes(text);
        //    Sha256Digest digest = new Sha256Digest();
        //    digest.BlockUpdate(bytes, 0, bytes.Length);
        //    byte[] result = new byte[digest.GetDigestSize()];
        //    digest.DoFinal(result, 0);
        //    return result;
        //}

        public static void CreateAndSignFaucetTransaction(string recipientAddress, int value)
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine("CREATING TRANSACTION FROM FAUCET");
            Console.WriteLine("--------------------------------\n");

            string dateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            TransactionData transactionData = new TransactionData
            {
                From = faucetAddress,
                To = recipientAddress,
                Value = value,
                Fee = 0,
                DateCreated = dateTime,
                Data = "faucetTX",
                SenderPubKey = faucetPublicKey
            };

            string transactionJson = JsonUtils.Serialize(transactionData, false);
            Console.WriteLine("Transaction (JSON): {0}\n\n", transactionJson);

            byte[] transactionDataHash = HashUtils.ComputeSha256(transactionJson.GetBytes());

            ////Sign Transaction

            //Console.WriteLine("\nSIGN TRANSACTION? y/n");
            //if (Console.ReadLine().ToLower() == "y")
            //if(true)
            //{
            var tranSignature = EncryptionUtils.Sign(transactionDataHash, faucetPrivateKey);

            CreateTransactionRequest signedTransaction = CreateTransactionRequest.FromTransactionData(transactionData);
            signedTransaction.SenderSignature[0] = tranSignature[0].ToString(16);
            signedTransaction.SenderSignature[1] = tranSignature[1].ToString(16);

            string signedTranJson = JsonConvert.SerializeObject(signedTransaction, Formatting.Indented);
            Console.WriteLine("\nSigned transaction (JSON):");
            Console.WriteLine(signedTranJson);

            //byte[] signedTranHash = CalcSHA256(signedTranJson);
            //Console.WriteLine("\nSigned Transaction hash: {0}\n", BytesToHex(signedTranHash));

            //}
            //else
            //{
            //    Console.WriteLine("\nTransaction Cancelled!");
            //}

            //var nodeClient = new NodeClient("http://localhost:64149");
            //Response<Transaction> response;
            //int retries = 0;
            //do
            //{
            //    response = await nodeClient.CreateTransaction(signedTransaction);
            //    retries++;
            //} while (response.Status == Status.Failed && retries <= 5);
            //
            //if (response.Status==Status.Success) Console.WriteLine("Transaction submitted to blockchain!");
            //else
            //{
            //    Console.WriteLine("Transaction unsuccessful!");
            //    foreach (var error in response.Errors) Console.WriteLine($"Error: {error}");
            //}
        }
    }
}

