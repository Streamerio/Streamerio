import type { Env } from '../types/env';
import type { LogBatch, NormalizedLogEvent } from '../types/log';
import { base64urlEncode } from '../utils/base64url';

interface ServiceAccount {
  client_email: string;
  private_key: string;
  token_uri: string;
  project_id?: string;
}

const textEncoder = new TextEncoder();

const pemToArrayBuffer = (pem: string): ArrayBuffer => {
  const normalized = pem.replace(/-----BEGIN [^-]+-----/u, '').replace(/-----END [^-]+-----/u, '').replace(/\s+/gu, '');
  const binary = atob(normalized);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i += 1) {
    bytes[i] = binary.charCodeAt(i);
  }
  return bytes.buffer;
};

const signJwt = async (data: string, privateKey: string): Promise<ArrayBuffer> => {
  const key = await crypto.subtle.importKey(
    'pkcs8',
    pemToArrayBuffer(privateKey),
    {
      name: 'RSASSA-PKCS1-v1_5',
      hash: 'SHA-256',
    },
    false,
    ['sign'],
  );
  return crypto.subtle.sign('RSASSA-PKCS1-v1_5', key, textEncoder.encode(data));
};

const scope = 'https://www.googleapis.com/auth/logging.write';

class GcpAccessTokenProvider {
  private cache: { token: string; expiresAt: number } | null = null;

  constructor(private readonly serviceAccount: ServiceAccount) {}

  async getToken(): Promise<string> {
    if (this.cache && this.cache.expiresAt - 60_000 > Date.now()) {
      return this.cache.token;
    }
    const jwt = await this.createAssertion();
    const form = new URLSearchParams();
    form.set('grant_type', 'urn:ietf:params:oauth:grant-type:jwt-bearer');
    form.set('assertion', jwt);

    const response = await fetch(this.serviceAccount.token_uri, {
      method: 'POST',
      headers: {
        'content-type': 'application/x-www-form-urlencoded',
      },
      body: form,
    });

    if (!response.ok) {
      const body = await response.text();
      throw new Error(`Failed to fetch access token: ${response.status} ${body}`);
    }

    const json = (await response.json()) as { access_token: string; expires_in: number };
    this.cache = {
      token: json.access_token,
      expiresAt: Date.now() + (json.expires_in - 60) * 1000,
    };
    return this.cache.token;
  }

  private async createAssertion(): Promise<string> {
    const now = Math.floor(Date.now() / 1000);
    const header = base64urlEncode(JSON.stringify({ alg: 'RS256', typ: 'JWT' }));
    const payload = base64urlEncode(
      JSON.stringify({
        iss: this.serviceAccount.client_email,
        scope,
        aud: this.serviceAccount.token_uri,
        exp: now + 3600,
        iat: now,
      }),
    );
    const toSign = `${header}.${payload}`;
    const signature = await signJwt(toSign, this.serviceAccount.private_key);
    const encodedSignature = base64urlEncode(new Uint8Array(signature));
    return `${toSign}.${encodedSignature}`;
  }
}

let cachedProvider: { key: string; provider: GcpAccessTokenProvider } | null = null;

const getProvider = (env: Env): GcpAccessTokenProvider => {
  if (!cachedProvider || cachedProvider.key !== env.GCP_SERVICE_ACCOUNT_JSON) {
    const account = JSON.parse(env.GCP_SERVICE_ACCOUNT_JSON) as ServiceAccount;
    cachedProvider = {
      key: env.GCP_SERVICE_ACCOUNT_JSON,
      provider: new GcpAccessTokenProvider(account),
    };
  }
  return cachedProvider.provider;
};

const extractProjectId = (logName: string, fallback?: string): string | undefined => {
  const match = /projects\/([^/]+)/u.exec(logName);
  if (match) {
    return match[1];
  }
  return fallback;
};

const buildEntry = (event: NormalizedLogEvent, batch: LogBatch) => ({
  jsonPayload: {
    timestamp: event.timestamp,
    platform: event.platform,
    eventType: event.eventType,
    message: event.message,
    roomId: event.roomId,
    viewerId: event.viewerId,
    requestId: event.requestId ?? batch.requestId,
    clientId: event.clientId,
    tags: event.tags,
    extra: event.extraJson,
    receivedAt: batch.receivedAt,
  },
  severity: event.severity,
  labels: {
    ...event.tags,
    client_id: event.clientId,
    room_id: event.roomId ?? 'unknown',
  },
  timestamp: event.timestamp,
});

export class CloudLoggingWriter {
  private readonly provider: GcpAccessTokenProvider;

  private readonly resourceType: string;

  private readonly projectId?: string;

  constructor(private readonly env: Env) {
    this.provider = getProvider(env);
    this.resourceType = env.GCP_RESOURCE_TYPE ?? 'global';
    this.projectId = extractProjectId(env.GCP_LOG_NAME, JSON.parse(env.GCP_SERVICE_ACCOUNT_JSON).project_id);
  }

  async write(batch: LogBatch): Promise<void> {
    const token = await this.provider.getToken();
    const body = JSON.stringify({
      logName: this.env.GCP_LOG_NAME,
      resource: {
        type: this.resourceType,
        labels: this.projectId ? { project_id: this.projectId } : undefined,
      },
      entries: batch.events.map((event) => buildEntry(event, batch)),
    });
    // console.debug('Cloud Logging write request body:', body);

    const response = await fetch('https://logging.googleapis.com/v2/entries:write', {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
        'content-type': 'application/json',
      },
      body,
    });
    // console.debug('Cloud Logging write response:', await response.text());

    if (!response.ok) {
      const text = await response.text();
      throw new Error(`Cloud Logging write failed: ${response.status} ${text}`);
    }
  }
}
