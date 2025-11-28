export type Platform = 'frontend' | 'unity' | 'backend';
export type Severity =
  | 'DEBUG'
  | 'INFO'
  | 'NOTICE'
  | 'WARNING'
  | 'ERROR'
  | 'CRITICAL'
  | 'ALERT'
  | 'EMERGENCY';

export interface LogEventInput {
  timestamp?: string;
  platform?: Platform;
  roomId?: string;
  viewerId?: string;
  requestId?: string;
  severity?: Severity;
  eventType?: string;
  message?: string;
  tags?: Record<string, string>;
  extraJson?: Record<string, unknown>;
}

export interface NormalizedLogEvent {
  timestamp: string;
  platform: Platform;
  roomId?: string;
  viewerId?: string;
  requestId?: string;
  severity: Severity;
  eventType: string;
  message: string;
  tags: Record<string, string>;
  extraJson?: Record<string, unknown>;
  clientId: string;
}

export interface IngestRequestBody {
  events?: LogEventInput[];
  event?: LogEventInput;
}

export interface VerifiedClientContext {
  clientId: string;
  roomId?: string;
  scopes: string[];
}

export interface LogBatch {
  events: NormalizedLogEvent[];
  receivedAt: string;
  requestId: string;
  client: VerifiedClientContext;
}
