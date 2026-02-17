from flask import Flask, request, send_file, current_app
import uuid
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
def peerupload():
    file = request.files['file']
    if file is None:
        return "", 200
    uploadpath = os.path.join(current_app.root_path, 'static', 'peerdata')
    os.makedirs(uploadpath, exist_ok=True)
    filename = str(uuid.uuid4()) + os.path.splitext(file.filename)[1]
    filepath = os.path.join(uploadpath, filename)
    try:
        file.save(filepath)
        baseurl = request.host_url
        if request.headers.get('X-Forwarded-Proto') == 'https' or request.headers.get('X-Forwarded-Scheme') == 'https' or request.headers.get('X-Scheme') == 'https':
            baseurl = baseurl.replace('http://', 'https://')
        return f"{baseurl.rstrip('/')}/peer/{filename}", 200
    except Exception:
        return "", 200

@app.route('/peer/<path:filename>')
def peerload(filename):
    filepath = os.path.join(current_app.root_path, 'static', 'peerdata', filename)
    if not os.path.exists(filepath):
        return "", 404
    extension = os.path.splitext(filename)[1]
    content_type = content_types.get(extension.lower(), 'application/octet-stream')
    return send_file(filepath, mimetype=content_type)

@app.route('/')
def peerindex():
    return send_file(os.path.join('static', 'peerdata', 'index.html'))

if __name__ == '__main__':
    app.run(host='0.0.0.0', debug=False)