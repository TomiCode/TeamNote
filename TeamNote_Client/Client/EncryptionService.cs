using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Bcpg;
using System.IO;
using Org.BouncyCastle.X509;

namespace TeamNote.Client
{
  class EncryptionService
  {
    public EncryptionService()
    {

    }

    public void GenerateKeyPair()
    {
      RsaKeyPairGenerator l_keyGenerator = new RsaKeyPairGenerator();
      l_keyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));

      AsymmetricCipherKeyPair l_keyPair = l_keyGenerator.GenerateKeyPair();

      AsymmetricKeyParameter l_privateKey = l_keyPair.Private;
      RsaKeyParameters publicKeyParam = l_keyPair.Public as RsaKeyParameters;

      string mod = Convert.ToBase64String(publicKeyParam.Modulus.ToByteArray());
      string exp = Convert.ToBase64String(publicKeyParam.Exponent.ToByteArray());

      Debug.Log("Public key Modulus: {0}, Exponent: {1}", mod, exp);

      if (l_privateKey is RsaKeyParameters) {
        //PGP PubKey
        PgpPublicKey pub = new PgpPublicKey(PublicKeyAlgorithmTag.RsaGeneral, l_keyPair.Public, DateTime.Now);

        //PGP PrivKey
        PgpPrivateKey priv = new PgpPrivateKey(pub.KeyId, new PublicKeyPacket(pub.Algorithm, pub.CreationTime, pub.PublicKeyPacket.Key), l_keyPair.Private);

        MemoryStream memStream = new MemoryStream();
        pub.Encode(memStream);

        string keyString = Convert.ToBase64String(memStream.ToArray());
        Debug.Log("PgpPublicKey: {0}", keyString);


        // PgpPublicKey test = new PgpPublicKey(PublicKeyAlgorithmTag.RsaEncrypt, .., DateTime.Now)
      }

      PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(l_keyPair.Private);
      var output = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(l_keyPair.Public);

      Debug.Log("Public key: {0}", Convert.ToBase64String(output.GetEncoded()));


      RsaKeyParameters otherPublicKey = new RsaKeyParameters(false, 
        new Org.BouncyCastle.Math.BigInteger(Convert.FromBase64String(mod)), 
        new Org.BouncyCastle.Math.BigInteger(Convert.FromBase64String(exp)));
      Debug.Log("Batman: {0}", Convert.ToBase64String(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(otherPublicKey).GetEncoded()));
    }
  }
}
