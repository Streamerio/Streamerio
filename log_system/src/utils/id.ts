export const generateRequestId = (): string => crypto.randomUUID();

export const generateStorageKey = (prefix: string): string => {
  const epoch = Date.now();
  const random = crypto.randomUUID().replace(/-/gu, '').slice(0, 8);
  return `${prefix}/${epoch}-${random}`;
};
