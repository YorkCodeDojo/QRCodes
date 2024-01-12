from main import load_image, load_format, get_mask_version, get_encoding
import unittest


class TestStringMethods(unittest.TestCase):
    def test_pattern(self):
        first_row = load_image()[0]

        davids_data = map(lambda x: x == 'B', "BBBBBBBWBBBWBWBBBBBBB")

        self.assertListEqual(list(first_row), list(davids_data))

    def test_format(self):
        data = [True, True, True, True, True]
        self.assertEqual(0b01010, load_format(data))

    def test_version(self):
        self.assertEqual(2, get_mask_version([True, True, True, True, True]))

    def test_encoding_1111(self):
        self.assertEqual(0b1111, get_encoding([[True, True], [True, True]]))

    def test_encoding_1001(self):
        self.assertEqual(0b1001, get_encoding([[True, False], [False, True]]))

    def test_encoding_0110(self):
        self.assertEqual(0b0110, get_encoding([[False, True], [True, False]]))

    def test_encoding_0001(self):
        self.assertEqual(0b0001, get_encoding([[True, False], [False, False]]))

    def test_encoding_0010(self):
        self.assertEqual(0b0010, get_encoding([[False, True], [False, False]]))
