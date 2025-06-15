import os
from flask import Flask, request, jsonify, send_file
from datetime import datetime, timedelta
from werkzeug.datastructures.file_storage import FileStorage
from domain.data import Data
from domain.message import Message
from config import Config
from application.minidb import Minidb

app = Flask(__name__)
db = Minidb(os.path.join(Config.text_path), os.path.join(Config.file_path))
next_commit_time = datetime.utcnow() + timedelta(seconds=Config.commit_block_second)
count_query = 0

def write(message_id, message_text, file: FileStorage):
    global count_query
    global next_commit_time
    delete_unix_at = db.write(Message(message_id, message_text, file))
    if (delete_unix_at <= 0): return str(delete_unix_at)
    count_query += 1
    if count_query % 20 == 0:
        db.shrink()
    if db.block_in_bytes > Config.avg_size_block or datetime.utcnow() > next_commit_time:
        next_commit_time = datetime.utcnow() + timedelta(seconds=Config.commit_block_second)
        db.commit()
    return str(delete_unix_at)

@app.route('/peer/write', methods=['POST'])
def write_post():
    message_id = int(request.form.get('Id'))
    message_text = request.form.get('Text')
    file = request.files.get('File')
    return write(message_id, message_text, file)

@app.route('/peer/write/<int:id>/<text>', methods=['GET'])
def write_get(id, text):
    return write(id, text, None)

@app.route('/peer/get/<int:id>', methods=['GET'])
def get(id):
    data = db.get(id)
    return data.text if data else ''

@app.route('/peer/getfile/<int:id>', methods=['GET'])
def getfile(id):
    data = db.get(id)
    if data:
        _, extension = os.path.splitext(data.filename)
        full_path = os.path.join(Config.file_path, str(id) + extension)
        return send_file(full_path, download_name=data.filename, mimetype=data.content_type)
    return '', 404

@app.route('/peer/getfilepath/<int:id>', methods=['GET'])
def get_file_path(id):
    data = db.get(id)
    return f"{Config.web_url_file_path}/{id}{os.path.splitext(data.filename)[1]}" if data and data.file_size_of_bytes > 0 else ""

if __name__ == '__main__':
    dir_path = os.path.join(Config.file_path)
    if not os.path.exists(dir_path):
        os.makedirs(dir_path)
    Config.load()
    app.run()