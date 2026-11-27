#include "jassim/scene/scene.hpp"
#include <algorithm>
#include <utility>
namespace jassim {
NodeId Scene::createNode(std::string name, NodeId parent) {
 if (parent!=InvalidNodeId && findNode(parent)==nullptr) return InvalidNodeId;
 const NodeId id=nextId_++;
 nodes_.push_back(SceneNode{id,parent,std::move(name),Transform{},true});
 return id;
}
bool Scene::removeNode(NodeId id) {
 const auto it=std::find_if(nodes_.begin(),nodes_.end(),[id](const SceneNode& n){return n.id==id;});
 if(it==nodes_.end()) return false;
 const NodeId parent=it->parent;
 for(auto& node:nodes_) if(node.parent==id) node.parent=parent;
 nodes_.erase(it);
 return true;
}
SceneNode* Scene::findNode(NodeId id) noexcept {
 const auto it=std::find_if(nodes_.begin(),nodes_.end(),[id](const SceneNode& n){return n.id==id;});
 return it==nodes_.end()?nullptr:&*it;
}
const SceneNode* Scene::findNode(NodeId id) const noexcept {
 const auto it=std::find_if(nodes_.begin(),nodes_.end(),[id](const SceneNode& n){return n.id==id;});
 return it==nodes_.end()?nullptr:&*it;
}
}
