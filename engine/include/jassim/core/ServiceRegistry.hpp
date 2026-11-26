#pragma once

#include <cstddef>
#include <memory>
#include <typeindex>
#include <unordered_map>
#include <utility>

namespace jassim::core {

// Explicit, type-safe service ownership for engine subsystems.
class ServiceRegistry {
public:
    template <typename T>
    bool registerService(std::shared_ptr<T> service) {
        if (!service) return false;
        const auto key = std::type_index(typeid(T));
        return services_.emplace(key, std::move(service)).second;
    }

    template <typename T>
    std::shared_ptr<T> get() const {
        const auto it = services_.find(std::type_index(typeid(T)));
        if (it == services_.end()) return {};
        return std::static_pointer_cast<T>(it->second);
    }

    template <typename T>
    bool remove() {
        return services_.erase(std::type_index(typeid(T))) != 0;
    }

    void clear() noexcept { services_.clear(); }
    std::size_t size() const noexcept { return services_.size(); }

private:
    std::unordered_map<std::type_index, std::shared_ptr<void>> services_;
};

} // namespace jassim::core
