# Peer

**Peer** is the server hosting the files with the index

For example, you can create a chat, a task tracker, or share files between users (peer-to-peer).

There are many challenges such as:
- building the backend
- managing hosting
- discovering users or servers in peer-to-peer apps
- load balancing on the server

Peer helps solve these problems, so you only need to create a frontend for your application to work.

All endpoints should be free of cors!

## API Endpoints

Peer includes 3 API endpoints: <br />

| Method | Endpoint                      | Parameters                 |
| ------ | ----------------------------- | -------------------------- |
| POST   | {host}/peer/upload            | file (blob)                |
| GET    | {host}/peer/{id}              | text (string)              |
| GET    | {host}/peer/check/{id}        | text (string)              |
