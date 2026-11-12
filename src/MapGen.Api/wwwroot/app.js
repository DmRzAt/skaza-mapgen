const MAX_SEED = 18446744073709551615n;
const MIN_SIZE = 32;
const MAX_SIZE = 256;
const DOWNLOAD_SCALE = 8;
const REVEAL_MS = 700;

const TILE_COLORS = {
  clean: { m: [122, 138, 84], f: [52, 82, 48], s: [78, 92, 96], r: [70, 66, 64] },
  corrupted: { m: [110, 96, 118], f: [62, 52, 78], s: [70, 58, 86], r: [70, 66, 64] },
};
const HEART_COLOR = [220, 60, 200];
const SHRINE_COLOR = [240, 210, 90];

const TILE_NAMES = { m: "Łąka", f: "Las", s: "Bagno", r: "Skała" };
const ZONE_NAMES = { c: "czyste ziemie", b: "pogranicze", d: "głęboka skaza" };

const form = document.querySelector("#map-form");
const seedInput = document.querySelector("#seed");
const widthInput = document.querySelector("#width");
const heightInput = document.querySelector("#height");
const submitButton = form.querySelector("button[type=submit]");
const randomButton = document.querySelector("#random-seed");
const canvas = document.querySelector("#map-canvas");
const status = document.querySelector("#map-status");
const readout = document.querySelector("#tile-readout");
const pngLink = document.querySelector("#download-png");
const jsonLink = document.querySelector("#open-json");
const context = canvas.getContext("2d");
const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");

let currentMap = null;
let revealFrame = 0;

function validate(values) {
  const errors = {};
  if (!/^\d{1,20}$/.test(values.seed) || BigInt(values.seed) > MAX_SEED) {
    errors.seed = "Ziarno to liczba całkowita od 0 do 18446744073709551615.";
  }
  for (const [name, label] of [["width", "Szerokość"], ["height", "Wysokość"]]) {
    const value = Number(values[name]);
    if (!Number.isInteger(value) || value < MIN_SIZE || value > MAX_SIZE) {
      errors[name] = `${label} musi być liczbą od ${MIN_SIZE} do ${MAX_SIZE}.`;
    }
  }
  return errors;
}

function showErrors(errors) {
  for (const input of [seedInput, widthInput, heightInput]) {
    const message = errors[input.name] ?? "";
    input.setAttribute("aria-invalid", message ? "true" : "false");
    document.querySelector(`#${input.name}-error`).textContent = message;
  }
}

function readForm() {
  return {
    seed: seedInput.value.trim(),
    width: widthInput.value.trim(),
    height: heightInput.value.trim(),
  };
}

function query(values) {
  return `width=${values.width}&height=${values.height}`;
}

async function generate(values) {
  const errors = validate(values);
  showErrors(errors);
  if (Object.keys(errors).length > 0) {
    return;
  }

  submitButton.disabled = true;
  status.textContent = "Generuję dolinę…";
  try {
    const [mapResponse, statsResponse] = await Promise.all([
      fetch(`/api/maps/${values.seed}?${query(values)}`),
      fetch(`/api/maps/${values.seed}/stats?${query(values)}`),
    ]);
    if (mapResponse.status === 400) {
      showErrors(translateServerErrors(await mapResponse.json()));
      status.textContent = "Popraw zaznaczone pola i spróbuj ponownie.";
      return;
    }
    if (!mapResponse.ok || !statsResponse.ok) {
      throw new Error(`HTTP ${mapResponse.status}`);
    }

    currentMap = await mapResponse.json();
    status.textContent = "";
    drawMap(currentMap);
    showStats(await statsResponse.json());
    updateLinks(values);
    history.replaceState(null, "", `?seed=${values.seed}&${query(values)}`);
  } catch (error) {
    status.textContent = "Serwer nie odpowiada. Sprawdź, czy aplikacja działa, i spróbuj ponownie.";
    console.error(error);
  } finally {
    submitButton.disabled = false;
  }
}

function translateServerErrors(problem) {
  const errors = {};
  for (const name of Object.keys(problem.errors ?? {})) {
    if (name === "width" || name === "height") {
      const label = name === "width" ? "Szerokość" : "Wysokość";
      errors[name] = `${label} musi być liczbą od ${MIN_SIZE} do ${MAX_SIZE}.`;
    } else {
      errors.seed = "Serwer odrzucił ziarno. Użyj liczby całkowitej.";
    }
  }
  return errors;
}

function colorAt(map, x, y) {
  if (x === map.heart.x && y === map.heart.y) {
    return HEART_COLOR;
  }
  if (map.shrines.some((shrine) => shrine.x === x && shrine.y === y)) {
    return SHRINE_COLOR;
  }
  const palette = map.zones[y][x] === "d" ? TILE_COLORS.corrupted : TILE_COLORS.clean;
  return palette[map.tiles[y][x]];
}

