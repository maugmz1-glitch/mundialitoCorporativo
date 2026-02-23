/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  reactStrictMode: true,
  // Proxy /api/* se hace en app/api/[...path]/route.ts (lee API_URL en runtime).
  // rewrites() eliminado a propósito (build time vs Docker).
};

module.exports = nextConfig;
