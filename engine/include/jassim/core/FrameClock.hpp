#pragma once

#include <algorithm>
#include <cmath>
#include <cstdint>

namespace jassim::core {

struct FrameTiming {
    double deltaSeconds{0.0};
    double fixedDeltaSeconds{1.0 / 60.0};
    std::uint32_t fixedSteps{0};
    double interpolationAlpha{0.0};
};

// Bounded fixed-step clock to keep simulation stable after slow frames.
class FrameClock {
public:
    static constexpr double FixedStep = 1.0 / 60.0;
    static constexpr double MaxFrameDelta = 0.25;
    static constexpr std::uint32_t MaxFixedSteps = 5;

    FrameTiming advance(double rawDeltaSeconds) noexcept {
        double delta = std::isfinite(rawDeltaSeconds)
            ? std::clamp(rawDeltaSeconds, 0.0, MaxFrameDelta)
            : 0.0;

        accumulator_ += delta;
        const auto available = static_cast<std::uint32_t>(accumulator_ / FixedStep);
        const auto steps = std::min(available, MaxFixedSteps);
        accumulator_ -= static_cast<double>(steps) * FixedStep;

        // Drop excess whole steps instead of allowing an unbounded catch-up spiral.
        if (steps == MaxFixedSteps && accumulator_ >= FixedStep) {
            accumulator_ = std::fmod(accumulator_, FixedStep);
        }

        return FrameTiming{
            delta,
            FixedStep,
            steps,
            accumulator_ / FixedStep
        };
    }

    void reset() noexcept { accumulator_ = 0.0; }
    double interpolationAlpha() const noexcept { return accumulator_ / FixedStep; }

private:
    double accumulator_{0.0};
};

} // namespace jassim::core
