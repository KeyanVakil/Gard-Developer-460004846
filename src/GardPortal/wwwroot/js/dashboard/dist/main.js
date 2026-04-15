/**
 * GardPortal Dashboard — self-contained ES module
 *
 * Dynamically loads Vue 3 (ESM CDN) and Chart.js (UMD CDN via injected <script>),
 * then mounts the full SPA to #app.  No build step required.
 *
 * API contracts (all responses: { data: ..., errors: [] }):
 *   GET /api/dashboard/summary
 *   GET /api/dashboard/claims-by-category?vesselType=X
 *   GET /api/dashboard/policies-by-coverage?vesselType=X
 *   GET /api/dashboard/claims-trend?vesselType=X
 */

// ---------------------------------------------------------------------------
// 1. Load Chart.js as a global before anything else (UMD build sets window.Chart)
// ---------------------------------------------------------------------------
function loadScript(src) {
  return new Promise((resolve, reject) => {
    if (document.querySelector(`script[src="${src}"]`)) { resolve(); return; }
    const s = document.createElement('script');
    s.src = src;
    s.onload = resolve;
    s.onerror = reject;
    document.head.appendChild(s);
  });
}

await loadScript('https://cdn.jsdelivr.net/npm/chart.js@4.4.3/dist/chart.umd.min.js');

// ---------------------------------------------------------------------------
// 2. Load Vue 3 from ESM CDN
// ---------------------------------------------------------------------------
const {
  createApp, ref, reactive, computed,
  onMounted, onBeforeUnmount, watch, nextTick,
} = await import('https://unpkg.com/vue@3/dist/vue.esm-browser.prod.js');

// ---------------------------------------------------------------------------
// 3. Design tokens — maritime blues / teals palette
// ---------------------------------------------------------------------------
const PALETTE = {
  navy:       '#0a2342',
  ocean:      '#1a5276',
  teal:       '#148f8f',
  seafoam:    '#1abc9c',
  sky:        '#5dade2',
  lagoon:     '#a9cce3',
  coral:      '#e74c3c',
  sand:       '#f0b27a',
  foam:       '#eaf4fb',
  muted:      '#6c757d',
};

const CHART_COLORS = [
  PALETTE.ocean, PALETTE.teal, PALETTE.sky, PALETTE.seafoam,
  PALETTE.lagoon, PALETTE.coral, PALETTE.sand, PALETTE.navy,
];

const CHART_DEFAULTS = {
  font: { family: "'Segoe UI', system-ui, sans-serif", size: 12 },
  color: PALETTE.muted,
};

// Apply Chart.js global defaults
Chart.defaults.font.family = CHART_DEFAULTS.font.family;
Chart.defaults.font.size   = CHART_DEFAULTS.font.size;
Chart.defaults.color       = CHART_DEFAULTS.color;

// ---------------------------------------------------------------------------
// 4. API helpers
// ---------------------------------------------------------------------------
async function apiFetch(path) {
  const res = await fetch(path);
  if (!res.ok) throw new Error(`HTTP ${res.status}: ${path}`);
  const json = await res.json();
  if (json.errors && json.errors.length) throw new Error(json.errors.join('; '));
  return json.data;
}

function buildUrl(base, vesselType) {
  return vesselType ? `${base}?vesselType=${encodeURIComponent(vesselType)}` : base;
}

// ---------------------------------------------------------------------------
// 5. Formatting helpers
// ---------------------------------------------------------------------------
function formatCurrency(value) {
  if (value == null) return '—';
  const n = Number(value);
  if (n >= 1e9) return `$${(n / 1e9).toFixed(2)}B`;
  if (n >= 1e6) return `$${(n / 1e6).toFixed(1)}M`;
  if (n >= 1e3) return `$${(n / 1e3).toFixed(0)}K`;
  return `$${n.toLocaleString()}`;
}

function formatNumber(value) {
  if (value == null) return '—';
  return Number(value).toLocaleString();
}

