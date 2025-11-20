import type { Context } from 'hono';
import { TokenVerificationError, verifyBearerToken } from '../security/token';
import type { AppEnv } from '../types/app';
import type { IngestRequestBody, LogBatch } from '../types/log';
import { extractEvents, normalizeEvents } from '../services/log-normalizer';
import { LogRouter, LogRouterError } from '../services/log-router';

export const ingestHandler = async (c: Context<AppEnv>): Promise<Response> => {
  const authorization = c.req.header('Authorization');
  if (!authorization) {
    return c.json({ error: 'Missing Authorization header' }, 401);
  }
  try {
    const verified = await verifyBearerToken(authorization, c.env.CLIENT_LOG_TOKEN_SECRET);
    const body = (await c.req.json()) as IngestRequestBody;
    const inputs = extractEvents(body);
    const normalized = normalizeEvents(inputs, verified, 100);

    const batch: LogBatch = {
      events: normalized,
      receivedAt: new Date().toISOString(),
      requestId: c.get('requestId'),
      client: {
        clientId: verified.clientId,
        roomId: verified.roomId,
        scopes: verified.scopes,
      },
    };

    const router = new LogRouter(c.env);
    const result = await router.dispatch(batch);

    return c.json({
      status: 'ok',
      mode: result.mode,
      deadLetterKey: result.deadLetterKey ?? undefined,
      count: batch.events.length,
    });
  } catch (error) {
    if (error instanceof TokenVerificationError) {
      return c.json({ error: error.message }, 401);
    }
    if (error instanceof LogRouterError) {
      return c.json({ error: error.message, deadLetterKey: error.deadLetterKey ?? undefined }, 502);
    }
    const message = error instanceof Error ? error.message : 'Unknown error';
    return c.json({ error: message }, 400);
  }
};
