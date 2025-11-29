import { check, sleep } from "k6";
import exec from "k6/execution";
import http from "k6/http";
import { Rate } from "k6/metrics";

const BASE_URL = __ENV.BASE_URL || "http://localhost:8888";
const ROOM_ID = __ENV.ROOM_ID || "dev-room";
const PUSH_INTERVAL = Number(__ENV.PUSH_INTERVAL || 1.2);
const EVENTS_PER_REQUEST = Number(__ENV.EVENTS_PER_REQUEST || 3);
const VUS = Number(__ENV.VUS || 100);
const DURATION = __ENV.DURATION || "2m";
const VIEWER_ID_RETRY = Number(__ENV.VIEWER_ID_RETRY || 3);
const VIEWER_ID_RETRY_INTERVAL = Number(__ENV.VIEWER_ID_RETRY_INTERVAL || 0.5);
const IDENTITY_TIMEOUT = __ENV.IDENTITY_TIMEOUT || "6s";
const EVENT_TIMEOUT = __ENV.EVENT_TIMEOUT || "10s";

const BUTTONS = ["skill1", "skill2", "skill3", "enemy1", "enemy2", "enemy3"];

const viewerIdErrorRate = new Rate("viewer_id_error_rate");
const eventPostErrorRate = new Rate("event_post_error_rate");

// k6 の各 VU は独立したランタイムを持つため、グローバル変数でも VU ごとに別管理される
let viewerId = null;

export const options = {
  scenarios: {
    viewer_load: {
      executor: "constant-vus",
      vus: VUS,
      duration: DURATION,
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],
    "http_req_duration{type:get_identity}": ["p(95)<400"],
    "http_req_duration{type:post_events}": ["p(95)<600"],
    viewer_id_error_rate: ["rate<0.02"],
    event_post_error_rate: ["rate<0.02"],
  },
};

function ensureViewerId(jar) {
  if (viewerId) return viewerId;

  for (let attempt = 0; attempt < VIEWER_ID_RETRY; attempt += 1) {
    const res = http.get(`${BASE_URL}/get_viewer_id`, {
      jar,
      tags: { type: "get_identity" },
      timeout: IDENTITY_TIMEOUT,
    });

    let payload = null;
    try {
      payload = res.json();
    } catch (err) {
      // ignore JSON parse errors, handled by ok flag
    }
    const ok =
      res.status === 200 && payload && typeof payload.viewer_id === "string";
    viewerIdErrorRate.add(!ok);

    if (ok) {
      viewerId = payload.viewer_id;
      return viewerId;
    }

    if (attempt < VIEWER_ID_RETRY - 1) {
      sleep(VIEWER_ID_RETRY_INTERVAL);
    }
  }

  console.error(
    `VU ${exec.vu.idInInstance}: failed to obtain viewer_id after ${VIEWER_ID_RETRY} attempts`
  );
  return null;
}

function buildPushEvents() {
  // 同じボタンが複数回選ばれた場合は push_count をまとめる
  const eventMap = new Map();

  for (let i = 0; i < EVENTS_PER_REQUEST; i += 1) {
    const button = BUTTONS[Math.floor(Math.random() * BUTTONS.length)];
    const current = eventMap.get(button) || 0;
    eventMap.set(button, current + 1);
  }

  // Map を配列に変換
  return Array.from(eventMap.entries()).map(([button_name, push_count]) => ({
    button_name,
    push_count,
  }));
}

export default function () {
  const jar = http.cookieJar();
  const ensuredViewerId = ensureViewerId(jar);
  if (!ensuredViewerId) {
    sleep(PUSH_INTERVAL);
    return;
  }

  const payload = JSON.stringify({
    viewer_id: ensuredViewerId,
    push_events: buildPushEvents(),
  });

  const res = http.post(
    `${BASE_URL}/api/rooms/${encodeURIComponent(ROOM_ID)}/events`,
    payload,
    {
      headers: { "Content-Type": "application/json" },
      jar,
      tags: { type: "post_events" },
      timeout: EVENT_TIMEOUT,
    }
  );

  const ok = check(res, {
    "events 200": (r) => r.status === 200,
  });

  eventPostErrorRate.add(!ok);

  sleep(PUSH_INTERVAL);
}
