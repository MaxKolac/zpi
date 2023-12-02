from PIL import Image
from numpy import uint8
from os.path import isfile as file_exists

_default_image_size = (640, 480)
_default_palette_bounds = ((620, 30), (635, 424))
_default_min_temp = -10.
_default_max_temp = 60.
_default_danger_temperature = 40.
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

def get_start_palette(scale_pixel_range: tuple[int, int], temperature_range: tuple[float, float], danger_temperature: float) -> int:
    danger_start = (scale_pixel_range[1] - scale_pixel_range[0]) * (danger_temperature - temperature_range[0]) / (temperature_range[1] - temperature_range[0]) + scale_pixel_range[0]
    return danger_start

def count_danger_pixels(image_arr: uint8, palette_start: int, scale_pixel_range: tuple[int, int], temperature_range: tuple[int, int], work_areas: list[tuple[tuple[int, int], tuple[int, int]]], rounding : int) -> tuple[float, float]:
    total_working_area = 0
    total_dangerous_pixels = 0
    hottest_pixel = 0
    for area in work_areas:
        total_working_area += (area[1][0] - area[0][0] + 1) * (area[1][1] - area[0][1] + 1)
        for y in range(area[0][1] - 1, area[1][1]):
            for x in range(area[0][0] - 1, area[1][0]):
                if image_arr[y][x] >= palette_start:
                    total_dangerous_pixels += 1
                if hottest_pixel < image_arr[y][x]:
                    hottest_pixel = image_arr[y][x]
    hottest_temp = (temperature_range[1] - temperature_range[0]) * (hottest_pixel - scale_pixel_range[0]) / (scale_pixel_range[1] - scale_pixel_range[0]) + temperature_range[0]
    total_dangerous_pixels *= 100
    if rounding < 0:
        return total_dangerous_pixels / total_working_area, hottest_temp
    return round(total_dangerous_pixels / total_working_area, rounding), round(hottest_temp, rounding)

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
        temp_min: float = _default_min_temp,
        temp_max: float = _default_max_temp,
        danger_temp: float = _default_danger_temperature,
        danger_color: tuple[int, int, int] = _default_danger_color,
        work_areas: list[tuple[tuple[int, int], tuple[int, int]]] = _default_work_areas,
        show_image: bool = True,
        print_result: bool = True,
        save_image: str | None = None,
        rounding : int = _default_rounding
) -> dict:
    original_arr, image_arr = get_image(filename, image_size)
    scale_pixel_range = (
        get_rounded_mean(image_arr[palette_bounds[1][1]][palette_bounds[0][0]:palette_bounds[1][0] + 1]),
        get_rounded_mean(image_arr[palette_bounds[0][1]][palette_bounds[0][0]:palette_bounds[1][0] + 1])
    )
    palette_start = get_start_palette(scale_pixel_range, (temp_min, temp_max), danger_temp)
    if show_image or save_image != None:
        new_image = Image.fromarray(paint_danger_area(original_arr, image_arr, palette_start, work_areas, danger_color, image_size), mode='RGB')
        if show_image:
            new_image.show()
        if save_image != None:
            new_image.saves(save_image)
    percentage, hottest_temp = count_danger_pixels(image_arr, palette_start, scale_pixel_range, (temp_min, temp_max), work_areas, rounding)
    if print_result:
        print('Hottest temperature: {} C\nPercentage: {}%'.format(hottest_temp, percentage))
    output = {
        'hottest temperature': hottest_temp,
        'percentage': percentage
    }
    return output

if __name__ == '__main__':
    print('Please input filename: ', end='')
    filename = input()
    if file_exists(filename):
        main(filename)
    else:
        print('File \"{}\" does not exist'.format(filename))