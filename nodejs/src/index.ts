import type { TsonicTargetCapabilityPlugin } from "@tsonic/target-api/provider";
import { createCsharpNodejsProviderPackage } from "./provider/package.js";

export function createTsonicPlugin(): TsonicTargetCapabilityPlugin {
  return createCsharpNodejsProviderPackage();
}
