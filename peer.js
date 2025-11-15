const express = require('express');
const multer = require('multer');
const path = require('path');
const fs = require('fs');
const app = express();
const PORT = process.env.PORT || 3000;

const contentTypes = {
  ".jpeg": "image/jpeg", ".jpg": "image/jpeg", ".png": "image/png", ".gif": "image/gif", ".webp": "image/webp",
  ".bmp": "image/bmp", ".svg": "image/svg+xml", ".ico": "image/x-icon",
  ".mp4": "video/mp4", ".webm": "video/webm", ".mkv": "video/x-matroska",
  ".mov": "video/quicktime", ".ogv": "video/ogg", ".flv": "video/x-flv",
  ".mp3": "audio/mpeg", ".aac": "audio/aac", ".wav": "audio/wav", ".flac": "audio/flac",
  ".ogg": "audio/ogg", ".opus": "audio/opus", ".m4a": "audio/mp4",
  ".pdf": "application/pdf", ".zip": "application/zip", ".php": "application/x-php",
  ".html": "text/html", ".css": "text/css", ".js": "application/javascript",
  ".json": "application/json", ".xml": "application/xml", ".txt": "text/plain", ".csv": "text/csv",
  ".woff": "font/woff", ".woff2": "font/woff2", ".rar": "application/vnd.rar", ".torrent": "application/x-bittorrent"
};

app.use((req, res, next) => {
  res.header('Access-Control-Allow-Origin', '*');
  res.header('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
  res.header('Access-Control-Allow-Headers', 'Content-Type, Authorization, Cache-Control');
  next();
});

const upload = multer({ storage: multer.memoryStorage() });
app.post('/peer/upload', upload.single('file'), (req, res) => {
  if (!req.file) {
    return res.status(400).send('');
  }
  const uploadpath = path.join(__dirname, 'peerdata');
  const filename = req.file.originalname;
  fs.mkdirSync(uploadpath, { recursive: true });
  let i = 0;
  while (true) {
    const fileid = `${i}_${filename}`;
    const filepath = path.join(uploadpath, fileid);
    i += 1;
    if (fs.existsSync(filepath)) {
      continue;
    }
    try {
      fs.writeFileSync(filepath, req.file.buffer);
      res.status(200).send(`${req.protocol}://${req.get('host')}/peer/${fileid}`);
    } catch (error) {
      if (fs.existsSync(filepath)) {
        continue;
      }
      res.status(200).send('');
    }
    break;
  }
});

app.get('/peer/:filename', (req, res) => {
  const filename = req.params.filename;
  const filepath = path.join(__dirname, 'peerdata', filename);
  if (!fs.existsSync(filepath)) {
    return res.status(404).send('File not found');
  }
  let contentType = contentTypes[path.extname(filename).toLowerCase()] || 'application/octet-stream';
  if (contentType.startsWith("text/") || contentType == "application/json" || contentType == "application/xml" || contentType == "application/javascript") {
    contentType += ";charset=utf-8";
  }
  res.setHeader('Content-Type', contentType);
  res.setHeader('Cache-Control', 'public, max-age=3600');
  res.sendFile(filepath);
});

app.get('/', (req, res) => { res.sendFile(path.join(__dirname, 'peerdata', 'index.html')); });

app.listen(PORT, () => {
  console.log(`Peer running on port ${PORT}`);
});