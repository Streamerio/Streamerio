import type { Context } from 'hono';
import type { AppEnv } from '../types/app';

export const healthHandler = (c: Context<AppEnv>): Response =>
  c.json({
    status: 'ok',
    timestamp: new Date().toISOString(),
    logName: c.env.GCP_LOG_NAME,
    dlqEnabled: Boolean(c.env.DLQ_BUCKET),
  });
