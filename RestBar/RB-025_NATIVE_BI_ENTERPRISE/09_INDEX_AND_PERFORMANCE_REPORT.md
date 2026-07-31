# 09 — Index and Performance

Target: executive hub <2s at branch/30d typical.

Mitigations: branch+closed_at index, set-based SQL, live panel 60s poll, no MV yet.

EXPLAIN ANALYZE on VPS post-migrate recommended before declaring unconditional PASS on performance.
