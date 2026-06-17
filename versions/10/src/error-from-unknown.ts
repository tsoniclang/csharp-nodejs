const stringifyUnknownError = (value: unknown, fallbackMessage: string): string => {
  if (value === undefined) {
    return fallbackMessage;
  }

  if (typeof value === "string") {
    return value;
  }

  if (typeof value === "number" || typeof value === "boolean") {
    return String(value);
  }

  if (value === null) {
    return "null";
  }

  if (typeof value === "object") {
    return fallbackMessage;
  }

  return fallbackMessage;
};

export const errorFromUnknown = (
  value: unknown,
  fallbackMessage: string,
): Error =>
  value instanceof Error
    ? value
    : new Error(stringifyUnknownError(value, fallbackMessage));
