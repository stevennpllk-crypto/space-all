#include "jassim/math/vec3.hpp"
#include "jassim/scene/scene.hpp"
#include <cassert>
#include <cmath>
int main() {
 using namespace jassim;
 const Vec3 a{1,2,3}, b{2,1,0};
 const Vec3 sum=a+b;
 assert(sum.x==3 && sum.y==3 && sum.z==3);
 assert(std::abs(a.dot(b)-4.0F)<1.0e-6F);
 assert(std::abs(Vec3{0,3,4}.normalized().length()-1.0F)<1.0e-5F);
 Scene scene;
 const NodeId root=scene.createNode("Root");
 const NodeId child=scene.createNode("Camera",root);
 assert(root!=InvalidNodeId && child!=InvalidNodeId);
 assert(scene.findNode(child)->parent==root);
 assert(scene.createNode("Invalid",999999U)==InvalidNodeId);
 assert(scene.removeNode(root));
 assert(scene.findNode(child)->parent==InvalidNodeId);
 assert(!scene.removeNode(root));
 return 0;
}
