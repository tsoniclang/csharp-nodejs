/**
 * os.availableParallelism — returns the default parallelism estimate.
 *
 */
import { Environment } from "@tsonic/dotnet/System.js";

export const availableParallelism = (): number =>
  Environment.ProcessorCount;
