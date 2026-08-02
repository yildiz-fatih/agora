import { handleLoginCallback } from './auth.ts'
import { apiGet } from './common.ts'

const statusEl = document.getElementById('status-message')!

async function init(): Promise<void> {
    try {
        const user = await handleLoginCallback()
        apiGet('/profiles/me', user.access_token).catch(error =>
            console.error('Failed to provision profile:', error),
        )
        window.location.href = 'index.html'
    } catch (error) {
        console.error(error)
        statusEl.textContent = 'Something went wrong signing you in. Try again from the home page.'
    }
}

init()