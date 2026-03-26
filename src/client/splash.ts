import { navigateTo, context, requestExpandedMode } from '@devvit/web/client';
import type { InitResponse } from '../shared/api';

// DOM Elements
const titleElement = document.getElementById('title') as HTMLHeadingElement;
const descriptionElement = document.getElementById(
  'description'
) as HTMLParagraphElement;
const statsContainer = document.getElementById(
  'stats-container'
) as HTMLDivElement;
const statTotalLevel = document.getElementById(
  'stat-total-level'
) as HTMLSpanElement;
const statArenaKills = document.getElementById(
  'stat-arena-kills'
) as HTMLSpanElement;
const statItems = document.getElementById('stat-items') as HTMLSpanElement;
const startButton = document.getElementById(
  'start-button'
) as HTMLButtonElement;
const docsLink = document.getElementById('docs-link') as HTMLDivElement;
const playtestLink = document.getElementById('playtest-link') as HTMLDivElement;
const discordLink = document.getElementById('discord-link') as HTMLDivElement;

// Navigation
startButton.addEventListener('click', (e) => {
  requestExpandedMode(e, 'game');
});

docsLink.addEventListener('click', () => {
  navigateTo('https://developers.reddit.com/docs');
});

playtestLink.addEventListener('click', () => {
  navigateTo('https://www.reddit.com/r/Devvit');
});

discordLink.addEventListener('click', () => {
  navigateTo('https://discord.com/invite/R7yu2wh9Qz');
});

// Init
const username = context.username ?? 'adventurer';

const init = async (): Promise<void> => {
  titleElement.textContent = `Welcome, ${username}!`;

  try {
    const response = await fetch('/api/init');
    if (!response.ok) return;

    const data = (await response.json()) as InitResponse;

    if (data.isNewPlayer) {
      descriptionElement.textContent =
        'Begin your adventure! Tap below to enter.';
    } else {
      descriptionElement.textContent =
        'Your realm awaits. Pick up where you left off!';

      // Show stats for returning players
      statTotalLevel.textContent = String(data.profile.totalLevel);
      statArenaKills.textContent = String(data.profile.arenaKills);
      statItems.textContent = `${data.inventory.items.length}/${data.inventory.maxSlots}`;
      statsContainer.style.display = 'flex';

      // Show offline gains if any
      const totalOfflineXp = Object.values(data.offlineXpGained).reduce(
        (a, b) => a + b,
        0
      );
      if (totalOfflineXp > 0) {
        descriptionElement.textContent = `You gained ${totalOfflineXp} XP while away! Tap to collect.`;
      }
    }
  } catch {
    // Silently fail — splash page works fine without init data
  }
};

init();
