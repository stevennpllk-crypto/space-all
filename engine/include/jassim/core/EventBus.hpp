#pragma once

#include <algorithm>
#include <cstdint>
#include <functional>
#include <typeindex>
#include <unordered_map>
#include <vector>

namespace jassim::core {

// Synchronous typed events. Not thread-safe; publish on the engine thread.
class EventBus {
public:
    using SubscriptionId = std::uint64_t;

    template <typename Event>
    SubscriptionId subscribe(std::function<void(const Event&)> callback) {
        if (!callback || nextId_ == 0) return 0;
        const SubscriptionId id = nextId_++;
        auto& list = handlers_[std::type_index(typeid(Event))];
        list.push_back(Handler{id, [fn = std::move(callback)](const void* event) {
            fn(*static_cast<const Event*>(event));
        }});
        return id;
    }

    template <typename Event>
    void publish(const Event& event) const {
        const auto it = handlers_.find(std::type_index(typeid(Event)));
        if (it == handlers_.end()) return;

        // Copy callbacks so a callback may subscribe/unsubscribe safely.
        std::vector<Callback> snapshot;
        snapshot.reserve(it->second.size());
        for (const auto& handler : it->second) snapshot.push_back(handler.callback);
        for (const auto& callback : snapshot) callback(&event);
    }

    bool unsubscribe(SubscriptionId id) {
        if (id == 0) return false;
        bool removed = false;
        for (auto it = handlers_.begin(); it != handlers_.end();) {
            auto& list = it->second;
            const auto oldSize = list.size();
            list.erase(std::remove_if(list.begin(), list.end(),
                [id](const Handler& handler) { return handler.id == id; }), list.end());
            removed = removed || list.size() != oldSize;
            if (list.empty()) it = handlers_.erase(it);
            else ++it;
        }
        return removed;
    }

    void clear() noexcept { handlers_.clear(); }

private:
    using Callback = std::function<void(const void*)>;
    struct Handler {
        SubscriptionId id;
        Callback callback;
    };

    SubscriptionId nextId_{1};
    std::unordered_map<std::type_index, std::vector<Handler>> handlers_;
};

} // namespace jassim::core
