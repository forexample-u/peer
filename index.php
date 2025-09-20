<?php ?>
<!DOCTYPE html>
<html lang="ru">

<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Peer</title>
  <style>
    body {
      margin: 0; padding: 0; height: 100%;
      background-color: #121212;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      color: #eee;
    }

    .chat-container {
      display: flex;
      flex-direction: column;
      height: 100vh;
      max-width: 100%;
      width: 100%;
      background: #1e1e1e;
    }

    .chat-header {
      display: none;
      padding: 15px;
      font-size: 1.5rem;
      font-weight: 700;
      text-align: center;
      border-bottom: 1px solid #333;
      color: #ddd;
      background-color: #1e1e1e;
    }

    .messages {
      flex-grow: 1;
      padding: 15px;
      overflow-y: auto;
      display: flex;
      flex-direction: column;
      gap: 12px;
      background: #121212;
    }

    .message {
      max-width: 70%;
      padding: 18px 16px;
      border-radius: 20px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.5);
      font-size: 14px;
      word-wrap: break-word;
      line-height: 1.3;
      user-select: text;
      position: relative;
    }

    .message.user {
      align-self: flex-start;
      background: #4a69bd;
      color: #e0e7ff;
      border-bottom-left-radius: 4px;
    }

    .message.user a {
      color: #c3dafe;
      text-decoration: underline;
      font-weight: 600;
    }

    form {
      display: flex;
      padding: 15px;
      border-top: 1px solid #333;
      background: #1e1e1e;
      gap: 10px;
      align-items: center;
    }

    label.file-input {
      flex-grow: 1;
      border: 2px dashed #555;
      border-radius: 20px;
      padding: 10px 15px;
      text-align: center;
      cursor: pointer;
      background-color: #222;
      color: #aaa;
      transition: border-color 0.3s, color 0.3s;
      user-select: none;
      font-size: 14px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    label.file-input:hover {
      border-color: #667eea;
      color: #c3dafe;
    }

    input[type="file"] {
      display: none;
    }

    button {
      background-color: #667eea;
      border: none;
      border-radius: 20px;
      padding: 10px 20px;
      color: white;
      font-weight: 600;
      cursor: pointer;
      transition: background-color 0.3s;
      font-size: 14px;
      flex-shrink: 0;
    }

    button:hover {
      background-color: #5a67d8;
    }

    .messages::-webkit-scrollbar {
      width: 8px;
    }

    .messages::-webkit-scrollbar-thumb {
      background-color: #555;
      border-radius: 4px;
    }

    .messages::-webkit-scrollbar-track {
      background-color: #222;
    }
  </style>
</head>

<body>
  <div class="chat-container">
    <div class="chat-header">Peer upload 📁</div>
    <div class="messages" id="messages"></div>

    <form id="uploadForm">
      <label class="file-input" for="fileInput" id="fileLabel">Click to select a file</label>
      <input type="file" id="fileInput" />
      <button type="submit">Send</button>
    </form>
  </div>

  <script>
    const hosturl = window.location.origin + "/peer.php";
    const form = document.getElementById('uploadForm');
    const fileInput = document.getElementById('fileInput');
    const fileLabel = document.getElementById('fileLabel');
    const messages = document.getElementById('messages');

    let links = JSON.parse(localStorage.getItem('files') || '[]');
    console.log(JSON.stringify(links));

    function addMessage(filename, url = null) {
      const msg = document.createElement('div');
      msg.classList.add('message', 'user');
      const ext = filename.split('.').pop().toLowerCase();
      if (['jpg', 'jpeg', 'png', 'gif'].includes(ext)) {
        const img = document.createElement('img');
        img.src = url;
        img.alt = filename;
        img.style.maxWidth = '100%';
        img.style.borderRadius = '12px';
        msg.appendChild(img);
      } else if (ext === 'mp4') {
        const video = document.createElement('video');
        video.src = url;
        video.controls = true;
        video.style.maxWidth = '100%';
        video.style.borderRadius = '12px';
        msg.appendChild(video);
      } else if (ext === 'mp3') {
        const audio = document.createElement('audio');
        audio.src = url;
        audio.controls = true;
        msg.appendChild(audio);
      }
      if (url) {
        const link = document.createElement('a');
        link.href = url;
        link.target = '_blank';
        link.rel = 'noopener noreferrer';
        link.textContent = url;
        link.style.display = 'block';
        link.style.marginTop = '6px';
        link.style.fontSize = '12px';
        link.style.color = '#ffffff';
        link.style.wordBreak = 'break-all';
        msg.appendChild(link);
      }
      messages.appendChild(msg);
      messages.scrollTop = messages.scrollHeight;
    }

    function renderStoredLinks() {
      messages.innerHTML = '';
      links.forEach(({ url }) => {
        const filename = url.split('/').pop();
        addMessage(filename, url);
      });
    }

    fileInput.addEventListener('change', () => {
      if (fileInput.files.length > 0) {
        fileLabel.textContent = fileInput.files[0].name;
      } else {
        fileLabel.textContent = 'Click to select a file';
      }
    });

    form.addEventListener('submit', async e => {
      e.preventDefault();
      if (!fileInput.files.length) {
        return;
      }
      const formData = new FormData();
      formData.append('file', fileInput.files[0]);
      try {
        const res = await fetch(hosturl + '/peer/upload', {
          method: 'POST',
          body: formData
        });
        if (!res.ok) throw new Error(`Server error: ${res.status}`);
        const fileurl = await res.text();
        const url = `${fileurl}`;
        links.push({ url });
        localStorage.setItem('files', JSON.stringify(links));
        addMessage(fileInput.files[0].name, url);
        fileInput.value = '';
        fileLabel.textContent = 'Click to select a file';
      } catch (err) {
        console.error(err);
      }
    });

    renderStoredLinks();
  </script>
</body>

</html>