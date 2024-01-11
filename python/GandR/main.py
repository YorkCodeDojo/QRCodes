#!/usr/bin/env python3

import imageio.v3 as iio
import numpy as np
import os


def load_qr_as_array(file):
    im = iio.imread(file)
    # ::8 to get every 8th pixel, from rows & columns
    # dot product with RGBA [1 0 0 0] to extract just red channel
    bw_image_scale_down_8 = np.dot(im[::8, ::8, ...], [1, 0, 0, 0])
    # compare with 0, black(0) == true, white(255) == false
    return bw_image_scale_down_8 == 0


def extract_info(arr):
    width = arr.shape[1]
    version = (width - 17) // 4
    format_mask = np.array([1, 0, 1, 0, 1], dtype=bool)
    format_info = np.logical_xor(arr[8, :5], format_mask)
    ecc_level = format_info[:2]
    mask_id = np.dot(format_info[2:], [4, 2, 1])
    return version, ecc_level, mask_id, width


mask_functions = {
    0: lambda r, c: (r + c) % 2 == 0,
    1: lambda r, c: (r % 2) == 0,
    2: lambda r, c: (c % 3) == 0,
    3: lambda r, c: (r + c) % 3 == 0,
    4: lambda r, c: (r // 2 + c // 3) % 2 == 0,
    5: lambda r, c: (r * c) % 2 + (r * c) % 3 == 0,
    6: lambda r, c: ((r * c) % 2 + (r * c) % 3) % 2 == 0,
    7: lambda r, c: ((r + c) % 2 + (r * c) % 3) % 2 == 0,
}

EXTRACTOR_ENCODING = np.array([[1, 2], [4, 8]])
EXTRACTOR_UP = np.array([[1, 2], [4, 8], [16, 32], [64, 128]])
EXTRACTOR_DOWN = np.array([[64, 128], [16, 32], [4, 8], [1, 2]])
EXTRACTOR_CW = np.array([[1, 2, 64, 128], [4, 8, 16, 32]])
EXTRACTOR_ACW = np.array([[4, 8, 16, 32], [1, 2, 64, 128]])


def extract(arr, extractor):
    return np.sum(arr * extractor)


def generate_mask_array(mask_id, size):
    return np.fromfunction(mask_functions[mask_id], (size, size))


def main():
    filename = os.path.join(os.path.abspath(__file__), "..", "..", "..", "dojo.png")
    arr = load_qr_as_array(filename)

    version, ecc_level, mask_id, width = extract_info(arr)
    print(f"{version=}, {mask_id=}")
    assert version == 1
    assert mask_id == 6

    mask_array = generate_mask_array(mask_id, width)
    data_array = np.logical_xor(arr, mask_array)
    encoding_info = extract(data_array[-2:, -2:], EXTRACTOR_ENCODING)

    length = extract(data_array[-6:-2, -2:], EXTRACTOR_UP)
    print(f"{length=}, {encoding_info=}")
    assert length == 12
    assert encoding_info == 4


    selector = {

    }

    current = EXTRACTOR_UP
    y = width - 10
    x = width - 2
    shape = (4, 2)
    for i in range(length):
        print(chr(extract(data_array[y:y + shape[0], x:x + shape[1]], current)), end='')
        if current is EXTRACTOR_UP:
            if (x >= width - 8) and y - 4 <= 9:
                current = EXTRACTOR_ACW
                y -= 2
                x -= 2
                shape = (2, 4)
            else:
                y -= 4
        elif current is EXTRACTOR_ACW:
            current = EXTRACTOR_DOWN
            shape = (4, 2)
            y += 2
        elif current is EXTRACTOR_DOWN:
            if (y + 6) >= width:
                current = EXTRACTOR_CW
                y += 4
                x -= 2
                shape = (2, 4)
            else:
                y += 4
        elif current is EXTRACTOR_CW:
            current = EXTRACTOR_UP
            y -= 4
            shape = (4, 2)


if __name__ == "__main__":
    main()