// ---------------------------------------------------------------------------
// 6. SummaryCards component
// ---------------------------------------------------------------------------
const SummaryCards = {
  props: { vesselType: { type: String, default: '' } },
  setup(props) {
    const loading = ref(true);
    const error   = ref(null);
    const data    = ref(null);

    const cards = computed(() => {
      if (!data.value) return [];
      return [
        {
          label: 'Active Policies',
          value: formatNumber(data.value.totalActivePolicies),
          icon:  'bi-file-earmark-check',
          color: PALETTE.ocean,
          bg:    '#eaf2fb',
        },
        {
          label: 'Open Claims',
          value: formatNumber(data.value.openClaimsCount),
          icon:  'bi-exclamation-triangle',
          color: PALETTE.coral,
          bg:    '#fdf0ee',
        },
        {
          label: 'Total Insured Value',
          value: formatCurrency(data.value.totalInsuredValue),
          icon:  'bi-shield-check',
          color: PALETTE.teal,
          bg:    '#eaf7f7',
        },
        {
          label: 'Outstanding Claims',
          value: formatCurrency(data.value.totalOutstandingClaims),
          icon:  'bi-cash-stack',
          color: PALETTE.sand,
          bg:    '#fef9f0',
        },
      ];
    });

    async function load() {
      loading.value = true;
      error.value   = null;
      try {
        data.value = await apiFetch('/api/dashboard/summary');
      } catch (e) {
        error.value = e.message;
      } finally {
        loading.value = false;
      }
    }

    onMounted(load);
    // Summary doesn't filter by vesselType per spec, but re-fetch if it changes
    watch(() => props.vesselType, load);

    return { loading, error, cards };
  },
  template: `
    <div class="row g-3 mb-4">
      <template v-if="loading">
        <div class="col-12 col-sm-6 col-xl-3" v-for="i in 4" :key="i">
          <div class="card border-0 shadow-sm h-100 placeholder-glow">
            <div class="card-body d-flex align-items-center gap-3">
              <span class="placeholder rounded-circle" style="width:52px;height:52px;"></span>
              <div class="flex-grow-1">
                <span class="placeholder col-8 d-block mb-2"></span>
                <span class="placeholder col-5 d-block" style="height:1.5rem;"></span>
              </div>
            </div>
          </div>
        </div>
      </template>
      <template v-else-if="error">
        <div class="col-12">
          <div class="alert alert-warning mb-0">
            <i class="bi bi-exclamation-circle me-2"></i>{{ error }}
          </div>
        </div>
      </template>
      <template v-else>
        <div class="col-12 col-sm-6 col-xl-3" v-for="card in cards" :key="card.label">
          <div class="card border-0 shadow-sm h-100" style="border-left:4px solid v-bind(card.color)!important;">
            <div class="card-body d-flex align-items-center gap-3">
              <div class="rounded-circle d-flex align-items-center justify-content-center flex-shrink-0"
                   :style="{ width:'52px', height:'52px', background: card.bg }">
                <i class="bi fs-4" :class="card.icon" :style="{ color: card.color }"></i>
              </div>
              <div>
                <div class="text-muted small mb-1">{{ card.label }}</div>
                <div class="fs-4 fw-bold" :style="{ color: card.color }">{{ card.value }}</div>
              </div>
            </div>
          </div>
        </div>
      </template>
    </div>
  `,
};

// ---------------------------------------------------------------------------
// 7. Reusable chart card wrapper
// ---------------------------------------------------------------------------
const ChartCard = {
  props: {
    title:   { type: String, required: true },
    icon:    { type: String, default: 'bi-bar-chart' },
    loading: { type: Boolean, default: false },
    error:   { type: String, default: null },
  },
  template: `
    <div class="card border-0 shadow-sm h-100">
      <div class="card-header bg-white border-bottom d-flex align-items-center gap-2 py-3">
        <i class="bi fs-5 text-primary" :class="icon"></i>
        <span class="fw-semibold">{{ title }}</span>
      </div>
      <div class="card-body position-relative" style="min-height:260px;">
        <div v-if="loading" class="position-absolute top-50 start-50 translate-middle text-center">
          <div class="spinner-border text-primary" style="width:2rem;height:2rem;" role="status"></div>
          <div class="text-muted small mt-2">Loading…</div>
        </div>
        <div v-else-if="error" class="alert alert-warning mb-0">
          <i class="bi bi-exclamation-circle me-2"></i>{{ error }}
        </div>
        <slot v-else></slot>
      </div>
    </div>
  `,
};

