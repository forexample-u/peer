from flask import Flask, Response, request, send_file, current_app
import os
import urllib.request
import requests

USER_AGENT = "forexampleu-peer/1.0"

def uploadfile(file):
    response = requests.post("https://0x0.st", files = { 'file': (file.filename, file.stream, file.mimetype) }, headers={'User-Agent': USER_AGENT})
    return response.text.strip()

def loadfile(filename):
    response = requests.get(f"https://0x0.st/{filename}", headers={'User-Agent': USER_AGENT})
    return response.content

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
    try:
        return uploadfile(file)
    except Exception:
        return "", 200

@app.route('/peer/<path:filename>')
def peerload(filename):
    try:
        extension = os.path.splitext(filename)[1]
        content_type = content_types.get(extension.lower(), 'application/octet-stream')
        content = loadfile(filename)
        return Response(content, content_type=content_type, headers={'Content-Disposition': f'inline; filename="{filename}"'})
    except Exception:
        return "", 404

@app.route('/')
def index():
    return send_file(os.path.join('static', 'peerdata', 'index.html'))

if __name__ == '__main__':
    app.run(host='0.0.0.0', debug=False)