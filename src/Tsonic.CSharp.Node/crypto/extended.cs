#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Tsonic.CSharp.Node;

public sealed class DigestResult
{
    public DigestResult(byte[] bytes)
    {
        this.bytes = bytes;
        hex = Convert.ToHexString(bytes).ToLowerInvariant();
        @base64 = Convert.ToBase64String(bytes);
    }

    public byte[] bytes { get; }

    public string hex { get; }

    public string @base64 { get; }
}

public sealed class SecureHeapUsage
{
    public long total { get; set; }

    public long used { get; set; }

    public double utilization { get; set; }

    public long min { get; set; }
}

public sealed class AesGcmCiphertext
{
    public byte[] ciphertext { get; set; } = [];

    public byte[] authTag { get; set; } = [];

    public byte[] nonce { get; set; } = [];
}

public sealed class RsaKeyPair
{
    public RsaKeyPair(KeyObject publicKey, KeyObject privateKey)
    {
        this.publicKey = publicKey;
        this.privateKey = privateKey;
    }

    public KeyObject publicKey { get; }

    public KeyObject privateKey { get; }
}

public sealed class CryptoConstants
{
    public int DH_CHECK_P_NOT_PRIME { get; init; } = 1;
    public int DH_CHECK_P_NOT_SAFE_PRIME { get; init; } = 2;
    public int DH_UNABLE_TO_CHECK_GENERATOR { get; init; } = 4;
    public int DH_NOT_SUITABLE_GENERATOR { get; init; } = 8;
    public int ENGINE_METHOD_NONE { get; init; }
    public int ENGINE_METHOD_RSA { get; init; } = 1;
    public int ENGINE_METHOD_DSA { get; init; } = 2;
    public int ENGINE_METHOD_DH { get; init; } = 4;
    public int ENGINE_METHOD_RAND { get; init; } = 8;
    public int ENGINE_METHOD_EC { get; init; } = 2048;
    public int ENGINE_METHOD_CIPHERS { get; init; } = 64;
    public int ENGINE_METHOD_DIGESTS { get; init; } = 128;
    public int ENGINE_METHOD_PKEY_METHS { get; init; } = 512;
    public int ENGINE_METHOD_PKEY_ASN1_METHS { get; init; } = 1024;
    public int ENGINE_METHOD_ALL { get; init; } = 65535;
    public int OPENSSL_VERSION_NUMBER { get; init; } = 0x30000000;
    public int POINT_CONVERSION_COMPRESSED { get; init; } = 2;
    public int POINT_CONVERSION_UNCOMPRESSED { get; init; } = 4;
    public int POINT_CONVERSION_HYBRID { get; init; } = 6;
    public int RSA_PKCS1_PADDING { get; init; } = 1;
    public int RSA_SSLV23_PADDING { get; init; } = 2;
    public int RSA_NO_PADDING { get; init; } = 3;
    public int RSA_PKCS1_OAEP_PADDING { get; init; } = 4;
    public int RSA_X931_PADDING { get; init; } = 5;
    public int RSA_PKCS1_PSS_PADDING { get; init; } = 6;
    public int RSA_PSS_SALTLEN_DIGEST { get; init; } = -1;
    public int RSA_PSS_SALTLEN_MAX_SIGN { get; init; } = -2;
    public int RSA_PSS_SALTLEN_AUTO { get; init; } = -2;
    public int SSL_OP_ALL { get; init; } = unchecked((int)0x80000BFF);
    public int SSL_OP_NO_SSLv2 { get; init; } = 0;
    public int SSL_OP_NO_SSLv3 { get; init; } = 0x02000000;
    public int SSL_OP_NO_TLSv1 { get; init; } = 0x04000000;
    public int SSL_OP_NO_TLSv1_1 { get; init; } = 0x10000000;
    public int SSL_OP_NO_TLSv1_2 { get; init; } = 0x08000000;
    public int SSL_OP_NO_TLSv1_3 { get; init; } = 0x20000000;
    public int SSL_OP_NO_COMPRESSION { get; init; } = 0x00020000;
    public int SSL_OP_NO_TICKET { get; init; } = 0x00004000;
    public int SSL_OP_CIPHER_SERVER_PREFERENCE { get; init; } = 0x00400000;
    public string defaultCipherList { get; init; } = "TLS_AES_256_GCM_SHA384:TLS_CHACHA20_POLY1305_SHA256:TLS_AES_128_GCM_SHA256";
    public string defaultCoreCipherList { get; init; } = "TLS_AES_256_GCM_SHA384:TLS_CHACHA20_POLY1305_SHA256:TLS_AES_128_GCM_SHA256";
}

