# Peer

| Lang   | Endpoint                                             | CORS    | Version |
| ------ | ---------------------------------------------------- | ------- | ------- |
| Python | https://forexample-u.github.io/peer/index.html       | Blocked | v 2.0   |
| PHP    | http://peer.infinityfreeapp.com/peer/indexx.php      | Blocked | v 2.0   |
| Python | https://peer1.pythonanywhere.com/peer/indexx.html    | Allowed | v 2.0   |
| Python | https://flask-hello-world-phi-liart.vercel.app       | Allowed | v 2.0   |
| Python | https://forexampleu.pythonanywhere.com               | Allowed | v 2.0   |
| PHP    | https://peertest.liveblog365.com/peer/0_index.php    | Blocked | v 2.0   |
| PHP    | https://peer1.yzz.me/index.php                       | Blocked | v 2.0   |
| PHP    | https://peertest.liveblog365.com/start.php?talk=Open | Blocked | v 0.5B  |

CURL example:
```bash
curl -F "file=@C:/Users/user/Desktop/file.png" https://peer1.pythonanywhere.com/peer/upload
```

**Peer** is the server hosting the files with the index

For example, you can create a chat, a task tracker, or share files between users (peer-to-peer).

There are many challenges such as:
- building the backend
- managing hosting
- discovering users or servers in peer-to-peer apps
- load balancing on the server

Peer helps solve these problems, so you only need to create a frontend for your application to work.

# Rule

When creating **Peer**, think about 3 ideas:
1. Keep files as long as possible without deleting
2. Make public links, without captcha, without api-keys
3. Add peers.json

If you want to join the peer network, add peer.json to another server and specify the data:
```json
{ "url": "https://example.com", "isfreecors": true, "version": "v2.0", "gb": 11.3 }
```

## API Endpoints

| Method | Endpoint                      | Parameters                 |
| ------ | ----------------------------- | -------------------------- |
| POST   | {host}/peer/upload            | file (blob)                |
| GET    | {host}/peer/{id}              | text (string)              |
