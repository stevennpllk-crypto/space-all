#pragma once
#include "jassim/math/transform.hpp"
#include <cstdint>
#include <string>
#include <vector>
namespace jassim {
using NodeId = std::uint32_t;
inline constexpr NodeId InvalidNodeId = 0;
struct SceneNode { NodeId id{InvalidNodeId}; NodeId parent{InvalidNodeId}; std::string name; Transform transform{}; bool active{true}; };
class Scene {
public:
 NodeId createNode(std::string name, NodeId parent=InvalidNodeId);
 bool removeNode(NodeId id);
 SceneNode* findNode(NodeId id) noexcept;
 const SceneNode* findNode(NodeId id) const noexcept;
 const std::vector<SceneNode>& nodes() const noexcept { return nodes_; }
private:
 std::vector<SceneNode> nodes_;
 NodeId nextId_{1};
};
}
