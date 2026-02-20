/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  reactStrictMode: true,
  // Proxy /api/* se hace en app/api/[...path]/route.ts (lee API_URL en runtime).
  // Los rewrites aquí se evalúan en build time y en Docker no sirven (localhost ≠ api).
  // rewrites() eliminado a propósito.
};

module.exports = nextConfig;
