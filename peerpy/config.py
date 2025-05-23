import os
import json

class Config:
    max_one_file_size = 3145728
    min_debuf_size_bytes = 28000000
    max_length_text = 1048576
    debuf_second = 53
    big_message_second = 60
    small_message_second = 960
    first_100_small_message_second = 86400
    small_text_length = 5000
    small_file_size_bytes = 10240

    @classmethod
    def load(cls, filename='config.json'):
        if os.path.exists(filename):
            try:
                with open(filename, 'r') as file:
                    data = json.load(file)
                config = data.get('PeerConfig', {})
                cls.max_one_file_size = config.get('MaxOneFileSize', cls.max_one_file_size)
                cls.min_debuf_size_bytes = config.get('MinDebufSizeBytes', cls.min_debuf_size_bytes)
                cls.max_length_text = config.get('MaxLengthText', cls.max_length_text)
                cls.debuf_second = config.get('DebufSecond', cls.debuf_second)
                cls.big_message_second = config.get('BigMessageSecond', cls.big_message_second)
                cls.small_message_second = config.get('SmallMessageSecond', cls.small_message_second)
                cls.first_100_small_message_second = config.get('First100SmallMessageSecond', cls.first_100_small_message_second)
                cls.small_text_length = config.get('SmallTextLength', cls.small_text_length)
                cls.small_file_size_bytes = config.get('SmallFileSizeBytes', cls.small_file_size_bytes)
                return True
            except Exception as e:
                pass
        return False