/**
 * URLPattern — minimal URL pattern matching.
 *
 */

export class URLPattern {
  regex: RegExp;

  constructor(pattern: string) {
    let escaped = "";
    for (let index = 0; index < pattern.length; index += 1) {
      const char = pattern.charAt(index);
      if (char === "*") {
        escaped += ".*";
      } else if (".+?^${}()|[]\\".includes(char)) {
        escaped += `\\${char}`;
      } else {
        escaped += char;
      }
    }
    const withWildcards = escaped;
    this.regex = new RegExp("^" + withWildcards + "$");
  }

  test(input: string): boolean {
    return this.regex.test(input);
  }

  exec(input: string): Record<string, string> | null {
    if (!this.test(input)) {
      return null;
    }
    return { input };
  }
}
