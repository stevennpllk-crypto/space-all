#pragma once

#include "jassim/math/Vec3.hpp"

#include <cstdint>
#include <string>
#include <vector>

namespace jassim::core {

using EntityId = std::uint32_t;

struct Transform {
    math::Vec3 position{};
    math::Vec3 rotationEulerDegrees{};
    math::Vec3 scale{1.0F, 1.0F, 1.0F};
};

struct Entity {
    EntityId id{0};
    std::string name;
    Transform transform{};
    bool active{true};
};

class Scene {
public:
    EntityId createEntity(std::string name);
    bool destroyEntity(EntityId id);
    Entity* findEntity(EntityId id);
    const Entity* findEntity(EntityId id) const;

    const std::vector<Entity>& entities() const noexcept { return entities_; }
    std::size_t entityCount() const noexcept { return entities_.size(); }

private:
    EntityId nextId_{1};
    std::vector<Entity> entities_;
};

} // namespace jassim::core
