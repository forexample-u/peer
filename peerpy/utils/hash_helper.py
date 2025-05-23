def hash_algorithm(input_val) -> int:
    hash_val = 5381
    for ch in input_val:
        hash_val = hash_val * 33 + ord(ch)
    return hash_val