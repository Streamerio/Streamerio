import { Hono } from 'hono';
import type { AppEnv } from './types/app';
import { generateRequestId } from './utils/id';
import { healthHandler } from './routes/health';
import { ingestHandler } from './routes/ingest';
import { replayHandler } from './routes/replay';

export const createApp = () => {
  const app = new Hono<AppEnv>();

  app.use('*', async (c, next) => {
    const requestId = c.req.header('cf-ray') ?? generateRequestId();
    c.set('requestId', requestId);
    await next();
  });

  app.get('/healthz', healthHandler);
  app.post('/v1/ingest', ingestHandler);
  app.post('/v1/replay', replayHandler);

  app.notFound((c) => c.json({ error: 'Not Found' }, 404));
  app.onError((err, c) => {
    console.error('Unhandled error', err);
    return c.json({ error: 'Internal Server Error' }, 500);
  });

  return app;
};
