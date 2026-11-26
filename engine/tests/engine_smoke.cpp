#include "jassim/core/EventBus.hpp"
#include "jassim/core/FrameClock.hpp"
#include "jassim/core/Scene.hpp"
#include "jassim/core/ServiceRegistry.hpp"
#include "jassim/math/Vec3.hpp"
#include "jassim/platform/DeviceProfile.hpp"
#include "jassim/platform/InputState.hpp"
#include "jassim/render/Camera.hpp"
#include "jassim/render/Mesh.hpp"
#include "jassim/math/Mat4.hpp"

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
    using jassim::core::FrameClock;
    using jassim::core::Scene;
    using jassim::core::ServiceRegistry;
    using jassim::math::Vec3;
    using jassim::platform::DeviceMetrics;
    using jassim::platform::DeviceProfile;
    using jassim::platform::QualityTier;
    using jassim::platform::InputState;
    using jassim::render::Camera;
    using jassim::render::makeUvSphere;
    using jassim::math::Mat4;

    const Vec3 direction{3.0F, 0.0F, 4.0F};
    assert(Vec3::dot(Vec3{1.0F, 0.0F, 0.0F}, Vec3{0.0F, 1.0F, 0.0F}) == 0.0F);
    const Vec3 perpendicular = Vec3::cross(Vec3{1.0F, 0.0F, 0.0F}, Vec3{0.0F, 1.0F, 0.0F});
    assert(perpendicular.z == 1.0F);
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

    const Mat4 identity = Mat4::identity();
    assert(identity.at(0, 0) == 1.0F && identity.at(3, 3) == 1.0F);
    const Mat4 perspective = Mat4::perspective(1.04719755F, 16.0F / 9.0F, 0.1F, 1000.0F);
    assert(perspective.at(0, 0) > 0.0F);
    assert(perspective.at(3, 2) == -1.0F);
    const Mat4 invalidProjection = Mat4::perspective(1.0F, 0.0F, 0.1F, 100.0F);
    assert(invalidProjection.at(0, 0) == 0.0F);
    const Mat4 view = Mat4::lookAt(Vec3{0.0F, 0.0F, 5.0F}, Vec3{0.0F, 0.0F, 0.0F}, Vec3{0.0F, 1.0F, 0.0F});
    assert(std::fabs(view.at(2, 3) + 5.0F) < 1.0e-5F);
    Camera camera;
    assert(std::fabs(camera.forward().z + 1.0F) < 1.0e-5F);
    camera.moveLocal(2.0F, 0.0F, -3.0F);
    assert(std::fabs(camera.position().x - 2.0F) < 1.0e-5F);
    assert(std::fabs(camera.position().z - 8.0F) < 1.0e-5F);
    camera.rotate(0.25F, 0.2F);
    assert(camera.forward().length() > 0.99F);
    assert(camera.viewProjection().at(3, 2) != 0.0F);

    const auto sphere = makeUvSphere(2.0F, 16, 8);
    assert(sphere.valid());
    assert(sphere.vertices.size() == 17U * 9U);
    assert(sphere.indices.size() == 16U * 8U * 6U);
    for (const auto& vertex : sphere.vertices) {
        const float normalLength = std::sqrt(
            vertex.normal[0] * vertex.normal[0] +
            vertex.normal[1] * vertex.normal[1] +
            vertex.normal[2] * vertex.normal[2]);
        assert(std::fabs(normalLength - 1.0F) < 1.0e-4F);
        const float positionLength = std::sqrt(
            vertex.position[0] * vertex.position[0] +
            vertex.position[1] * vertex.position[1] +
            vertex.position[2] * vertex.position[2]);
        assert(std::fabs(positionLength - 2.0F) < 1.0e-4F);
        assert(vertex.uv[0] >= 0.0F && vertex.uv[0] <= 1.0F);
        assert(vertex.uv[1] >= 0.0F && vertex.uv[1] <= 1.0F);
    }
    for (const auto index : sphere.indices) {
        assert(index < sphere.vertices.size());
    }
    assert(!makeUvSphere(0.0F, 16, 8).valid());
    assert(!makeUvSphere(1.0F, 2, 8).valid());
    assert(std::fabs(sphere.vertices.front().uv[0]) < 1.0e-6F);
    assert(std::fabs(sphere.vertices[16].uv[0] - 1.0F) < 1.0e-6F);

    FrameClock clock;
    const auto normalFrame = clock.advance(1.0 / 30.0);
    assert(normalFrame.fixedSteps == 2);
    assert(std::fabs(normalFrame.deltaSeconds - (1.0 / 30.0)) < 1.0e-9);
    assert(normalFrame.interpolationAlpha >= 0.0 && normalFrame.interpolationAlpha < 1.0);
    const auto slowFrame = clock.advance(2.0);
    assert(slowFrame.deltaSeconds == FrameClock::MaxFrameDelta);
    assert(slowFrame.fixedSteps <= FrameClock::MaxFixedSteps);
    assert(slowFrame.interpolationAlpha >= 0.0 && slowFrame.interpolationAlpha < 1.0);
    clock.reset();
    const auto resetFrame = clock.advance(0.0);
    assert(resetFrame.fixedSteps == 0);
    assert(resetFrame.interpolationAlpha == 0.0);
    assert(clock.advance(-1.0).deltaSeconds == 0.0);

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
