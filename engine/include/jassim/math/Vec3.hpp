#pragma once

#include <cmath>

namespace jassim::math {

struct Vec3 {
    float x{0.0F};
    float y{0.0F};
    float z{0.0F};

    constexpr Vec3() = default;
    constexpr Vec3(float xValue, float yValue, float zValue)
        : x(xValue), y(yValue), z(zValue) {}

    constexpr Vec3 operator+(const Vec3& rhs) const {
        return {x + rhs.x, y + rhs.y, z + rhs.z};
    }

    constexpr Vec3 operator-(const Vec3& rhs) const {
        return {x - rhs.x, y - rhs.y, z - rhs.z};
    }

    constexpr Vec3 operator*(float scalar) const {
        return {x * scalar, y * scalar, z * scalar};
    }

    constexpr float lengthSquared() const {
        return x * x + y * y + z * z;
    }

    float length() const { return std::sqrt(lengthSquared()); }

    Vec3 normalized() const {
        const float magnitude = length();
        if (magnitude <= 1.0e-6F) return {};
        return {x / magnitude, y / magnitude, z / magnitude};
    }

    static constexpr float dot(const Vec3& a, const Vec3& b) noexcept {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    static constexpr Vec3 cross(const Vec3& a, const Vec3& b) noexcept {
        return {a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x};
    }
};

inline constexpr float dot(const Vec3& a, const Vec3& b) noexcept {
    return Vec3::dot(a, b);
}

inline constexpr Vec3 cross(const Vec3& a, const Vec3& b) noexcept {
    return Vec3::cross(a, b);
}

} // namespace jassim::math
