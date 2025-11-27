declare global {
  // eslint-disable-next-line @typescript-eslint/consistent-type-definitions
  interface CryptoKeyPair {} // placeholder for TS when not using DOM libs
}

export interface Env {
  CLIENT_LOG_TOKEN_SECRET: string;
  GCP_SERVICE_ACCOUNT_JSON: string;
  GCP_LOG_NAME: string;
  GCP_RESOURCE_TYPE?: string;
  REPLAY_MAX_BATCH?: string;
  DLQ_BUCKET?: R2Bucket;
}
