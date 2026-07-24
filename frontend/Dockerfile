# syntax=docker/dockerfile:1

# ---------- build stage ----------
FROM node:20-alpine AS build
WORKDIR /app
COPY frontend/ ./
RUN npm install
RUN npm run build

# ---------- runtime stage (static files served by nginx) ----------
FROM nginx:1.27-alpine AS final
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/assetflow-web/browser /usr/share/nginx/html
EXPOSE 80
