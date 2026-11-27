def can_build(env, platform):
    # Needs no bundled SDK: scripts are compiled by a C++ compiler on the user's machine.
    return True


def configure(env):
    pass
