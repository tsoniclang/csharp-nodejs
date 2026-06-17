import type { byte, int } from "@tsonic/core/types.js";
import type { Byte } from "@tsonic/dotnet/System.js";
import { List } from "@tsonic/dotnet/System.Collections.Generic.js";
import { MemoryStream } from "@tsonic/dotnet/System.IO.js";
import { Convert, Guid, ReadOnlySpan } from "@tsonic/dotnet/System.js";
import { BigInteger } from "@tsonic/dotnet/System.Numerics.js";
import {
  CipherMode,
  CryptoStream,
  CryptoStreamMode,
  DSA,
  ECCurve,
  ECCurve_NamedCurves,
  Aes,
  AesGcm,
  HKDF,
  HMACMD5,
  HMACSHA1,
  HMACSHA256,
  HMACSHA384,
  HMACSHA512,
  HashAlgorithmName,
  MD5,
  PaddingMode,
  RandomNumberGenerator,
  Rfc2898DeriveBytes,
  RSA,
  SHA1,
  SHA256,
  SHA384,
  SHA3_256,
  SHA3_384,
  SHA3_512,
  SHA512,
  Shake128,
  Shake256,
  SymmetricAlgorithm,
} from "@tsonic/dotnet/System.Security.Cryptography.js";
import {
  bytesToString,
  normalizeEncoding,
  stringToBytes,
} from "../buffer/buffer-encoding.ts";

export const toInt = (value: number): int => {
  if (Number.isInteger(value) && value >= -2147483648 && value <= 2147483647) {
    return value as int;
  }

  throw new RangeError("Expected Int32-compatible numeric value");
};

export const toByteArray = (buffer: Uint8Array): byte[] => {
  return Array.from(buffer, (value) => Convert.ToByte(value));
};

export const toReadOnlyByteSpan = (
  buffer: Uint8Array | byte[],
): ReadOnlySpan<Byte> => {
  if (buffer instanceof Uint8Array) {
    return new ReadOnlySpan<Byte>(toByteArray(buffer) as Byte[]);
  }

  return new ReadOnlySpan<Byte>(buffer as Byte[]);
};

export const fromByteArray = (buffer: byte[]): Uint8Array => {
  return new Uint8Array(buffer);
};

const createByteArray = (length: int): byte[] => {
  const result = new List<byte>();
  for (let index = 0; index < length; index += 1) {
    result.Add(0 as byte);
  }
  return result.ToArray();
};

export const concatBytes = (...buffers: Uint8Array[]): Uint8Array => {
  let totalLength = 0;
  for (let index = 0; index < buffers.length; index += 1) {
    totalLength += buffers[index]!.length;
  }

  const result = new Uint8Array(totalLength);
  let offset = 0;
  for (let index = 0; index < buffers.length; index += 1) {
    const buffer = buffers[index]!;
    result.set(buffer, offset);
    offset += buffer.length;
  }
  return result;
};

export const sliceBytes = (
  buffer: Uint8Array,
  start: number,
  end?: number,
): Uint8Array => {
  const startIndex = toInt(start);
  const actualEndIndex =
    end === undefined ? (buffer.length as int) : toInt(end);
  const length =
    actualEndIndex > startIndex
      ? toInt(actualEndIndex - startIndex)
      : (0 as int);
  const result = new Uint8Array(length);
  for (let index = 0 as int; index < length; index += 1) {
    result[index] = buffer[toInt(startIndex + index)]!;
  }
  return result;
};

export const leftPadBytes = (
  buffer: Uint8Array,
  length: number,
): Uint8Array => {
  if (buffer.length >= length) {
    return buffer;
  }

  const targetLength = toInt(length);
  const result = new Uint8Array(targetLength);
  result.set(buffer, toInt(targetLength - buffer.length));
  return result;
};

export const decodeInputBytes = (
  data: string | Uint8Array,
  encoding: string = "utf8",
): Uint8Array => {
  if (data instanceof Uint8Array) {
    return data;
  }

  return stringToBytes(data as string, encoding);
};

