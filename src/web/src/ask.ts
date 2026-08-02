import type { User } from 'oidc-client-ts'
import {
    apiPost,
    initNavbarAuth,
    requireAuth,
    type QuestionResponse,
} from './common.ts'

const form = document.getElementById('ask-form') as HTMLFormElement
const titleInput = document.getElementById('ask-title') as HTMLInputElement
const bodyInput = document.getElementById('ask-body') as HTMLTextAreaElement
const tagsInput = document.getElementById('ask-tags') as HTMLInputElement
const errorEl = document.getElementById('ask-error')!
const submitBtn = document.getElementById('ask-submit') as HTMLButtonElement

async function submitQuestion(user: User): Promise<void> {
    submitBtn.disabled = true
    errorEl.classList.add('is-hidden')

    try {
        const question = await apiPost<QuestionResponse>('/questions', {
            title: titleInput.value,
            body: bodyInput.value,
            tags: tagsInput.value.split(',').map(tag => tag.trim()).filter(Boolean),
        }, user.access_token)

        window.location.href = `question.html?id=${question.id}`
    } catch (error) {
        console.error(error)
        errorEl.textContent = "Couldn't post your question. Try again."
        errorEl.classList.remove('is-hidden')
        submitBtn.disabled = false
    }
}

form.addEventListener('submit', event => {
    event.preventDefault()
    void requireAuth(submitQuestion)
})

function init(): void {
    initNavbarAuth()
}

init()