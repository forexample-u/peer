class Data:
    def __init__(self, id: int, text: str, filehash: int, filename: str, delete_unix_at: int, content_type: str, file_size_of_bytes: int):
        self.id = id
        self.text = text
        self.filehash = filehash
        self.filename = filename
        self.delete_unix_at = delete_unix_at
        self.content_type = content_type
        self.file_size_of_bytes = file_size_of_bytes

    def to_dict(self):
        return {
            'id': self.id,
            'text': self.text,
            'filehash': self.filehash,
            'filename': self.filename,
            'delete_unix_at': self.delete_unix_at,
            'content_type': self.content_type,
            'file_size_of_bytes': self.file_size_of_bytes
        }