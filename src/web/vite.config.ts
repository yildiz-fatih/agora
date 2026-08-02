import { resolve } from 'node:path'
import { defineConfig } from 'vite'

// Add a new line whenever a new top-level .html page is added.
export default defineConfig({
    build: {
        rollupOptions: {
            input: {
                index: resolve(import.meta.dirname, 'index.html'),
                question: resolve(import.meta.dirname, 'question.html'),
                search: resolve(import.meta.dirname, 'search.html'),
                callback: resolve(import.meta.dirname, 'callback.html'),
                ask: resolve(import.meta.dirname, 'ask.html'),
                profile: resolve(import.meta.dirname, 'profile.html'),
            },
        },
    },
})