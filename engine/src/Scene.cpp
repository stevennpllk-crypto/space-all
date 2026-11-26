#include "jassim/core/Scene.hpp"

#include <algorithm>
#include <utility>

namespace jassim::core {

EntityId Scene::createEntity(std::string name) {
    if (nextId_ == 0) {
        return 0; // ID space exhausted; zero is reserved as invalid.
    }

    const EntityId id = nextId_++;
    entities_.push_back(Entity{id, std::move(name), Transform{}, true});
    return id;
}

bool Scene::destroyEntity(EntityId id) {
    const auto it = std::find_if(
        entities_.begin(), entities_.end(),
        [id](const Entity& entity) { return entity.id == id; }
    );

    if (it == entities_.end()) {
        return false;
    }

    entities_.erase(it);
    return true;
}

Entity* Scene::findEntity(EntityId id) {
    const auto it = std::find_if(
        entities_.begin(), entities_.end(),
        [id](const Entity& entity) { return entity.id == id; }
    );
    return it == entities_.end() ? nullptr : &(*it);
}

const Entity* Scene::findEntity(EntityId id) const {
    const auto it = std::find_if(
        entities_.begin(), entities_.end(),
        [id](const Entity& entity) { return entity.id == id; }
    );
    return it == entities_.end() ? nullptr : &(*it);
}

} // namespace jassim::core
