name = "physx_physics_3d"
description = "NVIDIA PhysX 3D physics engine backend for KaizenEngine"
author = "Hardion Labs"
version = "1.0.0"
url = "https://github.com/CeoMatheusDev/KaizenEngine"
license = "AGPL-3.0"

dependencies = ["kaizen"]

def can_build(env, platform):
    if env["disable_physics_3d"]:
        return False
    if not env["builtin_physx"]:
        return False
    return True