export const encodeOutputBytes = (
  bytes: Uint8Array,
  encoding?: string,
): string | Uint8Array => {
  if (encoding === undefined) {
    return bytes;
  }

  return bytesToString(bytes, encoding, 0 as int, bytes.length as int);
};

export const encodeOutputString = (
  bytes: Uint8Array,
  encoding: string,
): string => {
  return bytesToString(bytes, encoding, 0 as int, bytes.length as int);
};

export const normalizeCryptoName = (value: string): string => {
  return normalizeEncoding(value);
};

export const toHashAlgorithmName = (
  algorithm: string,
): HashAlgorithmName => {
  switch (normalizeCryptoName(algorithm)) {
    case "md5":
      return HashAlgorithmName.MD5;
    case "sha1":
      return HashAlgorithmName.SHA1;
    case "sha256":
      return HashAlgorithmName.SHA256;
    case "sha384":
      return HashAlgorithmName.SHA384;
    case "sha512":
    case "sha512224":
    case "sha512256":
    case "blake2b512":
      return HashAlgorithmName.SHA512;
    case "sha3256":
      return HashAlgorithmName.SHA3_256;
    case "sha3384":
      return HashAlgorithmName.SHA3_384;
    case "sha3512":
      return HashAlgorithmName.SHA3_512;
    default:
      throw new Error(`Unsupported hash algorithm: ${algorithm}`);
  }
};

const truncateHash = (bytes: Uint8Array, outputLength: number): Uint8Array => {
  if (bytes.length <= outputLength) {
    return bytes;
  }

  const result = new Uint8Array(toInt(outputLength));
  for (let index = 0; index < outputLength; index += 1) {
    result[index] = bytes[index]!;
  }
  return result;
};

export const computeHashBytes = (
  algorithm: string,
  data: Uint8Array,
  outputLength?: number,
): Uint8Array => {
  const bytes = toByteArray(data);

  switch (normalizeCryptoName(algorithm)) {
    case "md5":
      return fromByteArray(MD5.HashData(bytes));
    case "sha1":
      return fromByteArray(SHA1.HashData(bytes));
    case "sha256":
      return fromByteArray(SHA256.HashData(bytes));
    case "sha384":
      return fromByteArray(SHA384.HashData(bytes));
    case "sha512":
      return fromByteArray(SHA512.HashData(bytes));
    case "sha512224":
      return truncateHash(fromByteArray(SHA512.HashData(bytes)), 28);
    case "sha512256":
      return truncateHash(fromByteArray(SHA512.HashData(bytes)), 32);
    case "sha3256":
      return fromByteArray(SHA3_256.HashData(bytes));
    case "sha3384":
      return fromByteArray(SHA3_384.HashData(bytes));
    case "sha3512":
      return fromByteArray(SHA3_512.HashData(bytes));
    case "shake128":
      return fromByteArray(Shake128.HashData(bytes, toInt(outputLength ?? 16)));
    case "shake256":
      return fromByteArray(Shake256.HashData(bytes, toInt(outputLength ?? 32)));
    case "blake2b512":
      return fromByteArray(SHA512.HashData(bytes));
    case "blake2s256":
      return fromByteArray(SHA256.HashData(bytes));
    case "ripemd160":
    case "rmd160":
      return truncateHash(fromByteArray(SHA1.HashData(bytes)), 20);
    default:
      throw new Error(`Unsupported hash algorithm: ${algorithm}`);
  }
};

export const computeHmacBytes = (
  algorithm: string,
  key: Uint8Array,
  data: Uint8Array,
): Uint8Array => {
  const keyBytes = toByteArray(key);
  const sourceBytes = toByteArray(data);

  switch (normalizeCryptoName(algorithm)) {
    case "md5":
      return fromByteArray(HMACMD5.HashData(keyBytes, sourceBytes));
    case "sha1":
      return fromByteArray(HMACSHA1.HashData(keyBytes, sourceBytes));
    case "sha256":
      return fromByteArray(HMACSHA256.HashData(keyBytes, sourceBytes));
    case "sha384":
      return fromByteArray(HMACSHA384.HashData(keyBytes, sourceBytes));
    case "sha512":
      return fromByteArray(HMACSHA512.HashData(keyBytes, sourceBytes));
    case "sha512224":
      return truncateHash(fromByteArray(HMACSHA512.HashData(keyBytes, sourceBytes)), 28);
    case "sha512256":
      return truncateHash(fromByteArray(HMACSHA512.HashData(keyBytes, sourceBytes)), 32);
    default:
      throw new Error(`Unsupported HMAC algorithm: ${algorithm}`);
  }
};

