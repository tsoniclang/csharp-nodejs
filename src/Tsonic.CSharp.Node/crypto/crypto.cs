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
    /// Creates and returns a Hash object that can be used to generate hash digests.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'sha256', 'md5').</param>
    /// <returns>A Hash object.</returns>
    public static Hash createHash(string algorithm)
    {
        return new Hash(algorithm);
    }

    /// <summary>
    /// Creates and returns an Hmac object that uses the given algorithm and key.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'sha256').</param>
    /// <param name="key">The HMAC key.</param>
    /// <returns>An Hmac object.</returns>
    public static Hmac createHmac(string algorithm, string key)
    {
        return new Hmac(algorithm, Encoding.UTF8.GetBytes(key));
    }

    /// <summary>
    /// Creates and returns an Hmac object that uses the given algorithm and key.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'sha256').</param>
    /// <param name="key">The HMAC key as a byte array.</param>
    /// <returns>An Hmac object.</returns>
    public static Hmac createHmac(string algorithm, byte[] key)
    {
        return new Hmac(algorithm, key);
    }

    /// <summary>
    /// Creates and returns a Cipher object with the given algorithm, key, and initialization vector.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'aes-256-cbc').</param>
    /// <param name="key">The encryption key.</param>
    /// <param name="iv">The initialization vector.</param>
    /// <returns>A Cipher object.</returns>
    public static Cipher createCipheriv(string algorithm, byte[] key, byte[]? iv)
    {
        return new Cipher(algorithm, key, iv);
    }

    /// <summary>
    /// Creates and returns a Cipher object with the given algorithm, key, and initialization vector.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'aes-256-cbc').</param>
    /// <param name="key">The encryption key as a string.</param>
    /// <param name="iv">The initialization vector as a string.</param>
    /// <returns>A Cipher object.</returns>
    public static Cipher createCipheriv(string algorithm, string key, string? iv)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var ivBytes = iv != null ? Encoding.UTF8.GetBytes(iv) : null;
        return new Cipher(algorithm, keyBytes, ivBytes);
    }

    /// <summary>
    /// Creates and returns a Decipher object with the given algorithm, key, and initialization vector.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'aes-256-cbc').</param>
    /// <param name="key">The decryption key.</param>
    /// <param name="iv">The initialization vector.</param>
    /// <returns>A Decipher object.</returns>
    public static Decipher createDecipheriv(string algorithm, byte[] key, byte[]? iv)
    {
        return new Decipher(algorithm, key, iv);
    }

    /// <summary>
    /// Creates and returns a Decipher object with the given algorithm, key, and initialization vector.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'aes-256-cbc').</param>
    /// <param name="key">The decryption key as a string.</param>
    /// <param name="iv">The initialization vector as a string.</param>
    /// <returns>A Decipher object.</returns>
    public static Decipher createDecipheriv(string algorithm, string key, string? iv)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var ivBytes = iv != null ? Encoding.UTF8.GetBytes(iv) : null;
        return new Decipher(algorithm, keyBytes, ivBytes);
    }

    /// <summary>
    /// Generates cryptographically strong pseudo-random data.
    /// </summary>
    /// <param name="size">The number of bytes to generate.</param>
    /// <returns>A byte array containing random bytes.</returns>
    public static byte[] randomBytes(int size)
    {
        var bytes = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return bytes;
    }

    /// <summary>
    /// Generates cryptographically strong pseudo-random data asynchronously.
    /// </summary>
    /// <param name="size">The number of bytes to generate.</param>
    /// <param name="callback">Callback function.</param>
    public static void randomBytes(int size, Action<Exception?, byte[]?> callback)
    {
        try
        {
            var bytes = randomBytes(size);
            callback(null, bytes);
        }
        catch (Exception ex)
        {
            callback(ex, null);
        }
    }

    /// <summary>
    /// Generates a random integer.
    /// </summary>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <returns>A random integer between 0 and max.</returns>
    public static int randomInt(int max)
    {
        return RandomNumberGenerator.GetInt32(max);
    }

    /// <summary>
    /// Generates a random integer.
    /// </summary>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <returns>A random integer between min and max.</returns>
    public static int randomInt(int min, int max)
    {
        return RandomNumberGenerator.GetInt32(min, max);
    }

    /// <summary>
    /// Fills a buffer with random bytes.
    /// </summary>
    /// <param name="buffer">The buffer to fill.</param>
    /// <param name="offset">The offset to start filling.</param>
    /// <param name="size">The number of bytes to fill.</param>
    /// <returns>The filled buffer.</returns>
    public static byte[] randomFillSync(byte[] buffer, int offset = 0, int? size = null)
    {
        var actualSize = size ?? (buffer.Length - offset);
        var bytes = randomBytes(actualSize);
        Array.Copy(bytes, 0, buffer, offset, actualSize);
        return buffer;
    }

    /// <summary>
    /// Fills a buffer with random bytes asynchronously.
    /// </summary>
    /// <param name="buffer">The buffer to fill.</param>
    /// <param name="offset">The offset to start filling.</param>
    /// <param name="size">The number of bytes to fill.</param>
    /// <param name="callback">Callback function.</param>
    public static void randomFill(byte[] buffer, int offset, int size, Action<Exception?, byte[]?> callback)
    {
        try
        {
            randomFillSync(buffer, offset, size);
            callback(null, buffer);
        }
        catch (Exception ex)
        {
            callback(ex, null);
        }
    }

    /// <summary>
    /// Generates a UUID v4.
    /// </summary>
    /// <returns>A UUID string.</returns>
    public static string randomUUID()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Provides an asynchronous Password-Based Key Derivation Function 2 (PBKDF2) implementation.
    /// </summary>
    public static byte[] pbkdf2Sync(string password, string salt, int iterations, int keylen, string digest)
    {
        return pbkdf2Sync(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt), iterations, keylen, digest);
    }

    /// <summary>
    /// Provides an asynchronous Password-Based Key Derivation Function 2 (PBKDF2) implementation.
    /// </summary>
    public static byte[] pbkdf2Sync(byte[] password, byte[] salt, int iterations, int keylen, string digest)
    {
        var hashAlgorithm = GetHashAlgorithmName(digest);
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keylen);
    }

    /// <summary>
    /// Provides an asynchronous Password-Based Key Derivation Function 2 (PBKDF2) implementation.
    /// </summary>
    public static void pbkdf2(string password, string salt, int iterations, int keylen, string digest, Action<Exception?, byte[]?> callback)
    {
        try
        {
            var result = pbkdf2Sync(password, salt, iterations, keylen, digest);
            callback(null, result);
        }
        catch (Exception ex)
        {
            callback(ex, null);
        }
    }

    /// <summary>
    /// Provides a synchronous scrypt implementation.
    /// </summary>
    public static byte[] scryptSync(string password, string salt, int keylen, object? options = null)
    {
        return scryptSync(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt), keylen, options);
    }

    /// <summary>
    /// Provides a synchronous scrypt implementation.
    /// </summary>
    public static byte[] scryptSync(byte[] password, byte[] salt, int keylen, object? options = null)
    {
        // Default scrypt parameters
        int N = 16384;  // CPU/memory cost parameter (must be power of 2)
        int r = 8;      // Block size parameter
        int p = 1;      // Parallelization parameter

        // Parse options if provided
        if (options != null)
        {
            var optionsDict = options as System.Collections.Generic.Dictionary<string, object>;
            if (optionsDict != null)
            {
                if (optionsDict.TryGetValue("N", out var nValue))
                    N = Convert.ToInt32(nValue);
                if (optionsDict.TryGetValue("cost", out var costValue))
                    N = Convert.ToInt32(costValue);
                if (optionsDict.TryGetValue("r", out var rValue))
                    r = Convert.ToInt32(rValue);
                if (optionsDict.TryGetValue("blockSize", out var blockSizeValue))
                    r = Convert.ToInt32(blockSizeValue);
                if (optionsDict.TryGetValue("p", out var pValue))
                    p = Convert.ToInt32(pValue);
                if (optionsDict.TryGetValue("parallelization", out var parallelizationValue))
                    p = Convert.ToInt32(parallelizationValue);
            }
        }

        return SCrypt.Generate(password, salt, N, r, p, keylen);
    }

    /// <summary>
    /// Provides an asynchronous scrypt implementation.
    /// </summary>
    public static void scrypt(string password, string salt, int keylen, object? options, Action<Exception?, byte[]?> callback)
    {
        try
        {
            var result = scryptSync(password, salt, keylen, options);
            callback(null, result);
        }
        catch (Exception ex)
        {
            callback(ex, null);
        }
    }

    /// <summary>
    /// Returns an array of the names of the supported cipher algorithms.
    /// </summary>
    /// <returns>An array of cipher algorithm names.</returns>
    public static string[] getCiphers()
    {
        return new[]
        {
            "aes-128-cbc", "aes-128-ecb", "aes-128-cfb",
            "aes-192-cbc", "aes-192-ecb", "aes-192-cfb",
            "aes-256-cbc", "aes-256-ecb", "aes-256-cfb",
            "des-cbc", "des-ecb",
            "des-ede3-cbc", "des-ede3-ecb",
            "rc2-cbc", "rc2-ecb"
        };
    }

    /// <summary>
    /// Returns an array of the names of the supported hash algorithms.
    /// </summary>
    /// <returns>An array of hash algorithm names.</returns>
    public static string[] getHashes()
    {
        return new[]
        {
            "md5",
            "sha1", "sha256", "sha384", "sha512"
        };
    }

    /// <summary>
    /// Returns an array of the names of the supported elliptic curves.
    /// </summary>
    /// <returns>An array of curve names.</returns>
    public static string[] getCurves()
    {
        return new[]
        {
            "secp256r1", "secp384r1", "secp521r1",
            "secp256k1",
            "ed25519", "ed448",
            "x25519", "x448"
        };
    }

    /// <summary>
    /// Test for equality in constant time.
    /// </summary>
    /// <param name="a">First buffer.</param>
    /// <param name="b">Second buffer.</param>
    /// <returns>True if the buffers are equal.</returns>
    public static bool timingSafeEqual(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        return result == 0;
    }

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

    /// <summary>
    /// Creates a KeyObject from a secret key.
    /// </summary>
    public static KeyObject createSecretKey(byte[] key)
    {
        return new SecretKeyObject(key);
    }

    /// <summary>
    /// Creates a KeyObject from a secret key.
    /// </summary>
    public static KeyObject createSecretKey(string key, string? encoding = null)
    {
        var enc = (encoding ?? "utf8").ToLowerInvariant();
        var keyBytes = enc switch
        {
            "hex" => Convert.FromHexString(key.Replace("-", "")),
            "base64" => Convert.FromBase64String(key),
            "base64url" => DecodeBase64Url(key),
            "utf8" or "utf-8" => Encoding.UTF8.GetBytes(key),
            "ascii" => Encoding.ASCII.GetBytes(key),
            "latin1" or "binary" => Encoding.Latin1.GetBytes(key),
            _ => Encoding.UTF8.GetBytes(key)
        };
        return new SecretKeyObject(keyBytes);
    }

    private static byte[] DecodeBase64Url(string base64url)
    {
        var base64 = base64url.Replace("-", "+").Replace("_", "/");
        // Add padding
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    /// <summary>
    /// Creates a public KeyObject from a key.
    /// </summary>
    public static KeyObject createPublicKey(string key)
    {
        try
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(key);
            return new PublicKeyObject(rsa, "rsa");
        }
        catch
        {
            try
            {
                var ecdsa = ECDsa.Create();
                ecdsa.ImportFromPem(key);
                return new PublicKeyObject(ecdsa, "ec");
            }
            catch
            {
                throw new ArgumentException("Unable to parse public key");
            }
        }
    }

    /// <summary>
    /// Creates a public KeyObject from a key.
    /// </summary>
    public static KeyObject createPublicKey(byte[] key)
    {
        try
        {
            var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(key, out _);
            return new PublicKeyObject(rsa, "rsa");
        }
        catch
        {
            try
            {
                var ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(key, out _);
                return new PublicKeyObject(ecdsa, "ec");
            }
            catch
            {
                throw new ArgumentException("Unable to parse public key");
            }
        }
    }

    /// <summary>
    /// Creates a public KeyObject from another KeyObject.
    /// </summary>
    public static KeyObject createPublicKey(KeyObject key)
    {
        if (key.type == "public")
            return key;

        if (key.type != "private")
            throw new ArgumentException("Key must be a private or public key");

        // Extract public key from private key
        var exported = key.export();
        if (exported is string pemKey)
        {
            return createPublicKey(pemKey);
        }

        throw new ArgumentException("Unable to extract public key");
    }

    /// <summary>
    /// Creates a private KeyObject from a key.
    /// </summary>
    public static KeyObject createPrivateKey(string key)
    {
        try
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(key);
            return new PrivateKeyObject(rsa, "rsa");
        }
        catch
        {
            try
            {
                var ecdsa = ECDsa.Create();
                ecdsa.ImportFromPem(key);
                return new PrivateKeyObject(ecdsa, "ec");
            }
            catch
            {
                throw new ArgumentException("Unable to parse private key");
            }
        }
    }

    /// <summary>
    /// Creates a private KeyObject from a key.
    /// </summary>
    public static KeyObject createPrivateKey(byte[] key)
    {
        try
        {
            var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(key, out _);
            return new PrivateKeyObject(rsa, "rsa");
        }
        catch
        {
            try
            {
                var ecdsa = ECDsa.Create();
                ecdsa.ImportPkcs8PrivateKey(key, out _);
                return new PrivateKeyObject(ecdsa, "ec");
            }
            catch
            {
                throw new ArgumentException("Unable to parse private key");
            }
        }
    }

    /// <summary>
    /// Generates a symmetric key for the specified algorithm.
    /// </summary>
    public static KeyObject generateKey(string type, object options)
    {
        // Default lengths for common algorithms (in bytes)
        int length = type.ToLowerInvariant() switch
        {
            "aes" or "aes-256-cbc" or "aes-256-gcm" => 32, // 256 bits
            "aes-192-cbc" or "aes-192-gcm" => 24, // 192 bits
            "aes-128-cbc" or "aes-128-gcm" => 16, // 128 bits
            "hmac" => 64, // 512 bits for HMAC
            _ => 32 // Default to 256 bits
        };

        // Generate random bytes
        var keyBytes = new byte[length];
        RandomNumberGenerator.Fill(keyBytes);

        return createSecretKey(keyBytes);
    }

    /// <summary>
    /// Generates a deterministic private key from a password and salt (async).
    /// </summary>
    public static void generateKey(string type, object options, Action<Exception?, KeyObject?> callback)
    {
        try
        {
            var key = generateKey(type, options);
            callback(null, key);
        }
        catch (Exception ex)
        {
            callback(ex, null);
        }
    }

    /// <summary>
    /// Derives a key using the HKDF algorithm.
    /// </summary>
    public static byte[] hkdfSync(string digest, byte[] ikm, byte[] salt, byte[] info, int keylen)
    {
        var hashAlgorithm = GetHashAlgorithmName(digest);
        return HKDF.DeriveKey(hashAlgorithm, ikm, keylen, salt, info);
    }

    /// <summary>
    /// Derives a key using the HKDF algorithm (async).
    /// </summary>
    public static void hkdf(string digest, byte[] ikm, byte[] salt, byte[] info, int keylen, Action<Exception?, byte[]?> callback)
    {
        try
        {
            var key = hkdfSync(digest, ikm, salt, info, keylen);
            callback(null, key);
        }
        catch (Exception ex)
        {
            callback(ex, null);
        }
    }

    /// <summary>
    /// Signs data using a private key.
    /// </summary>
    public static byte[] sign(string? algorithm, byte[] data, string privateKey)
    {
        var sign = createSign(algorithm ?? "sha256");
        sign.update(data);
        return sign.sign(privateKey);
    }

    /// <summary>
    /// Signs data using a private key.
    /// </summary>
    public static byte[] sign(string? algorithm, byte[] data, KeyObject privateKey)
    {
        var sign = createSign(algorithm ?? "sha256");
        sign.update(data);
        return sign.sign(privateKey);
    }

    /// <summary>
    /// Verifies a signature using a public key.
    /// </summary>
    public static bool verify(string? algorithm, byte[] data, string publicKey, byte[] signature)
    {
        var verify = createVerify(algorithm ?? "sha256");
        verify.update(data);
        return verify.verify(publicKey, signature);
    }

    /// <summary>
    /// Verifies a signature using a public key.
    /// </summary>
    public static bool verify(string? algorithm, byte[] data, KeyObject publicKey, byte[] signature)
    {
        var verify = createVerify(algorithm ?? "sha256");
        verify.update(data);
        return verify.verify(publicKey, signature);
    }

    /// <summary>
    /// Computes a hash of the given data.
    /// </summary>
    public static byte[] hash(string algorithm, byte[] data, string? outputEncoding = null)
    {
        var hash = createHash(algorithm);
        hash.update(data);
        return hash.digest();
    }

    /// <summary>
    /// Returns the default cipher list.
    /// </summary>
    public static string getDefaultCipherList()
    {
        return string.Join(":", getCiphers());
    }

    /// <summary>
    /// Gets the Diffie-Hellman group.
    /// </summary>
    public static DiffieHellman getDiffieHellman(string groupName)
    {
        // Predefined DH groups from RFC 3526 and RFC 2409
        if (!MODPGroups.IsValidGroup(groupName))
        {
            throw new ArgumentException($"Unknown DH group: {groupName}");
        }

        var (prime, generator) = MODPGroups.GetGroup(groupName);
        return new DiffieHellman(prime, generator);
    }

    /// <summary>
    /// Creates a DiffieHellman instance with a prime.
    /// </summary>
    public static DiffieHellman createDiffieHellman(byte[] prime, byte[] generator)
    {
        return new DiffieHellman(prime, generator);
    }

    /// <summary>
    /// Creates a DiffieHellman instance with a prime.
    /// </summary>
    public static DiffieHellman createDiffieHellman(byte[] prime, int generator = 2)
    {
        return new DiffieHellman(prime, generator);
    }

    /// <summary>
    /// Creates a DiffieHellman instance with a prime.
    /// </summary>
    public static DiffieHellman createDiffieHellman(string prime, string primeEncoding, int generator = 2)
    {
        var enc = primeEncoding.ToLowerInvariant();
        var primeBytes = enc switch
        {
            "hex" => Convert.FromHexString(prime.Replace("-", "")),
            "base64" => Convert.FromBase64String(prime),
            _ => Encoding.UTF8.GetBytes(prime)
        };
        return new DiffieHellman(primeBytes, generator);
    }

    /// <summary>
    /// Creates a DiffieHellman instance with a prime.
    /// </summary>
    public static DiffieHellman createDiffieHellman(string prime, string primeEncoding, string generator, string generatorEncoding)
    {
        var pEnc = primeEncoding.ToLowerInvariant();
        var gEnc = generatorEncoding.ToLowerInvariant();

        var primeBytes = pEnc switch
        {
            "hex" => Convert.FromHexString(prime.Replace("-", "")),
            "base64" => Convert.FromBase64String(prime),
            _ => Encoding.UTF8.GetBytes(prime)
        };

        var generatorBytes = gEnc switch
        {
            "hex" => Convert.FromHexString(generator.Replace("-", "")),
            "base64" => Convert.FromBase64String(generator),
            _ => Encoding.UTF8.GetBytes(generator)
        };

        return new DiffieHellman(primeBytes, generatorBytes);
    }

    /// <summary>
    /// Sets the default encoding for crypto operations.
    /// </summary>
    public static void setDefaultEncoding(string encoding)
    {
        // This is a legacy Node.js API that's deprecated
        // We'll just ignore it for now
    }

    /// <summary>
    /// Gets the fips mode status (always false in .NET).
    /// </summary>
    public static bool getFips()
    {
        // .NET uses CryptoConfig but doesn't have a FIPS mode flag
        return false;
    }

    /// <summary>
    /// Sets the fips mode (not supported in .NET).
    /// </summary>
    public static void setFips(bool enabled)
    {
        if (enabled)
        {
            throw new NotSupportedException("FIPS mode is not directly configurable in .NET. Use system-level FIPS policy instead.");
        }
    }

    private static HashAlgorithmName GetHashAlgorithmName(string digest)
    {
        return digest.ToLowerInvariant() switch
        {
            "sha1" => HashAlgorithmName.SHA1,
            "sha256" => HashAlgorithmName.SHA256,
            "sha384" => HashAlgorithmName.SHA384,
            "sha512" => HashAlgorithmName.SHA512,
            "md5" => HashAlgorithmName.MD5,
            _ => throw new ArgumentException($"Unsupported digest algorithm: {digest}")
        };
    }
}