// ---------------------------------------------------------------------------
// 8. ClaimsByCategory — Bar chart
// ---------------------------------------------------------------------------
const ClaimsByCategory = {
  props: { vesselType: { type: String, default: '' } },
  components: { ChartCard },
  setup(props) {
    const loading  = ref(true);
    const error    = ref(null);
    const canvasId = 'chart-claims-by-category';
    let chartInstance = null;

    async function load() {
      loading.value = true;
      error.value   = null;
      try {
        const rows = await apiFetch(buildUrl('/api/dashboard/claims-by-category', props.vesselType));

        await nextTick();

        const labels = rows.map(r => r.category);
        const values = rows.map(r => r.count);

        if (chartInstance) {
          chartInstance.data.labels        = labels;
          chartInstance.data.datasets[0].data = values;
          chartInstance.update();
        } else {
          const ctx = document.getElementById(canvasId);
          if (!ctx) return;
          chartInstance = new Chart(ctx, {
            type: 'bar',
            data: {
              labels,
              datasets: [{
                label:           'Claims',
                data:            values,
                backgroundColor: CHART_COLORS.map(c => c + 'cc'),
                borderColor:     CHART_COLORS,
                borderWidth:     1.5,
                borderRadius:    4,
              }],
            },
            options: {
              responsive:         true,
              maintainAspectRatio: false,
              plugins: {
                legend: { display: false },
                tooltip: {
                  callbacks: {
                    label: ctx => ` ${ctx.parsed.y} claim${ctx.parsed.y !== 1 ? 's' : ''}`,
                  },
                },
              },
              scales: {
                x: {
                  grid: { display: false },
                  ticks: { maxRotation: 30 },
                },
                y: {
                  beginAtZero: true,
                  ticks: { precision: 0 },
                  grid: { color: '#f0f0f0' },
                },
              },
            },
          });
        }
      } catch (e) {
        error.value = e.message;
      } finally {
        loading.value = false;
      }
    }

    onMounted(load);
    watch(() => props.vesselType, load);
    onBeforeUnmount(() => { if (chartInstance) chartInstance.destroy(); });

    return { loading, error, canvasId };
  },
  template: `
    <ChartCard title="Claims by Category" icon="bi-bar-chart-fill" :loading="loading" :error="error">
      <div style="height:240px;">
        <canvas :id="canvasId"></canvas>
      </div>
    </ChartCard>
  `,
};

