using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class createCipherivTests
{
    [Fact]
    public void createCipheriv_Decipher_AES256_RoundTrip()
    {
        var key = crypto.randomBytes(32); // 256 bits
        var iv = crypto.randomBytes(16);  // 128 bits
        var plaintext = "Hello, World! This is a test message.";

        // Encrypt
        var cipher = crypto.createCipheriv("aes-256-cbc", key, iv);
        var encrypted = cipher.update(plaintext, "utf8", "hex");
        encrypted += cipher.final("hex");

        // Decrypt
        var decipher = crypto.createDecipheriv("aes-256-cbc", key, iv);
        var decrypted = decipher.update(encrypted, "hex", "utf8");
        decrypted += decipher.final("utf8");

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void createCipheriv_FinalOnlyOnce()
    {
        var key = crypto.randomBytes(32);
        var iv = crypto.randomBytes(16);
        var cipher = crypto.createCipheriv("aes-256-cbc", key, iv);

        cipher.update("test");
        cipher.final();

        Assert.Throws<InvalidOperationException>(() => cipher.final());
    }

    [Fact]
    public void createCipheriv_AES128CBC_Works()
    {
        var key = crypto.randomBytes(16); // 128 bits
        var iv = crypto.randomBytes(16);
        var plaintext = "Test message";

        var cipher = crypto.createCipheriv("aes-128-cbc", key, iv);
        var encrypted = cipher.update(plaintext, "utf8", "hex");
        encrypted += cipher.final("hex");

        var decipher = crypto.createDecipheriv("aes-128-cbc", key, iv);
        var decrypted = decipher.update(encrypted, "hex", "utf8");
        decrypted += decipher.final("utf8");

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void createCipheriv_AES192CBC_Works()
    {
        var key = crypto.randomBytes(24); // 192 bits
        var iv = crypto.randomBytes(16);
        var plaintext = "Test message";

        var cipher = crypto.createCipheriv("aes-192-cbc", key, iv);
        var encrypted = cipher.update(plaintext, "utf8", "hex");
        encrypted += cipher.final("hex");

        var decipher = crypto.createDecipheriv("aes-192-cbc", key, iv);
        var decrypted = decipher.update(encrypted, "hex", "utf8");
        decrypted += decipher.final("utf8");

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void createCipheriv_AES256ECB_Works()
    {
        var key = crypto.randomBytes(32);
        var plaintext = "Test message";

        var cipher = crypto.createCipheriv("aes-256-ecb", key, null);
        var encrypted = cipher.update(plaintext, "utf8", "hex");
        encrypted += cipher.final("hex");

        var decipher = crypto.createDecipheriv("aes-256-ecb", key, null);
        var decrypted = decipher.update(encrypted, "hex", "utf8");
        decrypted += decipher.final("utf8");

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void createCipheriv_AES256CFB_Works()
    {
        var key = crypto.randomBytes(32);
        var iv = crypto.randomBytes(16);
        var plaintext = "Test message";

        var cipher = crypto.createCipheriv("aes-256-cfb", key, iv);
        var encrypted = cipher.update(plaintext, "utf8", "hex");
        encrypted += cipher.final("hex");

        var decipher = crypto.createDecipheriv("aes-256-cfb", key, iv);
        var decrypted = decipher.update(encrypted, "hex", "utf8");
        decrypted += decipher.final("utf8");

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void createCipheriv_WithByteArrays()
    {
        var key = crypto.randomBytes(32);
        var iv = crypto.randomBytes(16);
        var plaintextBytes = Encoding.UTF8.GetBytes("Test message");

        var cipher = crypto.createCipheriv("aes-256-cbc", key, iv);
        var encrypted1 = cipher.update(plaintextBytes, "base64");
        var encrypted2 = cipher.final("base64");
        var encryptedFull = encrypted1 + encrypted2;

        var decipher = crypto.createDecipheriv("aes-256-cbc", key, iv);
        var decrypted1 = decipher.update(encryptedFull, "base64", "utf8");
        var decrypted2 = decipher.final("utf8");

        Assert.Equal("Test message", decrypted1 + decrypted2);
    }

    [Fact]
    public void createCipheriv_StringKeyAndIV()
    {
        var cipher = crypto.createCipheriv("aes-256-cbc", "12345678901234567890123456789012", "1234567890123456");
        var encrypted = cipher.update("test", "utf8", "hex");
        encrypted += cipher.final("hex");
        Assert.NotEmpty(encrypted);
    }

    [Fact]
    public void createCipheriv_UpdateAfterFinal_Throws()
    {
        var cipher = crypto.createCipheriv("aes-256-cbc", crypto.randomBytes(32), crypto.randomBytes(16));
        cipher.update("test");
        cipher.final();

        Assert.Throws<InvalidOperationException>(() => cipher.update("more"));
    }

    [Fact]
    public void createCipheriv_GetAuthTag_ThrowsNotSupportedForNonAuthenticatedMode()
    {
        var cipher = crypto.createCipheriv("aes-256-cbc", crypto.randomBytes(32), crypto.randomBytes(16));
        cipher.update("test data", "utf8", "hex");
        cipher.final("hex");

        // getAuthTag only works with authenticated modes like GCM
        Assert.Throws<NotSupportedException>(() => cipher.getAuthTag());
    }

    [Fact]
    public void createCipheriv_SetAAD_ThrowsNotSupportedForNonAuthenticatedMode()
    {
        var cipher = crypto.createCipheriv("aes-256-cbc", crypto.randomBytes(32), crypto.randomBytes(16));

        // setAAD only works with authenticated modes like GCM
        Assert.Throws<NotSupportedException>(() => cipher.setAAD(Encoding.UTF8.GetBytes("additional data")));
    }

    [Fact]
    public void createCipheriv_InvalidAlgorithm_Throws()
    {
        Assert.Throws<ArgumentException>(() => crypto.createCipheriv("invalid-algo", new byte[16], new byte[16]));
    }
}
