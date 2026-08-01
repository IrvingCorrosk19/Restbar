# 04 — TEST DATA MATRIX

## Seed canónico: `ThreeCompaniesCertSeeder`

| Tenant | Company | Branch | Admin | Managers / ops emails | Tables | Areas | Stations |
|--------|---------|--------|-------|----------------------|--------|-------|----------|
| A | Restaurante Costa | Costa Centro | admin@costa.restbar.com | manager/cajero/chef/bartender/mesero1–2@costa.restbar.com | C-* (10) | Piso 1 Salón, Terraza; Piso 2 Salón, Terraza | Cocina/Bar Piso 1–2 |
| B | Restaurante Norte | Norte Mall | admin@norte.restbar.com | *@norte.restbar.com | NM-* (10) | Piso 1 Principal, Piso 2 VIP | Cocina Principal, Bar, Parrilla Norte |
| C | Restaurante Sur | Sur Hotel | admin@sur.restbar.com | *@sur.restbar.com | S-* (15) | Piso 1–2 Hotel, Piso 3 Rooftop | Cocina/Bar Hotel + Rooftop |

Password: `123456`  
Producto exclusivo: `Producto Exclusivo Costa|Norte` (+ Sur)  
SuperAdmin: `superadmin@restbar.com`

## Demo default (Tenant operacional VPS)

| Email | Rol típico |
|-------|------------|
| admin@restbar.com | admin |
| mesero@ / cajero@ / chef@ / bartender@restbar.com | ops |

## MFA

Privilegiados: TOTP. Seed cert compartido automatización: `JBSWY3DPEHPK3PXP` vía env `RESTBAR_MFA_SECRET`.

## Diferenciadores anti-contaminación

- Prefijos mesa C / NM / S  
- Nombres empresa Restaurante Costa|Norte|Sur  
- Productos exclusivos por company  
- Emails dominio `@costa|norte|sur.restbar.com`

## Cómo sembrar

- **Development:** `POST/GET Seed/SeedThreeCompaniesCertification` (gate Dev)  
- **Production VPS:** flag seed off → usar datos ya sembrados o aplicar seed desde contenedor con entorno Development puntual / script SQL controlado (nunca alterar resultados a mano para forzar PASS de un caso fallido)
