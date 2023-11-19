from sys import argv
from thermalImageParser import find_hottest_pixel
from os.path import isfile, realpath, dirname, join as path_join
from dotenv import dotenv_values
from json import dumps as to_json
from requests import post as post_request

env_file = path_join(dirname(realpath(__file__)), '.env')
required_keys = {
    'ENDPOINT_URL': str,
    'API_KEY': str,
    'TEMP_MIN': float,
    'TEMP_MAX': float,
    'RADIUS': int
}
allowed_NoneTypes = ['TEMP_MIN', 'TEMP_MAX']
thermal_images_extensions = ['tiff']
target_save_file = 'output.json'

def is_thermal_file(filepath: str):
    for ext in thermal_images_extensions:
        if filepath.endswith('.' + ext):
            return True
    return False

def get_filepath_from_args():
    ret = {
        'ok': True,
        'error msg': '',
        'filepath': None
    }
    if len(argv) < 2:
        ret['ok'] = False
        ret['error msg'] = 'No parameter provided'
        return ret
    ret['filepath'] = argv[1]
    verify_filepath(ret)
    return ret

def verify_filepath(parsing_dict: dict):
    if not isfile(parsing_dict['filepath']):
        parsing_dict['ok'] = False
        parsing_dict['error msg'] = 'File \"{}\" not found'.format(parsing_dict['filepath'])

def get_filepath_from_input():
    ret = {
        'ok': True,
        'error msg': '',
        'filepath': None
    }
    print('Please enter filename to parse: ', end = '')
    ret['filepath'] = input()
    verify_filepath(ret)
    return ret

def parse_dotenv(env_values: dict, req_keys: dict = required_keys):
    ret = {
        'ok': True,
        'error msg': ''
    }
    for key, type in req_keys.items():
        if key not in env_values.keys():
            ret['ok'] = False
            ret['error msg'] = 'Required key {} not found in environmental values'.format(key)
            return ret
        if env_values[key] == '' and key in allowed_NoneTypes:
            env_values[key] = None
        else:
            try:
                env_values[key] = type(env_values[key])
            except ValueError:
                ret['ok'] = False
                ret['error msg'] = 'Couldn\'t convert \"{}\" to type {}'.format(env_values[key], type)
                return ret
    return ret

def save_output_as_file(output: dict, filename: str):
    try:
        file = open(filename, 'w')
    except IOError:
        print('Error while trying to save file')
        return
    file.write(to_json(output))
    file.close()

def send_output_with_request(output: dict, endpoint_url: str, api_key: str):
    data = {
        'api_key': api_key,
        'data': output
    }
    post_request(url = endpoint_url, data = data) # Można ewentualnie sprawdzić czy poprawnie się wysłało

def main():
    # Pobranie danych na dwa sposoby, zależy od potrzeb
    input_data = get_filepath_from_args()
    # input_data = get_filepath_from_input()
    if not input_data['ok']:
        print('Error: {}'.format(input_data['error msg']))
        return
    env_values = dotenv_values(env_file)
    check = parse_dotenv(env_values)
    if not check['ok']:
        print('Environmental values error: {}'.format(check['error msg']))
        return
    output = find_hottest_pixel(
        image_fp = input_data['filepath'],
        thermal_file = is_thermal_file(input_data['filepath']),
        temp_min = env_values['TEMP_MIN'],
        temp_max = env_values['TEMP_MAX'],
        radius = env_values['RADIUS']
    )
    # Wysłanie danych na dwa sposoby, zależnie od potrzeb
    save_output_as_file(output, target_save_file)
    # send_output_with_request(output, env_values['ENDPOINT_URL'], env_values['API_KEY'])

if __name__ == '__main__':
    main()
