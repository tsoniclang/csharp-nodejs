import type { ProviderTypeExpression } from "@tsonic/tsts";

export const stringProviderType = Object.freeze({ kind: "string" }) satisfies ProviderTypeExpression;
export const numberProviderType = Object.freeze({ kind: "number" }) satisfies ProviderTypeExpression;
export const uint8ProviderType = Object.freeze({ kind: "source-primitive", name: "uint8" }) satisfies ProviderTypeExpression;
export const int32ProviderType = Object.freeze({ kind: "source-primitive", name: "int32" }) satisfies ProviderTypeExpression;
export const uint32ProviderType = Object.freeze({ kind: "source-primitive", name: "uint32" }) satisfies ProviderTypeExpression;
export const int64ProviderType = Object.freeze({ kind: "source-primitive", name: "int64" }) satisfies ProviderTypeExpression;
export const booleanProviderType = Object.freeze({ kind: "boolean" }) satisfies ProviderTypeExpression;
export const voidProviderType = Object.freeze({ kind: "void" }) satisfies ProviderTypeExpression;
export const unknownProviderType = Object.freeze({ kind: "unknown" }) satisfies ProviderTypeExpression;
export const undefinedProviderType = Object.freeze({ kind: "undefined" }) satisfies ProviderTypeExpression;
export const errorProviderType = Object.freeze({ kind: "source-global", name: "Error" }) satisfies ProviderTypeExpression;
