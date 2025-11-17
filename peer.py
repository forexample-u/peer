from flask import Flask, request, send_file, current_app
import os

app = Flask(__name__)
content_types = {
    ".jpeg": "image/jpeg", ".jpg": "image/jpeg", ".png": "image/png", ".gif": "image/gif", ".webp": "image/webp",
    ".bmp": "image/bmp", ".svg": "image/svg+xml", ".ico": "image/x-icon",
    ".mp4": "video/mp4", ".webm": "video/webm", ".mkv": "video/x-matroska",
    ".mov": "video/quicktime", ".ogv": "video/ogg", ".flv": "video/x-flv",
    ".mp3": "audio/mpeg", ".aac": "audio/aac", ".wav": "audio/wav", ".flac": "audio/flac",
    ".ogg": "audio/ogg", ".opus": "audio/opus", ".m4a": "audio/mp4",
    ".pdf": "application/pdf", ".zip": "application/zip", ".php": "application/x-php",
    ".html": "text/html", ".css": "text/css", ".js": "application/javascript",
    ".json": "application/json", ".xml": "application/xml", ".txt": "text/plain", ".csv": "text/csv",
    ".woff": "font/woff", ".woff2": "font/woff2", ".rar": "application/vnd.rar", ".torrent": "application/x-bittorrent",
}

@app.after_request
def after_request(response):
    response.headers.add('Access-Control-Allow-Origin', '*')
    response.headers.add('Access-Control-Allow-Headers', 'Content-Type,Authorization,Cache-Control')
    response.headers.add('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE,OPTIONS')
    return response

@app.route('/peer/upload', methods=['POST'])
def upload():
    file = request.files['file']
    if file is None:
        return "", 200
    uploadpath = os.path.join(current_app.root_path, 'static', 'peerdata')
    os.makedirs(uploadpath, exist_ok=True)
    i = 0
    while True:
        fileid = f"{i}_{file.filename}"
        filepath = os.path.join(uploadpath, fileid)
        i += 1
        if os.path.exists(filepath):
            continue
        try:
            file.save(filepath)
            return f"{request.host_url.rstrip('/')}/peer/{fileid}", 200
        except Exception:
            if os.path.exists(filepath):
                continue
            return "", 200

@app.route('/peer/<filename>')
def load(filename):
    if not os.path.exists(os.path.join('static', 'peerdata', filename)):
        return "", 404
    _, extension = os.path.splitext(filename)
    content_type = content_types.get(extension.lower(), 'application/octet-stream')
    return send_file(os.path.join('static', 'peerdata', filename), mimetype=content_type)

@app.route('/')
def index():
    return send_file(os.path.join('static', 'peerdata', 'index.html'))

if __name__ == '__main__':
    app.run(host='0.0.0.0', debug=False)
