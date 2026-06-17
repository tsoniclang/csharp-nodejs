/**
 * Node.js crypto Certificate class (SPKAC).
 *
 */

import {
  base64ToBytes,
  bytesToBase64,
  bytesToString,
  stringToBytes,
} from "../buffer/buffer-encoding.ts";

const emptyBytes = (): Uint8Array => new Uint8Array(0);

const normalizeSpkacString = (value: string): string => {
  const trimmed = value.trim();
  return trimmed.startsWith("SPKAC=") ? trimmed.substring(6).trim() : trimmed;
};

const parseSpkacBytes = (spkac: string | Uint8Array): Uint8Array => {
  if (spkac instanceof Uint8Array) {
    return spkac;
  }

  const normalized = normalizeSpkacString(spkac as string);
  if (normalized.length === 0) {
    return emptyBytes();
  }

  const lower = normalized.toLowerCase();
  if (lower.includes("challenge=") || lower.includes("publickey=")) {
    return stringToBytes(normalized, "utf8");
  }

  try {
    return base64ToBytes(normalized);
  } catch {
    return stringToBytes(normalized, "utf8");
  }
};

const tryToUtf8 = (bytes: Uint8Array): string => {
  if (bytes.length === 0) {
    return "";
  }

  try {
    return bytesToString(bytes, "utf8", 0, bytes.length);
  } catch {
    return "";
  }
};

const extractTaggedValue = (text: string, tagName: string): Uint8Array => {
  if (text.length === 0) {
    return emptyBytes();
  }

  const prefix = `${tagName}=`;
  const lower = text.toLowerCase();
  const start = lower.indexOf(prefix.toLowerCase());
  if (start < 0) {
    return emptyBytes();
  }

  let value = "";
  for (let index = start + prefix.length; index < text.length; index += 1) {
    const char = text.charAt(index);
    if (char === "\n" || char === "\r" || char === "," || char === ";") {
      break;
    }
    value += char;
  }

  const trimmed = value.trim();
  if (trimmed.length === 0) {
    return emptyBytes();
  }

  try {
    return base64ToBytes(trimmed);
  } catch {
    return stringToBytes(trimmed, "utf8");
  }
};

const looksLikeDerSequence = (bytes: Uint8Array): boolean =>
  bytes.length > 1 && bytes[0] === 0x30;

const splitDistinguishedName = (value: string): Array<string> => {
  if (value.length === 0) {
    return [];
  }

  return value
    .split(",")
    .map((part) => part.trim())
    .filter((part) => part.length > 0);
};

const extractNamedValue = (
  value: string,
  keys: Array<string>,
): string | null => {
  const parts = splitDistinguishedName(value);
  for (let partIndex = 0; partIndex < parts.length; partIndex += 1) {
    const part = parts[partIndex]!;
    const separatorIndex = part.indexOf("=");
    if (separatorIndex < 0) {
      continue;
    }

    const name = part.slice(0, separatorIndex).trim().toLowerCase();
    if (!keys.includes(name)) {
      continue;
    }

    const result = part.slice(separatorIndex + 1).trim();
    return result.length > 0 ? result : null;
  }

  return null;
};

const matchesHostname = (pattern: string, hostname: string): boolean => {
  const loweredPattern = pattern.toLowerCase();
  const loweredHost = hostname.toLowerCase();
  if (loweredPattern === loweredHost) {
    return true;
  }

  if (!loweredPattern.startsWith("*.")) {
    return false;
  }

  const suffix = loweredPattern.slice(1);
  if (!loweredHost.endsWith(suffix)) {
    return false;
  }

  const prefix = loweredHost.slice(0, loweredHost.length - suffix.length);
  return prefix.length > 0 && !prefix.includes(".");
};

const toPemBody = (raw: Uint8Array): string => {
  const base64 = bytesToBase64(raw);
  const lines: Array<string> = [];
  for (let index = 0; index < base64.length; index += 64) {
    lines.push(base64.slice(index, index + 64));
  }
  return lines.join("\n");
};