// ---------------------------------------------------------------------------
// 9. PoliciesByCoverage — Doughnut chart
// ---------------------------------------------------------------------------
const PoliciesByCoverage = {
  props: { vesselType: { type: String, default: '' } },
  components: { ChartCard },
  setup(props) {
    const loading  = ref(true);
    const error    = ref(null);
    const canvasId = 'chart-policies-by-coverage';
    let chartInstance = null;

    async function load() {
      loading.value = true;
      error.value   = null;
      try {
        const rows = await apiFetch(buildUrl('/api/dashboard/policies-by-coverage', props.vesselType));

        await nextTick();

        const labels = rows.map(r => r.coverageType);
        const values = rows.map(r => r.count);

        if (chartInstance) {
          chartInstance.data.labels        = labels;
          chartInstance.data.datasets[0].data = values;
          chartInstance.update();
        } else {
          const ctx = document.getElementById(canvasId);
          if (!ctx) return;
          chartInstance = new Chart(ctx, {
            type: 'doughnut',
            data: {
              labels,
              datasets: [{
                data:            values,
                backgroundColor: CHART_COLORS.map(c => c + 'dd'),
                borderColor:     '#ffffff',
                borderWidth:     2,
                hoverOffset:     6,
              }],
            },
            options: {
              responsive:         true,
              maintainAspectRatio: false,
              cutout:             '60%',
              plugins: {
                legend: {
                  position:  'right',
                  labels: { boxWidth: 14, padding: 12, font: { size: 11 } },
                },
                tooltip: {
                  callbacks: {
                    label: ctx => {
                      const total = ctx.dataset.data.reduce((a, b) => a + b, 0);
                      const pct   = total ? ((ctx.parsed / total) * 100).toFixed(1) : 0;
                      return ` ${ctx.label}: ${ctx.parsed} (${pct}%)`;
                    },
                  },
                },
              },
            },
          });
        }
      } catch (e) {
        error.value = e.message;
      } finally {
        loading.value = false;
      }
    }

    onMounted(load);
    watch(() => props.vesselType, load);
    onBeforeUnmount(() => { if (chartInstance) chartInstance.destroy(); });

    return { loading, error, canvasId };
  },
  template: `
    <ChartCard title="Policies by Coverage Type" icon="bi-pie-chart-fill" :loading="loading" :error="error">
      <div style="height:240px;">
        <canvas :id="canvasId"></canvas>
      </div>
    </ChartCard>
  `,
};

// ---------------------------------------------------------------------------
// 10. ClaimsTrend — Line chart (trailing 12 months)
// ---------------------------------------------------------------------------
const ClaimsTrend = {
  props: { vesselType: { type: String, default: '' } },
  components: { ChartCard },
  setup(props) {
    const loading  = ref(true);
    const error    = ref(null);
    const canvasId = 'chart-claims-trend';
    let chartInstance = null;

    async function load() {
      loading.value = true;
      error.value   = null;
      try {
        const rows = await apiFetch(buildUrl('/api/dashboard/claims-trend', props.vesselType));

        await nextTick();

        const labels = rows.map(r => r.month);
        const values = rows.map(r => r.count);

        if (chartInstance) {
          chartInstance.data.labels        = labels;
          chartInstance.data.datasets[0].data = values;
          chartInstance.update();
        } else {
          const ctx = document.getElementById(canvasId);
          if (!ctx) return;

          // Gradient fill
          const gradient = ctx.getContext('2d').createLinearGradient(0, 0, 0, 240);
          gradient.addColorStop(0, PALETTE.teal + '55');
          gradient.addColorStop(1, PALETTE.teal + '00');

          chartInstance = new Chart(ctx, {
            type: 'line',
            data: {
              labels,
              datasets: [{
                label:           'Claims',
                data:            values,
                borderColor:     PALETTE.teal,
                backgroundColor: gradient,
                pointBackgroundColor: PALETTE.teal,
                pointBorderColor:    '#ffffff',
                pointBorderWidth:    2,
                pointRadius:         4,
                pointHoverRadius:    6,
                borderWidth:         2.5,
                fill:                true,
                tension:             0.35,
              }],
            },
            options: {
              responsive:         true,
              maintainAspectRatio: false,
              plugins: {
                legend: { display: false },
                tooltip: {
                  callbacks: {
                    label: ctx => ` ${ctx.parsed.y} claim${ctx.parsed.y !== 1 ? 's' : ''}`,
                  },
                },
              },
              scales: {
                x: {
                  grid:  { display: false },
                  ticks: { maxRotation: 0, maxTicksLimit: 12, font: { size: 11 } },
                },
                y: {
                  beginAtZero: true,
                  ticks:       { precision: 0 },
                  grid:        { color: '#f0f0f0' },
                },
              },
            },
          });
        }
      } catch (e) {
        error.value = e.message;
      } finally {
        loading.value = false;
      }
    }

    onMounted(load);
    watch(() => props.vesselType, load);
    onBeforeUnmount(() => { if (chartInstance) chartInstance.destroy(); });

    return { loading, error, canvasId };
  },
  template: `
    <ChartCard title="Claims Trend (12 months)" icon="bi-graph-up" :loading="loading" :error="error">
      <div style="height:240px;">
        <canvas :id="canvasId"></canvas>
      </div>
    </ChartCard>
  `,
};

