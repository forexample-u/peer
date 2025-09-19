from flask import Flask, request, jsonify, Response, send_from_directory
import os

app = Flask(__name__)
UPLOAD_FOLDER = os.path.join(os.getcwd(), 'static')
os.makedirs(UPLOAD_FOLDER, exist_ok=True)

@app.after_request
def after_request(response):
    response.headers.add('Access-Control-Allow-Origin', '*')
    response.headers.add('Access-Control-Allow-Headers', 'Content-Type,Authorization,Cache-Control')
    response.headers.add('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE,OPTIONS')
    return response

@app.route('/peer/<path:filename>')
def static_file(filename):
    return send_from_directory('static', filename)

@app.route('/peer/upload', methods=['POST'])
def upload():
    file = request.files['file']
    if file is None:
        return "", 400
    file_length = len(file.read())
    file.seek(0)
    i = 0
    while True:
        file_id = f"{i}_{file.filename}"
        i += 1
        file_path = os.path.join(UPLOAD_FOLDER, file_id)
        if os.path.exists(file_path):
            continue
        try:
            file.save(file_path)
            url = f"{request.host_url.rstrip('/')}/peer/{file_id}"
            return url, 200
        except Exception:
            if os.path.exists(file_path):
                continue
            return "", 500

@app.route('/index.html')
def index():
    return send_from_directory('static', 'index.html')

if __name__ == '__main__':
    app.run(debug=True)