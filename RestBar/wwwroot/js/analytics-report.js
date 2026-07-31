(() => {
  const key = document.querySelector('[data-report-key]')?.getAttribute('data-report-key');
  let chart, allRows = [], page = 1;
  const pageSize = 25;

  function flatten(data) {
    if (Array.isArray(data)) return data;
    if (data?.rows && Array.isArray(data.rows)) return data.rows;
    if (data?.comparison && Array.isArray(data.comparison)) return data.comparison;
    if (data?.executive) {
      return [Object.fromEntries(Object.entries(data.executive).map(([k, v]) => [k, v]))];
    }
    if (data && typeof data === 'object') return [data];
    return [];
  }

  function cols(rows) {
    const set = new Set();
    rows.forEach(r => Object.keys(r || {}).forEach(k => set.add(k)));
    return [...set];
  }

  function renderTable() {
    const term = (document.getElementById('tblSearch').value || '').toLowerCase();
    let rows = allRows;
    if (term) rows = rows.filter(r => JSON.stringify(r).toLowerCase().includes(term));
    const totalPages = Math.max(1, Math.ceil(rows.length / pageSize));
    page = Math.min(page, totalPages);
    const slice = rows.slice((page - 1) * pageSize, page * pageSize);
    const c = cols(rows);
    const thead = document.querySelector('#reportTable thead');
    const tbody = document.querySelector('#reportTable tbody');
    thead.innerHTML = `<tr>${c.map(x => `<th style="cursor:pointer" data-sort="${x}">${x}</th>`).join('')}</tr>`;
    tbody.innerHTML = slice.map(r => `<tr>${c.map(x => `<td>${r[x] ?? ''}</td>`).join('')}</tr>`).join('')
      || '<tr><td class="text-muted p-3">Sin datos</td></tr>';
    document.getElementById('rowCount').textContent = `${rows.length} filas`;
    document.getElementById('pageInfo').textContent = `Pág. ${page} / ${totalPages}`;
    thead.querySelectorAll('[data-sort]').forEach(th => th.addEventListener('click', () => {
      const k = th.getAttribute('data-sort');
      allRows.sort((a, b) => String(a[k] ?? '').localeCompare(String(b[k] ?? ''), undefined, { numeric: true }));
      renderTable();
    }));
  }

  function renderChart(rows) {
    const c = cols(rows);
    const labelKey = c.find(x => /name|method|metric|hour|bucket|classification|product|station|waiter|supplier|table/i.test(x)) || c[0];
    const valueKey = c.find(x => /revenue|amount|total|qty|variance|margin|count|orders/i.test(x) && x !== labelKey) || c[1];
    const empty = document.getElementById('chartEmpty');
    const canvas = document.getElementById('reportChart');
    if (!labelKey || !valueKey || rows.length === 0) {
      empty.classList.remove('d-none');
      canvas.classList.add('d-none');
      return;
    }
    empty.classList.add('d-none');
    canvas.classList.remove('d-none');
    const top = rows.slice(0, 24);
    if (chart) chart.destroy();
    chart = new Chart(canvas, {
      type: rows.length > 12 ? 'bar' : 'bar',
      data: {
        labels: top.map(r => String(r[labelKey]).slice(0, 28)),
        datasets: [{ label: valueKey, data: top.map(r => Number(r[valueKey]) || 0), backgroundColor: 'rgba(13,110,253,.55)' }]
      },
      options: { responsive: true, plugins: { legend: { display: false } } }
    });
  }

  async function load() {
    const q = AnalyticsFilters.qs();
    const res = await fetch(`/ExecutiveAnalytics/ReportData?key=${encodeURIComponent(key)}&` + q);
    const payload = await res.json();
    if (!res.ok) {
      document.querySelector('#reportTable tbody').innerHTML = `<tr><td class="text-danger p-3">${payload.message || res.status}</td></tr>`;
      return;
    }
    const meta = document.getElementById('reportMeta');
    meta.innerHTML = `<div class="card p-3 h-100">
      <div><strong>Generado</strong> ${payload.generatedAtUtc}</div>
      <div><strong>Usuario</strong> ${payload.user || ''}</div>
      <div><strong>Company</strong> ${payload.filter?.companyId || ''}</div>
      <div><strong>Branch</strong> ${payload.filter?.branchId || ''}</div>
      <div><strong>Periodo</strong> ${payload.filter?.startUtc} → ${payload.filter?.endUtc}</div>
      <div><strong>Moneda</strong> ${payload.filter?.currency || 'USD'} · TZ ${payload.filter?.timeZone || 'UTC'}</div>
    </div>`;
    if (payload.data?.available === false) {
      document.querySelector('#reportTable tbody').innerHTML = `<tr><td class="p-3 text-warning">NO DISPONIBLE: ${payload.data.limitation || ''}</td></tr>`;
      return;
    }
    allRows = flatten(payload.data);
    page = 1;
    renderTable();
    renderChart(allRows);
  }

  function exportUrl(fmt) {
    return `/ExecutiveAnalytics/Export?key=${encodeURIComponent(key)}&format=${fmt}&` + AnalyticsFilters.qs();
  }
  document.getElementById('btnCsv').href = '#';
  document.getElementById('btnXlsx').href = '#';
  document.getElementById('btnPdf').href = '#';
  document.getElementById('btnCsv').addEventListener('click', e => { e.preventDefault(); location.href = exportUrl('csv'); });
  document.getElementById('btnXlsx').addEventListener('click', e => { e.preventDefault(); location.href = exportUrl('xlsx'); });
  document.getElementById('btnPdf').addEventListener('click', e => { e.preventDefault(); location.href = exportUrl('pdf'); });
  document.getElementById('tblSearch').addEventListener('input', () => { page = 1; renderTable(); });
  document.getElementById('btnPrev').addEventListener('click', () => { page = Math.max(1, page - 1); renderTable(); });
  document.getElementById('btnNext').addEventListener('click', () => { page = page + 1; renderTable(); });

  AnalyticsFilters.bind(load);
  load();
})();
