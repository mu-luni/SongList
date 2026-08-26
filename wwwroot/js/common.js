const SORT_STATE_KEY = 'songListSortState';
let currentSort = { key: null, asc: true };

function loadSortState() {
  try {
    const saved = sessionStorage.getItem(SORT_STATE_KEY);
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
    sessionStorage.setItem(SORT_STATE_KEY, JSON.stringify(currentSort));
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
}

document.addEventListener('DOMContentLoaded', () => {
  currentSort = loadSortState();

  if (currentSort.key) {
    sortTable(currentSort.key, false);
  } else if (document.querySelector('.sort-col[data-sort-key="ArtistName"]')) {
    sortTable('ArtistName', false);
  } else {
    updateSortIcons(null);
  }
});