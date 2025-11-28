# Peer

**Peer**: Free multi-hosting files

Upload and share files with a simple API. No tokens, no captcha, no accounts. Just free and easy file storage for your applications.

CURL example:
```bash
curl -F "file=@C:/Users/user/Desktop/file.png" https://peer1.pythonanywhere.com/peer/upload
```

# Hosting

| Lang   | Endpoint                                             | GB    | CORS    | Version |
| ------ | ---------------------------------------------------- | ----- | ------- | ------- |
| Python | https://forexample-u.github.io/peer/index.html       | 0.0   | Blocked | v 2.0   |
| PHP    | http://peer.infinityfreeapp.com/peer/indexx.php      | 5.0   | Blocked | v 2.0   |
| Python | https://peer1.pythonanywhere.com/peer/indexx.html    | 0.5   | Allowed | v 2.0   |
| Python | https://flask-hello-world-phi-liart.vercel.app       | 2.0   | Allowed | v 2.0   |
| Python | https://forexampleu.pythonanywhere.com               | 0.5   | Allowed | v 2.0   |
| Python | https://peer-y8z2.onrender.com                       | 20.0  | Blocked | v 2.0   |
| PHP    | https://peertest.liveblog365.com/peer/0_index.php    | 5.0   | Blocked | v 2.0   |
| PHP    | https://peer1.yzz.me/index.php                       | 5.0   | Blocked | v 2.0   |

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
