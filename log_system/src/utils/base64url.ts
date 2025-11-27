const encoder = new TextEncoder();
const decoder = new TextDecoder();

export const toUint8Array = (value: string): Uint8Array => encoder.encode(value);

export const fromUint8Array = (value: ArrayBuffer | Uint8Array): string =>
  typeof value === 'string' ? value : decoder.decode(value instanceof Uint8Array ? value : new Uint8Array(value));

export const base64urlEncode = (value: string | ArrayBuffer | Uint8Array): string => {
  const bytes =
    typeof value === 'string'
      ? encoder.encode(value)
      : value instanceof Uint8Array
        ? value
        : new Uint8Array(value);
  let base64 = btoa(String.fromCharCode(...bytes));
  base64 = base64.replace(/=+$/u, '').replace(/\+/gu, '-').replace(/\//gu, '_');
  return base64;
};

export const base64urlDecode = (value: string): Uint8Array => {
  const padLength = (4 - (value.length % 4)) % 4;
  const padded = `${value.replace(/-/gu, '+').replace(/_/gu, '/')}${'='.repeat(padLength)}`;
  const binary = atob(padded);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i += 1) {
    bytes[i] = binary.charCodeAt(i);
  }
  return bytes;
};

export const base64urlDecodeToString = (value: string): string => fromUint8Array(base64urlDecode(value));
