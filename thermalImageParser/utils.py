from sys import argv

def parse_argv(arg_flags: list = [], flags_shortened: dict = {}) -> tuple[list[str], list[str], dict]:
    params = []
    flags_not_found = []
    flags = {}
    for flag in arg_flags:
        flags[flag] = False
    for i in range(1, len(argv)):
        if argv[i].startswith('--'):
            if argv[i][2:] in flags.keys():
                flags[argv[i][2:]] = True
            else:
                flags_not_found.append(argv[i])
        elif argv[i].startswith('-'):
            if argv[i][1:] in flags_shortened.keys():
                flags[flags_shortened[argv[i][1:]]] = True
            else:
                flags_not_found.append(argv[i])
        else:
            params.append(argv[i])
    return (params, flags_not_found, flags)