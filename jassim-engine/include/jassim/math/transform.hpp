#pragma once
#include "jassim/math/vec3.hpp"
namespace jassim {
struct Transform {
 Vec3 position{};
 Vec3 rotationEulerRadians{};
 Vec3 scale{1.0F,1.0F,1.0F};
};
}
