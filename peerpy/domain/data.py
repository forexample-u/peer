class Data:
    def __init__(self, text: str, filehash: int, filename: str, delete_unix_at: int, content_type: str, file_size_of_bytes: int):
        self.text = text
        self.filehash = filehash
        self.filename = filename
        self.delete_unix_at = delete_unix_at
        self.file_size_of_bytes = file_size_of_bytes
        self.content_type = content_type