// BookSpace — დატვირთვის ტესტი (k6) არაფუნქციური მოთხოვნებისთვის (README → „არაფუნქციური მოთხოვნები“).
//
//   NFR-1: ოთახების სია 50 ერთდროული მომხმარებლისას, მოთხოვნების 95% ≤ 2 წამი.
//   NFR-2: 100 ერთდროული მომხმარებელი, შეცდომების წილი < 1%.
//
// გაშვება:  k6 run -e BASE_URL=http://localhost:5180 load/rooms-load.js
// CI-ში ეშვება ყოველ push-ზე (.github/workflows/ci.yml → e2e-and-load), შემოკლებული ხანგრძლივობით.
// თუ რომელიმე threshold ირღვევა, k6 აბრუნებს არანულოვან კოდს და CI წითლდება.

import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = (__ENV.BASE_URL || 'http://localhost:5180').replace(/\/$/, '');
const DURATION = __ENV.DURATION || '45s';

export const options = {
  scenarios: {
    // NFR-1
    rooms_list: {
      executor: 'constant-vus',
      vus: 50,
      duration: DURATION,
      exec: 'roomsList',
      tags: { nfr: 'nfr1' },
    },
    // NFR-2 — იწყება NFR-1-ის შემდეგ
    browsing: {
      executor: 'constant-vus',
      vus: 100,
      duration: DURATION,
      startTime: DURATION,
      exec: 'browsing',
      tags: { nfr: 'nfr2' },
    },
  },
  thresholds: {
    'http_req_duration{nfr:nfr1}': ['p(95)<2000'],
    'http_req_failed{nfr:nfr2}': ['rate<0.01'],
    checks: ['rate>0.99'],
  },
};

export function roomsList() {
  const res = http.get(`${BASE_URL}/Rooms`);
  check(res, { 'rooms page 200': (r) => r.status === 200 });
  sleep(1);
}

export function browsing() {
  const home = http.get(`${BASE_URL}/`);
  check(home, { 'home 200': (r) => r.status === 200 });

  const rooms = http.get(`${BASE_URL}/Rooms?size=small`);
  check(rooms, { 'filtered rooms 200': (r) => r.status === 200 });

  const details = http.get(`${BASE_URL}/Rooms/Details/1`);
  check(details, { 'schedule 200': (r) => r.status === 200 });
  sleep(1);
}
