#!/usr/bin/env python3

import imageio.v3 as iio
import numpy as np


def load_qr_as_ndarray(file: str) -> np.ndarray:
    im = iio.imread(file)
    bw_image = np.dot(im[..., :3], [1,0,0])
    scale_down_8 = bw_image[::8, ::8]
    return scale_down_8 == 0
    #return (((scale_down_8 // 255) * -1) + 1) != 0

def extact_info(arr: np.ndarray):
    width = arr.shape[1] 
    version = (width - 17) // 4
    format_mask = np.array([1,0,1,0,1], dtype=bool)
    format_info = np.logical_xor(arr[8, :5], format_mask)
    ecc_level = format_info[:2]
    mask_id_arr = format_info[2:]
    mask_id = 4*mask_id_arr[0] + 2*mask_id_arr[1] + 1*mask_id_arr[2]
    return version, ecc_level, mask_id, width

mask_functions = {
        0: lambda r, c : (r + c) %2 ==0,
        1: lambda r, c : (r % 2) ==0,
        2: lambda r, c : (c % 3) ==0,
        3: lambda r, c : (r+c)%3 ==0,
        4: lambda r, c : (r//2+c//3)%2==0,
        5: lambda r, c : (r*c)%2+(r*c)%3==0,
        6: lambda r, c : ((r*c)%2+(r*c)%3)%2==0,
        7: lambda r, c : ((r+c)%2+(r*c)%3)%2==0,
        }

read_functions = {
        'up': ([0, 0, -1, -1, -2, -2, -3, -3], [0, -1, 0, -1, 0, -1, 0, -1], lambda r, c : (r - 4, c)),
        'down':([-3, -3, -2, -2, -1, -1, 0, 0], [0, -1, 0, -1, 0, -1, 0, -1], lambda r, c: (r + 4, c)),
        'clockwise':([-1, -1, 0, 0, 0, 0, -1, -1], [0, -1, 0, -1, -2, -3, -2, 3], lambda r, c:(r, c)),
        'anticlockwise': ([0, 0, -1, -1, -1, -1, 0, 0], [0, -1, 0, -1, -2, -3, -2, -3], lambda r, c: (r, c))
        }

def generate_mask_array(mask_f, size):
    return np.fromfunction(mask_f, (size, size))

def get_encoding(data_array, r, c):
    return  8*data_array[r, c] + 4*data_array[r, c-1] + 2*data_array[r-1, c] + data_array[r-1, c-1]

def read_byte(data_array, read_dir, start_r, start_c):
    r_f = read_functions['up']
    acc = 0
    for r, c in zip(r_f[0], r_f[1]):
        acc *= 2
        acc += data_array[r + start_r, c + start_c]

    return chr(acc)

def main():
    arr = load_qr_as_ndarray("dojo.png")
    version, ecc_level, mask_id, width = extact_info(arr)
    print(ecc_level, mask_id)
    mask_f = mask_functions[mask_id]
    
    mask_array = generate_mask_array(mask_f, width)
    data_array = np.logical_xor(arr, mask_array)
    encoding_info = get_encoding(data_array, -1, -1)

    len_1 = get_encoding(data_array, -3, -1)
    len_2 = get_encoding(data_array, -5, -1)

    print(encoding_info)

    print(len_1 * 16 + len_2)

    print(read_byte(data_array, 'up', -7, -1))
    print(read_byte(data_array, 'anticlockwise', -11, -1))

if __name__ == "__main__":
    main()
