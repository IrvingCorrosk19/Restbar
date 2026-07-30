# 14 — UX DESIGN

**Principio:** Premium enterprise — velocidad cajero, confianza gerente, claridad auditor.  
**Stack UI:** Razor + JS (consistente RestBar) — no SPA rewrite.

---

# Pantallas v1

## 1. Opening Wizard (`/Cash/Open`)
- Step 1: Select register (cards with status)  
- Step 2: Opening float (denominations grid + quick total)  
- Step 3: Confirm + optional supervisor PIN  
- Success → Cash Dashboard  

## 2. Cash Dashboard (`/Cash` — cajero home)
- Header: Register, Session #, Open duration, Opened by  
- KPI cards: Expected cash (hidden if blind), Sales today, Tips, Paid-out  
- Quick actions: Paid-in, Paid-out, Start close, View movements  
- Live feed: last 10 movements (SignalR)  
- Alert banner: variance risk, stale session  

## 3. Cash Movements (`/Cash/Movements`)
- Filterable table: time, type, amount, user, payment link, reason  
- Export CSV  

## 4. Close / Arqueo (`/Cash/Close`)
- Step 1: Pause payments warning  
- Step 2: Denomination count (blind mode hides expected)  
- Step 3: Variance reveal + reason if needed  
- Step 4: Approval pending UI if required  
- Step 5: Z-report preview + print  

## 5. Session History (`/Cash/History`)
- Past sessions searchable by date, register, cashier, variance  

## 6. Supervisor Panel (`/Cash/Supervisor`)
- Open sessions branch-wide  
- Pending approvals queue  
- Incidents  
- Override log  

## 7. Manager Panel (`/Cash/Manager`)
- Register config link  
- Reopen requests  
- Threshold settings  
- Branch day summary  

## 8. Auditor Panel (`/Cash/Audit`)
- Hash chain verify  
- Timeline forense  
- Export signed bundle  

---

# POS integration UX

- Payment modal: badge "Caja: CAJA-01 abierta" or warning "Abrir caja para efectivo"  
- No duplicate payment UI — extend existing `payments.js`  

---

# Mobile responsive

Cajero tablet: dashboard + close wizard usable 768px+.

---

# Accessibility

High contrast variance alerts; keyboard shortcuts close wizard; ES primary.

---

# Empty states

- No register configured → manager setup CTA  
- No open session → opening wizard CTA  
- Module disabled → feature flag message  

---

# No implementar hasta aprobación diseño + wireframes sign-off.

Wireframes: low-fi descriptivos en implementación (Figma opcional fase dev).
