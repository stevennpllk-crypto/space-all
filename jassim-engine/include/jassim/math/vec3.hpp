#pragma once
#include <cmath>
namespace jassim {
struct Vec3 {
 float x{0.0F}, y{0.0F}, z{0.0F};
 constexpr Vec3 operator+(const Vec3& r) const noexcept { return {x+r.x,y+r.y,z+r.z}; }
 constexpr Vec3 operator-(const Vec3& r) const noexcept { return {x-r.x,y-r.y,z-r.z}; }
 constexpr Vec3 operator*(float s) const noexcept { return {x*s,y*s,z*s}; }
 constexpr float dot(const Vec3& r) const noexcept { return x*r.x+y*r.y+z*r.z; }
 constexpr Vec3 cross(const Vec3& r) const noexcept { return {y*r.z-z*r.y,z*r.x-x*r.z,x*r.y-y*r.x}; }
 float length() const noexcept { return std::sqrt(dot(*this)); }
 Vec3 normalized() const noexcept { const float n=length(); return n>1.0e-6F ? *this*(1.0F/n) : Vec3{}; }
};
}
