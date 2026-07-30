# 18 — TEST PLAN

---

# Unit

- PurchaseOrderStateMachine transitions  
- PurchaseRequestStateMachine  
- CostEngine WAC math (edge: stock 0, over-receive)  
- SupplierScore weights  
- Hash chain deterministic  

# Integration

- Open PR → Approve → Convert PO → Approve → Send → Receipt → Stock↑ → Cost↑  
- Partial receipt → PO PartiallyReceived  
- Blacklist blocks PO  
- Dual approval required over threshold  
- MT: Company A no ve Supplier Company B  

# Regression

- Cash tests 25/25  
- Foundation tests  
- Payment/Order flows unchanged  
- Recipe sale deduct still works  
- CreatePurchase ad-hoc still works con flag OFF  

# Security

- PurchasingAccess enforced  
- Cross-tenant 401/empty  

# Performance

- Receipt 50 lines < 3s (local)  

# Browser (UAT)

- Supplier CRUD  
- PO wizard  
- Receiving wizard  
- Command Center load  

**Objetivo:** 100% PASS automatizados de unit + compile + migration.
