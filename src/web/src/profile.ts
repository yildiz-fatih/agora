import {
    ApiError,
    apiGet,
    formatRelativeTime,
    initNavbarAuth,
    type ProfileResponse,
} from './common.ts'

const statusMessageEl = document.getElementById('status-message')!
const profileContentEl = document.getElementById('profile-content')!
const profileUsernameEl = document.getElementById('profile-username')!
const profileJoinedEl = document.getElementById('profile-joined')!
const profileBioEl = document.getElementById('profile-bio')!

function showStatus(message: string): void {
    statusMessageEl.textContent = message
    statusMessageEl.classList.remove('is-hidden')
    profileContentEl.classList.add('is-hidden')
}

function renderProfile(profile: ProfileResponse): void {
    profileUsernameEl.textContent = profile.username
    profileJoinedEl.textContent = `Joined ${formatRelativeTime(profile.createdAt)}`
    profileBioEl.textContent = profile.bio || 'No bio yet.'

    statusMessageEl.classList.add('is-hidden')
    profileContentEl.classList.remove('is-hidden')
}

async function init(): Promise<void> {
    initNavbarAuth()

    const profileId = new URLSearchParams(window.location.search).get('id')
    if (!profileId) {
        showStatus('No profile specified.')
        return
    }

    showStatus('Loading profile…')
    try {
        const profile = await apiGet<ProfileResponse>(`/profiles/${profileId}`)
        document.title = `${profile.username} — Agora`
        renderProfile(profile)
    } catch (error) {
        console.error(error)
        if (error instanceof ApiError && error.status === 404) {
            showStatus('Profile not found.')
        } else {
            showStatus("Couldn't load this profile. Is the backend running?")
        }
    }
}

init()