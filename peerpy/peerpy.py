import os
import shutil
from flask import Flask, request, jsonify, send_file
from datetime import datetime, timezone
from werkzeug.datastructures.file_storage import FileStorage
from utils.hash_helper import hash_algorithm
from domain.data import Data
from domain.message import Message
from config import Config

messages = {}
files = {}
app = Flask(__name__)

def write(message_id, message_text, file: FileStorage):
    # settings
    max_one_file_size = Config.max_one_file_size
    min_debuf_size_bytes = Config.min_debuf_size_bytes
    max_length_text = Config.max_length_text
    debuf_second = Config.debuf_second
    big_message_second = Config.big_message_second
    small_message_second = Config.small_message_second
    first_100_small_message_second = Config.first_100_small_message_second
    small_text_length = Config.small_text_length
    small_file_size_bytes = Config.small_file_size_bytes

    # if not correct not save file or text
    file_size_bytes = 0
    filename = ''
    content_type = ''
    if (file is None):
        file_size_bytes = file.content_length
        filename = file.filename;
        content_type = file.content_type

    if (message_id < 0 or file_size_bytes > max_one_file_size or len(message_text) > max_length_text or message_id in messages):
        return str(0)

    # remove old messages
    now_second_unix = int(datetime.utcnow().replace(tzinfo=timezone.utc).timestamp())
    filehash = hash_algorithm(filename + str(file_size_bytes))
    size_files = 0
    is_unique_file = filename != ''
    keys_to_remove = []
    for key, value in messages.items():
        size_files += value.file_size_of_bytes
        if (value.delete_unix_at < now_second_unix):
            _, extension = os.path.splitext(value.filename)
            fullpath = os.path.join('static', 'peer', str(key) + extension)
            keys_to_remove.append(key)
            try:
                os.remove(fullpath)
            except FileNotFoundError:
                pass
        if value.filehash == filehash:
            is_unique_file = False
    for key in keys_to_remove:
        del messages[key]

    # set bytes by hash
    if is_unique_file:
        _, extension = os.path.splitext(filename)
        filepath = os.path.join('static', 'peer', str(message_id) + extension)
        with open(filepath, 'wb') as stream:
            stream.write(file.read())

    debuf_subt_second = 0 if size_files < min_debuf_size_bytes else debuf_second
    add_second = big_message_second
    if len(message_text) <= small_text_length and file_size_bytes <= small_file_size_bytes:
        add_second = small_message_second if len(messages) > 100 else first_100_small_message_second
    delete_second_unix = now_second_unix + add_second - debuf_subt_second
    messages[message_id] = Data(message_text, filehash, filename, delete_second_unix, content_type, file_size_bytes)
    return str(delete_second_unix)

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
    data = messages.get(id)
    return data.text if data else ''

@app.route('/peer/getfile/<int:id>', methods=['GET'])
def getfile(id):
    data = messages.get(id)
    if data:
        _, extension = os.path.splitext(data.filename)
        full_path = os.path.join("static", "peer", str(id) + extension);
        return send_file(full_path, download_name=data.filename, mimetype=data.content_type)
    return '', 404

@app.route('/peer/getfilepath/<int:id>', methods=['GET'])
def get_file_path():
    data = messages.get(id)
    return f"peer/{id}{os.path.splitext(data.filename)[1]}" if data and data.file_size_of_bytes > 0 else ""

@app.route('/peer/list', methods=['GET'])
def list_messages():
    return jsonify([f"{k}:{v.delete_unix_at}" for k, v in messages.items() if v.filehash == 0])

if __name__ == '__main__':
    dir_path = os.path.join("static", "peer")
    if not os.path.exists(dir_path):
        os.makedirs(dir_path)
    Config.load()
    app.run()