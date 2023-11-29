//using System;
//using System.Security.Cryptography;
//using System.Security.Cryptography.X509Certificates;
//using crypto;
//using Org.BouncyCastle.Crypto;
//using Org.BouncyCastle.Crypto.Signers;
//using Org.BouncyCastle.Security;
//using Org.BouncyCastle.Utilities.Encoders;

//namespace ICAO.VDSnet
//{
//    public class Verifier
//    {
//        public enum Result
//        {
//            SignatureValid,
//            SignatureInvalid,
//            VerifyError
//        }

//        private ECPublicKeyParameters ecPubKey;
//        private int keySize = 256;
//        private byte[] messageBytes;
//        private byte[] signatureBytes;

//        private string signatureAlgorithmName = "SHA256WITHECDSA";

//        public Verifier(DigitalSeal digitalSeal, X509Certificate2 sealSignerCertificate)
//        {
//            CryptoProvider.RegisterProvider();

//            if (!(sealSignerCertificate.PublicKey.Key is ECPublicKeyParameters))
//            {
//                throw new ArgumentException("Certificate should contain EC public key!");
//            }

//            ecPubKey = (ECPublicKeyParameters)sealSignerCertificate.PublicKey.Key;
//            keySize = ecPubKey.Parameters.Curve.FieldSize;
//            messageBytes = digitalSeal.HeaderAndMessageBytes;
//            signatureBytes = digitalSeal.SignatureBytes;

//            Console.WriteLine($"Public Key bytes: 0x{Hex.ToHexString(ecPubKey.Q.GetEncoded())}");
//            Console.WriteLine($"Public Key size: {keySize}");
//            Console.WriteLine($"Message bytes: {Hex.ToHexString(messageBytes)}");
//            Console.WriteLine($"Signature bytes: {Hex.ToHexString(signatureBytes)}");
//        }

//        public Result Verify()
//        {
//            // Based on the length of the signature, the hash algorithm is determined
//            // TODO is there a better solution? Maybe based on the Public Key size?
//            if (signatureBytes[1] > 0x46)
//                signatureAlgorithmName = "SHA384WITHECDSA";
//            else if (signatureBytes[1] < 0x3F)
//                signatureAlgorithmName = "SHA224WITHECDSA";

//            try
//            {
//                ECDsaSigner ecdsaVerify = SignerUtilities.GetSigner(signatureAlgorithmName);
//                ecdsaVerify.Init(false, ecPubKey);
//                ecdsaVerify.BlockUpdate(messageBytes, 0, messageBytes.Length);

//                if (ecdsaVerify.VerifySignature(signatureBytes))
//                {
//                    return Result.SignatureValid;
//                }
//                else
//                {
//                    return Result.SignatureInvalid;
//                }
//            }
//            catch (NoSuchAlgorithmException e1)
//            {
//                Console.WriteLine($"NoSuchAlgorithmException: {e1.Message}");
//                return Result.VerifyError;
//            }
//            catch (InvalidKeyException e2)
//            {
//                Console.WriteLine($"InvalidKeyException: {e2.Message}");
//                return Result.VerifyError;
//            }
//            catch (SignatureException e3)
//            {
//                Console.WriteLine($"SignatureException: {e3.Message}");
//                return Result.VerifyError;
//            }
//        }
//    }

//    public static class CryptoProvider
//    {
//        public static void RegisterProvider()
//        {
//            // You may need to install the BouncyCastle NuGet package
//            Org.BouncyCastle.Security.Provider[] providers = Security.GetProviders("BC");
//            if (providers == null || providers.Length == 0)
//            {
//                Security.AddProvider(new Org.BouncyCastle.Security.BouncyCastleProvider());
//            }
//        }
//    }

//    // Add the required DigitalSeal class and other dependencies
//    public class DigitalSeal
//    {
//        public byte[] HeaderAndMessageBytes { get; set; }
//        public byte[] SignatureBytes { get; set; }
//    }

//    // Add the required ECPublicKeyParameters class
//    public class ECPublicKeyParameters
//    {
//        public ECPublicKeyParameters(byte[] q)
//        {
//            Q = q;
//        }

//        public byte[] Q { get; set; }
//        public ECKeyParameters Parameters { get; set; }
//    }

//    // Add the required ECKeyParameters class
//    public class ECKeyParameters
//    {
//        public ECCurve Curve { get; set; }
//    }

//    // Add the required ECCurve class
//    public class ECCurve
//    {
//        public int FieldSize { get; set; }
//    }
//}