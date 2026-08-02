import {
  apiGet,
  escapeHtml,
  formatRelativeTime,
  initNavbarAuth,
  requireAuth,
  truncate,
  type QuestionResponse,
} from './common.ts'

const questionListEl = document.getElementById('question-list')!
const questionCountEl = document.getElementById('question-count')!

let questions: QuestionResponse[] = []

function questionCardHtml(question: QuestionResponse): string {
  const title = escapeHtml(question.title)
  const bodyPreview = escapeHtml(truncate(question.body, 200))
  const tagsHtml = question.tags.map(tag => `<span class="tag">${escapeHtml(tag)}</span>`).join('')

  return `
    <div class="box mb-4">
      <div class="columns">
        <div class="column is-narrow stats-column has-text-centered">
          <p class="has-text-weight-semibold is-size-5 mb-0">${question.score}</p>
          <p class="is-size-7 has-text-grey mb-3">votes</p>
          <p class="has-text-weight-semibold is-size-5 mb-0">${question.answerCount}</p>
          <p class="is-size-7 has-text-grey">answers</p>
        </div>
        <div class="column">
          <h2 class="title is-5 mb-2"><a class="has-text-black" href="question.html?id=${question.id}">${title}</a></h2>
          <p class="mb-3 has-text-grey-dark">${bodyPreview}</p>
          <div class="level is-mobile mb-0">
            <div class="level-left">
              <div class="level-item">
                <div class="tags mb-0">${tagsHtml}</div>
              </div>
            </div>
            <div class="level-right">
              <div class="level-item">
                <p class="has-text-grey">asked ${formatRelativeTime(question.createdAt)} by <a href="profile.html?id=${question.authorId}">${escapeHtml(question.authorUsername)}</a></p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
}

const askQuestionBtn = document.getElementById('ask-question-btn')!

askQuestionBtn.addEventListener('click', () => {
  void requireAuth(_user => {
    window.location.href = 'ask.html'
  })
})

async function init(): Promise<void> {
  initNavbarAuth()
  try {
    questions = await apiGet<QuestionResponse[]>('/questions')
    questionCountEl.textContent = String(questions.length)

    if (questions.length === 0) {
      questionListEl.innerHTML = '<p class="has-text-grey">No questions yet.</p>'
    } else {
      questionListEl.innerHTML = questions.map(questionCardHtml).join('')
    }
  } catch (error) {
    console.error(error)
    questionListEl.innerHTML = '<p class="has-text-danger">Couldn\'t load questions. Is the backend running?</p>'
  }
}

init()