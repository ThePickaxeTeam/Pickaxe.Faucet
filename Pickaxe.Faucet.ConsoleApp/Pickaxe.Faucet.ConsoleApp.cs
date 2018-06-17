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

namespace Pickaxe.Faucet.ConsoleApp
{
    class Program
    {

        static void Main(string[] args)
        {

            Console.WriteLine("\n   ___PICKAXE BLOCKCHAIN FAUCET___\n");
            Console.Write("Enter your PickAxe blockchain address: ");
            var recipientAddress = Console.ReadLine();
            int value = 1;
            while (true)
            {
            Console.Write("\n\nEnter desired amount of coins [1-100]: ");
            value = int.Parse(Console.ReadLine());
                {
                    if (value > 0 && value <= 100)
                        break;
                    else
                        Console.WriteLine("\nError. Try again!!!");
                }
            }

            var transLog = new Dictionary<string, int>();
            if (transLog.Keys.Contains(recipientAddress))
            {
                if (transLog[recipientAddress] + value <= 100)
                {
                    CreateAndSignFaucetTransaction(recipientAddress, value);
                    transLog[recipientAddress] += value;
                }
                else
                {
                    Console.WriteLine($"Your maximum amount to request is: {100 - transLog[recipientAddress]}");
                    Console.WriteLine("Transaction cancelled!");
                }
            }
            else
            {
                CreateAndSignFaucetTransaction(recipientAddress, value);
                transLog.Add(recipientAddress, value);
            }

        }

        public static string BytesToHex(byte[] bytes)
        {
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }

        static readonly X9ECParameters curve = SecNamedCurves.GetByName("secp256k1");
        private static readonly BigInteger faucetPrivateKey = new BigInteger("7e4670ae70c98d24f3662c172dc510a085578b9ccc717e6c2f4e547edd960a34", 16);
        private static readonly string faucetPublicKey = GeneratePubKeyFromPrivateKey("7e4670ae70c98d24f3662c172dc510a085578b9ccc717e6c2f4e547edd960a34");
        private static readonly string faucetAddress = GenerateAddressFromPubKey(faucetPublicKey);

        public static ECPoint GetPublicKeyFromPrivateKey(BigInteger privKey)
        {
            ECPoint pubKey = curve.G.Multiply(privKey).Normalize();
            return pubKey;
        }

        public static string EncodeECPointHexCompressed(ECPoint point)
        {
            BigInteger x = point.XCoord.ToBigInteger();
            return x.ToString(16) + Convert.ToInt32(!x.TestBit(0));
        }

        public static string CalcRipeMD160(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            RipeMD160Digest digest = new RipeMD160Digest();
            digest.BlockUpdate(bytes, 0, bytes.Length);
            byte[] result = new byte[digest.GetDigestSize()];
            digest.DoFinal(result, 0);
            return BytesToHex(result);
        }

        public static string GeneratePubKeyFromPrivateKey(string privKeyHex)
        {
            BigInteger privateKey = new BigInteger(privKeyHex, 16);
            ECPoint pubKey = GetPublicKeyFromPrivateKey(privateKey);
            string pubKeyCompressed = EncodeECPointHexCompressed(pubKey);
            return pubKeyCompressed;
        }

        public static string GenerateAddressFromPubKey(string pubKey)
        {
            string address = CalcRipeMD160(pubKey);
            return address;
        }

        private static byte[] CalcSHA256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            Sha256Digest digest = new Sha256Digest();
            digest.BlockUpdate(bytes, 0, bytes.Length);
            byte[] result = new byte[digest.GetDigestSize()];
            digest.DoFinal(result, 0);
            return result;
        }

        private static BigInteger[] SignData(BigInteger privateKey, byte[] data)
        {
            ECDomainParameters ecSpec = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            ECPrivateKeyParameters keyParameters = new ECPrivateKeyParameters(privateKey, ecSpec);
            IDsaKCalculator kCalculator = new HMacDsaKCalculator(new Sha256Digest());
            ECDsaSigner signer = new ECDsaSigner(kCalculator);
            signer.Init(true, keyParameters);
            BigInteger[] signature = signer.GenerateSignature(data);
            return signature;
        }

        public static void CreateAndSignFaucetTransaction(string recipientAddress, int value)
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine("CREATING TRANSACTION FROM FAUCET");
            Console.WriteLine("--------------------------------\n");

            string dateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:sssZ");

            var transaction = new
            {
                from = faucetAddress,
                to = recipientAddress,
                value = value,
                dateCreated = dateTime,
                senderPubKey = faucetPublicKey,
            };

            string tranJson = JsonConvert.SerializeObject(transaction);
            Console.WriteLine("Transaction (JSON): {0}\n\n", tranJson);

            byte[] tranHash = CalcSHA256(tranJson);
            Console.WriteLine("Transaction hash: {0}", BytesToHex(tranHash));

            Console.WriteLine("\nSIGN TRANSACTION? y/n");
            var confirmation = Console.ReadLine().ToLower();
            if (confirmation == "y")
            {
                //Sign Transaction
                BigInteger[] tranSignature = SignData(faucetPrivateKey, tranHash);

                var tranSigned = new
                {
                    from = faucetAddress,
                    to = recipientAddress,
                    value = value,
                    dateCreated = dateTime,
                    senderPubKey = faucetPublicKey,
                    senderSignature = new string[]
                    {
                        tranSignature[0].ToString(16),
                        tranSignature[1].ToString(16)
                    }
                };

                string signedTranJson = JsonConvert.SerializeObject(tranSigned, Formatting.Indented);
                Console.WriteLine("\nSigned transaction (JSON):");
                Console.WriteLine(signedTranJson);

                byte[] signedTranHash = CalcSHA256(signedTranJson);
                Console.WriteLine("\nSigned Transaction hash: {0}\n", BytesToHex(signedTranHash));

                File.WriteAllText(@"D:\SoftUni\!Blockchain Dev Course\Tem Project\signed trans json files\jsonfile.txt", signedTranJson);
            }
            else
            {
                Console.WriteLine("\nTransaction Cancelled!");
            }
        }
    }
}

