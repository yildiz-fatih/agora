# build stage
FROM node:24.19.0-alpine3.24 AS build
WORKDIR /src

COPY package.json package-lock.json .
RUN npm ci

COPY . .
ARG VITE_API_BASE_URL
ARG VITE_KEYCLOAK_AUTHORITY
ARG VITE_KEYCLOAK_CLIENT_ID
RUN npm run build

# runtime stage
FROM nginx:1.31.3-alpine3.24 AS runtime
COPY --from=build /src/dist /usr/share/nginx/html
