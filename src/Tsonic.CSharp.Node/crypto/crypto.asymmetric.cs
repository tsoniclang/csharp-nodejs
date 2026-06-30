using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The crypto module provides cryptographic functionality.
/// </summary>
public static partial class crypto
{
    /// <summary>
    /// Creates a Sign object.
    /// </summary>
    public static Sign createSign(string algorithm)
    {
        return new Sign(algorithm);
    }

    /// <summary>
    /// Creates a Verify object.
    /// </summary>
    public static Verify createVerify(string algorithm)
    {
        return new Verify(algorithm);
    }

    /// <summary>
    /// Creates a DiffieHellman key exchange object.
    /// </summary>
    public static DiffieHellman createDiffieHellman(int primeLength, int generator = 2)
    {
        return new DiffieHellman(primeLength, generator);
    }

    /// <summary>
    /// Creates an ECDH key exchange object.
    /// </summary>
    public static ECDH createECDH(string curveName)
    {
        return new ECDH(curveName);
    }

    /// <summary>
    /// Generates a new asymmetric key pair.
    /// </summary>
    public static void generateKeyPair(string type, object? options, Action<Exception?, object?, object?> callback)
    {
        try
        {
            var (publicKey, privateKey) = generateKeyPairSync(type, options);
            callback(null, publicKey, privateKey);
        }
        catch (Exception ex)
        {
            callback(ex, null, null);
        }
    }

    /// <summary>
    /// Generates a new asymmetric key pair synchronously.
    /// </summary>
    public static (KeyObject publicKey, KeyObject privateKey) generateKeyPairSync(string type, object? options = null)
    {
        var keyType = type.ToLowerInvariant();

        if (keyType == "rsa")
        {
            // Generate RSA key pair
            using var sourceRsa = RSA.Create(2048); // Default 2048 bits

            // Export key data
            var publicKeyData = sourceRsa.ExportSubjectPublicKeyInfo();
            var privateKeyData = sourceRsa.ExportPkcs8PrivateKey();

            // Create separate RSA instances for public and private keys
            var publicRsa = RSA.Create();
            var privateRsa = RSA.Create();

            publicRsa.ImportSubjectPublicKeyInfo(publicKeyData, out _);
            privateRsa.ImportPkcs8PrivateKey(privateKeyData, out _);

            var publicKey = new PublicKeyObject(publicRsa, "rsa");
            var privateKey = new PrivateKeyObject(privateRsa, "rsa");

            return (publicKey, privateKey);
        }
        else if (keyType == "ec" || keyType == "ecdsa")
        {
            // Generate EC key pair
            using var sourceEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

            // Export key data
            var publicKeyData = sourceEcdsa.ExportSubjectPublicKeyInfo();
            var privateKeyData = sourceEcdsa.ExportPkcs8PrivateKey();

            // Create separate ECDSA instances
            var publicEcdsa = ECDsa.Create();
            var privateEcdsa = ECDsa.Create();

            publicEcdsa.ImportSubjectPublicKeyInfo(publicKeyData, out _);
            privateEcdsa.ImportPkcs8PrivateKey(privateKeyData, out _);

            var publicKey = new PublicKeyObject(publicEcdsa, "ec");
            var privateKey = new PrivateKeyObject(privateEcdsa, "ec");

            return (publicKey, privateKey);
        }
        else if (keyType == "ed25519" || keyType == "ed448" || keyType == "x25519" || keyType == "x448")
        {
            // Generate EdDSA key pair using BouncyCastle
            var algorithm = keyType switch
            {
                "ed25519" => "Ed25519",
                "ed448" => "Ed448",
                "x25519" => "X25519",
                "x448" => "X448",
                _ => throw new ArgumentException($"Unknown key type: {type}")
            };

            var keyPairGenerator = GeneratorUtilities.GetKeyPairGenerator(algorithm);
            keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 256));
            var bcKeyPair = keyPairGenerator.GenerateKeyPair();

            var publicKey = new EdDSAPublicKeyObject(bcKeyPair.Public, keyType);
            var privateKey = new EdDSAPrivateKeyObject(bcKeyPair.Private, keyType);

