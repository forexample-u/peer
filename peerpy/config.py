import os
import json

class Config:
    max_size_one_query = 3200000
    max_size_text = 1050000
    avg_size_block = 1050000
    limit_1 = 28000000
    limit_1_big_message_second = 60
    limit_1_small_message_second = 960
    limit_1_small_size_one_query = 120000
    limit_other_big_message_second = 7
    limit_other_small_message_second = 65
    limit_other_small_size_one_query = 5100

    @classmethod
    def load(cls, filename='config.json'):
        if os.path.exists(filename):
            try:
                with open(filename, 'r') as file:
                    data = json.load(file)
                config = data.get('PeerConfig', {})
                cls.max_size_one_query = config.get('MaxSizeOneQuery', cls.max_size_one_query)
                cls.max_size_text = config.get('MaxSizeText', cls.max_size_text)
                cls.avg_size_block = config.get('AvgSizeBlock', cls.avg_size_block)
                cls.limit_1 = config.get('Limit1', cls.limit_1)
                cls.limit_1_big_message_second = config.get('Limit1BigMessageSecond', cls.limit_1_big_message_second)
                cls.limit_1_small_message_second = config.get('Limit1SmallMessageSecond', cls.limit_1_small_message_second)
                cls.limit_1_small_size_one_query = config.get('Limit1SmallSizeOneQuery', cls.limit_1_small_size_one_query)
                cls.limit_other_big_message_second = config.get('LimitOtherBigMessageSecond', cls.limit_other_big_message_second)
                cls.limit_other_small_message_second = config.get('LimitOtherSmallMessageSecond', cls.limit_other_small_message_second)
                cls.limit_other_small_size_one_query = config.get('LimitOtherSmallSizeOneQuery', cls.limit_other_small_size_one_query)
                return True
            except Exception as e:
                pass
        return False