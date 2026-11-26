#pragma once

#include <array>
#include <cmath>

namespace jassim::render {

struct Material {
    std::array<float, 3> albedoTint{1.0F, 1.0F, 1.0F};
    float ambientStrength{0.18F};
    float diffuseStrength{0.82F};
    float uvScale{1.0F};

    bool valid() const noexcept {
        for (const float channel : albedoTint) {
            if (!std::isfinite(channel) || channel < 0.0F || channel > 1.0F) {
                return false;
            }
        }
        return std::isfinite(ambientStrength) && ambientStrength >= 0.0F &&
               ambientStrength <= 1.0F &&
               std::isfinite(diffuseStrength) && diffuseStrength >= 0.0F &&
               diffuseStrength <= 2.0F &&
               std::isfinite(uvScale) && uvScale > 0.0F && uvScale <= 64.0F;
    }
};

} // namespace jassim::render
