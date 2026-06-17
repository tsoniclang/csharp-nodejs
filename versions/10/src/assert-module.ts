
import type {} from "./type-bootstrap.ts";

import { AssertionError } from "./assertion-error.ts";
import { areDeepEqual, areStrictEqual } from "./deep-equality.ts";
import type { RuntimeValue } from "./runtime-value.ts";

export const ok = (value: boolean, message?: string): void => {
  if (!value) {
    throw new AssertionError(message, value, true, "==");
  }
};

export const fail = (message?: string): never => {
  throw new AssertionError(message ?? "Failed");
};

export const equal = (
  actual: RuntimeValue,
  expected: RuntimeValue,
  message?: string
): void => {
  if (!areStrictEqual(actual, expected)) {
    throw new AssertionError(message, actual, expected, "==");
  }
};

export const notEqual = (
  actual: RuntimeValue,
  expected: RuntimeValue,
  message?: string
): void => {
  if (areStrictEqual(actual, expected)) {
    throw new AssertionError(message, actual, expected, "!=");
  }
};

export const strictEqual = (
  actual: RuntimeValue,
  expected: RuntimeValue,
  message?: string
): void => {
  if (!areStrictEqual(actual, expected)) {
    throw new AssertionError(message, actual, expected, "===");
  }
};

export const notStrictEqual = (
  actual: RuntimeValue,
  expected: RuntimeValue,
  message?: string
): void => {
  if (areStrictEqual(actual, expected)) {
    throw new AssertionError(message, actual, expected, "!==");
  }
};

export const deepEqual = (
  actual: RuntimeValue,
  expected: RuntimeValue,
  message?: string
): void => {
  if (!areDeepEqual(actual, expected)) {
    throw new AssertionError(message, actual, expected, "deepEqual");
  }
};

export const notDeepEqual = (
  actual: RuntimeValue,
  expected: RuntimeValue,
  message?: string
): void => {
  if (areDeepEqual(actual, expected)) {
    throw new AssertionError(message, actual, expected, "notDeepEqual");
  }
};

export const deepStrictEqual = (
  actual: RuntimeValue,
  expected: RuntimeValue,
  message?: string
): void => {
  if (!areDeepEqual(actual, expected)) {
    throw new AssertionError(message, actual, expected, "deepEqual");
  }
};

export const notDeepStrictEqual = (
  actual: RuntimeValue,
  expected: RuntimeValue,
  message?: string
): void => {
  if (areDeepEqual(actual, expected)) {
    throw new AssertionError(message, actual, expected, "notDeepEqual");
  }
};

export const throws = (fn: () => void, message?: string): void => {
  try {
    fn();
  } catch (error) {
    if (error instanceof AssertionError) {
      throw error;
    }
    return;
  }

  throw new AssertionError(message ?? "Missing expected exception", null, null, "throws");
};

export const doesNotThrow = (fn: () => void, message?: string): void => {
  try {
    fn();
  } catch (error) {
    if (error instanceof Error) {
      throw new AssertionError(
        message ?? `Got unwanted exception: ${error.message}`,
        null,
        null,
        "doesNotThrow"
      );
    }
    throw new AssertionError(message ?? "Got unwanted exception", null, null, "doesNotThrow");
  }
};

export const match = (
  value: string,
  pattern: RegExp,
  message?: string
): void => {
  if (!pattern.test(value)) {
    throw new AssertionError(message, value, String(pattern), "match");
  }
};

export const doesNotMatch = (
  value: string,
  pattern: RegExp,
  message?: string
): void => {
  if (pattern.test(value)) {
    throw new AssertionError(message, value, String(pattern), "doesNotMatch");
  }
};

export const ifError = (value: RuntimeValue): void => {
  if (value === null || value === undefined) {
    return;
  }
  if (value instanceof Error) {
    throw value;
  }
  throw new AssertionError(
    `ifError got unwanted exception: ${String(value)}`,
    value,
    null,
    "ifError"
  );
};

export const strict = (
  actual: RuntimeValue,
  expected: RuntimeValue,
  message?: string
): void => strictEqual(actual, expected, message);

export const rejects = async (
  fn: () => Promise<RuntimeValue>,
  message?: string,
): Promise<void> => {
  try {
    await fn();
  } catch (error) {
    if (error instanceof AssertionError) {
      throw error;
    }
    return;
  }

  throw new AssertionError(message ?? "Missing expected rejection", null, null, "rejects");
};

export const doesNotReject = async (
  fn: () => Promise<RuntimeValue>,
  message?: string,
): Promise<void> => {
  try {
    await fn();
  } catch (error) {
    if (error instanceof Error) {
      throw new AssertionError(
        message ?? `Got unwanted rejection: ${error.message}`,
        null,
        null,
        "doesNotReject"
      );
    }
    throw new AssertionError(message ?? "Got unwanted rejection", null, null, "doesNotReject");
  }
};

export { AssertionError };
