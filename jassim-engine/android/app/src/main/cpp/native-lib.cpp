#include <jni.h>
#include "jassim/scene/scene.hpp"

extern "C" JNIEXPORT jstring JNICALL
Java_com_jassim_engine_NativeBridge_nativeEngineVersion(JNIEnv* env, jobject) {
    return env->NewStringUTF("JASSIM ENGINE 0.1.0 / C++20 / OpenGL ES 3.0");
}

extern "C" JNIEXPORT jint JNICALL
Java_com_jassim_engine_NativeBridge_nativeCreateRootNode(JNIEnv*, jobject) {
    static jassim::Scene scene;
    return static_cast<jint>(scene.createNode("AndroidRoot"));
}
