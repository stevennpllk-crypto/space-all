#pragma once

#include "jassim/math/Vec3.hpp"

#include <cmath>

namespace jassim::world {

struct OrbitParameters {
    float radius{0.0F};
    float angularSpeedRadiansPerSecond{0.0F};
    float phaseRadians{0.0F};

    bool valid() const noexcept {
        return std::isfinite(radius) && radius >= 0.0F &&
               std::isfinite(angularSpeedRadiansPerSecond) &&
               std::isfinite(phaseRadians);
    }
};

inline math::Vec3 orbitalPosition(const OrbitParameters& orbit, double elapsedSeconds) noexcept {
    if (!orbit.valid() || !std::isfinite(elapsedSeconds)) return {};
    constexpr double twoPi = 6.28318530717958647692;
    const double angle = std::remainder(
        static_cast<double>(orbit.phaseRadians) +
        static_cast<double>(orbit.angularSpeedRadiansPerSecond) * elapsedSeconds,
        twoPi);
    return {orbit.radius * static_cast<float>(std::cos(angle)),
            0.0F,
            orbit.radius * static_cast<float>(std::sin(angle))};
}

} // namespace jassim::world