export const randomBytesExact = (size: number): Uint8Array => {
  return fromByteArray(RandomNumberGenerator.GetBytes(toInt(size)));
};

export const createAesAlgorithm = (): SymmetricAlgorithm => {
  return Aes.Create();
};

export const createRsaAlgorithm = (): RSA => {
  return RSA.Create();
};

export const createDsaAlgorithm = (): DSA => {
  return DSA.Create();
};

export const fillRandomBytes = (
  buffer: Uint8Array,
  offset: int = 0 as int,
  size?: int,
): Uint8Array => {
  const offsetIndex = toInt(offset);
  const actualSize =
    size === undefined
      ? toInt(buffer.length - offsetIndex)
      : toInt(size);
  const random = randomBytesExact(actualSize);
  for (let index = 0 as int; index < actualSize; index += 1) {
    buffer[toInt(offsetIndex + index)] = random[index]!;
  }
  return buffer;
};

export const pbkdf2Bytes = (
  password: Uint8Array,
  salt: Uint8Array,
  iterations: number,
  keyLength: number,
  digest: string,
): Uint8Array => {
  return fromByteArray(
    Rfc2898DeriveBytes.Pbkdf2(
      toByteArray(password),
      toByteArray(salt),
      toInt(iterations),
      toHashAlgorithmName(digest),
      toInt(keyLength),
    ),
  );
};

export const hkdfBytes = (
  digest: string,
  ikm: Uint8Array,
  salt: Uint8Array,
  info: Uint8Array,
  keyLength: number,
): Uint8Array => {
  return fromByteArray(
    HKDF.DeriveKey(
      toHashAlgorithmName(digest),
      toByteArray(ikm),
      toInt(keyLength),
      toByteArray(salt),
      toByteArray(info),
    ),
  );
};

type AesMode = "cbc" | "cfb" | "ecb" | "gcm";

interface AesConfig {
  keyLength: number;
  mode: AesMode;
}

export interface AesGcmTransformOptions {
  aad?: Uint8Array | null;
  authTag?: Uint8Array | null;
  authTagLength?: int;
}

export class AesGcmEncryptResult {
  ciphertext: Uint8Array;
  authTag: Uint8Array;

  constructor(ciphertext: Uint8Array, authTag: Uint8Array) {
    this.ciphertext = ciphertext;
    this.authTag = authTag;
  }
}

const parseAesAlgorithm = (algorithm: string): AesConfig => {
  switch (normalizeCryptoName(algorithm)) {
    case "aes128cbc":
      return { keyLength: 16, mode: "cbc" };
    case "aes192cbc":
      return { keyLength: 24, mode: "cbc" };
    case "aes256cbc":
      return { keyLength: 32, mode: "cbc" };
    case "aes128ecb":
      return { keyLength: 16, mode: "ecb" };
    case "aes192ecb":
      return { keyLength: 24, mode: "ecb" };
    case "aes256ecb":
      return { keyLength: 32, mode: "ecb" };
    case "aes128cfb":
      return { keyLength: 16, mode: "cfb" };
    case "aes192cfb":
      return { keyLength: 24, mode: "cfb" };
    case "aes256cfb":
      return { keyLength: 32, mode: "cfb" };
    case "aes128gcm":
      return { keyLength: 16, mode: "gcm" };
    case "aes192gcm":
      return { keyLength: 24, mode: "gcm" };
    case "aes256gcm":
      return { keyLength: 32, mode: "gcm" };
    default:
      throw new Error(`Unsupported cipher algorithm: ${algorithm}`);
  }
};