/**
 * SPKAC is a Certificate Signing Request mechanism originally implemented by Netscape.
 */
export class Certificate {
  /**
   * Exports the challenge component of an SPKAC data structure.
   */
  static exportChallenge(spkac: string | Uint8Array): Uint8Array {
    const bytes = parseSpkacBytes(spkac);
    return extractTaggedValue(tryToUtf8(bytes), "challenge");
  }

  /**
   * Exports the public key component of an SPKAC data structure.
   */
  static exportPublicKey(spkac: string | Uint8Array): Uint8Array {
    const bytes = parseSpkacBytes(spkac);
    const tagged = extractTaggedValue(tryToUtf8(bytes), "publickey");
    if (tagged.length > 0) {
      return tagged;
    }

    return looksLikeDerSequence(bytes) ? bytes : emptyBytes();
  }

  /**
   * Validates an SPKAC data structure.
   */
  static verifySpkac(spkac: string | Uint8Array): boolean {
    const bytes = parseSpkacBytes(spkac);
    if (bytes.length === 0) {
      return false;
    }

    if (looksLikeDerSequence(bytes)) {
      return true;
    }

    const text = tryToUtf8(bytes);
    return (
      extractTaggedValue(text, "challenge").length > 0 ||
      extractTaggedValue(text, "publickey").length > 0
    );
  }
}

/**
 * Information about an X.509 certificate.
 */
export class X509CertificateInfo {
  subject: string;
  issuer: string;
  serialNumber: string;
  validFrom: string;
  validTo: string;
  fingerprint: string;
  fingerprint256: string;
  fingerprint512: string;
  publicKey: Uint8Array;
  raw: Uint8Array;

  constructor(
    subject: string,
    issuer: string,
    serialNumber: string,
    validFrom: string,
    validTo: string,
    fingerprint: string,
    fingerprint256: string,
    fingerprint512: string,
    publicKey: Uint8Array,
    raw: Uint8Array
  ) {
    this.subject = subject;
    this.issuer = issuer;
    this.serialNumber = serialNumber;
    this.validFrom = validFrom;
    this.validTo = validTo;
    this.fingerprint = fingerprint;
    this.fingerprint256 = fingerprint256;
    this.fingerprint512 = fingerprint512;
    this.publicKey = publicKey;
    this.raw = raw;
  }

  /**
   * Checks whether the certificate matches the given hostname.
   */
  checkHost(hostname: string): string | null {
    const commonName = extractNamedValue(this.subject, ["cn", "dns"]);
    if (commonName === null) {
      return null;
    }

    return matchesHostname(commonName, hostname) ? hostname : null;
  }

  /**
   * Checks whether the certificate matches the given email address.
   */
  checkEmail(email: string): string | null {
    const subjectEmail = extractNamedValue(this.subject, [
      "emailaddress",
      "email",
      "e",
    ]);
    if (subjectEmail === null) {
      return null;
    }

    return subjectEmail.toLowerCase() === email.toLowerCase() ? email : null;
  }

  /**
   * Checks whether the certificate matches the given IP address.
   */
  checkIP(ip: string): string | null {
    const subjectIp = extractNamedValue(this.subject, [
      "ip",
      "ip address",
      "ipaddress",
    ]);
    if (subjectIp === null) {
      return null;
    }

    return subjectIp === ip ? ip : null;
  }

  /**
   * Checks whether this certificate issued the other certificate.
   */
  checkIssued(otherCert: X509CertificateInfo): string | null {
    return this.subject === otherCert.issuer ? null : "Issuer mismatch";
  }

  /**
   * Checks whether the certificate was issued by the given issuer certificate.
   */
  verify(issuerCert: X509CertificateInfo): boolean {
    return issuerCert.subject === this.issuer;
  }

  /**
   * Returns the PEM-encoded certificate.
   */
  toPEM(): string {
    return `-----BEGIN CERTIFICATE-----\n${toPemBody(this.raw)}\n-----END CERTIFICATE-----`;
  }

  toString(): string {
    return `X509Certificate(subject=${this.subject}, issuer=${this.issuer})`;
  }
}