            return (publicKey, privateKey);
        }
        else if (keyType == "dsa")
        {
            // Generate DSA key pair using BouncyCastle
            var dsaGen = new Org.BouncyCastle.Crypto.Generators.DsaKeyPairGenerator();
            var dsaParams = new Org.BouncyCastle.Crypto.Generators.DsaParametersGenerator();
            dsaParams.Init(1024, 80, new SecureRandom()); // BouncyCastle requires 512-1024 range
            var parameters = dsaParams.GenerateParameters();

            dsaGen.Init(new Org.BouncyCastle.Crypto.Parameters.DsaKeyGenerationParameters(new SecureRandom(), parameters));
            var bcKeyPair = dsaGen.GenerateKeyPair();

            var publicKey = new DSAPublicKeyObject(bcKeyPair.Public);
            var privateKey = new DSAPrivateKeyObject(bcKeyPair.Private);

            return (publicKey, privateKey);
        }
        else if (keyType == "dh")
        {
            // Use a standard 2048-bit MODP group for the default DH keypair path.
            // Generating fresh 2048-bit DH parameters here is unnecessarily expensive
            // and makes the synchronous API unstable under full-suite execution.
            var dh = getDiffieHellman("modp14");
            dh.generateKeys();

            // Export keys (this is a simplification - real implementation would need proper KeyObject wrappers)
            var publicKeyBytes = dh.getPublicKey();
            var privateKeyBytes = dh.getPrivateKey();

            // For now, return as secret keys since DH doesn't fit the asymmetric key model cleanly
            var publicKey = new SecretKeyObject(publicKeyBytes);
            var privateKey = new SecretKeyObject(privateKeyBytes);

            return ((KeyObject)publicKey, (KeyObject)privateKey);
        }

        throw new ArgumentException($"Unknown key type: {type}");
    }

    /// <summary>
    /// Encrypts data with a public key.
    /// </summary>
    public static byte[] publicEncrypt(string key, byte[] buffer)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(key);
        return rsa.Encrypt(buffer, RSAEncryptionPadding.OaepSHA256);
    }

    /// <summary>
    /// Encrypts data with a public key.
    /// </summary>
    public static byte[] publicEncrypt(object key, byte[] buffer)
    {
        if (key is KeyObject keyObj)
        {
            if (keyObj.type != "public")
                throw new ArgumentException("Key must be a public key");

            var pemKey = keyObj.export().ToString();
            return publicEncrypt(pemKey!, buffer);
        }

        throw new ArgumentException("Invalid key format");
    }

    /// <summary>
    /// Decrypts data with a private key.
    /// </summary>
    public static byte[] privateDecrypt(string key, byte[] buffer)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(key);
        return rsa.Decrypt(buffer, RSAEncryptionPadding.OaepSHA256);
    }

    /// <summary>
    /// Decrypts data with a private key.
    /// </summary>
    public static byte[] privateDecrypt(object key, byte[] buffer)
    {
        if (key is KeyObject keyObj)
        {
            if (keyObj.type != "private")
                throw new ArgumentException("Key must be a private key");

            var pemKey = keyObj.export().ToString();
            return privateDecrypt(pemKey!, buffer);
        }

        throw new ArgumentException("Invalid key format");
    }

    /// <summary>
    /// Decrypts data with a public key.
    /// </summary>
    public static byte[] publicDecrypt(string key, byte[] buffer)
    {
        // Public key decryption - used for verifying data encrypted with private key
        // This is the inverse of private key encryption (privateEncrypt)
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(key);

            // Use Decrypt with PKCS1 padding (acts as "public decrypt")
            // Note: This will fail with standard .NET RSA because Decrypt requires private key
            // We need to use a workaround or BouncyCastle
            return PublicDecryptWithBouncyCastle(key, buffer);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Public decrypt failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Decrypts data with a public key.
    /// </summary>
    public static byte[] publicDecrypt(object key, byte[] buffer)
    {
        if (key is not PublicKeyObject keyObject)
            throw new ArgumentException("key must be a PublicKeyObject", nameof(key));

        var keyPem = keyObject.export() as string ?? throw new InvalidOperationException("Failed to export public key");
        return publicDecrypt(keyPem, buffer);
    }

    private static byte[] PublicDecryptWithBouncyCastle(string publicKeyPem, byte[] buffer)
    {
        using var reader = new StringReader(publicKeyPem);
        var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(reader);
        var keyObject = pemReader.ReadObject();

        Org.BouncyCastle.Crypto.AsymmetricKeyParameter publicKey = keyObject switch
        {
            Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair => keyPair.Public,
            Org.BouncyCastle.Crypto.AsymmetricKeyParameter key => key,
            _ => throw new ArgumentException("Invalid public key format")
        };

        var engine = new Org.BouncyCastle.Crypto.Encodings.Pkcs1Encoding(new Org.BouncyCastle.Crypto.Engines.RsaEngine());
        engine.Init(false, publicKey); // false = decrypt mode

        return engine.ProcessBlock(buffer, 0, buffer.Length);
    }

    /// <summary>
    /// Encrypts data with a private key.
    /// </summary>
    public static byte[] privateEncrypt(string key, byte[] buffer)
    {
        // Private key encryption - inverse of public key decryption
        // This is used for creating signatures (without hashing)
        try
        {
            return PrivateEncryptWithBouncyCastle(key, buffer);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Private encrypt failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Encrypts data with a private key.
    /// </summary>
    public static byte[] privateEncrypt(object key, byte[] buffer)
    {
        if (key is not PrivateKeyObject keyObject)
            throw new ArgumentException("key must be a PrivateKeyObject", nameof(key));

        var keyPem = keyObject.export() as string ?? throw new InvalidOperationException("Failed to export private key");
        return privateEncrypt(keyPem, buffer);
    }

    private static byte[] PrivateEncryptWithBouncyCastle(string privateKeyPem, byte[] buffer)
    {
        using var reader = new StringReader(privateKeyPem);
        var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(reader);
        var keyObject = pemReader.ReadObject();

        Org.BouncyCastle.Crypto.AsymmetricKeyParameter privateKey = keyObject switch
        {
            Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair => keyPair.Private,
            Org.BouncyCastle.Crypto.AsymmetricKeyParameter key => key,
            _ => throw new ArgumentException("Invalid private key format")
        };

        var engine = new Org.BouncyCastle.Crypto.Encodings.Pkcs1Encoding(new Org.BouncyCastle.Crypto.Engines.RsaEngine());
        engine.Init(true, privateKey); // true = encrypt mode

        return engine.ProcessBlock(buffer, 0, buffer.Length);
    }

}
