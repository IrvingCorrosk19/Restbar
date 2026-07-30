# 02 — REQUIREMENTS

## MUST
FR-01 Conversación NL ES/EN → intent → tools → explicación + recomendación  
FR-02 Context: Company/Branch/Role/Permissions  
FR-03 Tool registry: sales, FC, procurement, cash, inventory alerts, executive snapshot  
FR-04 Recommendations ordenadas por impacto  
FR-05 Decision briefing “¿qué hago hoy?”  
FR-06 Actions RBAC-gated (PR draft, links a módulos; writes peligrosas solo roles altos)  
FR-07 AI audit (pregunta, respuesta, tools, duración, hash)  
FR-08 Memory conversación persistente  
FR-09 IAiProvider abstraction (Deterministic v1)  
FR-10 EnableCopilot flag  
FR-11 Prompt injection / hallucination guards (deny unknown actions, cite tools)  
FR-12 <3s consultas típicas  

## NO
Acoplar a un vendor LLM · ejecutar pagos · bypasear tenant
