import 'bulma/css/versions/bulma-no-dark-mode.min.css'
import './style.css'
import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'
import type { User } from 'oidc-client-ts'
import { getUser, login, logout } from './auth.ts'

dayjs.extend(relativeTime)

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface QuestionResponse {
    id: string
    title: string
    body: string
    score: number
    createdAt: string
    tags: string[]
    answerCount: number
    authorId: string
    authorUsername: string
}

export interface AnswerResponse {
    id: string
    body: string
    score: number
    createdAt: string
    authorId: string
    authorUsername: string
}

export interface QuestionDetailsResponse {
    id: string
    title: string
    body: string
    score: number
    createdAt: string
    tags: string[]
    answers: AnswerResponse[]
    authorId: string
    authorUsername: string
}

export interface ProfileResponse {
    id: string
    username: string
    bio: string
    createdAt: string
}

export interface SearchQuestion {
    id: string
    title: string
    body: string
    tags: string[]
    createdAt: string
    authorId: string
    authorUsername: string
}

export interface VoteResponse {
    score: number
}

export interface MyVoteResponse {
    targetId: string
    value: number
}

export class ApiError extends Error {
    status: number

    constructor(status: number, message: string) {
        super(message)
        this.name = 'ApiError'
        this.status = status
    }
}

async function apiRequest<T>(
    method: string,
    path: string,
    options: { body?: unknown; accessToken?: string } = {},
): Promise<T> {
    const { body, accessToken } = options
    const headers: Record<string, string> = {}
    if (body !== undefined) {
        headers['Content-Type'] = 'application/json'
    }
    if (accessToken) {
        headers['Authorization'] = `Bearer ${accessToken}`
    }

    const response = await fetch(`${API_BASE_URL}${path}`, {
        method,
        headers,
        body: body !== undefined ? JSON.stringify(body) : undefined,
    })
    if (!response.ok) {
        throw new ApiError(response.status, `${method} ${path} failed with status ${response.status}`)
    }
    return response.json() as Promise<T>
}

export function apiGet<T>(path: string, accessToken?: string): Promise<T> {
    return apiRequest<T>('GET', path, { accessToken })
}

export function apiPost<T>(path: string, body: unknown, accessToken: string): Promise<T> {
    return apiRequest<T>('POST', path, { body, accessToken })
}

export function apiPut<T>(path: string, body: unknown, accessToken: string): Promise<T> {
    return apiRequest<T>('PUT', path, { body, accessToken })
}

export function apiDelete<T>(path: string, accessToken: string): Promise<T> {
    return apiRequest<T>('DELETE', path, { accessToken })
}

export function escapeHtml(value: string): string {
    const div = document.createElement('div')
    div.textContent = value
    return div.innerHTML
}

export function truncate(value: string, maxLength: number): string {
    if (value.length <= maxLength) {
        return value
    }
    return `${value.slice(0, maxLength).trimEnd()}…`
}

export function formatRelativeTime(isoDate: string): string {
    return dayjs(isoDate).fromNow()
}

export function initNavbarAuth(): void {
    const guestBlock = document.getElementById('navbar-auth-guest')
    const userBlock = document.getElementById('navbar-auth-user')
    const usernameEl = document.getElementById('navbar-username')
    const loginBtn = document.getElementById('login-btn')
    const logoutBtn = document.getElementById('logout-btn')

    if (!guestBlock || !userBlock || !usernameEl || !loginBtn || !logoutBtn) {
        return
    }

    loginBtn.addEventListener('click', () => {
        void login()
    })
    logoutBtn.addEventListener('click', () => {
        void logout()
    })

    getUser().then(user => {
        if (!user) {
            return
        }
        const username = user.profile.preferred_username ?? 'User'
        usernameEl.innerHTML = `<a href="profile.html?id=${user.profile.sub}">${escapeHtml(username)}</a>`
        guestBlock.classList.add('is-hidden')
        userBlock.classList.remove('is-hidden')
    })
}

export async function requireAuth(onAuthenticated: (user: User) => void): Promise<void> {
    const user = await getUser()
    if (user) {
        onAuthenticated(user)
    } else {
        await login()
    }
}