import os
import json

class Config:
    max_size_one_query = 0
    max_size_text = 0
    limit_1 = 0
    limit_1_big_message_second = 0
    limit_1_small_message_second = 0
    limit_1_small_size_one_query = 0
    limit_other_big_message_second = 0
    limit_other_small_message_second = 0
    limit_other_small_size_one_query = 0
    text_path = ""
    file_path = ""
    web_url_file_path = ""
    commit_block_second = 0
    avg_size_block = 0

    @classmethod
    def load(cls, filename='config.json'):
        with open(filename, 'r') as file:
            data = json.load(file)
        config = data.get('PeerMain', {})
        cls.max_size_one_query = config.get('MaxSizeOneQuery', cls.max_size_one_query)
        cls.max_size_text = config.get('MaxSizeText', cls.max_size_text)
        cls.limit_1 = config.get('Limit1', cls.limit_1)
        cls.limit_1_big_message_second = config.get('Limit1BigMessageSecond', cls.limit_1_big_message_second)
        cls.limit_1_small_message_second = config.get('Limit1SmallMessageSecond', cls.limit_1_small_message_second)
        cls.limit_1_small_size_one_query = config.get('Limit1SmallSizeOneQuery', cls.limit_1_small_size_one_query)
        cls.limit_other_big_message_second = config.get('LimitOtherBigMessageSecond', cls.limit_other_big_message_second)
        cls.limit_other_small_message_second = config.get('LimitOtherSmallMessageSecond', cls.limit_other_small_message_second)
        cls.limit_other_small_size_one_query = config.get('LimitOtherSmallSizeOneQuery', cls.limit_other_small_size_one_query)
        config = data.get('PeerAdditional', {})
        cls.text_path = config.get('TextPath', cls.text_path)
        cls.file_path = config.get('FilePath', cls.file_path)
        cls.web_url_file_path = config.get('WebUrlFilePath', cls.web_url_file_path)
        cls.commit_block_second = config.get('CommitBlockSecond', cls.commit_block_second)
        cls.avg_size_block = config.get('AvgSizeBlock', cls.avg_size_block)