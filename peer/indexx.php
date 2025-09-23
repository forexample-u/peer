<?php ?>
<!DOCTYPE html>
<html lang="ru">

<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Peer chat</title>
  <style>
    body,
    html {
      margin: 0;
      padding: 0;
      height: 100%;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      background: #121212;
      color: #eee;
      display: flex;
      flex-direction: column;
    }

    #app {
      flex: 1;
      display: flex;
      height: 98%;
      border-top: 1px solid #333;
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

    #chatPanel {
      flex: 1;
      display: flex;
      flex-direction: column;
      background: #171717;
    }

    #chatHeader {
      padding: 15px 20px;
      border-bottom: 1px solid #333;
      font-weight: 600;
      font-size: 1.2rem;
      user-select: none;
      background: #1f1f1f;
    }

    .message {
      max-width: 65%;
      padding: 10px 14px;
      border-radius: 15px;
      font-size: 0.9rem;
      line-height: 1.3;
      word-wrap: break-word;
      background: #2a2a2a;
      color: #ddd;
      align-self: flex-start;
    }

    .message.from-me {
      border-bottom-right-radius: 4px;
    }

    .message.from-them {
      border-bottom-left-radius: 4px;
    }

    @media (max-width: 521px) {
      .message.from-me {
        align-self: flex-end;
      }
    }

    #messageForm {
      display: flex;
      padding: 12px 16px;
      border-top: 1px solid #333;
      background: #1f1f1f;
    }

    #messageForm textarea[type="text"] {
      flex: 1;
      padding: 10px 14px;
      border-radius: 20px;
      border: none;
      outline-offset: 2px;
      font-size: 1rem;
      background: #2a2a2a;
      color: #eee;
      width: 80%;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      font-size: 1rem;
      line-height: 1.2;
      font-weight: 400;
      height: 18px;
      resize: none;
      overflow: hidden;
    }

    #messageForm textarea[type="text"]::placeholder {
      color: #777;
    }

    #sendbutton {
      margin-left: 12px;
      background: #3a86ff;
      border: none;
      color: white;
      font-weight: 600;
      padding: 0 18px;
      border-radius: 20px;
      cursor: pointer;
      font-size: 1rem;
      transition: background-color 0.2s ease;
    }

    #sendbutton:hover {
      background: #265ecf;
    }

    #usernameModal {
      position: fixed;
      inset: 0;
      background: rgba(18, 18, 18, 0.95);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 10;
    }

    #usernameModalContent {
      background: #1f1f1f;
      padding: 24px 32px;
      border-radius: 12px;
      box-shadow: 0 0 15px #3a86ffaa;
      text-align: center;
    }

    #usernameModalContent h2 {
      margin-bottom: 18px;
      color: #3a86ff;
    }

    #usernameModalContent input {
      width: 100%;
      padding: 10px 14px;
      border-radius: 8px;
      border: none;
      font-size: 1.1rem;
      outline-offset: 2px;
      background: #2a2a2a;
      color: #eee;
    }

    #usernameModalContent button {
      margin-top: 16px;
      background: #3a86ff;
      border: none;
      color: white;
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

    ::-webkit-scrollbar-track {
      background: #333;
    }

    ::-webkit-scrollbar-thumb {
      background: #555;
      border-radius: 6px;
    }

    ::-webkit-scrollbar-thumb:hover {
      background: #777;
    }

    #attachBtn {
      background: #2a2a2a;
      border: none;
      color: #fff;
      font-weight: 600;
      padding: 10px 12px;
      border-radius: 20px;
      cursor: pointer;
      font-size: 1rem;
      transition: background-color 0.2s ease;
      margin-right: 12px;
      width: 36px;
      height: 36px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    #attachBtn:hover {
      background: #36393e;
    }

    #attachBtn:disabled {
      background: #555;
      cursor: not-allowed;
    }
  </style>
</head>

