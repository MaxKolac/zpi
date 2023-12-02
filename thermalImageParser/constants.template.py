"""
    Copy this file and rename the copy to constants.py

    fill out all necessary informations before starting app
"""

ENDPOINT_URL = None #Your endpoint url
API_KEY = None #Api key for application to verify itself
IMAGE_SIZE = (640, 480) #Size of an image
PALETTE_BOUNDS = ((620, 30), (635, 424)) #Bounds for where palette is located
DANGER_LEVEL = 0.5 #What level of a palette should be achieved for a color to be considered dangerous
DANGER_COLOR = (255, 0, 0) #What RGB should be used to color hot pixels. Probably will not be used
WORK_AREAS = [
    ((2, 58), (585, 456))
] #Where to look for hot pixels. Work areas should avoid stuff like watermark or palette
OUPUT_SAVE_FILE = 'output'
ROUNDING = -1 #What decimal place to round the result to. Negative number means no rounding at all
SHOW_IMAGES = False #If set to true, program will show where danger pixels are. For testing purposes only
SAVE_IMAGES = None #If given path instead of None, it will create or overwrite an image as above, instead of showing it. For testing purposes only