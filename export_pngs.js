const puppeteer = require('puppeteer');
const path = require('path');
const fs = require('fs');

const HTML_FILE = path.resolve(__dirname, 'export_assets.html');
const OUT_DIR   = path.resolve(__dirname, 'Assets', 'Game', 'UI', 'Art');

async function main() {
  fs.mkdirSync(OUT_DIR, { recursive: true });

  console.log('Abriendo Chrome headless...');
  const browser = await puppeteer.launch({ headless: true });
  const page    = await browser.newPage();
  await page.setViewport({ width: 2000, height: 3000 });

  console.log('Cargando HTML y esperando fuentes...');
  await page.goto(`file://${HTML_FILE}`, { waitUntil: 'networkidle0' });
  // Espera extra para que Caveat y los filtros SVG terminen de renderizar
  await new Promise(r => setTimeout(r, 3000));

  // Ocultar labels/instrucciones y fondo para export limpio
  await page.addStyleTag({ content: `
    body { background: transparent !important; }
    .label, .instructions, .section > .instructions { display: none !important; }
  `});

  const targets = [
    { id: '#background-node', out: 'background.png', solid: true },
    { id: '#title-node',      out: 'title_glow.png' },
    { id: '#slug-node',       out: 'slug.png'        },
    { id: '#angel-node',      out: 'angel.png'       },
  ];

  for (const { id, out, solid } of targets) {
    const el = await page.$(id);
    if (!el) { console.warn(`  ⚠  No se encontró ${id}`); continue; }
    const outPath = path.join(OUT_DIR, out);
    await el.screenshot({ path: outPath, omitBackground: !solid });
    console.log(`  ✓  ${out}`);
  }

  await browser.close();
  console.log(`\nListo. PNGs en: ${OUT_DIR}`);
  console.log('En Unity: clic derecho en la carpeta Art → Refresh, o presioná Ctrl+R.');
}

main().catch(err => { console.error(err); process.exit(1); });
