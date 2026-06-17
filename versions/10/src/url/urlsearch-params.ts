/**
 * URLSearchParams — read/write access to the query of a URL.
 *
 */

type SearchParamEntry = {
  name: string;
  value: string;
};

export class URLSearchParams {
  params: SearchParamEntry[] = [];

  get size(): number {
    return this.params.length;
  }

  constructor(init?: string) {
    if (init !== undefined && init !== null && init.length > 0) {
      this.parseQueryString(init);
    }
  }

  append(name: string, value: string): void {
    this.params.push({ name, value });
  }

  set(name: string, value: string): void {
    // Remove all existing entries with this name
    for (let i = this.params.length - 1; i >= 0; i -= 1) {
      if (this.params[i]!.name === name) {
        this.params.splice(i, 1);
      }
    }
    this.params.push({ name, value });
  }

  get(name: string): string | null {
    for (let i = 0; i < this.params.length; i += 1) {
      if (this.params[i]!.name === name) {
        return this.params[i]!.value;
      }
    }
    return null;
  }

  getAll(name: string): string[] {
    const result: string[] = [];
    for (let i = 0; i < this.params.length; i += 1) {
      if (this.params[i]!.name === name) {
        result.push(this.params[i]!.value);
      }
    }
    return result;
  }

  has(name: string, value?: string): boolean {
    for (let i = 0; i < this.params.length; i += 1) {
      if (this.params[i]!.name === name) {
        if (value === undefined || this.params[i]!.value === value) {
          return true;
        }
      }
    }
    return false;
  }

  delete(name: string, value?: string): void {
    for (let i = this.params.length - 1; i >= 0; i -= 1) {
      if (this.params[i]!.name === name) {
        if (value === undefined || this.params[i]!.value === value) {
          this.params.splice(i, 1);
        }
      }
    }
  }

  sort(): void {
    this.params.sort((a, b) => {
      if (a.name < b.name) return -1;
      if (a.name > b.name) return 1;
      return 0;
    });
  }

  keys(): string[] {
    return this.params.map((p) => p.name);
  }

  values(): string[] {
    return this.params.map((p) => p.value);
  }

  entries(): Array<[string, string]> {
    return this.params.map((param) => [param.name, param.value]);
  }

  forEach(callback: (value: string, name: string) => void): void {
    for (let i = 0; i < this.params.length; i += 1) {
      callback(this.params[i]!.value, this.params[i]!.name);
    }
  }

  toString(): string {
    if (this.params.length === 0) {
      return "";
    }
    const parts: string[] = [];
    for (let i = 0; i < this.params.length; i += 1) {
      parts.push(
        encodeURIComponent(this.params[i]!.name) +
          "=" +
          encodeURIComponent(this.params[i]!.value)
      );
    }
    return parts.join("&");
  }

  parseQueryString(query: string): void {
    let input = query;
    if (input.startsWith("?")) {
      input = input.slice(1);
    }
    if (input.length === 0) {
      return;
    }
    const pairs = input.split("&");
    for (let i = 0; i < pairs.length; i += 1) {
      const pair = pairs[i]!;
      const eqIndex = pair.indexOf("=");
      if (eqIndex >= 0) {
        const key = decodeURIComponent(pair.slice(0, eqIndex));
        const value = decodeURIComponent(pair.slice(eqIndex + 1));
        this.params.push({ name: key, value });
      } else {
        this.params.push({ name: decodeURIComponent(pair), value: "" });
      }
    }
  }
}
