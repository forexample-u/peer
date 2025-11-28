from flask import Flask, Response, request, send_file, current_app
import os
import dropbox
from dropbox.exceptions import AuthError, ApiError
from urllib.parse import unquote

REFRESH_TOKEN = "<YOUR REFRESH_TOKEN>" #read in readme how get refresh token, NOT ACCESS TOKEN!
APP_KEY = "<YOUR APP_KEY>"
APP_SECRET = "<YOUR APP_SECRET>"

dbx = None

def connect_dropbox():
    global dbx
    dbx = dropbox.Dropbox(app_key=APP_KEY, app_secret=APP_SECRET, oauth2_refresh_token=REFRESH_TOKEN)
    dbx.users_get_current_account()

def upload_file(file, filename):
    try:
        dbx.files_upload(file, '/' + filename, mode=dropbox.files.WriteMode.overwrite)
    except:
        connect_dropbox()
        dbx.files_upload(file, '/' + filename, mode=dropbox.files.WriteMode.overwrite)
    
def get_file(filename):
    try:
        metadata, response = dbx.files_download('/' + filename)
        return response.content
    except:
        connect_dropbox()
        metadata, response = dbx.files_download('/' + filename)
        return response.content
    
def list_files(path=''):
    json_files = None
    try:
        json_files = dbx.files_list_folder(path)
    except:
        connect_dropbox()
        json_files = dbx.files_list_folder(path)
    files = []
    for entry in json_files.entries:
        if isinstance(entry, dropbox.files.FileMetadata):
            files.append(entry.name)
    return files

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
    files = list_files()
    i = 0
    while True:
        fileid = f"{i}_{file.filename}"
        i += 1
        if fileid in files:
            continue
        try:
            upload_file(file.read(), fileid)
            base_url = request.host_url
            if request.headers.get('X-Forwarded-Proto') == 'https' or request.headers.get('X-Forwarded-Scheme') == 'https' or request.headers.get('X-Scheme') == 'https':
                base_url = base_url.replace('http://', 'https://')
            return f"{base_url.rstrip('/')}/peer/{fileid}", 200
        except Exception:
            files = list_files()
            if fileid in files:
                continue
    return "", 200

@app.route('/peer/<path:filename>')
def load(filename):
    files = list_files()
    if not filename in files:
        decoded_filename = unquote(filename)
        if not decoded_filename in files:
            return "", 404
        filename = decoded_filename
    _, extension = os.path.splitext(filename)
    content_type = content_types.get(extension.lower(), 'application/octet-stream')
    return Response(get_file(filename), mimetype=content_type, headers={'Content-Disposition': f'inline; filename="{filename}"'})

@app.route('/')
def index():
    return send_file(os.path.join('static', 'peerdata', 'index.html'))

if __name__ == '__main__':
    connect_dropbox()
    app.run(host='0.0.0.0', debug=False)
