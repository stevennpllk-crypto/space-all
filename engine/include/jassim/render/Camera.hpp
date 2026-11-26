#pragma once

#include "jassim/math/Mat4.hpp"

#include <algorithm>
#include <cmath>

namespace jassim::render {

class Camera {
public:
    void setPosition(const math::Vec3& position) noexcept { position_ = position; }
    const math::Vec3& position() const noexcept { return position_; }

    void setPerspective(float fovYRadians, float aspect,
                        float nearPlane, float farPlane) noexcept {
        fovY_ = fovYRadians;
        aspect_ = aspect;
        near_ = nearPlane;
        far_ = farPlane;
    }

    void rotate(float yawDeltaRadians, float pitchDeltaRadians) noexcept {
        yaw_ += yawDeltaRadians;
        pitch_ = std::clamp(pitch_ + pitchDeltaRadians, -1.553343F, 1.553343F);
    }

    math::Vec3 forward() const noexcept {
        const float cp = std::cos(pitch_);
        return math::Vec3{cp * std::cos(yaw_), std::sin(pitch_),
                          cp * std::sin(yaw_)}.normalized();
    }

    math::Vec3 right() const noexcept {
        return math::cross(forward(), worldUp_).normalized();
    }

    void moveLocal(float rightAmount, float upAmount, float forwardAmount) noexcept {
        position_ = position_ + right() * rightAmount
                              + worldUp_ * upAmount
                              + forward() * forwardAmount;
    }

    math::Mat4 view() const noexcept {
        return math::Mat4::lookAt(position_, position_ + forward(), worldUp_);
    }

    math::Mat4 projection() const noexcept {
        return math::Mat4::perspective(fovY_, aspect_, near_, far_);
    }

    math::Mat4 viewProjection() const noexcept { return projection() * view(); }

private:
    math::Vec3 position_{0.0F, 0.0F, 5.0F};
    math::Vec3 worldUp_{0.0F, 1.0F, 0.0F};
    float yaw_{-1.57079632679F};
    float pitch_{0.0F};
    float fovY_{1.0471975512F};
    float aspect_{16.0F / 9.0F};
    float near_{0.1F};
    float far_{10000.0F};
};

} // namespace jassim::render
