#pragma once

#include <array>
#include <cstddef>
#include <cstdint>

namespace jassim::platform {

struct TouchPoint {
    std::int32_t id{-1};
    float x{0.0F};
    float y{0.0F};
    float deltaX{0.0F};
    float deltaY{0.0F};
    bool active{false};
    bool pressed{false};
    bool released{false};
};

// Fixed-capacity touch snapshot: no per-frame heap allocation.
class InputState {
public:
    static constexpr std::size_t MaxTouches = 10;

    bool touchDown(std::int32_t id, float x, float y) noexcept {
        if (id < 0) return false;
        for (const auto& point : touches_) {
            if (point.id == id && (point.active || point.pressed || point.released)) return false;
        }
        for (auto& point : touches_) {
            if (!point.active && !point.pressed && !point.released) {
                point = TouchPoint{id, x, y, 0.0F, 0.0F, true, true, false};
                ++activeCount_;
                return true;
            }
        }
        return false;
    }

    bool touchMove(std::int32_t id, float x, float y) noexcept {
        auto* point = find(id);
        if (point == nullptr) return false;
        point->deltaX += x - point->x;
        point->deltaY += y - point->y;
        point->x = x;
        point->y = y;
        return true;
    }

    bool touchUp(std::int32_t id, float x, float y) noexcept {
        auto* point = find(id);
        if (point == nullptr) return false;
        point->deltaX += x - point->x;
        point->deltaY += y - point->y;
        point->x = x;
        point->y = y;
        point->active = false;
        point->released = true;
        --activeCount_;
        return true;
    }

    const TouchPoint* touches() const noexcept { return touches_.data(); }
    std::size_t activeTouchCount() const noexcept { return activeCount_; }

    void cancelAll() noexcept {
        for (auto& point : touches_) {
            if (point.active) {
                point.active = false;
                point.released = true;
            }
        }
        activeCount_ = 0;
    }

    // Call once after the game has consumed the current frame's input.
    void clearFrame() noexcept {
        for (auto& point : touches_) {
            point.pressed = false;
            point.released = false;
            point.deltaX = 0.0F;
            point.deltaY = 0.0F;
            if (!point.active) point = TouchPoint{};
        }
    }

private:
    TouchPoint* find(std::int32_t id) noexcept {
        for (auto& point : touches_) {
            if (point.active && point.id == id) return &point;
        }
        return nullptr;
    }

    std::array<TouchPoint, MaxTouches> touches_{};
    std::size_t activeCount_{0};
};

} // namespace jassim::platform
