import { base64urlDecodeToString, base64urlEncode } from '../utils/base64url';
import type { VerifiedClientContext } from '../types/log';

interface TokenPayload extends VerifiedClientContext {
  expiresAt: string;
  issuedAt?: string;
}

const textEncoder = new TextEncoder();

const subtle = crypto.subtle;

const createKey = (secret: string) =>
  subtle.importKey('raw', textEncoder.encode(secret), { name: 'HMAC', hash: 'SHA-256' }, false, ['sign']);

const signPayload = async (payload: string, secret: string): Promise<string> => {
  const key = await createKey(secret);
  const signature = await subtle.sign('HMAC', key, textEncoder.encode(payload));
  return base64urlEncode(new Uint8Array(signature));
};

const timingSafeEqual = (a: string, b: string): boolean => {
  const aBytes = textEncoder.encode(a);
  const bBytes = textEncoder.encode(b);
  if (aBytes.length !== bBytes.length) {
    return false;
  }
  let result = 0;
  for (let i = 0; i < aBytes.length; i += 1) {
    result |= aBytes[i] ^ bBytes[i];
  }
  return result === 0;
};

export class TokenVerificationError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'TokenVerificationError';
  }
}

export const verifyBearerToken = async (
  authorizationHeader: string | null,
  secret: string,
): Promise<TokenPayload> => {
  if (!authorizationHeader?.startsWith('Bearer ')) {
    throw new TokenVerificationError('Missing bearer token');
  }

  const token = authorizationHeader.substring('Bearer '.length).trim();
  const [payloadPart, signaturePart] = token.split('.');
  if (!payloadPart || !signaturePart) {
    throw new TokenVerificationError('Malformed token');
  }

  const expectedSignature = await signPayload(payloadPart, secret);
  if (!timingSafeEqual(signaturePart, expectedSignature)) {
    throw new TokenVerificationError('Invalid signature');
  }

  const payloadJson = base64urlDecodeToString(payloadPart);
  let payload: TokenPayload;
  try {
    payload = JSON.parse(payloadJson) as TokenPayload;
  } catch {
    throw new TokenVerificationError('Invalid token payload');
  }

  if (!payload.clientId) {
    throw new TokenVerificationError('Missing clientId');
  }

  if (!payload.scopes?.includes('log:write')) {
    throw new TokenVerificationError('Insufficient scope');
  }

  if (!payload.expiresAt) {
    throw new TokenVerificationError('Missing expiry');
  }

  const expiresAt = Date.parse(payload.expiresAt);
  if (Number.isNaN(expiresAt) || expiresAt <= Date.now()) {
    throw new TokenVerificationError('Token expired');
  }

  return payload;
};
