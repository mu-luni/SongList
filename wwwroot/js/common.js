const INDEX_SORT_STATE_KEY = 'songListIndexSortState';
const DETAILS_SORT_STATE_KEY = 'songListDetailsSortState';
let toastTimer = null;

function showCopyToast(message) {
  const toast = document.getElementById('copy-toast');
  if (!toast) return;

  toast.textContent = message;
  toast.classList.add('show');

  if (toastTimer) {
    clearTimeout(toastTimer);
  }

  toastTimer = setTimeout(() => {
    toast.classList.remove('show');
  }, 1800);
}

function getSortStateKey() {
  return document.querySelector('.sort-col[data-sort-key="ArtistName"]')
    ? INDEX_SORT_STATE_KEY
    : DETAILS_SORT_STATE_KEY;
}
let currentSort = { key: null, asc: true };

function loadSortState() {
  try {
    const saved = sessionStorage.getItem(getSortStateKey());
    if (!saved) return { key: null, asc: true };

    const parsed = JSON.parse(saved);
    if (parsed && typeof parsed.key === 'string' && ['SongName', 'ArtistName', 'LastSungDate', 'SungCount', 'ReleaseDate', 'LimitedName'].includes(parsed.key)) {
      return {
        key: parsed.key,
        asc: typeof parsed.asc === 'boolean' ? parsed.asc : true
      };
    }
  } catch (error) {
    console.warn('Sort state could not be loaded:', error);
  }
  return { key: null, asc: true };
}

function saveSortState() {
  try {
    sessionStorage.setItem(getSortStateKey(), JSON.stringify(currentSort));
  } catch (error) {
    console.warn('Sort state could not be saved:', error);
  }
}

function getRowSortValue(row, key) {
  if (key === 'SongName') {
    return row.dataset.songName || row.querySelector('.song-title')?.textContent.trim() || '';
  }
  if (key === 'ArtistName') {
    return row.dataset.artistName || row.querySelector('.artist-name')?.textContent.trim() || '';
  }
  if (key === 'LastSungDate') {
    return row.dataset.lastSungDate || row.querySelector('.last-sung-date')?.textContent.trim() || '';
  }
  if (key === 'SungCount') {
    const countText = row.dataset.sungCount || row.querySelector('.count-badge')?.textContent.replace(/[^0-9]/g, '') || '0';
    return Number(countText);
  }
  if (key === 'ReleaseDate') {
    return row.dataset.releasedate || row.querySelector('.release-date')?.textContent.trim() || '';
  }
  if (key === 'LimitedName') {
    return row.dataset.limitedname || row.querySelector('.limited-name')?.textContent.trim() || '';
  }
  return '';
}

function compareStrings(valueA, valueB) {
  if (valueA < valueB) return -1;
  if (valueA > valueB) return 1;
  return 0;
}

function updateSortIcons(activeKey) {
  const keys = ['SongName', 'ArtistName', 'LastSungDate', 'SungCount', 'ReleaseDate', 'LimitedName'];
  keys.forEach(k => {
    const icon = document.getElementById(
      k === 'SongName' ? 'icon-title' :
      k === 'ArtistName' ? 'icon-artist' :
      k === 'LastSungDate' ? 'icon-lastsungdate' :
      k === 'ReleaseDate' ? 'icon-release' :
      k === 'LimitedName' ? 'icon-limited' :
      'icon-count'
    );

    if (!icon) return;
    icon.textContent = activeKey === k ? (currentSort.asc ? '▲' : '▼') : '↕';
  });
}

function syncMobileSortControls() {
  const keyControl = document.querySelector('.mobile-sort-key');
  const directionControl = document.querySelector('.mobile-sort-direction');
  if (!keyControl || !directionControl) return;

  if (Array.from(keyControl.options).some(option => option.value === currentSort.key)) {
    keyControl.value = currentSort.key;
  }
  directionControl.value = currentSort.asc ? 'asc' : 'desc';
}

function bindMobileSortControls() {
  const keyControl = document.querySelector('.mobile-sort-key');
  const directionControl = document.querySelector('.mobile-sort-direction');
  if (!keyControl || !directionControl) return;

  keyControl.addEventListener('change', () => {
    currentSort = { key: keyControl.value, asc: directionControl.value === 'asc' };
    sortTable(keyControl.value, false);
  });

  directionControl.addEventListener('change', () => {
    currentSort.asc = directionControl.value === 'asc';
    sortTable(currentSort.key || keyControl.value, false);
  });
}

function bindMobileSearchToggle() {
  const toggle = document.querySelector('.mobile-search-toggle');
  const fields = document.getElementById('search-form-fields');
  if (!toggle || !fields) return;

  const mobileQuery = window.matchMedia('(max-width: 640px)');
  const setVisibility = isCollapsed => {
    fields.hidden = isCollapsed;
    toggle.setAttribute('aria-expanded', String(!isCollapsed));
    toggle.querySelector('.mobile-search-toggle-label').textContent = isCollapsed
      ? '絞り込み'
      : '閉じる';
  };

  setVisibility(false);
  mobileQuery.addEventListener('change', () => setVisibility(false));

  toggle.addEventListener('click', () => {
    const isExpanded = toggle.getAttribute('aria-expanded') === 'true';
    fields.hidden = isExpanded;
    toggle.setAttribute('aria-expanded', String(!isExpanded));
    toggle.querySelector('.mobile-search-toggle-label').textContent = isExpanded
      ? '絞り込み'
      : '閉じる';
  });
}

