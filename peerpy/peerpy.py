from flask import Flask, request, jsonify, Response, send_from_directory
import os
import random
import string

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
    file_length = len(file.read())
    file.seek(0)
    file_id = ''.join(random.choices(string.digits, k=32))
    file_name = os.path.basename(file.filename)
    file.save(os.path.join(UPLOAD_FOLDER, f"{file_id}{file_name}"))
    current_url = request.host_url.rstrip('/')
    return f"{current_url}/static/{file_id}{file_name}", 200

@app.route('/index.html')
def index():
    return send_from_directory('static', 'index.html')

if __name__ == '__main__':
    app.run(debug=True)