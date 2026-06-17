/**
 * Node.js os module.
 *
 */

import type {} from "../type-bootstrap.ts";

import { arch as archValue } from "./arch-value.ts";
import { availableParallelism as availableParallelismValue } from "./available-parallelism.ts";
import { cpus as cpusValue } from "./cpus-value.ts";
import type { CpuInfo as CpuInfoType, CpuTimes as CpuTimesType } from "./cpus-value.ts";
import { devNull as devNullValue } from "./dev-null.ts";
import { endianness as endiannessValue } from "./endianness-value.ts";
import { EOL as eolValue } from "./eol.ts";
import { freemem as freememValue } from "./freemem-value.ts";
import { homedir as homedirValue } from "./homedir-value.ts";
import { hostname as hostnameValue } from "./hostname-value.ts";
import { loadavg as loadavgValue } from "./loadavg-value.ts";
import { machine as machineValue } from "./machine-value.ts";
import { platform as platformValue } from "./platform-value.ts";
import { release as releaseValue } from "./release-value.ts";
import { tmpdir as tmpdirValue } from "./tmpdir-value.ts";
import { totalmem as totalmemValue } from "./totalmem-value.ts";
import { type as typeValue } from "./type-value.ts";
import { uptime as uptimeValue } from "./uptime-value.ts";
import { userInfo as userInfoValue } from "./user-info.ts";
import type { UserInfo as UserInfoType } from "./user-info.ts";
import { version as versionValue } from "./version-value.ts";

export type { CpuInfoType as CpuInfo, CpuTimesType as CpuTimes, UserInfoType as UserInfo };

export const arch = (): string => archValue();
export const availableParallelism = (): number => availableParallelismValue();
export const cpus = (): CpuInfoType[] => cpusValue();
export const devNull = devNullValue;
export const endianness = (): string => endiannessValue();
export const EOL = eolValue;
export const freemem = (): number => freememValue();
export const homedir = (): string => homedirValue();
export const hostname = (): string => hostnameValue();
export const loadavg = (): number[] => loadavgValue();
export const machine = (): string => machineValue();
export const platform = (): string => platformValue();
export const release = (): string => releaseValue();
export const tmpdir = (): string => tmpdirValue();
export const totalmem = (): number => totalmemValue();
export const type = (): string => typeValue();
export const uptime = (): number => uptimeValue();
export const userInfo = (): UserInfoType => userInfoValue();
export const version = (): string => versionValue();
