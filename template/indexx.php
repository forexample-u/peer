<?php ?>
<!DOCTYPE html>
<html>
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>Peer private chat</title>
  <link rel="icon" href="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'%3E%3Crect width='100' height='100' fill='white'/%3E%3Ctext x='50' y='70' font-size='70' text-anchor='middle' font-family='Arial'%3EP%3C/text%3E%3C/svg%3E" type="image/svg+xml">
  <style>
    .message {
      padding: 16px;
      background: #2b2b2b;
      border-radius: 20px;
      align-self: flex-start;
    }
    body {
      margin: 0;
      font-family: 'Segoe UI';
      display: flex;
      flex-direction: column;
      height: 100vh;
    }
    #header {
      background: #1e1e1e;
      padding: 10px 15px;
      display: flex;
      justify-content: flex-end;
    }
    #messages {
      padding: 15px;
      background: #121212;
      overflow-y: auto;
      display: flex;
      flex-direction: column;
      gap: 12px;
      flex-grow: 1;
    }
    #uploadForm {
      background: #1e1e1e;
      display: flex;
      padding: 15px;
      gap: 10px;
    }
    #fileInput {
      padding: 10px;
      flex-grow: 1;
      border: 2px dashed#555;
      text-align: center;
      color: #aaa;
      cursor: pointer;
    }
    button {
      padding: 10px 20px;
      background: #667eea;
      border-radius: 20px;
      border: none;
      color: #fff;
      cursor: pointer;
    }
    audio, video, img {
      max-width: 100%;
      margin-bottom: 6px;
    }
    a {
      display: block;
      color: #fff;
      word-break: break-all;
      text-decoration: none;
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
  <div id="header"><a id="downloadLink" href="#" download="history.json">history</a></div>
  <div id="messages"></div>
  <form id="uploadForm">
    <input type="file" id="fileInput" multiple />
    <button type="submit">Send</button>
  </form>
  <script>
    const host = location.origin + "/peer.php";
    function addMsg(url) {
      const ext = url.split('.').pop().toLowerCase();
      const msg = document.createElement('div');
      if (['jpg', 'jpeg', 'png', 'gif'].includes(ext)) msg.innerHTML = '<img src="' + url + '" />';
      if (['mp4', 'mov'].includes(ext)) msg.innerHTML = '<video controls><source src="' + url + '"></source></video>';
      if (['mp3', 'wav'].includes(ext)) msg.innerHTML = '<audio controls><source src="' + url + '"></source></audio>';
      msg.className = 'message';
      msg.innerHTML += '<a target="_blank" href="' + url + '">' + url + '</a>';
      if (url && url.startsWith("http")) document.getElementById('messages').appendChild(msg);
    }
    let links = JSON.parse(localStorage.getItem('files') || '[]');
    links.forEach(({ url }) => addMsg(url));
    document.getElementById('uploadForm').onsubmit = async e => {
      e.preventDefault();
      const fileInput = document.getElementById('fileInput');
      const button = document.querySelector('#uploadForm button');
      button.disabled = true;
      for (let file of fileInput.files) {
        button.textContent = 'Uploading... 0%';
        const fd = new FormData(); fd.append('file', file);
        const xhr = new XMLHttpRequest();
        xhr.open('POST', host + '/peer/upload');
        xhr.upload.onprogress = function(e) { if (e.lengthComputable) { button.textContent = `Uploading... ${Math.round((e.loaded / e.total) * 100)}%`; } };
        xhr.onload = function() {
          const url = xhr.responseText.replace("/peer.php/", "/");
          if (url && url.startsWith("http")) {
            addMsg(url);
            links.push({ url });
            localStorage.setItem('files', JSON.stringify(links));
          }
          button.textContent = 'Send';
        };
        xhr.onerror = function() { button.textContent = 'Error'; };
        xhr.send(fd);
      }
      button.disabled = false;
      fileInput.value = '';
    };
    document.getElementById("downloadLink").onclick = e => {
       document.getElementById("downloadLink").href = URL.createObjectURL(new Blob([JSON.stringify(links)], { type: 'application/json' }));
    }
  </script>
</body>
</html>