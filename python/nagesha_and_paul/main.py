import cv2
import numpy as np

format_mask = 0b10101


def load_image():
    img = cv2.imread('img.png', cv2.IMREAD_GRAYSCALE)
    width = int(len(img[0]) / 8)
    output = np.ndarray(shape=(width, width), dtype=bool)
    for i in range(0, width):
        for j in range(0, width):
            output[i][j] = img[i * 8][j * 8] == 0
    return output


def load_format(row):
    format_array = [1 if x else 0 for x in row[0:5]]

    return array_to_int(format_array) ^ format_mask


def get_mask_version(row):
    mask_value = row[2:5]
    format_mask_bool = [True, False, True]

    output = [mask_value[i] ^ format_mask_bool[i] for i in range(0, len(format_mask_bool))]

    return array_to_int(output)


def array_to_int(arr):
    i = 0
    for bit in arr:
        i = (i << 1) | bit

    return i


mask_calcs = {
    0: lambda x, y: (x + y) % 2 == 0,
    1: lambda x, y: y % 2 == 0,
    2: lambda x, y: x % 3 == 0,
    3: lambda x, y: (x + y) % 3 == 0,
    4: lambda x, y: (x // 3 + y // 2) % 2 == 0,
    5: lambda x, y: (x * y) % 2 + (x * y) % 3 == 0,
    6: lambda x, y: ((x * y) % 2 + (x * y) % 3) % 2 == 0,
    7: lambda x, y: ((x * y) % 3 + (x + y) % 2) % 2 == 0
}


def apply_mask(x: int, y: int, mask, cell: bool):
    return not cell if mask(x, y) else cell


def get_encoding(matrix):
    order = [j for i in reversed(matrix) for j in reversed(i)]

    return array_to_int(order)


def decode_horizontal_top_down(matrix):
    order = [
        matrix[1][3],
        matrix[1][2],
        matrix[0][3],
        matrix[0][2],
        matrix[0][1],
        matrix[0][0],
        matrix[1][1],
        matrix[1][0],
    ]

    var = array_to_int(order)
    return chr(var)


def decode_horizontal_top_up(matrix):
    return decode_horizontal_top_down(list(reversed(matrix)))


def decode_vertical_up(matrix):
    return chr(get_encoding(matrix))


def decode_vertical_down(matrix):
    return chr(get_encoding(list(reversed(matrix))))


if __name__ == '__main__':
    bwMatrix = load_image()
    version = (len(bwMatrix) - 17) / 4
    format = load_format(bwMatrix[8])
    mask_version = get_mask_version(bwMatrix[8])
    mask_calc = mask_calcs[mask_version]
    masked_matrix = np.array([
        [apply_mask(i, j, mask_calc, bool(bwMatrix[i][j]))
         for j in range(0, len(bwMatrix))]
        for i in range(0, len(bwMatrix))])

    last_bits = masked_matrix[-2:, -2:]
    length_bits = masked_matrix[-6:-2:, -2:]

    encoding = get_encoding(last_bits)
    length = get_encoding(length_bits)

    print("\n\n")
    print(decode_vertical_up(masked_matrix[-10:-6, -2:])
          + (decode_horizontal_top_down(masked_matrix[-12:-10, -4:]))
          + (decode_vertical_down(masked_matrix[-10:-6, -4:-2]))
          + (decode_vertical_down(masked_matrix[-6:-2, -4:-2]))
          + (decode_horizontal_top_up(masked_matrix[-2:, -6:-2]))
          + (decode_vertical_up(masked_matrix[-6:-2, -6:-4]))
          + (decode_vertical_up(masked_matrix[-10:-6, -6:-4]))
          + (decode_horizontal_top_down(masked_matrix[-12:-10, -8:-4]))  #
          + (decode_vertical_down(masked_matrix[-10:-6, -8:-6]))
          + (decode_vertical_down(masked_matrix[-6:-2, -8:-6]))
          + (decode_horizontal_top_up(masked_matrix[-2:, -10:-6]))
          + (decode_vertical_up(masked_matrix[-6:-2, -10:-8])))
    print("\n\n")
