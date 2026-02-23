# Plataforma por ARG (permite multiarch y cumple regla FromPlatformFlagConstDisallowed)
ARG TARGETPLATFORM=linux/amd64
FROM --platform=$TARGETPLATFORM node:20-alpine AS base
WORKDIR /app

FROM base AS deps
COPY package.json ./
# npm install funciona sin package-lock.json; con lock file se puede usar npm ci
RUN npm install

FROM base AS build
COPY --from=deps /app/node_modules ./node_modules
COPY . .
ENV NEXT_TELEMETRY_DISABLED=1
# NextAuth exige AUTH_SECRET en tiempo de build; aquí es solo placeholder. El valor real debe inyectarse en runtime (ej. docker-compose).
ENV AUTH_SECRET=build-placeholder-dummy-secret-min-32-chars-for-docker-build
RUN npm run build

FROM base AS runner
ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs
COPY --from=build /app/public ./public
COPY --from=build --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=build --chown=nextjs:nodejs /app/.next/static ./.next/static
USER nextjs
EXPOSE 3000
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"
CMD ["node", "server.js"]
