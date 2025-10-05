<?php ?>
<!DOCTYPE html>
<html lang="ru">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Peer chat</title>
  <meta property="og:title" content="Peer free chat">
  <meta property="og:description" content="free chat">
  <meta property="og:type" content="website">
  <meta property="og:site_name" content="Peer">
  <link rel="icon" href="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'%3E%3Crect width='100' height='100' fill='white'/%3E%3Ctext x='50' y='70' font-size='70' text-anchor='middle' font-family='Arial'%3EP%3C/text%3E%3C/svg%3E" type="image/svg+xml">
  <style>
    body * {
      color: #fff;
    }

    body, html {
      margin: 0;
      padding: 0;
      height: 100%;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      background: #121212;
      display: flex;
      flex-direction: column;
    }

    #app {
      flex: 1;
      display: flex;
      height: 98%;
      flex-direction: column;
    }

    #messages {
      flex: 1;
      padding: 15px 20px;
      overflow-y: auto;
      background: #121212;
      display: flex;
      flex-direction: column;
      gap: 8px;
      max-width: 100vw;
    }

    #chatHeader {
      padding: 15px 20px;
      border-bottom: 1px solid #333;
      font-weight: 600;
      font-size: 1.2rem;
      user-select: none;
      background: #1f1f1f;
      position: relative;
      display: flex;
      justify-content: space-between;
    }

    #menuBtn {
      background: transparent;
      border: none;
      font-size: 1.5rem;
      cursor: pointer;
      padding: 0;
      line-height: 1;
    }

    #menu {
      list-style: none;
      margin: 0;
      padding: 5px 0;
      position: absolute;
      top: 100%;
      right: 20px;
      background: #2c2c2c;
      border: 1px solid #444;
      border-radius: 4px;
      margin-top: 5px;
      min-width: 120px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.5);
    }

    #menu button {
      background: transparent;
      border: none;
      width: 100%;
      padding: 8px 16px;
      text-align: left;
      cursor: pointer;
      font-size: 1rem;
    }

    #menu button:hover {
      background: #444;
    }

    .message {
      max-width: 65%;
      padding: 10px 14px;
      border-radius: 15px;
      font-size: 0.9rem;
      line-height: 1.3;
      word-wrap: break-word;
      background: #2a2a2a;
      align-self: flex-start;
      border-bottom-left-radius: 4px;
    }

    #messageForm {
      display: flex;
      padding: 12px 16px;
      border-top: 1px solid #333;
      background: #1f1f1f;
    }

    #attachBtn {
      background: #2a2a2a;
      border: none;
      border-radius: 20px;
      cursor: pointer;
      font-size: 1rem;
      transition: background-color 0.2s ease;
      margin-right: 12px;
      width: 36px;
      height: 38px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    #attachBtn:hover {
      background: #36393e;
    }

    #messageForm textarea {
      flex: 1;
      padding: 10px 14px;
      border-radius: 20px;
      border: none;
      background: #2a2a2a;
      width: 80%;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      font-size: 1rem;
      line-height: 1.2;
      font-weight: 400;
      height: 18px;
      resize: none;
      overflow: hidden;
    }

    #messageForm textarea::placeholder {
      color: #777;
    }

    #sendBtn {
      margin-left: 12px;
      background: #3a86ff;
      border: none;
      font-weight: 600;
      padding: 0 18px;
      border-radius: 20px;
      cursor: pointer;
      font-size: 1rem;
      height: 38px;
      transition: background-color 0.2s ease;
    }

    #sendBtn:hover {
      background: #265ecf;
    }

    #usernameModal {
      position: fixed;
      inset: 0;
      display: flex;
      justify-content: center;
      align-items: center;
    }

    #usernameModalContent {
      background: #1f1f1f;
      padding: 24px 32px;
      border-radius: 12px;
      box-shadow: 0 0 15px #3a86ffaa;
      text-align: center;
    }

    #usernameModalContent input {
      width: 100%;
      padding: 10px 14px;
      border-radius: 8px;
      border: none;
      font-size: 1.1rem;
      background: #2a2a2a;
      box-sizing: border-box;
    }
    
    #usernameModalContent button {
      margin-top: 16px;
      background: #3a86ff;
      border: none;
      font-weight: 600;
      padding: 10px 24px;
      border-radius: 8px;
      cursor: pointer;
      font-size: 1.1rem;
      width: 100%;
      transition: background-color 0.2s ease;
    }

    #usernameModalContent button:hover {
      background: #265ecf;
    }

    ::-webkit-scrollbar {
      width: 8px;
    }

    ::-webkit-scrollbar-thumb {
      background: #555;
      border-radius: 6px;
    }

    ::-webkit-scrollbar-thumb:hover {
      background: #777;
    }
  </style>
