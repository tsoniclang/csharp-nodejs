import type {} from "./type-bootstrap.ts";

import type { int } from "@tsonic/core/types.js";
import type { RuntimeValue } from "./runtime-value.ts";

export const isArrayValue = (value: RuntimeValue): boolean => {
  return Array.isArray(value);
};

export const areStrictEqual = (
  left: RuntimeValue,
  right: RuntimeValue
): boolean => {
  if (
    left === null ||
    left === undefined ||
    right === null ||
    right === undefined
  ) {
    return left === right;
  }

  if (typeof left === "number" && typeof right === "number") {
    return (left as number) === (right as number);
  }

  if (typeof left === "string" && typeof right === "string") {
    return (left as string) === (right as string);
  }

  if (typeof left === "boolean" && typeof right === "boolean") {
    return (left as boolean) === (right as boolean);
  }

  return left === right;
};

const areUint8ArraysEqual = (
  left: Uint8Array,
  right: Uint8Array
): boolean => {
  if (left.length !== right.length) {
    return false;
  }

  for (let index = 0 as int; index < left.length; index = (index + 1) as int) {
    if (left[index] !== right[index]) {
      return false;
    }
  }

  return true;
};

export const areDeepEqual = (
  left: RuntimeValue,
  right: RuntimeValue
): boolean => {
  if (areStrictEqual(left, right)) {
    return true;
  }

  if (
    left === null ||
    right === null ||
    left === undefined ||
    right === undefined
  ) {
    return false;
  }

  if (
    typeof left === "string" ||
    typeof left === "number" ||
    typeof left === "boolean" ||
    typeof right === "string" ||
    typeof right === "number" ||
    typeof right === "boolean"
  ) {
    return false;
  }

  if (typeof left !== "object" || typeof right !== "object") {
    return false;
  }

  if (left instanceof Uint8Array && right instanceof Uint8Array) {
    return areUint8ArraysEqual(left, right);
  }

  return false;
};
