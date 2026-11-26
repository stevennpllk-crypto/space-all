#include "jassim/core/EventBus.hpp"
#include "jassim/core/Scene.hpp"
#include "jassim/core/ServiceRegistry.hpp"
#include "jassim/math/Vec3.hpp"
#include "jassim/platform/DeviceProfile.hpp"
#include "jassim/platform/InputState.hpp"

#include <cassert>
#include <cmath>
#include <memory>

struct TickEvent {
    int frame;
};

struct TestService {
    int value{42};
};

int main() {
    using jassim::core::EventBus;
    using jassim::core::Scene;
    using jassim::core::ServiceRegistry;
    using jassim::math::Vec3;
    using jassim::platform::DeviceMetrics;
    using jassim::platform::DeviceProfile;
    using jassim::platform::QualityTier;
    using jassim::platform::InputState;

    const Vec3 direction{3.0F, 0.0F, 4.0F};
    const Vec3 unit = direction.normalized();
    assert(std::fabs(unit.length() - 1.0F) < 1.0e-5F);
    assert(Vec3{}.normalized().lengthSquared() == 0.0F);

    Scene scene;
    const auto sun = scene.createEntity("Sun");
    const auto earth = scene.createEntity("Earth");
    assert(sun != 0 && earth != 0 && sun != earth);
    assert(scene.entityCount() == 2);
    assert(scene.findEntity(earth) != nullptr);
    assert(scene.findEntity(earth)->name == "Earth");
    scene.findEntity(earth)->transform.position = {1.0F, 2.0F, 3.0F};
    assert(scene.findEntity(earth)->transform.position.y == 2.0F);
    assert(scene.destroyEntity(sun));
    assert(scene.findEntity(sun) == nullptr);
    assert(!scene.destroyEntity(sun));
    assert(scene.entityCount() == 1);

    ServiceRegistry services;
    auto service = std::make_shared<TestService>();
    assert(services.registerService<TestService>(service));
    assert(!services.registerService<TestService>(std::make_shared<TestService>()));
    assert(services.get<TestService>()->value == 42);
    assert(services.remove<TestService>());
    assert(!services.get<TestService>());

    EventBus events;
    int receivedFrame = 0;
    const auto subscription = events.subscribe<TickEvent>(
        [&receivedFrame](const TickEvent& event) { receivedFrame = event.frame; });
    assert(subscription != 0);
    events.publish(TickEvent{17});
    assert(receivedFrame == 17);
    assert(events.unsubscribe(subscription));
    receivedFrame = 0;
    events.publish(TickEvent{18});
    assert(receivedFrame == 0);

    const auto low = DeviceProfile::fromMetrics(DeviceMetrics{3072, 8, 3, 2400000});
    assert(low.tier == QualityTier::Low);
    assert(low.targetFrameRate == 30);
    assert(!low.dynamicShadows);

    const auto medium = DeviceProfile::fromMetrics(DeviceMetrics{4096, 6, 3, 2073600});
    assert(medium.tier == QualityTier::Medium);
    assert(medium.targetFrameRate == 45);

    const auto high = DeviceProfile::fromMetrics(DeviceMetrics{8192, 8, 3, 2560000});
    assert(high.tier == QualityTier::High);
    assert(high.dynamicShadows);
    assert(high.renderScale == 1.0F);

    const auto unknown = DeviceProfile::fromMetrics(DeviceMetrics{});
    assert(unknown.tier == QualityTier::Low);

    InputState input;
    assert(input.touchDown(4, 10.0F, 20.0F));
    assert(!input.touchDown(4, 10.0F, 20.0F));
    assert(input.touchDown(7, 30.0F, 40.0F));
    assert(input.activeTouchCount() == 2);
    assert(input.touchMove(4, 15.0F, 24.0F));
    assert(input.touchMove(4, 18.0F, 29.0F));
    const auto* points = input.touches();
    assert(points[0].id == 4);
    assert(points[0].pressed);
    assert(points[0].deltaX == 8.0F);
    assert(points[0].deltaY == 9.0F);
    assert(input.touchUp(4, 20.0F, 30.0F));
    assert(input.activeTouchCount() == 1);
    assert(!input.touchDown(4, 0.0F, 0.0F));
    input.clearFrame();
    assert(input.touchDown(4, 0.0F, 0.0F));
    assert(input.activeTouchCount() == 2);

    return 0;
}
