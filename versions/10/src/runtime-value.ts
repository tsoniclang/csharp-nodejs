export type RuntimeObject = object;

export type RuntimeArray = RuntimeValue[];

export type RuntimeValue =
  | string
  | number
  | boolean
  | RuntimeArray
  | RuntimeObject
  | null
  | undefined;
