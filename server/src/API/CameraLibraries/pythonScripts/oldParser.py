from PIL import Image
from numpy import asarray, uint16
from sys import argv
from os.path import isfile
from cv2 import imread as image_read, IMREAD_ANYDEPTH

default_range = (20., 40.)
  
def main(args: list):
    parsed_args = parse_args(args)
    if not parsed_args['ok']:
        print('Error while parsing: {}'.format(parsed_args['error msg']))
        return
    if parsed_args['use default']:
        min_temp, max_temp = default_range
    else:
        min_temp = parsed_args['min temp']
        max_temp = parsed_args['max temp']
    data = find_hottest_pixel(
        image_fp=parsed_args['filename'], 
        thermal_file=parsed_args['is thermal file'], 
        temp_min=min_temp,
        temp_max=max_temp,
        radius=parsed_args['radius']
    )
    print(data)

def parse_args(args : list):
    #Wymagane podanie ścieżki do obrazu, minimalna i maksymalna temperatura w skali
    #Użycie: program.py plik min_temp max_temp [promień] lub program.py plik -D [promień] lub program.py plik -T [promień]
    #przełącznik -D - domyślny przedział temperatur
    #przełącznik -T - plik termiczny
    ret = {
        'ok': True,
        'filename': None,
        'use default': False,
        'min temp': None,
        'max temp': None,
        'radius': 0,
        'error msg': '',
        'is thermal file': False
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
    elif args[1] == '-T':
        ret['is thermal file'] = True
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
    if not ret['use default'] and not ret['is thermal file']:
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

def get_temperature_from_value(value: int | float, thermal_file = True, temp_min = 0, temp_max = 0):
    if thermal_file:
        return value/100 - 273.15 #Możliwe że trzeba poprawić wzór - zależnie od kamery
    return (temp_max - temp_min) * value / 255 + temp_min
  
def find_hottest_pixel(image_fp: str, thermal_file = True, temp_min = 0, temp_max = 0, radius = 0):
    # Inicjalizacja zmiennych
    if thermal_file:
        image_array = uint16(image_read(image_fp, IMREAD_ANYDEPTH))
    else:
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
    output['temperature'] = get_temperature_from_value(hottest, thermal_file, temp_min, temp_max)
    output['center pixel'] = center_pixel
    output['radius temperature'] = output['temperature']
    # Znalezienie sąsiednich pikseli w promieniu najcieplejszego piksela
    if radius != 0:
        for y in range(len(image_array)):
            for x in range(len(image_array[y])):
                if radius < 0 or (x - center_pixel[0])**2 + (y - center_pixel[1])**2 <= radius**2:
                    sum_of_heats += image_array[y][x]
                    pixels_in_radius += 1
        if pixels_in_radius > 0:
            output['radius temperature'] = get_temperature_from_value(sum_of_heats / pixels_in_radius, thermal_file, temp_min, temp_max)
    return output

if __name__ == '__main__':
    main(argv[1:])
