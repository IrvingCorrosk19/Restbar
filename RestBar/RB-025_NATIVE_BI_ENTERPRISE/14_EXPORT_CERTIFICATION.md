# 14 — Export Certification

| Format | Engine | Status |
|--------|--------|--------|
| CSV | UTF-8 | PASS |
| Excel | ClosedXML resumen+datos, autofiltro, freeze | PASS |
| PDF | HTML imprimible (Imprimir → Guardar PDF) con mismos filtros/totales | PASS WITH CONDITIONS (binario PDF nativo diferido; QuestPDF Skia no estable en host de build) |
| Print | window.print on report | PASS |

Same tenant/filters/data flatten for all formats.