public sealed class RandomUUIDOptions { public bool disableEntropyCache { get; set; } }
public sealed class HashOptions { public int? outputLength { get; set; } }
public sealed class OneShotDigestOptions { public int? outputLength { get; set; } public string? outputEncoding { get; set; } }
public sealed class CipherInfoOptions { public int? keyLength { get; set; } public int? ivLength { get; set; } }
public sealed class AsymmetricKeyDetails { public int? modulusLength { get; set; } public long? publicExponent { get; set; } public string? hashAlgorithm { get; set; } public string? mgf1HashAlgorithm { get; set; } public int? saltLength { get; set; } public string? namedCurve { get; set; } public int? divisorLength { get; set; } }
public class KeyExportOptions { public string? format { get; set; } public string? type { get; set; } public object? passphrase { get; set; } public string? encoding { get; set; } }
public sealed class PrivateKeyExportOptions : KeyExportOptions { public string? cipher { get; set; } }
public sealed class PublicKeyExportOptions : KeyExportOptions { }
public sealed class SymmetricKeyExportOptions : KeyExportOptions { }
public sealed class KeyExportResult { public object? value { get; set; } }
public sealed class JsonWebKey { public string? kty { get; set; } public string? crv { get; set; } public string? x { get; set; } public string? y { get; set; } public string? d { get; set; } public string? n { get; set; } public string? e { get; set; } public string? k { get; set; } public string? alg { get; set; } public string[]? key_ops { get; set; } public bool? ext { get; set; } public string? p { get; set; } public string? q { get; set; } public string? dp { get; set; } public string? dq { get; set; } public string? qi { get; set; } public RsaOtherPrimesInfo[]? oth { get; set; } public string? use { get; set; } }
public sealed class JsonWebKeyInput { public JsonWebKey? key { get; set; } }
public sealed class JwkKeyExportOptions { public string format { get; set; } = "jwk"; }
public sealed class SigningOptions { public int? padding { get; set; } public int? saltLength { get; set; } public string? dsaEncoding { get; set; } public object? context { get; set; } }
public sealed class VerifyKeyObjectInput { public KeyObject? key { get; set; } public int? padding { get; set; } public int? saltLength { get; set; } }
public sealed class Pbkdf2Params { public byte[] password { get; set; } = []; public byte[] salt { get; set; } = []; public int iterations { get; set; } public int keylen { get; set; } public string digest { get; set; } = "sha256"; }
public sealed class HkdfParams { public byte[] inputKeyingMaterial { get; set; } = []; public byte[] salt { get; set; } = []; public byte[] info { get; set; } = []; public int keylen { get; set; } }
public sealed class ScryptOptions { public int? cost { get; set; } public int? N { get; set; } public int? blockSize { get; set; } public int? r { get; set; } public int? parallelization { get; set; } public int? p { get; set; } public int? maxmem { get; set; } }
public class KeyAlgorithm { public string name { get; set; } = string.Empty; }
public class RsaKeyAlgorithm : KeyAlgorithm { public int modulusLength { get; set; } public byte[] publicExponent { get; set; } = [1, 0, 1]; }
public class RsaHashedKeyAlgorithm : KeyAlgorithm { public RsaKeyAlgorithm? rsa { get; set; } public string hash { get; set; } = "SHA-256"; }
public class HmacKeyAlgorithm : KeyAlgorithm { public int length { get; set; } }
public sealed class AesKeyAlgorithm : KeyAlgorithm { public int length { get; set; } }
public sealed class AesKeyGenParams : KeyAlgorithm { public int length { get; set; } }
public sealed class AesDerivedKeyParams : KeyAlgorithm { public int length { get; set; } }
public sealed class AesCbcParams : KeyAlgorithm { public byte[] iv { get; set; } = []; }
public sealed class RsaKeyGenParams : RsaKeyAlgorithm { }
public sealed class RsaHashedKeyGenParams : RsaHashedKeyAlgorithm { }
public sealed class HmacKeyGenParams : HmacKeyAlgorithm { }
public sealed class CryptoKey { public bool extractable { get; set; } public KeyAlgorithm algorithm { get; set; } = new(); public string[] usages { get; set; } = []; public byte[] data { get; set; } = []; }
public sealed class CryptoKeyPair { public CryptoKey? publicKey { get; set; } public CryptoKey? privateKey { get; set; } }
public sealed class AeadParams : KeyAlgorithm { public byte[] iv { get; set; } = []; public byte[]? additionalData { get; set; } public int? tagLength { get; set; } }
public sealed class AesCtrParams : KeyAlgorithm { public byte[] counter { get; set; } = []; public int length { get; set; } }
public sealed class RsaPssParams : KeyAlgorithm { public int saltLength { get; set; } }
public sealed class RsaOaepParams : KeyAlgorithm { public byte[]? label { get; set; } }
public sealed class EcdsaParams : KeyAlgorithm { public string hash { get; set; } = "SHA-256"; }
public sealed class EcdhKeyDeriveParams : KeyAlgorithm { public CryptoKey? @public { get; set; } }
public sealed class RsaOtherPrimesInfo { public string? r { get; set; } public string? d { get; set; } public string? t { get; set; } }
public sealed class PrivateKeyInput { public object? key { get; set; } public string? format { get; set; } public string? encoding { get; set; } public string? passphrase { get; set; } }
public sealed class PublicKeyInput { public object? key { get; set; } public string? format { get; set; } public string? encoding { get; set; } }
public sealed class RsaPrivateKey { public object? key { get; set; } public int? padding { get; set; } public string? oaepHash { get; set; } public byte[]? oaepLabel { get; set; } public string? passphrase { get; set; } }
public sealed class RsaPublicKey { public object? key { get; set; } public int? padding { get; set; } }
public sealed class X509CheckOptions { public bool wildcards { get; set; } = true; public bool partialWildcards { get; set; } = true; public bool multiLabelWildcards { get; set; } public bool singleLabelSubdomains { get; set; } }

