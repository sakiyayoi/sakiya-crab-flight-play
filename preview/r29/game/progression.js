'use strict';
// Offline comparison economy only. This module never reads storage, updates UI,
// sends Unity messages, or changes flight parameters.
(function (root, factory) {
  const api = factory();
  if (typeof module === 'object' && module.exports) module.exports = api;
  else root.CrabProgression = api;
})(typeof window !== 'undefined' ? window : globalThis, function () {
  const SCHEMA = 'crab.flight.progression.v1';
  const STORAGE_KEY = 'crab-flight-core-progression-v1';
  const MAX_LEVEL = 3, MAX_COINS = 9999, MAX_SESSIONS = 32;
  const MAX_BREAKS = 512, MAX_DISTANCE = 10000;
  const MIN_DISTANCE = 10, MIN_ZERO_BREAK_DISTANCE = 80;
  const sessionPattern = /^[A-Za-z0-9_-]{1,64}$/;
  const integer = (n, lo, hi) => Number.isInteger(n) && n >= lo && n <= hi;

  function decodeID(id) {
    if (typeof id !== 'string') return null;
    const parts = id.split(':');
    if (parts.length !== 3 || !sessionPattern.test(parts[0]) ||
        !/^(0|[1-9][0-9]*)$/.test(parts[1]) || !/^[1-9][0-9]*$/.test(parts[2])) return null;
    const generation = Number(parts[1]), attempt = Number(parts[2]);
    return integer(generation, 0, 2147483647) && integer(attempt, 1, 2147483647)
      ? { session: parts[0], generation, attempt } : null;
  }
  function runIdentity(run) {
    if (!run || typeof run !== 'object' || typeof run.session !== 'string' || !sessionPattern.test(run.session) ||
        !integer(run.generation, 0, 2147483647) || !integer(run.attempt, 1, 2147483647)) return null;
    return { session: run.session, generation: run.generation, attempt: run.attempt,
      key: run.session + ':' + run.generation + ':' + run.attempt };
  }
  function validState(state) {
    if (!state || typeof state !== 'object' || Array.isArray(state) || state.schema !== SCHEMA ||
        !integer(state.coins, 0, MAX_COINS) || !integer(state.level, 0, MAX_LEVEL) ||
        !Array.isArray(state.recentRunIDs) || state.recentRunIDs.length > MAX_SESSIONS) return false;
    const sessions = new Set();
    for (const id of state.recentRunIDs) {
      const parsed = decodeID(id);
      if (!parsed || sessions.has(parsed.session)) return false;
      sessions.add(parsed.session);
    }
    return true;
  }
  function snapshot(state) {
    return Object.freeze({ schema: SCHEMA, coins: state.coins, level: state.level,
      recentRunIDs: Object.freeze(state.recentRunIDs.slice()) });
  }
  function checked(state) {
    if (!validState(state)) throw new TypeError('Invalid progression state');
    return state;
  }
  function create() { return snapshot({ coins: 0, level: 0, recentRunIDs: [] }); }

  // canWrite=false is deliberate: the caller must retain an unreadable original
  // value instead of overwriting it with this temporary level-zero state.
  function read(raw) {
    if (raw === null || raw === undefined) return { state: create(), status: 'fresh', canWrite: true };
    if (typeof raw !== 'string') return { state: create(), status: 'invalid-type', canWrite: false };
    let parsed;
    try { parsed = JSON.parse(raw); }
    catch (_) { return { state: create(), status: 'invalid-json', canWrite: false }; }
    if (!parsed || parsed.schema !== SCHEMA)
      return { state: create(), status: 'unsupported-schema', canWrite: false };
    if (!validState(parsed)) return { state: create(), status: 'invalid-state', canWrite: false };
    return { state: snapshot(parsed), status: 'loaded', canWrite: true };
  }
  function serialize(state) {
    checked(state);
    return JSON.stringify({ schema: SCHEMA, coins: state.coins, level: state.level,
      recentRunIDs: state.recentRunIDs.slice() });
  }
  function nextCost(state) {
    checked(state);
    return state.level === MAX_LEVEL ? null : 30 + state.level * 20;
  }
  function settle(state, run) {
    checked(state);
    const unchanged = status => ({ state, status, earned: 0, calculatedReward: 0 });
    const id = runIdentity(run);
    if (!id) return unchanged('invalid-run-id');
    if (run.finished !== true || (run.phase !== undefined && run.phase !== 'result'))
      return unchanged('not-finished');
    const priorID = state.recentRunIDs.find(value => decodeID(value).session === id.session);
    const prior = priorID && decodeID(priorID);
    if (prior && (id.attempt <= prior.attempt || id.generation < prior.generation))
      return unchanged('duplicate-or-stale');
    const hasFault = run.physicsFault !== undefined && run.physicsFault !== null && run.physicsFault !== '';
    if (!hasFault && (typeof run.distance !== 'number' || !Number.isFinite(run.distance) || run.distance < 0 ||
        typeof run.destructionCount !== 'number' || !Number.isFinite(run.destructionCount) || run.destructionCount < 0))
      return unchanged('invalid-run-metrics');
    const breaks = hasFault ? 0 : Math.min(MAX_BREAKS, Math.floor(run.destructionCount));
    const distance = hasFault ? 0 : Math.min(MAX_DISTANCE, run.distance);
    const eligible = !hasFault && distance >= MIN_DISTANCE && (breaks > 0 || distance >= MIN_ZERO_BREAK_DISTANCE);
    const reward = eligible ? breaks * 2 + Math.min(20, Math.floor(distance / 20)) : 0;
    const earned = Math.min(reward, MAX_COINS - state.coins);
    const ids = state.recentRunIDs.filter(value => decodeID(value).session !== id.session);
    ids.push(id.key);
    const next = snapshot({ coins: state.coins + earned, level: state.level,
      recentRunIDs: ids.slice(-MAX_SESSIONS) });
    return { state: next, status: hasFault ? 'fault-recorded' : reward === 0 ? 'no-reward' : earned === 0 ? 'wallet-full' : 'rewarded',
      earned, calculatedReward: reward };
  }
  // Capture expectedLevel when presenting the action, and reuse it for a retry.
  // The same request cannot buy the next tier after a successful first call.
  function purchase(state, request) {
    checked(state);
    const unchanged = status => ({ state, status, spent: 0 });
    if (!request || !integer(request.expectedLevel, 0, MAX_LEVEL)) return unchanged('invalid-purchase');
    if (request.expectedLevel !== state.level) return unchanged('stale-request');
    const cost = nextCost(state);
    if (cost === null) return unchanged('max-level');
    if (state.coins < cost) return unchanged('insufficient-coins');
    return { state: snapshot({ coins: state.coins - cost, level: state.level + 1,
      recentRunIDs: state.recentRunIDs }), status: 'purchased', spent: cost };
  }
  return Object.freeze({ SCHEMA, STORAGE_KEY, MAX_LEVEL, MAX_COINS, MAX_SESSIONS,
    MIN_DISTANCE, MIN_ZERO_BREAK_DISTANCE, create, read, settle, purchase, serialize, nextCost });
});
