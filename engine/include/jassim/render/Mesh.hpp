#pragma once

#include <cmath>
#include <cstdint>
#include <limits>
#include <vector>

namespace jassim::render {

struct MeshVertex {
    float position[3]{};
    float normal[3]{};
    float uv[2]{};
};

struct MeshData {
    std::vector<MeshVertex> vertices;
    std::vector<std::uint32_t> indices;

    bool valid() const noexcept {
        return !vertices.empty() && !indices.empty() && indices.size() % 3 == 0;
    }
};

inline MeshData makeUvSphere(float radius = 1.0F,
                             std::uint32_t longitudeSegments = 32,
                             std::uint32_t latitudeSegments = 24) {
    MeshData mesh;
    if (!(radius > 0.0F) || !std::isfinite(radius) ||
        longitudeSegments < 3 || latitudeSegments < 2) {
        return mesh;
    }

    const std::uint64_t vertexCount =
        (static_cast<std::uint64_t>(longitudeSegments) + 1U) *
        (static_cast<std::uint64_t>(latitudeSegments) + 1U);
    const std::uint64_t indexCount =
        static_cast<std::uint64_t>(longitudeSegments) *
        latitudeSegments * 6U;
    if (vertexCount > std::numeric_limits<std::uint32_t>::max() ||
        indexCount > std::numeric_limits<std::size_t>::max()) {
        return mesh;
    }

    mesh.vertices.reserve(static_cast<std::size_t>(vertexCount));
    mesh.indices.reserve(static_cast<std::size_t>(indexCount));

    constexpr float pi = 3.14159265358979323846F;
    for (std::uint32_t y = 0; y <= latitudeSegments; ++y) {
        const float v = static_cast<float>(y) / static_cast<float>(latitudeSegments);
        const float theta = v * pi;
        const float sinTheta = std::sin(theta);
        const float cosTheta = std::cos(theta);

        for (std::uint32_t x = 0; x <= longitudeSegments; ++x) {
            const float u = static_cast<float>(x) / static_cast<float>(longitudeSegments);
            const float phi = u * (2.0F * pi);
            const float nx = sinTheta * std::cos(phi);
            const float ny = cosTheta;
            const float nz = sinTheta * std::sin(phi);
            mesh.vertices.push_back({
                {radius * nx, radius * ny, radius * nz},
                {nx, ny, nz},
                {u, 1.0F - v}
            });
        }
    }

    const std::uint32_t row = longitudeSegments + 1U;
    for (std::uint32_t y = 0; y < latitudeSegments; ++y) {
        for (std::uint32_t x = 0; x < longitudeSegments; ++x) {
            const std::uint32_t a = y * row + x;
            const std::uint32_t b = a + row;
            mesh.indices.insert(mesh.indices.end(), {
                a, b, a + 1U,
                a + 1U, b, b + 1U
            });
        }
    }
    return mesh;
}

} // namespace jassim::render
