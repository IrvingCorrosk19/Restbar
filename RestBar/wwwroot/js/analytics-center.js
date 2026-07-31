(() => {
  const money = v => Number(v || 0).toLocaleString(undefined, { style: 'currency', currency: 'USD' });
  let trendChart;

  function card(title, value, hint, tone) {
    const border = tone === 'danger' ? 'border-danger' : tone === 'warning' ? 'border-warning' : tone === 'success' ? 'border-success' : '';
    return `<div class="col-6 col-md-4 col-xl-3"><div class="card p-3 h-100 ${border}"><small class="text-muted">${title}</small><h4 class="mb-0">${value}</h4><small class="text-muted">${hint || ''}</small></div></div>`;
  }

  async function loadLive() {
    const res = await fetch('/ExecutiveAnalytics/Live?' + AnalyticsFilters.qs());
    if (!res.ok) throw new Error('Live failed');
    const d = await res.json();
    document.getElementById('liveCards').innerHTML = [
      card('Ventas hoy', money(d.salesToday), 'Cierre Completed', 'success'),
      card('Pedidos abiertos', d.openOrders, 'No completed/cancelled'),
      card('Ítems atrasados >20m', d.delayedOrders, 'En cocina', d.delayedOrders > 0 ? 'danger' : ''),
      card('Mesas ocupadas / libres', `${d.occupiedTables} / ${d.freeTables}`, 'Estado actual'),
      card('Cajas abiertas', d.openCashSessions, 'Open/Operating'),
      card('Incidencias caja hoy', d.cashIncidents, 'Variance ≠ 0', d.cashIncidents > 0 ? 'warning' : ''),
      card('Stock crítico / cero', `${d.criticalStock} / ${d.zeroStock}`, 'TrackInventory', d.criticalStock > 0 ? 'warning' : ''),
      card('PO atrasadas', d.overduePurchaseOrders, 'ExpectedDelivery', d.overduePurchaseOrders > 0 ? 'danger' : ''),
      card('Mermas hoy', d.wasteEventsToday, 'waste_events')
    ].join('');
  }

  async function loadPeriod() {
    const q = AnalyticsFilters.qs();
    const [execRes, trendRes, periodRes] = await Promise.all([
      fetch('/ExecutiveAnalytics/ReportData?key=executive-summary&' + q),
      fetch('/ExecutiveAnalytics/ReportData?key=sales-trend&' + q),
      fetch('/ExecutiveAnalytics/ReportData?key=period-results&' + q)
    ]);
    const execPayload = await execRes.json();
    const trendPayload = await trendRes.json();
    const periodPayload = await periodRes.json();
    const ex = execPayload.data?.executive || {};
    document.getElementById('periodKpis').innerHTML = [
      card('Ventas', money(ex.revenue), `${ex.ordersCompleted || 0} órdenes`),
      card('Ticket promedio', money(ex.avgTicket), ''),
      card('Margen bruto est.', `${Number(ex.grossMarginPct || 0).toFixed(1)}%`, 'Estimado'),
      card('Merma', money(ex.wasteCost), ''),
      card('Var. caja', money(ex.cashVariance), '', Math.abs(ex.cashVariance||0) > 0 ? 'warning' : ''),
      card('Stock bajo / PO abiertas', `${ex.lowStockCount || 0} / ${ex.openPoCount || 0}`, '')
    ].join('');

    const trend = Array.isArray(trendPayload.data) ? trendPayload.data : [];
    const labels = trend.map(r => (r.bucket_start || r.bucketStart || '').toString().slice(0, 10));
    const values = trend.map(r => Number(r.revenue || 0));
    const ctx = document.getElementById('chartTrend');
    if (trendChart) trendChart.destroy();
    trendChart = new Chart(ctx, {
      type: 'line',
      data: { labels, datasets: [{ label: 'Ventas', data: values, borderColor: '#0d6efd', tension: .25, fill: false }] },
      options: { responsive: true, plugins: { legend: { display: false } } }
    });

    const rows = periodPayload.data?.rows || periodPayload.data || [];
    const tb = document.querySelector('#tblCompare tbody');
    tb.innerHTML = (Array.isArray(rows) ? rows : []).map(r => {
      const pct = r.pct_change ?? r.pctChange;
      const cls = pct == null ? '' : (Number(pct) < 0 ? 'text-danger' : 'text-success');
      return `<tr><td>${r.metric}</td><td>${r.current_value ?? r.currentValue}</td><td>${r.previous_value ?? r.previousValue}</td><td class="${cls}">${pct == null ? '—' : Number(pct).toFixed(1) + '%'}</td></tr>`;
    }).join('') || '<tr><td colspan="4" class="text-muted p-3">Sin comparación</td></tr>';
  }

  async function loadDecisions() {
    const res = await fetch('/ExecutiveAnalytics/Decisions?' + AnalyticsFilters.qs());
    const list = await res.json();
    const el = document.getElementById('decisionList');
    if (!list?.length) {
      el.innerHTML = '<div class="alert alert-success mb-0">Sin alertas de decisión para el periodo.</div>';
      return;
    }
    el.innerHTML = list.map(d => `
      <div class="card border-${d.priority === 'High' ? 'danger' : d.priority === 'Medium' ? 'warning' : 'secondary'}">
        <div class="card-body py-2">
          <div class="d-flex justify-content-between gap-2">
            <strong>${d.problem}</strong>
            <span class="badge text-bg-${d.priority === 'High' ? 'danger' : 'warning'}">${d.priority}</span>
          </div>
          <div class="small">Métrica ${d.metricCode} · actual ${d.currentValue ?? '—'} · ref ${d.referenceValue ?? '—'}
            ${d.isInference ? ' · <em>inferencia</em>' : ''}</div>
          <div class="small text-primary">→ ${d.suggestedAction}</div>
          <a class="small" href="/ExecutiveAnalytics/Report?key=${encodeURIComponent(d.relatedReportKey)}">Abrir reporte</a>
        </div>
      </div>`).join('');
  }

  async function refreshAll() {
    try {
      await Promise.all([loadLive(), loadPeriod(), loadDecisions()]);
    } catch (e) {
      console.error(e);
      document.getElementById('liveCards').innerHTML = `<div class="col-12"><div class="alert alert-danger">No se pudo cargar analytics. ¿Migración analytics aplicada? ${e.message}</div></div>`;
    }
  }

  document.getElementById('btnRefreshLive')?.addEventListener('click', loadLive);
  AnalyticsFilters.bind(refreshAll);
  refreshAll();
  setInterval(loadLive, 60000);
})();