<body>
  <div id="usernameModal" style="display:none;">
    <div id="usernameModalContent">
      <h2>Username</h2>
      <input type="text" id="usernameModalInput" placeholder="John" maxlength="35" />
      <button id="usernameModalSubmitBtn">Start chat</button>
    </div>
  </div>
  <div id="app">
    <section id="chatPanel">
      <div id="chatHeader">Chat</div>
      <div id="messages"></div>
      <form id="messageForm" autocomplete="off">
        <button type="button" id="attachBtn" title="Attach file">📎</button>
        <textarea type="text" id="messageInput" placeholder="Write a message..."></textarea>
        <button type="submit" id="sendbutton">Send</button>
        <input type="file" id="fileInput" style="display:none;" />
      </form>
    </section>
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
    const sendBtn = messageForm.querySelector('button');

    let color = localStorage.getItem("color") || "";
    let username = localStorage.getItem("username") || "null";
    let dataid = 0;
    let intervalId = setInterval(renderMessages, 500);
    messageInput.value = localStorage.getItem("submitText") || "";

    function getRandomColor() {
      const r = Math.floor(Math.random() * 156) + 100;
      const g = Math.floor(Math.random() * 156) + 100;
      const b = Math.floor(Math.random() * 156) + 100;
      const hex = n => n.toString(16).padStart(2, '0');
      return `#${hex(r)}${hex(g)}${hex(b)}`;
    }

    async function uploadAsync(url, filename, blob) {
      const formData = new FormData();
      formData.append('file', blob, filename);
      const response = await fetch(`${url}/peer/upload`, { method: 'POST', body: formData });
      return await response.text();
    }

    async function getAsync(url, filename) {
      try {
        const response = await fetch(`${url}/peer/${filename}`);
        if (response.ok) {
          return await response.text();
        }
      } catch {}
      return null;
    }

    async function renderMessages() {
      if (document.hidden || color.length == 0) {
        return;
      }
      clearInterval(intervalId);
      let data = localStorage.getItem(`m${dataid}`);
      if (data == undefined || data == null) {
        data = await getAsync(host, `${dataid}_data.json`);
        if (data == undefined || data == null) {
          intervalId = setInterval(renderMessages, 12000);
          console.clear();
          return;
        }
      }
      dataid += 1;
      if (data.trim().startsWith('<html') && data.includes('</noscript>') && data.length > 256) {
        intervalId = setInterval(renderMessages, 12000);
        return;
      }
      if (data.length <= 512 && dataid < 1200) {
        localStorage.setItem(`m${dataid - 1}`, data);
      }
      if (data.length <= 256 && dataid < 8000) {
        localStorage.setItem(`m${dataid - 1}`, data);
      }
      try {
        data = JSON.parse(data);
      } catch {}
      const div = document.createElement('div');
      div.id = `m${dataid}`;
      div.className = 'message ' + (data.from === username && data.color === color ? 'from-me' : 'from-them');
      const headerDiv = document.createElement('div');
      headerDiv.style.fontWeight = '600';
      headerDiv.style.fontSize = '0.8rem';
      headerDiv.style.marginBottom = '4px';
      headerDiv.style.color = data.color;
      headerDiv.textContent = data.from.length > 4096 ? (data.from.slice(0, 4096) + '…') : data.from;
      headerDiv.style.wordWrap = 'break-word';
      headerDiv.style.wordBreak = 'break-word';
      headerDiv.style.overflowWrap = 'break-word';
      headerDiv.style.whiteSpace = 'normal';
      headerDiv.style.maxWidth = '100%';
      const timeSpan = document.createElement('span');
      timeSpan.style.fontWeight = '400';
      timeSpan.style.fontSize = '0.7rem';
      timeSpan.style.color = '#999';
      timeSpan.style.marginLeft = '8px';
      timeSpan.textContent = new Date(data.time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      headerDiv.appendChild(timeSpan);
      div.appendChild(headerDiv);
      if (1 == 1) {
        const textDiv = document.createElement('div');
        if (data.text === undefined) {
          textDiv.textContent = data;
        } else {
          textDiv.textContent = data.text;
        }
        textDiv.style.wordBreak = 'break-word';
        textDiv.style.overflowWrap = 'break-word';
        textDiv.style.maxWidth = '100%';
        textDiv.style.boxSizing = 'border-box';
        textDiv.style.whiteSpace = 'pre-wrap';
        div.appendChild(textDiv);
      }
      else { // funny mode
        div.innerHTML += `<div>${data.text == undefined ? data : data.text}</div>`;
      }
      if (data.fileurl) {
        const fileExtension = data.fileurl.split('.').pop().toLowerCase();
        let fileElement;
        if (['mp4', 'webm', 'ogv', 'avi', 'mov', 'mkv', 'flv', 'wmv'].includes(fileExtension)) {
          fileElement = document.createElement('video');
          fileElement.controls = true;
          fileElement.style.maxWidth = '100%';
          fileElement.style.marginTop = '8px';
          const source = document.createElement('source');
          source.src = data.fileurl;
          fileElement.appendChild(source);
        } else if (['mp3', 'wav', 'ogg', 'aac', 'flac', 'm4a', 'wma'].includes(fileExtension)) {
          fileElement = document.createElement('audio');
          fileElement.controls = true;
          fileElement.style.marginTop = '8px';
          const source = document.createElement('source');
          source.src = data.fileurl;
          fileElement.appendChild(source);
        } else if (['png', 'jpg', 'jpeg', 'gif', 'webp', 'svg', 'bmp', 'tiff', 'ico'].includes(fileExtension)) {
          fileElement = document.createElement('img');
          fileElement.src = data.fileurl;
          fileElement.alt = 'Attached image';
          fileElement.style.maxWidth = '100%';
          fileElement.style.marginTop = '8px';
        } else {
          fileElement = document.createElement('a');
          fileElement.href = data.fileurl;
          fileElement.target = "_blank";
          fileElement.style.color = '#fff';
          fileElement.textContent = data.fileurl;
          fileElement.style.display = 'block';
          fileElement.style.marginTop = '8px';
          fileElement.style.maxWidth = '100%';
          fileElement.style.wordBreak = 'break-word';
          fileElement.style.overflowWrap = 'break-word';
          fileElement.style.whiteSpace = 'normal';
        }
        div.appendChild(fileElement);
      }
      messagesEl.appendChild(div);
      intervalId = setInterval(renderMessages, 0);
    }

    messageForm.addEventListener('submit', async e => {
      e.preventDefault();
      const text = messageInput.value.trim();
      localStorage.setItem("submitText", text);
      const file = fileInput.files[0];
      let fileurl = null;
      if (file != undefined && file != null) {
        fileurl = await uploadAsync(host, file.name, file);
      }
      const message = { from: username, text, time: new Date(), color: color, fileurl: fileurl };
      if (!text) return;
      let isSuccess = false;
      const blob = new Blob([JSON.stringify(message)], { type: 'text/plain' });
      for (let i = 0; i < 5; i++) {
        let messageid = await uploadAsync(host, `data.json`, blob);
        if (messageid == "" || (messageid.trim().startsWith('<html') && messageid.includes('</noscript>'))) {
          continue;
        }
        isSuccess = true;
        break;
      }
      await renderMessages();
      if (isSuccess) {
        messageInput.value = '';
        attachBtn.innerHTML = "📎";
        fileInput.value = "";
        messageInput.style.height = '18px';
        localStorage.removeItem("submitText");
      }
    });

    document.addEventListener('DOMContentLoaded', async function () {
      if (color.length > 0) {
        chatHeader.innerHTML = username;
        chatHeader.style.color = color;
        await renderMessages();
      }
      else {
        app.style = "display:none;";
        usernameModal.style = '';
      }
    });

    usernameModalSubmitBtn.onclick = async () => {
      username = usernameModalInput.value.trim();
      color = getRandomColor();
      if (!username) {
        usernameModalInput.focus();
        return;
      }
      usernameModal.style.display = 'none';
      app.style.display = 'flex';
      localStorage.setItem("username", username);
      localStorage.setItem("color", color);
      chatHeader.innerHTML = username;
      chatHeader.style.color = color;
      await renderMessages();
    };

    usernameModalInput.addEventListener('keydown', e => {
      if (e.key === 'Enter') usernameModalSubmitBtn.click();
    });

    attachBtn.addEventListener('click', () => {
      fileInput.click();
    });

    fileInput.addEventListener('change', async () => {
      const file = fileInput.files[0];
      if (file) {
        attachBtn.title = file.name;
        attachBtn.innerHTML = "📄";
      }
    });

    const textarea = document.querySelector('#messageForm textarea[type="text"]');
    textarea.addEventListener('input', autoResize);
    function autoResize() {
      this.style.height = 'auto';
      this.style.height = this.scrollHeight - 40 + 'px';
    }
  </script>
</body>

</html>