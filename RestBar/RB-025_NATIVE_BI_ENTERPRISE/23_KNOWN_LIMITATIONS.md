# 23 — Known Limitations

1. Export "PDF" entrega HTML imprimible (guardar como PDF desde el navegador); binario PDF nativo diferido (QuestPDF/Skia no viable en build host).
2. Menu engineering uses median qty/margin estimates — inference.
3. Gross margin is estimated COGS.
4. Live panel is polling 60s, not SignalR (intencional: no saturar POS).
5. Not all 48 aspirational report titles have dedicated advanced UX — catalog-driven shell.
6. AdvancedReports Excel: ClosedXML real; PDF = HTML print (alineado a Executive Analytics).
7. KPIs TAX / GUESTS / RESERVED / WAREHOUSE / COUNT_ACC: NO DISPONIBLE sin cambio de modelo.
