#pragma once

#include "jassim/math/Vec3.hpp"

#include <array>
#include <cmath>

namespace jassim::math {

// Column-major 4x4 matrix, compatible with OpenGL uniform uploads.
struct Mat4 {
    std::array<float, 16> values{};

    constexpr float& at(std::size_t row, std::size_t column) noexcept {
        return values[column * 4 + row];
    }

    constexpr float at(std::size_t row, std::size_t column) const noexcept {
        return values[column * 4 + row];
    }

    static constexpr Mat4 identity() noexcept {
        Mat4 result{};
        result.at(0, 0) = 1.0F;
        result.at(1, 1) = 1.0F;
        result.at(2, 2) = 1.0F;
        result.at(3, 3) = 1.0F;
        return result;
    }

    static Mat4 perspective(float verticalFovRadians, float aspect,
                            float nearPlane, float farPlane) noexcept {
        Mat4 result{};
        if (!(verticalFovRadians > 0.0F && verticalFovRadians < 3.14159265F) ||
            !(aspect > 0.0F) || !(nearPlane > 0.0F) || !(farPlane > nearPlane)) {
            return result;
        }

        const float f = 1.0F / std::tan(verticalFovRadians * 0.5F);
        result.at(0, 0) = f / aspect;
        result.at(1, 1) = f;
        result.at(2, 2) = (farPlane + nearPlane) / (nearPlane - farPlane);
        result.at(2, 3) = (2.0F * farPlane * nearPlane) / (nearPlane - farPlane);
        result.at(3, 2) = -1.0F;
        return result;
    }

    static Mat4 lookAt(const Vec3& eye, const Vec3& target, const Vec3& up) noexcept {
        const Vec3 forward = (target - eye).normalized();
        const Vec3 side = cross(forward, up).normalized();
        if (forward.lengthSquared() == 0.0F || side.lengthSquared() == 0.0F) {
            return identity();
        }
        const Vec3 correctedUp = cross(side, forward);

        Mat4 result = identity();
        result.at(0, 0) = side.x;
        result.at(0, 1) = side.y;
        result.at(0, 2) = side.z;
        result.at(0, 3) = -dot(side, eye);
        result.at(1, 0) = correctedUp.x;
        result.at(1, 1) = correctedUp.y;
        result.at(1, 2) = correctedUp.z;
        result.at(1, 3) = -dot(correctedUp, eye);
        result.at(2, 0) = -forward.x;
        result.at(2, 1) = -forward.y;
        result.at(2, 2) = -forward.z;
        result.at(2, 3) = dot(forward, eye);
        return result;
    }

    Mat4 operator*(const Mat4& rhs) const noexcept {
        Mat4 result{};
        for (std::size_t column = 0; column < 4; ++column) {
            for (std::size_t row = 0; row < 4; ++row) {
                for (std::size_t k = 0; k < 4; ++k) {
                    result.at(row, column) += at(row, k) * rhs.at(k, column);
                }
            }
        }
        return result;
    }
};

} // namespace jassim::math
