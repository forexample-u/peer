const CONTENT_TYPES = {
  ".jpeg": "image/jpeg", ".jpg": "image/jpeg", ".png": "image/png", ".gif": "image/gif", ".webp": "image/webp",
  ".bmp": "image/bmp", ".svg": "image/svg+xml", ".ico": "image/x-icon",
  ".mp4": "video/mp4", ".webm": "video/webm", ".mkv": "video/x-matroska",
  ".mov": "video/quicktime", ".ogv": "video/ogg", ".flv": "video/x-flv",
  ".mp3": "audio/mpeg", ".aac": "audio/aac", ".wav": "audio/wav", ".flac": "audio/flac",
  ".ogg": "audio/ogg", ".opus": "audio/opus", ".m4a": "audio/mp4",
  ".pdf": "application/pdf", ".zip": "application/zip", ".php": "application/x-php",
  ".html": "text/html", ".css": "text/css", ".js": "application/javascript",
  ".json": "application/json", ".xml": "application/xml", ".txt": "text/plain", ".csv": "text/csv",
  ".woff": "font/woff", ".woff2": "font/woff2", ".rar": "application/vnd.rar", ".torrent": "application/x-bittorrent",
};

const CORS_HEADERS = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Headers': 'Content-Type, Authorization, Cache-Control',
  'Access-Control-Allow-Methods': 'GET, PUT, POST, DELETE, OPTIONS',
};

export default {
  async fetch(request, env, ctx) {
    if (request.method === 'OPTIONS') {
      return new Response(null, { headers: CORS_HEADERS });
    }
    const url = new URL(request.url);
    const path = url.pathname;
    if (request.method === 'POST' && path === '/peer/upload') {
      return handleUpload(request, env);
    }
    if (request.method === 'GET' && path.startsWith('/peer/')) {
      const filename = path.replace('/peer/', '');
      return handleDownload(filename, env);
    }
    if (path === '/') {
      return handleIndex('index.html', env);
    }
    return new Response('', { status: 404, headers: CORS_HEADERS });
  }
};

async function handleUpload(request, env) {
  try {
    const formData = await request.formData();
    const file = formData.get('file');
    if (!file) {
      return new Response('', { status: 200, headers: CORS_HEADERS });
    }
    const fileExtension = getFileExtension(file.name);
    const filename = crypto.randomUUID() + fileExtension;
    await env.FILES.put(filename, file.stream(), {
      httpMetadata: {
        contentType: file.type || CONTENT_TYPES[fileExtension] || 'application/octet-stream',
        contentDisposition: `inline; filename="${filename}"`,
      }
    });
    return new Response(`https://${request.headers.get('Host')}/peer/${filename}`, { status: 200, headers: CORS_HEADERS });
  } catch (error) {
    return new Response('', { status: 200, headers: CORS_HEADERS });
  }
}

async function handleDownload(filename, env) {
  try {
    const file = await env.FILES.get(filename, 'stream');
    if (!file) {
      return new Response('', { status: 404, headers: CORS_HEADERS });
    }
    const metadata = await env.FILES.getWithMetadata(filename, 'stream');
    return new Response(file, {
      status: 200,
      headers: { 
        ...CORS_HEADERS,
        'Content-Type': metadata.metadata?.contentType || CONTENT_TYPES[getFileExtension(filename)] || 'application/octet-stream',
        'Cache-Control': 'public, max-age=31536000',
      }
    });

  } catch (error) {
    return new Response('', { status: 200, headers: CORS_HEADERS });
  }
}

async function handleIndex(filename, env) {
  const simplehtml = '<html><head></head><body><form action="/peer/upload" method="POST" enctype="multipart/form-data" target="_blank"><input type="file" name="file"><button type="submit">Save</button></form></body>';
  try {
    return await handleDownload(filename, env);
  } catch (error) {
    return new Response(simplehtml, { status: 200, headers: CORS_HEADERS });
  }
}

function getFileExtension(filename) {
  if (!filename) return '';
  const match = filename.match(/\.[^/.]+$/);
  return match ? match[0].toLowerCase() : '';
}