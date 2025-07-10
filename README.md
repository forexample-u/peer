# Peer

**Peer** is a server that can run many apps at the same time.

For example, you can create a chat, a task tracker, or share files between users (peer-to-peer).

There are many challenges such as:
- building the backend
- managing hosting
- discovering users or servers in peer-to-peer apps
- load balancing on the server

Peer helps solve these problems, so you only need to create a frontend for your application to work.

## API Endpoints

Peer includes 5 API endpoints: <br />

| Method | Endpoint                      | Parameters                 |
| ------ | ----------------------------- | -------------------------- |
| POST   | {host}/peer/write/{id}        | text (string), file (blob) |
| GET    | {host}/peer/write/{id}/{text} | text (string)              |
| GET    | {host}/peer/text/{id}         | —                          |
| GET    | {host}/peer/file/{id}         | —                          |
| GET    | {host}/peer/download/{id}     | —                          |
