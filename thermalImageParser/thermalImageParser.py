from PIL import Image
from numpy import uint8
from os.path import isfile as file_exists

_default_image_size = (640, 480)
_default_palette_bounds = ((620, 30), (635, 424))
_default_danger_level = 0.7 # 70%
_default_danger_color = (255, 0, 0)
_default_work_areas = [((2, 58), (585, 456))]
_default_rounding = 2

def get_image(filename: str, size: tuple[int, int]) -> uint8:
    image = Image.open(filename).convert('RGB')
    image = image.resize(size)
    original_image_arr = uint8(image)
    image = image.convert('L')
    image_arr = uint8(image)
    return original_image_arr, image_arr

def get_rounded_mean(data: list) -> int:
    return round(sum(data)/len(data))

def get_working_area(x: int, y: int, work_areas: list[tuple[tuple[int, int], tuple[int, int]]]) -> int:
    for i in range(len(work_areas)):
        if x >= work_areas[i][0][0] - 1 and x < work_areas[i][1][0] and y >= work_areas[i][0][1] - 1 and y < work_areas[i][1][1]:
            return i
    return -1

def get_start_palette(image_arr: uint8, palette_bounds: tuple[tuple[int, int], tuple[int, int]], danger_level: float) -> tuple[int, int]:
    danger_start = int((palette_bounds[1][1] - palette_bounds[0][1]) * (1 - danger_level))
    start_pixels = image_arr[danger_start][palette_bounds[0][0] - 1:palette_bounds[1][0]]
    return get_rounded_mean(start_pixels)

def count_danger_pixels(image_arr: uint8, palette_start: int, work_areas: list[tuple[tuple[int, int], tuple[int, int]]], rounding : int) -> float:
    total_working_area = 0
    total_dangerous_pixels = 0
    for area in work_areas:
        total_working_area += (area[1][0] - area[0][0] + 1) * (area[1][1] - area[0][1] + 1)
        for y in range(area[0][1] - 1, area[1][1]):
            for x in range(area[0][0] - 1, area[1][0]):
                if image_arr[y][x] >= palette_start:
                    total_dangerous_pixels += 1
    total_dangerous_pixels *= 100
    if rounding < 0:
        return total_dangerous_pixels / total_working_area
    return round(total_dangerous_pixels / total_working_area, rounding)

def paint_danger_area(original_arr: uint8, image_arr: uint8, palette_start: int, work_areas: list[tuple[tuple[int, int], tuple[int, int]]], danger_color: tuple[int, int, int], image_size: tuple[int, int]) -> uint8:
    new_arr = original_arr.copy()
    area_arrs = []
    for area in work_areas:
        area_arr = []
        for y in range(area[0][1] - 1, area[1][1]):
            line = []
            for x in range(area[0][0] - 1, area[1][0]):
                line.append(danger_color if image_arr[y][x] >= palette_start else original_arr[y][x])
            area_arr.append(line)
        area_arrs.append(area_arr)
    for y in range(image_size[1]):
        for x in range(image_size[0]):
            area_id = get_working_area(x, y, work_areas)
            if area_id < 0:
                continue
            new_arr[y][x] = area_arrs[area_id][y - work_areas[area_id][0][1] + 1][x - work_areas[area_id][0][0] + 1]
    return new_arr

def main(
        filename: str,
        image_size: tuple[int, int] = _default_image_size,
        palette_bounds: tuple[tuple[int, int], tuple[int, int]] = _default_palette_bounds,
        danger_level: float = _default_danger_level,
        danger_color: tuple[int, int, int] = _default_danger_color,
        work_areas: list[tuple[tuple[int, int], tuple[int, int]]] = _default_work_areas,
        show_image: bool = True,
        print_result: bool = True,
        save_image: str | None = None,
        rounding : int = _default_rounding
) -> float:
    original_arr, image_arr = get_image(filename, image_size)
    palette_start= get_start_palette(image_arr, palette_bounds, danger_level)
    if show_image or save_image != None:
        new_image = Image.fromarray(paint_danger_area(original_arr, image_arr, palette_start, work_areas, danger_color, image_size), mode='RGB')
        if show_image:
            new_image.show()
        if save_image != None:
            new_image.saves(save_image)
    percentage = count_danger_pixels(image_arr, palette_start, work_areas, rounding)
    if print_result:
        print('{}%'.format(percentage))
    return percentage

if __name__ == '__main__':
    print('Please input filename: ', end='')
    filename = input()
    if file_exists(filename):
        main(filename)
    else:
        print('File \"{}\" does not exist'.format(filename))