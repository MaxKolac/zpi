from PIL import Image
from numpy import asarray
from sys import argv
from os.path import isfile

default_range = (20., 40.)
  
def main(args):
    parsed_args = parse_args(args)
    if not parsed_args['ok']:
        print('Error while parsing: {}'.format(parsed_args['error msg']))
        return
    if parsed_args['use default']:
        min_temp, max_temp = default_range
    else:
        min_temp = parsed_args['min temp']
        max_temp = parsed_args['max temp']
    data = find_hottest_pixel(parsed_args['filename'], min_temp, max_temp, parsed_args['radius'])
    print(data)

def parse_args(args):
    #Wymagane podanie ścieżki do obrazu, minimalna i maksymalna temperatura w skali
    #Użycie: program.py plik min_temp max_temp [promień] lub program.py plik -D [promień]
    ret = {
        'ok': True,
        'filename': None,
        'use default': False,
        'min temp': None,
        'max temp': None,
        'radius': 0,
        'error msg': ''
    }
    if len(args) < 2:
        ret['ok'] = False
        ret['error msg'] = 'Not enough parameters'
        return ret
    ret['filename'] = args[0]
    if args[1] == '-D':
        ret['use default'] = True
        if len(args) > 2:
            ret['radius'] = args[2]
    else:
        if len(args) < 3:
            ret['ok'] = False
            ret['error msg'] = 'Not enough parameters'
            return ret
        ret['min temp'] = args[1]
        ret['max temp'] = args[2]
        if len(args) > 3:
            ret['radius'] = args[3]
    #Walidacja parametrów
    if not isfile(ret['filename']):
        ret['ok'] = False
        ret['error msg'] = 'File \"{}\" not found'.format(ret['filename'])
        return ret
    if not ret['use default']:
        try:
            ret['min temp'] = float(ret['min temp'])
        except ValueError:
            ret['ok'] = False
            ret['error msg'] = 'Couldn\'t convert \"{}\" to a number'.format(ret['min temp'])
            return ret
        try:
            ret['max temp'] = float(ret['max temp'])
        except ValueError:
            ret['ok'] = False
            ret['error msg'] = 'Couldn\'t convert \"{}\" to a number'.format(ret['max temp'])
            return ret
    if ret['radius'] != 0:
        try:
            ret['radius'] = int(ret['radius'])
        except ValueError:
            ret['ok'] = False
            ret['error msg'] = 'Couldn\'t convert \"{}\" to an integer'.format(ret['radius'])
            return ret
    return ret
  
def find_hottest_pixel(image_fp, temp_min, temp_max, radius = 0):
    # Inicjalizacja zmiennych
    image = Image.open(image_fp)
    image_array = asarray(image.convert('L'))
    hottest = 0
    center_pixel = (0, 0)
    sum_of_heats = 0
    pixels_in_radius = 0
    output = {}
    # Znalezienie najcieplejszego / najjaśniejszego piksela
    for y in range(len(image_array)):
        for x in range(len(image_array[y])):
            pixel = image_array[y][x]
            if pixel > hottest:
                hottest = pixel
                center_pixel = (x, y)
    # Znalezienie sąsiednich pikseli w promieniu najcieplejszego piksela
    if radius != 0:
        for y in range(len(image_array)):
            for x in range(len(image_array[y])):
                if radius < 0 or (x - center_pixel[0])**2 + (y - center_pixel[1])**2 <= radius**2:
                    sum_of_heats += image_array[y][x]
                    pixels_in_radius += 1
        if pixels_in_radius > 0:
            output['radius temperature'] = (temp_max - temp_min) * (sum_of_heats / pixels_in_radius) / 255 + temp_min
    output['temperature'] = (temp_max - temp_min) * hottest / 255 + temp_min
    output['center pixel'] = center_pixel
    return output

if __name__ == '__main__':
    main(argv[1:])