public sealed class X509CertificateLegacyObject
{
    public string subject { get; set; } = string.Empty;
    public string issuer { get; set; } = string.Empty;
    public string valid_from { get; set; } = string.Empty;
    public string valid_to { get; set; } = string.Empty;
    public string fingerprint { get; set; } = string.Empty;
}

public sealed class X509Certificate
{
    private readonly X509Certificate2 _certificate;

    public X509Certificate(byte[] raw)
    {
        _certificate = X509CertificateLoader.LoadCertificate(raw);
    }

    public X509Certificate(X509Certificate2 certificate)
    {
        _certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
    }

    public string subject => _certificate.Subject;
    public string issuer => _certificate.Issuer;
    public string subjectAltName => string.Empty;
    public DateTime validFromDate => _certificate.NotBefore;
    public DateTime validToDate => _certificate.NotAfter;
    public string validFrom => _certificate.NotBefore.ToString("R");
    public string validTo => _certificate.NotAfter.ToString("R");
    public string fingerprint => _certificate.Thumbprint?.ToLowerInvariant() ?? string.Empty;
    public string fingerprint256 => Convert.ToHexString(SHA256.HashData(_certificate.RawData)).ToLowerInvariant();
    public string fingerprint512 => Convert.ToHexString(SHA512.HashData(_certificate.RawData)).ToLowerInvariant();
    public string serialNumber => _certificate.SerialNumber;
    public string keyUsage => string.Empty;
    public string infoAccess => string.Empty;
    public string signatureAlgorithm => _certificate.SignatureAlgorithm.FriendlyName ?? string.Empty;
    public string signatureAlgorithmOid => _certificate.SignatureAlgorithm.Value ?? string.Empty;
    public bool ca => false;
    public X509Certificate? issuerCertificate => null;

    public X509CertificateLegacyObject toLegacyObject() => new() { subject = subject, issuer = issuer, valid_from = validFrom, valid_to = validTo, fingerprint = fingerprint };
    public bool checkPrivateKey(KeyObject key) { _ = key; return false; }
    public string toJSON() => _certificate.ExportCertificatePem();
}

public sealed class SubtleCrypto
{
    public CryptoKey importKey(string format, byte[] keyData, KeyAlgorithm algorithm, bool extractable, string[] keyUsages)
    {
        _ = format;
        return new CryptoKey { data = keyData, algorithm = algorithm, extractable = extractable, usages = keyUsages };
    }

    public byte[] exportKey(string format, CryptoKey key)
    {
        _ = format;
        return key.data;
    }
}

