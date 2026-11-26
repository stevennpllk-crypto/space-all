#pragma once

#include <cmath>
#include <cstddef>
#include <cstdint>
#include <vector>

namespace jassim::render {

struct TextureData {
    std::uint32_t width{0};
    std::uint32_t height{0};
    std::vector<std::uint8_t> rgb;

    bool valid() const noexcept {
        if (width == 0 || height == 0 || width > 2048 || height > 2048) return false;
        const std::size_t expected = static_cast<std::size_t>(width) *
                                     static_cast<std::size_t>(height) * 3U;
        return rgb.size() == expected;
    }
};

inline TextureData makeProceduralPlanetAlbedo(std::uint32_t width = 256,
                                               std::uint32_t height = 128) {
    TextureData texture;
    if (width == 0 || height == 0 || width > 2048 || height > 2048) return texture;

    texture.width = width;
    texture.height = height;
    texture.rgb.resize(static_cast<std::size_t>(width) * height * 3U);

    constexpr float pi = 3.14159265358979323846F;
    for (std::uint32_t y = 0; y < height; ++y) {
        const float latitude = (static_cast<float>(y) / static_cast<float>(height) - 0.5F) * pi;
        for (std::uint32_t x = 0; x < width; ++x) {
            const float u = static_cast<float>(x) / static_cast<float>(width);
            const float v = static_cast<float>(y) / static_cast<float>(height);
            const float field = std::sin(u * 18.0F + std::sin(v * 11.0F) * 2.2F) +
                                0.55F * std::sin(u * 37.0F - v * 19.0F) +
                                0.25F * std::cos(u * 71.0F + v * 43.0F);
            const bool land = field > 0.42F && std::fabs(latitude) < 1.35F;
            const bool ice = std::fabs(latitude) > 1.22F;
            const std::size_t offset = (static_cast<std::size_t>(y) * width + x) * 3U;
            if (ice) {
                texture.rgb[offset] = 190;
                texture.rgb[offset + 1U] = 218;
                texture.rgb[offset + 2U] = 235;
            } else if (land) {
                texture.rgb[offset] = 72;
                texture.rgb[offset + 1U] = 142;
                texture.rgb[offset + 2U] = 78;
            } else {
                texture.rgb[offset] = 24;
                texture.rgb[offset + 1U] = 91;
                texture.rgb[offset + 2U] = 166;
            }
        }
    }
    return texture;
}

} // namespace jassim::render
