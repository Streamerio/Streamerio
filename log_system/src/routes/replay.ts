import type { Context } from 'hono';
import { TokenVerificationError, verifyBearerToken } from '../security/token';
import { DeadLetterStore } from '../services/dead-letter';
import { CloudLoggingWriter } from '../services/gcp';
import type { AppEnv } from '../types/app';

interface ReplayRequestBody {
  keys: string[];
}

export const replayHandler = async (c: Context<AppEnv>): Promise<Response> => {
  const authorization = c.req.header('Authorization');
  if (!authorization) {
    return c.json({ error: 'Missing Authorization header' }, 401);
  }
  try {
    const verified = await verifyBearerToken(authorization, c.env.CLIENT_LOG_TOKEN_SECRET);
    if (!verified.scopes.includes('log:replay')) {
      return c.json({ error: 'Forbidden' }, 403);
    }
    if (!c.env.DLQ_BUCKET) {
      return c.json({ error: 'DLQ bucket is not configured' }, 503);
    }

    const body = (await c.req.json()) as ReplayRequestBody;
    if (!Array.isArray(body.keys) || body.keys.length === 0) {
      return c.json({ error: '`keys` must be a non-empty array' }, 400);
    }

    const store = new DeadLetterStore(c.env.DLQ_BUCKET);
    const writer = new CloudLoggingWriter(c.env);
    const results = [] as Array<{ key: string; status: string; error?: string }>;

    for (const key of body.keys) {
      try {
        const payload = await store.read(key);
        if (!payload) {
          results.push({ key, status: 'missing' });
          continue;
        }
        await writer.write(payload.batch);
        await store.remove(key);
        results.push({ key, status: 'replayed' });
      } catch (error) {
        results.push({ key, status: 'failed', error: error instanceof Error ? error.message : 'Unknown error' });
      }
    }

    return c.json({ status: 'ok', results });
  } catch (error) {
    if (error instanceof TokenVerificationError) {
      return c.json({ error: error.message }, 401);
    }
    const message = error instanceof Error ? error.message : 'Unknown error';
    return c.json({ error: message }, 400);
  }
};