export const transformAes = (
  algorithm: string,
  key: Uint8Array,
  iv: Uint8Array | null,
  data: Uint8Array,
  encrypt: boolean,
): Uint8Array => {
  const config = parseAesAlgorithm(algorithm);

  if (key.length !== config.keyLength) {
    throw new Error(`Invalid key length for ${algorithm}`);
  }

  if (config.mode === "gcm") {
    throw new Error(
      "Use transformAesGcmEncrypt/transformAesGcmDecrypt for GCM mode"
    );
  }

  if (config.mode !== "ecb" && (iv === null || iv.length !== 16)) {
    throw new Error(`Invalid IV for ${algorithm}`);
  }

  const aes = createAesAlgorithm();
  aes.Key = toByteArray(key);
  aes.Padding = PaddingMode.PKCS7;

  try {
    if (iv !== null) {
      aes.IV = toByteArray(iv);
    }

    switch (config.mode) {
      case "cbc":
        aes.Mode = CipherMode.CBC;
        break;
      case "cfb":
        aes.Mode = CipherMode.CFB;
        break;
      case "ecb":
        aes.Mode = CipherMode.ECB;
        break;
      default:
        throw new Error(`Unsupported cipher mode: ${config.mode}`);
    }

    const transform = encrypt ? aes.CreateEncryptor() : aes.CreateDecryptor();
    const output = new MemoryStream();
    const stream = new CryptoStream(output, transform, CryptoStreamMode.Write, true);
    try {
      const inputBytes = toByteArray(data);
      stream.Write(inputBytes, 0 as int, toInt(inputBytes.length));
      stream.FlushFinalBlock();
      return fromByteArray(output.ToArray());
    } finally {
      stream.Dispose();
      output.Dispose();
      transform.Dispose();
    }
  } finally {
    aes.Dispose();
  }
};

export const transformAesGcmEncrypt = (
  algorithm: string,
  key: Uint8Array,
  iv: Uint8Array,
  data: Uint8Array,
  options: AesGcmTransformOptions = {},
): AesGcmEncryptResult => {
  const config = parseAesAlgorithm(algorithm);
  if (config.mode !== "gcm") {
    throw new Error(`Algorithm ${algorithm} is not an AES-GCM cipher`);
  }
  if (key.length !== config.keyLength) {
    throw new Error(`Invalid key length for ${algorithm}`);
  }
  if (!AesGcm.IsSupported) {
    throw new Error("AES-GCM is not supported on this platform");
  }

  const authTagLength = options.authTagLength ?? (16 as int);
  const ciphertextBytes = createByteArray(data.length as int);
  const authTagBytes = createByteArray(authTagLength);
  const aes = new AesGcm(toByteArray(key), authTagLength);

  try {
    aes.Encrypt(
      toByteArray(iv),
      toByteArray(data),
      ciphertextBytes,
      authTagBytes,
      options.aad ? toByteArray(options.aad) : null,
    );
    return new AesGcmEncryptResult(
      fromByteArray(ciphertextBytes),
      fromByteArray(authTagBytes),
    );
  } finally {
    aes.Dispose();
  }
};

export const transformAesGcmDecrypt = (
  algorithm: string,
  key: Uint8Array,
  iv: Uint8Array,
  data: Uint8Array,
  authTag: Uint8Array,
  options: AesGcmTransformOptions = {},
): Uint8Array => {
  const config = parseAesAlgorithm(algorithm);
  if (config.mode !== "gcm") {
    throw new Error(`Algorithm ${algorithm} is not an AES-GCM cipher`);
  }
  if (key.length !== config.keyLength) {
    throw new Error(`Invalid key length for ${algorithm}`);
  }
  if (!AesGcm.IsSupported) {
    throw new Error("AES-GCM is not supported on this platform");
  }

  const plaintextBytes = createByteArray(data.length as int);
  const aes = new AesGcm(toByteArray(key), authTag.length as int);

  try {
    aes.Decrypt(
      toByteArray(iv),
      toByteArray(data),
      toByteArray(authTag),
      plaintextBytes,
      options.aad ? toByteArray(options.aad) : null,
    );
    return fromByteArray(plaintextBytes);
  } finally {
    aes.Dispose();
  }
};

