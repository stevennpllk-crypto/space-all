#pragma once

#include "jassim/core/Scene.hpp"

#include <algorithm>
#include <cmath>
#include <cstdint>
#include <vector>

namespace jassim::render {

using ResourceId = std::uint32_t;

struct RenderItem {
    core::EntityId entity{0};
    ResourceId mesh{0};
    ResourceId material{0};
    float cameraDistanceSquared{0.0F};
    bool transparent{false};

    bool valid() const noexcept {
        return entity != 0 && mesh != 0 && material != 0 &&
               std::isfinite(cameraDistanceSquared) && cameraDistanceSquared >= 0.0F;
    }
};

class RenderQueue {
public:
    bool submit(const RenderItem& item) {
        if (!item.valid()) return false;
        items_.push_back(item);
        return true;
    }

    void clear() noexcept { items_.clear(); }
    bool empty() const noexcept { return items_.empty(); }
    std::size_t size() const noexcept { return items_.size(); }
    const std::vector<RenderItem>& items() const noexcept { return items_; }

    void sort() {
        std::stable_sort(items_.begin(), items_.end(),
            [](const RenderItem& a, const RenderItem& b) {
                if (a.transparent != b.transparent) return !a.transparent;
                if (!a.transparent && a.material != b.material) {
                    return a.material < b.material;
                }
                if (a.transparent &&
                    a.cameraDistanceSquared != b.cameraDistanceSquared) {
                    return a.cameraDistanceSquared > b.cameraDistanceSquared;
                }
                return a.entity < b.entity;
            });
    }

private:
    std::vector<RenderItem> items_;
};

} // namespace jassim::render