function bindSortHeaders() {
  document.querySelectorAll('.sort-col').forEach(header => {
    // 二重登録防止（一度既存のリスナーを解除）
    header.removeEventListener('click', handleHeaderClick);
    header.addEventListener('click', handleHeaderClick);
  });
}

// クリックハンドラーを独立した関数として定義
function handleHeaderClick(e) {
  const key = e.currentTarget.dataset.sortKey;
  if (!key) {
    console.error('data-sort-key が設定されていません:', e.currentTarget);
    return;
  }
  sortTable(key);
}

function sortTable(key, toggle = true) {
  const rows = Array.from(document.querySelectorAll('#result-item .list-item'));
  if (!rows.length) return;

  if (toggle) {
    if (currentSort.key === key) {
      currentSort.asc = !currentSort.asc; // 同じキーなら昇順/降順を反転
    } else {
      currentSort.key = key;
      currentSort.asc = true; // 新しいキーなら昇順からスタート
    }
  } else {
    currentSort.key = key;
  }

  rows.sort((a, b) => {
    const valA = getRowSortValue(a, key);
    const valB = getRowSortValue(b, key);

    if (typeof valA === 'string') {
      const comparison = compareStrings(valA, valB);
      return currentSort.asc ? comparison : -comparison;
    }

    return currentSort.asc ? valA - valB : valB - valA;
  });

  const container = document.getElementById('result-item');
  rows.forEach(row => container.appendChild(row));
  
  updateSortIcons(key);
  saveSortState();
  syncMobileSortControls();
}

function fallbackCopyText(text) {
  const textarea = document.createElement('textarea');
  textarea.value = text;
  textarea.setAttribute('readonly', '');
  textarea.style.position = 'fixed';
  textarea.style.top = '-9999px';
  textarea.style.left = '-9999px';
  textarea.setAttribute('aria-hidden', 'true');
  document.body.appendChild(textarea);
  textarea.focus();
  textarea.select();
  textarea.setSelectionRange(0, textarea.value.length);

  try {
    return navigator.clipboard && window.isSecureContext
      ? navigator.clipboard.writeText(text).then(() => true).catch(() => false)
      : false;
  } catch (error) {
    console.error('Clipboard fallback failed:', error);
    return false;
  } finally {
    document.body.removeChild(textarea);
  }
}

async function copyTextToClipboard(text) {
  if (!text) return false;

  try {
    if (navigator.clipboard && window.isSecureContext) {
      await navigator.clipboard.writeText(text);
      return true;
    }
  } catch (error) {
    console.warn('Clipboard API failed:', error);
  }

  return await fallbackCopyText(text);
}

document.addEventListener('click', async (event) => {
  const button = event.target.closest('.btn-copy');
  if (!button) return;

  event.preventDefault();

  const text = button.dataset.copyText || '';
  const originalHtml = button.innerHTML;

  try {
    const copied = await copyTextToClipboard(text);
    if (!copied) {
      button.classList.add('copy-error');
      button.setAttribute('title', 'コピーに失敗しました');
      button.setAttribute('aria-label', 'コピーに失敗しました');
      showCopyToast('コピーに失敗しました');
      setTimeout(() => {
        button.classList.remove('copy-error');
        button.setAttribute('title', 'コピー');
        button.setAttribute('aria-label', '曲名とアーティスト名をコピー');
        button.innerHTML = originalHtml;
      }, 1200);
      return;
    }

    button.classList.add('copied');
    button.innerHTML = '<span aria-hidden="true">✓</span>';
    button.setAttribute('title', 'コピーしました');
    button.setAttribute('aria-label', 'コピーしました');
    showCopyToast('コピーしました: ' + text);

    setTimeout(() => {
      button.classList.remove('copied');
      button.setAttribute('title', 'コピー');
      button.setAttribute('aria-label', '曲名とアーティスト名をコピー');
      button.innerHTML = originalHtml;
    }, 1200);
  } catch (error) {
    console.error('Copy button handler failed:', error);
  }
});

document.addEventListener('DOMContentLoaded', () => {
  bindMobileSearchToggle();
  bindMobileSortControls();
  const isDetailsPage = document.querySelector('.sort-col[data-sort-key="ReleaseDate"]');

  if (isDetailsPage) {
    currentSort = { key: 'ReleaseDate', asc: false };
    syncMobileSortControls();
    sortTable('ReleaseDate', false);
    return;
  }

  currentSort = loadSortState();
  syncMobileSortControls();

  if (currentSort.key) {
    sortTable(currentSort.key, false);
  } else if (document.querySelector('.sort-col[data-sort-key="ArtistName"]')) {
    sortTable('ArtistName', false);
  } else if (document.querySelector('.sort-col[data-sort-key="ReleaseDate"]')) {
    sortTable('ReleaseDate', false);
  } else {
    updateSortIcons(null);
    syncMobileSortControls();
  }
});