// ---------------------------------------------------------------------------
// 11. VesselTypeFilter component
// ---------------------------------------------------------------------------
const VESSEL_TYPES = [
  { value: '',               label: 'All Vessel Types' },
  { value: 'Tanker',         label: 'Tanker' },
  { value: 'Bulk Carrier',   label: 'Bulk Carrier' },
  { value: 'Container Ship', label: 'Container Ship' },
  { value: 'General Cargo',  label: 'General Cargo' },
  { value: 'Offshore',       label: 'Offshore' },
  { value: 'Passenger',      label: 'Passenger' },
  { value: 'RORO',           label: 'RoRo / Ferry' },
];

const VesselTypeFilter = {
  props:  { modelValue: { type: String, default: '' } },
  emits:  ['update:modelValue'],
  setup(props, { emit }) {
    const selected = computed({
      get: () => props.modelValue,
      set: v => emit('update:modelValue', v),
    });
    return { selected, VESSEL_TYPES };
  },
  template: `
    <div class="d-flex align-items-center gap-2 mb-4">
      <i class="bi bi-funnel text-primary fs-5"></i>
      <label class="fw-semibold text-nowrap me-1 mb-0" style="color:${PALETTE.navy}">Filter by vessel type:</label>
      <select class="form-select form-select-sm w-auto" v-model="selected">
        <option v-for="opt in VESSEL_TYPES" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
      </select>
      <span v-if="selected" class="badge text-bg-primary ms-1">
        {{ selected }}
        <button type="button" class="btn-close btn-close-white btn-sm ms-1"
                style="font-size:0.6em;" @click="selected = ''"></button>
      </span>
    </div>
  `,
};

// ---------------------------------------------------------------------------
// 12. Root App
// ---------------------------------------------------------------------------
const App = {
  components: {
    SummaryCards,
    VesselTypeFilter,
    ClaimsByCategory,
    PoliciesByCoverage,
    ClaimsTrend,
  },
  setup() {
    const vesselType = ref('');
    const lastUpdated = ref(null);

    function refresh() {
      // bump vesselType to trigger watchers; use a trick: temporarily store value
      // Actually just re-set to same value won't trigger watch; use a timestamp signal instead
      lastUpdated.value = Date.now();
    }

    return { vesselType, lastUpdated, refresh };
  },
  template: `
    <div>
      <!-- Summary KPI cards -->
      <SummaryCards :vesselType="vesselType" />

      <!-- Filter row -->
      <div class="d-flex align-items-center justify-content-between flex-wrap gap-2 mb-3">
        <VesselTypeFilter v-model="vesselType" />
        <div class="text-muted small">
          <i class="bi bi-clock me-1"></i>
          <span v-if="lastUpdated">Last refreshed {{ new Date(lastUpdated).toLocaleTimeString() }}</span>
          <span v-else>Auto-refreshes on filter change</span>
        </div>
      </div>

      <!-- Charts row 1: Claims by Category + Policies by Coverage -->
      <div class="row g-3 mb-3">
        <div class="col-12 col-lg-6">
          <ClaimsByCategory :vesselType="vesselType" />
        </div>
        <div class="col-12 col-lg-6">
          <PoliciesByCoverage :vesselType="vesselType" />
        </div>
      </div>

      <!-- Charts row 2: Claims Trend full-width -->
      <div class="row g-3 mb-3">
        <div class="col-12">
          <ClaimsTrend :vesselType="vesselType" />
        </div>
      </div>

      <!-- Footer note -->
      <p class="text-muted small text-end mt-2 mb-0">
        <i class="bi bi-info-circle me-1"></i>
        Data reflects current portfolio state. Charts update automatically when vessel type filter changes.
      </p>
    </div>
  `,
};

// ---------------------------------------------------------------------------
// 13. Mount
// ---------------------------------------------------------------------------
const app = createApp(App);
app.mount('#app');
