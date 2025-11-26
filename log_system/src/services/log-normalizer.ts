import type { IngestRequestBody, LogEventInput, NormalizedLogEvent, VerifiedClientContext } from '../types/log';

const severityOrder = new Set([
  'DEBUG',
  'INFO',
  'NOTICE',
  'WARNING',
  'ERROR',
  'CRITICAL',
  'ALERT',
  'EMERGENCY',
]);

const platformSet = new Set(['frontend', 'unity', 'backend']);

const coerceSeverity = (value?: string): NormalizedLogEvent['severity'] =>
  severityOrder.has((value ?? '').toUpperCase())
    ? ((value ?? 'INFO').toUpperCase() as NormalizedLogEvent['severity'])
    : 'INFO';

const coercePlatform = (value?: string): NormalizedLogEvent['platform'] =>
  platformSet.has((value ?? '').toLowerCase())
    ? ((value ?? 'frontend').toLowerCase() as NormalizedLogEvent['platform'])
    : 'frontend';

const coerceTimestamp = (value?: string): string => {
  if (!value) {
    return new Date().toISOString();
  }
  const parsed = Date.parse(value);
  return Number.isNaN(parsed) ? new Date().toISOString() : new Date(parsed).toISOString();
};

const sanitizeMessage = (message?: string): string => {
  if (!message) {
    return 'log_event';
  }
  return message.slice(0, 500);
};

const sanitizeEventType = (eventType?: string): string => {
  if (!eventType) {
    return 'unspecified';
  }
  return eventType.replace(/[^a-zA-Z0-9_:/-]/gu, '').slice(0, 120) || 'unspecified';
};

const sanitizeTags = (tags: Record<string, string> = {}): Record<string, string> => {
  const sanitized: Record<string, string> = {};
  Object.entries(tags).forEach(([key, value]) => {
    if (!key) {
      return;
    }
    const normalizedKey = key.replace(/[^a-zA-Z0-9_:/-]/gu, '').toLowerCase();
    if (!normalizedKey) {
      return;
    }
    sanitized[normalizedKey] = String(value).slice(0, 200);
  });
  return sanitized;
};

const normalizeEvent = (input: LogEventInput, client: VerifiedClientContext): NormalizedLogEvent => ({
  timestamp: coerceTimestamp(input.timestamp),
  platform: coercePlatform(input.platform),
  roomId: input.roomId ?? client.roomId,
  viewerId: input.viewerId,
  requestId: input.requestId,
  severity: coerceSeverity(input.severity),
  eventType: sanitizeEventType(input.eventType),
  message: sanitizeMessage(input.message),
  tags: sanitizeTags(input.tags),
  extraJson: input.extraJson,
  clientId: client.clientId,
});

export const extractEvents = (body: IngestRequestBody): LogEventInput[] => {
  if (Array.isArray(body)) {
    throw new Error('Request body must be an object');
  }
  if (Array.isArray(body.events)) {
    return body.events;
  }
  if (body.event) {
    return [body.event];
  }
  throw new Error('Missing `event` or `events` field');
};

export const normalizeEvents = (
  inputs: LogEventInput[],
  client: VerifiedClientContext,
  maxEvents = 100,
): NormalizedLogEvent[] => {
  if (inputs.length === 0) {
    throw new Error('No events provided');
  }
  if (inputs.length > maxEvents) {
    throw new Error(`Too many events; received ${inputs.length}, max ${maxEvents}`);
  }
  return inputs.map((input) => normalizeEvent(input, client));
};
