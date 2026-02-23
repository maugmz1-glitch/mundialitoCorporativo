#!/usr/bin/env node
/**
 * Genera una línea AUTH_SECRET para .env.local (evita depender de npx auth secret).
 * Uso: node scripts/generate-auth-secret.js
 */
const crypto = require('crypto');
const secret = crypto.randomBytes(32).toString('base64');
console.log('Añade esta línea a tu archivo .env.local:\n');
console.log('AUTH_SECRET=' + secret);
