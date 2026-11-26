#include "jassim/core/Scene.hpp"
#include "jassim/math/Vec3.hpp"

#include <cassert>
#include <cmath>

int main() {
    using jassim::core::Scene;
    using jassim::math::Vec3;

    const Vec3 direction{3.0F, 0.0F, 4.0F};
    const Vec3 unit = direction.normalized();
    assert(std::fabs(unit.length() - 1.0F) < 1.0e-5F);

    Scene scene;
    const auto sun = scene.createEntity("Sun");
    const auto earth = scene.createEntity("Earth");

    assert(sun != 0);
    assert(earth != 0);
    assert(sun != earth);
    assert(scene.entityCount() == 2);
    assert(scene.findEntity(earth) != nullptr);
    assert(scene.findEntity(earth)->name == "Earth");

    scene.findEntity(earth)->transform.position = {1.0F, 2.0F, 3.0F};
    assert(scene.findEntity(earth)->transform.position.y == 2.0F);

    assert(scene.destroyEntity(sun));
    assert(scene.findEntity(sun) == nullptr);
    assert(!scene.destroyEntity(sun));
    assert(scene.entityCount() == 1);

    return 0;
}
