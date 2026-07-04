# Configuración RestBar - Despliegue en VPS

**Misma estructura que FixHub/PanamaTravelHub:** directorio propio, puerto propio, dominio propio. No se tocan las demás apps.

---

## Aplicaciones y dominios (VPS 164.68.99.83)

| Aplicación       | Puerto | Directorio             | Dominio                    |
|------------------|--------|------------------------|----------------------------|
| CarnetQR         | 80     | /opt/apps/aspnet       | carnet.autonomousflow.lat |
| FixHub           | 8081   | /opt/apps/fixhub       | fixhub.autonomousflow.lat |
| PanamaTravelHub  | 8082   | /opt/apps/panamatravelhub | travel.autonomousflow.lat |
| n8n              | 8083   | /opt/apps/n8n          | n8n.autonomousflow.lat    |
| **RestBar**      | **8084** | **/opt/apps/restbar**  | **restbar.autonomousflow.lat** |

- **RestBar:** dominio **restbar.autonomousflow.lat** (configurado en DNS, ej. Namecheap: registro A restbar → 164.68.99.83). Puerto 8084, contenedores **restbar_***, red **restbar_net**.

---

## Desplegar RestBar

Desde tu PC (PowerShell):

```powershell
cd "C:\Proyectos\RestBar\RestBar\RestBar\Com"
.\deploy-restbar.ps1
```

**Requisitos:**
- PuTTY (plink) en `C:\Program Files\PuTTY\plink.exe`
- Repositorio RestBar en GitHub (Restbar.git); el script clona o actualiza en el VPS en `/opt/apps/restbar`

**Qué hace el script:**
1. Clona o actualiza el repo en `/opt/apps/restbar`
2. Crea `.env` con PostgreSQL y `ASPNETCORE_ENVIRONMENT=Production`
3. Ejecuta `docker compose up -d --build` (restbar_postgres + restbar_web)

Al arrancar, la app aplica migraciones EF automáticamente (Production).

---

## Acceso

- **Por IP:** http://164.68.99.83:8084  
- **Por dominio (cuando nginx + SSL estén configurados):** https://restbar.autonomousflow.lat

---

## Variables .env (en el servidor)

En `/opt/apps/restbar/.env` el script deja algo como:

```
POSTGRES_DB=RestBar
POSTGRES_USER=restbaruser
POSTGRES_PASSWORD=RestBar2024!Secure

ASPNETCORE_ENVIRONMENT=Production
```

Para cambiar la contraseña de PostgreSQL, edita `.env` en el VPS y reinicia:

```bash
cd /opt/apps/restbar && docker compose up -d --force-recreate restbar_web
```

---

## Completar Nginx y SSL en el VPS (una sola vez)

Si en DNS (Namecheap, etc.) ya tienes **restbar** apuntando a **164.68.99.83**, sigue estos pasos **en el VPS** para dejar RestBar servido por HTTPS sin tocar las demás apps.

### 1. Subir el archivo Nginx de RestBar

Desde tu PC (PowerShell), copia el archivo al servidor:

```powershell
cd "C:\Proyectos\RestBar\RestBar\RestBar\Com"
scp -P 22 restbar/nginx-restbar-sites-available.conf root@164.68.99.83:/etc/nginx/sites-available/restbar.autonomousflow.lat
```

(O usa WinSCP/PuTTY para copiar `Com/restbar/nginx-restbar-sites-available.conf` a `/etc/nginx/sites-available/restbar.autonomousflow.lat` en el VPS.)

### 2. En el VPS: activar sitio y probar Nginx

```bash
ln -sf /etc/nginx/sites-available/restbar.autonomousflow.lat /etc/nginx/sites-enabled/
nginx -t
```

Si `nginx -t` sale OK, recarga Nginx:

```bash
systemctl reload nginx
```

### 3. Ampliar el certificado SSL con restbar.autonomousflow.lat

Si el certificado actual de Let's Encrypt ya incluye otros dominios (p. ej. carnet, fixhub, travel, n8n), amplíalo con:

```bash
certbot certonly --webroot -w /var/www/certbot --expand \
  -d carnet.autonomousflow.lat \
  -d fixhub.autonomousflow.lat \
  -d travel.autonomousflow.lat \
  -d n8n.autonomousflow.lat \
  -d restbar.autonomousflow.lat
```

Ajusta la lista de `-d` a los dominios que ya tengas en ese certificado y añade `-d restbar.autonomousflow.lat`. Si Certbot te pide renovar/expandir, confirma. Luego recarga Nginx de nuevo:

```bash
systemctl reload nginx
```

### 4. Comprobar

- `http://restbar.autonomousflow.lat` → debe redirigir a `https://restbar.autonomousflow.lat`
- `https://restbar.autonomousflow.lat` → debe mostrar la app RestBar (HTTP 200).

---

**Archivos en el repo:** `Com/restbar/nginx-restbar-http.conf` y `Com/restbar/nginx-restbar-https.conf` son los bloques por separado; `Com/restbar/nginx-restbar-sites-available.conf` es el archivo completo listo para copiar a `sites-available`. El script `deploy-restbar.ps1` **no** modifica nginx ni certbot (no toca las demás apps).

---

## Verificación

```bash
docker ps --filter name=restbar
curl -s -o /dev/null -w "%{http_code}" http://164.68.99.83:8084
```

---

## Archivos creados (RestBar)

| Archivo | Descripción |
|---------|-------------|
| `RestBar/Dockerfile` | Imagen de la app (raíz del repo) |
| `RestBar/docker-compose.yml` | PostgreSQL + web, puerto 8084 (raíz del repo) |
| `Com/deploy-restbar.ps1` | Deploy en VPS (clonar, .env, compose) |
| `Com/restbar/nginx-restbar-http.conf` | Bloque HTTP (80) restbar.autonomousflow.lat para certbot |
| `Com/restbar/nginx-restbar-https.conf` | Bloque HTTPS (443) proxy a 8084 |
| `Com/restbar/nginx-restbar-sites-available.conf` | **Archivo completo** HTTP+HTTPS para copiar a `/etc/nginx/sites-available/restbar.autonomousflow.lat` en el VPS |
| `Com/restore-restbar-db.ps1` | **Subir y restaurar la DB:** sube `restbarIIC.sql` al VPS y lo restaura en el contenedor `restbar_postgres`. Ejecutar desde `Com`: `.\restore-restbar-db.ps1` |
| `Com/CONFIGURACION_RESTBAR_VPS.md` | Este documento |

---

## No interferir con otras apps

- RestBar usa **solo** el puerto **8084** y recursos con prefijo **restbar_**.
- No se modifican CarnetQR (80), FixHub (8081), PanamaTravelHub (8082) ni n8n (8083).
- Nginx solo se toca si tú añades manualmente el bloque de restbar (o un script aparte que lo haga).
