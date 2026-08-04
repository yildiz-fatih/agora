# build stage
FROM node:24-trixie-slim AS build
WORKDIR /src

COPY package.json package-lock.json .
RUN npm ci

COPY . .
ARG VITE_API_BASE_URL
ARG VITE_KEYCLOAK_AUTHORITY
ARG VITE_KEYCLOAK_CLIENT_ID
RUN npm run build

# runtime stage
FROM nginx:1.31-trixie AS runtime
COPY --from=build /src/dist /usr/share/nginx/html
