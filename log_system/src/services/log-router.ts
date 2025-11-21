import type { Env } from '../types/env';
import type { LogBatch } from '../types/log';
import { CloudLoggingWriter } from './gcp';
import { DeadLetterStore } from './dead-letter';

export interface DispatchResult {
  mode: 'direct';
  deadLetterKey?: string | null;
}

export class LogRouterError extends Error {
  constructor(
    message: string,
    public readonly deadLetterKey: string | null = null,
    public readonly innerError?: Error,
  ) {
    super(message);
    this.name = 'LogRouterError';
  }
}

export class LogRouter {
  private readonly writer: CloudLoggingWriter;

  private readonly deadLetter: DeadLetterStore;

  constructor(env: Env) {
    this.writer = new CloudLoggingWriter(env);
    this.deadLetter = new DeadLetterStore(env.DLQ_BUCKET);
  }

  async dispatch(batch: LogBatch): Promise<DispatchResult> {
    try {
      await this.writer.write(batch);
      return { mode: 'direct' };
    } catch (error) {
      const key = await this.deadLetter.persist(batch, (error as Error).message);
      throw new LogRouterError('Failed to write logs', key, error as Error);
    }
  }
}
