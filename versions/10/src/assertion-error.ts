import type { RuntimeValue } from "./runtime-value.ts";

const formatAssertionValue = (value: RuntimeValue): string => {
  if (value === null) {
    return "null";
  }
  if (value === undefined) {
    return "undefined";
  }
  return String(value);
};

const generateMessage = (
  actual: RuntimeValue,
  expected: RuntimeValue,
  operator: string
): string => {
  switch (operator) {
    case "==":
      return `Expected ${formatAssertionValue(actual)} == ${formatAssertionValue(expected)}`;
    case "!=":
      return `Expected ${formatAssertionValue(actual)} != ${formatAssertionValue(expected)}`;
    case "===":
      return `Expected ${formatAssertionValue(actual)} === ${formatAssertionValue(expected)}`;
    case "!==":
      return `Expected ${formatAssertionValue(actual)} !== ${formatAssertionValue(expected)}`;
    case "deepEqual":
      return `Expected values to be deeply equal:\n${formatAssertionValue(actual)}\nvs\n${formatAssertionValue(expected)}`;
    case "notDeepEqual":
      return "Expected values not to be deeply equal";
    case "throws":
      return "Missing expected exception";
    case "doesNotThrow":
      return "Got unwanted exception";
    default:
      return `Assertion failed: ${formatAssertionValue(actual)}`;
  }
};

export class AssertionError extends Error {
  actual: RuntimeValue = undefined;
  expected: RuntimeValue = undefined;
  operator: string = "";
  generatedMessage: boolean = false;

  get code(): string {
    return "ERR_ASSERTION";
  }

  constructor(
    message?: string,
    actual?: RuntimeValue,
    expected?: RuntimeValue,
    operator: string = ""
  ) {
    super(message ?? generateMessage(actual, expected, operator));
    this.actual = actual;
    this.expected = expected;
    this.operator = operator;
    this.generatedMessage = message === undefined;
  }
}
