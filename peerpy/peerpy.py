import os
import shutil
from flask import Flask, request, jsonify, send_file
from datetime import datetime, timezone
from werkzeug.datastructures.file_storage import FileStorage
from utils.hash_helper import hash_algorithm
from domain.data import Data
from domain.message import Message
from config import Config
from application.minidb import Minidb

app = Flask(__name__)
db = Minidb(os.path.join("data", "text"), os.path.join("static", "peer"))
count_query = 0

def write(message_id, message_text, file: FileStorage):
    global count_query
    count_query += 1
    if count_query % 20 == 0:
        db.shrink()
    if db.block_in_bytes > Config.avg_size_block:
        db.commit()
    return str(db.write(Message(message_id, message_text, file)))

@app.route('/peer/write', methods=['POST'])
def write_post():
    message_id = int(request.form.get('Id'))
    message_text = request.form.get('Text')
    file = request.files.get('File')
    return write(message_id, message_text, file)

@app.route('/peer/write/<int:id>/<text>', methods=['GET'])
def write_get(id, text):
    return write(id, text, None);

@app.route('/peer/get/<int:id>', methods=['GET'])
def get(id):
    data = db.get(id)
    return data.text if data else ''

@app.route('/peer/getfile/<int:id>', methods=['GET'])
def getfile(id):
    data = db.get(id)
    if data:
        _, extension = os.path.splitext(data.filename)
        full_path = os.path.join("static", "peer", str(id) + extension);
        return send_file(full_path, download_name=data.filename, mimetype=data.content_type)
    return '', 404

@app.route('/peer/getfilepath/<int:id>', methods=['GET'])
def get_file_path(id):
    data = db.get(id)
    return f"peer/{id}{os.path.splitext(data.filename)[1]}" if data and data.file_size_of_bytes > 0 else ""

if __name__ == '__main__':
    dir_path = os.path.join("static", "peer")
    if not os.path.exists(dir_path):
        os.makedirs(dir_path)
    Config.load()
    app.run()