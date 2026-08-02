import {
  apiGet,
  escapeHtml,
  formatRelativeTime,
  initNavbarAuth,
  truncate,
  type SearchQuestion,
} from './common.ts'

const resultsHeadingEl = document.getElementById('results-heading')!
const resultsListEl = document.getElementById('results-list')!

function resultCardHtml(result: SearchQuestion): string {
  const title = escapeHtml(result.title)
  const bodyPreview = escapeHtml(truncate(result.body, 200))
  const tagsHtml = result.tags.map(tag => `<span class="tag">${escapeHtml(tag)}</span>`).join('')

  return `
    <div class="box mb-4">
      <h2 class="title is-5 mb-2"><a class="has-text-black" href="question.html?id=${result.id}">${title}</a></h2>
      <p class="mb-3 has-text-grey-dark">${bodyPreview}</p>
      <div class="level is-mobile mb-0">
        <div class="level-left">
          <div class="level-item">
            <div class="tags mb-0">${tagsHtml}</div>
          </div>
        </div>
        <div class="level-right">
          <div class="level-item">
            <p class="has-text-grey">asked ${formatRelativeTime(result.createdAt)} by <a href="profile.html?id=${result.authorId}">${escapeHtml(result.authorUsername)}</a></p>
          </div>
        </div>
      </div>
    </div>
  `
}

async function init(): Promise<void> {
  initNavbarAuth()

  const query = new URLSearchParams(window.location.search).get('q')?.trim() ?? ''

  const searchInput = document.getElementById('search-input') as HTMLInputElement
  searchInput.value = query

  if (!query) {
    resultsListEl.innerHTML = '<p class="has-text-grey">Enter a search term above.</p>'
    return
  }

  resultsHeadingEl.textContent = `Results for "${query}"`

  try {
    const results = await apiGet<SearchQuestion[]>(`/search?q=${encodeURIComponent(query)}`)
    resultsHeadingEl.textContent = `${results.length} result${results.length === 1 ? '' : 's'} for "${query}"`
    resultsListEl.innerHTML = results.length > 0
      ? results.map(resultCardHtml).join('')
      : '<p class="has-text-grey">No results found.</p>'
  } catch (error) {
    console.error(error)
    resultsListEl.innerHTML = '<p class="has-text-danger">Couldn\'t load search results. Is the backend running?</p>'
  }
}

init()