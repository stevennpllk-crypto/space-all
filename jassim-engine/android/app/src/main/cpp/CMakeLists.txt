cmake_minimum_required(VERSION 3.22.1)
project(jassim_engine_android LANGUAGES CXX)

set(JASSIM_ENGINE_ROOT "${CMAKE_CURRENT_LIST_DIR}/../../../../../../")
add_subdirectory("${JASSIM_ENGINE_ROOT}/jassim-engine" "${CMAKE_CURRENT_BINARY_DIR}/engine-core")

add_library(jassim_engine_native SHARED native-lib.cpp)
target_compile_features(jassim_engine_native PRIVATE cxx_std_20)
target_link_libraries(jassim_engine_native PRIVATE jassim_engine_core android log)
