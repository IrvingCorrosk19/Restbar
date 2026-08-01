/**
 * RestBar offline POS queue (IndexedDB).
 * Queues failed POS JSON POSTs and replays when online.
 */
(function () {
  const DB_NAME = 'restbar-offline-pos';
  const STORE = 'queue';
  const DB_VERSION = 1;

  function openDb() {
    return new Promise((resolve, reject) => {
      const req = indexedDB.open(DB_NAME, DB_VERSION);
      req.onupgradeneeded = () => {
        const db = req.result;
        if (!db.objectStoreNames.contains(STORE)) {
          db.createObjectStore(STORE, { keyPath: 'id', autoIncrement: true });
        }
      };
      req.onsuccess = () => resolve(req.result);
      req.onerror = () => reject(req.error);
    });
  }

  async function enqueue(entry) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(STORE, 'readwrite');
      tx.objectStore(STORE).add({
        url: entry.url,
        method: entry.method || 'POST',
        headers: entry.headers || { 'Content-Type': 'application/json' },
        body: entry.body,
        createdAt: Date.now(),
      });
      tx.oncomplete = () => resolve();
      tx.onerror = () => reject(tx.error);
    });
  }

  async function listQueue() {
    const db = await openDb();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(STORE, 'readonly');
      const req = tx.objectStore(STORE).getAll();
      req.onsuccess = () => resolve(req.result || []);
      req.onerror = () => reject(req.error);
    });
  }

  async function removeId(id) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(STORE, 'readwrite');
      tx.objectStore(STORE).delete(id);
      tx.oncomplete = () => resolve();
      tx.onerror = () => reject(tx.error);
    });
  }

  async function flushQueue() {
    if (!navigator.onLine) return { flushed: 0, remaining: (await listQueue()).length };
    const items = await listQueue();
    let flushed = 0;
    for (const item of items) {
      try {
        const res = await fetch(item.url, {
          method: item.method,
          headers: item.headers,
          body: item.body,
          credentials: 'same-origin',
        });
        if (res.ok || res.status === 409) {
          await removeId(item.id);
          flushed++;
        }
      } catch (_) {
        break;
      }
    }
    return { flushed, remaining: (await listQueue()).length };
  }

  /** Wrap fetch for POS mutation endpoints — queue on network failure. */
  const nativeFetch = window.fetch.bind(window);
  window.fetch = async function (input, init) {
    const url = typeof input === 'string' ? input : input.url;
    const method = (init && init.method) || (typeof input !== 'string' && input.method) || 'GET';
    const isPosMutation =
      method !== 'GET' &&
      method !== 'HEAD' &&
      (/\/Order\//i.test(url) || /\/api\/Payment/i.test(url));

    try {
      return await nativeFetch(input, init);
    } catch (err) {
      if (isPosMutation && init && init.body) {
        await enqueue({
          url,
          method,
          headers: init.headers || { 'Content-Type': 'application/json' },
          body: typeof init.body === 'string' ? init.body : JSON.stringify(init.body),
        });
        if ('serviceWorker' in navigator && 'SyncManager' in window) {
          const reg = await navigator.serviceWorker.ready;
          try {
            await reg.sync.register('restbar-pos-sync');
          } catch (_) {}
        }
        return new Response(JSON.stringify({ success: true, offlineQueued: true }), {
          status: 202,
          headers: { 'Content-Type': 'application/json' },
        });
      }
      throw err;
    }
  };

  function ensureManifestLink() {
    if (!document.querySelector('link[rel="manifest"]')) {
      const link = document.createElement('link');
      link.rel = 'manifest';
      link.href = '/manifest.webmanifest';
      document.head.appendChild(link);
    }
  }

  async function registerSw() {
    if (!('serviceWorker' in navigator)) return;
    try {
      await navigator.serviceWorker.register('/sw-restbar.js');
    } catch (_) {}
  }

  window.addEventListener('online', () => {
    flushQueue();
  });
  navigator.serviceWorker &&
    navigator.serviceWorker.addEventListener('message', (ev) => {
      if (ev.data && ev.data.type === 'RESTBAR_OFFLINE_SYNC') flushQueue();
    });

  ensureManifestLink();
  registerSw();
  window.RestBarOfflinePos = { enqueue, flushQueue, listQueue };

  if (document.readyState === 'complete') flushQueue();
  else window.addEventListener('load', () => flushQueue());
})();
