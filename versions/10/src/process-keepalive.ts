
import type {} from "./type-bootstrap.ts";

import {
  ManualResetEventSlim,
  Monitor,
  Thread,
} from "@tsonic/dotnet/System.Threading.js";

const sync = {};

let refCount = 0;
let releaseSignal: ManualResetEventSlim | null = null;

export const acquireProcessKeepAlive = (): void => {
  Monitor.Enter(sync);
  try {
    refCount += 1;
    if (refCount !== 1) {
      return;
    }

    const signal = new ManualResetEventSlim(false);
    const thread = new Thread(() => {
      signal.Wait();
    });
    thread.IsBackground = false;
    thread.Name = "nodejs.ProcessKeepAlive";

    releaseSignal = signal;
    thread.Start();
  } finally {
    Monitor.Exit(sync);
  }
};

export const releaseProcessKeepAlive = (): void => {
  let signal: ManualResetEventSlim | null = null;

  Monitor.Enter(sync);
  try {
    if (refCount === 0) {
      return;
    }

    refCount -= 1;
    if (refCount !== 0) {
      return;
    }

    signal = releaseSignal;
    releaseSignal = null;
  } finally {
    Monitor.Exit(sync);
  }

  signal?.Set();
};
