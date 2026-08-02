import { UserManager, type User } from 'oidc-client-ts'

const userManager = new UserManager({
    authority: import.meta.env.VITE_KEYCLOAK_AUTHORITY,
    client_id: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,

    redirect_uri: `${window.location.origin}/callback.html`,
    post_logout_redirect_uri: `${window.location.origin}/index.html`,
    response_type: 'code',
    scope: 'openid profile email',
})

export function login(): Promise<void> {
    return userManager.signinRedirect()
}

export function logout(): Promise<void> {
    return userManager.signoutRedirect()
}

export async function getUser(): Promise<User | null> {
    const user = await userManager.getUser()
    if (!user || user.expired) {
        return null
    }
    return user
}

export function handleLoginCallback(): Promise<User> {
    return userManager.signinRedirectCallback()
}