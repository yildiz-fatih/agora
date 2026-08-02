import type { User } from 'oidc-client-ts'
import {
    ApiError,
    apiDelete,
    apiGet,
    apiPost,
    apiPut,
    escapeHtml,
    formatRelativeTime,
    initNavbarAuth,
    requireAuth,
    type AnswerResponse,
    type MyVoteResponse,
    type QuestionDetailsResponse,
    type VoteResponse,
} from './common.ts'
import { getUser } from './auth.ts'

type VoteTargetType = 'questions' | 'answers'

const statusMessageEl = document.getElementById('status-message')!
const questionContentEl = document.getElementById('question-content')!
const questionTitleEl = document.getElementById('question-title')!
const questionBodyEl = document.getElementById('question-body')!
const questionScoreEl = document.getElementById('question-score')!
const questionUpvoteBtn = document.getElementById('question-upvote') as HTMLButtonElement
const questionDownvoteBtn = document.getElementById('question-downvote') as HTMLButtonElement
const questionTagsEl = document.getElementById('question-tags')!
const questionBylineEl = document.getElementById('question-byline')!
const answerCountEl = document.getElementById('answer-count')!
const answersListEl = document.getElementById('answers-list')!
const answerForm = document.getElementById('answer-form') as HTMLFormElement
const answerBodyInput = document.getElementById('answer-body') as HTMLTextAreaElement
const answerErrorEl = document.getElementById('answer-error')!
const answerSubmitBtn = document.getElementById('answer-submit') as HTMLButtonElement

const myVotes = new Map<string, number>()

function showStatus(message: string): void {
    statusMessageEl.textContent = message
    statusMessageEl.classList.remove('is-hidden')
    questionContentEl.classList.add('is-hidden')
}

function renderTags(tags: string[]): void {
    const tagElements = tags.map(tag => {
        const span = document.createElement('span')
        span.className = 'tag'
        span.textContent = tag
        return span
    })
    questionTagsEl.replaceChildren(...tagElements)
}

function answerCardHtml(answer: AnswerResponse): string {
    const body = escapeHtml(answer.body)
    const vote = myVotes.get(answer.id)
    const upClass = vote === 1 ? 'is-link' : 'is-white'
    const downClass = vote === -1 ? 'is-danger' : 'is-white'

    return `
    <div class="box mb-4">
      <div class="columns">
        <div class="column is-narrow stats-column has-text-centered">
          <button type="button" class="button is-small ${upClass} vote-btn" data-target-type="answers" data-target-id="${answer.id}" data-direction="1">▲</button>
          <p class="has-text-weight-semibold is-size-5 my-2 vote-score">${answer.score}</p>
          <button type="button" class="button is-small ${downClass} vote-btn" data-target-type="answers" data-target-id="${answer.id}" data-direction="-1">▼</button>
        </div>
        <div class="column">
          <p class="mb-3">${body}</p>
          <p class="has-text-grey has-text-right">answered ${formatRelativeTime(answer.createdAt)} by <a href="profile.html?id=${answer.authorId}">${escapeHtml(answer.authorUsername)}</a></p>
        </div>
      </div>
    </div>
  `
}

async function loadMyVotes(question: QuestionDetailsResponse, accessToken: string): Promise<void> {
    const params = new URLSearchParams({ questionId: question.id })
    const answerIds = question.answers.map(a => a.id).join(',')
    if (answerIds) {
        params.set('answerIds', answerIds)
    }

    try {
        const votes = await apiGet<MyVoteResponse[]>(`/votes/me?${params.toString()}`, accessToken)
        for (const vote of votes) {
            myVotes.set(vote.targetId, vote.value)
        }
    } catch (error) {
        console.error('Failed to load your existing votes:', error)
    }
}

function updateVoteHighlight(targetId: string, upBtn: HTMLButtonElement, downBtn: HTMLButtonElement): void {
    const vote = myVotes.get(targetId)
    upBtn.classList.toggle('is-link', vote === 1)
    upBtn.classList.toggle('is-white', vote !== 1)
    downBtn.classList.toggle('is-danger', vote === -1)
    downBtn.classList.toggle('is-white', vote !== -1)
}

