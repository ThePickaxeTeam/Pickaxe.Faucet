using System;
using System.Collections.Generic;
using Org.BouncyCastle.Math;
using Newtonsoft.Json;
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
            ////Creates several hardcoded transactions for test purposes:

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

        private static readonly BigInteger faucetPrivateKey = new BigInteger("7e4670ae70c98d24f3662c172dc510a085578b9ccc717e6c2f4e547edd960a34", 16);
        private static readonly string faucetPublicKey = "c74a8458cd7a7e48f4b7ae6f4ae9f56c5c88c0f03e7c59cb4132b9d9d1600bba1";
        private static readonly string faucetAddress = "c3293572dbe6ebc60de4a20ed0e21446cae66b17";
        
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

            var tranSignature = EncryptionUtils.Sign(transactionDataHash, faucetPrivateKey);

            CreateTransactionRequest signedTransaction = CreateTransactionRequest.FromTransactionData(transactionData);
            signedTransaction.SenderSignature[0] = tranSignature[0].ToString(16);
            signedTransaction.SenderSignature[1] = tranSignature[1].ToString(16);

            string signedTranJson = JsonConvert.SerializeObject(signedTransaction, Formatting.Indented);
            Console.WriteLine("\nSigned transaction (JSON):");
            Console.WriteLine(signedTranJson);

            //var nodeClient = new NodeClient("http://localhost:64149");
            //Response<Transaction> response;
            //int retries = 0;
            //do
            //{
            //    response = await nodeClient.CreateTransaction(signedTransaction);
            //    retries++;
            //} while (response.Status == Status.Failed && retries <= 5);
            //
            //if (response.Status == Status.Success) Console.WriteLine("Transaction submitted to blockchain!");
            //else
            //{
            //    Console.WriteLine("Transaction unsuccessful!");
            //    foreach (var error in response.Errors) Console.WriteLine($"Error: {error}");
            //}
        }
    }
}

