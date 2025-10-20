const express = require('express');
const multer = require('multer');
const path = require('path');
const fs = require('fs');
const app = express();
const PORT = process.env.PORT || 3000;

app.use((req, res, next) => {
  res.header('Access-Control-Allow-Origin', '*');
  res.header('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
  res.header('Access-Control-Allow-Headers', 'Content-Type, Authorization, Cache-Control');
  next();
});

app.use('/peer', express.static(path.join(__dirname, 'peer')));

app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'peer', 'index.html'));
});

const upload = multer({ storage: multer.memoryStorage() });
app.post('/peer/upload', upload.single('file'), (req, res) => {
  if (!req.file) {
    return res.status(400).send('');
  }
  const uploadFolder = path.join(__dirname, 'peer');
  fs.mkdirSync(uploadFolder, { recursive: true });
  let i = 0;
  let fileId;
  let filePath;
  do {
    fileId = `${i}_${req.file.originalname}`;
    filePath = path.join(uploadFolder, fileId);
    i++;
  } while (fs.existsSync(filePath));
  try {
    fs.writeFileSync(filePath, req.file.buffer);
    const fileUrl = `${req.protocol}://${req.get('host')}/peer/${fileId}`;
    res.status(200).send(fileUrl);
  } catch (error) {
    res.status(500).send('');
  }
});

app.listen(PORT, () => {
  console.log(`Peer running on port ${PORT}`);
});