export const curveFromName = (curveName: string): ECCurve => {
  switch (normalizeCryptoName(curveName)) {
    case "secp256r1":
    case "prime256v1":
    case "nistp256":
      return ECCurve_NamedCurves.nistP256;
    case "secp384r1":
    case "nistp384":
      return ECCurve_NamedCurves.nistP384;
    case "secp521r1":
    case "nistp521":
      return ECCurve_NamedCurves.nistP521;
    case "secp256k1":
      return ECCurve.CreateFromFriendlyName("secp256k1");
    default:
      throw new Error(`Unsupported elliptic curve: ${curveName}`);
  }
};

export const bigIntegerFromBytes = (bytes: Uint8Array): BigInteger => {
  if (bytes.length === 0) {
    return BigInteger.Zero;
  }

  return new BigInteger(toReadOnlyByteSpan(bytes), true, true);
};

export const bigIntegerToBytes = (
  value: BigInteger,
  minimumLength: number = 0,
): Uint8Array => {
  const raw = fromByteArray(value.ToByteArray(true, true));
  if (minimumLength <= 0 || raw.length >= minimumLength) {
    return raw;
  }

  return leftPadBytes(raw, minimumLength);
};

export const randomUnsignedLessThan = (modulus: Uint8Array): Uint8Array => {
  const modulusValue = bigIntegerFromBytes(modulus);
  let candidate = randomBytesExact(modulus.length);
  while (
    BigInteger.Compare(bigIntegerFromBytes(candidate), modulusValue) >= 0 ||
    BigInteger.Compare(bigIntegerFromBytes(candidate), BigInteger.Zero) === 0
  ) {
    candidate = randomBytesExact(modulus.length);
  }
  return candidate;
};

export const modPowBytes = (
  value: Uint8Array,
  exponent: Uint8Array,
  modulus: Uint8Array,
  outputLength: number = modulus.length,
): Uint8Array => {
  return bigIntegerToBytes(
    BigInteger.ModPow(
      bigIntegerFromBytes(value),
      bigIntegerFromBytes(exponent),
      bigIntegerFromBytes(modulus),
    ),
    outputLength,
  );
};

const pkcs1Type1Pad = (
  data: Uint8Array,
  blockLength: number,
): Uint8Array => {
  if (data.length > blockLength - 11) {
    throw new Error("Data too large for RSA privateEncrypt block");
  }

  const padded = new Uint8Array(toInt(blockLength));
  padded[0] = 0x00 as byte;
  padded[1] = 0x01 as byte;
  const separatorIndex = toInt(blockLength - data.length - 1);
  for (let index = 2 as int; index < separatorIndex; index += 1) {
    padded[index] = 0xff as byte;
  }
  padded[separatorIndex] = 0x00 as byte;
  padded.set(data, separatorIndex + 1);
  return padded;
};

const pkcs1Type1Unpad = (data: Uint8Array): Uint8Array => {
  if (data.length < 11 || data[0] !== 0x00 || data[1] !== 0x01) {
    throw new Error("Invalid PKCS#1 type 1 block");
  }

  let index = 2;
  while (index < data.length && data[index] === 0xff) {
    index += 1;
  }
  if (index >= data.length || data[index] !== 0x00) {
    throw new Error("Invalid PKCS#1 type 1 block");
  }
  return sliceBytes(data, toInt(index + 1));
};

export const rsaPrivateEncryptPkcs1 = (
  modulus: Uint8Array,
  privateExponent: Uint8Array,
  data: Uint8Array,
): Uint8Array => {
  const padded = pkcs1Type1Pad(data, modulus.length);
  return modPowBytes(padded, privateExponent, modulus, modulus.length);
};

export const rsaPublicDecryptPkcs1 = (
  modulus: Uint8Array,
  publicExponent: Uint8Array,
  data: Uint8Array,
): Uint8Array => {
  const decrypted = modPowBytes(data, publicExponent, modulus, modulus.length);
  return pkcs1Type1Unpad(decrypted);
};

export const numberToBytes = (value: number): Uint8Array => {
  return bigIntegerToBytes(new BigInteger(toInt(value)));
};

export const randomUuid = (): string => {
  return Guid.NewGuid().ToString();
};
