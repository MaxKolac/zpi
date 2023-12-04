from constants import WORK_AREAS as work_areas
from PIL import Image
from numpy import uint8
import utils
from os.path import isfile

blue = (0, 0, 255)

def main() -> None:
    params, _, _ = utils.parse_argv()
    if len(params) < 1:
        print('Error:\n\tNo filename specified')
        return
    filename = params[0]
    if not isfile(filename):
        print('Error:\n\tfile \"{}\" not found'.format(filename))
        return
    image = uint8(Image.open(filename).convert('RGB')).copy()
    for area in work_areas:
        for x in range(area[0][0], area[1][0]):
            image[area[0][1]][x] = blue
            image[area[1][1]][x] = blue
        for y in range(area[0][1] + 1, area[1][1]):
            image[y][area[0][0]] = blue
            image[y][area[1][0]] = blue
            for x in range(area[0][0] + 1, area[1][0]):
                r, g, _ = image[y][x]
                image[y][x] = (r, g, 255)
    image = Image.fromarray(image, 'RGB')
    image.show()

if __name__ == '__main__':
    main()