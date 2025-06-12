import threading
import os
import time
import json
from config import Config
from domain.data import Data
from domain.block import Block
from domain.message import Message

class Minidb:
    def __init__(self, text_path: str, file_path: str, block_start_index: int = 0):
        self._text_path = text_path
        self._file_path = file_path
        self._is_lock_by_commit = False
        self._is_lock_by_shrink = False
        self._block_data = []
        self._blocks = {}
        self.block_index = block_start_index
        self.block_in_bytes = 0
        self.size_all_block_in_bytes = 0
        self.load()

    def write(self, message: Message) -> int:
        text_size = len(message.text)
        file_size = 0
        filename = ""
        content_type = ""
        if message.file is not None:
            file_size = message.file.content_length
            filename = message.file.filename;
            content_type = message.file.content_type
        query_size = text_size + file_size

        if text_size > Config.max_size_text or query_size > Config.max_size_one_query or message.id in self._blocks:
            return 0

        if message.file is not None:
            _, extension = os.path.splitext(filename)
            with open(os.path.join(self._file_path, str(message.id) + extension), 'wb') as stream:
                stream.write(message.file.read())

        add_second = Config.limit_other_big_message_second if query_size > Config.limit_other_small_size_one_query else Config.limit_other_small_message_second
        if (self.size_all_block_in_bytes <= Config.limit_1):
            add_second = Config.limit_1_big_message_second if query_size > Config.limit_1_small_size_one_query else Config.limit_1_small_message_second

        delete_unix_at = int(time.time()) + add_second
        self._block_data.append(Data(message.id, message.text, 0, filename, delete_unix_at, content_type, file_size))
        self._blocks[message.id] = Block(self.block_index, delete_unix_at)
        self.size_all_block_in_bytes += query_size
        self.block_in_bytes += query_size
        return delete_unix_at

    def get(self, id: int) -> Data:
        for data in self._block_data:
            if data.id == id:
                return data

        block = self._blocks.get(id)
        if block is not None:
            path = os.path.join(self._text_path, str(block.index))
            if os.path.exists(path):
                datas = []
                with open(path, 'r', encoding='utf-8') as f:
                    datas = [Data(**item) for item in json.load(f)]
                return next((x for x in datas if x.id == id), None)
        return None

    def load(self):
        for file_path in os.listdir(self._text_path):
            block_index = int(file_path)
            full_path = os.path.join(self._text_path, file_path)
            if os.path.isfile(full_path):
                datas = []
                try:
                    with open(full_path, 'r', encoding='utf-8') as f:
                        datas = [Data(**item) for item in json.load(f)]
                    for data in datas:
                        if not data == None:
                            self._blocks[data.Id] = Block(self.block_index, data.delete_unix_at);
                            self.size_all_block_in_bytes += len(data.text) + data.file_size_of_bytes
                    self.block_index = block_index if block_index > self.block_index else self.block_index
                except:
                    pass
        self.block_index += 1

    def commit(self):
        if not self._is_lock_by_commit:
            self._is_lock_by_commit = True
            try:
                path = os.path.join(self._text_path, str(self.block_index))
                self.block_index += 1;
                if not os.path.exists(path):
                    block_count = len(self._blocks)
                    json_string = json.dumps([data.to_dict() for data in self._block_data], ensure_ascii=True)
                    with open(path, 'w', encoding='utf-8') as f:
                        f.write(json_string);
                    self._block_data.clear()
                    self.block_in_bytes = 0
            except:
                pass
            self._is_lock_by_commit = False

    def shrink(self):
        if not self._is_lock_by_shrink:
            self._is_lock_by_shrink = True
            try:
                now_unix = int(time.time())
                indexes = set()
                for block in self._blocks.values():
                    if block.delete_unix_at < now_unix:
                        indexes.add(block.index)

                for index in indexes:
                    path = os.path.join(self._textPath, str(index))
                    if not os.path.exists(path): continue
                    datas = []
                    with open(path, 'r', encoding='utf-8') as f:
                        datas = [Data(**item) for item in json.load(f)]
                    delete_ids = []
                    size_reduced = 0
                    for i in range(datas.count):
                        data = datas[datas.count - 1 - i]
                        if data.delete_unix_at < now_unix:
                            _, extension = os.path.splitext(data.filename)
                            fullpath = os.path.join(self._file_path, str(data.id) + extension)
                            if os.path.exists(fullpath):
                                os.remove(fullpath)
                            size_reduced += len(data.text) + data.file_size_of_bytes
                            delete_ids.append(data.id)
                            datas.remove(data)

                    if datas.count > 0:
                        with open(path, 'w', encoding='utf-8') as f:
                            json.dump(datas, f, ensure_ascii=False, indent=0)
                    else:
                        os.remove(path)
                    for id in delete_ids:
                        self._blocks.pop(id, None)
                    self.size_all_block_in_bytes -= size_reduced
            except:
                pass
            self._is_lock_by_shrink = False