</head>

<body>
  <div id="usernameModal" style="display:none;">
    <div id="usernameModalContent">
      <h2 style="color: #3a86ff;">Username</h2>
      <input type="text" id="usernameModalInput" placeholder="user" maxlength="35" />
      <button id="usernameModalSubmitBtn">Start chat</button>
    </div>
  </div>
  <div id="app">
    <div id="chatHeader">
      <button id="menuBtn" onclick="document.getElementById('menu').style.display = document.getElementById('menu').style.display == 'none' ? '' : 'none';" title="Menu" aria-label="Menu">&#9776;</button>
      <ul id="menu" style="display:none;">
        <li><button onclick="window.open('https://github.com/forexample-u/peer', '_blank')">Github</button></li>
        <li><button id="option2">Change wallpaper</button></li>
        <li><button onclick="localStorage.removeItem('username'); localStorage.removeItem('color'); location.reload();">Change username</button></li>
      </ul>
    </div>
    <div id="messages"></div>
    <form id="messageForm" autocomplete="off">
      <button type="button" id="attachBtn" onclick="fileInput.click();" title="Attach file">📎</button>
      <textarea type="text" id="messageInput" placeholder="Write a message..."></textarea>
      <button type="submit" id="sendBtn">Send</button>
      <input type="file" id="fileInput" style="display:none;" />
    </form>
  </div>

  <script>
    const host = location.origin + "/peer.php";
    const usernameModal = document.getElementById('usernameModal');
    const usernameModalInput = document.getElementById('usernameModalInput');
    const usernameModalSubmitBtn = document.getElementById('usernameModalSubmitBtn');
    const app = document.getElementById('app');
    const messagesEl = document.getElementById('messages');
    const messageForm = document.getElementById('messageForm');
    const attachBtn = document.getElementById('attachBtn');
    const fileInput = document.getElementById('fileInput');
    const messageInput = document.getElementById('messageInput');
    const chatHeader = document.getElementById('chatHeader');

    let color = localStorage.getItem("color") || "";
    let username = localStorage.getItem("username") || "null";
    let dataid = 0;
    let intervalId = setInterval(renderMessages, 500);

    async function uploadAsync(url, filename, blob) {
      const formData = new FormData();
      formData.append('file', blob, filename);
      const response = await fetch(`${url}/peer/upload`, { method: 'POST', body: formData });
      return await response.text();
    }

    async function getAsync(url, filename) {
      try {
        const response = await fetch(`${url}/peer/${filename}`);
        return response.ok ? (await response.text()) : null;
      } catch { return null; }
    }

    async function renderMessages() {
      if (document.hidden || !color) { return; }
      clearInterval(intervalId);
      let data = localStorage.getItem(`m${dataid}`);
      if (data == undefined || data == null) {
        data = await getAsync(host, `${dataid}_data.json`);
        if (data == undefined || data == null) {
          intervalId = setInterval(renderMessages, 14000);
          if (Math.random() < 0.1) { console.clear(); }
          return;
        }
      }
      dataid += 1;
      if (data.trim().startsWith('<html') && data.includes('</noscript>') && data.length > 256) {
        intervalId = setInterval(renderMessages, 14000);
        return;
      }
      if (data.length <= 256 && dataid < 8000 || data.length <= 512 && dataid < 800) {
        localStorage.setItem(`m${dataid - 1}`, data);
      }
      try { data = JSON.parse(data); } catch { }
      const div = document.createElement('div');
      div.id = `m${dataid}`;
      div.className = 'message ' + (data.from === username && data.color === color ? 'from-me' : 'from-them');
      const headerDiv = document.createElement('div');
      headerDiv.style.color = data.color;
      headerDiv.style.fontWeight = '600';
      headerDiv.style.fontSize = '0.8rem';
      headerDiv.style.marginBottom = '4px';
      headerDiv.style.wordWrap = 'break-word';
      headerDiv.style.overflowWrap = 'break-word';
      headerDiv.style.maxWidth = '100%';
      headerDiv.style.whiteSpace = 'normal';
      headerDiv.style.wordBreak = 'break-word';
      headerDiv.textContent = data.from ? (data.from.length > 4096 ? (data.from.slice(0, 4096) + '…') : data.from) : "null";
      const timeSpan = document.createElement('span');
      timeSpan.style.fontWeight = '400';
      timeSpan.style.fontSize = '0.7rem';
      timeSpan.style.color = '#999';
      timeSpan.style.marginLeft = '8px';
      timeSpan.textContent = new Date(data.time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      const textDiv = document.createElement('div');
      textDiv.style.wordBreak = 'break-word';
      textDiv.style.overflowWrap = 'break-word';
      textDiv.style.maxWidth = '100%';
      textDiv.style.whiteSpace = 'pre-wrap';
      textDiv.style.boxSizing = 'border-box';
      const urlRegex = /(https?:\/\/[^\s]+)/g;
      (data.text || data).split(urlRegex).forEach(part => {
        if (urlRegex.test(part)) {
          const link = document.createElement('a');
          link.href = part;
          link.target = '_blank';
          link.textContent = part;
          textDiv.appendChild(link);
        } else {
          textDiv.appendChild(document.createTextNode(part));
        }
      });
      headerDiv.appendChild(timeSpan);
      div.appendChild(headerDiv);
      div.appendChild(textDiv);
      if (data.fileurl) {
        const ext = data.fileurl.split('.').pop().toLowerCase();
        let fileElement;
        if (['mp4', 'webm', 'ogv', 'avi', 'mov', 'mkv', 'flv', 'wmv'].includes(ext)) {
          fileElement = document.createElement('video');
          fileElement.controls = true;
          const source = document.createElement('source');
          source.src = data.fileurl;
          fileElement.appendChild(source);
        } else if (['mp3', 'wav', 'ogg', 'aac', 'flac', 'm4a', 'wma'].includes(ext)) {
          fileElement = document.createElement('audio');
          fileElement.controls = true;
          const source = document.createElement('source');
          source.src = data.fileurl;
          fileElement.appendChild(source);
        } else if (['png', 'jpg', 'jpeg', 'gif', 'webp', 'svg', 'bmp', 'tiff', 'ico'].includes(ext)) {
          fileElement = document.createElement('img');
          fileElement.src = data.fileurl;
        } else {
          fileElement = document.createElement('a');
          fileElement.href = data.fileurl;
          fileElement.target = "_blank";
          fileElement.textContent = data.fileurl;
          fileElement.style.display = 'block';
          fileElement.style.wordBreak = 'break-word';
          fileElement.style.overflowWrap = 'break-word';
          fileElement.style.whiteSpace = 'normal';
        }
        fileElement.style.marginTop = '8px';
        fileElement.style.maxWidth = '100%';
        div.appendChild(fileElement);
      }
      messagesEl.appendChild(div);
      if (dataid > 16 && messagesEl.scrollHeight - messagesEl.scrollTop - messagesEl.clientHeight < 100 + div.clientHeight) {
        messagesEl.scrollTop = messagesEl.scrollHeight;
      }
      intervalId = setInterval(renderMessages, 0);
    }

    messageForm.addEventListener('submit', async e => {
      e.preventDefault();
      const text = messageInput.value.trim();
      localStorage.setItem("submitText", text);
      if (!text) { return; }
      const file = fileInput.files[0];
      const fileurl = file ? (await uploadAsync(host, file.name, file)) : null;
      const message = { from: username, text, time: new Date(), color: color, fileurl: fileurl };
      const blob = new Blob([JSON.stringify(message)], { type: 'text/plain' });
      const messageid = await uploadAsync(host, 'data.json', blob);
      if (messageid == "" || (messageid.trim().startsWith('<html') && messageid.includes('</noscript>'))) {
        return;
      }
      await renderMessages();
      messageInput.value = "";
      messageInput.style.height = '18px';
      attachBtn.innerHTML = "📎";
      fileInput.value = "";
      localStorage.removeItem("submitText");
    });

    document.addEventListener('DOMContentLoaded', async () => {
      messageInput.value = localStorage.getItem("submitText") || "";
      if (color) {
        chatHeader.innerHTML = username + chatHeader.innerHTML;
        chatHeader.style.color = color;
        await renderMessages();
      } else {
        app.style.display = "none";
        usernameModal.style = '';
      }
    });

    usernameModalSubmitBtn.addEventListener('click', async () => {
      const r = Math.floor(Math.random() * 156) + 100;
      const g = Math.floor(Math.random() * 156) + 100;
      const b = Math.floor(Math.random() * 156) + 100;
      const hex = n => n.toString(16).padStart(2, '0');
      color = `#${hex(r)}${hex(g)}${hex(b)}`;
      username = usernameModalInput.value.trim() || ("user" + (28 + Math.floor(Math.random() * 50) * 2));
      usernameModal.style.display = 'none';
      app.style.display = '';
      localStorage.setItem("username", username);
      localStorage.setItem("color", color);
      chatHeader.innerHTML = username + chatHeader.innerHTML;
      chatHeader.style.color = color;
      await renderMessages();
    });

    fileInput.addEventListener('change', () => {
      attachBtn.title = fileInput.files[0].name;
      attachBtn.innerHTML = "📄";
    });

    messageInput.addEventListener('input', function () {
      this.style.height = 'auto';
      this.style.height = this.scrollHeight - 40 + 'px';
    });
  </script>
</body>
</html>