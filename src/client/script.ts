import type { InitResponse } from '../shared/api';

// Type definitions for Unity WebGL
type UnityBannerType = 'error' | 'warning' | 'info';

type UnityConfig = {
  arguments: string[];
  dataUrl: string;
  frameworkUrl: string;
  codeUrl: string;
  streamingAssetsUrl: string;
  companyName: string;
  productName: string;
  productVersion: string;
  showBanner: (msg: string, type: UnityBannerType) => void;
  matchWebGLToCanvasSize?: boolean;
  autoSyncPersistentDataPath?: boolean;
  devicePixelRatio?: number;
};

type UnityInstance = {
  SetFullscreen: (fullscreen: number) => void;
  SendMessage: (
    objectName: string,
    methodName: string,
    value?: string | number
  ) => void;
  Quit: () => Promise<void>;
};

// Declare Unity loader function that will be loaded from external script
declare function createUnityInstance(
  canvas: HTMLCanvasElement,
  config: UnityConfig,
  onProgress?: (progress: number) => void
): Promise<UnityInstance>;

// ---------------------------------------------------------------------------
// Loading Tips
// ---------------------------------------------------------------------------

const LOADING_TIPS = [
  'Sharpening axes...',
  'Baiting hooks...',
  'Surveying the mines...',
  'Stoking the furnace...',
  'Seasoning the catch...',
  'Polishing rune armor...',
  'Scouting the arena...',
  'Rolling for rare drops...',
];

const loadingTipElement = document.getElementById('loading-tip');
let tipInterval: ReturnType<typeof setInterval> | null = null;

const showRandomTip = (): void => {
  if (loadingTipElement) {
    const index = Math.floor(Math.random() * LOADING_TIPS.length);
    loadingTipElement.textContent = LOADING_TIPS[index] ?? '';
  }
};

const startLoadingTips = (): void => {
  showRandomTip();
  tipInterval = setInterval(showRandomTip, 3000);
};

const stopLoadingTips = (): void => {
  if (tipInterval !== null) {
    clearInterval(tipInterval);
    tipInterval = null;
  }
  if (loadingTipElement) {
    loadingTipElement.textContent = '';
  }
};

// ---------------------------------------------------------------------------
// Unity Setup
// ---------------------------------------------------------------------------

const canvas = document.querySelector<HTMLCanvasElement>('#unity-canvas');

if (!canvas) {
  throw new Error('Unity canvas element not found');
}

function unityShowBanner(msg: string, type: UnityBannerType): void {
  const warningBanner = document.querySelector<HTMLElement>('#unity-warning');

  if (!warningBanner) {
    console.error('Warning banner element not found');
    return;
  }

  const banner = warningBanner;

  function updateBannerVisibility(): void {
    banner.style.display = banner.children.length ? 'block' : 'none';
  }

  const div = document.createElement('div');
  div.innerHTML = msg;
  warningBanner.appendChild(div);

  if (type === 'error') {
    div.style.cssText = 'background: #8b0000; padding: 10px; color: #ffcc00;';
  } else {
    if (type === 'warning') {
      div.style.cssText = 'background: #5c4a2a; padding: 10px; color: #ffcc00;';
    }
    setTimeout(() => {
      warningBanner.removeChild(div);
      updateBannerVisibility();
    }, 5000);
  }
  updateBannerVisibility();
}

const buildUrl = 'Build';
const loaderUrl = buildUrl + '/RuneRealms.loader.js';
const config: UnityConfig = {
  arguments: [],
  dataUrl: buildUrl + '/RuneRealms.data.unityweb',
  frameworkUrl: buildUrl + '/RuneRealms.framework.js',
  codeUrl: buildUrl + '/RuneRealms.wasm.unityweb',
  streamingAssetsUrl: 'StreamingAssets',
  companyName: 'Rune Realms',
  productName: 'Rune Realms',
  productVersion: '0.1.0',
  showBanner: unityShowBanner,
};

// ---------------------------------------------------------------------------
// Device Detection & Canvas Sizing
// ---------------------------------------------------------------------------

if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
  const meta = document.createElement('meta');
  meta.name = 'viewport';
  meta.content =
    'width=device-width, height=device-height, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes';
  document.getElementsByTagName('head')[0]?.appendChild(meta);

  const container = document.querySelector<HTMLElement>('#unity-container');
  if (container) {
    container.className = 'unity-mobile';
  }
  canvas.className = 'unity-mobile';
} else {
  canvas.style.width = '100%';
  canvas.style.height = '100%';

  const container = document.querySelector<HTMLElement>('#unity-container');
  if (container) {
    container.style.width = '100%';
    container.style.height = '100%';
    container.style.position = 'fixed';
    container.style.left = '0';
    container.style.top = '0';
    container.style.transform = 'none';
  }
}

// ---------------------------------------------------------------------------
// Init Data Fetch (runs in parallel with Unity load)
// ---------------------------------------------------------------------------

const initDataPromise: Promise<InitResponse | null> = fetch('/api/init')
  .then((res) => {
    if (!res.ok) throw new Error('Init fetch failed');
    return res.json() as Promise<InitResponse>;
  })
  .catch((err) => {
    console.error('Failed to fetch init data:', err);
    return null;
  });

// ---------------------------------------------------------------------------
// Load Unity
// ---------------------------------------------------------------------------

const loadingBar = document.querySelector<HTMLElement>('#unity-loading-bar');
if (loadingBar) {
  loadingBar.style.display = 'block';
}

startLoadingTips();

const script = document.createElement('script');
script.src = loaderUrl;
script.onload = () => {
  createUnityInstance(canvas, config, (progress: number) => {
    const progressBarFull = document.querySelector<HTMLElement>(
      '#unity-progress-bar-full'
    );
    if (progressBarFull) {
      progressBarFull.style.width = 100 * progress + '%';
    }
  })
    .then(async (unityInstance: UnityInstance) => {
      // Hide loading UI
      const loadingBar =
        document.querySelector<HTMLElement>('#unity-loading-bar');
      if (loadingBar) {
        loadingBar.style.display = 'none';
      }
      stopLoadingTips();

      // Send Devvit context to Unity
      const initData = await initDataPromise;
      if (initData) {
        unityInstance.SendMessage(
          'DevvitBridge',
          'OnInitData',
          JSON.stringify(initData)
        );
      }
    })
    .catch((message: unknown) => {
      console.error('Unity load error:', message);
      stopLoadingTips();
    });
};

document.body.appendChild(script);