public sealed class WebCrypto
{
    public SubtleCrypto subtle { get; } = new();

    public byte[] getRandomValues(byte[] array)
    {
        return crypto.randomFillSync(array);
    }
}

public static partial class crypto
{
    public static readonly CryptoConstants constants = new();
    public static readonly WebCrypto webcrypto = new();

    public static DigestResult digest(string algorithm, byte[] data, OneShotDigestOptions? options = null)
    {
        var bytes = HashData(algorithm, data);
        if (options?.outputLength is > 0 && options.outputLength < bytes.Length)
            Array.Resize(ref bytes, options.outputLength.Value);
        return new DigestResult(bytes);
    }

    public static DigestResult hmacDigest(string algorithm, byte[] key, byte[] data)
    {
        using var hmac = CreateHmacAlgorithm(algorithm, key);
        return new DigestResult(hmac.ComputeHash(data));
    }

    public static SecureHeapUsage secureHeapUsed() => new();

    public static AesGcmCiphertext aes256GcmEncrypt(byte[] key, byte[] nonce, byte[] plaintext, byte[]? associatedData = null)
    {
        if (key.Length != 32)
            throw new ArgumentException("AES-256-GCM requires a 32-byte key.", nameof(key));
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(key, tag.Length);
        aes.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);
        return new AesGcmCiphertext { ciphertext = ciphertext, authTag = tag, nonce = nonce };
    }

    public static byte[] aes256GcmDecrypt(byte[] key, byte[] nonce, byte[] ciphertext, byte[] authTag, byte[]? associatedData = null)
    {
        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(key, authTag.Length);
        aes.Decrypt(nonce, ciphertext, authTag, plaintext, associatedData);
        return plaintext;
    }

    public static RsaKeyPair generateRsaKeyPair(int modulusLength = 2048)
    {
        using var source = RSA.Create(modulusLength);
        var publicRsa = RSA.Create();
        var privateRsa = RSA.Create();
        publicRsa.ImportSubjectPublicKeyInfo(source.ExportSubjectPublicKeyInfo(), out _);
        privateRsa.ImportPkcs8PrivateKey(source.ExportPkcs8PrivateKey(), out _);
        return new RsaKeyPair(new PublicKeyObject(publicRsa, "rsa"), new PrivateKeyObject(privateRsa, "rsa"));
    }

    public static byte[] signSha256(byte[] data, KeyObject privateKey) => sign("sha256", data, privateKey);

    public static bool verifySha256(byte[] data, KeyObject publicKey, byte[] signature) => verify("sha256", data, publicKey, signature);

    public static CipherInfo? getCipherInfo(string name, CipherInfoOptions? options = null)
    {
        _ = options;
        var normalized = name.ToLowerInvariant();
        if (!normalized.StartsWith("aes-", StringComparison.Ordinal))
            return null;
        var parts = normalized.Split('-');
        var keyLength = parts.Length > 1 && int.TryParse(parts[1], out var bits) ? bits / 8 : 32;
        return new CipherInfo { name = normalized, keyLength = keyLength, ivLength = normalized.Contains("gcm", StringComparison.Ordinal) ? 12 : 16, blockSize = 16, mode = parts[^1] };
    }

    private static byte[] HashData(string algorithm, byte[] data)
    {
        return algorithm.ToLowerInvariant().Replace("-", string.Empty, StringComparison.Ordinal) switch
        {
            "sha1" => SHA1.HashData(data),
            "sha256" => SHA256.HashData(data),
            "sha384" => SHA384.HashData(data),
            "sha512" => SHA512.HashData(data),
            "md5" => MD5.HashData(data),
            _ => throw new ArgumentException($"Unsupported digest algorithm: {algorithm}", nameof(algorithm))
        };
    }

    private static HMAC CreateHmacAlgorithm(string algorithm, byte[] key)
    {
        return algorithm.ToLowerInvariant().Replace("-", string.Empty, StringComparison.Ordinal) switch
        {
            "sha1" => new HMACSHA1(key),
            "sha256" => new HMACSHA256(key),
            "sha384" => new HMACSHA384(key),
            "sha512" => new HMACSHA512(key),
            "md5" => new HMACMD5(key),
            _ => throw new ArgumentException($"Unsupported HMAC algorithm: {algorithm}", nameof(algorithm))
        };
    }
}
