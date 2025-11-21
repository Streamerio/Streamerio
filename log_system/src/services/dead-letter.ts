import type { LogBatch } from '../types/log';
import { generateStorageKey } from '../utils/id';


export interface DeadLetterPayload {
  batch: LogBatch;
  reason: string;
  storedAt: string;
}

export class DeadLetterStore {
  constructor(private readonly bucket?: R2Bucket) {}

  available(): boolean {
    return Boolean(this.bucket);
  }

  async persist(batch: LogBatch, reason: string): Promise<string | null> {
    if (!this.bucket) {
      return null;
    }
    const payload: DeadLetterPayload = {
      batch,
      reason,
      storedAt: new Date().toISOString(),
    };
    const key = generateStorageKey('dlq');
    await this.bucket.put(key, JSON.stringify(payload), {
      httpMetadata: {
        contentType: 'application/json',
      },
    });
    return key;
  }

  async read(key: string): Promise<DeadLetterPayload | null> {
    if (!this.bucket) {
      return null;
    }
    const object = await this.bucket.get(key);
    if (!object) {
      return null;
    }
    const text = await object.text();
    return JSON.parse(text) as DeadLetterPayload;
  }

  async remove(key: string): Promise<void> {
    await this.bucket?.delete(key);
  }
}
