import type { ProviderTypeExpression } from "@tsonic/tsts";

export const stringProviderType = Object.freeze({ kind: "string" }) satisfies ProviderTypeExpression;
export const numberProviderType = Object.freeze({ kind: "number" }) satisfies ProviderTypeExpression;
export const booleanProviderType = Object.freeze({ kind: "boolean" }) satisfies ProviderTypeExpression;
export const voidProviderType = Object.freeze({ kind: "void" }) satisfies ProviderTypeExpression;
export const unknownProviderType = Object.freeze({ kind: "unknown" }) satisfies ProviderTypeExpression;
export const undefinedProviderType = Object.freeze({ kind: "undefined" }) satisfies ProviderTypeExpression;
