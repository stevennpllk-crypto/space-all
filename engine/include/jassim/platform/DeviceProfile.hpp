#pragma once

#include <cstdint>

namespace jassim::platform {

enum class QualityTier {
    Low,
    Medium,
    High
};

struct DeviceMetrics {
    std::uint32_t totalRamMB{0};
    std::uint32_t cpuCores{0};
    std::uint32_t glesMajorVersion{0};
    std::uint64_t screenPixels{0};
};

struct DeviceProfile {
    QualityTier tier{QualityTier::Low};
    float renderScale{0.65F};
    std::uint32_t targetFrameRate{30};
    std::uint32_t textureBudgetMB{128};
    std::uint32_t particleBudget{256};
    bool dynamicShadows{false};

    static DeviceProfile fromMetrics(const DeviceMetrics& metrics) noexcept;
};

} // namespace jassim::platform
