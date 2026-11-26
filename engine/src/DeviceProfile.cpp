#include "jassim/platform/DeviceProfile.hpp"

namespace jassim::platform {

DeviceProfile DeviceProfile::fromMetrics(const DeviceMetrics& metrics) noexcept {
    // GLES 3 is the baseline renderer target. Unknown metrics stay conservative.
    const bool lowMemory = metrics.totalRamMB == 0 || metrics.totalRamMB <= 3072;
    const bool lowCpu = metrics.cpuCores == 0 || metrics.cpuCores <= 4;
    const bool oldGraphics = metrics.glesMajorVersion < 3;

    if (lowMemory || lowCpu || oldGraphics) {
        return {QualityTier::Low, 0.65F, 30, 128, 256, false};
    }

    const bool highEnd = metrics.totalRamMB >= 6144 &&
                         metrics.cpuCores >= 8 &&
                         metrics.glesMajorVersion >= 3 &&
                         metrics.screenPixels >= 2073600ULL;
    if (highEnd) {
        return {QualityTier::High, 1.0F, 60, 512, 2048, true};
    }

    return {QualityTier::Medium, 0.80F, 45, 256, 512, false};
}

} // namespace jassim::platform
