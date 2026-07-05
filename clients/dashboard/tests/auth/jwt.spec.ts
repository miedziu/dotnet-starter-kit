import { expect, test } from "@playwright/test";

// Helper to simulate JWT decoding in Playwright context (browser environment)
function decodeJwt(token: string | null | undefined): { name?: string; sub?: string } | null {
  if (!token) return null;
  const parts = token.split(".");
  if (parts.length !== 3) return null;
  try {
    const payload = parts[1].replace(/-/g, "+").replace(/_/g, "/");
    const padded = payload + "=".repeat((4 - (payload.length % 4)) % 4);
    const binaryString = atob(padded);
    // Use TextDecoder for proper UTF-8 handling
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    const json = new TextDecoder("utf-8").decode(bytes);
    return JSON.parse(json);
  } catch {
    return null;
  }
}

// Helper to create a JWT-like token for testing (payload only, no signature)
function createTestToken(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: "none", typ: "JWT" }));
  const payloadB64 = btoa(JSON.stringify(payload));
  return `${header}.${payloadB64}.signature`;
}

test.describe("JWT decode with polish characters", () => {
  test("should correctly decode polish characters in JWT payload", () => {
    // Polish characters: ą, ć, ę, ł, ń, ó, ś, ź, ż
    const polishNames = [
      "Jan Kowalski",
      "Łukasz Żukowski",
      "ąęćłńóśźż",
      "Maria Ćwik",
      "Paweł Łuczak",
      "Katarzyna Śliwińska",
      "Zbigniew Żak",
    ];

    for (const name of polishNames) {
      const token = createTestToken({ name, sub: "123" });
      const decoded = decodeJwt(token);
      expect(decoded).not.toBeNull();
      expect(decoded?.name).toBe(name);
    }
  });

  test("should handle mixed polish and ASCII characters", () => {
    const mixedName = "Jan Łukasz Kowalski-ĄĆĘŁŃÓŚŹŻ";
    const token = createTestToken({ name: mixedName, sub: "456" });
    const decoded = decodeJwt(token);
    expect(decoded?.name).toBe(mixedName);
  });

  test("should return null for invalid tokens", () => {
    expect(decodeJwt(null)).toBeNull();
    expect(decodeJwt(undefined)).toBeNull();
    expect(decodeJwt("")).toBeNull();
    expect(decodeJwt("invalid")).toBeNull();
    expect(decodeJwt("header.payload")).toBeNull(); // Only 2 parts
  });

  test("should handle token with special characters in sub", () => {
    const token = createTestToken({ name: "Test User", sub: "user-ąęć-123" });
    const decoded = decodeJwt(token);
    expect(decoded?.sub).toBe("user-ąęć-123");
  });
});