async function castVote(
    targetType: VoteTargetType,
    targetId: string,
    direction: 1 | -1,
    accessToken: string,
    clickedBtn: HTMLButtonElement,
): Promise<void> {
    const container = clickedBtn.closest('.stats-column')
    const scoreEl = container?.querySelector<HTMLElement>('.vote-score')
    const upBtn = container?.querySelector<HTMLButtonElement>('[data-direction="1"]')
    const downBtn = container?.querySelector<HTMLButtonElement>('[data-direction="-1"]')
    if (!scoreEl || !upBtn || !downBtn) {
        return
    }

    const currentVote = myVotes.get(targetId)

    try {
        let response: VoteResponse
        if (currentVote === direction) {
            response = await apiDelete<VoteResponse>(`/votes/${targetType}/${targetId}`, accessToken)
            myVotes.delete(targetId)
        } else {
            response = await apiPut<VoteResponse>(`/votes/${targetType}/${targetId}`, { value: direction }, accessToken)
            myVotes.set(targetId, direction)
        }
        scoreEl.textContent = String(response.score)
        updateVoteHighlight(targetId, upBtn, downBtn)
    } catch (error) {
        console.error('Vote failed:', error)
    }
}

function renderQuestion(question: QuestionDetailsResponse): void {
    questionTitleEl.textContent = question.title
    questionBodyEl.textContent = question.body
    questionScoreEl.textContent = String(question.score)
    renderTags(question.tags)

    questionBylineEl.innerHTML = `asked ${formatRelativeTime(question.createdAt)} by <a href="profile.html?id=${question.authorId}">${escapeHtml(question.authorUsername)}</a>`

    questionUpvoteBtn.dataset.targetId = question.id
    questionDownvoteBtn.dataset.targetId = question.id
    updateVoteHighlight(question.id, questionUpvoteBtn, questionDownvoteBtn)

    answerCountEl.textContent = String(question.answers.length)
    answersListEl.innerHTML = question.answers.length > 0
        ? question.answers.map(answerCardHtml).join('')
        : '<p class="has-text-grey">No answers yet.</p>'

    statusMessageEl.classList.add('is-hidden')
    questionContentEl.classList.remove('is-hidden')
}

questionContentEl.addEventListener('click', event => {
    const button = (event.target as HTMLElement).closest<HTMLButtonElement>('.vote-btn')
    if (!button) {
        return
    }

    const targetType = button.dataset.targetType
    const targetId = button.dataset.targetId
    const direction = button.dataset.direction === '1' ? 1 : button.dataset.direction === '-1' ? -1 : undefined
    if ((targetType !== 'questions' && targetType !== 'answers') || !targetId || !direction) {
        return
    }

    void requireAuth(user => {
        void castVote(targetType, targetId, direction, user.access_token, button)
    })
})

async function loadAndRenderQuestion(questionId: string): Promise<void> {
    showStatus('Loading question…')
    try {
        const question = await apiGet<QuestionDetailsResponse>(`/questions/${questionId}`)
        document.title = `${question.title} — Agora`

        const user = await getUser()
        if (user) {
            await loadMyVotes(question, user.access_token)
        }

        renderQuestion(question)
    } catch (error) {
        console.error(error)
        if (error instanceof ApiError && error.status === 404) {
            showStatus('Question not found.')
        } else {
            showStatus("Couldn't load this question. Is the backend running?")
        }
    }
}

async function submitAnswer(user: User, questionId: string): Promise<void> {
    answerSubmitBtn.disabled = true
    answerErrorEl.classList.add('is-hidden')

    try {
        await apiPost(`/questions/${questionId}/answers`, { body: answerBodyInput.value }, user.access_token)
        answerBodyInput.value = ''
        await loadAndRenderQuestion(questionId)
    } catch (error) {
        console.error('Failed to post answer:', error)
        answerErrorEl.textContent = "Couldn't post your answer. Try again."
        answerErrorEl.classList.remove('is-hidden')
    } finally {
        answerSubmitBtn.disabled = false
    }
}

answerForm.addEventListener('submit', event => {
    event.preventDefault()
    const questionId = new URLSearchParams(window.location.search).get('id')
    if (!questionId) {
        return
    }
    void requireAuth(user => {
        void submitAnswer(user, questionId)
    })
})

async function init(): Promise<void> {
    initNavbarAuth()

    const questionId = new URLSearchParams(window.location.search).get('id')
    if (!questionId) {
        showStatus('No question specified.')
        return
    }

    await loadAndRenderQuestion(questionId)
}

init()