# 09 — SignalR Performance (RB-1002)

## Current design (post RB-1001)

- Tenant groups: `c_{company:N}_kitchen|orders|table_all|stock|cash_dashboard|station_*`.
- No global `kitchen` / `orders` broadcast.

## This cycle

- No SignalR protocol changes (out of DB scope except reducing DB work behind hub fan-out).
- Kitchen hub consumers benefit from faster KitchenService polls (indexes + NoTracking).

## Observations

| ID | Item |
|----|------|
| O-SIG-01 | Payload size of kitchen push not compressed; OK at current board size. |
| O-SIG-02 | Scale-out needs Redis backplane — not present; single `restbar_web` instance today. |
