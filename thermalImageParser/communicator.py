import constants
import utils
from sys import stderr
from thermalImageParser import main as find_danger_percentage
from os.path import isfile
from requests import post as post_request

arg_flags = ['send', 'filename', 'save']
flags_shortened = {
    'S': 'send',
    'F': 'filename',
    'V': 'save'
}

def get_filepath_from_args(args: list) -> dict:
    ret = {
        'ok': True,
        'error msg': '',
        'filepath': None
    }
    if len(args) < 1:
        ret['ok'] = False
        ret['error msg'] = 'No parameter provided'
        return ret
    ret['filepath'] = args[0]
    verify_filepath(ret)
    return ret

def verify_filepath(parsing_dict: dict) -> None:
    if not isfile(parsing_dict['filepath']):
        parsing_dict['ok'] = False
        parsing_dict['error msg'] = 'File \"{}\" not found'.format(parsing_dict['filepath'])

def get_filepath_from_input() -> dict:
    ret = {
        'ok': True,
        'error msg': '',
        'filepath': None
    }
    print('Please enter filename to parse: ', end = '')
    ret['filepath'] = input()
    verify_filepath(ret)
    return ret

def save_output_as_file(output: float, filename: str) -> None:
    try:
        file = open(filename, 'w')
    except IOError:
        print('Error while trying to save file', file = stderr)
        return
    file.write(str(output))
    file.close()

def send_output_with_request(output: float, endpoint_url: str, api_key: str) -> None:
    data = {
        'data': output,
        'api key': api_key
    }
    post_request(url = endpoint_url, data = data) # Można ewentualnie sprawdzić czy poprawnie się wysłało

def main():
    args, errs, flags = utils.parse_argv(arg_flags, flags_shortened)
    for err in errs:
        print('Error:\n\tUnrecognized flag \"{}\"'.format(err), file = stderr)
    if len(errs) > 0:
        return 
    # Pobranie danych na dwa sposoby, zależy od potrzeb
    if flags['filename']:
        input_data = get_filepath_from_args(args)
    else:
        input_data = get_filepath_from_input()
    if not input_data['ok']:
        print('Error:\n\t{}'.format(input_data['error msg']), file = stderr)
        return
    output = find_danger_percentage(
        filename = input_data['filepath'],
        image_size = constants.IMAGE_SIZE,
        palette_bounds = constants.PALETTE_BOUNDS,
        danger_level = constants.DANGER_LEVEL,
        work_areas = constants.WORK_AREAS,
        show_image = constants.SHOW_IMAGES,
        print_result = False,
        save_image = constants.SAVE_IMAGES,
        rounding = constants.ROUNDING
    )
    # Wysłanie danych na dwa sposoby, zależnie od potrzeb
    if flags['send']:
        send_output_with_request(output, constants.ENDPOINT_URL, constants.API_KEY)
    if flags['save']:
        save_output_as_file(output, constants.OUPUT_SAVE_FILE)
    if not (flags['send'] or flags['save']):
        print(output)

if __name__ == '__main__':
    main()
