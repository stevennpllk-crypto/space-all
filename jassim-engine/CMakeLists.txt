cmake_minimum_required(VERSION 3.20)
project(JassimEngine3D VERSION 0.1.0 LANGUAGES CXX)

option(JASSIM_ENGINE_BUILD_TESTS "Build core tests" ON)
add_library(jassim_engine_core src/scene.cpp)
target_include_directories(jassim_engine_core PUBLIC include)
target_compile_features(jassim_engine_core PUBLIC cxx_std_20)

if(JASSIM_ENGINE_BUILD_TESTS)
  enable_testing()
  add_executable(jassim_engine_tests tests/core_tests.cpp)
  target_link_libraries(jassim_engine_tests PRIVATE jassim_engine_core)
  add_test(NAME jassim_engine_core_tests COMMAND jassim_engine_tests)
endif()
