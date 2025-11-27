from flask import Flask, Response, request, send_file, current_app
from urllib.parse import unquote
import os
import json
import urllib.request
import requests

FIREBASE_URL = "<YOUR_FIREBASE_URL>"
WEB0X0ST_URL = "https://0x0.st"

def saveurl(filename, url):
    full_url = f"{FIREBASE_URL}/0x0st.json"
    json_data = json.dumps({ "name": filename, "url": url })
    req = urllib.request.Request(url=full_url, data=json_data.encode('utf-8'), headers={'Content-Type': 'application/json'}, method='POST')
    response_json = {}
    with urllib.request.urlopen(req) as response:
        response_json = json.loads(response.read().decode('utf-8'))
        print(response_json)

def geturls():
    full_url = f"{FIREBASE_URL}/0x0st.json"
    req = urllib.request.Request(url=full_url, method='GET')
    with urllib.request.urlopen(req) as response:
        response_data = response.read().decode('utf-8')
        if response_data == 'null':
            return {}
        result = {}
        for item_id, item_data in json.loads(response_data).items():
            result[item_data.get('name')] = item_data.get('url')
        return result

def uploadfile(file):
    response = requests.post(WEB0X0ST_URL, files = { 'file': (file.filename, file.stream, file.mimetype) }, headers = { 'User-Agent': 'forexampleu-github/1.0' })
    return response.text.strip()

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
    i = 0
    urls = geturls()
    while True:
        fileid = f"{i}_{file.filename}"
        i += 1
        if urls.get(fileid):
            continue
        try:
            saveurl(fileid, uploadfile(file))
            return f"{request.host_url.rstrip('/')}/peer/{fileid}", 200
        except Exception:
            urls = geturls()
            if urls.get(fileid):
                continue
            return "", 200

@app.route('/peer/<path:filename>')
def load(filename):
    urls = geturls()
    if urls.get(filename) is None:
        filename = unquote(filename)
        if urls.get(filename) is None:
            return "", 404
    _, extension = os.path.splitext(filename)
    content_type = content_types.get(extension.lower(), 'application/octet-stream')
    fileurl = urls[filename]
    response = requests.get(fileurl)
    return Response(response.content, content_type=content_type, headers={ 'Content-Disposition': f'inline; filename="{filename}"' })

@app.route('/')
def index():
    return send_file(os.path.join('static', 'peerdata', 'index.html'))

if __name__ == '__main__':
    app.run(host='0.0.0.0', debug=False)