function buildPixels(map) {
  const image = context.createImageData(map.width, map.height);
  for (let y = 0; y < map.height; y++) {
    for (let x = 0; x < map.width; x++) {
      const [r, g, b] = colorAt(map, x, y);
      const offset = (y * map.width + x) * 4;
      image.data[offset] = r;
      image.data[offset + 1] = g;
      image.data[offset + 2] = b;
      image.data[offset + 3] = 255;
    }
  }
  return image;
}

function drawMap(map) {
  cancelAnimationFrame(revealFrame);
  canvas.width = map.width;
  canvas.height = map.height;
  canvas.setAttribute("aria-label", `Mapa ${map.width} na ${map.height} pól dla ziarna ${seedInput.value.trim()}`);
  const image = buildPixels(map);

  if (reducedMotion.matches) {
    context.putImageData(image, 0, 0);
    return;
  }
  revealFromHeart(map, image);
}

function revealFromHeart(map, image) {
  const maxRadius = Math.hypot(Math.max(map.heart.x, map.width - map.heart.x), Math.max(map.heart.y, map.height - map.heart.y));
  const offscreen = new OffscreenCanvas(map.width, map.height);
  offscreen.getContext("2d").putImageData(image, 0, 0);
  let start = null;

  const step = (now) => {
    start ??= now;
    const progress = Math.min(1, Math.max(0, (now - start) / REVEAL_MS));
    const radius = maxRadius * (1 - Math.pow(1 - progress, 3));
    context.clearRect(0, 0, map.width, map.height);
    context.save();
    context.beginPath();
    context.arc(map.heart.x + 0.5, map.heart.y + 0.5, radius, 0, Math.PI * 2);
    context.clip();
    context.drawImage(offscreen, 0, 0);
    context.restore();
    if (progress < 1) {
      revealFrame = requestAnimationFrame(step);
    }
  };
  revealFrame = requestAnimationFrame(step);
}

function showStats(stats) {
  document.querySelector("#stat-walkable").textContent = `${Math.round(stats.walkableShare * 100)}%`;
  document.querySelector("#stat-meadow").textContent = stats.tileCounts.Meadow;
  document.querySelector("#stat-forest").textContent = stats.tileCounts.Forest;
  document.querySelector("#stat-swamp").textContent = stats.tileCounts.Swamp;
  document.querySelector("#stat-rock").textContent = stats.tileCounts.Rock;
  document.querySelector("#stat-shrines").textContent = stats.shrineCount;
}

function updateLinks(values) {
  pngLink.href = `/api/maps/${values.seed}/image?${query(values)}&scale=${DOWNLOAD_SCALE}`;
  pngLink.download = `skaza-${values.seed}.png`;
  jsonLink.href = `/api/maps/${values.seed}?${query(values)}`;
}

function describeTile(event) {
  if (!currentMap) {
    return;
  }
  const rect = canvas.getBoundingClientRect();
  const x = Math.floor(((event.clientX - rect.left) / rect.width) * currentMap.width);
  const y = Math.floor(((event.clientY - rect.top) / rect.height) * currentMap.height);
  if (x < 0 || y < 0 || x >= currentMap.width || y >= currentMap.height) {
    return;
  }

  let name = TILE_NAMES[currentMap.tiles[y][x]];
  if (x === currentMap.heart.x && y === currentMap.heart.y) {
    name = "Serce Skazy";
  } else if (currentMap.shrines.some((shrine) => shrine.x === x && shrine.y === y)) {
    name = "Kapliczka";
  }
  readout.textContent = `(${x}, ${y}) ${name}, ${ZONE_NAMES[currentMap.zones[y][x]]}`;
}

function randomSeed() {
  const words = crypto.getRandomValues(new Uint32Array(2));
  return ((BigInt(words[0]) << 32n) | BigInt(words[1])).toString();
}

form.addEventListener("submit", (event) => {
  event.preventDefault();
  generate(readForm());
});

randomButton.addEventListener("click", () => {
  seedInput.value = randomSeed();
  generate(readForm());
});

canvas.addEventListener("mousemove", describeTile);
canvas.addEventListener("mouseleave", () => {
  readout.textContent = "Najedź na mapę, aby zobaczyć pole.";
});

const params = new URLSearchParams(location.search);
seedInput.value = params.get("seed") ?? seedInput.value;
widthInput.value = params.get("width") ?? widthInput.value;
heightInput.value = params.get("height") ?? heightInput.value;
generate(readForm());
